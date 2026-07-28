using FluentAssertions;
using Hms.Api.Features.Orders;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Hms.Api.IntegrationTests;

[Collection("pg")]
public class OrderFlowTests(PostgresFixture fx) : IAsyncLifetime
{
    public Task InitializeAsync() => fx.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Compound_tax_stack_is_correct()
    {
        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "1", 2, null), default);
        // 10 × Chicken Kottu @ 900 = 9,000 subtotal
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 10, "kitchen", null), default);
        var got = await orders.GetAsync(o.Id, default);

        got!.SubtotalAmount.Should().Be(9000m);
        got.ServiceChargeAmount.Should().Be(900m);          // 10% of 9,000
        got.TaxAmount.Should().Be(247.50m + 1826.55m);      // SSCL 2.5% of 9,900 + VAT 18% of 10,147.50
        got.TotalAmount.Should().Be(11974.05m);
    }

    [Fact]
    public async Task Settle_decrements_stock_atomically()   // R1
    {
        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "2", 1, null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 3, "kitchen", null), default);
        var total = (await orders.GetAsync(o.Id, default))!.TotalAmount;

        await orders.SettleAsync(o.Id, new SettleInput(new() { new PaymentInput("cash", total, null) }), default);

        await using var db = fx.NewTenantContext();
        var stock = await db.ProductStocks.IgnoreQueryFilters()
            .FirstAsync(s => s.ProductId == fx.ChickenKottuId && s.LocationId == fx.LocationId);
        stock.QuantityOnHand.Should().Be(47m);   // 50 - 3
    }

    [Fact]
    public async Task Confirm_generates_kot_split_by_station()   // R2
    {
        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "3", 2, null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", "Spicy"), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.LionLagerId, 2, "bar", null), default);

        await orders.ConfirmAsync(o.Id, default);

        await using var db = fx.NewTenantContext();
        var tickets = await db.KitchenTickets.IgnoreQueryFilters().Where(t => t.OrderId == o.Id).ToListAsync();
        tickets.Should().HaveCount(2);
        tickets.Select(t => t.Station).Should().BeEquivalentTo(new[] { "kitchen", "bar" });
    }

    [Fact]
    public async Task Insufficient_payment_is_rejected()
    {
        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "4", 1, null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);

        var act = async () => await orders.SettleAsync(o.Id, new SettleInput(new() { new PaymentInput("cash", 10m, null) }), default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Insufficient*");
    }

    [Fact]
    public async Task Multi_tender_split_payment_settles()
    {
        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "5", 1, null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);
        var total = (await orders.GetAsync(o.Id, default))!.TotalAmount;

        await orders.SettleAsync(o.Id, new SettleInput(new()
        {
            new PaymentInput("cash", 500m, null),
            new PaymentInput("card", total, null),   // overpay → change
        }), default);

        var settled = await orders.GetAsync(o.Id, default);
        settled!.Status.Should().Be("settled");
        settled.InvoiceNumber.Should().StartWith("INV/");
    }

    [Fact]
    public async Task Open_custom_price_item_adds_to_subtotal_without_stock()
    {
        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "6", 1, null), default);
        await orders.AddCustomItemAsync(o.Id, "Special platter", 1500m, 1, "kitchen", default);
        var got = await orders.GetAsync(o.Id, default);
        got!.SubtotalAmount.Should().Be(1500m);
        got.Items.Should().ContainSingle(i => i.Sku == "OPEN" && i.ProductName == "Special platter");
    }
}
