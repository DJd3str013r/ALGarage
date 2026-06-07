using ALGarage.Application;
using ALGarage.Application.Abstractions;
using ALGarage.Infrastructure;
using ALGarage.Infrastructure.Persistence;
using ALGarage.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Testcontainers.PostgreSql;
using Xunit;

namespace ALGarage.IntegrationTests;

/// <summary>
/// Sobe um PostgreSQL real (Testcontainers), cria o schema (EnsureCreated) e semeia o catálogo do
/// V40 uma vez. Os testes compartilham o banco; cada teste usa um userId próprio para isolar.
/// Requer Docker disponível (no CI ubuntu-latest e no dev).
/// </summary>
public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using var db = CreateDbContext();
        await db.Database.EnsureCreatedAsync();

        var seeder = new CuratedDataSeeder(db, NullLogger<CuratedDataSeeder>.Instance);
        await seeder.SeedAsync();
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();

    private ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;
        return new ApplicationDbContext(options);
    }

    /// <summary>Monta o provedor de DI real (mesma composição do app), com um usuário de teste.</summary>
    public ServiceProvider BuildProvider(Guid userId)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = ConnectionString
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApplication();
        services.AddInfrastructure(config);
        services.AddScoped<ICurrentUser>(_ => new TestCurrentUser(userId));
        return services.BuildServiceProvider();
    }
}

[CollectionDefinition(nameof(PostgresCollection))]
public sealed class PostgresCollection : ICollectionFixture<PostgresFixture>;
