using ALGarage.Application.Abstractions;
using ALGarage.Domain.Common;
using ALGarage.Domain.Vehicles;

namespace ALGarage.Infrastructure.Vin;

/// <summary>
/// STUB do decoder baseado no NHTSA vPIC (grátis, sem chave): https://vpic.nhtsa.dot.gov/api/
/// Primeiro elo da cadeia de decode (ADR-0008). NÃO implementado — apenas a forma do provider.
/// A implementação real fará GET em
///   /vehicles/DecodeVinValues/{vin}?format=json
/// com HttpClient resiliente (Polly), mapeando o resultado para <see cref="VinDecodeResult"/>.
/// </summary>
public sealed class NhtsaVinDecoder : IVinDecoder
{
    public Task<VinDecodeResult> DecodeAsync(Domain.Vehicles.Vin vin, CancellationToken ct = default)
    {
        // TODO(feature): chamar a API vPIC e mapear make/model/year/trim/engine.
        // Fase de estruturação: devolvemos "não identificado" para deixar o contrato explícito.
        return Task.FromResult(VinDecodeResult.NotIdentified(DataSource.Vpic));
    }
}
