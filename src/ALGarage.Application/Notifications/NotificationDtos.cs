using ALGarage.Domain.Common;

namespace ALGarage.Application.Notifications;

public sealed record DueItem(string Name, MaintenanceState State);

public sealed record VehicleReminder(string Display, IReadOnlyList<DueItem> Items);

/// <summary>Digest de manutenção pendente de um usuário (um e-mail).</summary>
public sealed record UserReminder(string Email, IReadOnlyList<VehicleReminder> Vehicles);
