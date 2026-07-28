using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Hms.Api.Domain;
using Hms.Api.Features.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Hms.Api.IntegrationTests;

/// <summary>
/// #55c price levels. A product can override its price per level; an order
/// resolves a level (explicit &gt; level matching the order type &gt; default) and
/// prices its lines accordingly — e.g. delivery orders priced above dine-in,
/// automatically.
/// </summary>
[Collection("pg")]
public class PriceLevelTests(PostgresFixture fx) : IAsyncLifetime
{
    public Task InitializeAsync() => fx.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private async Task SetDeliveryPrice(Guid productId, decimal price)
    {
        await using var db = fx.NewTenantContext();
        db.ProductPrices.Add(new ProductPrice { ProductId = productId, PriceLevelId = fx.DeliveryLevelId, Price = price });
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task Delivery_order_uses_the_delivery_level_override()
    {
        await SetDeliveryPrice(fx.ChickenKottuId, 1100m);   // dine-in base is 900
        var (orders, _) = fx.Services();

        // delivery order auto-resolves to the DELIVERY level (applies_to_order_type)
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "delivery", null, null, null, 1, null), default);
        o.PriceLevelId.Should().Be(fx.DeliveryLevelId);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);

        var line = (await orders.GetAsync(o.Id, default))!.Items.Single();
        line.UnitPrice.Should().Be(1100m);
    }

    [Fact]
    public async Task Dine_in_order_uses_the_base_price_when_no_override()
    {
        await SetDeliveryPrice(fx.ChickenKottuId, 1100m);
        var (orders, _) = fx.Services();

        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "5", 1, null), default);
        o.PriceLevelId.Should().Be(fx.DineInLevelId);       // default level
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);

        var line = (await orders.GetAsync(o.Id, default))!.Items.Single();
        line.UnitPrice.Should().Be(900m);                   // base — no DINEIN override
    }

    [Fact]
    public async Task Explicit_price_level_overrides_order_type_resolution()
    {
        await SetDeliveryPrice(fx.ChickenKottuId, 1100m);
        var (orders, _) = fx.Services();

        // a takeaway order, but the caller forces the DELIVERY level
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "takeaway", null, null, null, 1, null,
            PriceLevelId: fx.DeliveryLevelId), default);
        o.PriceLevelId.Should().Be(fx.DeliveryLevelId);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);

        (await orders.GetAsync(o.Id, default))!.Items.Single().UnitPrice.Should().Be(1100m);
    }

    [Fact]
    public async Task Variant_price_wins_over_a_level_override()
    {
        await SetDeliveryPrice(fx.LionLagerId, 999m);
        Guid potId;
        await using (var db = fx.NewTenantContext())
        {
            var pot = new ProductVariant { ProductId = fx.LionLagerId, Code = "POT", Name = "Pot", Price = 400m };
            db.ProductVariants.Add(pot); await db.SaveChangesAsync(); potId = pot.Id;
        }
        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "delivery", null, null, null, 1, null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.LionLagerId, 1, "bar", null, null, potId), default);

        (await orders.GetAsync(o.Id, default))!.Items.Single().UnitPrice.Should().Be(400m);  // variant, not 999
    }

    // ── endpoints + RBAC ──────────────────────────────────────────────────────

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
    public async Task Manager_manages_levels_and_per_product_prices()
    {
        var mgr = Client("Manager");

        var put = await mgr.PutAsJsonAsync("/api/v1/price-levels",
            new { code = "agg", name = "Aggregator", isDefault = false, appliesToOrderType = (string?)null, sortOrder = 2 });
        put.StatusCode.Should().Be(HttpStatusCode.OK);
        var level = await put.Content.ReadFromJsonAsync<JsonElement>();
        level.GetProperty("code").GetString().Should().Be("AGG");
        var aggId = level.GetProperty("id").GetString();

        // set a per-product override against the aggregator level
        var setPrices = await mgr.PutAsJsonAsync($"/api/v1/products/{fx.ChickenKottuId}/prices",
            new { prices = new[] { new { priceLevelId = aggId, price = 1250m } } });
        setPrices.StatusCode.Should().Be(HttpStatusCode.OK);

        var prices = await mgr.GetFromJsonAsync<JsonElement>($"/api/v1/products/{fx.ChickenKottuId}/prices");
        prices.EnumerateArray().First().GetProperty("price").GetDecimal().Should().Be(1250m);

        var list = await mgr.GetFromJsonAsync<JsonElement>("/api/v1/price-levels");
        list.EnumerateArray().Select(l => l.GetProperty("code").GetString())
            .Should().Contain(new[] { "DINEIN", "DELIVERY", "AGG" });
    }

    [Fact]
    public async Task Default_level_cannot_be_deleted()
    {
        var mgr = Client("Manager");
        var del = await mgr.DeleteAsync($"/api/v1/price-levels/{fx.DineInLevelId}");
        del.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Cashier_cannot_manage_price_levels()
    {
        var cashier = Client("Cashier");
        var put = await cashier.PutAsJsonAsync("/api/v1/price-levels", new { code = "X", name = "X" });
        put.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
