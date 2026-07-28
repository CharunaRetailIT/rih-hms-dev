using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Hms.Api.Features.Orders;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Hms.Api.IntegrationTests;

/// <summary>
/// Reports over HTTP with explicit ?from=&amp;to= query dates — the exact path the
/// Reports screen uses. These bind as DateTime Kind=Unspecified, which Npgsql
/// rejects against timestamptz; the endpoints used to 500. (Service-level tests
/// passed UTC-kind dates and never caught it.)
/// </summary>
[Collection("pg")]
public class ReportEndpointTests(PostgresFixture fx) : IAsyncLifetime
{
    public Task InitializeAsync() => fx.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private HttpClient Client()
    {
        var host = new HmsAuthTestFactory(fx.ControlConn, fx.TenantTemplate);
        var c = host.CreateClient();
        var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            "dev-only-signing-key-replace-in-production-with-vault-secret-min-32-chars")), SecurityAlgorithms.HmacSha256);
        var jwt = new JwtSecurityToken(
            issuer: "https://localhost:5001", audience: "rit-hms-api",
            claims: new[] { new Claim("tenant_id", fx.TenantId.ToString()), new Claim("role", "Owner") },
            expires: DateTime.UtcNow.AddHours(1), signingCredentials: creds);
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", new JwtSecurityTokenHandler().WriteToken(jwt));
        return c;
    }

    [Theory]
    [InlineData("/api/v1/reports/vat/summary")]
    [InlineData("/api/v1/reports/sales/summary")]
    [InlineData("/api/v1/reports/outlets/summary")]
    public async Task Report_endpoint_accepts_explicit_date_range(string path)
    {
        var client = Client();
        var res = await client.GetAsync($"{path}?from=2026-06-01&to=2026-06-30");
        res.StatusCode.Should().Be(HttpStatusCode.OK, $"{path} must accept query-string dates without a 500");
    }

    [Fact]
    public async Task Report_endpoint_works_without_a_range_too()
    {
        var client = Client();
        (await client.GetAsync("/api/v1/reports/sales/summary")).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── #72 food costing + bin card ────────────────────────────────────────────
    [Fact]
    public async Task Food_costing_endpoint_returns_ok()
    {
        var res = await Client().GetAsync("/api/v1/reports/food-costing");
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("items").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    public async Task Budget_vs_sales_compares_target_to_actual()
    {
        var c = Client();
        var month = DateTime.UtcNow.ToString("yyyy-MM-01");
        // A per-outlet target …
        (await c.PutAsJsonAsync("/api/v1/budgets", new { locationId = fx.LocationId, month, amount = 100000 }))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        // … and a company-wide (null location) target.
        (await c.PutAsJsonAsync("/api/v1/budgets", new { locationId = (Guid?)null, month, amount = 250000 }))
            .StatusCode.Should().Be(HttpStatusCode.OK);

        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "BV", 1, null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);
        var total = (await orders.GetAsync(o.Id, default))!.TotalAmount;
        await orders.SettleAsync(o.Id, new SettleInput(new() { new PaymentInput("cash", total, null) }), default);

        // Per-outlet view → that outlet's target.
        var outletBody = await (await c.GetAsync($"/api/v1/reports/budget-vs-sales?locationId={fx.LocationId}")).Content.ReadFromJsonAsync<JsonElement>();
        outletBody.GetProperty("rows").EnumerateArray()
            .Should().Contain(x => x.GetProperty("budget").GetDecimal() == 100000m && x.GetProperty("actual").GetDecimal() > 0);

        // All-outlets view → the company-wide target.
        var groupBody = await (await c.GetAsync("/api/v1/reports/budget-vs-sales")).Content.ReadFromJsonAsync<JsonElement>();
        groupBody.GetProperty("rows").EnumerateArray()
            .Should().Contain(x => x.GetProperty("budget").GetDecimal() == 250000m && x.GetProperty("actual").GetDecimal() > 0);
    }

    [Fact]
    public async Task Bin_card_shows_a_sale_movement_with_running_balance()
    {
        // Settle a sale of 2 Chicken Kottu (stocked) → a −2 movement on the bin card.
        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "BC1", 1, null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 2, "kitchen", null), default);
        var total = (await orders.GetAsync(o.Id, default))!.TotalAmount;
        await orders.SettleAsync(o.Id, new SettleInput(new() { new PaymentInput("cash", total, null) }), default);

        var res = await Client().GetAsync($"/api/v1/reports/bin-card?productId={fx.ChickenKottuId}&locationId={fx.LocationId}");
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();

        var lines = body.GetProperty("lines").EnumerateArray().ToList();
        lines.Should().Contain(l => l.GetProperty("type").GetString() == "Sale" && l.GetProperty("qty").GetDecimal() == -2m);
        // closing balance reconciles to current on-hand (started at 50, sold 2)
        body.GetProperty("closing").GetDecimal().Should().Be(48m);
    }
}
