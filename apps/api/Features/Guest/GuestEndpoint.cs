using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Hms.Api.Features.Availability;
using Hms.Api.Features.Orders;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Hms.Api.Features.Guest;

/// <summary>
/// Guest QR ordering (#108). A per-table QR encodes a server-SIGNED token (tenant+outlet+table).
/// Scanning opens the guest page which: (1) GET /guest/{token} → verifies the token, checks the
/// guest-QR add-on, and returns the availability-aware menu + a short-lived scoped "Guest" JWT;
/// (2) POST /guest/order with that JWT → reuses OrderService to create a guest_qr order on THAT
/// table and fire it to the kitchen. The table is baked into the token/JWT, so a guest can only
/// ever order for their own table — no login, no spoofing.
/// </summary>
public static class GuestEndpoint
{
    public static IEndpointRouteBuilder MapGuestEndpoints(this IEndpointRouteBuilder app)
    {
        // ── Back-office: get the signed QR token/URL for a table (to print) ──
        // The guest ordering PAGE lives on the web app, not this API — App:WebBaseUrl (same
        // config the magic-link email uses), never the API's own host/port.
        app.MapGet("/api/v1/tables/{id:guid}/guest-token", async (Guid id, ITenantDbContextFactory f, ITenantContext tc, IConfiguration cfg, CancellationToken ct) =>
        {
            await using var db = await f.CreateForCurrentAsync(ct);
            var table = await db.RestaurantTables.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
            if (table is null) return Results.NotFound();
            var token = Sign(cfg, tc.TenantIdOrThrow(), table.LocationId, table.Id, table.Code);
            var origin = cfg["App:WebBaseUrl"] ?? "http://localhost:3000";
            return Results.Ok(new { token, url = $"{origin}/guest/{token}", table = table.Code });
        }).WithName("Guest.TableToken").WithTags("Guest").RequireAuthorization("BackOffice");

        // ── Guest: open the menu for a table (anonymous, token-gated) ──
        app.MapGet("/api/v1/guest/{token}", async (string token, ControlDbContext control, ITenantDbContextFactory f,
            AvailabilityService avail, IConfiguration cfg, CancellationToken ct) =>
        {
            var p = Verify(cfg, token);
            if (p is null) return Results.Json(new { error = "This QR code is invalid or expired." }, statusCode: 400);
            var (tenantId, locationId, tableId, tableLabel) = p.Value;

            var tenant = await control.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tenantId, ct);
            if (tenant is null) return Results.NotFound();

            await using var db = f.Create(tenantId);
            var s = await db.OrgSettings.AsNoTracking().FirstOrDefaultAsync(ct);
            if (s?.GuestQrEnabled != true)
                return Results.Json(new { error = "Guest ordering isn't enabled for this venue." }, statusCode: 403);

            var products = await db.Products.AsNoTracking().Where(x => x.IsActive && x.IsSold)
                .OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
                .Select(x => new { x.Id, x.Name, x.BasePrice, x.CategoryId, x.ColorHex }).ToListAsync(ct);
            var categories = await db.Categories.AsNoTracking().Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder).Select(c => new { c.Id, c.Name, c.ParentId }).ToListAsync(ct);
            var availability = (await avail.ComputeAsync(db, locationId, ct))
                .Select(a => new { a.ProductId, a.Available }).ToList();

            var guestJwt = MintGuestJwt(cfg, tenantId, tenant.Slug, locationId, tableId, tableLabel);
            return Results.Ok(new
            {
                guestToken = guestJwt,
                tenant = new { tenant.DisplayName },
                tableLabel,
                currency = s?.BaseCurrency ?? "LKR",
                products, categories, availability,
            });
        }).WithName("Guest.Menu").WithTags("Guest").AllowAnonymous();

        // ── Guest: place an order for the token's table (uses the scoped Guest JWT) ──
        // Lands as PendingAcceptance — NOT auto-sent to the kitchen. A steward reviews and
        // accepts it (POST /api/v1/orders/{id}/accept), which is what actually fires the KOT.
        app.MapPost("/api/v1/guest/order", async (GuestOrderInput input, OrderService orders, ITenantContext tc, ITenantDbContextFactory f,
            ClaimsPrincipal user, Hms.Api.Features.Realtime.RealtimeBus rt, Hms.Api.Features.Push.IPushSender push,
            Hms.Api.Features.Push.IFcmSender fcm, CancellationToken ct) =>
        {
            if (input.Items is not { Count: > 0 }) return Results.BadRequest(new { error = "Add at least one item." });
            var locId = user.FindFirst("home_location_id")?.Value;
            if (!Guid.TryParse(locId, out var locationId)) return Results.BadRequest(new { error = "Invalid table session." });
            Guid? tableId = Guid.TryParse(user.FindFirst("table_id")?.Value, out var ti) ? ti : null;
            var tableLabel = user.FindFirst("table_label")?.Value;

            try
            {
                var order = await orders.CreateAsync(new CreateOrderInput(
                    LocationId: locationId, OrderType: "dine_in", OrderSource: "guest_qr",
                    ExternalOrderId: null, TableLabel: tableLabel, Covers: 1, CashierId: null,
                    TableId: tableId, PendingAcceptance: true), ct);
                foreach (var it in input.Items)
                {
                    if (!Guid.TryParse(it.ProductId, out var pid) || it.Quantity <= 0) continue;
                    await orders.AddItemAsync(order.Id, new AddItemInput(pid, it.Quantity, null, null), ct);
                }

                var tid = tc.TenantIdOrThrow();
                var targets = await ResolveFloorTargetsAsync(f, locationId, tableId, ct);
                if (targets.Count > 0) rt.PublishToUsers(tid, "orders", targets);
                else rt.Publish(tid, "orders");   // no stewards configured yet — safety net, same as before

                // Push reaches a target steward even with their tab/app closed — the SSE
                // signal above only helps someone already looking at the Floor screen.
                if (targets.Count > 0)
                    await SendPushToTargetsAsync(f, push, fcm, targets, tableLabel, order.OrderNumber, ct);

                return Results.Ok(new { orderNumber = order.OrderNumber, status = "pending_acceptance" });
            }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Guest.Order").WithTags("Guest").RequireAuthorization("GuestOrders");

        return app;
    }

    /// <summary>
    /// Which stewards should be notified about a new guest order at this table (#floor-push).
    /// A steward with NO floor assignments at all covers every floor (same convention as the
    /// Team screen: "leave all unchecked to cover every floor") — so they're always included.
    /// If the table has no floor set, every steward is notified (nothing to scope by).
    /// </summary>
    private static async Task<List<Guid>> ResolveFloorTargetsAsync(ITenantDbContextFactory f, Guid locationId, Guid? tableId, CancellationToken ct)
    {
        await using var db = await f.CreateForCurrentAsync(ct);
        var stewardIds = await db.Users.AsNoTracking()
            .Where(u => u.IsServer && u.IsActive && (u.HomeLocationId == null || u.HomeLocationId == locationId))
            .Select(u => u.Id).ToListAsync(ct);
        if (stewardIds.Count == 0) return new();

        var floorId = tableId is Guid tid
            ? await db.RestaurantTables.AsNoTracking().Where(t => t.Id == tid).Select(t => t.FloorId).FirstOrDefaultAsync(ct)
            : null;
        if (floorId is null) return stewardIds;

        var assignments = await db.UserFloors.AsNoTracking()
            .Where(uf => stewardIds.Contains(uf.UserId)).ToListAsync(ct);
        var assignedUserIds = assignments.Select(a => a.UserId).ToHashSet();
        var coveringThisFloor = assignments.Where(a => a.FloorId == floorId).Select(a => a.UserId);
        var noAssignmentAtAll = stewardIds.Where(s => !assignedUserIds.Contains(s));
        return coveringThisFloor.Union(noAssignmentAtAll).Distinct().ToList();
    }

    /// <summary>Push every subscribed device of the target stewards — both web (browser tabs
    /// with notifications enabled) and mobile (the Flutter handheld app). Best-effort: a
    /// failed send never blocks the guest's order confirmation. A "Gone" subscription/token
    /// (unsubscribed, app uninstalled, browser data cleared) is deleted so it's not retried
    /// forever.</summary>
    private static async Task SendPushToTargetsAsync(ITenantDbContextFactory f, Hms.Api.Features.Push.IPushSender push,
        Hms.Api.Features.Push.IFcmSender fcm, List<Guid> targetUserIds, string? tableLabel, string orderNumber, CancellationToken ct)
    {
        await using var db = await f.CreateForCurrentAsync(ct);
        var subs = await db.PushSubscriptions.Where(s => targetUserIds.Contains(s.UserId)).ToListAsync(ct);
        var tokens = await db.DeviceTokens.Where(t => targetUserIds.Contains(t.UserId)).ToListAsync(ct);
        if (subs.Count == 0 && tokens.Count == 0) return;

        var payload = new Hms.Api.Features.Push.PushPayload(
            Title: tableLabel is { } tl ? $"New order — Table {tl}" : $"New order — {orderNumber}",
            Body: $"{orderNumber} · awaiting acceptance",
            Url: "/floor");

        var deadSubs = new List<Domain.PushSubscription>();
        foreach (var s in subs)
        {
            var result = await push.SendAsync(s.Endpoint, s.P256dh, s.Auth, payload, ct);
            if (result == Hms.Api.Features.Push.PushSendResult.Gone) deadSubs.Add(s);
            else if (result == Hms.Api.Features.Push.PushSendResult.Sent) s.LastUsedAt = DateTime.UtcNow;
        }
        foreach (var s in deadSubs) s.IsDeleted = true;

        var deadTokens = new List<Domain.DeviceToken>();
        foreach (var t in tokens)
        {
            var result = await fcm.SendAsync(t.Token, payload, ct);
            if (result == Hms.Api.Features.Push.PushSendResult.Gone) deadTokens.Add(t);
            else if (result == Hms.Api.Features.Push.PushSendResult.Sent) t.LastUsedAt = DateTime.UtcNow;
        }
        foreach (var t in deadTokens) t.IsDeleted = true;

        await db.SaveChangesAsync(ct);
    }

    // ── Signed table token (HMAC over tenant|location|table|label, keyed by the JWT secret) ──
    private static string Sign(IConfiguration cfg, Guid tenantId, Guid locationId, Guid? tableId, string? tableLabel)
    {
        var payload = $"{tenantId}|{locationId}|{tableId}|{tableLabel}";
        var b = B64(Encoding.UTF8.GetBytes(payload));
        return $"{b}.{B64(Hmac(cfg, b))}";
    }

    private static (Guid tenantId, Guid locationId, Guid? tableId, string? tableLabel)? Verify(IConfiguration cfg, string token)
    {
        try
        {
            var dot = token.IndexOf('.');
            if (dot <= 0) return null;
            var b = token[..dot];
            var sig = token[(dot + 1)..];
            if (!CryptographicOperations.FixedTimeEquals(UnB64(sig), Hmac(cfg, b))) return null;
            var parts = Encoding.UTF8.GetString(UnB64(b)).Split('|');
            if (parts.Length < 4) return null;
            Guid? tid = Guid.TryParse(parts[2], out var t) ? t : null;
            var label = parts[3].Length == 0 ? null : parts[3];
            return (Guid.Parse(parts[0]), Guid.Parse(parts[1]), tid, label);
        }
        catch { return null; }
    }

    private static byte[] Hmac(IConfiguration cfg, string data)
    {
        using var h = new HMACSHA256(Encoding.UTF8.GetBytes(cfg.GetSection("Jwt")["SigningKey"]!));
        return h.ComputeHash(Encoding.UTF8.GetBytes(data));
    }

    private static string B64(byte[] b) => Convert.ToBase64String(b).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    private static byte[] UnB64(string s)
    {
        s = s.Replace('-', '+').Replace('_', '/');
        switch (s.Length % 4) { case 2: s += "=="; break; case 3: s += "="; break; }
        return Convert.FromBase64String(s);
    }

    private static string MintGuestJwt(IConfiguration cfg, Guid tenantId, string slug, Guid locationId, Guid? tableId, string? tableLabel)
    {
        var jwtCfg = cfg.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtCfg["SigningKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, $"guest:{tableId}"),
            new("tenant_id", tenantId.ToString()),
            new("tenant_slug", slug),
            new("role", "Guest"),
            new("home_location_id", locationId.ToString()),
            new("table_id", tableId?.ToString() ?? ""),
            new("table_label", tableLabel ?? ""),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };
        var token = new JwtSecurityToken(jwtCfg["Issuer"], jwtCfg["Audience"], claims,
            expires: DateTime.UtcNow.AddHours(3), signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public record GuestOrderInput(List<GuestLine> Items);
public record GuestLine(string ProductId, int Quantity);
