using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ALGarage.Infrastructure.Persistence;

/// <summary>
/// Permite gerar migrations sem subir o app:
///   dotnet ef migrations add InitialCreate -p src/ALGarage.Infrastructure -s src/ALGarage.Web
/// A connection string vem de ALGARAGE_DESIGN_CONNECTION ou de um valor local de placeholder
/// (não é usada de fato durante a geração da migration).
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var conn = Environment.GetEnvironmentVariable("ALGARAGE_DESIGN_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=algarage;Username=algarage;Password=algarage";

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(conn)
            .Options;

        return new ApplicationDbContext(options);
    }
}
