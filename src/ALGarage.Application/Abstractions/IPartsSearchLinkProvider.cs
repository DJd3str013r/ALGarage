using ALGarage.Domain.Vehicles;

namespace ALGarage.Application.Abstractions;

/// <summary>
/// Porta do buscador de peças. Cada loja é UMA implementação. O app SÓ devolve links
/// (deep-link de busca; enriquecido por API de afiliado quando houver). NUNCA scraping. Ver ADR-0007.
/// </summary>
public interface IPartsSearchLinkProvider
{
    /// <summary>Nome amigável da loja (ex.: "Mercado Livre").</summary>
    string StoreName { get; }

    /// <summary>Monta o(s) link(s) de busca para um termo de peça e, opcionalmente, o contexto do veículo.</summary>
    Task<IReadOnlyList<PartSearchLink>> BuildLinksAsync(string partQuery, Vin? vin = null, CancellationToken ct = default);
}

/// <summary>Um link de resultado de busca. <see cref="Price"/>/<see cref="Title"/> só quando há API.</summary>
public sealed record PartSearchLink(
    string StoreName,
    string Url,
    string? Title = null,
    decimal? Price = null,
    bool IsAffiliate = false);
