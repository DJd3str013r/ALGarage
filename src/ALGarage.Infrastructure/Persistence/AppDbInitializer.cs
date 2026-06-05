using ALGarage.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ALGarage.Infrastructure.Persistence;

/// <summary>
/// Aplica migrations pendentes e semeia o catálogo curado na inicialização do app.
/// Chamado uma vez no Program.cs. Idempotente.
/// </summary>
public static class AppDbInitializer
{
    public static async Task RunAsync(IServiceProvider services, CancellationToken ct = default)
    {
        await using var scope = services.CreateAsyncScope();
        var sp = scope.ServiceProvider;
        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("AppDbInitializer");
        var db = sp.GetRequiredService<ApplicationDbContext>();

        logger.LogInformation("Aplicando migrations…");
        await db.Database.MigrateAsync(ct);

        var seeder = sp.GetRequiredService<CuratedDataSeeder>();
        await seeder.SeedAsync(ct);
    }
}
