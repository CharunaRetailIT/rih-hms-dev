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
/// #68 tables/floor + reservations. A table is "occupied" when an open order
/// references it; reservations have a status lifecycle. Exercised over HTTP.
/// </summary>
[Collection("pg")]
public class TablesTests(PostgresFixture fx) : IAsyncLifetime
{
    public Task InitializeAsync() => fx.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private HttpClient Client(string role = "Owner")
    {
        var c = new HmsAuthTestFactory(fx.ControlConn, fx.TenantTemplate).CreateClient();
        var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            "dev-only-signing-key-replace-in-production-with-vault-secret-min-32-chars")), SecurityAlgorithms.HmacSha256);
        var jwt = new JwtSecurityToken("https://localhost:5001", "rit-hms-api",
            new[] { new Claim("tenant_id", fx.TenantId.ToString()), new Claim("role", role) },
            expires: DateTime.UtcNow.AddHours(1), signingCredentials: creds);
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", new JwtSecurityTokenHandler().WriteToken(jwt));
        return c;
    }

    [Fact]
    public async Task Table_shows_occupied_when_an_open_order_references_it()
    {
        var c = Client();

        // create a table
        var put = await c.PutAsJsonAsync("/api/v1/tables", new { locationId = fx.LocationId, code = "T1", name = "Window 1", seats = 4, area = "Main" });
        put.StatusCode.Should().Be(HttpStatusCode.OK);
        var table = await put.Content.ReadFromJsonAsync<JsonElement>();
        var tableId = table.GetProperty("id").GetString();

        // status: open
        var st1 = await c.GetFromJsonAsync<JsonElement>($"/api/v1/tables/status?locationId={fx.LocationId}");
        st1.EnumerateArray().First(x => x.GetProperty("id").GetString() == tableId)
            .GetProperty("occupied").GetBoolean().Should().BeFalse();

        // open an order against the table (via the service)
        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "T1", 2, null,
            TableId: Guid.Parse(tableId!)), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);

        // status: occupied + linked to the order
        var st2 = await c.GetFromJsonAsync<JsonElement>($"/api/v1/tables/status?locationId={fx.LocationId}");
        var row = st2.EnumerateArray().First(x => x.GetProperty("id").GetString() == tableId);
        row.GetProperty("occupied").GetBoolean().Should().BeTrue();
        row.GetProperty("orderId").GetString().Should().Be(o.Id.ToString());
        row.GetProperty("total").GetDecimal().Should().BeGreaterThan(0m);
    }

    [Fact]
    public async Task Reservation_lifecycle()
    {
        var c = Client();
        var put = await c.PutAsJsonAsync("/api/v1/reservations", new
        {
            locationId = fx.LocationId, customerName = "Mr Perera", phone = "0771234567",
            partySize = 4, reservedAt = "2026-06-02T19:00:00Z", durationMinutes = 90,
        });
        put.StatusCode.Should().Be(HttpStatusCode.OK);
        var id = (await put.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetString();

        var list = await c.GetFromJsonAsync<JsonElement>($"/api/v1/reservations?locationId={fx.LocationId}&from=2026-06-02&to=2026-06-03");
        list.EnumerateArray().Should().Contain(x => x.GetProperty("customerName").GetString() == "Mr Perera");

        var seat = await c.PostAsJsonAsync($"/api/v1/reservations/{id}/status", new { status = "seated" });
        seat.StatusCode.Should().Be(HttpStatusCode.OK);
        (await seat.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("status").GetString().Should().Be("seated");

        var bad = await c.PostAsJsonAsync($"/api/v1/reservations/{id}/status", new { status = "teleported" });
        bad.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Cashier_cannot_create_tables_but_can_read_the_floor()
    {
        var cashier = Client("Cashier");
        (await cashier.PutAsJsonAsync("/api/v1/tables", new { locationId = fx.LocationId, code = "X1" }))
            .StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await cashier.GetAsync($"/api/v1/tables/status?locationId={fx.LocationId}"))
            .StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Floor_plan_layout_saves_and_reads_back_table_positions()
    {
        var c = Client();
        var table = await (await c.PutAsJsonAsync("/api/v1/tables", new { locationId = fx.LocationId, code = "FP1", seats = 2 })).Content.ReadFromJsonAsync<JsonElement>();
        var id = table.GetProperty("id").GetString();

        (await c.PutAsJsonAsync("/api/v1/tables/layout", new { items = new[] { new { id, posX = 240, posY = 130 } } }))
            .StatusCode.Should().Be(HttpStatusCode.OK);

        var status = await (await c.GetAsync($"/api/v1/tables/status?locationId={fx.LocationId}")).Content.ReadFromJsonAsync<JsonElement>();
        var row = status.EnumerateArray().First(t => t.GetProperty("code").GetString() == "FP1");
        row.GetProperty("posX").GetInt32().Should().Be(240);
        row.GetProperty("posY").GetInt32().Should().Be(130);
    }
}
