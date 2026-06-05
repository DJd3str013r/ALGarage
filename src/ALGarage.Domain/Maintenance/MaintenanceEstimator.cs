using ALGarage.Domain.Common;

namespace ALGarage.Domain.Maintenance;

/// <summary>Dados de entrada para avaliar UM item de manutenção de UM veículo.</summary>
public readonly record struct MaintenanceInput(
    int? IntervalKm,
    int? IntervalDays,
    bool WhicheverComesFirst,
    DateOnly LastServiceDate,
    int LastServiceKm,
    int CurrentKm,
    double AvgDailyKm);

/// <summary>Resultado da avaliação. <see cref="EstimatedDueDate"/> é a data prevista de vencimento.</summary>
public readonly record struct MaintenanceEvaluation(
    int? DueAtKm,
    DateOnly? DueByDate,
    DateOnly? EstimatedDueDate,
    MaintenanceState State);

/// <summary>Motor de estimativa de manutenção. Função PURA e determinística (ADR-0009).</summary>
public interface IMaintenanceEstimator
{
    MaintenanceEvaluation Evaluate(MaintenanceInput input, DateOnly today, int dueSoonThresholdDays = 14);
}

/// <inheritdoc cref="IMaintenanceEstimator"/>
public sealed class MaintenanceEstimator : IMaintenanceEstimator
{
    public MaintenanceEvaluation Evaluate(MaintenanceInput input, DateOnly today, int dueSoonThresholdDays = 14)
    {
        // 1) Vencimento por KM → projetado para uma data via km/dia.
        int? dueAtKm = input.IntervalKm is { } km ? input.LastServiceKm + km : null;
        DateOnly? dateFromKm = null;
        if (dueAtKm is { } target && input.AvgDailyKm > 0)
        {
            var kmRemaining = target - input.CurrentKm;
            var daysFromKm = (int)Math.Floor(kmRemaining / input.AvgDailyKm);
            dateFromKm = today.AddDays(daysFromKm);
        }

        // 2) Vencimento por TEMPO.
        DateOnly? dueByDate = input.IntervalDays is { } days
            ? input.LastServiceDate.AddDays(days)
            : null;

        // 3) Data prevista efetiva: o que vier primeiro (quando ambos existem e a regra pede).
        var estimatedDueDate = CombineDueDates(dateFromKm, dueByDate, input.WhicheverComesFirst);

        // 4) Estado. KM já ultrapassado também conta como Overdue, mesmo sem data projetada.
        var kmOverdue = dueAtKm is { } t && input.CurrentKm >= t;
        var state = EvaluateState(estimatedDueDate, today, dueSoonThresholdDays, kmOverdue);

        return new MaintenanceEvaluation(dueAtKm, dueByDate, estimatedDueDate, state);
    }

    private static DateOnly? CombineDueDates(DateOnly? fromKm, DateOnly? byDate, bool whicheverFirst)
    {
        if (fromKm is null)
        {
            return byDate;
        }

        if (byDate is null)
        {
            return fromKm;
        }

        return whicheverFirst
            ? (fromKm < byDate ? fromKm : byDate)
            : (fromKm > byDate ? fromKm : byDate);
    }

    private static MaintenanceState EvaluateState(DateOnly? estimatedDueDate, DateOnly today, int thresholdDays, bool kmOverdue)
    {
        if (kmOverdue)
        {
            return MaintenanceState.Overdue;
        }

        if (estimatedDueDate is not { } due)
        {
            return MaintenanceState.Ok; // sem informação suficiente para alertar
        }

        if (due <= today)
        {
            return MaintenanceState.Overdue;
        }

        return due <= today.AddDays(thresholdDays)
            ? MaintenanceState.DueSoon
            : MaintenanceState.Ok;
    }
}
