using ALGarage.Domain.Catalog;
using ALGarage.Domain.Common;
using ALGarage.Domain.Maintenance;
using ALGarage.Domain.Upgrades;
using ALGarage.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ALGarage.Infrastructure.Seed;

/// <summary>
/// Semeia o catálogo curado (Volvo V40) no banco, de forma idempotente: se já houver marca, não faz
/// nada. Mapeia o JSON (CuratedDatasetLoader) para as entidades de domínio. Ver docs/09-dataset-v40.md.
/// </summary>
public sealed class CuratedDataSeeder(ApplicationDbContext db, ILogger<CuratedDataSeeder> logger)
{
    public async Task SeedAsync(CancellationToken ct = default)
    {
        if (await db.Brands.AnyAsync(ct))
        {
            return; // já semeado
        }

        var data = CuratedDatasetLoader.LoadV40();
        logger.LogInformation("Semeando catálogo curado: {Scope}", data.ModelScope);

        var brand = new Brand { Name = data.Brand.Name, Slug = data.Brand.Slug };
        db.Brands.Add(brand);

        var modelsByKey = data.Models.ToDictionary(
            m => m.Key,
            m => new VehicleModel { BrandId = brand.Id, Name = m.Name, Generation = m.Generation, BodyStyle = m.BodyStyle });
        db.VehicleModels.AddRange(modelsByKey.Values);

        var enginesByCode = data.Engines.ToDictionary(
            e => e.Code,
            e => new EngineSpec
            {
                BrandId = brand.Id,
                Code = e.Code,
                Family = e.Family,
                FuelType = ParseFuel(e.Fuel),
                DisplacementCc = e.DisplacementCc,
                PowerHp = e.PowerHpVariants.Count > 0 ? e.PowerHpVariants.Max() : 0,
                Cylinders = e.Cylinders,
                Aspiration = ParseAspiration(e.Aspiration),
                OilGrade = e.OilGrade,
                OilCapacityLiters = e.OilCapacityLiters
            });
        db.EngineSpecs.AddRange(enginesByCode.Values);

        var variants = data.Variants
            .Where(v => modelsByKey.ContainsKey(v.ModelKey) && enginesByCode.ContainsKey(v.EngineCode))
            .Select(v => new ModelVariant
            {
                VehicleModelId = modelsByKey[v.ModelKey].Id,
                EngineSpecId = enginesByCode[v.EngineCode].Id,
                Trim = v.Trim,
                MarketingEngine = v.MarketingEngine,
                ModelYearFrom = v.ModelYearFrom,
                ModelYearTo = v.ModelYearTo,
                Market = v.Market,
                Transmission = v.Transmission,
                Drivetrain = v.Drivetrain
            })
            .ToList();
        db.ModelVariants.AddRange(variants);

        // Opcionais ficam no modelo principal (V40 hatch), por simplicidade do MVP.
        var mainModel = modelsByKey.TryGetValue("v40", out var v40) ? v40 : modelsByKey.Values.First();
        var options = data.FactoryOptions.Select(o => new FactoryOption
        {
            VehicleModelId = mainModel.Id,
            Code = o.Code,
            Category = o.Category,
            Name = o.Name,
            Description = o.Description,
            IsStandard = o.Standard
        });
        db.FactoryOptions.AddRange(options);

        // Um plano por família de motor (clonando os itens) → busca direta por EngineFamily.
        var plans = new List<MaintenancePlan>();
        foreach (var plan in data.MaintenancePlans)
        {
            foreach (var family in plan.AppliesToFamilies)
            {
                plans.Add(new MaintenancePlan
                {
                    BrandId = brand.Id,
                    EngineFamily = family,
                    Name = plan.Name,
                    SourceRef = "curated:volvo-v40-2012-2019",
                    Items = plan.Items.Select(i => new MaintenanceItem
                    {
                        Name = i.Name,
                        Category = i.Category,
                        IntervalKm = i.IntervalKm,
                        IntervalMonths = i.IntervalMonths,
                        WhicheverComesFirst = i.WhicheverFirst,
                        PartHint = i.PartHint,
                        Notes = i.Notes
                    }).ToList()
                });
            }
        }
        db.MaintenancePlans.AddRange(plans);

        // Upgrades + stages (ILUSTRATIVO).
        var upgrades = (data.Upgrades ?? [])
            .Where(u => modelsByKey.ContainsKey(u.ModelKey))
            .Select(u => new Upgrade
            {
                VehicleModelId = modelsByKey[u.ModelKey].Id,
                EngineFamily = u.EngineFamily,
                Type = ParseUpgradeType(u.Type),
                Name = u.Name,
                Description = u.Description,
                Stages = u.Stages.Select(s => new Stage
                {
                    Level = s.Level,
                    Name = s.Name,
                    Description = s.Description,
                    GainsHp = s.GainsHp,
                    GainsNm = s.GainsNm,
                    Requirements = s.Requirements
                }).ToList()
            })
            .ToList();
        db.Upgrades.AddRange(upgrades);

        await db.SaveChangesAsync(ct);
        logger.LogInformation(
            "Catálogo semeado: {Models} modelos, {Engines} motores, {Variants} versões, {Plans} planos, {Upgrades} upgrades.",
            modelsByKey.Count, enginesByCode.Count, variants.Count, plans.Count, upgrades.Count);
    }

    private static FuelType ParseFuel(string fuel) => fuel switch
    {
        "Gasoline" => FuelType.Gasoline,
        "Diesel" => FuelType.Diesel,
        "Flex" => FuelType.Flex,
        "Ethanol" => FuelType.Ethanol,
        "Hybrid" => FuelType.Hybrid,
        "Electric" => FuelType.Electric,
        _ => FuelType.Unknown
    };

    private static UpgradeType ParseUpgradeType(string t) => t switch
    {
        "Performance" => UpgradeType.Performance,
        "Aesthetic" => UpgradeType.Aesthetic,
        _ => UpgradeType.Performance
    };

    private static Aspiration ParseAspiration(string a) => a switch
    {
        "Turbo" => Aspiration.Turbo,
        "Supercharged" => Aspiration.Supercharged,
        "TwinCharged" => Aspiration.TwinCharged,
        "Naturally" => Aspiration.Naturally,
        _ => Aspiration.Unknown
    };
}
