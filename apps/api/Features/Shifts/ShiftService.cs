using Hms.Api.Domain;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.Shifts;

/// <summary>
/// Cashier shift sessions + cash-up (Z-report). A shift aggregates the orders
/// settled at its outlet during [opened_at, closed_at]. Cash that should be in
/// the drawer for an order = order total − its non-cash payments (so overpaid
/// cash / change never inflates the expected drawer). Expected cash = opening
/// float + that cash; variance = declared − expected.
/// </summary>
public class ShiftService(ITenantDbContextFactory factory)
{
    public async Task<ShiftView?> GetCurrentAsync(Guid locationId, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var shift = await db.Shifts.FirstOrDefaultAsync(s => s.LocationId == locationId && s.Status == "open", ct);
        if (shift is null) return null;
        var totals = await ComputeTotalsAsync(db, shift, DateTime.UtcNow, ct);
        return Live(shift, totals);
    }

    public async Task<ShiftView> OpenAsync(Guid locationId, Guid? userId, string? fallbackName, decimal openingFloat, CancellationToken ct)
    {
        if (openingFloat < 0) throw new InvalidOperationException("Opening float cannot be negative");
        await using var db = await factory.CreateForCurrentAsync(ct);
        if (await db.Shifts.AnyAsync(s => s.LocationId == locationId && s.Status == "open", ct))
            throw new InvalidOperationException("A shift is already open at this outlet");

        // Prefer the user's display name; fall back to the email/name claim.
        var name = fallbackName;
        if (userId is Guid uid)
        {
            var u = await db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == uid, ct);
            if (u is not null) name = u.DisplayName;
        }

        var settings = await db.OrgSettings.AsNoTracking().FirstOrDefaultAsync(ct) ?? new OrgSettings();
        var shift = new Shift
        {
            LocationId = locationId,
            UserId = userId,
            OpenedByName = name,
            ShiftNumber = await NextNumberAsync(db, "SHIFT", settings.ShiftPrefix, ct),
            OpenedAt = DateTime.UtcNow,
            OpeningFloat = openingFloat,
            Status = "open",
        };
        db.Shifts.Add(shift);
        await db.SaveChangesAsync(ct);

