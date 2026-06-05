using ALGarage.Application.Abstractions;
using ALGarage.Application.Catalog;
using ALGarage.Domain.Maintenance;
using Microsoft.EntityFrameworkCore;

namespace ALGarage.Infrastructure.Persistence.Repositories;

internal sealed class CatalogRepository(ApplicationDbContext db) : ICatalogRepository
{
    public Task<bool> HasAnyCatalogAsync(CancellationToken ct = default) =>
        db.Brands.AnyAsync(ct);

    public async Task<IReadOnlyList<ModelVariantSummary>> FindVariantsByYearAsync(int? modelYear, CancellationToken ct = default)
    {
        var query =
            from variant in db.ModelVariants.AsNoTracking()
            join model in db.VehicleModels.AsNoTracking() on variant.VehicleModelId equals model.Id
            select new { variant, model };

        if (modelYear is { } year)
        {
            query = query.Where(x => x.variant.ModelYearFrom <= year && x.variant.ModelYearTo >= year);
        }

        return await query
            .OrderBy(x => x.model.Name).ThenBy(x => x.variant.Trim)
            .Select(x => new ModelVariantSummary(
                x.variant.Id,
                x.model.Name,
                x.variant.Trim,
                x.variant.MarketingEngine ?? "",
                x.variant.ModelYearFrom,
                x.variant.ModelYearTo,
                x.variant.Market))
            .ToListAsync(ct);
    }

    public async Task<ModelVariantDetail?> GetVariantDetailAsync(Guid variantId, CancellationToken ct = default)
    {
        return await (
            from variant in db.ModelVariants.AsNoTracking()
            join model in db.VehicleModels.AsNoTracking() on variant.VehicleModelId equals model.Id
            join engine in db.EngineSpecs.AsNoTracking() on variant.EngineSpecId equals (Guid?)engine.Id
            where variant.Id == variantId
            select new ModelVariantDetail(
                variant.Id,
                model.Name,
                model.Generation,
                variant.Trim,
                variant.MarketingEngine,
                variant.ModelYearFrom,
                variant.ModelYearTo,
                variant.Market,
                variant.Transmission,
                variant.Drivetrain,
                engine.Code,
                engine.Family,
                engine.FuelType.ToString(),
                engine.PowerHp,
                engine.DisplacementCc,
                engine.OilGrade,
                engine.OilCapacityLiters))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<MaintenancePlan?> GetPlanByEngineFamilyAsync(string engineFamily, CancellationToken ct = default) =>
        await db.MaintenancePlans.AsNoTracking()
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.EngineFamily == engineFamily, ct);
}
