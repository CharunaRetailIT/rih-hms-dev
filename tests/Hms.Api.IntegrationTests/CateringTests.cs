using FluentAssertions;
using Hms.Api.Domain;
using Hms.Api.Features.Catering;
using Hms.Api.Features.Production;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Hms.Api.IntegrationTests;

/// <summary>#75 Catering — function billing (pax × package + extras − discount), deposits, hall double-booking.</summary>
[Collection("pg")]
public class CateringTests(PostgresFixture fx) : IAsyncLifetime
{
    public Task InitializeAsync() => fx.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    CateringService Svc() => new(fx.Factory(), new ProductionService(fx.Factory()));
    static DateTime At(int h) => new DateTime(2026, 7, 1, h, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Producing_an_event_consumes_recipe_ingredients_and_records_food_cost()
    {
        // Recipe: Chicken Kottu output, yields 10 servings, uses 1 Lion Lager per batch.
        await using (var seed = fx.NewTenantContext())
        {
            seed.Recipes.Add(new Recipe
            {
                ProductId = fx.ChickenKottuId, YieldQuantity = 10, IsActive = true,
                Lines = { new RecipeLine { IngredientProductId = fx.LionLagerId, Sku = "BEV-LION", ProductName = "Lion Lager", Quantity = 1 } },
            });
            await seed.SaveChangesAsync();
        }
        decimal lagerBefore;
        await using (var db = fx.NewTenantContext())
            lagerBefore = await db.ProductStocks.Where(s => s.ProductId == fx.LionLagerId && s.LocationId == fx.LocationId).Select(s => s.QuantityOnHand).FirstAsync();

        var svc = Svc();
        var pkg = await svc.SavePackageAsync(null, "BUF", "Buffet", 3500m, null, true, fx.ChickenKottuId, default);
        var ev = await svc.SaveEventAsync(new SaveEventInput(null, "Wedding", fx.LocationId, null, pkg.Id, null, "X", null,
            20, At(18), At(23), null, null, null, false, null, null, null, null), default);

        ev = await svc.ProduceEventAsync(ev.Id, default);
        ev.ProducedAt.Should().NotBeNull();
        ev.ProductionOrderId.Should().NotBeNull();
        ev.FoodCost.Should().BeGreaterThan(0m, "ingredients were consumed at cost");

        await using var db2 = fx.NewTenantContext();
        var lagerAfter = await db2.ProductStocks.Where(s => s.ProductId == fx.LionLagerId && s.LocationId == fx.LocationId).Select(s => s.QuantityOnHand).FirstAsync();
        (lagerBefore - lagerAfter).Should().Be(2m, "20 pax / yield 10 = 2 batches × 1 lager");

        // Producing twice is blocked.
        var again = async () => await svc.ProduceEventAsync(ev.Id, default);
        await again.Should().ThrowAsync<InvalidOperationException>().WithMessage("*already been produced*");
    }

    [Fact]
    public async Task Function_bill_is_pax_times_package_plus_extras_minus_discount()
    {
        var svc = Svc();
        var hall = await svc.SaveHallAsync(null, "BALL", "Grand Ballroom", 300, null, true, null, default);
        var pkg = await svc.SavePackageAsync(null, "BUF-A", "Buffet A", 3500m, null, true, null, default);

        var ev = await svc.SaveEventAsync(new SaveEventInput(null, "Perera Wedding", null, hall.Id, pkg.Id,
            null, "Mr Perera", "0771234567", 100, At(18), At(23), DiscountAmount: 25000m, null, null,
            false, null, null, null, null), default);

        ev.PackageTotal.Should().Be(350000m, "100 pax × 3,500");
        ev.TotalAmount.Should().Be(325000m, "350,000 − 25,000 discount");

        ev = await svc.AddItemAsync(ev.Id, "Stage & décor", 1, 50000m, default);
        ev.ExtrasTotal.Should().Be(50000m);
        ev.TotalAmount.Should().Be(375000m);

        ev = await svc.AddPaymentAsync(ev.Id, 100000m, "bank", "deposit", "DEP-1", default);
        ev.PaidAmount.Should().Be(100000m);
        (ev.TotalAmount - ev.PaidAmount).Should().Be(275000m, "balance after the deposit");
    }

    [Fact]
    public async Task Editing_an_existing_booking_updates_pax_and_recomputes_the_bill()
    {
        var svc = Svc();
        var pkg = await svc.SavePackageAsync(null, "BUF-E", "Buffet E", 3500m, null, true, null, default);
        var ev = await svc.SaveEventAsync(new SaveEventInput(null, "Edit me", null, null, pkg.Id, null, null, null,
            100, At(18), At(23), null, null, null, false, null, null, null, null), default);
        ev.PackageTotal.Should().Be(350000m, "100 × 3,500");

        // Edit: same id, drop the head-count to 50 — the bill must recompute.
        ev = await svc.SaveEventAsync(new SaveEventInput(ev.Id, "Edit me", null, null, pkg.Id, null, null, null,
            50, At(18), At(23), null, null, null, false, null, null, null, null), default);
        ev.Pax.Should().Be(50);
        ev.PackageTotal.Should().Be(175000m, "50 × 3,500 after the edit");
    }

    [Fact]
    public async Task An_extra_requires_a_positive_price()
    {
        var svc = Svc();
        var ev = await svc.SaveEventAsync(new SaveEventInput(null, "Has extras", null, null, null, null, null, null,
            10, At(18), At(22), null, null, null, false, null, null, null, null), default);
        var noPrice = async () => await svc.AddItemAsync(ev.Id, "Freebie", 1, 0m, default);
        await noPrice.Should().ThrowAsync<InvalidOperationException>().WithMessage("*price*");
        // A priced extra is accepted.
        ev = await svc.AddItemAsync(ev.Id, "Décor", 1, 5000m, default);
        ev.ExtrasTotal.Should().Be(5000m);
    }

    [Fact]
    public async Task A_hall_cannot_be_double_booked_for_an_overlapping_time()
    {
        var svc = Svc();
        var hall = await svc.SaveHallAsync(null, "LAWN", "Garden Lawn", 150, null, true, null, default);
        await svc.SaveEventAsync(new SaveEventInput(null, "Event A", null, hall.Id, null, null, "A", null,
            50, At(18), At(22), null, null, null, false, null, null, null, null), default);

        var clash = async () => await svc.SaveEventAsync(new SaveEventInput(null, "Event B", null, hall.Id, null, null, "B", null,
            40, At(20), At(23), null, null, null, false, null, null, null, null), default);
        await clash.Should().ThrowAsync<InvalidOperationException>().WithMessage("*already booked*");

        // A non-overlapping slot in the same hall is fine.
        var ok = await svc.SaveEventAsync(new SaveEventInput(null, "Event C", null, hall.Id, null, null, "C", null,
            30, new DateTime(2026, 7, 2, 18, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 2, 22, 0, 0, DateTimeKind.Utc),
            null, null, null, false, null, null, null, null), default);
        ok.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Booking_dates_are_validated_no_backdating_and_end_after_start()
    {
        var svc = Svc();
        // Start in the past is rejected on a new booking.
        var past = async () => await svc.SaveEventAsync(new SaveEventInput(null, "Past", null, null, null, null, "X", null,
            10, new DateTime(2020, 1, 1, 18, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, null, null, null), default);
        await past.Should().ThrowAsync<InvalidOperationException>().WithMessage("*past*");

        // End before start is rejected.
        var endBeforeStart = async () => await svc.SaveEventAsync(new SaveEventInput(null, "Bad", null, null, null, null, "X", null,
            10, At(20), At(18), null, null, null, false, null, null, null, null), default);
        await endBeforeStart.Should().ThrowAsync<InvalidOperationException>().WithMessage("*End must be after*");
    }

    [Fact]
    public async Task Booking_requires_a_name_and_a_positive_pax()
    {
        var svc = Svc();
        // pax 0 is rejected (the bug: a booking saved with 0 pax then couldn't be produced).
        var noPax = async () => await svc.SaveEventAsync(new SaveEventInput(null, "Wedding", null, null, null, null, "Cust", null,
            0, At(18), At(22), null, null, null, false, null, null, null, null), default);
        await noPax.Should().ThrowAsync<InvalidOperationException>().WithMessage("*pax*");

        // No title AND no customer name is rejected.
        var noName = async () => await svc.SaveEventAsync(new SaveEventInput(null, null, null, null, null, null, null, null,
            20, At(18), At(22), null, null, null, false, null, null, null, null), default);
        await noName.Should().ThrowAsync<InvalidOperationException>().WithMessage("*title or a customer*");

        // A title (or a customer) + pax ≥ 1 is accepted.
        var ok = await svc.SaveEventAsync(new SaveEventInput(null, "Valid", null, null, null, null, null, null,
            1, At(18), At(22), null, null, null, false, null, null, null, null), default);
        ok.Pax.Should().Be(1);
    }

    [Fact]
    public async Task Off_site_dispatch_status_can_be_tracked()
    {
        var svc = Svc();
        var ev = await svc.SaveEventAsync(new SaveEventInput(null, "Outdoor catering", null, null, null, null, "Office", null,
            200, At(11), At(15), null, null, null, true, "123 Galle Rd, Colombo 03", "Van WP-1234", "Sunil", null), default);
        ev.IsOffsite.Should().BeTrue();

        ev = await svc.SetDispatchAsync(ev.Id, "dispatched", default);
        ev.DispatchStatus.Should().Be("dispatched");
    }
}
