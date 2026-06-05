using ALGarage.Application.Abstractions;
using ALGarage.Domain.Catalog;
using ALGarage.Domain.Maintenance;
using ALGarage.Domain.Vehicles;
using Microsoft.EntityFrameworkCore;

namespace ALGarage.Infrastructure.Persistence;

/// <summary>
/// DbContext do EF Core (PostgreSQL). Esqueleto: DbSets das entidades principais para refletir o
/// modelo de dados. Configurações detalhadas (JSONB, índices, conversões de value object como
/// <see cref="Vin"/>) entram em IEntityTypeConfiguration por módulo nas features. Ver ADR-0003.
/// </summary>
public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IUnitOfWork
{
    // Garagem
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();

    // Catálogo
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<VehicleModel> VehicleModels => Set<VehicleModel>();
    public DbSet<ModelVariant> ModelVariants => Set<ModelVariant>();
    public DbSet<EngineSpec> EngineSpecs => Set<EngineSpec>();
    public DbSet<FactoryOption> FactoryOptions => Set<FactoryOption>();

    // Manutenção
    public DbSet<MaintenancePlan> MaintenancePlans => Set<MaintenancePlan>();
    public DbSet<MaintenanceItem> MaintenanceItems => Set<MaintenanceItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // TODO(feature): aplicar configurações por módulo:
        //   modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        // Inclui: conversão do value object Vin <-> string, índice único de Vin por usuário,
        // mapeamento de SpecsJson para jsonb, etc.
        base.OnModelCreating(modelBuilder);
    }

    Task<int> IUnitOfWork.SaveChangesAsync(CancellationToken ct) => SaveChangesAsync(ct);
}
