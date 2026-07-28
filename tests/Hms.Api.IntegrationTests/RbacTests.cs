using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Hms.Api.IntegrationTests;

/// <summary>
/// Role-based access enforced at the API. Mints a JWT carrying a `role` claim
/// (Owner/Manager/Cashier/Kitchen/Accountant) for the test tenant and checks the
/// policy on representative endpoints. Hosted in Production so only the JWT path
/// is live (no dev header).
/// </summary>
[Collection("pg")]
public class RbacTests(PostgresFixture fx) : IAsyncLifetime
{
    public Task InitializeAsync() => fx.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private HttpClient Client(string role)
    {
        var host = new HmsAuthTestFactory(fx.ControlConn, fx.TenantTemplate);
        var client = host.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Mint(role));
        return client;
    }

    private string Mint(string role)
    {
        var creds = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                "dev-only-signing-key-replace-in-production-with-vault-secret-min-32-chars")),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: "https://localhost:5001", audience: "rit-hms-api",
            claims: new[] { new Claim("tenant_id", fx.TenantId.ToString()), new Claim("role", role) },
            expires: DateTime.UtcNow.AddHours(1), signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [Theory]
    // endpoint                              Owner Manager Cashier Kitchen Accountant
    [InlineData("/api/v1/users",            "Owner",      HttpStatusCode.OK)]
    [InlineData("/api/v1/users",            "Manager",    HttpStatusCode.Forbidden)]
    [InlineData("/api/v1/users",            "Cashier",    HttpStatusCode.Forbidden)]
    [InlineData("/api/v1/orders/open",      "Cashier",    HttpStatusCode.OK)]
    [InlineData("/api/v1/orders/open",      "Accountant", HttpStatusCode.Forbidden)]
    [InlineData("/api/v1/reports/sales/summary", "Accountant", HttpStatusCode.OK)]
    [InlineData("/api/v1/reports/sales/summary", "Cashier", HttpStatusCode.Forbidden)]
    [InlineData("/api/v1/suppliers",        "Manager",    HttpStatusCode.OK)]
    [InlineData("/api/v1/suppliers",        "Cashier",    HttpStatusCode.Forbidden)]
    [InlineData("/api/v1/kitchen/tickets",  "Kitchen",    HttpStatusCode.OK)]
    public async Task Endpoint_access_matches_role(string path, string role, HttpStatusCode expected)
    {
        var res = await Client(role).GetAsync(path);
        res.StatusCode.Should().Be(expected);
    }
}
