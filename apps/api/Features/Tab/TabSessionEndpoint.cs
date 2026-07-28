using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Hms.Api.Features.Tab;

/// <summary>
/// Tab handheld auth (#107). A device registered via the #106 spine holds a one-time token +
/// knows its workspace slug. This exchanges {tenantSlug, token} for a short-lived JWT scoped to
/// the tenant, with role "Cashier" (so it can take orders via the Operations policy) and pinned
/// to the device's outlet (home_location_id → server-side location clamp). The Flutter app stores
/// the pairing creds and re-exchanges when the JWT expires.
/// </summary>
public static class TabSessionEndpoint
{
    public static IEndpointRouteBuilder MapTabSessionEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/tab/session", async (TabSessionInput i, ControlDbContext control,
            ITenantDbContextFactory factory, IConfiguration cfg, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(i.TenantSlug) || string.IsNullOrWhiteSpace(i.Token))
                return Results.BadRequest(new { error = "tenantSlug and token are required" });

            var slug = i.TenantSlug.Trim().ToLowerInvariant();
            var tenant = await control.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Slug == slug, ct);
            if (tenant is null) return Results.Unauthorized();

            await using var db = factory.Create(tenant.Id);
            // Device codes are case-insensitive (issued as upper-hex) — normalise so a tester
            // who types it lowercase still pairs instead of getting a 401.
            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(i.Token.Trim().ToUpperInvariant())));
            var dev = await db.TabDevices.FirstOrDefaultAsync(d => d.TokenHash == hash && d.IsActive, ct);
            if (dev is null) return Results.Unauthorized();

            var s = await db.OrgSettings.AsNoTracking().FirstOrDefaultAsync(ct);
            if ((s?.TabDeviceLimit ?? 0) <= 0)
                return Results.Json(new { error = "Tab Ordering is not enabled for this workspace." }, statusCode: StatusCodes.Status403Forbidden);

            dev.LastSeenAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);

            var jwt = MintDeviceJwt(cfg, tenant.Id, tenant.Slug, dev.Id, dev.Name, dev.LocationId);
            return Results.Ok(new
            {
                accessToken = jwt,
                tenant = new { tenant.Slug, tenant.DisplayName, logoUrl = s?.LogoUrl },
                locationId = dev.LocationId,
                deviceId = dev.Id,
                deviceName = dev.Name,
            });
        }).WithName("Tab.Session").WithTags("Tab").AllowAnonymous();

        return app;
    }

    private static string MintDeviceJwt(IConfiguration cfg, Guid tenantId, string slug, Guid deviceId, string name, Guid? locationId)
    {
        var jwtCfg = cfg.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtCfg["SigningKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, deviceId.ToString()),
            new("tenant_id", tenantId.ToString()),
            new("tenant_slug", slug),
            new("role", "Cashier"),                 // handheld takes orders (Operations policy)
            new("tab_device_id", deviceId.ToString()),
            new("name", name),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };
        if (locationId is Guid loc) claims.Add(new Claim("home_location_id", loc.ToString())); // pin handheld to its outlet
        var token = new JwtSecurityToken(jwtCfg["Issuer"], jwtCfg["Audience"], claims,
            expires: DateTime.UtcNow.AddHours(12), signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public record TabSessionInput(string TenantSlug, string Token);
