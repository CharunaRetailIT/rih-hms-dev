using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Hms.Api.IntegrationTests;

[Collection("pg")]
public class AggregatorFlowTests(PostgresFixture fx) : IAsyncLifetime
{
    public Task InitializeAsync() => fx.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Simulate_ingests_pending_unsettled_order_with_menu_items()
    {
        var (_, agg) = fx.Services();

        var order = await agg.SimulateAsync("ubereats", fx.LocationId, 1, default);

        order.OrderSource.Should().Be("ubereats");
        order.AggregatorStatus.Should().Be("pending");
        order.Status.Should().Be("open");          // NOT settled — sits in the incoming queue
        order.InvoiceNumber.Should().BeNull();     // no money taken until accept
        order.Items.Should().NotBeEmpty();
        // Lines are mapped from the tenant's real menu by SKU.
        order.Items.Should().OnlyContain(i => i.ProductName == "Chicken Kottu" || i.ProductName == "Lion Lager");
    }

    [Fact]
    public async Task Simulate_is_idempotent_on_same_seed()
    {
        var (_, agg) = fx.Services();

        var first = await agg.SimulateAsync("ubereats", fx.LocationId, 7, default);
        var second = await agg.SimulateAsync("ubereats", fx.LocationId, 7, default);

        second.Id.Should().Be(first.Id);   // same external id → returns existing, no duplicate

        await using var db = fx.NewTenantContext();
        var count = await db.Orders.IgnoreQueryFilters()
            .CountAsync(o => o.OrderSource == "ubereats" && o.ExternalOrderId == "UE-00007");
        count.Should().Be(1);
    }

    [Fact]
    public async Task Accept_fires_kot_settles_prepaid_and_writes_priceless_ticket()
    {
        var (_, agg) = fx.Services();
        var pending = await agg.SimulateAsync("ubereats", fx.LocationId, 1, default);

        var accepted = await agg.AcceptAsync(pending.Id, 25, default);

        accepted.AggregatorStatus.Should().Be("preparing");
        accepted.PrepMinutes.Should().Be(25);
        accepted.Status.Should().Be("settled");                 // prepaid recorded on accept
        accepted.InvoiceNumber.Should().StartWith("INV/");

        await using var db = fx.NewTenantContext();
        var ticket = await db.KitchenTickets.IgnoreQueryFilters()
            .FirstAsync(t => t.OrderId == pending.Id);
        ticket.OrderSource.Should().Be("ubereats");
        ticket.OrderLabel.Should().Contain("Uber");

        // R4: the kitchen ticket is price-less — items carry name + quantity only.
        using var doc = JsonDocument.Parse(ticket.ItemsJson);
        doc.RootElement.ValueKind.Should().Be(JsonValueKind.Array);
        doc.RootElement.GetArrayLength().Should().BeGreaterThan(0);
        foreach (var el in doc.RootElement.EnumerateArray())
        {
            el.TryGetProperty("ProductName", out _).Should().BeTrue();
            el.TryGetProperty("Quantity", out _).Should().BeTrue();
            el.TryGetProperty("Price", out _).Should().BeFalse();
            el.TryGetProperty("UnitPrice", out _).Should().BeFalse();
        }
    }

    [Fact]
    public async Task Ready_then_picked_up_advance_aggregator_status()
    {
        var (_, agg) = fx.Services();
        var pending = await agg.SimulateAsync("pickme", fx.LocationId, 3, default);
        await agg.AcceptAsync(pending.Id, 20, default);

        var ready = await agg.SetReadyAsync(pending.Id, default);
        ready.AggregatorStatus.Should().Be("ready");

        var pickedUp = await agg.SetPickedUpAsync(pending.Id, default);
        pickedUp.AggregatorStatus.Should().Be("picked_up");
    }

    [Fact]
    public async Task Reject_voids_order_and_marks_rejected()
    {
        var (_, agg) = fx.Services();
        var pending = await agg.SimulateAsync("ubereats", fx.LocationId, 5, default);

        var rejected = await agg.RejectAsync(pending.Id, "Out of stock", default);

        rejected.Status.Should().Be("void");
        rejected.AggregatorStatus.Should().Be("rejected");
    }

    [Fact]
    public async Task Accepting_an_already_accepted_order_throws()
    {
        var (_, agg) = fx.Services();
        var pending = await agg.SimulateAsync("ubereats", fx.LocationId, 9, default);
        await agg.AcceptAsync(pending.Id, 15, default);

        var act = async () => await agg.AcceptAsync(pending.Id, 15, default);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
