using FluentAssertions;
using Hms.Api.Features.Inventory;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Hms.Api.IntegrationTests;

[Collection("pg")]
public class InventoryMovementTests(PostgresFixture fx) : IAsyncLifetime
{
    public Task InitializeAsync() => fx.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private InventoryMovementService Svc() => new(fx.Factory());

    private async Task<decimal> QtyAt(Guid product, Guid loc)
    {
        await using var db = fx.NewTenantContext();
        var s = await db.ProductStocks.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.ProductId == product && x.LocationId == loc);
        return s?.QuantityOnHand ?? 0;
    }
    private async Task<decimal> AvgAt(Guid product, Guid loc)
    {
        await using var db = fx.NewTenantContext();
        var s = await db.ProductStocks.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.ProductId == product && x.LocationId == loc);
        return s?.AverageCost ?? 0;
    }

    [Fact]
    public async Task Transfer_dispatch_then_receive_moves_stock_and_cost()
    {
        var svc = Svc();
        // MAIN has 50 @ 350. Transfer 20 to Central Kitchen.
        var t = await svc.CreateTransferAsync(new CreateTransferInput(
            fx.LocationId, fx.CentralKitchenId, false, "to CK",
            new() { new TransferLineInput(fx.ChickenKottuId, 20) }), default);
        t.Status.Should().Be("draft");

        await svc.DispatchTransferAsync(t.Id, default);
        (await QtyAt(fx.ChickenKottuId, fx.LocationId)).Should().Be(30m);          // 50 - 20 at source
        (await QtyAt(fx.ChickenKottuId, fx.CentralKitchenId)).Should().Be(0m);     // not received yet

        await svc.ReceiveTransferAsync(t.Id, default);
        (await QtyAt(fx.ChickenKottuId, fx.CentralKitchenId)).Should().Be(20m);     // received
        (await AvgAt(fx.ChickenKottuId, fx.CentralKitchenId)).Should().Be(350m);    // cost moved with goods

        var after = await svc.GetTransferAsync(t.Id, default);
        after!.Status.Should().Be("received");
        after.TotalCost.Should().Be(7000m);   // 20 * 350
    }

    [Fact]
    public async Task Return_transfer_flag_is_recorded()
    {
        var svc = Svc();
        var t = await svc.CreateTransferAsync(new CreateTransferInput(
            fx.LocationId, fx.CentralKitchenId, true, "return to CK",
            new() { new TransferLineInput(fx.ChickenKottuId, 5) }), default);
        t.IsReturn.Should().BeTrue();
        t.TransferNumber.Should().StartWith("TRF-");   // shared transfer prefix; is_return marks it
        await svc.DispatchTransferAsync(t.Id, default);
        (await QtyAt(fx.ChickenKottuId, fx.LocationId)).Should().Be(45m);
    }

    [Fact]
    public async Task Wastage_decrements_stock_and_values_at_avg_cost()
    {
        var svc = Svc();
        var w = await svc.RecordWastageAsync(new WastageInput(
            fx.LocationId, "spoilage", "dropped",
            new() { new WastageLineInput(fx.ChickenKottuId, 5) }), default);
        w.WastageNumber.Should().StartWith("WST-");
        w.TotalCost.Should().Be(1750m);   // 5 * 350
        (await QtyAt(fx.ChickenKottuId, fx.LocationId)).Should().Be(45m);
    }

    [Fact]
    public async Task Adjustment_applies_signed_delta_leaving_avg_unchanged()
    {
        var svc = Svc();
        await svc.AdjustAsync(new AdjustmentInput(fx.LocationId, "count", "recount",
            new() { new AdjustmentLineInput(fx.ChickenKottuId, +10) }), default);
        (await QtyAt(fx.ChickenKottuId, fx.LocationId)).Should().Be(60m);
        (await AvgAt(fx.ChickenKottuId, fx.LocationId)).Should().Be(350m);

        await svc.AdjustAsync(new AdjustmentInput(fx.LocationId, "correction", null,
            new() { new AdjustmentLineInput(fx.ChickenKottuId, -8) }), default);
        (await QtyAt(fx.ChickenKottuId, fx.LocationId)).Should().Be(52m);
    }

    // ── negatives ──
    [Fact]
    public async Task Transfer_to_same_location_throws()
    {
        var svc = Svc();
        var act = () => svc.CreateTransferAsync(new CreateTransferInput(
            fx.LocationId, fx.LocationId, false, null, new() { new TransferLineInput(fx.ChickenKottuId, 1) }), default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*must differ*");
    }

    [Fact]
    public async Task Dispatch_insufficient_stock_throws()
    {
        var svc = Svc();
        var t = await svc.CreateTransferAsync(new CreateTransferInput(
            fx.LocationId, fx.CentralKitchenId, false, null,
            new() { new TransferLineInput(fx.ChickenKottuId, 9999) }), default);
        var act = () => svc.DispatchTransferAsync(t.Id, default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Insufficient*");
    }

    [Fact]
    public async Task Receive_before_dispatch_throws()
    {
        var svc = Svc();
        var t = await svc.CreateTransferAsync(new CreateTransferInput(
            fx.LocationId, fx.CentralKitchenId, false, null,
            new() { new TransferLineInput(fx.ChickenKottuId, 1) }), default);
        var act = () => svc.ReceiveTransferAsync(t.Id, default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Cannot receive*");
    }

    [Fact]
    public async Task Adjustment_with_zero_delta_throws()
    {
        var svc = Svc();
        var act = () => svc.AdjustAsync(new AdjustmentInput(fx.LocationId, "count", null,
            new() { new AdjustmentLineInput(fx.ChickenKottuId, 0) }), default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*cannot be zero*");
    }
}
