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
/// #55d per-product tax class. The bill-level engine (SVC → SSCL → VAT) now
/// applies VAT-type charges to the *taxable* subtotal only, so zero-rated /
/// exempt products drop out of the VAT base. All-standard baskets are unchanged.
/// </summary>
[Collection("pg")]
public class ProductTaxClassTests(PostgresFixture fx) : IAsyncLifetime
{
    public Task InitializeAsync() => fx.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Exempt_line_drops_out_of_the_vat_base()
    {
        // Mark Lion Lager exempt; Chicken Kottu stays standard.
        await using (var db = fx.NewTenantContext())
        {
            var lion = await db.Products.IgnoreQueryFilters().FirstAsync(p => p.Id == fx.LionLagerId);
            lion.TaxClass = "exempt"; lion.IsTaxable = false;
            await db.SaveChangesAsync();
        }

        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "1", 1, null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);   // 900 standard
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.LionLagerId, 1, "bar", null), default);           // 450 exempt

        var got = (await orders.GetAsync(o.Id, default))!;
        got.SubtotalAmount.Should().Be(1350m);
        got.ServiceChargeAmount.Should().Be(135m);   // SVC 10% on full subtotal (1,350)
        // VAT base = 900 (Kottu only). SVC folds into the taxable running total:
        //   SSCL 2.5% of (900+135)=1,035 → 25.875
        //   VAT 18% of (1,035+25.875)=1,060.875 → 190.9575
        got.TaxAmount.Should().Be(25.875m + 190.9575m);
        got.TotalAmount.Should().Be(1350m + 135m + 25.875m + 190.9575m);

        // line snapshot reflects taxability
        var lines = got.Items;
        lines.Single(i => i.ProductId == fx.LionLagerId).IsTaxable.Should().BeFalse();
        lines.Single(i => i.ProductId == fx.ChickenKottuId).IsTaxable.Should().BeTrue();
    }

    [Fact]
    public async Task All_standard_basket_is_unchanged()
    {
        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "2", 1, null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 10, "kitchen", null), default);

        var got = (await orders.GetAsync(o.Id, default))!;
        got.SubtotalAmount.Should().Be(9000m);
        got.ServiceChargeAmount.Should().Be(900m);
        got.TaxAmount.Should().Be(247.50m + 1826.55m);    // identical to the pre-#55d engine
        got.TotalAmount.Should().Be(11974.05m);
    }

    // ── endpoint behaviour ────────────────────────────────────────────────────

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
    public async Task Setting_tax_class_keeps_is_taxable_in_sync()
    {
        var mgr = Client("Manager");
        var put = await mgr.PutAsJsonAsync($"/api/v1/products/{fx.ChickenKottuId}",
            new { taxClass = "zero_rated" });
        put.StatusCode.Should().Be(HttpStatusCode.OK);

        var products = await mgr.GetFromJsonAsync<JsonElement>("/api/v1/products");
        var kottu = products.EnumerateArray().First(p => p.GetProperty("id").GetString() == fx.ChickenKottuId.ToString());
        kottu.GetProperty("taxClass").GetString().Should().Be("zero_rated");
        kottu.GetProperty("isTaxable").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task Unknown_tax_class_falls_back_to_standard()
    {
        var mgr = Client("Manager");
        await mgr.PutAsJsonAsync($"/api/v1/products/{fx.LionLagerId}", new { taxClass = "nonsense" });

        var products = await mgr.GetFromJsonAsync<JsonElement>("/api/v1/products");
        var lion = products.EnumerateArray().First(p => p.GetProperty("id").GetString() == fx.LionLagerId.ToString());
        lion.GetProperty("taxClass").GetString().Should().Be("standard");
        lion.GetProperty("isTaxable").GetBoolean().Should().BeTrue();
    }
}
