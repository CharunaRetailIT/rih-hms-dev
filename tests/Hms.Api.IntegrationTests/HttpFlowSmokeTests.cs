using System.Net;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Hms.Api.IntegrationTests;

/// <summary>
/// HTTP-level "every flow returns 2xx" smoke — drives each mutating flow through
/// the REAL request pipeline (JWT auth, model binding, RLS) the way the UI does.
/// This is the layer that caught the Reports 500: service-level tests can pass
/// while the HTTP boundary (query-string binding, serialization) is broken.
/// </summary>
[Collection("pg")]
public class HttpFlowSmokeTests(PostgresFixture fx) : IAsyncLifetime
{
    public Task InitializeAsync() => fx.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private HttpClient Client(string role = "Owner")
    {
        var host = new HmsAuthTestFactory(fx.ControlConn, fx.TenantTemplate);
        var c = host.CreateClient();
        var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            "dev-only-signing-key-replace-in-production-with-vault-secret-min-32-chars")), SecurityAlgorithms.HmacSha256);
        var jwt = new JwtSecurityToken("https://localhost:5001", "rit-hms-api",
            new[] { new Claim("tenant_id", fx.TenantId.ToString()), new Claim("role", role) },
            expires: DateTime.UtcNow.AddHours(1), signingCredentials: creds);
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", new JwtSecurityTokenHandler().WriteToken(jwt));
        return c;
    }

    private static async Task<JsonElement> Ok(HttpResponseMessage r, string what)
    {
        var body = await r.Content.ReadAsStringAsync();
        r.IsSuccessStatusCode.Should().BeTrue($"{what} should succeed but got {(int)r.StatusCode}: {body}");
        return string.IsNullOrWhiteSpace(body) ? default : JsonDocument.Parse(body).RootElement;
    }

    private static StringContent Json(object o) =>
        new(JsonSerializer.Serialize(o), Encoding.UTF8, "application/json");

    [Fact]
    public async Task Shift_open_current_close()
    {
        var c = Client();
        var open = await Ok(await c.PostAsync("/api/v1/shifts/open", Json(new { locationId = fx.LocationId, openingFloat = 5000m })), "shift open");
        var id = open.GetProperty("id").GetString();
        await Ok(await c.GetAsync($"/api/v1/shifts/current?locationId={fx.LocationId}"), "shift current");
        await Ok(await c.PostAsync($"/api/v1/shifts/{id}/close", Json(new { declaredCash = 5000m, notes = (string?)null })), "shift close");
    }

    [Fact]
    public async Task Order_discount_then_void()
    {
        var c = Client();
        var o = await Ok(await c.PostAsync("/api/v1/orders", Json(new { locationId = fx.LocationId, orderType = "dine_in", tableLabel = "7", covers = 2 })), "create order");
        var id = o.GetProperty("id").GetString();
        await Ok(await c.PostAsync($"/api/v1/orders/{id}/items", Json(new { productId = fx.ChickenKottuId, quantity = 2, station = "kitchen" })), "add item");
        await Ok(await c.PostAsync($"/api/v1/orders/{id}/discount", Json(new { amount = 100m })), "discount");
        await Ok(await c.PostAsync($"/api/v1/orders/{id}/void", Json(new { reason = "smoke" })), "void");
    }

    [Fact]
    public async Task Editing_a_lines_qty_keeps_the_item_order_stable()
    {
        // The POS list must not reshuffle when one line is edited (an unordered
        // Include returns a DB order that shifts after UPDATE → the row jumps and
        // the next tap lands on the wrong line). ToDto orders by CreatedAt, Id.
        var c = Client();
        var o = await Ok(await c.PostAsync("/api/v1/orders", Json(new { locationId = fx.LocationId, orderType = "dine_in", tableLabel = "9", covers = 2 })), "create order");
        var id = o.GetProperty("id").GetString();
        // three distinct lines (each add appends)
        for (var n = 0; n < 3; n++)
            await Ok(await c.PostAsync($"/api/v1/orders/{id}/items", Json(new { productId = fx.ChickenKottuId, quantity = 1, station = "kitchen" })), "add item");

        var before = await Ok(await c.GetAsync($"/api/v1/orders/{id}"), "get order");
        var ids = before.GetProperty("items").EnumerateArray().Select(i => i.GetProperty("id").GetString()).ToList();
        ids.Should().HaveCount(3);
        var middleId = ids[1];

        // bump the middle line's qty several times — order must be identical each time
        for (var q = 2; q <= 5; q++)
        {
            var updated = await Ok(await c.PutAsync($"/api/v1/orders/{id}/items/{middleId}", Json(new { quantity = q })), "update qty");
            var nowIds = updated.GetProperty("items").EnumerateArray().Select(i => i.GetProperty("id").GetString()).ToList();
            nowIds.Should().Equal(ids, "line order must stay stable across edits");
            updated.GetProperty("items").EnumerateArray().First(i => i.GetProperty("id").GetString() == middleId)
                .GetProperty("quantity").GetDecimal().Should().Be(q);
        }
    }

    [Fact]
    public async Task Discount_and_void_respect_the_role_permission_matrix()
    {
        // #71: owner tightens Cashier — max 10% discount, no voids.
        var owner = Client("Owner");
        await Ok(await owner.PutAsync("/api/v1/permissions", Json(new
        { role = 2, maxDiscountPercent = 10m, canApplyDiscount = true, canVoid = false, canComp = true })), "set cashier perms");

        var cashier = Client("Cashier");
        var o = await Ok(await cashier.PostAsync("/api/v1/orders", Json(new { locationId = fx.LocationId, orderType = "dine_in", tableLabel = "P1", covers = 1 })), "create");
        var id = o.GetProperty("id").GetString();
        await Ok(await cashier.PostAsync($"/api/v1/orders/{id}/items", Json(new { productId = fx.ChickenKottuId, quantity = 1, station = "kitchen" })), "add item"); // 900 subtotal

        // within the 10% cap → allowed
        (await cashier.PostAsync($"/api/v1/orders/{id}/discount", Json(new { percent = 5m }))).StatusCode.Should().Be(HttpStatusCode.OK);
        // over the cap (percent) → blocked
        (await cashier.PostAsync($"/api/v1/orders/{id}/discount", Json(new { percent = 25m }))).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        // over the cap (a fixed amount that's >10% of the 900 subtotal) → blocked
        (await cashier.PostAsync($"/api/v1/orders/{id}/discount", Json(new { amount = 200m }))).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        // cashier can't void
        (await cashier.PostAsync($"/api/v1/orders/{id}/void", Json(new { reason = "x" }))).StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // owner bypasses both limits
        (await owner.PostAsync($"/api/v1/orders/{id}/discount", Json(new { percent = 50m }))).StatusCode.Should().Be(HttpStatusCode.OK);
        (await owner.PostAsync($"/api/v1/orders/{id}/void", Json(new { reason = "ok" }))).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task A_manager_pin_authorises_an_over_limit_discount_or_void()
    {
        var owner = Client("Owner");
        // Cashier capped at 10% / no void; a manager with a PIN can override.
        await Ok(await owner.PutAsync("/api/v1/permissions", Json(new { role = 2, maxDiscountPercent = 10m, canApplyDiscount = true, canVoid = false, canComp = true })), "cap cashier");
        var mgr = await Ok(await owner.PostAsync("/api/v1/users", Json(new { displayName = "Floor Manager", username = "floormgr", role = 1, pin = "778899" })), "manager + pin");
        mgr.GetProperty("hasPin").GetBoolean().Should().BeTrue();

        var cashier = Client("Cashier");
        var o = await Ok(await cashier.PostAsync("/api/v1/orders", Json(new { locationId = fx.LocationId, orderType = "dine_in", tableLabel = "M1", covers = 1 })), "create");
        var id = o.GetProperty("id").GetString();
        await Ok(await cashier.PostAsync($"/api/v1/orders/{id}/items", Json(new { productId = fx.ChickenKottuId, quantity = 1, station = "kitchen" })), "item");

        // 25% blocked without a PIN…
        (await cashier.PostAsync($"/api/v1/orders/{id}/discount", Json(new { percent = 25m }))).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        // …allowed with the manager PIN.
        (await cashier.PostAsync($"/api/v1/orders/{id}/discount", Json(new { percent = 25m, managerPin = "778899" }))).StatusCode.Should().Be(HttpStatusCode.OK);
        // a wrong PIN is still refused.
        (await cashier.PostAsync($"/api/v1/orders/{id}/void", Json(new { reason = "x", managerPin = "000000" }))).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        // the manager PIN authorises the void.
        (await cashier.PostAsync($"/api/v1/orders/{id}/void", Json(new { reason = "x", managerPin = "778899" }))).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Order_custom_item_then_confirm_and_kot_status()
    {
        var c = Client();
        var o = await Ok(await c.PostAsync("/api/v1/orders", Json(new { locationId = fx.LocationId, orderType = "dine_in", tableLabel = "8", covers = 1 })), "create order");
        var id = o.GetProperty("id").GetString();
        await Ok(await c.PostAsync($"/api/v1/orders/{id}/custom-item", Json(new { name = "Special platter", unitPrice = 1200m, quantity = 1, station = "kitchen" })), "custom item");
        await Ok(await c.PostAsync($"/api/v1/orders/{id}/confirm", Json(new { })), "confirm");

        var tickets = await Ok(await c.GetAsync("/api/v1/kitchen/tickets"), "list tickets");
        var ticketId = tickets.EnumerateArray().First().GetProperty("id").GetString();
        await Ok(await c.PostAsync($"/api/v1/kitchen/tickets/{ticketId}/status", Json(new { status = "preparing" })), "kot status");
    }

    [Fact]
    public async Task Sensitive_actions_are_written_to_the_audit_log()
    {
        var c = Client();   // Owner
        var o = await Ok(await c.PostAsync("/api/v1/orders", Json(new { locationId = fx.LocationId, orderType = "dine_in", tableLabel = "A1", covers = 1 })), "create");
        var id = o.GetProperty("id").GetString();
        await Ok(await c.PostAsync($"/api/v1/orders/{id}/items", Json(new { productId = fx.ChickenKottuId, quantity = 1, station = "kitchen" })), "add");
        await Ok(await c.PostAsync($"/api/v1/orders/{id}/discount", Json(new { percent = 10m })), "discount");
        await Ok(await c.PostAsync($"/api/v1/orders/{id}/void", Json(new { reason = "audit test" })), "void");

        var log = await Ok(await c.GetAsync("/api/v1/audit"), "audit list");
        var actions = log.EnumerateArray().Select(e => e.GetProperty("action").GetString()).ToList();
        actions.Should().Contain("order.discount");
        actions.Should().Contain("order.void");
        // entries carry the actor's role
        log.EnumerateArray().First().GetProperty("actorRole").GetString().Should().Be("Owner");
    }

    [Fact]
    public async Task Stale_token_for_a_missing_tenant_returns_401_not_500()
    {
        // A validly-signed JWT whose tenant_id no longer exists (e.g. after a DB
        // reset) must 401 (re-authenticate), not crash with a 500.
        var host = new HmsAuthTestFactory(fx.ControlConn, fx.TenantTemplate);
        var c = host.CreateClient();
        var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            "dev-only-signing-key-replace-in-production-with-vault-secret-min-32-chars")), SecurityAlgorithms.HmacSha256);
        var jwt = new JwtSecurityToken("https://localhost:5001", "rit-hms-api",
            new[] { new Claim("tenant_id", Guid.NewGuid().ToString()), new Claim("role", "Owner") },
            expires: DateTime.UtcNow.AddHours(1), signingCredentials: creds);
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", new JwtSecurityTokenHandler().WriteToken(jwt));

        (await c.GetAsync("/api/v1/products")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Staff_pin_login_works_then_locks_out_after_failures()
    {
        var owner = Client("Owner");
        // Emailless POS staff with a username + PIN.
        var created = await Ok(await owner.PostAsync("/api/v1/users", Json(new { displayName = "PIN Cashier", username = "asela", role = 2, pin = "246810" })), "create pin user");
        created.GetProperty("hasPin").GetBoolean().Should().BeTrue();

        // Correct username + PIN → access token (no public roster).
        var ok = await Ok(await owner.PostAsync("/api/v1/auth/pin", Json(new { tenantSlug = "it", username = "asela", pin = "246810" })), "pin login");
        ok.GetProperty("accessToken").GetString().Should().NotBeNullOrEmpty();

        // Wrong PIN → 401.
        (await owner.PostAsync("/api/v1/auth/pin", Json(new { tenantSlug = "it", username = "asela", pin = "000000" }))).StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // Five misses → locked (423), even with the right PIN.
        for (var i = 0; i < 5; i++)
            await owner.PostAsync("/api/v1/auth/pin", Json(new { tenantSlug = "it", username = "asela", pin = "111111" }));
        (await owner.PostAsync("/api/v1/auth/pin", Json(new { tenantSlug = "it", username = "asela", pin = "246810" }))).StatusCode.Should().Be(HttpStatusCode.Locked);
    }

    [Fact]
    public async Task Reports_library_endpoints_respond()
    {
        var c = Client();
        // settle a bill so the reports have data to aggregate
        var o = await Ok(await c.PostAsync("/api/v1/orders", Json(new { locationId = fx.LocationId, orderType = "dine_in", tableLabel = "R1", covers = 1 })), "create");
        var id = o.GetProperty("id").GetString();
        await Ok(await c.PostAsync($"/api/v1/orders/{id}/items", Json(new { productId = fx.ChickenKottuId, quantity = 2, station = "kitchen" })), "add item");
        var total = (await Ok(await c.GetAsync($"/api/v1/orders/{id}"), "get")).GetProperty("totalAmount").GetDecimal();
        await Ok(await c.PostAsync($"/api/v1/orders/{id}/settle", Json(new { payments = new[] { new { payType = "cash", amount = total } } })), "settle");

        await Ok(await c.GetAsync("/api/v1/reports/sales/register"), "sales register");
        await Ok(await c.GetAsync("/api/v1/reports/sales/items"), "item sales");
        await Ok(await c.GetAsync("/api/v1/reports/stock/balance"), "stock balance");
        await Ok(await c.GetAsync("/api/v1/reports/shifts"), "shift settlement");
        await Ok(await c.GetAsync("/api/v1/reports/promotions"), "promotion usage");
        // explicit date range + location filter — the query-string→timestamptz binding
        // that previously regressed into a 500.
        await Ok(await c.GetAsync($"/api/v1/reports/sales/register?from=2026-01-01&to=2026-12-31&locationId={fx.LocationId}"), "register (ranged)");
    }

    [Fact]
    public async Task Stock_count_posts_variance_to_on_hand()
    {
        var c = Client();
        var created = await Ok(await c.PostAsync("/api/v1/stock-counts", Json(new { locationId = fx.LocationId, notes = "nightly" })), "create count");
        var id = created.GetProperty("id").GetString();
        // fixture seeds 50 on hand; count Rice at 42 → variance −8
        await Ok(await c.PutAsync($"/api/v1/stock-counts/{id}/lines", Json(new { lines = new[] { new { productId = fx.RiceId, countedQty = 42m } } })), "save lines");
        var posted = await Ok(await c.PostAsync($"/api/v1/stock-counts/{id}/post", Json(new { })), "post");
        posted.GetProperty("varianceLines").GetInt32().Should().BeGreaterThan(0);

        await using var db = fx.NewTenantContext();
        var rice = await db.ProductStocks.FirstAsync(s => s.ProductId == fx.RiceId && s.LocationId == fx.LocationId);
        rice.QuantityOnHand.Should().Be(42m, "the physical count overwrites system on-hand");
    }

    [Fact]
    public async Task Procurement_po_then_grn_receive()
    {
        var c = Client();
        var sup = await Ok(await c.PostAsync("/api/v1/suppliers", Json(new { code = "SUP1", name = "Smoke Supplier" })), "create supplier");
        var supId = sup.GetProperty("id").GetString();
        var po = await Ok(await c.PostAsync("/api/v1/purchase-orders", Json(new
        {
            locationId = fx.LocationId,
            supplierId = supId,
            lines = new[] { new { productId = fx.RiceId, quantity = 10m, unitCost = 180m } },
        })), "create PO");
        var poId = po.GetProperty("id").GetString();
        await Ok(await c.PostAsync("/api/v1/grn", Json(new
        {
            locationId = fx.LocationId,
            supplierId = supId,
            purchaseOrderId = poId,
            lines = new[] { new { productId = fx.RiceId, quantity = 10m, unitCost = 180m } },
        })), "receive GRN");
    }

    [Fact]
    public async Task Production_recipe_produce_then_void()
    {
        var c = Client();
        // set a recipe (Chicken Kottu uses 0.2 kg rice), then produce against it
        await Ok(await c.PutAsync("/api/v1/recipes", Json(new
        {
            productId = fx.ChickenKottuId, yieldQuantity = 1m, notes = (string?)null,
            lines = new[] { new { ingredientProductId = fx.RiceId, quantity = 0.2m, unitId = (Guid?)null } },
        })), "set recipe");
        var prod = await Ok(await c.PostAsync("/api/v1/production", Json(new
        {
            locationId = fx.LocationId, productId = fx.ChickenKottuId, quantity = 3m, notes = (string?)null, post = true,
        })), "produce (recipe)");
        var id = prod.GetProperty("id").GetString();
        await Ok(await c.PostAsync($"/api/v1/production/{id}/void", Json(new { })), "void production");
    }

    [Fact]
    public async Task Production_custom_adhoc()
    {
        var c = Client();
        await Ok(await c.PostAsync("/api/v1/production/custom", Json(new
        {
            locationId = fx.LocationId, productId = fx.ChickenKottuId, quantity = 2m,
            lines = new[] { new { ingredientProductId = fx.RiceId, quantity = 0.5m, unitId = (Guid?)null } },
            post = true,
        })), "ad-hoc custom production");
    }

    [Fact]
    public async Task Transfer_dispatch_then_receive()
    {
        var c = Client();
        var t = await Ok(await c.PostAsync("/api/v1/transfers", Json(new
        {
            fromLocationId = fx.LocationId, toLocationId = fx.CentralKitchenId, isReturn = false,
            lines = new[] { new { productId = fx.RiceId, quantity = 5m } },
        })), "create transfer");
        var id = t.GetProperty("id").GetString();
        await Ok(await c.PostAsync($"/api/v1/transfers/{id}/dispatch", Json(new { })), "dispatch transfer");
        await Ok(await c.PostAsync($"/api/v1/transfers/{id}/receive", Json(new { })), "receive transfer");
    }

    [Fact]
    public async Task Aggregator_simulate_then_accept()
    {
        var c = Client();
        await Ok(await c.PostAsync("/api/v1/aggregator/pickme/simulate", Json(new { locationId = fx.LocationId })), "simulate aggregator order");
        var orders = await Ok(await c.GetAsync("/api/v1/aggregator/orders"), "list aggregator orders");
        var first = orders.EnumerateArray().FirstOrDefault();
        first.ValueKind.Should().NotBe(JsonValueKind.Undefined, "simulate should create an aggregator order");
        var id = first.GetProperty("id").GetString();
        await Ok(await c.PostAsync($"/api/v1/aggregator/orders/{id}/accept", Json(new { prepMinutes = 20 })), "accept aggregator order");
    }

    [Fact]
    public async Task Wastage_and_adjustment()
    {
        var c = Client();
        await Ok(await c.PostAsync("/api/v1/wastage", Json(new
        {
            locationId = fx.LocationId, reason = "spoilage",
            lines = new[] { new { productId = fx.RiceId, quantity = 1m } },
        })), "wastage");
        await Ok(await c.PostAsync("/api/v1/adjustments", Json(new
        {
            locationId = fx.LocationId, reason = "stock count",
            lines = new[] { new { productId = fx.RiceId, quantityDelta = 2m } },
        })), "adjustment");
    }

    [Fact]
    public async Task Team_and_settings()
    {
        var c = Client();
        await Ok(await c.GetAsync("/api/v1/users"), "list users");
        await Ok(await c.PostAsync("/api/v1/users", Json(new { email = "smoke@demo.local", displayName = "Smoke Tester", role = 2 })), "create user");
        await Ok(await c.GetAsync("/api/v1/settings"), "get settings");
        await Ok(await c.PutAsync("/api/v1/settings", Json(new { legalName = "Smoke Restaurant (Pvt) Ltd" })), "put settings");
    }

    [Fact]
    public async Task Print_job_queue_enqueues_polls_and_acks()
    {
        var c = Client();
        var loc = (await Ok(await c.GetAsync("/api/v1/locations"), "loc")).EnumerateArray().First().GetProperty("id").GetString();
        var job = await Ok(await c.PostAsync("/api/v1/print/jobs", Json(new { locationId = loc, kind = "bill", payload = "<html>bill</html>" })), "enqueue");
        var id = job.GetProperty("id").GetString();

        var queued = await Ok(await c.GetAsync($"/api/v1/print/jobs?locationId={loc}"), "poll");
        queued.EnumerateArray().ToList().Should().Contain(x => x.GetProperty("id").GetString() == id);

        await Ok(await c.PostAsync($"/api/v1/print/jobs/{id}/ack", Json(new { status = "printed" })), "ack");
        var after = await Ok(await c.GetAsync($"/api/v1/print/jobs?locationId={loc}"), "poll2");
        after.EnumerateArray().ToList().Should().NotContain(x => x.GetProperty("id").GetString() == id);   // printed → off the queue
    }

    [Fact]
    public async Task Function_level_screen_access_hides_a_screen_from_a_role()
    {
        await Ok(await Client().PutAsync("/api/v1/permissions/screens", Json(new { role = 2, screen = "/reports", allowed = false })), "deny reports for cashier");
        var me = await Ok(await Client("Cashier").GetAsync("/api/v1/permissions/screens/me"), "my screens");
        me.GetProperty("denied").EnumerateArray().Select(x => x.GetString()).Should().Contain("/reports");
    }

    [Fact]
    public async Task Settled_bill_can_be_recalled_and_reprinted_with_a_copy_count()
    {
        var c = Client();
        // a settled bill (shift gate is off in tests)
        var loc = (await Ok(await c.GetAsync("/api/v1/locations"), "loc")).EnumerateArray().First().GetProperty("id").GetString();
        var prod = (await Ok(await c.GetAsync("/api/v1/products?activeOnly=true"), "prods")).EnumerateArray().First().GetProperty("id").GetString();
        var o = await Ok(await c.PostAsync("/api/v1/orders", Json(new { locationId = loc, orderType = "dine_in", tableLabel = "RC1", covers = 1 })), "order");
        var oid = o.GetProperty("id").GetString();
        await Ok(await c.PostAsync($"/api/v1/orders/{oid}/items", Json(new { productId = prod, quantity = 1, station = "kitchen" })), "item");
        var total = (await Ok(await c.GetAsync($"/api/v1/orders/{oid}"), "get")).GetProperty("totalAmount").GetDecimal();
        await Ok(await c.PostAsync($"/api/v1/orders/{oid}/settle", Json(new { payments = new[] { new { payType = "cash", amount = total } } })), "settle");

        // recall lists it among recent settled bills
        var settled = await Ok(await c.GetAsync("/api/v1/orders/settled"), "settled");
        settled.EnumerateArray().ToList().Should().Contain(x => x.GetProperty("id").GetString() == oid);

        // reprint bumps the copy count
        (await Ok(await c.PostAsync($"/api/v1/orders/{oid}/reprint", Json(new { })), "reprint")).GetProperty("reprintCount").GetInt32().Should().Be(1);
        (await Ok(await c.PostAsync($"/api/v1/orders/{oid}/reprint", Json(new { })), "reprint2")).GetProperty("reprintCount").GetInt32().Should().Be(2);
        (await Ok(await c.GetAsync($"/api/v1/orders/{oid}/invoice"), "invoice")).GetProperty("reprintCount").GetInt32().Should().Be(2);
    }

    [Fact]
    public async Task Outlet_can_be_added_with_a_type_and_capabilities()
    {
        var c = Client();
        await Ok(await c.PutAsync("/api/v1/locations", Json(new { code = "CK1", name = "Central Kitchen", city = "Colombo", locationType = "central_kitchen", canSell = false, canProduce = true })), "add outlet");
        var list = await Ok(await c.GetAsync("/api/v1/locations?all=true"), "locations");
        list.EnumerateArray().ToList().Should().Contain(x =>
            x.GetProperty("code").GetString() == "CK1" &&
            x.GetProperty("locationType").GetString() == "central_kitchen" &&
            x.GetProperty("canProduce").GetBoolean() && !x.GetProperty("canSell").GetBoolean());
    }

    [Fact]
    public async Task PickMe_poll_is_a_noop_when_unconfigured()
    {
        var c = Client();
        var body = await Ok(await c.PostAsync("/api/v1/aggregator/pickme/poll", Json(new { })), "pickme poll");
        body.GetProperty("ingested").GetInt32().Should().Be(0);   // no creds → nothing ingested, no network
    }

    [Fact]
    public async Task PickMe_outlet_api_key_is_stored_encrypted_and_never_returned()
    {
        var c = Client();
        await Ok(await c.PutAsync("/api/v1/aggregator/credentials/pickme",
            Json(new { isEnabled = true, environment = "sandbox" })), "enable pickme");

        var creds = await Ok(await c.GetAsync("/api/v1/aggregator/credentials"), "creds");
        var pm = creds.EnumerateArray().First(x => x.GetProperty("aggregator").GetString() == "pickme");
        var locId = pm.GetProperty("stores").EnumerateArray().First().GetProperty("locationId").GetString();

        var set = await Ok(await c.PutAsync($"/api/v1/aggregator/credentials/pickme/stores/{locId}",
            Json(new { externalStoreId = "OUTLET-1", apiKey = "super-secret-pickme-key", isEnabled = true })), "set key");
        set.GetProperty("hasApiKey").GetBoolean().Should().BeTrue();

        var after = await Ok(await c.GetAsync("/api/v1/aggregator/credentials"), "creds after");
        after.GetRawText().Should().NotContain("super-secret-pickme-key");   // raw key never leaves the server
        after.EnumerateArray().First(x => x.GetProperty("aggregator").GetString() == "pickme")
            .GetProperty("stores").EnumerateArray().First()
            .GetProperty("hasApiKey").GetBoolean().Should().BeTrue();
    }
}
