using Hms.Api.Domain;
using Hms.Api.Features.Orders;
using Hms.Api.Features.Production;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.Catering;

/// <summary>
/// Catering / banquet (#75): hall + package masters, event bookings and the
/// function bill. An event is priced pax × price/head + extras − discount;
/// deposits/advances reduce the balance. Booking a hall is blocked if it would
/// overlap another live (non-cancelled) event for the same hall.
/// </summary>
public class CateringService(ITenantDbContextFactory factory, ProductionService production)
{
    static DateTime Utc(DateTime d) => OrderService.AsUtc(d);

    // ── Events ────────────────────────────────────────────────────────────────
    public async Task<List<CateringEvent>> ListEventsAsync(DateTime? from, DateTime? to, string? status, Guid? locationId, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var q = db.CateringEvents.AsNoTracking().AsQueryable();
        if (from is { } f) { var ff = Utc(f); q = q.Where(e => e.StartsAt >= ff); }
        if (to is { } t) { var tt = Utc(t); q = q.Where(e => e.StartsAt < tt); }
        if (!string.IsNullOrWhiteSpace(status)) q = q.Where(e => e.Status == status);
        if (locationId is { } loc) q = q.Where(e => e.LocationId == loc);
        return await q.OrderByDescending(e => e.StartsAt).ToListAsync(ct);
    }

    public async Task<CateringEvent?> GetEventAsync(Guid id, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        return await db.CateringEvents.Include(e => e.Items).Include(e => e.Payments)
            .FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    public async Task<CateringEvent> SaveEventAsync(SaveEventInput i, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var starts = Utc(i.StartsAt);
        DateTime? ends = i.EndsAt is { } e ? Utc(e) : null;

        // Required fields (defense-in-depth — the form validates too).
        if (string.IsNullOrWhiteSpace(i.Title) && string.IsNullOrWhiteSpace(i.CustomerName))
            throw new InvalidOperationException("An event title or a customer name is required");
        if (i.Pax < 1) throw new InvalidOperationException("Head-count (pax) must be at least 1");

        // Date sanity (no back-dating a new booking; end after start).
        if (ends is { } en && en <= starts) throw new InvalidOperationException("End must be after the start");
        if (i.Id is null && starts < DateTime.UtcNow.AddMinutes(-5)) throw new InvalidOperationException("A booking can't start in the past");

        CateringEvent ev;
        if (i.Id is Guid id)
            ev = await db.CateringEvents.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id, ct)
                 ?? throw new InvalidOperationException("Event not found");
        else { ev = new CateringEvent { EventNo = await NextEventNoAsync(db, ct), Status = "enquiry" }; db.CateringEvents.Add(ev); }

        // Hall double-booking guard (skip cancelled + self). Overlap of [starts, end)
        // where a missing end defaults to +4h.
        if (i.HallId is Guid hall)
        {
            var newEnd = ends ?? starts.AddHours(4);
            var clash = await db.CateringEvents.AsNoTracking()
                .Where(x => x.HallId == hall && x.Status != "cancelled" && (i.Id == null || x.Id != i.Id))
                .Select(x => new { x.StartsAt, x.EndsAt })
                .ToListAsync(ct);
            if (clash.Any(c => starts < (c.EndsAt ?? c.StartsAt.AddHours(4)) && (c.StartsAt < newEnd)))
                throw new InvalidOperationException("That hall is already booked for an overlapping time");
        }

        ev.Title = i.Title; ev.LocationId = i.LocationId; ev.HallId = i.HallId; ev.PackageId = i.PackageId;
        ev.CustomerId = i.CustomerId; ev.CustomerName = i.CustomerName; ev.CustomerPhone = i.CustomerPhone;
        ev.Pax = Math.Max(0, i.Pax); ev.StartsAt = starts; ev.EndsAt = ends;
        ev.DiscountAmount = Math.Max(0, i.DiscountAmount ?? ev.DiscountAmount);
        ev.ServiceCharge = Math.Max(0, i.ServiceCharge ?? ev.ServiceCharge);
        ev.TaxAmount = Math.Max(0, i.TaxAmount ?? ev.TaxAmount);
        ev.IsOffsite = i.IsOffsite ?? ev.IsOffsite;
        ev.DeliveryAddress = i.DeliveryAddress; ev.Vehicle = i.Vehicle; ev.Driver = i.Driver;
        ev.CountryCode = i.CountryCode; ev.Province = i.Province; ev.District = i.District; ev.PostalCode = i.PostalCode;
        // Off-site (outside) catering must have a delivery address — you can't dispatch without one.
        if (ev.IsOffsite && (string.IsNullOrWhiteSpace(ev.DeliveryAddress) || string.IsNullOrWhiteSpace(ev.CountryCode) || string.IsNullOrWhiteSpace(ev.District)))
            throw new InvalidOperationException("An off-site booking needs a delivery address (street, country and city/district).");
        ev.Notes = i.Notes;

        // Snapshot the package rate.
        ev.PricePerHead = i.PackageId is Guid pid
            ? (await db.CateringPackages.AsNoTracking().Where(p => p.Id == pid).Select(p => p.PricePerHead).FirstOrDefaultAsync(ct))
            : 0m;

        await db.SaveChangesAsync(ct);
        await RecomputeAsync(db, ev.Id, ct);
        return (await db.CateringEvents.Include(x => x.Items).Include(x => x.Payments).FirstAsync(x => x.Id == ev.Id, ct));
    }

