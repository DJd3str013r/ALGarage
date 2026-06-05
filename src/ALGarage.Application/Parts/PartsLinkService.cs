using ALGarage.Application.Abstractions;
using ALGarage.Domain.Vehicles;

namespace ALGarage.Application.Parts;

/// <summary>
/// Agrega LINKS de busca de peças de todos os provedores configurados. O app só mostra links;
/// a compra é responsabilidade do usuário. Nunca faz scraping (ADR-0007).
/// </summary>
public sealed class PartsLinkService(IEnumerable<IPartsSearchLinkProvider> providers)
{
    public async Task<IReadOnlyList<PartSearchLink>> SearchAsync(
        string partQuery, string? rawVin = null, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(partQuery))
        {
            return [];
        }

        Vin? vin = Vin.TryParse(rawVin, out var parsed) ? parsed : null;

        var results = new List<PartSearchLink>();
        foreach (var provider in providers)
        {
            results.AddRange(await provider.BuildLinksAsync(partQuery, vin, ct));
        }

        return results;
    }
}
