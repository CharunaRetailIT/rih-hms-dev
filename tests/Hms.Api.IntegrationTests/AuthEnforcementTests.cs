using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Hms.Api.IntegrationTests;

/// <summary>
/// HTTP-level auth enforcement, hosting the real Program over an in-memory test
/// server (Production environment, so the dev X-Tenant-Id scheme is inert). Locks
/// the fix where the minted JWT was never validated and the dev header bypassed
/// tenant isolation. Connection strings are pointed at the fixture's test DBs.
/// </summary>
[Collection("pg")]
public class AuthEnforcementTests(PostgresFixture fx) : IAsyncLifetime
{
    public Task InitializeAsync() => fx.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    // Mirrors the Jwt config injected into the test host below.
    private const string Issuer = "https://localhost:5001";
    private const string Audience = "rit-hms-api";
    private const string SigningKey = "dev-only-signing-key-replace-in-production-with-vault-secret-min-32-chars";

    private WebApplicationFactory<Program> NewHost() =>
        new HmsAuthTestFactory(fx.ControlConn, fx.TenantTemplate);

    private static string MintJwt(Guid tenantId)
    {
        var creds = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: Issuer, audience: Audience,
            claims: new[] { new Claim("tenant_id", tenantId.ToString()), new Claim("role", "0") },
            expires: DateTime.UtcNow.AddHours(1), signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [Fact]
    public async Task Protected_endpoint_without_a_token_is_401()
    {
        using var host = NewHost();
        var res = await host.CreateClient().GetAsync("/api/v1/products");
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Bogus_bearer_token_is_401()
    {
        using var host = NewHost();
        var client = host.CreateClient();
        var req = new HttpRequestMessage(HttpMethod.Get, "/api/v1/products");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "not.a.real.token");
        (await client.SendAsync(req)).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task X_Tenant_Id_header_does_not_bypass_in_production()
    {
        using var host = NewHost();
        var client = host.CreateClient();
        var req = new HttpRequestMessage(HttpMethod.Get, "/api/v1/products");
        req.Headers.Add("X-Tenant-Id", fx.TenantId.ToString());
        (await client.SendAsync(req)).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Valid_jwt_is_accepted()
    {
        using var host = NewHost();
        var client = host.CreateClient();
        var req = new HttpRequestMessage(HttpMethod.Get, "/api/v1/products");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", MintJwt(fx.TenantId));
        (await client.SendAsync(req)).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Public_endpoints_stay_anonymous()
    {
        using var host = NewHost();
        var client = host.CreateClient();
        (await client.GetAsync("/health")).StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

/// <summary>Hosts Program in Production, with connection strings + JWT config overridden for tests.</summary>
internal sealed class HmsAuthTestFactory(string controlConn, string tenantTemplate) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // "Testing" (not "Development") so the dev X-Tenant-Id scheme stays inert
        // AND the Production secret fail-fast doesn't trip on the dev signing key.
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, cfg) =>
        {
            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:ControlDb"] = controlConn,
                ["ConnectionStrings:TenantTemplateDb"] = tenantTemplate,
                ["Jwt:Issuer"] = "https://localhost:5001",
                ["Jwt:Audience"] = "rit-hms-api",
                ["Jwt:SigningKey"] = "dev-only-signing-key-replace-in-production-with-vault-secret-min-32-chars",
                ["Secrets:MasterKey"] = "ZGV2LW9ubHktbWFzdGVyLWtleS0zMmJ5dGVzLXJpdC1obXM=",
            });
        });
    }
}
