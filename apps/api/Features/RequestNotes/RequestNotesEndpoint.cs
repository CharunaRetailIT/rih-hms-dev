using System.Security.Claims;
using Hms.Api.Features.Permissions;
using Hms.Api.Infrastructure;

namespace Hms.Api.Features.RequestNotes;

public static class RequestNotesEndpoint
{
    public static IEndpointRouteBuilder MapRequestNotesEndpoints(this IEndpointRouteBuilder app)
    {
        var req = app.MapGroup("/api/v1/request-notes").WithTags("Request notes").RequireAuthorization("SupplyChain");

        req.MapGet("/paged", async (
            RequestNoteService svc, CancellationToken ct,
            int pageNumber, int pageSize, string? search, Guid? fromLocationId, Guid? toLocationId,
            string? mode, string? status, DateOnly? dateFrom, DateOnly? dateTo) =>
            Results.Ok(await svc.GetPagedAsync(pageNumber, pageSize, search, fromLocationId, toLocationId, mode, status, dateFrom, dateTo, ct)))
            .WithName("RequestNotes.Paged").WithSummary("Server-paged list of request notes.");

        req.MapGet("/{id:guid}", async (Guid id, RequestNoteService svc, CancellationToken ct) =>
        {
            var r = await svc.GetAsync(id, ct);
            return r is null ? Results.NotFound() : Results.Ok(RequestNoteDto(r));
        }).WithName("RequestNotes.Get");

        req.MapPost("", async (RequestNoteDraftInput i, RequestNoteService svc, LocationScope scope, CancellationToken ct) =>
        {
            scope.Assert(i.FromLocationId);   // #scoping-P2
            try { return Results.Created($"/api/v1/request-notes", RequestNoteDto(await svc.CreateDraftAsync(i, ct))); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("RequestNotes.Create").WithSummary("Create a request note (draft).");

        req.MapPut("/{id:guid}", async (Guid id, RequestNoteDraftInput i, RequestNoteService svc, CancellationToken ct) =>
        {
            try { return Results.Ok(RequestNoteDto(await svc.EditDraftAsync(id, i, ct))); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("RequestNotes.Edit").WithSummary("Edit a draft request note.");

        req.MapPost("/{id:guid}/submit", async (Guid id, ClaimsPrincipal user, RequestNoteService svc, CancellationToken ct) =>
        {
            try { return Results.Ok(RequestNoteDto(await svc.SubmitAsync(id, CurrentUserId(user), ct))); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("RequestNotes.Submit").WithSummary("Submit a draft — resolves to approved or pending depending on approval policy.");

        req.MapPost("/{id:guid}/approve", async (Guid id, ClaimsPrincipal user, RequestNoteService svc, CancellationToken ct) =>
        {
            try { return Results.Ok(RequestNoteDto(await svc.ApproveAsync(id, CurrentUserId(user), PermissionService.RoleIdOf(user), CurrentUserName(user), ct))); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("RequestNotes.Approve").WithSummary("Approve a request note awaiting authorisation.");

        req.MapPost("/{id:guid}/reject", async (Guid id, ClaimsPrincipal user, ReasonInput? body, RequestNoteService svc, CancellationToken ct) =>
        {
            try { return Results.Ok(RequestNoteDto(await svc.RejectAsync(id, CurrentUserId(user), PermissionService.RoleIdOf(user), CurrentUserName(user), body?.Reason, ct))); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("RequestNotes.Reject");

        req.MapDelete("/{id:guid}", async (Guid id, RequestNoteService svc, CancellationToken ct) =>
        {
            try { await svc.RemoveAsync(id, ct); return Results.NoContent(); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("RequestNotes.Remove").WithSummary("Soft-delete a request note that hasn't been approved.");

        return app;
    }

    private static object RequestNoteDto(Hms.Api.Domain.RequestNote r) => new
    {
        r.Id, r.RequestNumber, r.Mode, r.FromLocationId, r.ToLocationId, r.ExpectedDeliveryDate,
        r.Status, r.CommonRemark, r.SubmittedAt, r.ApprovedBy, r.ApprovedAt, r.RejectedBy, r.RejectedAt, r.RejectReason,
        r.PurchaseOrderId, r.TransferId, r.CreatedAt,
        lines = r.Lines.Where(l => !l.IsDeleted).Select(l => new {
            l.ProductId, l.Sku, l.ProductName, l.Sih, l.Quantity, l.Remark }),
    };

    /// <summary>The acting user's id from the JWT (sub / NameIdentifier), for approval audit.</summary>
    private static Guid? CurrentUserId(ClaimsPrincipal user)
    {
        var raw = user.FindFirst("sub")?.Value ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(raw, out var g) ? g : null;
    }

    private static string CurrentUserName(ClaimsPrincipal user) =>
        user.FindFirst("name")?.Value ?? user.FindFirst(ClaimTypes.Email)?.Value ?? "Approver";
}

public record ReasonInput(string? Reason);
