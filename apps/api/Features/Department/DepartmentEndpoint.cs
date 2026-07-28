namespace Hms.Api.Features.Departments;

public static class DepartmentsEndpoint
{
    public static IEndpointRouteBuilder MapDepartmentEndpoints(this IEndpointRouteBuilder app)
    {
        var departments = app.MapGroup("/api/v1/departments")
            .WithTags("Departments");

        departments.MapGet("/paged", async (DepartmentsService svc, CancellationToken ct, int pageNumber = 1, int pageSize = 10, string? search = null, Guid? locationId = null, bool? isActive = null) =>
        {
            var result = await svc.GetPagedAsync(pageNumber, pageSize, search, locationId, isActive, ct);
            return Results.Ok(result);
        })
        .WithName("Departments.Paged");

        departments.MapGet("/{id:guid}", async (Guid id, DepartmentsService svc, CancellationToken ct) =>
        {
            var item = await svc.GetAsync(id, ct);
            return item is null ? Results.NotFound() : Results.Ok(item);
        })
        .WithName("Departments.Get");

        departments.MapPut("", async (SaveDepartmentRequest req, DepartmentsService svc, CancellationToken ct) =>
        {
            var result = await svc.SaveAsync(req, ct);

            return result.Success
                ? Results.Ok(result.Data)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("Departments.Save")
        .RequireAuthorization("BackOffice");

        departments.MapDelete("/{id:guid}", async (Guid id, DepartmentsService svc, CancellationToken ct) =>
        {
            var result = await svc.DeleteAsync(id, ct);

            return result.Success
                ? Results.NoContent()
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("Departments.Delete")
        .RequireAuthorization("BackOffice");

        return app;
    }
}