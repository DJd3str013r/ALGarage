using ALGarage.Application.Abstractions;
using ALGarage.Domain.Maintenance;

namespace ALGarage.Application.Maintenance;

/// <summary>
/// Calcula o status de manutenção de um veículo: resolve o plano pela família de motor da versão,
/// aplica o histórico de serviços e roda o avaliador puro do domínio.
/// </summary>
public sealed class MaintenanceStatusService(
    IGarageRepository garage,
    ICatalogRepository catalog,
    IMaintenanceEstimator estimator,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
{
    public async Task<VehicleMaintenanceDto> GetForVehicleAsync(Guid vehicleId, CancellationToken ct = default)
    {
        var userId = currentUser.RequireUserId();
        var vehicle = await garage.GetVehicleAsync(vehicleId, userId, ct);
        if (vehicle?.ModelVariantId is not { } variantId)
        {
            return new VehicleMaintenanceDto(false, null, []); // sem versão identificada → sem plano
        }

        var variant = await catalog.GetVariantDetailAsync(variantId, ct);
        if (variant is null)
        {
            return new VehicleMaintenanceDto(false, null, []);
        }

        var plan = await catalog.GetPlanByEngineFamilyAsync(variant.EngineFamily, ct);
        if (plan is null || plan.Items.Count == 0)
        {
            return new VehicleMaintenanceDto(false, null, []);
        }

        var records = await garage.GetServiceRecordsAsync(vehicleId, ct);
        var lastByItem = BuildLastServiceByItem(records);

        var context = new VehicleMaintenanceContext(
            vehicle.PurchaseDate, vehicle.OdometerAtAcquisitionKm, vehicle.CurrentOdometerKm, vehicle.AvgDailyKm);

        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        var evaluator = new MaintenanceScheduleEvaluator(estimator);
        var statuses = evaluator.Evaluate(context, plan.Items, lastByItem, today);

        var items = statuses
            .Select(s => new MaintenanceStatusDto(
                s.ItemName, s.Category, s.PartHint, s.Evaluation.State,
                s.Evaluation.DueAtKm, s.Evaluation.EstimatedDueDate))
            .ToList();

        return new VehicleMaintenanceDto(true, plan.Name, items);
    }

    private static Dictionary<string, LastServiceForItem> BuildLastServiceByItem(
        IReadOnlyList<Domain.Service.ServiceRecord> records)
    {
        var map = new Dictionary<string, LastServiceForItem>(StringComparer.OrdinalIgnoreCase);
        foreach (var record in records.OrderByDescending(r => r.PerformedOn))
        {
            foreach (var item in record.Items)
            {
                if (item.MaintenanceItemKey is { } key && !map.ContainsKey(key))
                {
                    map[key] = new LastServiceForItem(record.PerformedOn, record.OdometerKm);
                }
            }
        }

        return map;
    }
}
