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
