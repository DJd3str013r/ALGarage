using ALGarage.Application.Abstractions;
using ALGarage.Domain.Vehicles;

namespace ALGarage.Application.Vehicles;

/// <summary>
/// Casos de uso da garagem: decodificar VIN (com candidatos), cadastrar veículo (VIN obrigatório),
/// listar e detalhar. Orquestra domínio + repositórios; sem dependência de framework de UI.
/// </summary>
public sealed class VehicleService(
    IVinDecoder vinDecoder,
    ICatalogRepository catalog,
    IGarageRepository garage,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    Maintenance.MaintenanceStatusService maintenance)
{
    /// <summary>Decodifica o VIN e devolve as versões candidatas para confirmação do usuário.</summary>
    public async Task<VinDecodeResultDto> DecodeAsync(string rawVin, CancellationToken ct = default)
    {
        if (!Vin.TryParse(rawVin, out var vin))
        {
            return new VinDecodeResultDto(false, rawVin, null, null, null, null,
                Domain.Common.DataSource.Unknown, []);
        }

        var decoded = await vinDecoder.DecodeAsync(vin, ct);
        var modelYear = decoded.ModelYear ?? VinModelYear.TryDecode(vin);
        var candidates = await catalog.FindVariantsByYearAsync(modelYear, ct);

        return new VinDecodeResultDto(
            Identified: decoded.Identified || candidates.Count > 0,
            Vin: vin.Value,
            Wmi: vin.Wmi,
            ModelYear: modelYear,
            Make: decoded.Make,
            Model: decoded.Model,
            Source: decoded.Source,
            CandidateVariants: candidates);
    }

    /// <summary>Cadastra um veículo na garagem do usuário autenticado. Retorna o Id criado.</summary>
    public async Task<Guid> RegisterAsync(RegisterVehicleRequest request, CancellationToken ct = default)
    {
        var userId = currentUser.RequireUserId();
        var vin = Vin.Parse(request.Vin);

        if (await garage.VinExistsForUserAsync(userId, vin.Value, ct))
        {
            throw new InvalidOperationException("Este VIN já está cadastrado na sua garagem.");
        }

        var vehicle = new Vehicle(
            userId, vin, request.PurchaseDate, request.PreviousOwners,
            request.OdometerAtAcquisitionKm, request.CurrentOdometerKm, request.AvgDailyKm, request.Nickname);

        if (request.ModelVariantId is { } variantId)
        {
            var variant = await catalog.GetVariantDetailAsync(variantId, ct)
                ?? throw new InvalidOperationException("Versão selecionada não encontrada.");
            vehicle.AttachVariant(variant.Id);
        }

        await garage.AddVehicleAsync(vehicle, ct);
        await garage.AddOdometerReadingAsync(
            new OdometerReading(vehicle.Id, request.OdometerAtAcquisitionKm, request.PurchaseDate, OdometerSource.Acquisition), ct);
        await garage.AddOdometerReadingAsync(
            new OdometerReading(vehicle.Id, request.CurrentOdometerKm, DateOnly.FromDateTime(DateTime.UtcNow), OdometerSource.Manual), ct);

        await unitOfWork.SaveChangesAsync(ct);
        return vehicle.Id;
    }

    public async Task<IReadOnlyList<VehicleSummaryDto>> ListAsync(CancellationToken ct = default)
    {
        var userId = currentUser.RequireUserId();
        var vehicles = await garage.GetVehiclesByUserAsync(userId, ct);

        var list = new List<VehicleSummaryDto>(vehicles.Count);
        foreach (var v in vehicles)
        {
            var variant = v.ModelVariantId is { } id ? await catalog.GetVariantDetailAsync(id, ct) : null;
            var summary = await maintenance.GetSummaryAsync(v.Id, ct);
            list.Add(new VehicleSummaryDto(
                v.Id, v.Vin.Value, v.Nickname,
                Display: BuildDisplay(variant),
                v.CurrentOdometerKm,
                Identified: variant is not null,
                OverdueCount: summary.OverdueCount,
                DueSoonCount: summary.DueSoonCount));
        }

        return list;
    }

    public async Task<VehicleDetailDto?> GetAsync(Guid vehicleId, CancellationToken ct = default)
    {
        var userId = currentUser.RequireUserId();
        var v = await garage.GetVehicleAsync(vehicleId, userId, ct);
        if (v is null)
        {
            return null;
        }

        var variant = v.ModelVariantId is { } id ? await catalog.GetVariantDetailAsync(id, ct) : null;
        return new VehicleDetailDto(
            v.Id, v.Vin.Value, v.Nickname, v.PurchaseDate, v.PreviousOwners,
            v.OdometerAtAcquisitionKm, v.CurrentOdometerKm, v.AvgDailyKm, variant);
    }

    private static string BuildDisplay(Catalog.ModelVariantDetail? variant) =>
        variant is null
            ? "VIN não identificado"
            : $"{variant.ModelName} {variant.Trim} ({variant.ModelYearFrom}–{variant.ModelYearTo})";
}
