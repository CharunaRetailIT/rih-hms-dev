using FluentAssertions;
using Hms.Api.Features.Production;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Hms.Api.IntegrationTests;

/// <summary>
/// Step 7: recipes + production. Uses Lion Lager (avg 220) as the ingredient for
/// a recipe producing Chicken Kottu, so the cost roll-up is unambiguous.
/// </summary>
[Collection("pg")]
public class ProductionTests(PostgresFixture fx) : IAsyncLifetime
{
    public Task InitializeAsync() => fx.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private ProductionService Svc() => new(fx.Factory());

    private async Task<decimal[]> Stock(System.Guid product)
    {
        await using var db = fx.NewTenantContext();
        var s = await db.ProductStocks.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.ProductId == product && x.LocationId == fx.LocationId);
        return s is null ? new[] { 0m, 0m } : new[] { s.QuantityOnHand, s.AverageCost };
    }

    [Fact]
    public async Task Set_recipe_then_get_returns_lines()
    {
        var svc = Svc();
        var r = await svc.SetRecipeAsync(new SetRecipeInput(fx.ChickenKottuId, 1, "test",
            new() { new RecipeLineInput(fx.LionLagerId, 2) }), default);
        r.Lines.Should().HaveCount(1);
        var got = await svc.GetRecipeAsync(fx.ChickenKottuId, null, default);
        got!.Lines.Single().Quantity.Should().Be(2m);
    }

    [Fact]
    public async Task Production_consumes_ingredients_and_yields_finished_with_rolled_up_cost()
    {
        var svc = Svc();
        // Recipe: 1 Chicken Kottu = 2 Lion Lager (Lion avg cost 220).
        await svc.SetRecipeAsync(new SetRecipeInput(fx.ChickenKottuId, 1, null,
            new() { new RecipeLineInput(fx.LionLagerId, 2) }), default);

        // Reset finished stock so the yield cost is unambiguous: start Chicken at 0.
        await using (var db = fx.NewTenantContext())
        {
            var chk = await db.ProductStocks.IgnoreQueryFilters().FirstAsync(s => s.ProductId == fx.ChickenKottuId && s.LocationId == fx.LocationId);
            chk.QuantityOnHand = 0; await db.SaveChangesAsync();
        }

        var po = await svc.ProduceAsync(new ProduceInput(fx.LocationId, fx.ChickenKottuId, 10, null), default);

        po.ProductionNumber.Should().StartWith("PRD-");
        po.TotalInputCost.Should().Be(4400m);     // 10 units × 2 Lion × 220
        po.UnitCost.Should().Be(440m);            // 4400 / 10

        var lion = await Stock(fx.LionLagerId);
        lion[0].Should().Be(30m);                 // 50 − 20 consumed
        var kottu = await Stock(fx.ChickenKottuId);
        kottu[0].Should().Be(10m);                // produced
        kottu[1].Should().Be(440m);              // rolled-up cost (started from 0)
    }

    [Fact]
    public async Task List_returns_recipes_and_production_orders()
    {
        var svc = Svc();
        await svc.SetRecipeAsync(new SetRecipeInput(fx.ChickenKottuId, 1, null,
            new() { new RecipeLineInput(fx.LionLagerId, 2) }), default);

        var recipes = await svc.ListRecipesAsync(default);
        recipes.Should().ContainSingle(r => r.ProductId == fx.ChickenKottuId);
        recipes.Single().ProductName.Should().Be("Chicken Kottu");
        recipes.Single().LineCount.Should().Be(1);

        await svc.ProduceAsync(new ProduceInput(fx.LocationId, fx.ChickenKottuId, 5, null), default);
        var prod = await svc.ListProductionAsync(50, default);
        prod.Should().ContainSingle(p => p.ProductId == fx.ChickenKottuId && p.Quantity == 5m);
        prod.Single().ProductionNumber.Should().StartWith("PRD-");
    }

    [Fact]
    public async Task Recipe_in_grams_deducts_kilograms_from_stock_at_cost()
    {
        var svc = Svc();
        // Chicken Kottu recipe: 500 g Raw Rice. Rice is stocked in KG @ 200/kg.
        await svc.SetRecipeAsync(new SetRecipeInput(fx.ChickenKottuId, 1, null,
            new() { new RecipeLineInput(fx.RiceId, 500, fx.GramUnitId) }), default);

        // Produce 2 → needs 2 × 500 g = 1000 g = 1 kg of rice.
        var po = await svc.ProduceAsync(new ProduceInput(fx.LocationId, fx.ChickenKottuId, 2, null), default);

        po.TotalInputCost.Should().Be(200m);     // 1 kg × 200/kg
        po.UnitCost.Should().Be(100m);           // 200 / 2

        await using var db = fx.NewTenantContext();
        var rice = await db.ProductStocks.IgnoreQueryFilters()
            .FirstAsync(s => s.ProductId == fx.RiceId && s.LocationId == fx.LocationId);
        rice.QuantityOnHand.Should().Be(49m);    // 50 kg − 1 kg
    }

    [Fact]
    public async Task Recipe_unit_must_be_compatible_with_stock_unit()
    {
        var svc = Svc();
        // Rice is stocked by mass (kg); a volume unit (L) can't convert → rejected.
        var act = () => svc.SetRecipeAsync(new SetRecipeInput(fx.ChickenKottuId, 1, null,
            new() { new RecipeLineInput(fx.RiceId, 1, fx.LitreUnitId) }), default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*can't be converted*");
    }

    // ── negatives ──
    [Fact]
    public async Task Produce_without_recipe_throws()
    {
        var svc = Svc();
        var act = () => svc.ProduceAsync(new ProduceInput(fx.LocationId, fx.LionLagerId, 1, null), default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*No active recipe*");
    }

    [Fact]
    public async Task Produce_with_insufficient_ingredients_throws()
    {
        var svc = Svc();
        await svc.SetRecipeAsync(new SetRecipeInput(fx.ChickenKottuId, 1, null,
            new() { new RecipeLineInput(fx.LionLagerId, 2) }), default);
        // 50 Lion ÷ 2 = max 25 producible; ask for 100.
        var act = () => svc.ProduceAsync(new ProduceInput(fx.LocationId, fx.ChickenKottuId, 100, null), default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Insufficient*");
    }

    [Fact]
    public async Task Recipe_cannot_reference_itself()
    {
        var svc = Svc();
        var act = () => svc.SetRecipeAsync(new SetRecipeInput(fx.ChickenKottuId, 1, null,
            new() { new RecipeLineInput(fx.ChickenKottuId, 1) }), default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*own ingredient*");
    }
}
