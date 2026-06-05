using ALGarage.Domain.Common;

namespace ALGarage.Domain.Vehicles;

/// <summary>
/// Leitura de hodômetro registrada ao longo do tempo. Histórico melhora a estimativa de km/dia e dá
/// rastreabilidade. O km "atual" do veículo é a leitura mais recente.
/// </summary>
public sealed class OdometerReading : Entity
{
    private OdometerReading() { } // EF

    public OdometerReading(Guid vehicleId, int km, DateOnly recordedOn, OdometerSource source)
    {
        VehicleId = vehicleId;
        Km = km;
        RecordedOn = recordedOn;
        Source = source;
    }

    public Guid VehicleId { get; private set; }
    public int Km { get; private set; }
    public DateOnly RecordedOn { get; private set; }
    public OdometerSource Source { get; private set; }
}

public enum OdometerSource
{
    Manual = 0,
    Acquisition,
    Service
}
