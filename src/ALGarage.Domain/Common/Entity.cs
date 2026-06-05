namespace ALGarage.Domain.Common;

/// <summary>
/// Base de toda entidade com identidade própria. Esqueleto — auditoria/eventos de domínio
/// serão acrescentados conforme as features evoluírem.
/// </summary>
public abstract class Entity
{
    public Guid Id { get; protected set; } = Guid.CreateVersion7();

    public DateTime CreatedAtUtc { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; protected set; }
}

/// <summary>
/// Raiz de agregado: ponto de entrada transacional e de consistência de um agregado.
/// </summary>
public abstract class AggregateRoot : Entity;
