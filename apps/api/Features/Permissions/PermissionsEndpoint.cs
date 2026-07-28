namespace Hms.Api.Features.Permissions;

public static class PermissionsEndpoint
{
    private static readonly Dictionary<int, string> RoleName = new()
    { [1] = "Manager", [2] = "Cashier", [3] = "Kitchen", [4] = "Accountant" };

    public static IEndpointRouteBuilder MapPermissionEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/v1/permissions").WithTags("Permissions");

        // Any back-office user can read the matrix (the POS doesn't need it — limits
        // are enforced server-side). Only owners can change it.
        g.MapGet("", async (PermissionService svc, CancellationToken ct) =>
        {
            var rows = await svc.ListAsync(ct);
            return Results.Ok(rows.Select(p => new
            {
                p.Role, roleName = RoleName.GetValueOrDefault(p.Role, $"Role {p.Role}"),
                p.MaxDiscountPercent, p.CanApplyDiscount, p.CanVoid, p.CanComp,
            }));
        }).WithName("Permissions.List").RequireAuthorization("BackOffice");

        g.MapPut("", async (RolePermissionInput body, PermissionService svc, Hms.Api.Features.Audit.AuditService audit, CancellationToken ct) =>
        {
            try
            {
                var p = await svc.UpsertAsync(body, ct);
                await audit.LogAsync("permissions.update", "role", null,
                    $"{RoleName.GetValueOrDefault(p.Role, $"Role {p.Role}")}: max discount {p.MaxDiscountPercent:0.#}%, void {(p.CanVoid ? "on" : "off")}", ct);
                return Results.Ok(new { p.Role, p.MaxDiscountPercent, p.CanApplyDiscount, p.CanVoid, p.CanComp });
            }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Permissions.Upsert").RequireAuthorization("Owners");

        // ── Function-level (per-screen) access (#71 follow-on) ──
        // The full matrix (Owner-config). UI gating sits on top of the API's RBAC.
        g.MapGet("/screens", async (PermissionService svc, CancellationToken ct) =>
            Results.Ok((await svc.ScreenMatrixAsync(ct)).Select(s => new { s.Role, s.Screen, s.Allowed })))
            .WithName("Permissions.ScreenMatrix").RequireAuthorization("BackOffice");

        // The caller's own denied screens — the sidebar uses this to hide nav items.
        g.MapGet("/screens/me", async (PermissionService svc, System.Security.Claims.ClaimsPrincipal user, CancellationToken ct) =>
            Results.Ok(new { denied = await svc.DeniedScreensAsync(PermissionService.RoleIdOf(user), ct) }))
            .WithName("Permissions.MyScreens").RequireAuthorization();

        g.MapPut("/screens", async (ScreenAccessInput body, PermissionService svc, Hms.Api.Features.Audit.AuditService audit, CancellationToken ct) =>
        {
            if (body.Role == 0) return Results.BadRequest(new { error = "Owner access can't be restricted" });
            if (string.IsNullOrWhiteSpace(body.Screen)) return Results.BadRequest(new { error = "Screen is required" });
            await svc.SetScreenAccessAsync(body.Role, body.Screen, body.Allowed, ct);
            await audit.LogAsync("permissions.screen", "role", null,
                $"{RoleName.GetValueOrDefault(body.Role, $"Role {body.Role}")}: {body.Screen} {(body.Allowed ? "shown" : "hidden")}", ct);
            return Results.Ok(new { body.Role, body.Screen, body.Allowed });
        }).WithName("Permissions.SetScreen").RequireAuthorization("Owners");

        return app;
    }
}

public record ScreenAccessInput(int Role, string Screen, bool Allowed);
