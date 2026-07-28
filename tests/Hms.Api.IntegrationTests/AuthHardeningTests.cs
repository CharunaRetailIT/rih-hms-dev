using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Hms.Api.IntegrationTests;

/// <summary>#64 auth hardening: prod-secret fail-fast, admin-gated tenant list, refresh/logout.</summary>
[Collection("pg")]
public class AuthHardeningTests(PostgresFixture fx) : IAsyncLifetime
{
    public Task InitializeAsync() => fx.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    // ── pure secret-guard logic (no host needed) ──
    [Fact]
    public void Production_rejects_dev_signing_key()
    {
        var act = () => SecurityStartup.AssertProductionSecrets(true, SecurityStartup.DevSigningKey, "a-real-master-key");
        act.Should().Throw<InvalidOperationException>().WithMessage("*SigningKey*");
    }

    [Fact]
    public void Production_rejects_dev_master_key()
    {
        var act = () => SecurityStartup.AssertProductionSecrets(true, new string('k', 40), SecurityStartup.DevMasterKeyB64);
        act.Should().Throw<InvalidOperationException>().WithMessage("*MasterKey*");
    }

    [Fact]
    public void Production_accepts_real_secrets()
    {
        var act = () => SecurityStartup.AssertProductionSecrets(true, new string('k', 40), "some-real-base64-master-key==");
        act.Should().NotThrow();
    }

    [Fact]
    public void Non_production_allows_dev_defaults()
    {
        var act = () => SecurityStartup.AssertProductionSecrets(false, SecurityStartup.DevSigningKey, SecurityStartup.DevMasterKeyB64);
        act.Should().NotThrow();
    }

    // ── HTTP: tenant list is gated, refresh/logout rotate ──
    private HttpClient Client(string role = "Owner", string email = "owner@demo.local")
    {
        var host = new HmsAuthTestFactory(fx.ControlConn, fx.TenantTemplate);
        var c = host.CreateClient();
        var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            "dev-only-signing-key-replace-in-production-with-vault-secret-min-32-chars")), SecurityAlgorithms.HmacSha256);
        var jwt = new JwtSecurityToken("https://localhost:5001", "rit-hms-api",
            new[] { new Claim("tenant_id", fx.TenantId.ToString()), new Claim("role", role), new Claim("email", email) },
            expires: DateTime.UtcNow.AddHours(1), signingCredentials: creds);
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", new JwtSecurityTokenHandler().WriteToken(jwt));
        return c;
    }

    [Fact]
    public async Task Tenant_list_is_forbidden_for_a_normal_owner()
    {
        // Owner is authenticated but not a PlatformAdmin (no allowlist configured).
        var res = await Client("Owner").GetAsync("/api/v1/tenants");
        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Signup_lookup_stays_anonymous()
    {
        var anon = new HmsAuthTestFactory(fx.ControlConn, fx.TenantTemplate).CreateClient();
        (await anon.GetAsync("/api/v1/tenants/by-slug/it")).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Magic_link_is_not_returned_outside_development()
    {
        await using (var db = fx.NewTenantContext())
        {
            db.Users.Add(new Hms.Api.Domain.User { Email = "ml@it.local", DisplayName = "ML User", IsActive = true });
            await db.SaveChangesAsync();
        }
        var anon = new HmsAuthTestFactory(fx.ControlConn, fx.TenantTemplate).CreateClient();
        var ml = await anon.PostAsJsonAsync("/api/v1/auth/magic-link", new { tenantSlug = "it", email = "ml@it.local" });
        ml.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await ml.Content.ReadFromJsonAsync<JsonElement>();
        body.TryGetProperty("devLink", out _).Should().BeFalse("the bearer link must not appear in the response body outside Development");
    }

    private static string HashToken(string raw) =>
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(raw))).ToLowerInvariant();

    [Fact]
    public async Task Refresh_rotates_and_logout_revokes()
    {
        // seed an active user + a known refresh token pointing at it
        var userId = Guid.NewGuid();
        await using (var db = fx.NewTenantContext())
        {
            db.Users.Add(new Hms.Api.Domain.User { Id = userId, Email = "refresh@demo.local", DisplayName = "Refresh User", IsActive = true });
            await db.SaveChangesAsync();
        }
        await using (var control = new ControlDbContext(new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<ControlDbContext>()
            .UseNpgsql(fx.ControlConn).UseSnakeCaseNamingConvention().Options))
        {
            control.RefreshTokens.Add(new Hms.Api.Domain.RefreshToken
            {
                TenantId = fx.TenantId, UserId = userId, TokenHash = HashToken("seed-token"),
                ExpiresAt = DateTime.UtcNow.AddDays(30),
            });
            await control.SaveChangesAsync();
        }

        var anon = new HmsAuthTestFactory(fx.ControlConn, fx.TenantTemplate).CreateClient();

        // refresh → new access + rotated refresh token
        var r1 = await anon.PostAsJsonAsync("/api/v1/auth/refresh", new { refreshToken = "seed-token" });
        r1.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await r1.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("accessToken").GetString().Should().NotBeNullOrEmpty();
        var rotated = body.GetProperty("refreshToken").GetString()!;
        rotated.Should().NotBe("seed-token");

        // the old token is now revoked
        (await anon.PostAsJsonAsync("/api/v1/auth/refresh", new { refreshToken = "seed-token" }))
            .StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // logout revokes the rotated token too
        (await anon.PostAsJsonAsync("/api/v1/auth/logout", new { refreshToken = rotated }))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await anon.PostAsJsonAsync("/api/v1/auth/refresh", new { refreshToken = rotated }))
            .StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
