using FluentAssertions;
using Hms.Api.Domain;
using Hms.Api.Features.Orders;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Hms.Api.IntegrationTests;

/// <summary>
/// CRM #70: attaching a customer applies their (or their category's) default
/// discount, and credit ("charge to account") settlement moves money onto the
/// customer's AR balance, bounded by their limit.
/// </summary>
[Collection("pg")]
public class CustomerTests(PostgresFixture fx) : IAsyncLifetime
{
    public Task InitializeAsync() => fx.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<Guid> NewCustomer(Action<Customer> cfg)
    {
        await using var db = fx.NewTenantContext();
        var c = new Customer { Code = ("C" + Guid.NewGuid().ToString("N"))[..8].ToUpperInvariant(), Name = "Test Customer" };
        cfg(c);
        db.Customers.Add(c);
        await db.SaveChangesAsync();
        return c.Id;
    }

    private async Task<Guid> NewCategory(decimal discountPct)
    {
        await using var db = fx.NewTenantContext();
        var cat = new CustomerCategory { Code = ("CAT" + Guid.NewGuid().ToString("N"))[..8].ToUpperInvariant(), Name = "Group", DiscountPercent = discountPct };
        db.CustomerCategories.Add(cat);
        await db.SaveChangesAsync();
        return cat.Id;
    }

    [Fact]
    public async Task Attaching_a_customer_applies_their_default_discount()
    {
        var custId = await NewCustomer(c => c.DiscountPercent = 10m);
        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "1", 1, null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);   // 900 subtotal

