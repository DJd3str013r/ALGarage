namespace ALGarage.Application.ServiceHistory;

public sealed record AddServiceItemDto(string Description, string? MaintenanceItemKey, decimal? Cost);

public sealed record AddServiceRecordRequest(
    Guid VehicleId,
    DateOnly PerformedOn,
    int OdometerKm,
    string? Workshop,
    decimal? TotalCost,
    string? Notes,
    IReadOnlyList<AddServiceItemDto> Items);

public sealed record ServiceItemDto(string Description, string? MaintenanceItemKey, decimal? Cost);

public sealed record ServiceRecordDto(
    Guid Id,
    DateOnly PerformedOn,
    int OdometerKm,
    string? Workshop,
    decimal? TotalCost,
    string? Notes,
    IReadOnlyList<ServiceItemDto> Items);
