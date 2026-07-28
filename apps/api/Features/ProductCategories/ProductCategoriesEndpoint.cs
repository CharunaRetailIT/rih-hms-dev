using Hms.Api.Domain;

namespace Hms.Api.Features.Products;

public static class ProductCategoriesEndpoint
{
    public static IEndpointRouteBuilder MapProductCategoriesEndpoints(this IEndpointRouteBuilder app)
    {
        var categories = app.MapGroup("/api/v1/categories").WithTags("Categories");

        // List categories, flat — includes both parents and children with ParentId set,
        // so the frontend can build the one-level hierarchy itself. Pass ?all=true to
        // include inactive rows (needed for the "Activate" action and for picking a
        // parent during edit when that parent happens to already be inactive).
        categories.MapGet("", async (bool? all, ProductCategoriesService svc, CancellationToken ct) =>
        {
            var list = await svc.ListAsync(ct, includeInactive: all == true);
            var result = list.Select(c => new
            { c.Id, c.ParentId, c.Code, c.Name, c.Description, c.ColorHex, c.IconName, c.SortOrder, c.IsActive });
            return Results.Ok(result);
        })
        .WithName("Categories.List");

        categories.MapGet("/{id:guid}", async (Guid id, ProductCategoriesService svc, CancellationToken ct) =>
        {
            var c = await svc.GetAsync(id, ct);
            if (c is null) return Results.NotFound();
            return Results.Ok(new
            { c.Id, c.ParentId, c.Code, c.Name, c.Description, c.ColorHex, c.IconName, c.SortOrder, c.IsActive });
        })
        .WithName("Categories.Get");

        // Create / update a category. Does not touch IsActive — see the dedicated
        // activate/deactivate endpoints below for that.
        categories.MapPut("", async (CategoryUpsertRequest req, ProductCategoriesService svc, CancellationToken ct) =>
        {
            var input = new CategoryInput(req.Code, req.Name, req.ParentId, req.Description, req.ColorHex, req.IconName, req.SortOrder);
            try
            {
                var c = req.Id is Guid id ? await svc.EditAsync(id, input, ct) : await svc.CreateAsync(input, ct);
                var result = new { c.Id, c.ParentId, c.Code, c.Name, c.Description, c.ColorHex, c.IconName, c.SortOrder, c.IsActive };
                return req.Id is null ? Results.Created($"/api/v1/categories/{c.Id}", result) : Results.Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return ToProblemResult(ex);
            }
        })
        .WithName("Categories.Upsert").RequireAuthorization("SupplyChain");

        // Reactivate a category. Blocked if its parent is currently inactive,
        // since an active child under an inactive parent doesn't make sense for the POS tree.
        categories.MapPut("/{id:guid}/activate", async (Guid id, ProductCategoriesService svc, CancellationToken ct) =>
        {
            try
            {
                var c = await svc.ActivateAsync(id, ct);
                return Results.Ok(new { c.Id, c.Code, c.Name, c.IsActive });
            }
            catch (InvalidOperationException ex)
            {
                return ToProblemResult(ex);
            }
        })
        .WithName("Categories.Activate").RequireAuthorization("SupplyChain");

        // Deactivate a category. Blocked while it still has active children or active
        // products pointing at it, so the POS menu tree never ends up with orphaned active rows.
        categories.MapPut("/{id:guid}/deactivate", async (Guid id, ProductCategoriesService svc, CancellationToken ct) =>
        {
            try
            {
                var c = await svc.DeactivateAsync(id, ct);
                return Results.Ok(new { c.Id, c.Code, c.Name, c.IsActive });
            }
            catch (InvalidOperationException ex)
            {
                return ToProblemResult(ex);
            }
        })
        .WithName("Categories.Deactivate").RequireAuthorization("SupplyChain");

        return app;
    }

    // The service throws InvalidOperationException uniformly for both "not found" and validation
    // failures; this maps "not found" to 404 and everything else to 400 with the message intact.
    private static IResult ToProblemResult(InvalidOperationException ex) =>
        ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)
            ? Results.NotFound(new { error = ex.Message })
            : Results.BadRequest(new { error = ex.Message });
}

public record CategoryUpsertRequest(Guid? Id, string Code, string Name, Guid? ParentId = null, string? Description = null,
    string? ColorHex = null, string? IconName = null, int SortOrder = 0);