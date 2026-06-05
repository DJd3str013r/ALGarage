using ALGarage.Application.Catalog;
using ALGarage.Domain.Catalog;
using ALGarage.Domain.Maintenance;

namespace ALGarage.Application.Abstractions;

/// <summary>Acesso de leitura ao catálogo curado (marca, modelos, versões, motores, planos).</summary>
public interface ICatalogRepository
{
    Task<bool> HasAnyCatalogAsync(CancellationToken ct = default);

    /// <summary>Versões candidatas para um ano-modelo (usado para confirmar a decodificação do VIN).</summary>
    Task<IReadOnlyList<ModelVariantSummary>> FindVariantsByYearAsync(int? modelYear, CancellationToken ct = default);

    /// <summary>Detalhe de uma versão (com modelo e motor) — null se não existir.</summary>
    Task<ModelVariantDetail?> GetVariantDetailAsync(Guid variantId, CancellationToken ct = default);

    /// <summary>Plano de manutenção (com itens) para uma família de motor — null se não houver.</summary>
    Task<MaintenancePlan?> GetPlanByEngineFamilyAsync(string engineFamily, CancellationToken ct = default);
}
