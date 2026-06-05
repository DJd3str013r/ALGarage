namespace ALGarage.Application.Abstractions;

/// <summary>Usuário autenticado da requisição atual. Implementado na camada Web (HttpContext).</summary>
public interface ICurrentUser
{
    Guid? UserId { get; }
    bool IsAuthenticated { get; }

    /// <summary>Conveniência: lança se não autenticado. Use em serviços que exigem login.</summary>
    Guid RequireUserId() => UserId ?? throw new UnauthorizedAccessException("Usuário não autenticado.");
}
