using System.Security.Claims;
using Hms.Api.Infrastructure;

namespace Hms.Api.Features.Shifts;

public static class ShiftEndpoint
{
    public static IEndpointRouteBuilder MapShiftEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/v1/shifts").WithTags("Shifts").RequireAuthorization("Operations");

        g.MapGet("/current", async (Guid locationId, ShiftService svc, CancellationToken ct) =>
        {
            var s = await svc.GetCurrentAsync(locationId, ct);
            return s is null ? Results.NoContent() : Results.Ok(s);
        }).WithName("Shifts.Current").WithSummary("The open shift at an outlet (with live cash-up totals), or 204.");

        g.MapGet("", async (ShiftService svc, LocationScope scope, CancellationToken ct) =>
            Results.Ok(await svc.ListAsync(50, ct, scope.IsPinned ? scope.HomeLocationId : null)))
            .WithName("Shifts.List").WithSummary("Recent shifts (most recent first).");

        g.MapPost("/open", async (OpenShiftInput body, ClaimsPrincipal user, ShiftService svc, LocationScope scope, Hms.Api.Features.Audit.AuditService audit, CancellationToken ct) =>
        {
            try
            {
                scope.Assert(body.LocationId);   // #scoping-P2
                var (uid, name) = CurrentUser(user);
                var s = await svc.OpenAsync(body.LocationId, uid, name, body.OpeningFloat, ct);
                await audit.LogAsync("shift.open", "shift", s.Id, $"Opened {s.ShiftNumber} · float LKR {s.OpeningFloat:F2}", ct);
                return Results.Ok(s);
            }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Shifts.Open").WithSummary("Open a shift with a starting cash float.");

        g.MapPost("/{id:guid}/close", async (Guid id, CloseShiftInput body, ShiftService svc, Hms.Api.Features.Audit.AuditService audit, CancellationToken ct) =>
        {
            try
            {
                var s = await svc.CloseAsync(id, body.DeclaredCash, body.Notes, body.AllowOpenBills ?? false, ct);
                await audit.LogAsync("shift.close", "shift", s.Id, $"Closed {s.ShiftNumber} · variance LKR {(s.CashVariance ?? 0):F2}", ct);
                return Results.Ok(s);
            }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Shifts.Close").WithSummary("Close a shift — declare counted cash, get the Z-report + variance.");

        return app;
    }

    /// <summary>User id (sub / NameIdentifier) + a display fallback (email) from the JWT.</summary>
    private static (Guid?, string?) CurrentUser(ClaimsPrincipal user)
    {
        var idRaw = user.FindFirst("sub")?.Value ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Guid? uid = Guid.TryParse(idRaw, out var g) ? g : null;
        var email = user.FindFirst(ClaimTypes.Email)?.Value ?? user.FindFirst("email")?.Value;
        return (uid, email);
    }
}
