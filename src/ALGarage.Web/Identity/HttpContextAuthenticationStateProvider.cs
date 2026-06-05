using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace ALGarage.Web.Identity;

/// <summary>
/// Fornece o estado de autenticação a partir do HttpContext (render estático/SSR). Suficiente para
/// as telas da garagem do MVP, que são SSR. Para componentes interativos com auth, trocar por um
/// provider revalidante no futuro.
/// </summary>
public sealed class HttpContextAuthenticationStateProvider(IHttpContextAccessor accessor)
    : AuthenticationStateProvider
{
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var user = accessor.HttpContext?.User ?? new ClaimsPrincipal(new ClaimsIdentity());
        return Task.FromResult(new AuthenticationState(user));
    }
}
