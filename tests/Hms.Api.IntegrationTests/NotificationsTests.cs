using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Hms.Api.IntegrationTests;

/// <summary>
/// In-app notification feed (topbar bell). Verifies the aggregation queries
/// translate and run over HTTP — the low-stock correlated-subquery in
/// particular — and that a product at/below its reorder level surfaces.
/// </summary>
[Collection("pg")]
public class NotificationsTests(PostgresFixture fx) : IAsyncLifetime
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

    private sealed record Notif(string type, string severity, string title, string detail, string href);
    private sealed record Feed(int count, List<Notif> items);

    [Fact]
    public async Task Feed_returns_ok_and_a_well_formed_shape()
    {
        var res = await Client().GetAsync("/api/v1/notifications");
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var feed = await res.Content.ReadFromJsonAsync<Feed>(new JsonSerializerOptions(JsonSerializerDefaults.Web));
        feed.Should().NotBeNull();
        feed!.count.Should().Be(feed.items.Count, "the badge count is the number of items");
    }

    [Fact]
    public async Task A_product_at_or_below_its_reorder_level_surfaces_as_low_stock()
    {
        // Chicken Kottu is seeded with 50 on hand; set its reorder level above that.
        await using (var db = fx.NewTenantContext())
        {
            var p = await db.Products.FirstAsync(x => x.Id == fx.ChickenKottuId);
            p.ReorderLevel = 100m;
            await db.SaveChangesAsync();
        }

        var res = await Client().GetAsync("/api/v1/notifications");
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var feed = (await res.Content.ReadFromJsonAsync<Feed>(new JsonSerializerOptions(JsonSerializerDefaults.Web)))!;

        feed.items.Should().Contain(n => n.type == "low_stock" && n.title.Contains("Chicken Kottu"));
        feed.items.First(n => n.type == "low_stock").href.Should().Be("/inventory");
    }
}
