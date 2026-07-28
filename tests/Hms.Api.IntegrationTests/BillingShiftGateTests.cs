using FluentAssertions;
using Hms.Api.Features.Orders;
using Hms.Api.Features.Shifts;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Hms.Api.IntegrationTests;

/// <summary>
/// Cash control: when require_open_shift_for_billing is on, a bill can't be
/// settled without an open shift for the outlet. (Fixture default is off so the
/// rest of the suite is unaffected; this test turns it on.)
/// </summary>
[Collection("pg")]
public class BillingShiftGateTests(PostgresFixture fx) : IAsyncLifetime
{
    public Task InitializeAsync() => fx.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private async Task RequireShift(bool on)
    {
        await using var db = fx.NewTenantContext();
        await db.Database.ExecuteSqlRawAsync($"UPDATE org_settings SET require_open_shift_for_billing = {(on ? "true" : "false")}");
    }

    [Fact]
    public async Task Order_creation_is_blocked_without_an_open_shift_then_allowed()
    {
        await RequireShift(true);
        var (orders, _) = fx.Services();

        // no shift → can't even open a bill
        var act = () => orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "1", 1, null), default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*shift*");

        // open a shift → create + settle work
        await new ShiftService(fx.Factory()).OpenAsync(fx.LocationId, null, "Cashier", 1000m, default);
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "1", 1, null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);
        var total = (await orders.GetAsync(o.Id, default))!.TotalAmount;
        var settled = await orders.SettleAsync(o.Id, new SettleInput(new() { new PaymentInput("cash", total, null) }), default);
        settled.Status.Should().Be("settled");
    }

    [Fact]
    public async Task Aggregator_orders_are_not_shift_gated()
    {
        await RequireShift(true);   // on, but no shift open
        var (orders, _) = fx.Services();
        // a delivery order from an aggregator must NOT require a POS shift
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "delivery", "pickme", "PM-1", null, null, null), default);
        o.Status.Should().Be("open");
    }

    [Fact]
    public async Task Setting_off_lets_billing_proceed_without_a_shift()
    {
        await RequireShift(false);
        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "2", 1, null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);
        var total = (await orders.GetAsync(o.Id, default))!.TotalAmount;
        var settled = await orders.SettleAsync(o.Id, new SettleInput(new() { new PaymentInput("cash", total, null) }), default);
        settled.Status.Should().Be("settled");
    }

    [Fact]
    public async Task Closing_a_shift_auto_voids_empty_tables_but_blocks_on_open_bills()
    {
        await RequireShift(true);
        var shifts = new ShiftService(fx.Factory());
        var sh = await shifts.OpenAsync(fx.LocationId, null, "Cashier", 1000m, default);
        var (orders, _) = fx.Services();

        var empty = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "T1", 2, null), default);   // abandoned, no items
        var live = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "T2", 2, null), default);
        await orders.AddItemAsync(live.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);

        // a real open bill blocks the close
        var act = () => shifts.CloseAsync(sh.Id, 1000m, null, false, default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*open bill*");

        // settle it, then close → the empty table is auto-voided and the shift closes
        var total = (await orders.GetAsync(live.Id, default))!.TotalAmount;
        await orders.SettleAsync(live.Id, new SettleInput(new() { new PaymentInput("cash", total, null) }), default);
        var z = await shifts.CloseAsync(sh.Id, 1000m + total, null, false, default);
        z.Status.Should().Be("closed");

        await using var db = fx.NewTenantContext();
        (await db.Orders.IgnoreQueryFilters().FirstAsync(o => o.Id == empty.Id)).Status.Should().Be("void");
    }

    [Fact]
    public async Task Continue_and_close_carries_an_open_bill_over_to_the_next_shift()
    {
        await RequireShift(true);
        var shifts = new ShiftService(fx.Factory());
        var sh = await shifts.OpenAsync(fx.LocationId, null, "Cashier", 1000m, default);
        var (orders, _) = fx.Services();

        var live = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "T9", 2, null), default);
        await orders.AddItemAsync(live.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);

        // "Continue & close" (allowOpenBills=true): the shift closes and the live bill
        // stays open for the next shift — a night→day handover, not a void.
        var z = await shifts.CloseAsync(sh.Id, 1000m, null, true, default);
        z.Status.Should().Be("closed");

        await using var db = fx.NewTenantContext();
        (await db.Orders.IgnoreQueryFilters().FirstAsync(o => o.Id == live.Id)).Status.Should().Be("open");
    }
}
