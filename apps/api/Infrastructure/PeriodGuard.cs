using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Infrastructure;

/// <summary>
/// Month-end period lock (#77). Once the books are closed through a date, any
/// posting or reversal whose effective date falls on/before it is rejected — so
/// a closed month's figures can't shift. Live postings (dated "now") are
/// unaffected unless the current period itself is locked.
/// </summary>
public static class PeriodGuard
{
    public static async Task EnsureOpenAsync(TenantDbContext db, DateTime date, CancellationToken ct)
    {
        var locked = await db.OrgSettings.AsNoTracking().Select(s => s.BooksLockedThrough).FirstOrDefaultAsync(ct);
        if (locked is { } lt && DateOnly.FromDateTime(date) <= lt)
            throw new InvalidOperationException(
                $"Accounting period is closed through {lt:yyyy-MM-dd} — this entry can't be posted or reversed.");
    }
}
