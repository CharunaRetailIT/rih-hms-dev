using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Hms.Api.IntegrationTests;

/// <summary>
/// Product create/update/delete over HTTP, with write-RBAC: Owner/Manager may
/// manage the menu; a Cashier is forbidden. Hosted via the real Program.
/// </summary>
[Collection("pg")]
public class ProductCrudTests(PostgresFixture fx) : IAsyncLifetime
{
    public Task InitializeAsync() => fx.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private HttpClient Client(string role)
    {
        var host = new HmsAuthTestFactory(fx.ControlConn, fx.TenantTemplate);
        var c = host.CreateClient();
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Mint(role));
        return c;
    }

    private string Mint(string role)
    {
        var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            "dev-only-signing-key-replace-in-production-with-vault-secret-min-32-chars")), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: "https://localhost:5001", audience: "rit-hms-api",
            claims: new[] { new Claim("tenant_id", fx.TenantId.ToString()), new Claim("role", role) },
            expires: DateTime.UtcNow.AddHours(1), signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [Fact]
    public async Task Owner_can_create_update_and_delete_a_product()
    {
        var owner = Client("Owner");

        var units = await owner.GetFromJsonAsync<JsonElement>("/api/v1/units-of-measure");
        var unitId = units[0].GetProperty("id").GetString();

        var create = await owner.PostAsJsonAsync("/api/v1/products", new
        {
            sku = "NEW-SKU-1", name = "New Dish", unitOfMeasureId = unitId, basePrice = 100m,
        });
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var id = (await create.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetString();

        var upd = await owner.PutAsJsonAsync($"/api/v1/products/{id}", new { basePrice = 999m, name = "Renamed Dish" });
        upd.StatusCode.Should().Be(HttpStatusCode.OK);

        var got = await owner.GetFromJsonAsync<JsonElement>($"/api/v1/products/{id}");
        got.GetProperty("basePrice").GetDecimal().Should().Be(999m);
        got.GetProperty("name").GetString().Should().Be("Renamed Dish");

        var del = await owner.DeleteAsync($"/api/v1/products/{id}");
        del.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Cashier_cannot_create_or_update_products()
    {
        var cashier = Client("Cashier");
        var units = await Client("Owner").GetFromJsonAsync<JsonElement>("/api/v1/units-of-measure");
        var unitId = units[0].GetProperty("id").GetString();

        var create = await cashier.PostAsJsonAsync("/api/v1/products", new
        {
            sku = "X", name = "X", unitOfMeasureId = unitId, basePrice = 1m,
        });
        create.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var upd = await cashier.PutAsJsonAsync($"/api/v1/products/{fx.ChickenKottuId}", new { basePrice = 5m });
        upd.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
