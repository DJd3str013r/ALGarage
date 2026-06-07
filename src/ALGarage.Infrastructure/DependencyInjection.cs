using ALGarage.Application.Abstractions;
using ALGarage.Infrastructure.Email;
using ALGarage.Infrastructure.Notifications;
using ALGarage.Infrastructure.Parts;
using ALGarage.Infrastructure.Persistence;
using ALGarage.Infrastructure.Persistence.Repositories;
using ALGarage.Infrastructure.Seed;
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

        // Repositórios.
        services.AddScoped<ICatalogRepository, CatalogRepository>();
        services.AddScoped<IGarageRepository, GarageRepository>();

        // Decodificação de VIN: cadeia vPIC (stub) → curado Volvo (fallback garantido).
        services.AddScoped<IVinDecoder>(_ => new CompositeVinDecoder(
        [
            new NhtsaVinDecoder(),
            new CuratedVolvoVinDecoder()
        ]));

        // Buscador de peças (links). Adicionar lojas = registrar mais provedores.
        services.AddScoped<IPartsSearchLinkProvider, MercadoLivreSearchLinkProvider>();

        // Seed do catálogo curado.
        services.AddScoped<CuratedDataSeeder>();

        // Notificações de manutenção (e-mail). Desabilitado por padrão; configure "Email"/"Reminders".
        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));
        services.Configure<ReminderOptions>(configuration.GetSection(ReminderOptions.SectionName));
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddScoped<IReminderQueries, ReminderQueries>();
        services.AddHostedService<MaintenanceReminderWorker>();

        return services;
    }
}
