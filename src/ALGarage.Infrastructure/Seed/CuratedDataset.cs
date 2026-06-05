namespace ALGarage.Infrastructure.Seed;

/// <summary>
/// Modelo fortemente tipado do dataset curado (JSON em Seed/Data). Reflete o catálogo do Volvo V40
/// 2012–2019: marca, modelos, motores, versões, opcionais e planos de manutenção.
/// Ver docs/09-dataset-v40.md.
/// </summary>
public sealed record CuratedDataset(
    int SchemaVersion,
    string Source,
    string BrandScope,
    string ModelScope,
    string Disclaimer,
    BrandSeed Brand,
    IReadOnlyList<ModelSeed> Models,
    IReadOnlyList<EngineSeed> Engines,
    IReadOnlyList<VariantSeed> Variants,
    IReadOnlyList<FactoryOptionSeed> FactoryOptions,
    IReadOnlyList<MaintenancePlanSeed> MaintenancePlans);

public sealed record BrandSeed(string Name, string Slug, string? Country);

public sealed record ModelSeed(
    string Key, string Name, string Generation, string BodyStyle, int YearsFrom, int YearsTo);

public sealed record EngineSeed(
    string Code,
    string Family,
    IReadOnlyList<string> MarketingNames,
    string Fuel,
    int DisplacementCc,
    IReadOnlyList<int> PowerHpVariants,
    string Aspiration,
    int Cylinders,
    string Phase,
    int YearsFrom,
    int YearsTo,
    string? OilGrade,
    double? OilCapacityLiters,
    string? Notes);

public sealed record VariantSeed(
    string ModelKey,
    string Trim,
    string EngineCode,
    string MarketingEngine,
    int ModelYearFrom,
    int ModelYearTo,
    string Market,
    string Transmission,
    string Drivetrain,
    string Phase);

public sealed record FactoryOptionSeed(
    string Code, string Category, string Name, string Description, bool Standard = false);

public sealed record MaintenancePlanSeed(
    string Key,
    IReadOnlyList<string> AppliesToFamilies,
    string Name,
    IReadOnlyList<MaintenanceItemSeed> Items);

public sealed record MaintenanceItemSeed(
    string Name,
    string Category,
    int? IntervalKm,
    int? IntervalMonths,
    bool WhicheverFirst,
    string? PartHint,
    string? Notes);
