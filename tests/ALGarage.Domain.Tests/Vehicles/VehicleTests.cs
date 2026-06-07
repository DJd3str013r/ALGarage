using ALGarage.Domain.Vehicles;
using Shouldly;
using Xunit;

namespace ALGarage.Domain.Tests.Vehicles;

public sealed class VehicleTests
{
    private static readonly Vin SampleVin = Vin.Parse("YV1MV7320G1234567");
    private static readonly DateOnly Purchase = new(2024, 1, 1);

    [Fact]
    public void Rejects_current_km_below_acquisition_km()
    {
        var ex = Should.Throw<ArgumentException>(() => new Vehicle(
            Guid.NewGuid(), SampleVin, Purchase, previousOwners: 1,
            odometerAtAcquisitionKm: 50_000, currentOdometerKm: 49_000, avgDailyKm: 20));

        ex.Message.ShouldContain("Km atual");
    }

    [Fact]
    public void Rejects_negative_previous_owners()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => new Vehicle(
            Guid.NewGuid(), SampleVin, Purchase, previousOwners: -1,
            odometerAtAcquisitionKm: 0, currentOdometerKm: 0, avgDailyKm: 20));
    }

    [Fact]
    public void Update_odometer_rejects_going_backwards()
    {
        var v = new Vehicle(Guid.NewGuid(), SampleVin, Purchase, 1, 50_000, 55_000, 20);

        Should.Throw<ArgumentException>(() => v.UpdateCurrentOdometer(54_000));
        v.UpdateCurrentOdometer(60_000);
        v.CurrentOdometerKm.ShouldBe(60_000);
    }
}
