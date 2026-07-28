using FluentAssertions;
using Hms.Api.Domain;
using Hms.Api.Features.Orders;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Hms.Api.IntegrationTests;

/// <summary>
/// #65 promotions engine — auto-applied discounts netted off the bill.
/// Covers product_discount (% off a product), bill_value (spend-and-save),
/// inactive/scoped promos being skipped, and the snapshot on settle.
/// </summary>
[Collection("pg")]
public class PromotionTests(PostgresFixture fx) : IAsyncLifetime
{
    public Task InitializeAsync() => fx.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private async Task AddPromo(Promotion p)
    {
        await using var db = fx.NewTenantContext();
        db.Promotions.Add(p);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task Product_discount_promo_reduces_the_bill()
    {
        // 10% off Chicken Kottu, always-on, auto-apply
        await AddPromo(new Promotion
        {
            Code = "KOTTU10", Name = "10% off Kottu", PromoType = "product_discount", IsActive = true, AutoApply = true,
            Lines = { new PromotionLine { ProductId = fx.ChickenKottuId, DiscountPercent = 10m } },
        });

        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "1", 1, null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 2, "kitchen", null), default);   // 1800

        var got = (await orders.GetAsync(o.Id, default))!;
        got.SubtotalAmount.Should().Be(1800m);
        got.PromotionDiscountAmount.Should().Be(180m);   // 10% of 1,800
        // total = subtotal + svc + tax − discount − promo  (promo is netted like a manual discount)
        got.TotalAmount.Should().Be(1800m + got.ServiceChargeAmount + got.TaxAmount - 180m);
    }

    [Fact]
    public async Task Bill_value_promo_applies_above_threshold()
    {
        await AddPromo(new Promotion
        {
            Code = "SPEND5K", Name = "LKR 500 off over 5,000", PromoType = "bill_value", IsActive = true, AutoApply = true,
            Lines = { new PromotionLine { BillFrom = 5000m, DiscountAmount = 500m } },
        });

        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "2", 1, null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 4, "kitchen", null), default);   // 3600 — below

        (await orders.GetAsync(o.Id, default))!.PromotionDiscountAmount.Should().Be(0m);

        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 2, "kitchen", null), default);   // now 5400 — above
        (await orders.GetAsync(o.Id, default))!.PromotionDiscountAmount.Should().Be(500m);
    }

    [Fact]
    public async Task Inactive_or_wrong_order_type_promo_is_skipped()
    {
        await AddPromo(new Promotion
        {
            Code = "DELONLY", Name = "Delivery only", PromoType = "bill_value", IsActive = true, AutoApply = true,
            AppliesToOrderType = "delivery",
            Lines = { new PromotionLine { BillFrom = 0m, DiscountPercent = 50m } },
        });

        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "3", 1, null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);
        (await orders.GetAsync(o.Id, default))!.PromotionDiscountAmount.Should().Be(0m);   // dine-in ≠ delivery
    }

    [Fact]
    public async Task Bogo_same_product_makes_every_second_one_free()
    {
        // Buy 1 Kottu get 1 Kottu free (same product → group of 2).
        await AddPromo(new Promotion
        {
            Code = "BOGO-KOTTU", Name = "Buy 1 Kottu get 1 free", PromoType = "buy_x_get_y", IsActive = true, AutoApply = true,
            Lines = { new PromotionLine { ProductId = fx.ChickenKottuId, MinQty = 1, GetProductId = fx.ChickenKottuId, GetQty = 1 } },
        });

        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "10", 1, null), default);

        // qty 1 → no complete group → no discount
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);
        (await orders.GetAsync(o.Id, default))!.PromotionDiscountAmount.Should().Be(0m);

        // qty 3 → one group of 2 (1 free) + 1 paid → exactly one Kottu (900) free
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 2, "kitchen", null), default);
        (await orders.GetAsync(o.Id, default))!.PromotionDiscountAmount.Should().Be(900m);
    }

    [Fact]
    public async Task Bogo_cross_product_discounts_the_reward_item_only_when_present()
    {
        // Buy 1 Kottu (900) get 1 Lion (450) free.
        await AddPromo(new Promotion
        {
            Code = "KOTTU-FREE-BEER", Name = "Free beer with Kottu", PromoType = "buy_x_get_y", IsActive = true, AutoApply = true,
            Lines = { new PromotionLine { ProductId = fx.ChickenKottuId, MinQty = 1, GetProductId = fx.LionLagerId, GetQty = 1 } },
        });

        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "11", 1, null), default);

        // Kottu only → reward item not in cart → no discount
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);
        (await orders.GetAsync(o.Id, default))!.PromotionDiscountAmount.Should().Be(0m);

        // add the beer → 1 free Lion (450)
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.LionLagerId, 1, "bar", null), default);
        (await orders.GetAsync(o.Id, default))!.PromotionDiscountAmount.Should().Be(450m);
    }

    [Fact]
    public async Task Bundle_charges_the_combo_price_for_the_parts()
    {
        // Combo: 1 Kottu (900) + 1 Lion (450) = 1,350 normally → bundle for 1,200 ⇒ 150 off.
        await AddPromo(new Promotion
        {
            Code = "COMBO", Name = "Kottu + Beer combo", PromoType = "bundle", IsActive = true, AutoApply = true,
            Lines =
            {
                new PromotionLine { ProductId = fx.ChickenKottuId, MinQty = 1, BundlePrice = 1200m },
                new PromotionLine { ProductId = fx.LionLagerId, MinQty = 1, BundlePrice = 1200m },
            },
        });

        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "12", 1, null), default);

        // only one component → no bundle
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);
        (await orders.GetAsync(o.Id, default))!.PromotionDiscountAmount.Should().Be(0m);

        // both components present → one bundle → 150 off
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.LionLagerId, 1, "bar", null), default);
        (await orders.GetAsync(o.Id, default))!.PromotionDiscountAmount.Should().Be(150m);
    }

    [Fact]
    public async Task Lowest_price_promo_waves_the_cheapest_of_each_group()
    {
        await AddPromo(new Promotion
        {
            Code = "3FOR2", Name = "3 for 2", PromoType = "lowest_price", IsActive = true, AutoApply = true,
            Lines = { new PromotionLine { MinQty = 3, GetQty = 1 } },   // every 3 units → cheapest free
        });
        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "LP", 1, null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 3, "kitchen", null), default);   // 3 × 900
        (await orders.GetAsync(o.Id, default))!.PromotionDiscountAmount.Should().Be(900m);   // one Kottu free
    }

    [Fact]
    public async Task Customer_segment_promo_only_applies_for_that_category()
    {
        Guid catId, custId;
        await using (var db = fx.NewTenantContext())
        {
            var cat = new CustomerCategory { Code = "CORP", Name = "Corporate" };
            db.CustomerCategories.Add(cat); await db.SaveChangesAsync(); catId = cat.Id;
            var cust = new Customer { Code = "CC1", Name = "Corp Co", CategoryId = catId };
            db.Customers.Add(cust); await db.SaveChangesAsync(); custId = cust.Id;
        }
        await AddPromo(new Promotion
        {
            Code = "CORP10", Name = "Corporate 10%", PromoType = "bill_value", IsActive = true, AutoApply = true,
            AppliesToCategoryId = catId, Lines = { new PromotionLine { BillFrom = 0, DiscountPercent = 10 } },
        });

        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "SEG", 1, null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);   // 900
        (await orders.GetAsync(o.Id, default))!.PromotionDiscountAmount.Should().Be(0m);   // no customer → segment promo skipped

        await orders.AttachCustomerAsync(o.Id, custId, default);
        (await orders.GetAsync(o.Id, default))!.PromotionDiscountAmount.Should().Be(90m);   // corporate → 10% of 900
    }

    [Fact]
    public async Task Applied_promotions_are_snapshotted_on_settle()
    {
        await AddPromo(new Promotion
        {
            Code = "KOTTU10", Name = "10% off Kottu", PromoType = "product_discount", IsActive = true, AutoApply = true,
            Lines = { new PromotionLine { ProductId = fx.ChickenKottuId, DiscountPercent = 10m } },
        });

        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "4", 1, null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);
        var total = (await orders.GetAsync(o.Id, default))!.TotalAmount;
        await orders.SettleAsync(o.Id, new SettleInput(new() { new PaymentInput("cash", total, null) }), default);

        await using var db = fx.NewTenantContext();
        var snap = await db.OrderPromotions.IgnoreQueryFilters().Where(op => op.OrderId == o.Id).ToListAsync();
        snap.Should().ContainSingle(x => x.Code == "KOTTU10" && x.DiscountAmount == 90m);
    }
}
