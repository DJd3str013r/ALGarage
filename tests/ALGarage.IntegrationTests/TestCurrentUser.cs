using ALGarage.Application.Abstractions;

namespace ALGarage.IntegrationTests;

/// <summary>ICurrentUser fixo para os testes de integração (substitui o do HttpContext).</summary>
internal sealed class TestCurrentUser(Guid userId) : ICurrentUser
{
    public Guid? UserId => userId;
    public bool IsAuthenticated => true;
}
