using ALGarage.Application.Abstractions;
using ALGarage.Domain.Catalog;
using ALGarage.Domain.Maintenance;
using ALGarage.Domain.Service;
using ALGarage.Domain.Vehicles;
using ALGarage.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ALGarage.Infrastructure.Persistence;

/// <summary>
/// DbContext do EF Core (PostgreSQL). Combina ASP.NET Identity (usuário com chave Guid) com o
/// domínio do ÄLGarage. Configurações em Persistence/Configurations. Ver ADR-0003.
/// </summary>
public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options), IUnitOfWork
{
    // Garagem
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<OdometerReading> OdometerReadings => Set<OdometerReading>();

    // Histórico
    public DbSet<ServiceRecord> ServiceRecords => Set<ServiceRecord>();
    public DbSet<ServiceItem> ServiceItems => Set<ServiceItem>();

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
        base.OnModelCreating(modelBuilder); // Identity
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    Task<int> IUnitOfWork.SaveChangesAsync(CancellationToken ct) => SaveChangesAsync(ct);
}
