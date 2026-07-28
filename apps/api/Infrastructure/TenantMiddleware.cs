using Microsoft.Extensions.Primitives;

namespace Hms.Api.Infrastructure;

/// <summary>
/// Resolves the tenant for the current request from the authenticated principal's
/// <c>tenant_id</c> claim and stores it in <see cref="ITenantContext"/>.
///
/// The claim is populated by the authentication step (see Program.cs): a validated
/// JWT in production, or — only in Development — the X-Tenant-Id dev header via
/// <see cref="DevHeaderAuthHandler"/>. Anonymous endpoints (auth, health, signup)
/// simply leave the tenant unset.
/// </summary>
public class TenantMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext ctx, ITenantContext tenantContext, LocationScope scope)
    {
        var claim = ctx.User?.FindFirst("tenant_id")?.Value;
        if (Guid.TryParse(claim, out var tid))
            tenantContext.Set(tid);

        // Capture the actor for the audit log (#77) + the location scope (#scoping-P2).
        var u = ctx.User;
        if (u?.Identity?.IsAuthenticated == true)
        {
            var subRaw = u.FindFirst("sub")?.Value ?? u.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            Guid? uid = Guid.TryParse(subRaw, out var g) ? g : null;
            var name = u.FindFirst("name")?.Value ?? u.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? u.FindFirst("email")?.Value;
            var role = u.FindFirst("role")?.Value ?? u.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            tenantContext.SetUser(uid, name, role);

            Guid? home = Guid.TryParse(u.FindFirst("home_location_id")?.Value, out var h) ? h : null;
            scope.Set(role, home);
        }

        try
        {
            // Pinned users (#scoping-P2): clamp the locationId that arrives via the
            // route or query string BEFORE the endpoint binds it. A mismatch is 403;
            // an omitted query locationId is forced to the user's home outlet so the
            // "null ⇒ all locations" list endpoints can't leak other branches.
            if (scope.IsPinned && !ctx.Response.HasStarted)
                ClampLocationInRequest(ctx, scope.HomeLocationId!.Value);

            await next(ctx);
        }
        catch (LocationForbiddenException) when (!ctx.Response.HasStarted)
        {
            ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsync("{\"error\":\"You don't have access to that outlet.\"}");
        }
        catch (TenantNotFoundException) when (!ctx.Response.HasStarted)
        {
            // Stale token for a tenant that no longer exists (e.g. after a DB reset):
            // 401 so the client clears the session and re-authenticates — not a 500.
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsync("{\"error\":\"Your session is no longer valid — please sign in again.\"}");
        }
    }

    private static void ClampLocationInRequest(HttpContext ctx, Guid home)
    {
        // Route param, e.g. /aggregator/credentials/{name}/stores/{locationId}.
        if (ctx.Request.RouteValues.TryGetValue("locationId", out var rv)
            && Guid.TryParse(rv?.ToString(), out var routeLoc) && routeLoc != home)
            throw new LocationForbiddenException();

        // Query param ?locationId=… — reject a mismatch, inject home when omitted.
        var q = ctx.Request.Query;
        if (q.TryGetValue("locationId", out var qv) && !string.IsNullOrEmpty(qv))
        {
            if (Guid.TryParse(qv.ToString(), out var queryLoc) && queryLoc != home)
                throw new LocationForbiddenException();
        }
        else
        {
            var dict = q.ToDictionary(k => k.Key, v => v.Value);
            dict["locationId"] = home.ToString();
            ctx.Request.Query = new Microsoft.AspNetCore.Http.QueryCollection(dict);
        }
    }
}
