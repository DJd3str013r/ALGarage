using ALGarage.Domain.Common;

namespace ALGarage.Domain.Upgrades;

/// <summary>
/// Upgrade catalogado para um modelo. Performance liga-se a uma <see cref="EngineFamily"/> (ex.: só
/// o 2.0 Drive-E); estética aplica-se ao modelo (EngineFamily nulo). Ganhos são ILUSTRATIVOS.
/// </summary>
public sealed class Upgrade : Entity
{
    public Guid VehicleModelId { get; set; }
    public string? EngineFamily { get; set; }   // null = aplica ao modelo (estética)
    public UpgradeType Type { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    public List<Stage> Stages { get; set; } = [];
}

/// <summary>Um nível/estágio de um upgrade (ex.: Stage 1/2) ou um item estético.</summary>
public sealed class Stage : Entity
{
    public Guid UpgradeId { get; set; }
    public int Level { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int? GainsHp { get; set; }
    public int? GainsNm { get; set; }
    public string? Requirements { get; set; }
}
