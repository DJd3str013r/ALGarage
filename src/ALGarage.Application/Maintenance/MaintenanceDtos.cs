using ALGarage.Domain.Common;

namespace ALGarage.Application.Maintenance;

/// <summary>Status de um item de manutenção para exibição (o "vermelho" vem de State).</summary>
public sealed record MaintenanceStatusDto(
    string ItemName,
    string? Category,
    string? PartHint,
    MaintenanceState State,
    int? DueAtKm,
    DateOnly? EstimatedDueDate);

/// <summary>Resultado consolidado de manutenção de um veículo.</summary>
public sealed record VehicleMaintenanceDto(
    bool HasPlan,
    string? PlanName,
    IReadOnlyList<MaintenanceStatusDto> Items);
