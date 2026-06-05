using ALGarage.Domain.Vehicles;
using Shouldly;
using Xunit;

namespace ALGarage.Domain.Tests.Vehicles;

public sealed class VinModelYearTests
{
    [Theory]
    [InlineData("YV1MV7320G1234567", 2016)] // 10º caractere 'G' → 2016
    [InlineData("YV1MV7320C1234567", 2012)] // 'C' → 2012
    [InlineData("YV1MV7320K1234567", 2019)] // 'K' → 2019
    public void Decodes_model_year_from_position_10(string vin, int expectedYear)
    {
        var parsed = Vin.Parse(vin);

        VinModelYear.TryDecode(parsed).ShouldBe(expectedYear);
    }

    [Fact]
    public void Volvo_wmi_is_extracted()
    {
        var vin = Vin.Parse("YV1MV7320G1234567");

        vin.Wmi.ShouldBe("YV1"); // WMI Volvo Cars (Suécia)
    }
}
