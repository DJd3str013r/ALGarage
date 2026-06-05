using ALGarage.Domain.Common;

namespace ALGarage.Domain.Maintenance;

/// <summary>Dados de baseline de um veículo para avaliar o cronograma (sem dependência de infra).</summary>
public sealed record VehicleMaintenanceContext(
    DateOnly AcquisitionDate,
    int AcquisitionKm,
    int CurrentKm,
    double AvgDailyKm);

/// <summary>Último cumprimento de um item de manutenção (vindo do histórico de serviços).</summary>
public sealed record LastServiceForItem(DateOnly Date, int OdometerKm);

/// <summary>Status calculado de UM item do plano para UM veículo.</summary>
public sealed record MaintenanceItemStatus(
    string ItemName,
    string? Category,
    string? PartHint,
    MaintenanceEvaluation Evaluation);

/// <summary>
/// Avalia TODOS os itens de um plano de manutenção para um veículo, aplicando o histórico de serviços
/// (quando existe) ou o baseline de aquisição (quando o item nunca foi feito). Função pura — toda a
/// IO fica fora. Testável em ALGarage.Domain.Tests.
/// </summary>
public sealed class MaintenanceScheduleEvaluator(IMaintenanceEstimator estimator)
{
    public IReadOnlyList<MaintenanceItemStatus> Evaluate(
        VehicleMaintenanceContext vehicle,
        IReadOnlyList<MaintenanceItem> items,
        IReadOnlyDictionary<string, LastServiceForItem> lastServiceByItemName,
        DateOnly today,
        int dueSoonThresholdDays = 14)
    {
        var result = new List<MaintenanceItemStatus>(items.Count);

        foreach (var item in items)
        {
            // Baseline: último serviço do item, ou a aquisição do veículo se nunca foi feito.
            var baseline = lastServiceByItemName.TryGetValue(item.Name, out var last)
                ? last
                : new LastServiceForItem(vehicle.AcquisitionDate, vehicle.AcquisitionKm);

            var input = new MaintenanceInput(
                IntervalKm: item.IntervalKm,
                IntervalMonths: item.IntervalMonths,
                WhicheverComesFirst: item.WhicheverComesFirst,
                LastServiceDate: baseline.Date,
                LastServiceKm: baseline.OdometerKm,
                CurrentKm: vehicle.CurrentKm,
                AvgDailyKm: vehicle.AvgDailyKm);

            var evaluation = estimator.Evaluate(input, today, dueSoonThresholdDays);
            result.Add(new MaintenanceItemStatus(item.Name, item.Category, item.PartHint, evaluation));
        }

        // Ordena pelo mais urgente primeiro (Overdue → DueSoon → Ok), depois por data prevista.
        return result
            .OrderByDescending(s => s.Evaluation.State)
            .ThenBy(s => s.Evaluation.EstimatedDueDate ?? DateOnly.MaxValue)
            .ToList();
    }
}
