using FluentAssertions;
using Hms.Api.Features.Orders;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Hms.Api.IntegrationTests;

/// <summary>#77 month-end lock: a closed period rejects posting + reversal into it.</summary>
[Collection("pg")]
public class PeriodLockTests(PostgresFixture fx) : IAsyncLifetime
{
    public Task InitializeAsync() => fx.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private async Task LockThrough(DateOnly through)
    {
        await using var db = fx.NewTenantContext();
        await db.Database.ExecuteSqlRawAsync($"UPDATE org_settings SET books_locked_through = DATE '{through:yyyy-MM-dd}'");
    }

    [Fact]
    public async Task Settling_into_a_closed_period_is_rejected()
    {
        await LockThrough(DateOnly.FromDateTime(DateTime.UtcNow.Date));   // close through today
        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "P1", 1, null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);
        var total = (await orders.GetAsync(o.Id, default))!.TotalAmount;

        var act = () => orders.SettleAsync(o.Id, new SettleInput(new() { new PaymentInput("cash", total, null) }), default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*period is closed*");
    }

    [Fact]
    public async Task Settling_is_allowed_when_only_a_past_period_is_closed()
    {
        await LockThrough(DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-1)));   // close through yesterday
        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "P2", 1, null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);
        var total = (await orders.GetAsync(o.Id, default))!.TotalAmount;

        await orders.SettleAsync(o.Id, new SettleInput(new() { new PaymentInput("cash", total, null) }), default);
        (await orders.GetAsync(o.Id, default))!.Status.Should().Be("settled");   // today is still open
    }
}
