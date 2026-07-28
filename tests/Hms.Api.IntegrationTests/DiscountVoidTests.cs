using FluentAssertions;
using Hms.Api.Features.Orders;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Hms.Api.IntegrationTests;

[Collection("pg")]
public class DiscountVoidTests(PostgresFixture fx) : IAsyncLifetime
{
    public Task InitializeAsync() => fx.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Voiding_an_order_pulls_its_tickets_off_the_kitchen_board()
    {
        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "9", 1, null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);
        await orders.ConfirmAsync(o.Id, default);   // creates a KOT ticket

        await using (var db = fx.NewTenantContext())
            (await db.KitchenTickets.CountAsync(t => t.OrderId == o.Id && t.Status != "cancelled")).Should().BeGreaterThan(0);

        await orders.VoidAsync(o.Id, "test", default);

        await using (var db = fx.NewTenantContext())
        {
            var active = await db.KitchenTickets.CountAsync(t => t.OrderId == o.Id
                && (t.Status == "new" || t.Status == "preparing" || t.Status == "ready"));
            active.Should().Be(0, "voiding clears the order's tickets from the board");
        }
    }

    [Fact]
    public async Task Settling_an_order_leaves_its_tickets_for_the_kitchen_to_finish()
    {
        // Payment and the KDS are independent. Paying the bill must NOT bump the
        // ticket — the line still has to cook the food and mark it ready/served.
        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "8", 1, null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);
        await orders.ConfirmAsync(o.Id, default);   // creates a live KOT ticket ('new')

        var total = (await orders.GetAsync(o.Id, default))!.TotalAmount;
        await orders.SettleAsync(o.Id, new SettleInput(new() { new PaymentInput("cash", total, null) }), default);

        await using var db = fx.NewTenantContext();
        // The ticket is still live on the board until the kitchen bumps it.
        (await db.KitchenTickets.CountAsync(t => t.OrderId == o.Id
            && (t.Status == "new" || t.Status == "preparing" || t.Status == "ready")))
            .Should().BeGreaterThan(0, "payment must not clear the kitchen — the line still cooks & marks it ready");
    }

    [Fact]
    public async Task Updating_one_lines_quantity_leaves_other_lines_untouched()
    {
        // Regression: pressing +/- on one POS line must never change another line.
        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "7", 1, null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 2, "kitchen", null), default);   // a second, distinct line

        var before = (await orders.GetAsync(o.Id, default))!.Items.Where(i => !i.IsDeleted).ToList();
        before.Should().HaveCount(2);
        var line1 = before[0]; var line2 = before[1];

        await orders.UpdateItemQtyAsync(o.Id, line1.Id, 5m, default);   // bump line 1 only

        var after = (await orders.GetAsync(o.Id, default))!.Items.Where(i => !i.IsDeleted).ToList();
        after.First(i => i.Id == line1.Id).Quantity.Should().Be(5m);
        after.First(i => i.Id == line2.Id).Quantity.Should().Be(line2.Quantity, "the other line must be untouched");
        after.First(i => i.Id == line2.Id).UnitPrice.Should().Be(line2.UnitPrice, "and its price must not drift");
    }

    [Fact]
    public async Task SetDiscount_fixed_amount_reduces_total()
    {
        var (orders, _) = fx.Services();

        // Baseline: 1 × Chicken Kottu @ 900, no discount.
        var baseline = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "1", 1, null), default);
        await orders.AddItemAsync(baseline.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);
        var baselineTotal = (await orders.GetAsync(baseline.Id, default))!.TotalAmount;

        // Same order with a 200 fixed discount applied to the subtotal.
        var discounted = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "2", 1, null), default);
        await orders.AddItemAsync(discounted.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);
        await orders.SetDiscountAsync(discounted.Id, amount: 200m, percent: null, default);

        var got = await orders.GetAsync(discounted.Id, default);
        got!.DiscountAmount.Should().Be(200m);
        got.SubtotalAmount.Should().Be(900m);
        // Discount is subtracted from the post-charge total.
        got.TotalAmount.Should().Be(baselineTotal - 200m);
        got.TotalAmount.Should().BeLessThan(baselineTotal);
    }

    [Fact]
    public async Task SetDiscount_percent_sets_amount_to_percent_of_subtotal()
    {
        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "3", 1, null), default);
        // 2 × Chicken Kottu @ 900 = 1,800 subtotal.
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 2, "kitchen", null), default);

        await orders.SetDiscountAsync(o.Id, amount: null, percent: 10m, default);

        var got = await orders.GetAsync(o.Id, default);
        got!.SubtotalAmount.Should().Be(1800m);
        got.DiscountAmount.Should().Be(180m);   // 10% of 1,800
    }

    [Fact]
    public async Task UpdateItemQty_to_zero_removes_the_line()
    {
        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "4", 1, null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);   // 900
        var withKottu = await orders.AddItemAsync(o.Id, new AddItemInput(fx.LionLagerId, 1, "bar", null), default); // + 450
        (await orders.GetAsync(o.Id, default))!.SubtotalAmount.Should().Be(1350m);

        var lager = withKottu.Items.First(i => i.ProductId == fx.LionLagerId);
        await orders.UpdateItemQtyAsync(o.Id, lager.Id, 0m, default);

        var got = await orders.GetAsync(o.Id, default);
        // The Lager line is soft-deleted: no live (non-deleted) line for it remains.
        got!.Items.Where(i => !i.IsDeleted).Should().NotContain(i => i.ProductId == fx.LionLagerId);
        got.Items.Where(i => !i.IsDeleted).Should().ContainSingle(i => i.ProductId == fx.ChickenKottuId);
        got.SubtotalAmount.Should().Be(900m);   // dropped by the 450 line
    }

    [Fact]
    public async Task UpdateItemQty_changes_qty_and_recomputes_subtotal()
    {
        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "5", 1, null), default);
        var added = await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);
        (await orders.GetAsync(o.Id, default))!.SubtotalAmount.Should().Be(900m);

        var line = added.Items.First(i => i.ProductId == fx.ChickenKottuId);
        await orders.UpdateItemQtyAsync(o.Id, line.Id, 4m, default);

        var got = await orders.GetAsync(o.Id, default);
        var updated = got!.Items.First(i => i.ProductId == fx.ChickenKottuId && !i.IsDeleted);
        updated.Quantity.Should().Be(4m);
        updated.LineSubtotal.Should().Be(3600m);   // 4 × 900
        got.SubtotalAmount.Should().Be(3600m);
    }

    [Fact]
    public async Task Void_sets_status_and_blocks_subsequent_settle()
    {
        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "6", 1, null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);
        var total = (await orders.GetAsync(o.Id, default))!.TotalAmount;

        await orders.VoidAsync(o.Id, "customer left", default);
        (await orders.GetAsync(o.Id, default))!.Status.Should().Be("void");

        var act = async () => await orders.SettleAsync(o.Id, new SettleInput(new() { new PaymentInput("cash", total, null) }), default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*void*");
    }

    [Fact]
    public async Task Settling_an_already_settled_order_throws()
    {
        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "7", 1, null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);
        var total = (await orders.GetAsync(o.Id, default))!.TotalAmount;

        await orders.SettleAsync(o.Id, new SettleInput(new() { new PaymentInput("cash", total, null) }), default);
        (await orders.GetAsync(o.Id, default))!.Status.Should().Be("settled");

        var act = async () => await orders.SettleAsync(o.Id, new SettleInput(new() { new PaymentInput("cash", total, null) }), default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*already settled*");
    }

    [Fact]
    public async Task Takeaway_skips_service_charge_but_applies_vat_and_sscl()
    {
        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "takeaway", null, null, null, 1, null), default);
        // 1 × Chicken Kottu @ 900 subtotal.
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);

        var got = await orders.GetAsync(o.Id, default);
        got!.SubtotalAmount.Should().Be(900m);
        // SVC.applies_to_takeaway = false → no service charge.
        got.ServiceChargeAmount.Should().Be(0m);
        // SSCL 2.5% of 900 (= 22.50) + VAT 18% of 922.50 (= 166.05), both compound.
        got.TaxAmount.Should().Be(22.50m + 166.05m);
        got.TaxAmount.Should().BeGreaterThan(0m);
        got.TotalAmount.Should().Be(900m + 22.50m + 166.05m);
    }
}
