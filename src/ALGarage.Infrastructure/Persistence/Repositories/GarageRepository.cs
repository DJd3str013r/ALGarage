using ALGarage.Application.Abstractions;
using ALGarage.Domain.Service;
using ALGarage.Domain.Vehicles;
using Microsoft.EntityFrameworkCore;

namespace ALGarage.Infrastructure.Persistence.Repositories;

internal sealed class GarageRepository(ApplicationDbContext db) : IGarageRepository
{
    public async Task AddVehicleAsync(Vehicle vehicle, CancellationToken ct = default) =>
        await db.Vehicles.AddAsync(vehicle, ct);

    public async Task<IReadOnlyList<Vehicle>> GetVehiclesByUserAsync(Guid userId, CancellationToken ct = default) =>
        await db.Vehicles.AsNoTracking()
            .Where(v => v.UserId == userId)
            .OrderBy(v => v.CreatedAtUtc)
            .ToListAsync(ct);

    public async Task<Vehicle?> GetVehicleAsync(Guid vehicleId, Guid userId, CancellationToken ct = default) =>
        await db.Vehicles.FirstOrDefaultAsync(v => v.Id == vehicleId && v.UserId == userId, ct);

    public async Task<bool> VinExistsForUserAsync(Guid userId, string vin, CancellationToken ct = default) =>
        await db.Vehicles
            .Where(v => v.UserId == userId)
            .AnyAsync(v => EF.Property<string>(v, nameof(Vehicle.Vin)) == vin, ct);

    public async Task AddOdometerReadingAsync(OdometerReading reading, CancellationToken ct = default) =>
        await db.OdometerReadings.AddAsync(reading, ct);

    public async Task AddServiceRecordAsync(ServiceRecord record, CancellationToken ct = default) =>
        await db.ServiceRecords.AddAsync(record, ct);

    public async Task<IReadOnlyList<ServiceRecord>> GetServiceRecordsAsync(Guid vehicleId, CancellationToken ct = default) =>
        await db.ServiceRecords.AsNoTracking()
            .Include(r => r.Items)
            .Where(r => r.VehicleId == vehicleId)
            .OrderByDescending(r => r.PerformedOn)
            .ToListAsync(ct);
}
