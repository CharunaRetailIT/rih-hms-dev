namespace Hms.Api.Features.Modifiers;

public static class ModifierEndpoint
{
    public static IEndpointRouteBuilder MapModifierEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/v1/modifier-groups").WithTags("Modifiers").RequireAuthorization("SupplyChain");

        g.MapGet("", async (ModifierService svc, CancellationToken ct) =>
            Results.Ok((await svc.ListGroupsAsync(ct)).Select(GroupDto)))
            .WithName("Modifiers.ListGroups");

        g.MapGet("/paged", async (ModifierService svc, CancellationToken ct, int pageNumber = 1, int pageSize = 10, string? search = null) =>
            Results.Ok(await svc.GetPagedAsync(pageNumber, pageSize, search, ct)))
            .WithName("Modifiers.ListGroupsPaged");

        g.MapPut("", async (SetModifierGroupInput i, ModifierService svc, CancellationToken ct) =>
        {
            try { return Results.Ok(GroupDto(await svc.SetGroupAsync(i, ct))); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Modifiers.SetGroup").WithSummary("Create/replace a modifier group + its items.");

        g.MapDelete("/{id:guid}", async (Guid id, ModifierService svc, CancellationToken ct) =>
        {
            try { await svc.DeleteGroupAsync(id, ct); return Results.NoContent(); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Modifiers.DeleteGroup");

        // Product ↔ modifier groups. GET is authenticated (POS reads it); PUT is SupplyChain.
        var pm = app.MapGroup("/api/v1/products").WithTags("Modifiers");

        pm.MapGet("/{id:guid}/modifiers", async (Guid id, ModifierService svc, CancellationToken ct) =>
            Results.Ok((await svc.GetProductModifiersAsync(id, ct)).Select(GroupDto)))
            .WithName("Modifiers.ForProduct").WithSummary("Modifier groups attached to a product (for the POS).");

        pm.MapPut("/{id:guid}/modifiers", async (Guid id, SetProductModifiersInput body, ModifierService svc, CancellationToken ct) =>
        {
            try { await svc.SetProductGroupsAsync(id, body.GroupIds ?? new(), ct); return Results.Ok(new { ok = true }); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Modifiers.SetForProduct").RequireAuthorization("SupplyChain");

        return app;
    }

    private static object GroupDto(Hms.Api.Domain.ModifierGroup g) => new
    {
        g.Id, g.Name, g.MinSelect, g.MaxSelect, g.IsRequired, g.SortOrder, g.IsActive,
        items = g.Items.Where(i => !i.IsDeleted && i.IsActive).OrderBy(i => i.SortOrder)
            .Select(i => new { i.Id, i.Name, i.PriceDelta, i.IsDefault, i.SortOrder }),
    };
}
