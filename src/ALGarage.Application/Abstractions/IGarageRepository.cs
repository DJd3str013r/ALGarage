using ALGarage.Domain.Service;
using ALGarage.Domain.Vehicles;

namespace ALGarage.Application.Abstractions;

/// <summary>Persistência da garagem do usuário (veículos, leituras e histórico de serviços).</summary>
public interface IGarageRepository
{
    Task AddVehicleAsync(Vehicle vehicle, CancellationToken ct = default);
    Task<IReadOnlyList<Vehicle>> GetVehiclesByUserAsync(Guid userId, CancellationToken ct = default);
    Task<Vehicle?> GetVehicleAsync(Guid vehicleId, Guid userId, CancellationToken ct = default);
    Task<bool> VinExistsForUserAsync(Guid userId, string vin, CancellationToken ct = default);

    Task AddOdometerReadingAsync(OdometerReading reading, CancellationToken ct = default);

    Task AddServiceRecordAsync(ServiceRecord record, CancellationToken ct = default);
    Task<IReadOnlyList<ServiceRecord>> GetServiceRecordsAsync(Guid vehicleId, CancellationToken ct = default);
}
