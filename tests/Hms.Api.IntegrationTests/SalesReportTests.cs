using FluentAssertions;
using Hms.Api.Features.Orders;
using Xunit;

namespace Hms.Api.IntegrationTests;

/// <summary>
/// Step 8 (reporting): the settled-sales summary that feeds the Reports screen.
/// Settles a POS order and a delivery order, then asserts the headline KPIs,
/// the per-source split, and the top-items ranking.
/// </summary>
[Collection("pg")]
public class SalesReportTests(PostgresFixture fx) : IAsyncLifetime
{
    public Task InitializeAsync() => fx.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private static readonly DateTime From = new(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime To = From.AddMonths(1);

    [Fact]
    public async Task Sales_summary_aggregates_settled_orders_by_source_and_item()
    {
        var (orders, _) = fx.Services();

        // POS order: 2 Chicken Kottu.
        var pos = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", "pos", null, "1", 1, null), default);
        await orders.AddItemAsync(pos.Id, new AddItemInput(fx.ChickenKottuId, 2, "kitchen", null), default);
        var posTotal = (await orders.GetAsync(pos.Id, default))!.TotalAmount;
        await orders.SettleAsync(pos.Id, new SettleInput(new() { new PaymentInput("cash", posTotal, null) }), default);

        // Delivery order (PickMe): 1 Chicken Kottu + 1 Lion Lager.
        var del = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "delivery", "pickme", "PM-1", null, null, null), default);
        await orders.AddItemAsync(del.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);
        await orders.AddItemAsync(del.Id, new AddItemInput(fx.LionLagerId, 1, "bar", null), default);
        var delTotal = (await orders.GetAsync(del.Id, default))!.TotalAmount;
        await orders.SettleAsync(del.Id, new SettleInput(new() { new PaymentInput("pickme_prepaid", delTotal, null) }), default);

        var s = await orders.SalesSummaryAsync(From, To, null, default);

        s.OrderCount.Should().Be(2);
        s.GrossSales.Should().Be(posTotal + delTotal);

        // Two sources, POS first by revenue (2 Kottu > 1 Kottu + 1 Lion).
        s.BySource.Should().HaveCount(2);
        s.BySource.Select(x => x.Source).Should().BeEquivalentTo(new[] { "pos", "pickme" });
        s.BySource.Sum(x => x.OrderCount).Should().Be(2);

        // Chicken Kottu sold 3 across both orders; tops the chart.
        var kottu = s.TopItems.Single(t => t.ProductId == fx.ChickenKottuId);
        kottu.Quantity.Should().Be(3m);
        s.TopItems.Should().Contain(t => t.ProductId == fx.LionLagerId && t.Quantity == 1m);
        s.TopItems.First().ProductId.Should().Be(fx.ChickenKottuId);
    }

    [Fact]
    public async Task Outlet_summary_breaks_sales_and_stock_down_by_location()
    {
        var (orders, _) = fx.Services();

        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", "pos", null, "1", 1, null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);
        var total = (await orders.GetAsync(o.Id, default))!.TotalAmount;
        await orders.SettleAsync(o.Id, new SettleInput(new() { new PaymentInput("cash", total, null) }), default);

        var r = await orders.OutletSummaryAsync(From, To, default);

        r.Outlets.Should().HaveCount(2);                                  // MAIN + CK
        r.TotalGross.Should().Be(total);
        var main = r.Outlets.Single(x => x.LocationId == fx.LocationId);
        main.OrderCount.Should().Be(1);
        main.GrossSales.Should().Be(total);
        main.StockValue.Should().BeGreaterThan(0m);                      // MAIN holds the seeded stock
        var ck = r.Outlets.Single(x => x.LocationId == fx.CentralKitchenId);
        ck.OrderCount.Should().Be(0);
    }

    [Fact]
    public async Task Empty_period_returns_zeroed_summary()
    {
        var (orders, _) = fx.Services();
        // A window in the far past — nothing settled there.
        var s = await orders.SalesSummaryAsync(
            new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2000, 2, 1, 0, 0, 0, DateTimeKind.Utc), null, default);
        s.OrderCount.Should().Be(0);
        s.GrossSales.Should().Be(0m);
        s.TopItems.Should().BeEmpty();
        s.ByDay.Should().BeEmpty();
    }
}
