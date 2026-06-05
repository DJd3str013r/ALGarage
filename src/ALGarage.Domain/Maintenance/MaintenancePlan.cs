using ALGarage.Domain.Common;

namespace ALGarage.Domain.Maintenance;

/// <summary>
/// Cronograma de manutenção de um modelo/motor. "Dados, não código": os intervalos moram no banco,
/// não em <c>if</c>s. Ver ADR-0009.
/// </summary>
public sealed class MaintenancePlan : Entity
{
    public Guid ModelVariantId { get; set; }
    public required string Name { get; set; }
    public string? SourceRef { get; set; } // proveniência (manual de fábrica, curado, etc.)

    public List<MaintenanceItem> Items { get; set; } = [];
}

/// <summary>
/// Um item de manutenção. Pode ter intervalo por km, por tempo, ou ambos
/// (<see cref="WhicheverComesFirst"/>). Tempo é representado em dias para simplicidade do esqueleto;
/// na modelagem final pode virar ISO-8601 duration.
/// </summary>
public sealed class MaintenanceItem : Entity
{
    public Guid MaintenancePlanId { get; set; }
    public required string Name { get; set; }       // ex.: "Troca de óleo e filtro"

    public int? IntervalKm { get; set; }            // ex.: 10000
    public int? IntervalDays { get; set; }          // ex.: 365

    /// <summary>Quando ambos os intervalos existem, vence o que ocorrer primeiro.</summary>
    public bool WhicheverComesFirst { get; set; } = true;

    /// <summary>Peça associada (quando houver referência no catálogo). Opcional.</summary>
    public Guid? PartReferenceId { get; set; }
}
