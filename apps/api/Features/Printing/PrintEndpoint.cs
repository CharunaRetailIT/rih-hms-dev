using Hms.Api.Domain;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.Printing;

/// <summary>
/// Print-job queue (#16). The POS/back-office enqueues KOT chits + bills; a thin
/// venue Print Agent polls the queue for its outlet, prints to thermal hardware,
/// and acks each job. The agent binary is a separate deliverable — this is the
/// contract it drives. Auth is the standard tenant JWT for now; a dedicated
/// per-agent token is a follow-on.
/// </summary>
public static class PrintEndpoint
{
    public static IEndpointRouteBuilder MapPrintEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/v1/print").WithTags("Printing").RequireAuthorization("Operations");

        // Enqueue a job (POS auto-print on send / bill print when an agent is used).
        g.MapPost("/jobs", async (PrintJobInput i, ITenantDbContextFactory f, LocationScope scope, CancellationToken ct) =>
        {
            if (i.LocationId == Guid.Empty || string.IsNullOrWhiteSpace(i.Payload))
                return Results.BadRequest(new { error = "locationId and payload are required" });
            scope.Assert(i.LocationId);   // #scoping-P2
            if (i.Kind is not ("kot" or "bill")) return Results.BadRequest(new { error = "kind must be kot or bill" });
            await using var db = await f.CreateForCurrentAsync(ct);
            var job = new PrintJob
            {
                LocationId = i.LocationId, Kind = i.Kind, OrderId = i.OrderId,
                PrinterName = i.PrinterName, Payload = i.Payload, Status = "queued",
            };
            db.PrintJobs.Add(job);
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { job.Id, job.Status });
        }).WithName("Print.Enqueue").WithSummary("Queue a KOT/bill for the venue print agent.");

        // Agent poll: queued jobs for an outlet (oldest first).
        g.MapGet("/jobs", async (ITenantDbContextFactory f, CancellationToken ct, Guid? locationId, string? status, int? limit) =>
        {
            await using var db = await f.CreateForCurrentAsync(ct);
            var q = db.PrintJobs.AsNoTracking().Where(j => j.Status == (status ?? "queued"));
            if (locationId is Guid l) q = q.Where(j => j.LocationId == l);
            var rows = await q.OrderBy(j => j.CreatedAt).Take(Math.Clamp(limit ?? 20, 1, 100))
                .Select(j => new { j.Id, j.LocationId, j.Kind, j.OrderId, j.PrinterName, j.Payload, j.Attempts, j.CreatedAt })
                .ToListAsync(ct);
            return Results.Ok(rows);
        }).WithName("Print.Poll").WithSummary("Poll queued print jobs (the agent's work list).");

        // Agent ack: mark a job printed or failed.
        g.MapPost("/jobs/{id:guid}/ack", async (Guid id, PrintAckInput body, ITenantDbContextFactory f, CancellationToken ct) =>
        {
            await using var db = await f.CreateForCurrentAsync(ct);
            var job = await db.PrintJobs.FirstOrDefaultAsync(j => j.Id == id, ct);
            if (job is null) return Results.NotFound();
            job.Attempts += 1;
            job.Status = body.Status == "printed" ? "printed" : "failed";
            job.Error = body.Status == "printed" ? null : body.Error;
            if (job.Status == "printed") job.PrintedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { job.Id, job.Status });
        }).WithName("Print.Ack").WithSummary("Agent acknowledges a job as printed or failed.");

        return app;
    }
}

public record PrintJobInput(Guid LocationId, string Kind, Guid? OrderId, string Payload, string? PrinterName);
public record PrintAckInput(string Status, string? Error);
