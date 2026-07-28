using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.Health;

public static class HealthEndpoint
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        // Probes must answer without a token (load balancers / k8s liveness).
        var g = app.MapGroup("/health").WithTags("Health").AllowAnonymous();

        g.MapGet("", () => Results.Ok(new { status = "ok" }))
         .WithName("Health.Liveness")
         .WithSummary("Liveness probe — always 200 if the process is up.");

        g.MapGet("/ready", async (ControlDbContext db, CancellationToken ct) =>
        {
            try
            {
                var canConnect = await db.Database.CanConnectAsync(ct);
                return canConnect
                    ? Results.Ok(new { status = "ready", db = "ok" })
                    : Results.Json(new { status = "degraded", db = "unreachable" }, statusCode: 503);
            }
            catch (Exception ex)
            {
                return Results.Json(new { status = "degraded", db = "error", message = ex.Message }, statusCode: 503);
            }
        })
        .WithName("Health.Readiness")
        .WithSummary("Readiness probe — checks the control-plane DB connection.");

        return app;
    }
}
