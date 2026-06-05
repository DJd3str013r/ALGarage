namespace ALGarage.Application.Abstractions;

/// <summary>
/// Porta de persistência transacional. Implementada na Infrastructure sobre o DbContext do EF Core.
/// Mantém a Application ignorante quanto ao ORM.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
