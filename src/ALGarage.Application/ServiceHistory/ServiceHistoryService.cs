using ALGarage.Application.Abstractions;
using ALGarage.Domain.Service;
using ALGarage.Domain.Vehicles;

namespace ALGarage.Application.ServiceHistory;

/// <summary>Registra e lista serviços realizados. Registrar um item "zera o relógio" da estimativa.</summary>
public sealed class ServiceHistoryService(
    IGarageRepository garage,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
{
    public async Task<Guid> AddAsync(AddServiceRecordRequest request, CancellationToken ct = default)
    {
        var userId = currentUser.RequireUserId();
        var vehicle = await garage.GetVehicleAsync(request.VehicleId, userId, ct)
            ?? throw new InvalidOperationException("Veículo não encontrado na sua garagem.");

        var record = new ServiceRecord(
            vehicle.Id, request.PerformedOn, request.OdometerKm, request.Workshop, request.TotalCost, request.Notes);

        foreach (var item in request.Items)
        {
            record.AddItem(item.Description, item.MaintenanceItemKey, item.Cost);
        }

        await garage.AddServiceRecordAsync(record, ct);

        // O serviço também é uma leitura de hodômetro; atualiza o km atual se avançou.
        if (request.OdometerKm > vehicle.CurrentOdometerKm)
        {
            vehicle.UpdateCurrentOdometer(request.OdometerKm);
        }

        await garage.AddOdometerReadingAsync(
            new OdometerReading(vehicle.Id, request.OdometerKm, request.PerformedOn, OdometerSource.Service), ct);

        await unitOfWork.SaveChangesAsync(ct);
        return record.Id;
    }

    public async Task<IReadOnlyList<ServiceRecordDto>> ListAsync(Guid vehicleId, CancellationToken ct = default)
    {
        var userId = currentUser.RequireUserId();
        if (await garage.GetVehicleAsync(vehicleId, userId, ct) is null)
        {
            return [];
        }

        var records = await garage.GetServiceRecordsAsync(vehicleId, ct);
        return records
            .OrderByDescending(r => r.PerformedOn)
            .Select(r => new ServiceRecordDto(
                r.Id, r.PerformedOn, r.OdometerKm, r.Workshop, r.TotalCost, r.Notes,
                r.Items.Select(i => new ServiceItemDto(i.Description, i.MaintenanceItemKey, i.Cost)).ToList()))
            .ToList();
    }
}
