using System.Net;
using ALGarage.Application.Abstractions;
using ALGarage.Domain.Vehicles;

namespace ALGarage.Infrastructure.Parts;

/// <summary>
/// Provider de links de busca de peças no Mercado Livre. Constrói um DEEP-LINK de busca
/// (estável, legal). NÃO faz scraping. O enriquecimento via API de afiliados (título/preço/comissão)
/// é trabalho de fase posterior. Ver ADR-0007.
/// </summary>
public sealed class MercadoLivreSearchLinkProvider : IPartsSearchLinkProvider
{
    public string StoreName => "Mercado Livre";

    public Task<IReadOnlyList<PartSearchLink>> BuildLinksAsync(string partQuery, Domain.Vehicles.Vin? vin = null, CancellationToken ct = default)
    {
        // Deep-link de busca: aponta para a página de resultados, não para um SKU volátil.
        var term = WebUtility.UrlEncode(partQuery.Trim());
        var url = $"https://lista.mercadolivre.com.br/{term}";

        IReadOnlyList<PartSearchLink> links =
        [
            new PartSearchLink(StoreName, url, IsAffiliate: false)
        ];

        return Task.FromResult(links);
    }
}
