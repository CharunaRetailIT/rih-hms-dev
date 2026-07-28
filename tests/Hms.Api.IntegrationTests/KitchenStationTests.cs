using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Hms.Api.Features.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Hms.Api.IntegrationTests;

/// <summary>
/// #55a kitchen / printer routing. A product carries a KitchenStationCode; when
/// it's added to an order the line's Station is taken from the product (not the
/// caller), and ConfirmAsync splits the KOT one-ticket-per-station so each prints
/// at the right place. Also covers the kitchen-stations CRUD endpoint + write-RBAC.
/// </summary>
[Collection("pg")]
public class KitchenStationTests(PostgresFixture fx) : IAsyncLifetime
{
    public Task InitializeAsync() => fx.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    // ── service-level routing ────────────────────────────────────────────────

    [Fact]
    public async Task Product_station_code_overrides_the_caller_supplied_station()
    {
        // Route Chicken Kottu to the BAR station on the product master.
        await using (var db = fx.NewTenantContext())
        {
            var p = await db.Products.IgnoreQueryFilters().FirstAsync(x => x.Id == fx.ChickenKottuId);
            p.KitchenStationCode = "BAR";
            await db.SaveChangesAsync();
        }

        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "1", 1, null), default);
        // Caller asks for "kitchen" — the product's station must win.
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);

        await using var db2 = fx.NewTenantContext();
        var line = await db2.OrderItems.IgnoreQueryFilters().FirstAsync(i => i.OrderId == o.Id);
        line.Station.Should().Be("BAR");
    }

    [Fact]
    public async Task Product_without_a_station_code_falls_back_to_caller_station()
    {
        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "2", 1, null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.LionLagerId, 1, "bar", null), default);

        await using var db = fx.NewTenantContext();
        var line = await db.OrderItems.IgnoreQueryFilters().FirstAsync(i => i.OrderId == o.Id);
        line.Station.Should().Be("bar");
    }

    [Fact]
    public async Task Confirm_splits_kot_by_per_product_station()
    {
        await using (var db = fx.NewTenantContext())
        {
            (await db.Products.IgnoreQueryFilters().FirstAsync(x => x.Id == fx.ChickenKottuId)).KitchenStationCode = "KITCHEN";
            (await db.Products.IgnoreQueryFilters().FirstAsync(x => x.Id == fx.LionLagerId)).KitchenStationCode = "BAR";
            await db.SaveChangesAsync();
        }

        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "3", 2, null), default);
        // both added with the same caller station — the product routing must still split them
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.LionLagerId, 2, "kitchen", null), default);

        await orders.ConfirmAsync(o.Id, default);

        await using var db2 = fx.NewTenantContext();
        var tickets = await db2.KitchenTickets.IgnoreQueryFilters().Where(t => t.OrderId == o.Id).ToListAsync();
        tickets.Should().HaveCount(2);
        tickets.Select(t => t.Station).Should().BeEquivalentTo(new[] { "KITCHEN", "BAR" });
    }

    // ── endpoint CRUD + RBAC ──────────────────────────────────────────────────

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
    public async Task Manager_can_upsert_and_delete_a_station()
    {
        var mgr = Client("Manager");

        var put = await mgr.PutAsJsonAsync("/api/v1/kitchen-stations",
            new { code = "grill", name = "Grill", printerName = "kot-grill", sortOrder = 5 });
        put.StatusCode.Should().Be(HttpStatusCode.OK);
        var created = await put.Content.ReadFromJsonAsync<JsonElement>();
        created.GetProperty("code").GetString().Should().Be("GRILL");           // normalised upper-case
        var id = created.GetProperty("id").GetString();

        var list = await mgr.GetFromJsonAsync<JsonElement>("/api/v1/kitchen-stations");
        list.EnumerateArray().Select(s => s.GetProperty("code").GetString())
            .Should().Contain(new[] { "KITCHEN", "BAR", "GRILL" });

        var del = await mgr.DeleteAsync($"/api/v1/kitchen-stations/{id}");
        del.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Cashier_cannot_manage_stations_but_can_read_them()
    {
        var cashier = Client("Cashier");

        var put = await cashier.PutAsJsonAsync("/api/v1/kitchen-stations", new { code = "X", name = "X" });
        put.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var list = await cashier.GetFromJsonAsync<JsonElement>("/api/v1/kitchen-stations");
        list.GetArrayLength().Should().BeGreaterThan(0);   // seeded KITCHEN/BAR readable
    }
}
