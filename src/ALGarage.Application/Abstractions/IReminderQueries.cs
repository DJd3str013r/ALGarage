using ALGarage.Domain.Vehicles;

namespace ALGarage.Application.Abstractions;

/// <summary>Veículo de um usuário com e-mail, para o job de lembretes (varre TODOS os usuários).</summary>
public sealed record ReminderTarget(Vehicle Vehicle, string Email);

/// <summary>Consultas usadas pelo worker de lembretes (fora do contexto de um usuário logado).</summary>
public interface IReminderQueries
{
    Task<IReadOnlyList<ReminderTarget>> GetTargetsAsync(CancellationToken ct = default);
}
