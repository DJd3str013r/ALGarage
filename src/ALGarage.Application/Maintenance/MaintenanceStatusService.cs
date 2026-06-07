using ALGarage.Application.Abstractions;
using ALGarage.Domain.Common;
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
        var (planName, items) = await EvaluateAsync(vehicleId, ct);
        return items is null
            ? new VehicleMaintenanceDto(false, null, [])
            : new VehicleMaintenanceDto(true, planName, items);
    }

    /// <summary>Resumo leve (contagens por estado) — usado nos cards da garagem.</summary>
    public async Task<MaintenanceSummaryDto> GetSummaryAsync(Guid vehicleId, CancellationToken ct = default)
    {
        var (_, items) = await EvaluateAsync(vehicleId, ct);
        if (items is null)
        {
            return new MaintenanceSummaryDto(false, 0, 0);
        }

        return new MaintenanceSummaryDto(
            HasPlan: true,
            OverdueCount: items.Count(i => i.State == MaintenanceState.Overdue),
            DueSoonCount: items.Count(i => i.State == MaintenanceState.DueSoon));
    }

    private async Task<(string? PlanName, IReadOnlyList<MaintenanceStatusDto>? Items)> EvaluateAsync(
        Guid vehicleId, CancellationToken ct)
    {
        var userId = currentUser.RequireUserId();
        var vehicle = await garage.GetVehicleAsync(vehicleId, userId, ct);
        if (vehicle?.ModelVariantId is not { } variantId)
        {
            return (null, null);
        }

        var variant = await catalog.GetVariantDetailAsync(variantId, ct);
        if (variant is null)
        {
            return (null, null);
        }

        var plan = await catalog.GetPlanByEngineFamilyAsync(variant.EngineFamily, ct);
        if (plan is null || plan.Items.Count == 0)
        {
            return (null, null);
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

        return (plan.Name, items);
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
