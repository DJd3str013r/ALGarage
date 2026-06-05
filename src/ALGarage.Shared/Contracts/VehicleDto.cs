namespace ALGarage.Shared.Contracts;

/// <summary>
/// Contrato de leitura de um veículo para a UI. Exemplo de DTO compartilhado servidor↔cliente.
/// Mantém a UI desacoplada das entidades de domínio e prepara o terreno para o futuro render Auto.
/// </summary>
public sealed record VehicleDto(
    Guid Id,
    string Vin,
    string? Nickname,
    string? Make,
    string? Model,
    int? ModelYear,
    int CurrentOdometerKm,
    double AvgDailyKm,
    bool Identified);
