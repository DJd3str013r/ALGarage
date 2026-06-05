using ALGarage.Application.Abstractions;
using ALGarage.Domain.Common;

namespace ALGarage.Infrastructure.Vin;

/// <summary>
/// Cadeia de decodificadores de VIN (ADR-0008): tenta cada um na ordem e devolve o primeiro resultado
/// "identificado". Se nenhum identificar, devolve o último resultado obtido (que ainda pode trazer o
/// ano-modelo do fallback curado).
/// </summary>
public sealed class CompositeVinDecoder(IReadOnlyList<IVinDecoder> decoders) : IVinDecoder
{
    public async Task<VinDecodeResult> DecodeAsync(Domain.Vehicles.Vin vin, CancellationToken ct = default)
    {
        VinDecodeResult? last = null;
        foreach (var decoder in decoders)
        {
            var result = await decoder.DecodeAsync(vin, ct);
            if (result.Identified)
            {
                return result;
            }

            last = result;
        }

        return last ?? VinDecodeResult.NotIdentified(DataSource.Unknown);
    }
}
