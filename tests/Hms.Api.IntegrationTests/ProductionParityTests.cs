using FluentAssertions;
using Hms.Api.Features.Production;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Hms.Api.IntegrationTests;

/// <summary>
/// Legacy-parity production features: custom/ad-hoc, draft→post, void/reverse,
/// multiple-product documents, multiple recipes per product, inter-location.
/// </summary>
[Collection("pg")]
public class ProductionParityTests(PostgresFixture fx) : IAsyncLifetime
{
    public Task InitializeAsync() => fx.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private ProductionService Svc() => new(fx.Factory());

    private async Task<decimal> Qty(System.Guid product, System.Guid location)
    {
        await using var db = fx.NewTenantContext();
        var s = await db.ProductStocks.IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.ProductId == product && x.LocationId == location);
        return s?.QuantityOnHand ?? 0m;
    }

    [Fact]
    public async Task Custom_production_consumes_supplied_materials_without_a_recipe()
    {
        var svc = Svc();
        var po = await svc.ProduceCustomAsync(new CustomProduceInput(
            fx.LocationId, fx.ChickenKottuId, 1, new() { new CustomLineInput(fx.LionLagerId, 2) }), default);

        po.IsCustom.Should().BeTrue();
        po.Status.Should().Be("posted");
        (await Qty(fx.LionLagerId, fx.LocationId)).Should().Be(48m);   // 50 − 2
    }

    [Fact]
    public async Task Draft_does_not_move_stock_until_posted()
    {
        var svc = Svc();
        await svc.SetRecipeAsync(new SetRecipeInput(fx.ChickenKottuId, 1, null,
            new() { new RecipeLineInput(fx.LionLagerId, 2) }), default);

        var draft = await svc.ProduceAsync(new ProduceInput(fx.LocationId, fx.ChickenKottuId, 1, null, Post: false), default);
        draft.Status.Should().Be("draft");
        (await Qty(fx.LionLagerId, fx.LocationId)).Should().Be(50m);   // unchanged

        var posted = await svc.PostAsync(draft.Id, default);
        posted.Status.Should().Be("posted");
        (await Qty(fx.LionLagerId, fx.LocationId)).Should().Be(48m);   // now issued
    }

    [Fact]
    public async Task Void_reverses_a_posted_order()
    {
        var svc = Svc();
        await svc.SetRecipeAsync(new SetRecipeInput(fx.ChickenKottuId, 1, null,
            new() { new RecipeLineInput(fx.LionLagerId, 2) }), default);
        var po = await svc.ProduceAsync(new ProduceInput(fx.LocationId, fx.ChickenKottuId, 1, null), default);
        (await Qty(fx.LionLagerId, fx.LocationId)).Should().Be(48m);

        var v = await svc.VoidAsync(po.Id, "mistake", default);
        v.Status.Should().Be("void");
        (await Qty(fx.LionLagerId, fx.LocationId)).Should().Be(50m);   // materials returned
    }

    [Fact]
    public async Task Multiple_product_document_shares_a_number()
    {
        var svc = Svc();
        await svc.SetRecipeAsync(new SetRecipeInput(fx.ChickenKottuId, 1, null, new() { new RecipeLineInput(fx.LionLagerId, 2) }), default);
        await svc.SetRecipeAsync(new SetRecipeInput(fx.RiceId, 1, null, new() { new RecipeLineInput(fx.LionLagerId, 1) }), default);

        var orders = await svc.ProduceBatchAsync(new BatchProduceInput(fx.LocationId, new()
        {
            new BatchItemInput(fx.ChickenKottuId, 1),
            new BatchItemInput(fx.RiceId, 1),
        }), default);

        orders.Should().HaveCount(2);
        orders.Select(o => o.DocumentNo).Distinct().Should().HaveCount(1);
        orders[0].DocumentNo.Should().StartWith("PB-");
        (await Qty(fx.LionLagerId, fx.LocationId)).Should().Be(47m);   // 50 − 2 − 1
    }

    [Fact]
    public async Task A_product_can_have_multiple_recipes_keyed_by_output_unit()
    {
        var svc = Svc();
        await svc.SetRecipeAsync(new SetRecipeInput(fx.ChickenKottuId, 1, null, new() { new RecipeLineInput(fx.LionLagerId, 2) }), default);
        await svc.SetRecipeAsync(new SetRecipeInput(fx.ChickenKottuId, 1, null, new() { new RecipeLineInput(fx.LionLagerId, 3) }, OutputUnitId: fx.GramUnitId), default);

        var recipes = await svc.ListRecipesAsync(default);
        recipes.Count(r => r.ProductId == fx.ChickenKottuId).Should().Be(2);

        // Producing by the gram-keyed recipe consumes 3 Lion; the default consumes 2.
        await svc.ProduceAsync(new ProduceInput(fx.LocationId, fx.ChickenKottuId, 1, null, OutputUnitId: fx.GramUnitId), default);
        (await Qty(fx.LionLagerId, fx.LocationId)).Should().Be(47m);   // 50 − 3
        await svc.ProduceAsync(new ProduceInput(fx.LocationId, fx.ChickenKottuId, 1, null), default);
        (await Qty(fx.LionLagerId, fx.LocationId)).Should().Be(45m);   // − 2
    }

    [Fact]
    public async Task Inter_location_consumes_here_and_delivers_finished_elsewhere()
    {
        var svc = Svc();
        await svc.SetRecipeAsync(new SetRecipeInput(fx.ChickenKottuId, 1, null, new() { new RecipeLineInput(fx.LionLagerId, 2) }), default);

        await svc.ProduceAsync(new ProduceInput(fx.LocationId, fx.ChickenKottuId, 1, null,
            ReceiptLocationId: fx.CentralKitchenId), default);

        (await Qty(fx.LionLagerId, fx.LocationId)).Should().Be(48m);          // consumed at MAIN
        (await Qty(fx.ChickenKottuId, fx.CentralKitchenId)).Should().Be(1m);  // finished landed at CK
    }
}