        var attached = await orders.AttachCustomerAsync(o.Id, custId, default);
        attached.CustomerName.Should().Be("Test Customer");
        attached.DiscountAmount.Should().Be(90m, "10% of the 900 subtotal");
    }

    [Fact]
    public async Task Category_discount_applies_when_the_customer_has_no_own_percent()
    {
        var catId = await NewCategory(15m);
        var custId = await NewCustomer(c => { c.CategoryId = catId; c.DiscountPercent = null; });
        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "2", 1, null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);

        var attached = await orders.AttachCustomerAsync(o.Id, custId, default);
        attached.DiscountAmount.Should().Be(135m, "15% category discount on 900");
    }

    [Fact]
    public async Task Credit_settlement_increases_the_customers_balance()
    {
        var custId = await NewCustomer(c => { c.IsCreditCustomer = true; c.CreditLimit = 100_000m; });
        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "3", 1, null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);
        await orders.AttachCustomerAsync(o.Id, custId, default);
        var total = (await orders.GetAsync(o.Id, default))!.TotalAmount;

        var settled = await orders.SettleAsync(o.Id, new SettleInput(new() { new PaymentInput("credit", total, null) }), default);
        settled.Status.Should().Be("settled");

        await using var db = fx.NewTenantContext();
        (await db.Customers.FirstAsync(c => c.Id == custId)).CurrentBalance.Should().Be(total);
    }

    [Fact]
    public async Task Credit_beyond_the_limit_is_rejected()
    {
        var custId = await NewCustomer(c => { c.IsCreditCustomer = true; c.CreditLimit = 100m; });
        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "4", 1, null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);
        await orders.AttachCustomerAsync(o.Id, custId, default);
        var total = (await orders.GetAsync(o.Id, default))!.TotalAmount;   // ~1170, over the 100 limit

        var act = () => orders.SettleAsync(o.Id, new SettleInput(new() { new PaymentInput("credit", total, null) }), default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Credit limit exceeded*");
    }

    private async Task EnableLoyalty(decimal earnRate, decimal redeemValue)
    {
        await using var db = fx.NewTenantContext();
        await db.Database.ExecuteSqlRawAsync(
            $"UPDATE org_settings SET loyalty_enabled = true, loyalty_earn_rate = {earnRate.ToString(System.Globalization.CultureInfo.InvariantCulture)}, loyalty_redeem_value = {redeemValue.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
    }

    [Fact]
    public async Task Settling_earns_loyalty_points_for_the_attached_customer()
    {
        await EnableLoyalty(0.01m, 1m);   // 1 point per 100 LKR
        var custId = await NewCustomer(_ => { });
        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "L1", 1, null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);
        await orders.AttachCustomerAsync(o.Id, custId, default);
        var total = (await orders.GetAsync(o.Id, default))!.TotalAmount;
        await orders.SettleAsync(o.Id, new SettleInput(new() { new PaymentInput("cash", total, null) }), default);

        await using var db = fx.NewTenantContext();
        (await db.Customers.FirstAsync(c => c.Id == custId)).LoyaltyPoints.Should().Be(Math.Floor(total * 0.01m));
    }

    [Fact]
    public async Task Redeeming_points_as_a_loyalty_tender_deducts_the_balance()
    {
        await EnableLoyalty(0m, 1m);   // earn off to isolate redemption; 1 LKR per point
        var custId = await NewCustomer(_ => { });
        await using (var seed = fx.NewTenantContext())
        {
            var c = await seed.Customers.FirstAsync(x => x.Id == custId);
            c.LoyaltyPoints = 500m; await seed.SaveChangesAsync();
        }
        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "L2", 1, null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);
        await orders.AttachCustomerAsync(o.Id, custId, default);
        var total = (await orders.GetAsync(o.Id, default))!.TotalAmount;
        // 100 points (= LKR 100) toward the bill, cash for the rest.
        await orders.SettleAsync(o.Id, new SettleInput(new() { new PaymentInput("loyalty", 100m, null), new PaymentInput("cash", total - 100m, null) }), default);

        await using var db = fx.NewTenantContext();
        (await db.Customers.FirstAsync(c => c.Id == custId)).LoyaltyPoints.Should().Be(400m);
    }

    [Fact]
    public async Task Redeeming_more_points_than_the_balance_is_rejected()
    {
        await EnableLoyalty(0m, 1m);
        var custId = await NewCustomer(_ => { });
        await using (var seed = fx.NewTenantContext())
        {
            var c = await seed.Customers.FirstAsync(x => x.Id == custId);
            c.LoyaltyPoints = 50m; await seed.SaveChangesAsync();
        }
        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "L3", 1, null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);
        await orders.AttachCustomerAsync(o.Id, custId, default);
        var total = (await orders.GetAsync(o.Id, default))!.TotalAmount;
        var act = () => orders.SettleAsync(o.Id, new SettleInput(new() { new PaymentInput("loyalty", 500m, null), new PaymentInput("cash", total - 500m, null) }), default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Not enough points*");
    }

    [Fact]
    public async Task Charging_to_account_without_a_credit_customer_is_rejected()
    {
        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "5", 1, null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);
        var total = (await orders.GetAsync(o.Id, default))!.TotalAmount;

        var act = () => orders.SettleAsync(o.Id, new SettleInput(new() { new PaymentInput("credit", total, null) }), default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*credit customer*");
    }

    // ── #66 loyalty card lookup ────────────────────────────────────────────────
    [Fact]
    public async Task Customer_can_be_found_by_loyalty_card_number()
    {
        var custId = await NewCustomer(c => c.LoyaltyCardNo = "CARD-9001");
        await using var db = fx.NewTenantContext();
        var found = await db.Customers.FirstOrDefaultAsync(c => c.LoyaltyCardNo == "CARD-9001");
        found.Should().NotBeNull();
        found!.Id.Should().Be(custId);
    }

    // ── #70 advance / deposit ──────────────────────────────────────────────────
    [Fact]
    public async Task Advance_deposit_can_be_drawn_as_a_tender_and_is_bounded()
    {
        var custId = await NewCustomer(_ => { });
        await using (var db = fx.NewTenantContext())
        {
            var c = await db.Customers.FirstAsync(x => x.Id == custId);
            c.AdvanceBalance = 2000m; await db.SaveChangesAsync();
        }
        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "AD", 1, null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);   // 900 + tax/svc
        await orders.AttachCustomerAsync(o.Id, custId, default);
        var total = (await orders.GetAsync(o.Id, default))!.TotalAmount;

        await orders.SettleAsync(o.Id, new SettleInput(new() { new PaymentInput("advance", total, null) }), default);
        (await fx.NewTenantContext().Customers.FirstAsync(c => c.Id == custId)).AdvanceBalance.Should().Be(2000m - total);
    }

    [Fact]
    public async Task Drawing_more_than_the_advance_balance_is_rejected()
    {
        var custId = await NewCustomer(_ => { });
        await using (var db = fx.NewTenantContext())
        {
            var c = await db.Customers.FirstAsync(x => x.Id == custId);
            c.AdvanceBalance = 100m; await db.SaveChangesAsync();
        }
        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "AD2", 1, null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);
        await orders.AttachCustomerAsync(o.Id, custId, default);
        var total = (await orders.GetAsync(o.Id, default))!.TotalAmount;
        var act = () => orders.SettleAsync(o.Id, new SettleInput(new() { new PaymentInput("advance", total, null) }), default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Not enough advance*");
    }

    // ── #70 per-product contract pricing ───────────────────────────────────────
    private async Task SetCustomerPrice(Guid customerId, Guid productId, decimal price)
    {
        await using var db = fx.NewTenantContext();
        db.CustomerProductPrices.Add(new CustomerProductPrice { CustomerId = customerId, ProductId = productId, Price = price });
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task Customer_contract_price_reprices_the_bill_on_attach_and_reverts_on_detach()
    {
        var custId = await NewCustomer(_ => { });
        await SetCustomerPrice(custId, fx.ChickenKottuId, 800m);   // base is 900

        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "20", 1, null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 2, "kitchen", null), default);
        (await orders.GetAsync(o.Id, default))!.SubtotalAmount.Should().Be(1800m);   // base price before attach

        await orders.AttachCustomerAsync(o.Id, custId, default);
        (await orders.GetAsync(o.Id, default))!.SubtotalAmount.Should().Be(1600m);   // 2 × 800 contract

        await orders.DetachCustomerAsync(o.Id, default);
        (await orders.GetAsync(o.Id, default))!.SubtotalAmount.Should().Be(1800m);   // reverts to base
    }

    [Fact]
    public async Task Item_added_after_attach_uses_the_contract_price()
    {
        var custId = await NewCustomer(_ => { });
        await SetCustomerPrice(custId, fx.ChickenKottuId, 800m);

        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "21", 1, null), default);
        await orders.AttachCustomerAsync(o.Id, custId, default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);

        (await orders.GetAsync(o.Id, default))!.Items.Single(i => i.ProductName == "Chicken Kottu").UnitPrice.Should().Be(800m);
    }

    // ── #66 loyalty tiers + expiry ─────────────────────────────────────────────
    [Fact]
    public async Task Tier_earn_multiplier_boosts_points_earned()
    {
        await EnableLoyalty(0.01m, 1m);   // 1 point / 100 LKR base
        var custId = await NewCustomer(c => c.LoyaltyLifetimePoints = 1000m);   // already Gold
        await using (var db = fx.NewTenantContext())
        {
            db.LoyaltyTiers.Add(new LoyaltyTier { Name = "Gold", MinLifetimePoints = 1000m, EarnMultiplier = 2m });
            await db.SaveChangesAsync();
        }
        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "T1", 1, null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);
        await orders.AttachCustomerAsync(o.Id, custId, default);
        var total = (await orders.GetAsync(o.Id, default))!.TotalAmount;
        await orders.SettleAsync(o.Id, new SettleInput(new() { new PaymentInput("cash", total, null) }), default);

        await using var check = fx.NewTenantContext();
        (await check.Customers.FirstAsync(c => c.Id == custId)).LoyaltyPoints.Should().Be(Math.Floor(total * 0.01m * 2m));   // ×2 Gold
    }

    [Fact]
    public async Task Stale_points_expire_before_redemption()
    {
        await EnableLoyalty(0m, 1m);   // earn off
        await using (var db = fx.NewTenantContext())
            await db.Database.ExecuteSqlRawAsync("UPDATE org_settings SET loyalty_expiry_days = 30");
        var custId = await NewCustomer(_ => { });
        await using (var seed = fx.NewTenantContext())
        {
            var c = await seed.Customers.FirstAsync(x => x.Id == custId);
            c.LoyaltyPoints = 500m;
            c.LoyaltyLastActivityAt = DateTime.UtcNow.AddDays(-60);   // last active 60 days ago > 30
            await seed.SaveChangesAsync();
        }
        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "T2", 1, null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);
        await orders.AttachCustomerAsync(o.Id, custId, default);
        var total = (await orders.GetAsync(o.Id, default))!.TotalAmount;
        // points are stale → expired → redemption now fails (nothing to redeem)
        var act = () => orders.SettleAsync(o.Id, new SettleInput(new() { new PaymentInput("loyalty", 100m, null), new PaymentInput("cash", total - 100m, null) }), default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Not enough points*");
    }

    [Fact]
    public async Task Category_contract_price_applies_when_no_customer_override()
    {
        var catId = await NewCategory(0m);
        var custId = await NewCustomer(c => c.CategoryId = catId);
        await using (var db = fx.NewTenantContext())
        {
            db.CustomerProductPrices.Add(new CustomerProductPrice { CategoryId = catId, ProductId = fx.ChickenKottuId, Price = 850m });
            await db.SaveChangesAsync();
        }

        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "22", 1, null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);
        await orders.AttachCustomerAsync(o.Id, custId, default);
        (await orders.GetAsync(o.Id, default))!.SubtotalAmount.Should().Be(850m);
    }
}
