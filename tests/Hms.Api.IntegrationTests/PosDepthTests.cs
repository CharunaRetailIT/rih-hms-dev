using FluentAssertions;
using Hms.Api.Domain;
using Hms.Api.Features.Orders;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Hms.Api.IntegrationTests;

/// <summary>
/// #76 POS depth — tips, multi-currency tender, tour-operator commission and
/// steward attribution. Covers already ride on CreateOrderInput.
/// </summary>
[Collection("pg")]
public class PosDepthTests(PostgresFixture fx) : IAsyncLifetime
{
    public Task InitializeAsync() => fx.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Tip_is_added_on_top_of_the_total()
    {
        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "1", 2, null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);
        var baseTotal = (await orders.GetAsync(o.Id, default))!.TotalAmount;

        await orders.SetTipAsync(o.Id, 150m, default);

        var got = (await orders.GetAsync(o.Id, default))!;
        got.TipAmount.Should().Be(150m);
        got.TotalAmount.Should().Be(baseTotal + 150m, "the tip is added on top of the bill, untaxed");
    }

    [Fact]
    public async Task Foreign_currency_tender_converts_to_base_for_settlement()
    {
        await using (var seed = fx.NewTenantContext())
        {
            seed.Currencies.Add(new Currency { Code = "USD", Name = "US Dollar", Symbol = "$", RateToBase = 300m, IsActive = true });
            await seed.SaveChangesAsync();
        }

        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "2", 1, null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);
        var total = (await orders.GetAsync(o.Id, default))!.TotalAmount;   // base LKR

        // Pay USD 5 (× 300 = 1,500 base) — comfortably covers the ~1,197 LKR bill.
        await orders.SettleAsync(o.Id, new SettleInput(new() { new PaymentInput("cash", 5m, null, "USD") }), default);

        (await orders.GetAsync(o.Id, default))!.Status.Should().Be("settled");
        await using var db = fx.NewTenantContext();
        var pay = await db.Payments.AsNoTracking().FirstAsync(p => p.OrderId == o.Id);
        pay.CurrencyCode.Should().Be("USD");
        pay.FxRate.Should().Be(300m);
        pay.Amount.Should().Be(5m);
        pay.BaseAmount.Should().Be(1500m, "amount × rate is the base-currency value");
        pay.BaseAmount.Should().BeGreaterThanOrEqualTo(total);
    }

    [Fact]
    public async Task Insufficient_foreign_tender_is_rejected()
    {
        await using (var seed = fx.NewTenantContext())
        {
            seed.Currencies.Add(new Currency { Code = "USD", Name = "US Dollar", RateToBase = 300m, IsActive = true });
            await seed.SaveChangesAsync();
        }

        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "3", 1, null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);   // ~1,197 LKR

        // USD 1 = 300 base — well short of the bill.
        var act = async () => await orders.SettleAsync(o.Id, new SettleInput(new() { new PaymentInput("cash", 1m, null, "USD") }), default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Insufficient payment*");
    }

    [Fact]
    public async Task Unknown_currency_is_rejected()
    {
        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "4", 1, null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);

        var act = async () => await orders.SettleAsync(o.Id, new SettleInput(new() { new PaymentInput("cash", 10m, null, "EUR") }), default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*EUR is not configured*");
    }

    [Fact]
    public async Task Tour_operator_commission_is_booked_at_settle()
    {
        Guid opId;
        await using (var seed = fx.NewTenantContext())
        {
            var op = new TourOperator { Code = "JETWING", Name = "Jetwing Travels", CommissionPercent = 10m, IsActive = true };
            seed.TourOperators.Add(op);
            await seed.SaveChangesAsync();
            opId = op.Id;
        }

        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "5", 4, null,
            TourOperatorId: opId), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);
        var total = (await orders.GetAsync(o.Id, default))!.TotalAmount;

        await orders.SettleAsync(o.Id, new SettleInput(new() { new PaymentInput("cash", total, null) }), default);

        var got = (await orders.GetAsync(o.Id, default))!;
        got.TourCommissionAmount.Should().Be(Math.Round(total * 0.10m, 4), "10% commission off the net bill");
    }

    [Fact]
    public async Task Steward_sales_report_attributes_bills_and_covers()
    {
        Guid stewardId;
        await using (var seed = fx.NewTenantContext())
        {
            // A steward IS a user flagged is_server (#76) — no separate master.
            var st = new User { DisplayName = "Nuwan", IsServer = true, IsActive = true, Role = UserRole.Cashier };
            seed.Users.Add(st);
            await seed.SaveChangesAsync();
            stewardId = st.Id;
        }

        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "6", 4, null,
            StewardId: stewardId), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);
        var total = (await orders.GetAsync(o.Id, default))!.TotalAmount;
        await orders.SettleAsync(o.Id, new SettleInput(new() { new PaymentInput("cash", total, null) }), default);

        var report = await orders.StewardSalesAsync(null, null, null, default);
        var row = report.Should().ContainSingle(r => r.StewardId == stewardId).Subject;
        row.Name.Should().Be("Nuwan");
        row.OrderCount.Should().Be(1);
        row.Covers.Should().Be(4);
        row.GrossSales.Should().Be(total, "no tip ⇒ gross == total");
    }

    [Fact]
    public async Task SetMeta_rejects_an_unknown_steward()
    {
        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "7", 1, null), default);
        var act = async () => await orders.SetMetaAsync(o.Id, covers: 3, stewardId: Guid.NewGuid(), tourOperatorId: null, default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Steward not found*");
    }

    // ── #66 partial loyalty redemption (points + cash) ──
    private async Task<Guid> EnableLoyaltyWithCustomerAsync(decimal points)
    {
        await using var db = fx.NewTenantContext();
        var os = await db.OrgSettings.FirstAsync();
        os.LoyaltyEnabled = true; os.LoyaltyRedeemValue = 1m; os.LoyaltyEarnRate = 0m;   // earn off → points only move on redemption
        var c = new Customer { Code = "LP1", Name = "Loyalty Guest", LoyaltyPoints = points, LoyaltyLifetimePoints = points, IsActive = true };
        db.Customers.Add(c);
        await db.SaveChangesAsync();
        return c.Id;
    }

    [Fact]
    public async Task Partial_points_redemption_takes_the_balance_as_cash()
    {
        var custId = await EnableLoyaltyWithCustomerAsync(200m);
        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "8", 1, null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);
        await orders.AttachCustomerAsync(o.Id, custId, default);
        var total = (await orders.GetAsync(o.Id, default))!.TotalAmount;

        // Redeem 150 points (worth LKR 150) + cash for the rest of a ~1,197 bill.
        await orders.SettleAsync(o.Id, new SettleInput(new()
            { new PaymentInput("loyalty", 150m, null), new PaymentInput("cash", total - 150m, null) }), default);

        (await orders.GetAsync(o.Id, default))!.Status.Should().Be("settled");
        await using var db = fx.NewTenantContext();
        (await db.Customers.FirstAsync(c => c.Id == custId)).LoyaltyPoints.Should().Be(50m, "200 − 150 redeemed");
    }

    [Fact]
    public async Task Redeeming_more_points_than_held_is_rejected()
    {
        var custId = await EnableLoyaltyWithCustomerAsync(50m);
        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "9", 1, null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);
        await orders.AttachCustomerAsync(o.Id, custId, default);
        var total = (await orders.GetAsync(o.Id, default))!.TotalAmount;

        var act = async () => await orders.SettleAsync(o.Id, new SettleInput(new()
            { new PaymentInput("loyalty", 100m, null), new PaymentInput("cash", total - 100m, null) }), default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Not enough points*");
    }
}
