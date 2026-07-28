using FluentAssertions;
using Hms.Api.Features.Orders;
using Hms.Api.Features.Shifts;
using Xunit;

namespace Hms.Api.IntegrationTests;

/// <summary>
/// Cashier shifts + cash-up. Verifies the drawer math: cash kept for an order is
/// its total minus non-cash payments (so overpaid cash / change never inflates
/// the expected drawer), expected = opening float + cash, variance = declared −
/// expected. Plus the one-open-shift-per-location and lifecycle guards.
/// </summary>
[Collection("pg")]
public class ShiftTests(PostgresFixture fx) : IAsyncLifetime
{
    public Task InitializeAsync() => fx.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private ShiftService Shifts() => new(fx.Factory());

    [Fact]
    public async Task Open_then_close_computes_zreport_and_variance()
    {
        var shifts = Shifts();
        var (orders, _) = fx.Services();

        var opened = await shifts.OpenAsync(fx.LocationId, null, "Tester", 5000m, default);
        opened.Status.Should().Be("open");
        opened.ShiftNumber.Should().StartWith("SH-");
        opened.OpeningFloat.Should().Be(5000m);

        // Cash order — overpay by 100 (change), so the drawer must NOT count the 100.
        var o1 = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", "pos", null, "1", 1, null), default);
        await orders.AddItemAsync(o1.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);
        var t1 = (await orders.GetAsync(o1.Id, default))!.TotalAmount;
        await orders.SettleAsync(o1.Id, new SettleInput(new() { new PaymentInput("cash", t1 + 100m, null) }), default);

        // Card order — no cash impact.
        var o2 = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", "pos", null, "2", 1, null), default);
        await orders.AddItemAsync(o2.Id, new AddItemInput(fx.LionLagerId, 2, "bar", null), default);
        var t2 = (await orders.GetAsync(o2.Id, default))!.TotalAmount;
        await orders.SettleAsync(o2.Id, new SettleInput(new() { new PaymentInput("card", t2, null) }), default);

        // Live totals on the open shift.
        var cur = await shifts.GetCurrentAsync(fx.LocationId, default);
        cur!.OrderCount.Should().Be(2);
        cur.TotalSales.Should().Be(t1 + t2);
        cur.CashSales.Should().Be(t1);                  // drawer cash = order total (overpay excluded)
        cur.CardSales.Should().Be(t2);
        cur.ExpectedCash.Should().Be(5000m + t1);

        // Close declaring exactly the expected cash → zero variance.
        var report = await shifts.CloseAsync(opened.Id, 5000m + t1, "end of day", false, default);
        report.Status.Should().Be("closed");
        report.ExpectedCash.Should().Be(5000m + t1);
        report.CashVariance.Should().Be(0m);
        report.CardSales.Should().Be(t2);
        report.OrderCount.Should().Be(2);

        // No open shift remains.
        (await shifts.GetCurrentAsync(fx.LocationId, default)).Should().BeNull();
    }

    [Fact]
    public async Task Short_drawer_yields_negative_variance()
    {
        var shifts = Shifts();
        var (orders, _) = fx.Services();
        var opened = await shifts.OpenAsync(fx.LocationId, null, "Tester", 1000m, default);

        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", "pos", null, "1", 1, null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);
        var t = (await orders.GetAsync(o.Id, default))!.TotalAmount;
        await orders.SettleAsync(o.Id, new SettleInput(new() { new PaymentInput("cash", t, null) }), default);

        var expected = 1000m + t;
        var report = await shifts.CloseAsync(opened.Id, expected - 50m, null, false, default);   // 50 short
        report.CashVariance.Should().Be(-50m);
    }

    [Fact]
    public async Task Only_one_open_shift_per_location()
    {
        var shifts = Shifts();
        await shifts.OpenAsync(fx.LocationId, null, "A", 0m, default);
        var act = () => shifts.OpenAsync(fx.LocationId, null, "B", 0m, default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*already open*");
    }

    [Fact]
    public async Task Cannot_close_twice()
    {
        var shifts = Shifts();
        var opened = await shifts.OpenAsync(fx.LocationId, null, "A", 0m, default);
        await shifts.CloseAsync(opened.Id, 0m, null, false, default);
        var act = () => shifts.CloseAsync(opened.Id, 0m, null, false, default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*already closed*");
    }

    [Fact]
    public async Task Negative_float_is_rejected()
    {
        var shifts = Shifts();
        var act = () => shifts.OpenAsync(fx.LocationId, null, "A", -1m, default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*float*");
    }
}
