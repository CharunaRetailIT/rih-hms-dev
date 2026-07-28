using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Hms.Api.Domain;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Hms.Api.Features.Auth;

/// <summary>
/// v1 magic-link auth, stubbed for local dev.
/// POST /api/v1/auth/magic-link  →  logs the link to console (no email yet)
/// POST /api/v1/auth/exchange     →  exchanges the token for a JWT
///
/// Sprint 2 wires real email delivery. Sprint 4+ adds Entra External ID.
/// </summary>
public static class AuthEndpoint
{
    private static readonly Dictionary<string, MagicLinkRecord> _pending = new();

    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        // Pre-authentication endpoints — these are how you GET a token.
        var g = app.MapGroup("/api/v1/auth").WithTags("Auth").AllowAnonymous();

        g.MapPost("/magic-link", async (
            MagicLinkRequest req,
            ControlDbContext control,
            ITenantDbContextFactory factory,
            IConfiguration cfg,
            IWebHostEnvironment env,
            Hms.Api.Features.Email.IEmailSender email,
            ILogger<MagicLinkRecord> logger,
            CancellationToken ct) =>
        {
            // Find tenant by slug.
            var tenant = await control.Tenants.AsNoTracking()
                .FirstOrDefaultAsync(t => t.Slug == req.TenantSlug.ToLowerInvariant(), ct);
            if (tenant is null) return Results.NotFound(new { error = "tenant not found" });

            // Find user in tenant DB.
            await using var tenantDb = factory.Create(tenant.Id);
            var user = await tenantDb.Users.AsNoTracking()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.TenantId == tenant.Id && u.Email == req.Email.ToLowerInvariant() && !u.IsDeleted, ct);
            if (user is null) return Results.NotFound(new { error = "no user with that email in this tenant" });
            if (!user.IsActive) return Results.Json(new { error = "this account is deactivated" }, statusCode: 403);

            var token = Guid.NewGuid().ToString("N");
            _pending[token] = new MagicLinkRecord(tenant.Id, user.Id, DateTime.UtcNow.AddMinutes(15));

            var baseUrl = cfg["App:WebBaseUrl"] ?? "http://localhost:3000";
            var link = $"{baseUrl}/auth/callback?token={token}";
            // DEV: log + return the link. PROD: send via SES/SendGrid/Mailgun and
            // NEVER return it in the response (it's a bearer credential).
            logger.LogInformation("MAGIC LINK for {Email} in {TenantSlug}: {Link}", req.Email, req.TenantSlug, link);

            // Deliver the link by email (best-effort — it's also logged, and returned in Development).
            var html = Hms.Api.Features.Email.EmailTemplates.MagicLink(tenant.DisplayName, link);
            var emailed = await email.SendAsync(req.Email, "Your RIT HMS sign-in link", html, ct);
            if (!emailed) logger.LogWarning("Magic-link email to {Email} was not delivered (see provider logs)", req.Email);

            return env.IsDevelopment()
                ? Results.Ok(new { sent = true, devLink = link })
                : Results.Ok(new { sent = true });
        })
        .WithName("Auth.RequestMagicLink")
        .WithSummary("Request a magic-link email. The link is only returned in the body in Development.");

        g.MapPost("/exchange", async (
            ExchangeRequest req,
            ControlDbContext control,
            ITenantDbContextFactory factory,
            IConfiguration cfg,
            CancellationToken ct) =>
        {
            if (!_pending.TryGetValue(req.Token, out var rec) || rec.ExpiresAt < DateTime.UtcNow)
                return Results.Unauthorized();

            _pending.Remove(req.Token);

            var tenant = await control.Tenants.AsNoTracking().FirstAsync(t => t.Id == rec.TenantId, ct);

            await using var tenantDb = factory.Create(tenant.Id);
            var user = await tenantDb.Users.IgnoreQueryFilters().FirstAsync(u => u.Id == rec.UserId, ct);
            if (!user.IsActive || user.IsDeleted) return Results.Unauthorized();
            user.LastLoginAt = DateTime.UtcNow;
            await tenantDb.SaveChangesAsync(ct);

            var jwt = MintJwt(cfg, tenant, user);
            var refresh = await IssueRefreshAsync(control, tenant.Id, user.Id, ct);
            return Results.Ok(new
            {
                accessToken = jwt,
                refreshToken = refresh,
                tenant = new { tenant.Id, tenant.Slug, tenant.DisplayName },
                user = new { user.Id, user.Email, user.DisplayName, user.Role, user.HomeLocationId, user.IsServer }
            });
        })
        .WithName("Auth.ExchangeMagicLink")
        .WithSummary("Exchange a magic-link token for an access JWT + refresh token.");

        // Renew an access token using a (rotating) refresh token.
        g.MapPost("/refresh", async (RefreshRequest req, ControlDbContext control,
            ITenantDbContextFactory factory, IConfiguration cfg, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(req.RefreshToken)) return Results.Unauthorized();
            var hash = HashToken(req.RefreshToken);
            var rec = await control.RefreshTokens.FirstOrDefaultAsync(r => r.TokenHash == hash, ct);
            if (rec is null || !rec.IsValid(DateTime.UtcNow)) return Results.Unauthorized();

            var tenant = await control.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == rec.TenantId, ct);
            if (tenant is null) return Results.Unauthorized();
            await using var tenantDb = factory.Create(tenant.Id);
            var user = await tenantDb.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == rec.UserId, ct);
            if (user is null || !user.IsActive || user.IsDeleted) return Results.Unauthorized();

            // rotate: revoke the used token, issue a fresh one
            rec.RevokedAt = DateTime.UtcNow;
            var refresh = await IssueRefreshAsync(control, tenant.Id, user.Id, ct);
            return Results.Ok(new
            {
                accessToken = MintJwt(cfg, tenant, user),
                refreshToken = refresh,
                tenant = new { tenant.Id, tenant.Slug, tenant.DisplayName },
                user = new { user.Id, user.Email, user.DisplayName, user.Role, user.HomeLocationId, user.IsServer }
            });
        })
        .WithName("Auth.Refresh")
        .WithSummary("Exchange a refresh token for a new access JWT (rotates the refresh token).");

        // Revoke a refresh token (logout).
        g.MapPost("/logout", async (RefreshRequest req, ControlDbContext control, CancellationToken ct) =>
        {
            if (!string.IsNullOrWhiteSpace(req.RefreshToken))
            {
                var hash = HashToken(req.RefreshToken);
                var rec = await control.RefreshTokens.FirstOrDefaultAsync(r => r.TokenHash == hash, ct);
                if (rec is not null && rec.RevokedAt is null) { rec.RevokedAt = DateTime.UtcNow; await control.SaveChangesAsync(ct); }
            }
            return Results.NoContent();
        })
        .WithName("Auth.Logout")
        .WithSummary("Revoke a refresh token.");

        // ── Staff PIN login (for POS staff without email) ───────────────────────
        // No public roster (that would leak the staff list per tenant slug). Staff
        // sign in with workspace + username + PIN.
        g.MapPost("/pin", async (PinLoginRequest req, ControlDbContext control,
            ITenantDbContextFactory factory, IConfiguration cfg, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(req.TenantSlug) || string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Pin))
                return Results.BadRequest(new { error = "tenantSlug, username and pin are required" });
            var tenant = await control.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Slug == req.TenantSlug.ToLowerInvariant(), ct);
            if (tenant is null) return Results.Unauthorized();

            await using var db = factory.Create(tenant.Id);
            var username = req.Username.Trim().ToLowerInvariant();
            var user = await db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.TenantId == tenant.Id && u.Username == username, ct);
            if (user is null || user.IsDeleted || !user.IsActive || user.PasscodeHash is null) return Results.Unauthorized();

            var now = DateTime.UtcNow;
            if (user.PinLockedUntil is { } until && until > now)
                return Results.Json(new { error = "Too many attempts — locked. Try again shortly." }, statusCode: 423);

            if (!PinHasher.Verify(req.Pin, user.PasscodeHash))
            {
                // Lock for 15 min after 5 consecutive misses (brute-force guard).
                user.PinFailedCount += 1;
                if (user.PinFailedCount >= 5) { user.PinLockedUntil = now.AddMinutes(15); user.PinFailedCount = 0; }
                await db.SaveChangesAsync(ct);
                return Results.Unauthorized();
            }

            user.PinFailedCount = 0; user.PinLockedUntil = null; user.LastLoginAt = now;
            await db.SaveChangesAsync(ct);

            var jwt = MintJwt(cfg, tenant, user);
            var refresh = await IssueRefreshAsync(control, tenant.Id, user.Id, ct);
            return Results.Ok(new
            {
                accessToken = jwt, refreshToken = refresh,
                tenant = new { tenant.Id, tenant.Slug, tenant.DisplayName },
                user = new { user.Id, user.Email, user.DisplayName, user.Role, user.HomeLocationId, user.IsServer },
            });
        }).WithName("Auth.PinLogin").WithSummary("Sign in with a staff PIN (PBKDF2-hashed; rate-limited + lockout).");

        return app;
    }

    private static string HashToken(string raw) =>
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(raw))).ToLowerInvariant();

    private static async Task<string> IssueRefreshAsync(ControlDbContext control, Guid tenantId, Guid userId, CancellationToken ct)
    {
        var raw = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32))
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');   // url-safe
        control.RefreshTokens.Add(new RefreshToken
        {
            TenantId = tenantId, UserId = userId, TokenHash = HashToken(raw),
            ExpiresAt = DateTime.UtcNow.AddDays(30),
        });
        await control.SaveChangesAsync(ct);
        return raw;
    }

    private static string MintJwt(IConfiguration cfg, Tenant tenant, User user)
    {
        var jwtCfg = cfg.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtCfg["SigningKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new("tenant_id", tenant.Id.ToString()),
            new("tenant_slug", tenant.Slug),
            new("role", user.Role.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        // Pinned-outlet claim (#scoping-P2): drives server-side location enforcement.
        // Absent for all-access users (Owner / no home location).
        if (user.HomeLocationId is Guid home)
            claims.Add(new Claim("home_location_id", home.ToString()));

        var token = new JwtSecurityToken(
            issuer: jwtCfg["Issuer"],
            audience: jwtCfg["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public record PinLoginRequest(string TenantSlug, string Username, string Pin);
public record MagicLinkRequest(string TenantSlug, string Email);
public record ExchangeRequest(string Token);
public record RefreshRequest(string RefreshToken);
public record MagicLinkRecord(Guid TenantId, Guid UserId, DateTime ExpiresAt);
