using ALGarage.Application.Catalog;
using ALGarage.Domain.Common;

namespace ALGarage.Application.Vehicles;

/// <summary>Resultado da decodificação do VIN, com candidatos de versão para o usuário confirmar.</summary>
public sealed record VinDecodeResultDto(
    bool Identified,
    string Vin,
    string? Wmi,
    int? ModelYear,
    string? Make,
    string? Model,
    DataSource Source,
    IReadOnlyList<ModelVariantSummary> CandidateVariants);

/// <summary>Pedido de cadastro de um veículo na garagem (VIN obrigatório).</summary>
public sealed record RegisterVehicleRequest(
    string Vin,
    DateOnly PurchaseDate,
    int PreviousOwners,
    int OdometerAtAcquisitionKm,
    int CurrentOdometerKm,
    double AvgDailyKm,
    string? Nickname,
    Guid? ModelVariantId);

/// <summary>Veículo resumido para a lista da garagem (com alerta de manutenção).</summary>
public sealed record VehicleSummaryDto(
    Guid Id,
    string Vin,
    string? Nickname,
    string Display,        // "V40 R-Design (2017)" ou "VIN não identificado"
    int CurrentOdometerKm,
    bool Identified,
    int OverdueCount,
    int DueSoonCount);

/// <summary>Detalhe do veículo (cabeçalho da tela). Status de manutenção vêm em serviço à parte.</summary>
public sealed record VehicleDetailDto(
    Guid Id,
    string Vin,
    string? Nickname,
    DateOnly PurchaseDate,
    int PreviousOwners,
    int OdometerAtAcquisitionKm,
    int CurrentOdometerKm,
    double AvgDailyKm,
    ModelVariantDetail? Variant);
