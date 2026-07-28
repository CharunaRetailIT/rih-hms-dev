using System.Security.Claims;
using Hms.Api.Domain;
using Hms.Api.Features.Permissions;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.Approvals;

/// <summary>Approval workflow API (#approvals): rule config (BackOffice), an in-app inbox for
/// logged-in approvers, and an ANONYMOUS token endpoint backing the email-link approval page.</summary>
public static class ApprovalsEndpoint
{
    public static IEndpointRouteBuilder MapApprovalEndpoints(this IEndpointRouteBuilder app)
    {
        // ── Rule configuration (owners / back-office) ──
        var cfg = app.MapGroup("/api/v1/approval-rules").WithTags("Approvals").RequireAuthorization("BackOffice");

        cfg.MapGet("", async (ITenantDbContextFactory f, CancellationToken ct) =>
        {
            await using var db = await f.CreateForCurrentAsync(ct);
            var rules = await db.ApprovalRules.OrderBy(r => r.DocType).ThenBy(r => r.MinAmount).ToListAsync(ct);
            var ids = rules.Select(r => r.Id).ToList();
            var allSteps = await db.ApprovalRuleSteps.Where(s => ids.Contains(s.RuleId) && !s.IsDeleted).ToListAsync(ct);
            return Results.Ok(rules.Select(r => new
            {
                r.Id, r.DocType, r.Name, r.MinAmount, r.LocationId, r.IsActive,
                steps = allSteps.Where(s => s.RuleId == r.Id).OrderBy(s => s.Level)
                    .Select(s => new { s.Id, s.Level, s.ApproverType, s.ApproverUserId, s.ApproverRole, s.ApproverEmail, s.ApproverLabel }),
            }));
        }).WithName("Approvals.Rules.List");

        cfg.MapPut("", async (ApprovalRuleInput i, ITenantDbContextFactory f, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(i.DocType) || string.IsNullOrWhiteSpace(i.Name))
                return Results.BadRequest(new { error = "Document type and name are required" });
            if ((i.Steps?.Count ?? 0) == 0) return Results.BadRequest(new { error = "Add at least one approval level" });
            await using var db = await f.CreateForCurrentAsync(ct);
            ApprovalRule rule;
            if (i.Id is Guid id)
                rule = await db.ApprovalRules.FirstOrDefaultAsync(r => r.Id == id, ct)
                       ?? throw new InvalidOperationException("Rule not found");
            else { rule = new ApprovalRule(); db.ApprovalRules.Add(rule); }
            var existingSteps = rule.Id == Guid.Empty ? new() : await db.ApprovalRuleSteps.Where(s => s.RuleId == rule.Id).ToListAsync(ct);
            rule.DocType = i.DocType; rule.Name = i.Name.Trim(); rule.MinAmount = Math.Max(0, i.MinAmount);
            rule.LocationId = i.LocationId; rule.IsActive = i.IsActive;
            db.ApprovalRuleSteps.RemoveRange(existingSteps);   // replace the level set wholesale
            foreach (var s in i.Steps!.OrderBy(x => x.Level))
                db.ApprovalRuleSteps.Add(new ApprovalRuleStep
                {
                    RuleId = rule.Id, Level = s.Level <= 0 ? 1 : s.Level,
                    ApproverType = s.ApproverType is "user" or "role" or "email" ? s.ApproverType : "role",
                    ApproverUserId = s.ApproverUserId, ApproverRole = s.ApproverRole, ApproverEmail = s.ApproverEmail?.Trim(),
                    ApproverLabel = string.IsNullOrWhiteSpace(s.ApproverLabel) ? "Approver" : s.ApproverLabel.Trim(),
                });
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { rule.Id });
        }).WithName("Approvals.Rules.Upsert");

        cfg.MapDelete("/{id:guid}", async (Guid id, ITenantDbContextFactory f, CancellationToken ct) =>
        {
            await using var db = await f.CreateForCurrentAsync(ct);
            var r = await db.ApprovalRules.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (r is null) return Results.NotFound();
            r.IsActive = false; r.IsDeleted = true;
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        }).WithName("Approvals.Rules.Delete");

        // ── In-app inbox + decide (any authenticated approver) ──
        var ap = app.MapGroup("/api/v1/approvals").WithTags("Approvals").RequireAuthorization();

        ap.MapGet("/inbox", async (ITenantDbContextFactory f, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var role = PermissionService.RoleIdOf(user);
            Guid.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value, out var uid);
            await using var db = await f.CreateForCurrentAsync(ct);
            var pending = await db.ApprovalRequests.Where(r => r.Status == "pending").OrderBy(r => r.CreatedAt).ToListAsync(ct);
            var ruleIds = pending.Select(r => r.RuleId).Distinct().ToList();
            var steps = await db.ApprovalRuleSteps.Where(s => ruleIds.Contains(s.RuleId) && !s.IsDeleted).ToListAsync(ct);
            var mine = pending.Where(r => steps.Any(s => s.RuleId == r.RuleId && s.Level == r.CurrentLevel
                && ((s.ApproverType == "user" && s.ApproverUserId == uid) || (s.ApproverType == "role" && s.ApproverRole == role)))).ToList();
            return Results.Ok(mine.Select(r => new { r.Id, r.DocType, r.DocNumber, r.Amount, r.DocSummary, r.RequestedByName, r.CurrentLevel, r.CreatedAt }));
        }).WithName("Approvals.Inbox");

        // Server-paginated, tabbed view of the requests relevant to the logged-in user:
        // "pending"/"on_hold" are scoped to requests where this user is an eligible approver
        // at the current level (same eligibility check as the inbox above); "approved"/"rejected"
        // are the full tenant-wide decision history (an approver's identity on a past decision is
        // only ever recorded as a display label, not a stable user id, so those two can't be
        // reliably scoped to "mine" — they're shown as a shared audit trail instead).
        ap.MapGet("", async (
            ITenantDbContextFactory f, ClaimsPrincipal user, CancellationToken ct,
            string status = "pending", int pageNumber = 1, int pageSize = 10) =>
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            status = (status ?? "pending").Trim().ToLowerInvariant();
            if (status is not ("pending" or "on_hold" or "approved" or "rejected"))
                return Results.BadRequest(new { error = "status must be pending, on_hold, approved, or rejected" });

            await using var db = await f.CreateForCurrentAsync(ct);
            var all = await db.ApprovalRequests.Where(r => r.Status == status).OrderByDescending(r => r.CreatedAt).ToListAsync(ct);

            List<ApprovalRequest> scoped;
            if (status is "pending" or "on_hold")
            {
                var role = PermissionService.RoleIdOf(user);
                Guid.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value, out var uid);
                var ruleIds = all.Select(r => r.RuleId).Distinct().ToList();
                var steps = await db.ApprovalRuleSteps.Where(s => ruleIds.Contains(s.RuleId) && !s.IsDeleted).ToListAsync(ct);
                scoped = all.Where(r => steps.Any(s => s.RuleId == r.RuleId && s.Level == r.CurrentLevel
                    && ((s.ApproverType == "user" && s.ApproverUserId == uid) || (s.ApproverType == "role" && s.ApproverRole == role)))).ToList();
            }
            else
            {
                scoped = all;
            }

            var totalCount = scoped.Count;
            var page = scoped.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            // On Hold rows carry the reason the holding approver gave — pulled from that level's
            // ApprovalAction, since the remark belongs to the decision, not the request itself.
            Dictionary<Guid, string?> holdReasonByRequest = new();
            if (status == "on_hold" && page.Count > 0)
            {
                var pageIds = page.Select(r => r.Id).ToList();
                holdReasonByRequest = await db.ApprovalActions
                    .Where(a => pageIds.Contains(a.RequestId) && a.Status == "on_hold")
                    .OrderByDescending(a => a.ActedAt)
                    .GroupBy(a => a.RequestId)
                    .Select(g => new { RequestId = g.Key, Remark = g.First().Remark })
                    .ToDictionaryAsync(x => x.RequestId, x => x.Remark, ct);
            }

            var data = page
                .Select(r => new {
                    r.Id, r.DocType, r.DocNumber, r.Amount, r.DocSummary, r.RequestedByName, r.CurrentLevel, r.CreatedAt, r.Status, r.DecidedAt,
                    holdReason = holdReasonByRequest.GetValueOrDefault(r.Id),
                })
                .ToList();

            return Results.Ok(new
            {
                data,
                pagination = new { totalCount, pageNumber, pageSize, totalPages = (int)Math.Ceiling(totalCount / (double)Math.Max(1, pageSize)) },
            });
        }).WithName("Approvals.ListPaged");

        ap.MapPost("/{requestId:guid}/decide", async (Guid requestId, DecideInput body, ApprovalService svc, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var role = PermissionService.RoleIdOf(user);
            Guid.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value, out var uid);
            var name = user.FindFirst("name")?.Value ?? user.FindFirst(ClaimTypes.Email)?.Value ?? "Approver";
            var r = await svc.DecideInAppAsync(requestId, uid, role, name, body.Action, body.Remark, ct);
            return r.Ok ? Results.Ok(new { status = r.Status }) : Results.BadRequest(new { error = r.Error });
        }).WithName("Approvals.Decide");

        // ── Public, no-login link (backs the /approve/{token} page) ──
        var pub = app.MapGroup("/api/v1/approvals/link").WithTags("Approvals").AllowAnonymous();

        pub.MapGet("/{token}", async (string token, ApprovalService svc, CancellationToken ct) =>
            await svc.GetByTokenAsync(token, ct) is { } v ? Results.Ok(v) : Results.NotFound(new { error = "This approval link is invalid or has expired." }))
            .WithName("Approvals.Link.Get");

        pub.MapPost("/{token}", async (string token, DecideInput body, ApprovalService svc, CancellationToken ct) =>
        {
            var r = await svc.DecideByTokenAsync(token, body.Action, body.Remark, ct);
            return r.Ok ? Results.Ok(new { status = r.Status }) : Results.BadRequest(new { error = r.Error });
        }).WithName("Approvals.Link.Decide");

        return app;
    }
}

public record ApprovalRuleInput(Guid? Id, string DocType, string Name, decimal MinAmount, Guid? LocationId, bool IsActive, List<RuleStepInput>? Steps);
public record RuleStepInput(int Level, string ApproverType, Guid? ApproverUserId, int? ApproverRole, string? ApproverEmail, string? ApproverLabel);
public record DecideInput(string Action, string? Remark);
