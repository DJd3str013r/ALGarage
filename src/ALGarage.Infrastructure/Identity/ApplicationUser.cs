using Microsoft.AspNetCore.Identity;

namespace ALGarage.Infrastructure.Identity;

/// <summary>
/// Usuário da aplicação (ASP.NET Core Identity) com chave <see cref="Guid"/> — casa com
/// <c>Vehicle.UserId</c>. Campo de consentimento LGPD previsto (ADR-0011).
/// </summary>
public sealed class ApplicationUser : IdentityUser<Guid>
{
    public DateTime? ConsentAcceptedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
