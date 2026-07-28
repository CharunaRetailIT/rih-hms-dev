using FluentAssertions;
using Hms.Api.Domain;
using Hms.Api.Features.Orders;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Hms.Api.IntegrationTests;

/// <summary>#69 merge bills + transfer table.</summary>
[Collection("pg")]
public class MergeTransferTests(PostgresFixture fx) : IAsyncLifetime
{
    public Task InitializeAsync() => fx.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<Guid> Table(string code)
    {
        await using var db = fx.NewTenantContext();
        var t = new RestaurantTable { LocationId = fx.LocationId, Code = code, Seats = 4 };
        db.RestaurantTables.Add(t); await db.SaveChangesAsync(); return t.Id;
    }

    [Fact]
    public async Task Split_moves_chosen_quantities_to_a_new_bill()
    {
        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "S", 4, null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 3, "kitchen", null), default);   // qty 3
        var line = (await orders.GetAsync(o.Id, default))!.Items.First(i => !i.IsDeleted);

        var dest = await orders.SplitAsync(o.Id, new List<SplitMove> { new(line.Id, 1) }, default);   // split off 1
        dest.Status.Should().Be("open");
        dest.Items.Where(i => !i.IsDeleted).Sum(i => i.Quantity).Should().Be(1);

        var src = (await orders.GetAsync(o.Id, default))!;
        src.Items.Where(i => !i.IsDeleted).Sum(i => i.Quantity).Should().Be(2, "the rest stays on the original bill");
        dest.Id.Should().NotBe(o.Id);
    }

    [Fact]
    public async Task Merge_moves_lines_and_voids_the_source()
    {
        var (orders, _) = fx.Services();
        var a = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "A", 2, null), default);
        await orders.AddItemAsync(a.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);  // 900
        var b = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "B", 2, null), default);
        await orders.AddItemAsync(b.Id, new AddItemInput(fx.ChickenKottuId, 2, "kitchen", null), default);  // 1800

        var merged = await orders.MergeAsync(a.Id, b.Id, default);
        merged.SubtotalAmount.Should().Be(2700m);   // 900 + 1800

        var full = (await orders.GetAsync(a.Id, default))!;
        full.Items.Count(i => !i.IsDeleted).Should().Be(2);

        await using var db = fx.NewTenantContext();
        var src = await db.Orders.IgnoreQueryFilters().FirstAsync(o => o.Id == b.Id);
        src.Status.Should().Be("void");
        src.VoidReason.Should().Contain(merged.OrderNumber);
    }

    [Fact]
    public async Task Cannot_merge_into_self()
    {
        var (orders, _) = fx.Services();
        var a = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "A", 1, null), default);
        var act = () => orders.MergeAsync(a.Id, a.Id, default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*itself*");
    }

    [Fact]
    public async Task Transfer_moves_to_an_empty_table_but_not_an_occupied_one()
    {
        var t1 = await Table("M1");
        var t2 = await Table("M2");
        var (orders, _) = fx.Services();

        var a = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "A", 2, null, TableId: t1), default);
        await orders.AddItemAsync(a.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);

        // move A from M1 → M2 (empty) — ok
        var moved = await orders.TransferTableAsync(a.Id, t2, default);
        moved.TableId.Should().Be(t2);
        moved.TableLabel.Should().Be("M2");

        // open B on M1, then try to move A back onto M1 — blocked
        var b = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "B", 2, null, TableId: t1), default);
        await orders.AddItemAsync(b.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);
        var act = () => orders.TransferTableAsync(a.Id, t1, default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*occupied*");
    }
}
