using ALGarage.Domain.Common;

namespace ALGarage.Domain.Maintenance;

/// <summary>
/// Cronograma de manutenção associado a uma FAMÍLIA de motor (ex.: "petrol-driveE-2.0"). "Dados, não
/// código": os intervalos moram no banco, não em <c>if</c>s. Um plano vale para todas as versões
/// equipadas com motores daquela família. Ver ADR-0009 e docs/09-dataset-v40.md.
/// </summary>
public sealed class MaintenancePlan : Entity
{
    public Guid? BrandId { get; set; }
    public required string EngineFamily { get; set; }  // liga ao EngineSpec.Family
    public required string Name { get; set; }
    public string? SourceRef { get; set; }             // proveniência (curado, manual de fábrica, …)

    public List<MaintenanceItem> Items { get; set; } = [];
}

/// <summary>
/// Um item de manutenção. Pode ter intervalo por km, por tempo (meses), ou ambos
/// (<see cref="WhicheverComesFirst"/>).
/// </summary>
public sealed class MaintenanceItem : Entity
{
    public Guid MaintenancePlanId { get; set; }
    public required string Name { get; set; }       // ex.: "Troca de óleo e filtro de óleo"
    public string? Category { get; set; }           // ex.: "Óleo", "Filtros", "Distribuição"

    public int? IntervalKm { get; set; }            // ex.: 10000
    public int? IntervalMonths { get; set; }        // ex.: 12

    /// <summary>Quando ambos os intervalos existem, vence o que ocorrer primeiro.</summary>
    public bool WhicheverComesFirst { get; set; } = true;

    /// <summary>Termo de busca para o buscador de peças (links). Opcional.</summary>
    public string? PartHint { get; set; }

    public string? Notes { get; set; }
}