    public async Task<CateringEvent> SetStatusAsync(Guid id, string status, CancellationToken ct)
    {
        var allowed = new[] { "enquiry", "quote", "confirmed", "running", "completed", "cancelled" };
        if (!allowed.Contains(status)) throw new InvalidOperationException("Invalid status");
        await using var db = await factory.CreateForCurrentAsync(ct);
        var ev = await db.CateringEvents.FirstOrDefaultAsync(x => x.Id == id, ct) ?? throw new InvalidOperationException("Event not found");
        ev.Status = status;
        await db.SaveChangesAsync(ct);
        return ev;
    }

    public async Task<CateringEvent> SetDispatchAsync(Guid id, string dispatchStatus, string? vehicle, string? driver, CancellationToken ct)
    {
        var allowed = new[] { "pending", "dispatched", "delivered" };
        if (!allowed.Contains(dispatchStatus)) throw new InvalidOperationException("Invalid dispatch status");
        await using var db = await factory.CreateForCurrentAsync(ct);
        var ev = await db.CateringEvents.FirstOrDefaultAsync(x => x.Id == id, ct) ?? throw new InvalidOperationException("Event not found");
        ev.IsOffsite = true; ev.DispatchStatus = dispatchStatus;
        // Vehicle/driver are assigned at dispatch time (not at enquiry); only overwrite when supplied.
        if (vehicle is not null) ev.Vehicle = string.IsNullOrWhiteSpace(vehicle) ? null : vehicle.Trim();
        if (driver is not null) ev.Driver = string.IsNullOrWhiteSpace(driver) ? null : driver.Trim();
        // Can't dispatch a van with nobody driving it.
        if (dispatchStatus is "dispatched" or "delivered" && string.IsNullOrWhiteSpace(ev.Driver))
            throw new InvalidOperationException("Assign a driver before marking the delivery dispatched.");
        await db.SaveChangesAsync(ct);
        return ev;
    }

