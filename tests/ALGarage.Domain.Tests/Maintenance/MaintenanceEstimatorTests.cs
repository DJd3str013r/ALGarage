using ALGarage.Domain.Common;
using ALGarage.Domain.Maintenance;
using Shouldly;
using Xunit;

namespace ALGarage.Domain.Tests.Maintenance;

/// <summary>
/// Exemplo de teste do motor de estimativa (ADR-0009). Demonstra que a lógica de "km e/ou tempo,
/// o que vier primeiro" é pura e testável. NÃO é cobertura completa — é o scaffolding de testes.
/// </summary>
public sealed class MaintenanceEstimatorTests
{
    private readonly MaintenanceEstimator _estimator = new();
    private static readonly DateOnly Today = new(2026, 6, 5);

    [Fact]
    public void Oil_change_due_by_km_projects_a_future_date_and_marks_ok_when_far()
    {
        // 10.000 km de intervalo, rodou 2.000 desde a última troca, 20 km/dia → ~400 dias → Ok.
        var input = new MaintenanceInput(
            IntervalKm: 10_000,
            IntervalDays: null,
            WhicheverComesFirst: true,
            LastServiceDate: new DateOnly(2026, 1, 1),
            LastServiceKm: 50_000,
            CurrentKm: 52_000,
            AvgDailyKm: 20);

        var result = _estimator.Evaluate(input, Today);

        result.DueAtKm.ShouldBe(60_000);
        result.EstimatedDueDate.ShouldBe(Today.AddDays(400));
        result.State.ShouldBe(MaintenanceState.Ok);
    }

    [Fact]
    public void Whichever_comes_first_picks_the_earlier_of_time_or_km()
    {
        // Por km daria ~400 dias; por tempo vence em 30 dias → vence o tempo, e está DueSoon? não (30 > 14) → Ok.
        var input = new MaintenanceInput(
            IntervalKm: 10_000,
            IntervalDays: 365,
            WhicheverComesFirst: true,
            LastServiceDate: Today.AddDays(-335), // vence por tempo em 30 dias
            LastServiceKm: 50_000,
            CurrentKm: 52_000,
            AvgDailyKm: 20);

        var result = _estimator.Evaluate(input, Today);

        result.DueByDate.ShouldBe(Today.AddDays(30));
        result.EstimatedDueDate.ShouldBe(Today.AddDays(30)); // tempo veio primeiro
        result.State.ShouldBe(MaintenanceState.Ok);
    }

    [Fact]
    public void Within_threshold_is_DueSoon()
    {
        var input = new MaintenanceInput(
            IntervalKm: null,
            IntervalDays: 365,
            WhicheverComesFirst: true,
            LastServiceDate: Today.AddDays(-358), // vence em 7 dias (< 14)
            LastServiceKm: 50_000,
            CurrentKm: 50_000,
            AvgDailyKm: 0);

        var result = _estimator.Evaluate(input, Today);

        result.State.ShouldBe(MaintenanceState.DueSoon);
    }

    [Fact]
    public void Past_km_target_is_Overdue()
    {
        var input = new MaintenanceInput(
            IntervalKm: 10_000,
            IntervalDays: null,
            WhicheverComesFirst: true,
            LastServiceDate: Today.AddDays(-100),
            LastServiceKm: 50_000,
            CurrentKm: 61_000, // já passou de 60.000
            AvgDailyKm: 30);

        var result = _estimator.Evaluate(input, Today);

        result.State.ShouldBe(MaintenanceState.Overdue);
    }
}
