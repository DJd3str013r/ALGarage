using System.Security.Claims;
using ALGarage.Application.Abstractions;

namespace ALGarage.Web.Identity;

/// <summary>
/// <see cref="ICurrentUser"/> lendo o usuário do HttpContext. As páginas autenticadas da garagem
/// usam render estático (SSR), então o HttpContext (e o principal de cookie) está disponível.
/// </summary>
public sealed class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public Guid? UserId
    {
        get
        {
            var id = Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(id, out var guid) ? guid : null;
        }
    }
}
