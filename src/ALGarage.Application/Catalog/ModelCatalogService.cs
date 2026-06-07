using ALGarage.Application.Abstractions;

namespace ALGarage.Application.Catalog;

/// <summary>
/// Expõe o catálogo de fábrica (versões + opcionais) e os upgrades do MODELO de um veículo do
/// usuário. Retorna null quando o veículo não foi identificado (sem versão resolvida).
/// </summary>
public sealed class ModelCatalogService(
    IGarageRepository garage,
    ICatalogRepository catalog,
    ICurrentUser currentUser)
{
    public async Task<ModelFactoryInfoDto?> GetFactoryInfoAsync(Guid vehicleId, CancellationToken ct = default)
    {
        var variant = await ResolveVariantAsync(vehicleId, ct);
        if (variant is null)
        {
            return null;
        }

        var versions = await catalog.GetVersionsByModelAsync(variant.VehicleModelId, ct);
        var options = await catalog.GetFactoryOptionsByModelAsync(variant.VehicleModelId, ct);
        return new ModelFactoryInfoDto(variant.ModelName, versions, options);
    }

    public async Task<IReadOnlyList<UpgradeDto>?> GetUpgradesAsync(Guid vehicleId, CancellationToken ct = default)
    {
        var variant = await ResolveVariantAsync(vehicleId, ct);
        return variant is null
            ? null
            : await catalog.GetUpgradesByModelAsync(variant.VehicleModelId, variant.EngineFamily, ct);
    }

    private async Task<ModelVariantDetail?> ResolveVariantAsync(Guid vehicleId, CancellationToken ct)
    {
        var userId = currentUser.RequireUserId();
        var vehicle = await garage.GetVehicleAsync(vehicleId, userId, ct);
        return vehicle?.ModelVariantId is { } id
            ? await catalog.GetVariantDetailAsync(id, ct)
            : null;
    }
}
