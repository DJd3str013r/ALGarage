using System.Reflection;
using System.Text.Json;

namespace ALGarage.Infrastructure.Seed;

/// <summary>
/// Carrega o(s) dataset(s) curado(s) embutido(s) como recurso (Seed/Data/*.json) e os desserializa.
/// É código REAL e executável: serve para validar/inspecionar os dados. A PERSISTÊNCIA no banco
/// (mapeamento para entidades + upsert idempotente) é tarefa da fase de features — ver
/// docs/09-dataset-v40.md e o TODO em <see cref="SeedSummary"/>.
/// </summary>
public static class CuratedDatasetLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    /// <summary>Carrega todos os datasets embutidos em Seed/Data.</summary>
    public static IReadOnlyList<CuratedDataset> LoadAll()
    {
        var assembly = typeof(CuratedDatasetLoader).Assembly;
        var resourceNames = assembly.GetManifestResourceNames()
            .Where(n => n.Contains(".Seed.Data.") && n.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            .OrderBy(n => n, StringComparer.Ordinal);

        var datasets = new List<CuratedDataset>();
        foreach (var name in resourceNames)
        {
            using var stream = assembly.GetManifestResourceStream(name)
                ?? throw new InvalidOperationException($"Recurso embutido não encontrado: {name}");
            var dataset = JsonSerializer.Deserialize<CuratedDataset>(stream, JsonOptions)
                ?? throw new InvalidOperationException($"Falha ao desserializar o dataset: {name}");
            datasets.Add(dataset);
        }

        return datasets;
    }

    /// <summary>Carrega o dataset do Volvo V40 (o único do MVP).</summary>
    public static CuratedDataset LoadV40() =>
        LoadAll().Single(d => d.ModelScope.Contains("V40", StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Resumo de contagens — útil para um teste de sanidade ("o JSON carrega e tem N motores").
    /// </summary>
    public static SeedSummary Summarize(CuratedDataset d) => new(
        Models: d.Models.Count,
        Engines: d.Engines.Count,
        Variants: d.Variants.Count,
        FactoryOptions: d.FactoryOptions.Count,
        MaintenancePlans: d.MaintenancePlans.Count,
        MaintenanceItems: d.MaintenancePlans.Sum(p => p.Items.Count));
}

public sealed record SeedSummary(
    int Models, int Engines, int Variants, int FactoryOptions, int MaintenancePlans, int MaintenanceItems);
