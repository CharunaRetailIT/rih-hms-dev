using FluentAssertions;
using Hms.Api.Features.Aggregators.PickMe;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Hms.Api.IntegrationTests;

/// <summary>Pure mapping helpers — no DB, so no fixture.</summary>
public class PickMeMappingTests
{
    [Fact]
    public void Resolves_base_url_by_environment_with_override()
    {
        PickMeClient.ResolveBaseUrl("sandbox", null).Should().Be("https://api.stage-mytaxi.com");
        PickMeClient.ResolveBaseUrl(null, null).Should().Be("https://api.stage-mytaxi.com");
        PickMeClient.ResolveBaseUrl("live", null).Should().Be("https://api.pickme.lk");
        PickMeClient.ResolveBaseUrl("production", null).Should().Be("https://api.pickme.lk");
        PickMeClient.ResolveBaseUrl("live", "https://custom.example/").Should().Be("https://custom.example");
    }

    [Fact]
    public void Maps_delivery_mode_and_cancel_statuses()
    {
        PickMeService.MapOrderType("PickUp").Should().Be("takeaway");
        PickMeService.MapOrderType("Delivery").Should().Be("delivery");
        PickMeService.IsPickup("PickUp").Should().BeTrue();

        PickMeService.IsCancelled("Job Cancelled").Should().BeTrue();
        PickMeService.IsCancelled("Job Declined").Should().BeTrue();
        PickMeService.IsCancelled("Job Timed Out").Should().BeTrue();
        PickMeService.IsCancelled("Merchant Confirmed").Should().BeFalse();
        PickMeService.IsCancelled(null).Should().BeFalse();
    }

    [Fact]
    public void Derives_unit_price_and_folds_notes()
    {
        PickMeService.UnitPrice(new PickMeOrderItem(1, "X", "n", 4, 1000, null, null)).Should().Be(250);
        PickMeService.UnitPrice(new PickMeOrderItem(1, "X", "n", 0, 300, null, null)).Should().Be(300); // guard /0

        var it = new PickMeOrderItem(1, "X", "n", 1, 100, "No onion",
            new() { new PickMeOption("Sauce", new() { new PickMeOptionItem("BBQ", 1, 0, null) }) });
        var notes = PickMeService.BuildItemNotes(it);
        notes.Should().Contain("No onion");
        notes.Should().Contain("Sauce: BBQ");

        PickMeService.BuildItemNotes(new PickMeOrderItem(1, "X", "n", 1, 100, null, null)).Should().BeNull();
    }
}

/// <summary>PickMe order ingestion against the real services + test DB (no network).</summary>
[Collection("pg")]
public class PickMeIngestTests(PostgresFixture fx) : IAsyncLifetime
{
    public Task InitializeAsync() => fx.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private static PickMeJob Job(string id, string status, string refId = "KOTTU-CHK",
        string name = "Chicken Kottu", decimal qty = 2, decimal total = 1800, string? spIns = "Extra spicy") =>
        new(
            PickmeJobId: id,
            Customer: new PickMeCustomer("+94771234567", new PickMeLocation("12 Galle Rd, Colombo 03")),
            Outlet: new PickMeOutlet("Demo", "+94110000000", new PickMeLocation("Outlet")),
            Order: new PickMeOrder(
                new() { new PickMeOrderItem(1505, refId, name, qty, total, spIns,
                    new() { new PickMeOption("Spice", new() { new PickMeOptionItem("Hot", 1, 0, null) }) }) },
                "Leave at reception"),
            Payment: new PickMePayment(total, "CASH"),
            Status: new PickMeStatus(status, "1700000000000"),
            DeliveryMode: "Delivery",
            CreatedTimestamp: "1700000000000");

    [Fact]
    public async Task Confirmed_job_ingests_as_a_settled_prepaid_delivery_order()
    {
        var created = await fx.PickMe().IngestJobAsync(fx.LocationId, Job("PM-JOB-1", "Merchant Confirmed"), default);
        created.Should().BeTrue();

        await using var db = fx.NewTenantContext();
        var order = await db.Orders.Include(o => o.Items)
            .FirstAsync(o => o.OrderSource == "pickme" && o.ExternalOrderId == "PM-JOB-1");

        order.OrderType.Should().Be("delivery");
        order.AggregatorStatus.Should().Be("Merchant Confirmed");
        order.Status.Should().Be("settled");                 // prepaid recorded
        order.DeliveryPhone.Should().Be("+94771234567");
        var line = order.Items.Single(i => !i.IsDeleted);
        line.ProductName.Should().Be("Chicken Kottu");        // mapped by ref_id == SKU
        line.Quantity.Should().Be(2);
        line.Notes.Should().Contain("Extra spicy");           // sp_ins + option folded in
    }

    [Fact]
    public async Task Ingest_is_idempotent_on_pickme_job_id()
    {
        var svc = fx.PickMe();
        (await svc.IngestJobAsync(fx.LocationId, Job("PM-JOB-2", "Merchant Confirmed"), default)).Should().BeTrue();
        (await svc.IngestJobAsync(fx.LocationId, Job("PM-JOB-2", "Driver Accepted"), default)).Should().BeFalse();

        await using var db = fx.NewTenantContext();
        (await db.Orders.CountAsync(o => o.ExternalOrderId == "PM-JOB-2")).Should().Be(1);
        // status mirrored forward on the second poll
        var o = await db.Orders.FirstAsync(x => x.ExternalOrderId == "PM-JOB-2");
        o.AggregatorStatus.Should().Be("Driver Accepted");
    }

    [Fact]
    public async Task Cancel_status_mirrors_onto_an_existing_order()
    {
        var svc = fx.PickMe();
        await svc.IngestJobAsync(fx.LocationId, Job("PM-JOB-3", "Merchant Confirmed"), default);
        (await svc.IngestJobAsync(fx.LocationId, Job("PM-JOB-3", "Job Cancelled"), default)).Should().BeFalse();

        await using var db = fx.NewTenantContext();
        var o = await db.Orders.FirstAsync(x => x.ExternalOrderId == "PM-JOB-3");
        o.AggregatorStatus.Should().Be("Job Cancelled");
    }

    [Fact]
    public async Task Cancelled_new_job_is_never_materialised()
    {
        (await fx.PickMe().IngestJobAsync(fx.LocationId, Job("PM-JOB-4", "Job Declined"), default)).Should().BeFalse();

        await using var db = fx.NewTenantContext();
        (await db.Orders.CountAsync(o => o.ExternalOrderId == "PM-JOB-4")).Should().Be(0);
    }

    [Fact]
    public async Task Unmapped_ref_id_falls_back_to_a_custom_line()
    {
        await fx.PickMe().IngestJobAsync(fx.LocationId,
            Job("PM-JOB-5", "Merchant Confirmed", refId: "NOT-A-SKU", name: "Mystery Box", qty: 1, total: 500, spIns: null), default);

        await using var db = fx.NewTenantContext();
        var order = await db.Orders.Include(o => o.Items).FirstAsync(o => o.ExternalOrderId == "PM-JOB-5");
        order.Items.Where(i => !i.IsDeleted).Should().ContainSingle(i => i.ProductName.StartsWith("Mystery Box"));
    }
}
