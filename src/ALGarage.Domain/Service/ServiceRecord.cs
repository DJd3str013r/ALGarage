using ALGarage.Domain.Common;

namespace ALGarage.Domain.Service;

/// <summary>
/// Um serviço realizado no veículo (histórico). Quando um item se refere a um
/// <see cref="ServiceItem.MaintenanceItemKey"/>, ele "zera o relógio" daquele item de manutenção.
/// </summary>
public sealed class ServiceRecord : AggregateRoot
{
    private readonly List<ServiceItem> _items = [];

    private ServiceRecord() { } // EF

    public ServiceRecord(Guid vehicleId, DateOnly performedOn, int odometerKm, string? workshop = null,
        decimal? totalCost = null, string? notes = null)
    {
        VehicleId = vehicleId;
        PerformedOn = performedOn;
        OdometerKm = odometerKm;
        Workshop = workshop;
        TotalCost = totalCost;
        Notes = notes;
    }

    public Guid VehicleId { get; private set; }
    public DateOnly PerformedOn { get; private set; }
    public int OdometerKm { get; private set; }
    public string? Workshop { get; private set; }
    public decimal? TotalCost { get; private set; }
    public string? Notes { get; private set; }

    public IReadOnlyList<ServiceItem> Items => _items;

    public ServiceItem AddItem(string description, string? maintenanceItemKey = null, decimal? cost = null)
    {
        var item = new ServiceItem(Id, description, maintenanceItemKey, cost);
        _items.Add(item);
        return item;
    }
}

/// <summary>
/// Um item dentro de um serviço. <see cref="MaintenanceItemKey"/> liga a um item do plano de
/// manutenção (ex.: "Troca de óleo e filtro de óleo") para resetar a estimativa.
/// </summary>
public sealed class ServiceItem : Entity
{
    private ServiceItem() { } // EF

    internal ServiceItem(Guid serviceRecordId, string description, string? maintenanceItemKey, decimal? cost)
    {
        ServiceRecordId = serviceRecordId;
        Description = description;
        MaintenanceItemKey = maintenanceItemKey;
        Cost = cost;
    }

    public Guid ServiceRecordId { get; private set; }
    public string Description { get; private set; } = null!;

    /// <summary>Chave do item de manutenção que este serviço cumpriu (nome do item). Opcional.</summary>
    public string? MaintenanceItemKey { get; private set; }
    public decimal? Cost { get; private set; }
}
