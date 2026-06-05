using ALGarage.Domain.Catalog;
using ALGarage.Domain.Maintenance;
using ALGarage.Domain.Service;
using ALGarage.Domain.Vehicles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ALGarage.Infrastructure.Persistence.Configurations;

internal sealed class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> b)
    {
        b.ToTable("vehicles");
        b.HasKey(v => v.Id);

        b.Property(v => v.Vin)
            // Qualificado: 'Vin' bare colidiria com o namespace ALGarage.Infrastructure.Vin.
            .HasConversion(vin => vin.Value, value => ALGarage.Domain.Vehicles.Vin.Parse(value))
            .HasMaxLength(17)
            .IsRequired();

        b.HasIndex(v => new { v.UserId, v.Vin }).IsUnique();
        b.Property(v => v.Nickname).HasMaxLength(80);
    }
}

internal sealed class OdometerReadingConfiguration : IEntityTypeConfiguration<OdometerReading>
{
    public void Configure(EntityTypeBuilder<OdometerReading> b)
    {
        b.ToTable("odometer_readings");
        b.HasKey(o => o.Id);
        b.Property(o => o.Source).HasConversion<string>().HasMaxLength(20);
        b.HasIndex(o => o.VehicleId);
    }
}

internal sealed class ServiceRecordConfiguration : IEntityTypeConfiguration<ServiceRecord>
{
    public void Configure(EntityTypeBuilder<ServiceRecord> b)
    {
        b.ToTable("service_records");
        b.HasKey(r => r.Id);
        b.Property(r => r.Workshop).HasMaxLength(120);
        b.Property(r => r.Notes).HasMaxLength(1000);
        b.Property(r => r.TotalCost).HasColumnType("numeric(12,2)");
        b.HasIndex(r => r.VehicleId);

        b.HasMany(r => r.Items)
            .WithOne()
            .HasForeignKey(i => i.ServiceRecordId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Metadata.FindNavigation(nameof(ServiceRecord.Items))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class ServiceItemConfiguration : IEntityTypeConfiguration<ServiceItem>
{
    public void Configure(EntityTypeBuilder<ServiceItem> b)
    {
        b.ToTable("service_items");
        b.HasKey(i => i.Id);
        b.Property(i => i.Description).HasMaxLength(200).IsRequired();
        b.Property(i => i.MaintenanceItemKey).HasMaxLength(120);
        b.Property(i => i.Cost).HasColumnType("numeric(12,2)");
    }
}

internal sealed class BrandConfiguration : IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> b)
    {
        b.ToTable("brands");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(60).IsRequired();
        b.Property(x => x.Slug).HasMaxLength(60).IsRequired();
        b.HasIndex(x => x.Slug).IsUnique();
    }
}

internal sealed class VehicleModelConfiguration : IEntityTypeConfiguration<VehicleModel>
{
    public void Configure(EntityTypeBuilder<VehicleModel> b)
    {
        b.ToTable("vehicle_models");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(60).IsRequired();
        b.Property(x => x.Generation).HasMaxLength(40);
        b.Property(x => x.BodyStyle).HasMaxLength(40);
        b.HasIndex(x => x.BrandId);
    }
}

internal sealed class ModelVariantConfiguration : IEntityTypeConfiguration<ModelVariant>
{
    public void Configure(EntityTypeBuilder<ModelVariant> b)
    {
        b.ToTable("model_variants");
        b.HasKey(x => x.Id);
        b.Property(x => x.Trim).HasMaxLength(80).IsRequired();
        b.Property(x => x.MarketingEngine).HasMaxLength(10);
        b.Property(x => x.Market).HasMaxLength(10);
        b.Property(x => x.Transmission).HasMaxLength(10);
        b.Property(x => x.Drivetrain).HasMaxLength(10);
        b.Property(x => x.SpecsJson).HasColumnType("jsonb");
        b.HasIndex(x => x.VehicleModelId);
        b.HasIndex(x => new { x.ModelYearFrom, x.ModelYearTo });
    }
}

internal sealed class EngineSpecConfiguration : IEntityTypeConfiguration<EngineSpec>
{
    public void Configure(EntityTypeBuilder<EngineSpec> b)
    {
        b.ToTable("engine_specs");
        b.HasKey(x => x.Id);
        b.Property(x => x.Code).HasMaxLength(20).IsRequired();
        b.Property(x => x.Family).HasMaxLength(40).IsRequired();
        b.Property(x => x.FuelType).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.Aspiration).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.OilGrade).HasMaxLength(40);
        b.HasIndex(x => x.Family);
    }
}

internal sealed class FactoryOptionConfiguration : IEntityTypeConfiguration<FactoryOption>
{
    public void Configure(EntityTypeBuilder<FactoryOption> b)
    {
        b.ToTable("factory_options");
        b.HasKey(x => x.Id);
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Category).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(120).IsRequired();
        b.Property(x => x.Description).HasMaxLength(400);
        b.HasIndex(x => x.VehicleModelId);
    }
}

internal sealed class MaintenancePlanConfiguration : IEntityTypeConfiguration<MaintenancePlan>
{
    public void Configure(EntityTypeBuilder<MaintenancePlan> b)
    {
        b.ToTable("maintenance_plans");
        b.HasKey(x => x.Id);
        b.Property(x => x.EngineFamily).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(120).IsRequired();
        b.Property(x => x.SourceRef).HasMaxLength(120);
        b.HasIndex(x => x.EngineFamily);

        b.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(i => i.MaintenancePlanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class MaintenanceItemConfiguration : IEntityTypeConfiguration<MaintenanceItem>
{
    public void Configure(EntityTypeBuilder<MaintenanceItem> b)
    {
        b.ToTable("maintenance_items");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(120).IsRequired();
        b.Property(x => x.Category).HasMaxLength(40);
        b.Property(x => x.PartHint).HasMaxLength(200);
        b.Property(x => x.Notes).HasMaxLength(400);
    }
}