    public async Task<CateringEvent> AddItemAsync(Guid eventId, string description, decimal quantity, decimal unitPrice, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(description)) throw new InvalidOperationException("Description is required");
        if (unitPrice <= 0) throw new InvalidOperationException("An extra needs a price greater than zero");
        await using var db = await factory.CreateForCurrentAsync(ct);
        var ev = await db.CateringEvents.FirstOrDefaultAsync(x => x.Id == eventId, ct) ?? throw new InvalidOperationException("Event not found");
        var qty = quantity <= 0 ? 1 : quantity;
        db.CateringEventItems.Add(new CateringEventItem
        {
            TenantId = db.TenantId, EventId = ev.Id, Description = description.Trim(), Quantity = qty,
            UnitPrice = unitPrice, LineTotal = Math.Round(qty * unitPrice, 4), CreatedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync(ct);
        await RecomputeAsync(db, ev.Id, ct);
        return (await db.CateringEvents.Include(x => x.Items).Include(x => x.Payments).FirstAsync(x => x.Id == ev.Id, ct));
    }

    public async Task<CateringEvent> RemoveItemAsync(Guid eventId, Guid itemId, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var item = await db.CateringEventItems.FirstOrDefaultAsync(x => x.Id == itemId && x.EventId == eventId, ct);
        if (item is not null) { item.IsDeleted = true; await db.SaveChangesAsync(ct); await RecomputeAsync(db, eventId, ct); }
        return (await db.CateringEvents.Include(x => x.Items).Include(x => x.Payments).FirstAsync(x => x.Id == eventId, ct));
    }

    public async Task<CateringEvent> AddPaymentAsync(Guid eventId, decimal amount, string payType, string kind, string? reference, CancellationToken ct)
    {
        if (amount <= 0) throw new InvalidOperationException("Amount must be greater than zero");
        await using var db = await factory.CreateForCurrentAsync(ct);
        var ev = await db.CateringEvents.FirstOrDefaultAsync(x => x.Id == eventId, ct) ?? throw new InvalidOperationException("Event not found");
        db.CateringEventPayments.Add(new CateringEventPayment
        {
            TenantId = db.TenantId, EventId = ev.Id, Amount = Math.Round(amount, 4),
            PayType = string.IsNullOrWhiteSpace(payType) ? "cash" : payType,
            Kind = string.IsNullOrWhiteSpace(kind) ? "deposit" : kind,
            Reference = reference, PaidAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync(ct);
        await RecomputeAsync(db, ev.Id, ct);
        return (await db.CateringEvents.Include(x => x.Items).Include(x => x.Payments).FirstAsync(x => x.Id == ev.Id, ct));
    }

    /// <summary>Recompute the function bill from pax × rate + extras − discount (+ svc + tax), and sum payments.</summary>
    private static async Task RecomputeAsync(TenantDbContext db, Guid eventId, CancellationToken ct)
    {
        var ev = await db.CateringEvents.Include(x => x.Items).Include(x => x.Payments).FirstAsync(x => x.Id == eventId, ct);
        ev.PackageTotal = Math.Round(ev.Pax * ev.PricePerHead, 4);
        ev.ExtrasTotal = ev.Items.Where(i => !i.IsDeleted).Sum(i => i.LineTotal);
        ev.TotalAmount = Math.Max(0, ev.PackageTotal + ev.ExtrasTotal + ev.ServiceCharge + ev.TaxAmount - ev.DiscountAmount);
        ev.PaidAmount = ev.Payments.Where(p => !p.IsDeleted).Sum(p => p.Amount);
        await db.SaveChangesAsync(ct);
    }

    private static async Task<string> NextEventNoAsync(TenantDbContext db, CancellationToken ct)
    {
        var year = DateTime.UtcNow.Year;
        var n = await db.CateringEvents.IgnoreQueryFilters().CountAsync(e => e.TenantId == db.TenantId && e.EventNo.StartsWith($"EVT-{year}-"), ct);
        return $"EVT-{year}-{n + 1:D4}";
    }

    // ── Halls ──────────────────────────────────────────────────────────────────
    public async Task<List<EventHall>> ListHallsAsync(bool all, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var q = db.EventHalls.AsNoTracking();
        if (!all) q = q.Where(h => h.IsActive);
        return await q.OrderBy(h => h.Name).ToListAsync(ct);
    }

    public async Task<EventHall> SaveHallAsync(Guid? id, string code, string name, int capacity, string? notes, bool isActive, Guid? locationId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name)) throw new InvalidOperationException("Code and name are required");
        await using var db = await factory.CreateForCurrentAsync(ct);
        EventHall h;
        if (id is Guid x) h = await db.EventHalls.FirstOrDefaultAsync(z => z.Id == x, ct) ?? throw new InvalidOperationException("Hall not found");
        else { h = new EventHall(); db.EventHalls.Add(h); }
        h.Code = code.Trim(); h.Name = name.Trim(); h.Capacity = Math.Max(0, capacity);
        h.Notes = notes; h.IsActive = isActive; h.LocationId = locationId;
        await db.SaveChangesAsync(ct);
        return h;
    }

    public async Task DeleteHallAsync(Guid id, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var h = await db.EventHalls.FirstOrDefaultAsync(z => z.Id == id, ct);
        if (h is not null) { h.IsDeleted = true; h.IsActive = false; await db.SaveChangesAsync(ct); }
    }

    // ── Packages ────────────────────────────────────────────────────────────────
    public async Task<List<CateringPackage>> ListPackagesAsync(bool all, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var q = db.CateringPackages.AsNoTracking();
        if (!all) q = q.Where(p => p.IsActive);
        return await q.OrderBy(p => p.Name).ToListAsync(ct);
    }

    public async Task<CateringPackage> SavePackageAsync(Guid? id, string code, string name, decimal pricePerHead, string? description, bool isActive, Guid? recipeProductId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name)) throw new InvalidOperationException("Code and name are required");
        await using var db = await factory.CreateForCurrentAsync(ct);
        CateringPackage p;
        if (id is Guid x) p = await db.CateringPackages.FirstOrDefaultAsync(z => z.Id == x, ct) ?? throw new InvalidOperationException("Package not found");
        else { p = new CateringPackage(); db.CateringPackages.Add(p); }
        p.Code = code.Trim(); p.Name = name.Trim(); p.PricePerHead = Math.Max(0, pricePerHead);
        p.Description = description; p.IsActive = isActive;
        p.RecipeProductId = recipeProductId == Guid.Empty ? null : recipeProductId;
        await db.SaveChangesAsync(ct);
        return p;
    }

    /// <summary>
    /// Produce an event (#75): scale the package's recipe by pax and post a
    /// production order that consumes ingredients from inventory (reuses the
    /// Production module), recording the event's food cost. Idempotent-guarded.
    /// </summary>
    public async Task<CateringEvent> ProduceEventAsync(Guid eventId, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var ev = await db.CateringEvents.FirstOrDefaultAsync(x => x.Id == eventId, ct) ?? throw new InvalidOperationException("Event not found");
        if (ev.Status == "cancelled") throw new InvalidOperationException("Cancelled events can't be produced");
        if (ev.ProducedAt is not null) throw new InvalidOperationException("This event has already been produced");
        if (ev.Pax <= 0) throw new InvalidOperationException("Set the head-count (pax) before producing");
        if (ev.PackageId is not Guid pkgId) throw new InvalidOperationException("Attach a package first");
        var pkg = await db.CateringPackages.FirstOrDefaultAsync(p => p.Id == pkgId, ct);
        if (pkg?.RecipeProductId is not Guid recipeProduct)
            throw new InvalidOperationException("This package has no recipe linked — set one on the package to consume stock");
        var locId = ev.LocationId ?? await db.Locations.Where(l => l.IsActive).OrderBy(l => l.Code).Select(l => (Guid?)l.Id).FirstOrDefaultAsync(ct)
            ?? throw new InvalidOperationException("No outlet to consume stock from");

        // Consume ingredients × (pax / recipe yield) and post — via the Production module.
        var po = await production.ProduceAsync(
            new ProduceInput(locId, recipeProduct, ev.Pax, $"Catering {ev.EventNo}", Post: true), ct);

        ev.ProductionOrderId = po.Id;
        ev.FoodCost = po.TotalInputCost;
        ev.ProducedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return (await db.CateringEvents.Include(x => x.Items).Include(x => x.Payments).FirstAsync(x => x.Id == ev.Id, ct));
    }

    public async Task DeletePackageAsync(Guid id, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var p = await db.CateringPackages.FirstOrDefaultAsync(z => z.Id == id, ct);
        if (p is not null) { p.IsDeleted = true; p.IsActive = false; await db.SaveChangesAsync(ct); }
    }
}

public record SaveEventInput(
    Guid? Id, string? Title, Guid? LocationId, Guid? HallId, Guid? PackageId,
    Guid? CustomerId, string? CustomerName, string? CustomerPhone,
    int Pax, DateTime StartsAt, DateTime? EndsAt,
    decimal? DiscountAmount, decimal? ServiceCharge, decimal? TaxAmount,
    bool? IsOffsite, string? DeliveryAddress, string? Vehicle, string? Driver, string? Notes,
    string? CountryCode = null, string? Province = null, string? District = null, string? PostalCode = null);
