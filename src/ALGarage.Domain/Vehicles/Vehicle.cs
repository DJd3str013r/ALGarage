using ALGarage.Domain.Common;

namespace ALGarage.Domain.Vehicles;

/// <summary>
/// Um carro na garagem do usuário. Raiz de agregado. VIN é obrigatório.
/// <see cref="ModelVariantId"/> pode ficar nulo se a decodificação do VIN não resolver a versão
/// (estado "não identificado" — tratado graciosamente pela UI).
/// Esqueleto: comportamento (registrar leitura de hodômetro, etc.) será adicionado nas features.
/// </summary>
public sealed class Vehicle : AggregateRoot
{
    private Vehicle() { } // EF

    public Vehicle(Guid userId, Vin vin, DateOnly purchaseDate, int previousOwners,
        int odometerAtAcquisitionKm, int currentOdometerKm, double avgDailyKm, string? nickname = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(previousOwners);
        ArgumentOutOfRangeException.ThrowIfNegative(odometerAtAcquisitionKm);
        ArgumentOutOfRangeException.ThrowIfNegative(avgDailyKm);
        if (currentOdometerKm < odometerAtAcquisitionKm)
        {
            throw new ArgumentException(
                "Km atual não pode ser menor que o km na aquisição.", nameof(currentOdometerKm));
        }

        UserId = userId;
        Vin = vin;
        PurchaseDate = purchaseDate;
        PreviousOwners = previousOwners;
        OdometerAtAcquisitionKm = odometerAtAcquisitionKm;
        CurrentOdometerKm = currentOdometerKm;
        AvgDailyKm = avgDailyKm;
        Nickname = nickname;
    }

    public Guid UserId { get; private set; }
    public Vin Vin { get; private set; }

    /// <summary>Versão resolvida pela decodificação do VIN (catálogo). Nulo = não identificado.</summary>
    public Guid? ModelVariantId { get; private set; }

    public string? Nickname { get; private set; }
    public DateOnly PurchaseDate { get; private set; }
    public int PreviousOwners { get; private set; }
    public int OdometerAtAcquisitionKm { get; private set; }
    public int CurrentOdometerKm { get; private set; }

    /// <summary>Uso médio informado/estimado (km/dia). Base da projeção de manutenção por km.</summary>
    public double AvgDailyKm { get; private set; }

    /// <summary>Liga a versão resolvida pela decodificação do VIN (ou confirmada pelo usuário).</summary>
    public void AttachVariant(Guid modelVariantId)
    {
        ModelVariantId = modelVariantId;
        Touch();
    }

    /// <summary>Atualiza o km atual (a partir de uma nova leitura de hodômetro).</summary>
    public void UpdateCurrentOdometer(int km)
    {
        if (km < CurrentOdometerKm)
        {
            throw new ArgumentException("Nova leitura de hodômetro não pode ser menor que a atual.", nameof(km));
        }

        CurrentOdometerKm = km;
        Touch();
    }

    private void Touch() => UpdatedAtUtc = DateTime.UtcNow;
}