        var totals = await ComputeTotalsAsync(db, shift, DateTime.UtcNow, ct);
        return Live(shift, totals);
    }

    public async Task<ShiftView> CloseAsync(Guid shiftId, decimal declaredCash, string? notes, bool allowOpenBills, CancellationToken ct)
    {
        if (declaredCash < 0) throw new InvalidOperationException("Declared cash cannot be negative");
        await using var db = await factory.CreateForCurrentAsync(ct);
        var shift = await db.Shifts.FirstOrDefaultAsync(s => s.Id == shiftId, ct)
            ?? throw new InvalidOperationException("Shift not found");
        if (shift.Status == "closed") throw new InvalidOperationException("Shift is already closed");

        var closedAt = DateTime.UtcNow;

        // Reconcile open bills at this outlet. Abandoned (empty) tables are always
        // auto-voided. Bills *with items* normally block the close (can't close over
        // live money) — unless the cashier chose "continue & close", in which case
        // they carry over, still open, to the next shift (e.g. a night→day handover).
        var openOrders = await db.Orders.Include(o => o.Items)
            .Where(o => o.LocationId == shift.LocationId && (o.Status == "open" || o.Status == "confirmed"))
            .ToListAsync(ct);
        var withItems = openOrders.Where(o => o.Items.Any(i => !i.IsDeleted)).ToList();
        if (withItems.Count > 0 && !allowOpenBills)
            throw new InvalidOperationException(
                $"Settle or void {withItems.Count} open bill(s) before closing the shift: "
                + string.Join(", ", withItems.Select(o => o.TableLabel is { } t ? $"Table {t}" : o.OrderNumber)));
        foreach (var empty in openOrders.Where(o => !o.Items.Any(i => !i.IsDeleted)))   // empties only
        {
            empty.Status = "void";
            empty.VoidedAt = closedAt;
            empty.VoidReason = "Abandoned empty table — auto-voided at shift close";
        }
        var totals = await ComputeTotalsAsync(db, shift, closedAt, ct);

        shift.ClosedAt = closedAt;
        shift.Status = "closed";
        shift.DeclaredCash = declaredCash;
        shift.ExpectedCash = shift.OpeningFloat + totals.CashDrawer;
        shift.CashVariance = declaredCash - shift.ExpectedCash;
        shift.TotalSales = totals.TotalSales;
        shift.CashSales = totals.CashDrawer;
        shift.CardSales = totals.Card;
        shift.OtherSales = totals.Other;
        shift.OrderCount = totals.OrderCount;
        shift.Notes = notes;
        await db.SaveChangesAsync(ct);

        return Live(shift, totals);
    }

    public async Task<List<ShiftView>> ListAsync(int take, CancellationToken ct, Guid? locationId = null)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var q = db.Shifts.AsNoTracking().AsQueryable();
        if (locationId is Guid l) q = q.Where(s => s.LocationId == l);   // #scoping-P2: confine pinned users to their outlet
        var shifts = await q.OrderByDescending(s => s.OpenedAt).Take(take).ToListAsync(ct);
        return shifts.Select(Stored).ToList();
    }

    // ── aggregation ───────────────────────────────────────────────────────────
    private static async Task<Totals> ComputeTotalsAsync(TenantDbContext db, Shift shift, DateTime upto, CancellationToken ct)
    {
        var from = shift.OpenedAt;
        var orders = await db.Orders.AsNoTracking()
            .Where(o => o.LocationId == shift.LocationId && o.Status == "settled"
                        && o.SettledAt >= from && o.SettledAt <= upto)
            .Select(o => new { o.Id, o.TotalAmount })
            .ToListAsync(ct);

        var ids = orders.Select(o => o.Id).ToList();
        var payments = ids.Count == 0
            ? new List<PayRow>()
            : await db.Payments.AsNoTracking()
                .Where(p => ids.Contains(p.OrderId))
                .Select(p => new PayRow(p.OrderId, p.PayType, p.Amount))
                .ToListAsync(ct);

        var nonCashByOrder = payments.Where(p => p.PayType != "cash")
            .GroupBy(p => p.OrderId).ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

        decimal total = 0, cashDrawer = 0;
        foreach (var o in orders)
        {
            total += o.TotalAmount;
            nonCashByOrder.TryGetValue(o.Id, out var nonCash);
            var drawer = o.TotalAmount - nonCash;
            if (drawer > 0) cashDrawer += drawer;
        }

        var card = payments.Where(p => p.PayType == "card").Sum(p => p.Amount);
        var other = payments.Where(p => p.PayType != "cash" && p.PayType != "card").Sum(p => p.Amount);

        // Keep the stored precision so the Z-report reconciles exactly with order
        // totals; the UI formats to 2dp for display.
        return new Totals(total, cashDrawer, card, other, orders.Count);
    }

    private static ShiftView Live(Shift s, Totals t) => new(
        s.Id, s.ShiftNumber, s.LocationId, s.UserId, s.OpenedByName, s.OpenedAt, s.OpeningFloat,
        s.ClosedAt, s.Status, t.TotalSales, t.CashDrawer, t.Card, t.Other, t.OrderCount,
        s.OpeningFloat + t.CashDrawer, s.DeclaredCash, s.CashVariance, s.Notes);

    private static ShiftView Stored(Shift s) => new(
        s.Id, s.ShiftNumber, s.LocationId, s.UserId, s.OpenedByName, s.OpenedAt, s.OpeningFloat,
        s.ClosedAt, s.Status, s.TotalSales, s.CashSales, s.CardSales, s.OtherSales, s.OrderCount,
        s.ExpectedCash ?? (s.OpeningFloat + s.CashSales), s.DeclaredCash, s.CashVariance, s.Notes);

    private static async Task<string> NextNumberAsync(TenantDbContext db, string counterKey, string prefix, CancellationToken ct)
    {
        var rows = await db.Database.SqlQuery<int>($@"
            INSERT INTO document_counters (tenant_id, doc_type, last_value)
            VALUES ({db.TenantId}, {counterKey}, 1)
            ON CONFLICT (tenant_id, doc_type)
            DO UPDATE SET last_value = document_counters.last_value + 1
            RETURNING last_value AS ""Value""").ToListAsync(ct);
        return $"{prefix}-{rows[0]:D4}";
    }

    private record PayRow(Guid OrderId, string PayType, decimal Amount);
    private record Totals(decimal TotalSales, decimal CashDrawer, decimal Card, decimal Other, int OrderCount);
}

public record ShiftView(
    Guid Id, string ShiftNumber, Guid LocationId, Guid? UserId, string? OpenedByName,
    DateTime OpenedAt, decimal OpeningFloat, DateTime? ClosedAt, string Status,
    decimal TotalSales, decimal CashSales, decimal CardSales, decimal OtherSales, int OrderCount,
    decimal ExpectedCash, decimal? DeclaredCash, decimal? CashVariance, string? Notes);

public record OpenShiftInput(Guid LocationId, decimal OpeningFloat);
public record CloseShiftInput(decimal DeclaredCash, string? Notes, bool? AllowOpenBills = null);
