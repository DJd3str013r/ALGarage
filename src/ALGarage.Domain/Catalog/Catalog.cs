using ALGarage.Domain.Common;

namespace ALGarage.Domain.Catalog;

/// <summary>
/// Catálogo de fábrica / dados curados. Entidades-esqueleto: propriedades suficientes para o esboço
/// do modelo de dados; comportamento e validações entram com as features.
/// Specs/opcionais heterogêneos vão para JSONB (ex.: <see cref="ModelVariant.SpecsJson"/>).
/// </summary>
public sealed class Brand : Entity
{
    public required string Name { get; set; }
    public required string Slug { get; set; } // ex.: "volvo"
}

public sealed class VehicleModel : Entity
{
    public Guid BrandId { get; set; }
    public required string Name { get; set; }       // ex.: "XC60"
    public string? Generation { get; set; }          // ex.: "II"
}

/// <summary>A "versão" do veículo, resolvida pela decodificação do VIN.</summary>
public sealed class ModelVariant : Entity
{
    public Guid VehicleModelId { get; set; }
    public required string Trim { get; set; }        // ex.: "T5 Momentum"
    public int ModelYear { get; set; }
    public string? Market { get; set; }              // ex.: "BR"

    /// <summary>Specs/opcionais heterogêneos (JSONB no Postgres).</summary>
    public string? SpecsJson { get; set; }
}

public sealed class EngineSpec : Entity
{
    public Guid ModelVariantId { get; set; }
    public required string Code { get; set; }        // ex.: "B4204T"
    public FuelType FuelType { get; set; }
    public int DisplacementCc { get; set; }
    public int PowerHp { get; set; }
    public Aspiration Aspiration { get; set; }
}

public sealed class FactoryOption : Entity
{
    public Guid ModelVariantId { get; set; }
    public required string Code { get; set; }
    public required string Category { get; set; }
    public required string Name { get; set; }
}
