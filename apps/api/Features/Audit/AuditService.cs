using Hms.Api.Domain;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.Audit;

/// <summary>
/// Append-only audit log (#77). Records who did what from the current request's
/// user context. Logging is best-effort — a failure here must never break the
/// action being audited.
/// </summary>
public class AuditService(ITenantDbContextFactory factory, ITenantContext ctx)
{
    public async Task LogAsync(string action, string? entityType, Guid? entityId, string? summary, CancellationToken ct, string? meta = null)
    {
        try
        {
            await using var db = await factory.CreateForCurrentAsync(ct);
            db.ActivityLogs.Add(new ActivityLog
            {
                ActorId = ctx.UserId, ActorName = ctx.UserName, ActorRole = ctx.UserRole,
                Action = action, EntityType = entityType, EntityId = entityId,
                Summary = summary, Meta = meta,
            });
            await db.SaveChangesAsync(ct);
        }
        catch { /* audit is best-effort — never surface a logging failure to the caller */ }
    }
}
