using ALGarage.Domain.Common;
using ALGarage.Domain.Vehicles;

namespace ALGarage.Application.Abstractions;

/// <summary>
/// Porta de decodificação de VIN. Implementada na Infrastructure como uma CADEIA
/// (vPIC → provider comercial opcional → dataset curado/fallback). Ver ADR-0008.
/// </summary>
public interface IVinDecoder
{
    Task<VinDecodeResult> DecodeAsync(Vin vin, CancellationToken ct = default);
}

/// <summary>Resultado do decode, com proveniência por origem para resolução de conflitos.</summary>
public sealed record VinDecodeResult(
    bool Identified,
    string? Make,
    string? Model,
    int? ModelYear,
    string? Trim,
    string? EngineCode,
    DataSource Source)
{
    public static VinDecodeResult NotIdentified(DataSource source) =>
        new(false, null, null, null, null, null, source);
}
