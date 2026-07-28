using System.Security.Cryptography;
using System.Text;
using Hms.Api.Domain;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.Tab;

/// <summary>
/// Tab Ordering licensing spine (#106). Tenants buy a number of handheld DEVICE seats
/// (OrgSettings.TabDeviceLimit) and a guest-QR add-on (GuestQrEnabled) — both granted by
/// the RIT platform admin (MSO365-style). Each registered, active <see cref="TabDevice"/>
/// consumes one seat; registration is refused once active devices reach the limit.
/// </summary>
public static class TabDevicesEndpoint
{
    public static IEndpointRouteBuilder MapTabDeviceEndpoints(this IEndpointRouteBuilder app)
    {
        // ── Tenant-facing: see usage + register/revoke devices (owner/manager) ──
        var g = app.MapGroup("/api/v1/tab-devices").WithTags("Tab devices");

        // Usage + add-on status — drives the read-only "Add-ons & licenses" panel and the handheld gate.
        // Also lists devices with server-side paging/search (Devices screen); seat math (limit/used/
        // available) always reflects ALL active devices, independent of the current page/search/filter.
        g.MapGet("", async (ITenantDbContextFactory f, CancellationToken ct,
            int pageNumber = 1, int pageSize = 10, string? search = null, bool? isActive = null) =>
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            await using var db = await f.CreateForCurrentAsync(ct);
            var s = await db.OrgSettings.AsNoTracking().FirstOrDefaultAsync(ct);
            var usedCount = await db.TabDevices.CountAsync(d => d.IsActive, ct);
            var limit = s?.TabDeviceLimit ?? 0;

            var query = db.TabDevices.AsNoTracking().AsQueryable();
            if (isActive is bool active) query = query.Where(d => d.IsActive == active);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = $"%{search.Trim()}%";
                query = query.Where(d => EF.Functions.ILike(d.Name, term));
            }

            var totalCount = await query.CountAsync(ct);
            var devices = await query
                .OrderBy(d => d.Name)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(d => new { d.Id, d.Name, d.LastSeenAt, d.IsActive, d.LocationId, d.CreatedAt })
                .ToListAsync(ct);

            return Results.Ok(new
            {
                limit,
                used = usedCount,
                available = Math.Max(0, limit - usedCount),
                guestQrEnabled = s?.GuestQrEnabled ?? false,
                data = devices,
                pagination = new PaginationMeta(totalCount, pageNumber, pageSize, (int)Math.Ceiling(totalCount / (double)pageSize)),
            });
        }).WithName("TabDevices.List").RequireAuthorization("BackOffice");

        // Register a new device → consumes a seat (or 403 if at the licensed limit). Raw token shown once.
        g.MapPost("/register", async (RegisterDeviceInput i, ITenantDbContextFactory f, CancellationToken ct) =>
        {
            await using var db = await f.CreateForCurrentAsync(ct);
            var s = await db.OrgSettings.AsNoTracking().FirstOrDefaultAsync(ct);
            var limit = s?.TabDeviceLimit ?? 0;
            var active = await db.TabDevices.CountAsync(d => d.IsActive, ct);
            if (active >= limit)
                return Results.Json(new { error = limit == 0
                    ? "Tab Ordering isn't enabled for your account — contact RIT to add device licenses."
                    : $"All {limit} tab-device licence(s) are in use. Revoke a device or buy more from RIT." },
                    statusCode: StatusCodes.Status403Forbidden);

            var raw = Convert.ToHexString(RandomNumberGenerator.GetBytes(24));   // 48-char one-time token
            var dev = new TabDevice
            {
                Name = string.IsNullOrWhiteSpace(i.Name) ? "Handheld" : i.Name.Trim(),
                TokenHash = Hash(raw),
                Fingerprint = i.Fingerprint,
                LocationId = i.LocationId,
                LastSeenAt = DateTime.UtcNow,
                IsActive = true,
            };
            db.TabDevices.Add(dev);
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { dev.Id, dev.Name, token = raw, seatsUsed = active + 1, limit });   // token is shown ONCE
        }).WithName("TabDevices.Register").RequireAuthorization("BackOffice");

        // Revoke (deactivate) a device → frees its seat.
        g.MapPost("/{id:guid}/revoke", async (Guid id, ITenantDbContextFactory f, CancellationToken ct) =>
        {
            await using var db = await f.CreateForCurrentAsync(ct);
            var dev = await db.TabDevices.FirstOrDefaultAsync(d => d.Id == id, ct);
            if (dev is null) return Results.NotFound();
            dev.IsActive = false;
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { dev.Id, dev.IsActive });
        }).WithName("TabDevices.Revoke").RequireAuthorization("BackOffice");

        // Rename / re-pin a device's outlet. Does not touch the pairing token or active state.
        g.MapPut("/{id:guid}", async (Guid id, UpdateDeviceInput i, ITenantDbContextFactory f, CancellationToken ct) =>
        {
            await using var db = await f.CreateForCurrentAsync(ct);
            var dev = await db.TabDevices.FirstOrDefaultAsync(d => d.Id == id, ct);
            if (dev is null) return Results.NotFound();
            if (string.IsNullOrWhiteSpace(i.Name)) return Results.BadRequest(new { error = "Name is required" });
            if (i.LocationId is Guid lid && !await db.Locations.AnyAsync(l => l.Id == lid, ct))
                return Results.BadRequest(new { error = "Unknown outlet" });
            dev.Name = i.Name.Trim();
            dev.LocationId = i.LocationId;
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { dev.Id, dev.Name, dev.LocationId });
        }).WithName("TabDevices.Update").RequireAuthorization("BackOffice");

        // Permanently remove a device record (soft-delete) → frees its seat if still active.
        // Distinct from Revoke: a revoked device stays visible for history, a removed one doesn't.
        g.MapDelete("/{id:guid}", async (Guid id, ITenantDbContextFactory f, CancellationToken ct) =>
        {
            await using var db = await f.CreateForCurrentAsync(ct);
            var dev = await db.TabDevices.FirstOrDefaultAsync(d => d.Id == id, ct);
            if (dev is null) return Results.NotFound();
            dev.IsActive = false;
            dev.IsDeleted = true;
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        }).WithName("TabDevices.Remove").RequireAuthorization("BackOffice");

        // ── Platform-admin (RIT): grant entitlements to a tenant (stands in for billing) ──
        app.MapPut("/api/v1/admin/tenants/{tenantId:guid}/entitlements", async (Guid tenantId, EntitlementInput i, ITenantDbContextFactory f, CancellationToken ct) =>
        {
            await using var db = f.Create(tenantId);   // target tenant's DB (control-plane action)
            var s = await db.OrgSettings.FirstOrDefaultAsync(ct);
            if (s is null) { s = new OrgSettings { TenantId = tenantId, CreatedAt = DateTime.UtcNow }; db.OrgSettings.Add(s); }
            if (i.TabDeviceLimit is int lim) s.TabDeviceLimit = Math.Max(0, lim);
            if (i.GuestQrEnabled is bool q) s.GuestQrEnabled = q;
            s.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { tenantId, s.TabDeviceLimit, s.GuestQrEnabled });
        }).WithName("Admin.SetEntitlements").WithTags("Platform admin").RequireAuthorization("PlatformAdmin");

        return app;
    }

    private static string Hash(string raw) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));
}

public record RegisterDeviceInput(string? Name, string? Fingerprint = null, Guid? LocationId = null);
public record UpdateDeviceInput(string? Name, Guid? LocationId = null);
public record EntitlementInput(int? TabDeviceLimit = null, bool? GuestQrEnabled = null);
public record PaginationMeta(int TotalCount, int PageNumber, int PageSize, int TotalPages);
