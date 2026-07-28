using System.Text.RegularExpressions;
using Hms.Api.Infrastructure;

namespace Hms.Api.Features.Products;

public static class ProductsEndpoint
{
    private static readonly Dictionary<string, string> AllowedImageTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".png"] = "image/png",
        [".webp"] = "image/webp",
        [".gif"] = "image/gif",
    };

    private const long MaxImageBytes = 5 * 1024 * 1024;

    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var products = app.MapGroup("/api/v1/products")
            .WithTags("Products");

        // Flat, unpaginated list — kept for backward compatibility with pages that
        // just need the full catalog for a dropdown/picker (not a paged grid).
        products.MapGet("", async (
            ProductsService svc,
            CancellationToken ct,
            bool? activeOnly = null,
            string? search = null,
            Guid? categoryId = null) =>
        {
            var result = await svc.GetAllAsync(search, categoryId, activeOnly, ct);
            return Results.Ok(result);
        })
        .WithName("Products.List");

        products.MapGet("/paged", async (
            ProductsService svc,
            CancellationToken ct,
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null,
            Guid? locationId = null,
            Guid? departmentId = null,
            Guid? categoryId = null,
            string? productType = null,
            bool? isActive = null) =>
        {
            var result = await svc.GetPagedAsync(pageNumber, pageSize, search, locationId, departmentId, categoryId, productType, isActive, ct);
            return Results.Ok(result);
        })
        .WithName("Products.Paged");

        products.MapGet("/lookups", async (ProductsService svc, CancellationToken ct) =>
        {
            var result = await svc.GetLookupsAsync(ct);
            return Results.Ok(result);
        })
        .WithName("Products.Lookups");

        products.MapGet("/next-code", async (ProductsService svc, CancellationToken ct) =>
        {
            var (code, prefix) = await svc.GetNextCodeAsync(ct);
            return Results.Ok(new { code, prefix });
        })
        .WithName("Products.NextCode").RequireAuthorization("SupplyChain");

        // Bulk CSV import (#master-data-import): upsert by Product Code; auto-creates Category and
        // Unit of Measure by name. Powers the Data Import hub's Products tab (apps/web/app/(tenant)/data-import).
        products.MapPost("/import", async (List<ProductImportRow> rows, ProductsService svc, CancellationToken ct) =>
        {
            if (rows is null || rows.Count == 0) return Results.BadRequest(new { error = "No rows to import" });
            return Results.Ok(await svc.ImportAsync(rows, ct));
        })
        .WithName("Products.Import").WithSummary("Bulk CSV import of products (upsert by Product Code).")
        .RequireAuthorization("SupplyChain");

        products.MapGet("/{id:guid}", async (Guid id, ProductsService svc, CancellationToken ct) =>
        {
            var item = await svc.GetAsync(id, ct);
            return item is null ? Results.NotFound() : Results.Ok(item);
        })
        .WithName("Products.Get");

        products.MapPut("", async (SaveProductRequest req, ProductsService svc, CancellationToken ct) =>
        {
            var result = await svc.SaveAsync(req, ct);

            return result.Success
                ? Results.Ok(result.Data)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("Products.Save")
        .RequireAuthorization("SupplyChain");

        products.MapDelete("/{id:guid}", async (Guid id, ProductsService svc, CancellationToken ct) =>
        {
            var result = await svc.DeleteAsync(id, ct);

            return result.Success
                ? Results.NoContent()
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("Products.Delete")
        .RequireAuthorization("SupplyChain");

        products.MapPost("/images", async (
            HttpRequest request,
            ITenantContext tenantContext,
            IWebHostEnvironment env,
            CancellationToken ct) =>
        {
            if (!request.HasFormContentType)
                return Results.BadRequest(new { error = "Expected multipart/form-data." });

            var form = await request.ReadFormAsync(ct);
            var file = form.Files.GetFile("file");

            if (file is null || file.Length == 0)
                return Results.BadRequest(new { error = "No file uploaded." });

            if (file.Length > MaxImageBytes)
                return Results.BadRequest(new { error = "Image must be 5MB or smaller." });

            var ext = Path.GetExtension(file.FileName);
            if (string.IsNullOrEmpty(ext) || !AllowedImageTypes.ContainsKey(ext))
                return Results.BadRequest(new { error = "Unsupported image type. Use JPG, PNG, WEBP, or GIF." });

            var tenantId = tenantContext.TenantIdOrThrow();
            var fileName = $"{Guid.NewGuid():N}{ext.ToLowerInvariant()}";
            var folder = Path.Combine(env.ContentRootPath, "uploads", "products", tenantId.ToString());
            Directory.CreateDirectory(folder);

            var fullPath = Path.Combine(folder, fileName);
            await using (var stream = File.Create(fullPath))
                await file.CopyToAsync(stream, ct);

            return Results.Ok(new { url = $"/api/v1/products/images/{tenantId}/{fileName}" });
        })
        .WithName("Products.UploadImage")
        .RequireAuthorization("SupplyChain");

        // Anonymous: <img> tags can't attach an Authorization header. The file
        // name is a server-generated GUID, so it isn't guessable.
        products.MapGet("/images/{tenantId:guid}/{fileName}", (Guid tenantId, string fileName, IWebHostEnvironment env) =>
        {
            if (!Regex.IsMatch(fileName, @"^[0-9a-f]{32}\.(jpg|jpeg|png|webp|gif)$"))
                return Results.NotFound();

            var fullPath = Path.Combine(env.ContentRootPath, "uploads", "products", tenantId.ToString(), fileName);
            if (!File.Exists(fullPath))
                return Results.NotFound();

            return Results.File(fullPath, AllowedImageTypes[Path.GetExtension(fileName)]);
        })
        .WithName("Products.GetImage")
        .AllowAnonymous();

        return app;
    }
}