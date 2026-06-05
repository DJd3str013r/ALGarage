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
    public required string Name { get; set; }       // ex.: "V40"
    public string? Generation { get; set; }          // ex.: "II (P1)"
    public string? BodyStyle { get; set; }           // ex.: "Hatchback"
}

/// <summary>
/// Motor — entidade de catálogo COMPARTILHADA por várias versões (um motor equipa vários trims/anos).
/// <see cref="Family"/> liga a um <see cref="ALGarage.Domain.Maintenance.MaintenancePlan"/>.
/// </summary>
public sealed class EngineSpec : Entity
{
    public Guid BrandId { get; set; }
    public required string Code { get; set; }        // ex.: "B4204T"
    public required string Family { get; set; }      // ex.: "petrol-driveE-2.0" (chave p/ plano de manutenção)
    public FuelType FuelType { get; set; }
    public int DisplacementCc { get; set; }
    public int PowerHp { get; set; }
    public int Cylinders { get; set; }
    public Aspiration Aspiration { get; set; }

    /// <summary>Especificação de óleo recomendada pela Volvo (ex.: "0W-20 (VCC RBS0-2AE)").</summary>
    public string? OilGrade { get; set; }
    public double? OilCapacityLiters { get; set; }
}

/// <summary>A "versão" do veículo, resolvida pela decodificação do VIN. Referencia um motor.</summary>
public sealed class ModelVariant : Entity
{
    public Guid VehicleModelId { get; set; }
    public Guid? EngineSpecId { get; set; }
    public required string Trim { get; set; }        // ex.: "R-Design"
    public int ModelYearFrom { get; set; }
    public int ModelYearTo { get; set; }
    public string? Market { get; set; }              // ex.: "BR"
    public string? Transmission { get; set; }        // "AT" / "MT"
    public string? Drivetrain { get; set; }          // "FWD" / "AWD"

    /// <summary>Specs/opcionais heterogêneos (JSONB no Postgres).</summary>
    public string? SpecsJson { get; set; }
}

public sealed class FactoryOption : Entity
{
    public Guid VehicleModelId { get; set; }
    public required string Code { get; set; }
    public required string Category { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public bool IsStandard { get; set; }
}
