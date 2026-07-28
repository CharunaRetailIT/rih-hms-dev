using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Hms.Api.Infrastructure;

/// <summary>
/// #6 Pro/Lite feature gating. A plan code unlocks a set of modules; the core POS
/// modules are always on, the "Pro" modules need the pro tier or higher. A
/// null/unknown plan (trial, or not-yet-projected) is treated as full access so we
/// never accidentally lock an existing or trialing tenant.
/// </summary>
public static class PlanFeatures
{
    // Modules that are Pro-and-up only (everything else is core/Lite).
    public static readonly string[] ProModules = { "accounting", "production", "promotions", "catering", "loyalty" };

    static int Rank(string? code) => code?.Trim().ToLowerInvariant() switch
    {
        "lite" => 0,
        "pro" => 1,
        "enterprise" => 2,
        _ => 99,   // null / trial / unknown ⇒ full access
    };

    public static bool Includes(string? planCode, string module)
        => !ProModules.Contains(module) || Rank(planCode) >= 1;

    /// <summary>Per-module on/off map sent to the web for nav gating.</summary>
    public static object Map(string? planCode) => new
    {
        accounting = Includes(planCode, "accounting"),
        production = Includes(planCode, "production"),
        promotions = Includes(planCode, "promotions"),
        catering   = Includes(planCode, "catering"),
        loyalty    = Includes(planCode, "loyalty"),
    };
}

/// <summary>
/// Endpoint filter that blocks a route when the tenant's plan doesn't include the
/// given module — defense-in-depth behind the web's nav lock. Returns 403 with an
/// upgrade message the client surfaces verbatim.
/// </summary>
public sealed class RequireFeatureFilter(string module) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext ctx, EndpointFilterDelegate next)
    {
        var ct = ctx.HttpContext.RequestAborted;
        var factory = ctx.HttpContext.RequestServices.GetRequiredService<ITenantDbContextFactory>();
        await using var db = await factory.CreateForCurrentAsync(ct);
        var plan = await db.OrgSettings.AsNoTracking().Select(o => o.PlanCode).FirstOrDefaultAsync(ct);
        if (!PlanFeatures.Includes(plan, module))
            return Results.Json(new { error = $"{char.ToUpper(module[0]) + module[1..]} is a Pro feature — upgrade your plan to use it." },
                statusCode: StatusCodes.Status403Forbidden);
        return await next(ctx);
    }
}
