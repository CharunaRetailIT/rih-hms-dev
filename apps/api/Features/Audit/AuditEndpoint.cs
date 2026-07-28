using Hms.Api.Features.Orders;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.Audit;

public static class AuditEndpoint
{
    public static IEndpointRouteBuilder MapAuditEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/v1/audit").WithTags("Audit").RequireAuthorization("BackOffice");

        g.MapGet("", async (ITenantDbContextFactory f, CancellationToken ct, DateTime? from, DateTime? to, string? action, int? take) =>
        {
            await using var db = await f.CreateForCurrentAsync(ct);
            var q = db.ActivityLogs.AsNoTracking().AsQueryable();
            if (from is { } ff) { var d = OrderService.AsUtc(ff); q = q.Where(a => a.CreatedAt >= d); }
            if (to is { } tt) { var d = OrderService.AsUtc(tt); q = q.Where(a => a.CreatedAt < d); }
            if (!string.IsNullOrWhiteSpace(action)) q = q.Where(a => a.Action == action);
            var rows = await q.OrderByDescending(a => a.CreatedAt).Take(Math.Clamp(take ?? 200, 1, 1000))
                .Select(a => new { a.Id, at = a.CreatedAt, a.ActorName, a.ActorRole, a.Action, a.EntityType, a.EntityId, a.Summary })
                .ToListAsync(ct);
            return Results.Ok(rows);
        }).WithName("Audit.List").WithSummary("Recent audited actions (who did what, when).");

        g.MapGet("/paged", async (ITenantDbContextFactory f, CancellationToken ct,
            DateTime? from, DateTime? to, string? action, string? search, int pageNumber = 1, int pageSize = 25) =>
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            await using var db = await f.CreateForCurrentAsync(ct);
            var q = db.ActivityLogs.AsNoTracking().AsQueryable();
            if (from is { } ff) { var d = OrderService.AsUtc(ff); q = q.Where(a => a.CreatedAt >= d); }
            if (to is { } tt) { var d = OrderService.AsUtc(tt); q = q.Where(a => a.CreatedAt < d); }
            if (!string.IsNullOrWhiteSpace(action)) q = q.Where(a => a.Action == action);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = $"%{search.Trim()}%";
                q = q.Where(a =>
                    (a.ActorName != null && EF.Functions.ILike(a.ActorName, s)) ||
                    (a.ActorRole != null && EF.Functions.ILike(a.ActorRole, s)) ||
                    EF.Functions.ILike(a.Action, s) ||
                    (a.Summary != null && EF.Functions.ILike(a.Summary, s)) ||
                    (a.EntityType != null && EF.Functions.ILike(a.EntityType, s)));
            }

            var totalCount = await q.CountAsync(ct);
            var rows = await q.OrderByDescending(a => a.CreatedAt)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(a => new { a.Id, at = a.CreatedAt, a.ActorName, a.ActorRole, a.Action, a.EntityType, a.EntityId, a.Summary })
                .ToListAsync(ct);

            return Results.Ok(new
            {
                data = rows,
                pagination = new PaginationMeta(totalCount, pageNumber, pageSize, (int)Math.Ceiling(totalCount / (double)pageSize)),
            });
        }).WithName("Audit.ListPaged").WithSummary("Filterable, paginated activity log.");

        return app;
    }
}

public record PaginationMeta(int TotalCount, int PageNumber, int PageSize, int TotalPages);
