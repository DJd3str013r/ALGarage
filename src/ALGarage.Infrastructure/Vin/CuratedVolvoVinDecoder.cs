using ALGarage.Application.Abstractions;
using ALGarage.Domain.Common;
using ALGarage.Domain.Vehicles;

namespace ALGarage.Infrastructure.Vin;

/// <summary>
/// Decodificador curado (fallback garantido — ADR-0008). Reconhece VINs Volvo pelo WMI e extrai o
/// ano-modelo do 10º caractere. NÃO resolve a versão exata (isso é feito por confirmação do usuário
/// entre os candidatos do catálogo, filtrados pelo ano).
/// </summary>
public sealed class CuratedVolvoVinDecoder : IVinDecoder
{
    public Task<VinDecodeResult> DecodeAsync(Domain.Vehicles.Vin vin, CancellationToken ct = default)
    {
        var isVolvo = vin.Wmi.StartsWith("YV", StringComparison.OrdinalIgnoreCase);
        if (!isVolvo)
        {
            return Task.FromResult(VinDecodeResult.NotIdentified(DataSource.Curated));
        }

        var year = VinModelYear.TryDecode(vin);
        var result = new VinDecodeResult(
            Identified: year is not null,
            Make: "Volvo",
            Model: null,          // catálogo do MVP é só V40; a versão vem da confirmação por ano
            ModelYear: year,
            Trim: null,
            EngineCode: null,
            Source: DataSource.Curated);

        return Task.FromResult(result);
    }
}
