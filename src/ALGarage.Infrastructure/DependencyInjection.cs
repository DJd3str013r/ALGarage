using ALGarage.Application.Abstractions;
using ALGarage.Infrastructure.Parts;
using ALGarage.Infrastructure.Persistence;
using ALGarage.Infrastructure.Vin;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ALGarage.Infrastructure;

/// <summary>Composição da camada de Infrastructure no contêiner de DI.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Postgres (string de conexão "Default" — definida por ambiente/secret, nunca no código).
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default")));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());

        // Portas → implementações (stubs nesta fase de estruturação).
        services.AddScoped<IVinDecoder, NhtsaVinDecoder>();
        services.AddScoped<IPartsSearchLinkProvider, MercadoLivreSearchLinkProvider>();

        return services;
    }
}
