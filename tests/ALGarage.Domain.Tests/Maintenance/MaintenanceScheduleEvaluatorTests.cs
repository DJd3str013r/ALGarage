using ALGarage.Domain.Common;
using ALGarage.Domain.Maintenance;
using Shouldly;
using Xunit;

namespace ALGarage.Domain.Tests.Maintenance;

public sealed class MaintenanceScheduleEvaluatorTests
{
    private readonly MaintenanceScheduleEvaluator _sut = new(new MaintenanceEstimator());
    private static readonly DateOnly Today = new(2026, 6, 5);

    private static MaintenanceItem Item(string name, int? km, int? months) => new()
    {
        Name = name,
        Category = "Óleo",
        IntervalKm = km,
        IntervalMonths = months,
        WhicheverComesFirst = true,
        PartHint = "óleo Volvo V40"
    };

    [Fact]
    public void Uses_acquisition_baseline_when_item_never_serviced()
    {
        // Adquirido há 13 meses a 50.000 km; óleo a cada 12 meses → vencido (sem histórico).
        var vehicle = new VehicleMaintenanceContext(
            AcquisitionDate: Today.AddMonths(-13),
            AcquisitionKm: 50_000,
            CurrentKm: 55_000,
            AvgDailyKm: 15);

        var items = new[] { Item("Óleo", km: 20_000, months: 12) };

        var result = _sut.Evaluate(vehicle, items, new Dictionary<string, LastServiceForItem>(), Today);

        result.ShouldHaveSingleItem();
        result[0].Evaluation.State.ShouldBe(MaintenanceState.Overdue);
    }

    [Fact]
    public void Uses_last_service_to_reset_the_clock()
    {
        var vehicle = new VehicleMaintenanceContext(
            AcquisitionDate: Today.AddMonths(-13),
            AcquisitionKm: 50_000,
            CurrentKm: 55_000,
            AvgDailyKm: 15);

        var items = new[] { Item("Óleo", km: 20_000, months: 12) };

        // Óleo trocado há 1 mês a 54.000 km → volta a ficar Ok.
        var history = new Dictionary<string, LastServiceForItem>
        {
            ["Óleo"] = new(Today.AddMonths(-1), 54_000)
        };

        var result = _sut.Evaluate(vehicle, items, history, Today);

        result[0].Evaluation.State.ShouldBe(MaintenanceState.Ok);
    }

    [Fact]
    public void Orders_most_urgent_first()
    {
        var vehicle = new VehicleMaintenanceContext(Today.AddMonths(-24), 40_000, 70_000, 30);
        var items = new[]
        {
            Item("Fluido de freio", km: null, months: 24),  // vencido (24 meses)
            Item("Velas", km: 200_000, months: 240)         // tranquilo
        };

        var result = _sut.Evaluate(vehicle, items, new Dictionary<string, LastServiceForItem>(), Today);

        result[0].ItemName.ShouldBe("Fluido de freio");
        result[0].Evaluation.State.ShouldBe(MaintenanceState.Overdue);
    }
}
