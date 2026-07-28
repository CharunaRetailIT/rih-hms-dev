using FluentAssertions;
using Hms.Api.Features.Modifiers;
using Hms.Api.Features.Orders;
using Xunit;

namespace Hms.Api.IntegrationTests;

/// <summary>
/// Modifiers / add-ons: groups + items, attach to a product, and pricing the
/// selections into an order line (with required/min/max validation).
/// </summary>
[Collection("pg")]
public class ModifierTests(PostgresFixture fx) : IAsyncLifetime
{
    public Task InitializeAsync() => fx.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private ModifierService Mod() => new(fx.Factory());

    [Fact]
    public async Task Selected_modifier_adds_its_delta_to_the_line_price()
    {
        var mod = Mod();
        var grp = await mod.SetGroupAsync(new SetModifierGroupInput(null, "Add-ons", 0, 0, false, 0,
            new() { new ModifierItemInput("Extra cheese", 150m), new ModifierItemInput("Bacon", 200m) }), default);
        await mod.SetProductGroupsAsync(fx.ChickenKottuId, new() { grp.Id }, default);
        var cheeseId = grp.Items.First(i => i.Name == "Extra cheese").Id;

        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", "pos", null, "1", 1, null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null,
            ModifierItemIds: new() { cheeseId }), default);

        var line = (await orders.GetAsync(o.Id, default))!.Items.Single();
        line.UnitPrice.Should().Be(1050m);   // Chicken Kottu 900 + cheese 150
        line.Modifiers.Should().ContainSingle(m => m.Name == "Extra cheese" && m.PriceDelta == 150m);
    }

    [Fact]
    public async Task Product_modifiers_endpoint_returns_attached_groups()
    {
        var mod = Mod();
        var grp = await mod.SetGroupAsync(new SetModifierGroupInput(null, "Spice", 1, 1, true, 0,
            new() { new ModifierItemInput("Mild", 0m), new ModifierItemInput("Hot", 0m) }), default);
        await mod.SetProductGroupsAsync(fx.ChickenKottuId, new() { grp.Id }, default);

        var groups = await mod.GetProductModifiersAsync(fx.ChickenKottuId, default);
        groups.Should().ContainSingle(g => g.Name == "Spice");
        groups.Single().Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task Required_group_must_be_chosen()
    {
        var mod = Mod();
        var grp = await mod.SetGroupAsync(new SetModifierGroupInput(null, "Size", 1, 1, true, 0,
            new() { new ModifierItemInput("Regular", 0m), new ModifierItemInput("Large", 100m) }), default);
        await mod.SetProductGroupsAsync(fx.LionLagerId, new() { grp.Id }, default);

        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", "pos", null, "1", 1, null), default);
        var act = () => orders.AddItemAsync(o.Id, new AddItemInput(fx.LionLagerId, 1, "bar", null), default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*required*");
    }

    [Fact]
    public async Task Exceeding_max_select_is_rejected()
    {
        var mod = Mod();
        var grp = await mod.SetGroupAsync(new SetModifierGroupInput(null, "Sauce", 0, 1, false, 0,
            new() { new ModifierItemInput("Ketchup", 0m), new ModifierItemInput("Mayo", 0m) }), default);
        await mod.SetProductGroupsAsync(fx.ChickenKottuId, new() { grp.Id }, default);
        var ids = grp.Items.Select(i => i.Id).ToList();   // both — exceeds max 1

        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", "pos", null, "1", 1, null), default);
        var act = () => orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null, ModifierItemIds: ids), default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*at most*");
    }

    [Fact]
    public async Task Modifier_not_attached_to_the_product_is_rejected()
    {
        var mod = Mod();
        // group exists but is NOT attached to Chicken Kottu
        var grp = await mod.SetGroupAsync(new SetModifierGroupInput(null, "Orphan", 0, 0, false, 0,
            new() { new ModifierItemInput("Thing", 50m) }), default);
        var itemId = grp.Items.First().Id;

        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", "pos", null, "1", 1, null), default);
        var act = () => orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null, ModifierItemIds: new() { itemId }), default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Invalid modifier*");
    }
}
