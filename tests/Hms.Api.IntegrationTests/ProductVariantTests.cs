using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Hms.Api.Domain;
using Hms.Api.Features.Modifiers;
using Hms.Api.Features.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Hms.Api.IntegrationTests;

/// <summary>
/// #55b serving-size variants. A product can have several sellable sizes
/// (Cup/Pot, S/M/L), each with its own price; choosing one on a line replaces
/// the base price and snapshots the size label onto the order item & KOT.
/// </summary>
[Collection("pg")]
public class ProductVariantTests(PostgresFixture fx) : IAsyncLifetime
{
    public Task InitializeAsync() => fx.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<(Guid cup, Guid pot)> SeedTeaSizes()
    {
        await using var db = fx.NewTenantContext();
        var cup = new ProductVariant { ProductId = fx.LionLagerId, Code = "CUP", Name = "Cup", Price = 150m, SortOrder = 0 };
        var pot = new ProductVariant { ProductId = fx.LionLagerId, Code = "POT", Name = "Pot", Price = 400m, SortOrder = 1 };
        db.ProductVariants.AddRange(cup, pot);
        await db.SaveChangesAsync();
        return (cup.Id, pot.Id);
    }

    [Fact]
    public async Task Variant_price_replaces_base_price_on_the_line()
    {
        var (_, pot) = await SeedTeaSizes();
        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "1", 1, null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.LionLagerId, 2, "bar", null, null, pot), default);

        await using var db = fx.NewTenantContext();
        var line = await db.OrderItems.IgnoreQueryFilters().FirstAsync(i => i.OrderId == o.Id);
        line.UnitPrice.Should().Be(400m);          // pot price, not the product base price
        line.LineSubtotal.Should().Be(800m);        // 400 × 2
        line.VariantName.Should().Be("Pot");
        line.VariantId.Should().Be(pot);
    }

    [Fact]
    public async Task Variant_combines_with_modifier_deltas()
    {
        var (cup, _) = await SeedTeaSizes();
        // attach a +50 modifier to the same product (via the service, like ModifierTests)
        var mod = new ModifierService(fx.Factory());
        var grp = await mod.SetGroupAsync(new SetModifierGroupInput(null, "Extras", 0, 1, false, 0,
            new() { new ModifierItemInput("Extra milk", 50m) }), default);
        await mod.SetProductGroupsAsync(fx.LionLagerId, new() { grp.Id }, default);
        var milkId = grp.Items.First().Id;

        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "2", 1, null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.LionLagerId, 1, "bar", null, new() { milkId }, cup), default);

        await using var db2 = fx.NewTenantContext();
        var line = await db2.OrderItems.IgnoreQueryFilters().FirstAsync(i => i.OrderId == o.Id);
        line.UnitPrice.Should().Be(200m);           // 150 cup + 50 milk
    }

    [Fact]
    public async Task Invalid_variant_for_product_is_rejected()
    {
        await SeedTeaSizes();
        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "3", 1, null), default);

        var act = async () => await orders.AddItemAsync(
            o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null, null, Guid.NewGuid()), default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*serving size*");
    }

    [Fact]
    public async Task Kot_line_shows_the_chosen_size()
    {
        var (cup, _) = await SeedTeaSizes();
        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "4", 1, null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.LionLagerId, 1, "bar", null, null, cup), default);
        await orders.ConfirmAsync(o.Id, default);

        await using var db = fx.NewTenantContext();
        var ticket = await db.KitchenTickets.IgnoreQueryFilters().FirstAsync(t => t.OrderId == o.Id);
        ticket.ItemsJson.Should().Contain("(Cup)");
    }

    // ── endpoint replace-set + RBAC ───────────────────────────────────────────

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
    public async Task Manager_replace_set_creates_updates_and_drops_sizes()
    {
        var mgr = Client("Manager");

        // create two sizes
        var put1 = await mgr.PutAsJsonAsync($"/api/v1/products/{fx.LionLagerId}/variants", new
        {
            variants = new[]
            {
                new { code = "S", name = "Small", price = 100m },
                new { code = "L", name = "Large", price = 200m },
            },
        });
        put1.StatusCode.Should().Be(HttpStatusCode.OK);
        var made = await put1.Content.ReadFromJsonAsync<JsonElement>();
        made.GetArrayLength().Should().Be(2);
        var largeId = made.EnumerateArray().First(v => v.GetProperty("code").GetString() == "L").GetProperty("id").GetString();

        // replace-set: keep Large (by id, repriced), drop Small, add Medium
        var put2 = await mgr.PutAsJsonAsync($"/api/v1/products/{fx.LionLagerId}/variants", new
        {
            variants = new object[]
            {
                new { id = largeId, code = "L", name = "Large", price = 250m },
                new { code = "M", name = "Medium", price = 175m },
            },
        });
        put2.StatusCode.Should().Be(HttpStatusCode.OK);

        var list = await mgr.GetFromJsonAsync<JsonElement>($"/api/v1/products/{fx.LionLagerId}/variants");
        var codes = list.EnumerateArray().Select(v => v.GetProperty("code").GetString()).ToList();
        codes.Should().BeEquivalentTo(new[] { "L", "M" });   // Small dropped
        list.EnumerateArray().First(v => v.GetProperty("code").GetString() == "L")
            .GetProperty("price").GetDecimal().Should().Be(250m);

        // the product list should now report a variant count
        var products = await mgr.GetFromJsonAsync<JsonElement>("/api/v1/products");
        var lion = products.EnumerateArray().First(p => p.GetProperty("id").GetString() == fx.LionLagerId.ToString());
        lion.GetProperty("variantCount").GetInt32().Should().Be(2);
    }

    [Fact]
    public async Task Cashier_cannot_edit_sizes()
    {
        var cashier = Client("Cashier");
        var put = await cashier.PutAsJsonAsync($"/api/v1/products/{fx.LionLagerId}/variants", new
        {
            variants = new[] { new { code = "S", name = "Small", price = 1m } },
        });
        put.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
