using ALGarage.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace ALGarage.Infrastructure.Persistence.Repositories;

internal sealed class ReminderQueries(ApplicationDbContext db) : IReminderQueries
{
    public async Task<IReadOnlyList<ReminderTarget>> GetTargetsAsync(CancellationToken ct = default)
    {
        return await (
            from v in db.Vehicles.AsNoTracking()
            join u in db.Users.AsNoTracking() on v.UserId equals u.Id
            where u.Email != null
            select new ReminderTarget(v, u.Email!))
            .ToListAsync(ct);
    }
}
