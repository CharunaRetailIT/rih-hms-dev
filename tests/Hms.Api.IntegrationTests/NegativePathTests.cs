using FluentAssertions;
using Hms.Api.Features.Orders;
using Xunit;

namespace Hms.Api.IntegrationTests;

/// <summary>
/// Negative / failure-path coverage — the guards that protect the
/// transactional core. Every one asserts the operation is REJECTED (throws)
/// rather than silently corrupting data.
/// </summary>
[Collection("pg")]
public class NegativePathTests(PostgresFixture fx) : IAsyncLifetime
{
    public Task InitializeAsync() => fx.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<Hms.Api.Domain.Order> NewOrderWithItem(OrderService orders)
    {
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "1", 1, null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);
        return (await orders.GetAsync(o.Id, default))!;
    }

    // ── order / item guards ──────────────────────────────────────────────────
    [Fact]
    public async Task AddItem_to_missing_order_throws()
    {
        var (orders, _) = fx.Services();
        var act = () => orders.AddItemAsync(Guid.NewGuid(), new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Order not found*");
    }

    [Fact]
    public async Task AddItem_with_unknown_product_throws()
    {
        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "1", 1, null), default);
        var act = () => orders.AddItemAsync(o.Id, new AddItemInput(Guid.NewGuid(), 1, "kitchen", null), default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Product not found*");
    }

    [Fact]
    public async Task AddItem_to_settled_order_throws()
    {
        var (orders, _) = fx.Services();
        var o = await NewOrderWithItem(orders);
        await orders.SettleAsync(o.Id, new SettleInput(new() { new PaymentInput("cash", o.TotalAmount, null) }), default);
        var act = () => orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*settled*");
    }

    [Fact]
    public async Task UpdateQty_on_missing_item_throws()
    {
        var (orders, _) = fx.Services();
        var o = await NewOrderWithItem(orders);
        var act = () => orders.UpdateItemQtyAsync(o.Id, Guid.NewGuid(), 5, default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Item not found*");
    }

    // ── custom item guards ─────────────────────────────────────────────────────
    [Fact]
    public async Task CustomItem_blank_name_throws()
    {
        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "1", 1, null), default);
        var act = () => orders.AddCustomItemAsync(o.Id, "   ", 100m, 1, "kitchen", default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*name is required*");
    }

    [Fact]
    public async Task CustomItem_negative_price_throws()
    {
        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "1", 1, null), default);
        var act = () => orders.AddCustomItemAsync(o.Id, "Bad", -5m, 1, "kitchen", default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*negative*");
    }

    // ── confirm / KOT guards ───────────────────────────────────────────────────
    [Fact]
    public async Task Confirm_with_no_new_items_throws()
    {
        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "1", 1, null), default);
        var act = () => orders.ConfirmAsync(o.Id, default);   // nothing added
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*No new items*");
    }

    [Fact]
    public async Task Confirm_twice_throws_second_time()
    {
        var (orders, _) = fx.Services();
        var o = await NewOrderWithItem(orders);
        await orders.ConfirmAsync(o.Id, default);             // first confirm OK
        var act = () => orders.ConfirmAsync(o.Id, default);   // no remaining pending items
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*No new items*");
    }

    // ── settle guards ──────────────────────────────────────────────────────────
    [Fact]
    public async Task Settle_with_no_payments_is_insufficient()
    {
        var (orders, _) = fx.Services();
        var o = await NewOrderWithItem(orders);
        var act = () => orders.SettleAsync(o.Id, new SettleInput(new()), default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Insufficient*");
    }

    [Fact]
    public async Task Settle_already_settled_throws()
    {
        var (orders, _) = fx.Services();
        var o = await NewOrderWithItem(orders);
        await orders.SettleAsync(o.Id, new SettleInput(new() { new PaymentInput("cash", o.TotalAmount, null) }), default);
        var act = () => orders.SettleAsync(o.Id, new SettleInput(new() { new PaymentInput("cash", o.TotalAmount, null) }), default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*already settled*");
    }

    [Fact]
    public async Task Settle_void_order_throws()
    {
        var (orders, _) = fx.Services();
        var o = await NewOrderWithItem(orders);
        await orders.VoidAsync(o.Id, "test", default);
        var act = () => orders.SettleAsync(o.Id, new SettleInput(new() { new PaymentInput("cash", o.TotalAmount, null) }), default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*void*");
    }

    // ── void guards ────────────────────────────────────────────────────────────
    [Fact]
    public async Task Void_settled_order_throws()
    {
        var (orders, _) = fx.Services();
        var o = await NewOrderWithItem(orders);
        await orders.SettleAsync(o.Id, new SettleInput(new() { new PaymentInput("cash", o.TotalAmount, null) }), default);
        var act = () => orders.VoidAsync(o.Id, "oops", default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Cannot void a settled order*");
    }

    // ── discount guards ────────────────────────────────────────────────────────
    [Fact]
    public async Task Discount_on_void_order_throws()
    {
        var (orders, _) = fx.Services();
        var o = await NewOrderWithItem(orders);
        await orders.VoidAsync(o.Id, "x", default);
        var act = () => orders.SetDiscountAsync(o.Id, 100m, null, default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*void*");
    }

    [Fact]
    public async Task Negative_discount_is_clamped_to_zero()
    {
        var (orders, _) = fx.Services();
        var o = await NewOrderWithItem(orders);
        var after = await orders.SetDiscountAsync(o.Id, -500m, null, default);
        after.DiscountAmount.Should().Be(0m);
    }

    // ── aggregator guards ──────────────────────────────────────────────────────
    [Fact]
    public async Task Accept_non_pending_order_throws()
    {
        var (orders, agg) = fx.Services();
        // a normal POS order has no aggregator_status → not "pending"
        var o = await NewOrderWithItem(orders);
        var act = () => agg.AcceptAsync(o.Id, 20, default);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Accept_twice_throws_second_time()
    {
        var (_, agg) = fx.Services();
        var sim = await agg.SimulateAsync("ubereats", fx.LocationId, 9911, default);
        await agg.AcceptAsync(sim.Id, 20, default);
        var act = () => agg.AcceptAsync(sim.Id, 20, default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*already*");
    }
}
