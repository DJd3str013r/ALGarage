namespace ALGarage.Application.Catalog;

/// <summary>Versão (trim) resumida — para listas e confirmação de decodificação do VIN.</summary>
public sealed record ModelVariantSummary(
    Guid Id,
    string ModelName,
    string Trim,
    string MarketingEngine,
    int ModelYearFrom,
    int ModelYearTo,
    string? Market);

/// <summary>Detalhe de uma versão com modelo e motor — usado na tela do veículo.</summary>
public sealed record ModelVariantDetail(
    Guid Id,
    Guid VehicleModelId,
    string ModelName,
    string? Generation,
    string Trim,
    string? MarketingEngine,
    int ModelYearFrom,
    int ModelYearTo,
    string? Market,
    string? Transmission,
    string? Drivetrain,
    string EngineCode,
    string EngineFamily,
    string Fuel,
    int PowerHp,
    int DisplacementCc,
    string? OilGrade,
    double? OilCapacityLiters);

/// <summary>Opcional/extra de fábrica.</summary>
public sealed record FactoryOptionDto(
    string Code, string Category, string Name, string? Description, bool IsStandard);

/// <summary>Catálogo de fábrica de um modelo (todas as versões + opcionais).</summary>
public sealed record ModelFactoryInfoDto(
    string ModelName,
    IReadOnlyList<ModelVariantSummary> Versions,
    IReadOnlyList<FactoryOptionDto> Options);

public sealed record StageDto(
    int Level, string Name, string? Description, int? GainsHp, int? GainsNm, string? Requirements);

public sealed record UpgradeDto(
    string Type, string Name, string? Description, IReadOnlyList<StageDto> Stages);
