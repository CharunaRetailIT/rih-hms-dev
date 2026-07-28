namespace Hms.Api.Features.Suppliers;

public static class SuppliersEndpoint
{
    public static IEndpointRouteBuilder MapSupplierEndpoints(this IEndpointRouteBuilder app)
    {
        var suppliers = app.MapGroup("/api/v1/suppliers")
            .WithTags("Suppliers");

        suppliers.MapGet("", async (SuppliersService svc, CancellationToken ct) =>
        {
            var list = await svc.LookupAsync(ct);
            return Results.Ok(list);
        })
        .WithName("Suppliers.Lookup");

        suppliers.MapGet("/paged", async (
            SuppliersService svc,
            CancellationToken ct,
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null,
            Guid? supplierGroupId = null,
            Guid? supplierTypeId = null,
            bool? isActive = null) =>
        {
            var result = await svc.GetPagedAsync(pageNumber, pageSize, search, supplierGroupId, supplierTypeId, isActive, ct);
            return Results.Ok(result);
        })
        .WithName("Suppliers.Paged");

        suppliers.MapGet("/{id:guid}", async (Guid id, SuppliersService svc, CancellationToken ct) =>
        {
            var item = await svc.GetAsync(id, ct);
            return item is null ? Results.NotFound() : Results.Ok(item);
        })
        .WithName("Suppliers.Get");

        suppliers.MapPut("", async (SaveSupplierRequest req, SuppliersService svc, CancellationToken ct) =>
        {
            var result = await svc.SaveAsync(req, ct);
            return result.Success ? Results.Ok(result.Data) : Results.BadRequest(new { error = result.Error });
        })
        .WithName("Suppliers.Save")
        .RequireAuthorization("SupplyChain");

        suppliers.MapDelete("/{id:guid}", async (Guid id, SuppliersService svc, CancellationToken ct) =>
        {
            var result = await svc.DeleteAsync(id, ct);
            return result.Success ? Results.NoContent() : Results.BadRequest(new { error = result.Error });
        })
        .WithName("Suppliers.Delete")
        .RequireAuthorization("SupplyChain");

        suppliers.MapGet("/groups", async (SuppliersService svc, CancellationToken ct) =>
        {
            var list = await svc.GetGroupsAsync(ct);
            return Results.Ok(list);
        })
        .WithName("Suppliers.Groups.List");

        suppliers.MapGet("/groups/paged", async (
            SuppliersService svc,
            CancellationToken ct,
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null,
            bool? isActive = null) =>
        {
            var result = await svc.GetGroupsPagedAsync(pageNumber, pageSize, search, isActive, ct);
            return Results.Ok(result);
        })
        .WithName("Suppliers.Groups.Paged");

        suppliers.MapPut("/groups", async (SaveSupplierGroupRequest req, SuppliersService svc, CancellationToken ct) =>
        {
            var result = await svc.SaveGroupAsync(req, ct);
            return result.Success ? Results.Ok(result.Data) : Results.BadRequest(new { error = result.Error });
        })
        .WithName("Suppliers.Groups.Save")
        .RequireAuthorization("SupplyChain");

        suppliers.MapDelete("/groups/{id:guid}", async (Guid id, SuppliersService svc, CancellationToken ct) =>
        {
            var result = await svc.DeleteGroupAsync(id, ct);
            return result.Success ? Results.NoContent() : Results.BadRequest(new { error = result.Error });
        })
        .WithName("Suppliers.Groups.Delete")
        .RequireAuthorization("SupplyChain");

        suppliers.MapGet("/types", async (SuppliersService svc, CancellationToken ct) =>
        {
            var list = await svc.GetTypesAsync(ct);
            return Results.Ok(list);
        })
        .WithName("Suppliers.Types.List");

        suppliers.MapGet("/types/paged", async (
            SuppliersService svc,
            CancellationToken ct,
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null,
            bool? isActive = null) =>
        {
            var result = await svc.GetTypesPagedAsync(pageNumber, pageSize, search, isActive, ct);
            return Results.Ok(result);
        })
        .WithName("Suppliers.Types.Paged");

        suppliers.MapPut("/types", async (SaveSupplierTypeRequest req, SuppliersService svc, CancellationToken ct) =>
        {
            var result = await svc.SaveTypeAsync(req, ct);
            return result.Success ? Results.Ok(result.Data) : Results.BadRequest(new { error = result.Error });
        })
        .WithName("Suppliers.Types.Save")
        .RequireAuthorization("SupplyChain");

        suppliers.MapDelete("/types/{id:guid}", async (Guid id, SuppliersService svc, CancellationToken ct) =>
        {
            var result = await svc.DeleteTypeAsync(id, ct);
            return result.Success ? Results.NoContent() : Results.BadRequest(new { error = result.Error });
        })
        .WithName("Suppliers.Types.Delete")
        .RequireAuthorization("SupplyChain");

        return app;
    }
}