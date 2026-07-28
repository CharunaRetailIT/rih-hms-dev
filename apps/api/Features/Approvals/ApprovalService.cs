using System.Security.Cryptography;
using Hms.Api.Domain;
using Hms.Api.Features.Email;
using Hms.Api.Features.Inventory;
using Hms.Api.Features.Procurement;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.Approvals;

/// <summary>
/// Configurable approval workflow engine (#approvals). Documents (PO / GRN / transfer) call
/// <see cref="SubmitAsync"/> when submitted; if a tenant rule matches, a request is created, the
/// document is blocked, and each level's approvers get an email LINK to Approve/Reject/Hold
/// (no login). On final approval the engine releases the document via the matching releaser.
/// </summary>
public class ApprovalService(ITenantDbContextFactory factory, IEmailSender email, IConfiguration cfg, IServiceProvider sp, ILogger<ApprovalService> log)
{
    public const string DocPurchaseOrder = "purchase_order";
    public const string DocGrn = "grn";
    public const string DocTransfer = "stock_transfer";
    public const string DocWastage = "wastage_note";
    public const string DocRequestNote = "request_note";
    public const string DocStockAdjustment = "stock_adjustment";

    public record SubmitResult(bool Required, Guid? RequestId, int Levels);

    // Token carries its tenant ("{tenantId:N}.{random}") so an ANONYMOUS link request can resolve
    // which tenant DB to open (no logged-in tenant context on the public approval page).
    private static string NewToken(Guid tenantId) =>
        $"{tenantId:N}." + Convert.ToBase64String(RandomNumberGenerator.GetBytes(18)).Replace('+', '-').Replace('/', '_').TrimEnd('=');

    private static bool TryTenantFromToken(string token, out Guid tenantId)
    {
        tenantId = Guid.Empty;
        var dot = token?.IndexOf('.') ?? -1;
        return dot == 32 && Guid.TryParseExact(token![..32], "N", out tenantId);
    }

    private async Task<ApprovalRule?> MatchRuleAsync(TenantDbContext db, string docType, decimal amount, Guid? locationId, CancellationToken ct)
    {
        var rules = await db.ApprovalRules.Where(r => r.IsActive && r.DocType == docType).ToListAsync(ct);
        await PopulateStepsAsync(db, rules, ct);
        // Most specific (highest threshold, location-matched) rule that has steps and whose threshold is met.
        return rules
            .Where(r => amount >= r.MinAmount && r.Steps.Any(s => !s.IsDeleted)
                && (r.LocationId == null || r.LocationId == locationId))
            .OrderByDescending(r => r.LocationId != null)
            .ThenByDescending(r => r.MinAmount)
            .FirstOrDefault();
    }

    private static async Task PopulateStepsAsync(TenantDbContext db, List<ApprovalRule> rules, CancellationToken ct)
    {
        if (rules.Count == 0) return;
        var ids = rules.Select(r => r.Id).ToList();
        var steps = await db.ApprovalRuleSteps.Where(x => ids.Contains(x.RuleId) && !x.IsDeleted).ToListAsync(ct);
        foreach (var r in rules) r.Steps = steps.Where(x => x.RuleId == r.Id).OrderBy(x => x.Level).ToList();
    }

    /// <summary>True if the given document type has at least one active rule (so callers can decide
    /// whether to even build a summary). Cheap pre-check.</summary>
    public async Task<bool> AnyRuleAsync(string docType, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        return await db.ApprovalRules.AnyAsync(r => r.IsActive && r.DocType == docType && !r.IsDeleted, ct);
    }

    /// <summary>Create an approval request for a document if a rule matches (else Required=false → caller
    /// proceeds). Idempotent: returns the existing pending request if one already exists for the doc.</summary>
    public async Task<SubmitResult> SubmitAsync(string docType, Guid docId, string docNumber, decimal amount,
        string summaryJson, Guid? locationId, Guid? requestedBy, string? requestedByName, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var existing = await db.ApprovalRequests.FirstOrDefaultAsync(r => r.DocType == docType && r.DocId == docId && r.Status == "pending", ct);
        if (existing is not null) return new(true, existing.Id, 0);

        var rule = await MatchRuleAsync(db, docType, amount, locationId, ct);
        if (rule is null) return new(false, null, 0);

        var req = new ApprovalRequest
        {
            DocType = docType, DocId = docId, DocNumber = docNumber, DocSummary = summaryJson,
            Amount = amount, RuleId = rule.Id, Status = "pending", CurrentLevel = 1,
            RequestedBy = requestedBy, RequestedByName = requestedByName,
        };
        db.ApprovalRequests.Add(req);
        await db.SaveChangesAsync(ct);
        await SpawnLevelAsync(db, req, rule, 1, ct);
        await db.SaveChangesAsync(ct);
        var levels = rule.Steps.Where(s => !s.IsDeleted).Select(s => s.Level).Distinct().Count();
        return new(true, req.Id, levels);
    }

    private async Task SpawnLevelAsync(TenantDbContext db, ApprovalRequest req, ApprovalRule rule, int level, CancellationToken ct)
    {
        foreach (var s in rule.Steps.Where(s => !s.IsDeleted && s.Level == level))
        {
            var act = new ApprovalAction
            {
                RequestId = req.Id, Level = level, ApproverType = s.ApproverType, ApproverLabel = s.ApproverLabel,
                ApproverEmail = s.ApproverEmail, Token = NewToken(db.TenantId), TokenExpiresAt = DateTime.UtcNow.AddDays(7), Status = "pending",
            };
            db.ApprovalActions.Add(act);
            foreach (var to in await RecipientEmailsAsync(db, s, ct))
                await SendLinkAsync(to, req, act, ct);
        }
    }

    private async Task<List<string>> RecipientEmailsAsync(TenantDbContext db, ApprovalRuleStep s, CancellationToken ct)
    {
        if (s.ApproverType == "email" && !string.IsNullOrWhiteSpace(s.ApproverEmail)) return [s.ApproverEmail!];
        if (s.ApproverType == "user" && s.ApproverUserId is Guid uid)
        {
            var e = await db.Users.AsNoTracking().Where(u => u.Id == uid && u.Email != null && u.Email != "").Select(u => u.Email!).FirstOrDefaultAsync(ct);
            return e is null ? [] : [e];
        }
        if (s.ApproverType == "role" && s.ApproverRole is int role)
            return await db.Users.AsNoTracking().Where(u => (int)u.Role == role && u.IsActive && u.Email != null && u.Email != "").Select(u => u.Email!).ToListAsync(ct);
        return [];
    }

    private async Task SendLinkAsync(string to, ApprovalRequest req, ApprovalAction act, CancellationToken ct)
    {
        var baseUrl = (cfg["App:WebBaseUrl"] ?? "https://hms.retailit.lk").TrimEnd('/');
        var link = $"{baseUrl}/approve/{act.Token}";
        try { await email.SendAsync(to, $"Approval needed — {req.DocNumber}", EmailTemplates.ApprovalLink(req.DocType, req.DocNumber, req.Amount, link), ct); }
        catch (Exception ex) { log.LogWarning(ex, "Approval-link email to {To} failed", to); }
    }

    public record DecideResult(bool Ok, string? Error, string Status);

    /// <summary>Act on an approval via an email-link token (no login). Approve advances/closes the
    /// request; Reject/Hold require a remark. Single-use: a decided token can't be reused.</summary>
    public async Task<DecideResult> DecideByTokenAsync(string token, string action, string? remark, CancellationToken ct)
    {
        if (!TryTenantFromToken(token, out var tid)) return new(false, "This approval link is invalid.", "");
        TenantDbContext db;
        try { db = factory.Create(tid); } catch { return new(false, "This approval link is invalid.", ""); }
        await using var _ = db;
        var act = await db.ApprovalActions.FirstOrDefaultAsync(a => a.Token == token, ct);
        if (act is null) return new(false, "This approval link is invalid.", "");
        if (act.Status != "pending") return new(false, "This link has already been used.", act.Status);
        if (act.TokenExpiresAt < DateTime.UtcNow) return new(false, "This approval link has expired.", "expired");
        var req = await db.ApprovalRequests.FirstOrDefaultAsync(r => r.Id == act.RequestId, ct);
        if (req is null || req.Status != "pending") return new(false, "This request is no longer open.", req?.Status ?? "");
        return await ApplyDecisionAsync(db, req, act, action, remark, act.ApproverLabel, ct);
    }

    /// <summary>Act on an approval from inside the app (a logged-in manager). Finds the caller's pending
    /// action at the current level (by user or by role).</summary>
    public async Task<DecideResult> DecideInAppAsync(Guid requestId, Guid userId, int userRole, string actorName, string action, string? remark, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var req = await db.ApprovalRequests.FirstOrDefaultAsync(r => r.Id == requestId, ct);
        if (req is null || req.Status != "pending") return new(false, "This request is no longer open.", req?.Status ?? "");
        var rule = await db.ApprovalRules.FirstOrDefaultAsync(r => r.Id == req.RuleId, ct);
        if (rule is not null) await PopulateStepsAsync(db, new() { rule }, ct);
        var steps = rule?.Steps.Where(s => !s.IsDeleted && s.Level == req.CurrentLevel).ToList() ?? new();
        var allowed = steps.Any(s => (s.ApproverType == "user" && s.ApproverUserId == userId) || (s.ApproverType == "role" && s.ApproverRole == userRole));
        if (!allowed) return new(false, "You're not an approver for this step.", req.Status);
        var act = await db.ApprovalActions.FirstOrDefaultAsync(a => a.RequestId == req.Id && a.Level == req.CurrentLevel && a.Status == "pending", ct);
        if (act is null) return new(false, "No pending approval for this step.", req.Status);
        return await ApplyDecisionAsync(db, req, act, action, remark, actorName, ct);
    }

    private async Task<DecideResult> ApplyDecisionAsync(TenantDbContext db, ApprovalRequest req, ApprovalAction act, string action, string? remark, string actorLabel, CancellationToken ct)
    {
        action = (action ?? "").Trim().ToLowerInvariant();
        if (action is not ("approve" or "reject" or "hold")) return new(false, "Choose approve, reject or hold.", req.Status);
        if (action is "reject" or "hold" && string.IsNullOrWhiteSpace(remark))
            return new(false, "A remark is required to reject or hold.", req.Status);

        var now = DateTime.UtcNow;
        act.Remark = remark; act.ActedBy = actorLabel; act.ActedAt = now;
        act.Status = action == "approve" ? "approved" : action == "reject" ? "rejected" : "on_hold";

        if (action == "reject") { req.Status = "rejected"; req.DecidedAt = now; }
        else if (action == "hold") { req.Status = "on_hold"; }
        else
        {
            // Approved this action. Level passes when every pending action at this level is decided-approved.
            var levelActions = await db.ApprovalActions.Where(a => a.RequestId == req.Id && a.Level == req.CurrentLevel).ToListAsync(ct);
            var levelDone = levelActions.All(a => a.Status == "approved");
            if (levelDone)
            {
                var rule = await db.ApprovalRules.FirstOrDefaultAsync(r => r.Id == req.RuleId, ct);
                await PopulateStepsAsync(db, rule is null ? new() : new() { rule }, ct);
                var nextLevel = rule!.Steps.Where(s => !s.IsDeleted && s.Level > req.CurrentLevel).Select(s => s.Level).DefaultIfEmpty(0).Min();
                if (nextLevel > 0) { req.CurrentLevel = nextLevel; await SpawnLevelAsync(db, req, rule, nextLevel, ct); }
                else { req.Status = "approved"; req.DecidedAt = now; }
            }
        }
        await db.SaveChangesAsync(ct);

        // On a terminal decision, let the source document proceed (approved) or unblock it as
        // rejected (so it isn't left stuck "pending" forever) — "on_hold" leaves it as-is.
        if (req.Status is "approved" or "rejected") await ReleaseAsync(req, act.Remark, ct);
        return new(true, null, req.Status);
    }

    /// <summary>Read an approval by its link token (for the public page) — summary + current status.</summary>
    public async Task<object?> GetByTokenAsync(string token, CancellationToken ct)
    {
        if (!TryTenantFromToken(token, out var tid)) return null;
        TenantDbContext db;
        try { db = factory.Create(tid); } catch { return null; }
        await using var _ = db;
        var act = await db.ApprovalActions.AsNoTracking().FirstOrDefaultAsync(a => a.Token == token, ct);
        if (act is null) return null;
        var req = await db.ApprovalRequests.AsNoTracking().FirstOrDefaultAsync(r => r.Id == act.RequestId, ct);
        if (req is null) return null;
        var expired = act.TokenExpiresAt < DateTime.UtcNow;
        return new
        {
            req.DocType, req.DocNumber, req.Amount, req.DocSummary, req.RequestedByName,
            requestStatus = req.Status, actionStatus = act.Status, level = act.Level,
            decidable = act.Status == "pending" && !expired && req.Status == "pending", expired,
        };
    }

    /// <summary>
    /// Let the source document proceed now that its rule chain reached a terminal state: fully
    /// approved (posts to stock for a GRN/transfer, just flips status for a PO) or rejected (unblocks
    /// it as "rejected" instead of leaving it stuck "pending" forever — nothing to reverse, since a
    /// pending doc never touched stock). Runs in BOTH the in-app and the anonymous-link path, so we
    /// open the tenant DB by id (req.TenantId) rather than relying on a request tenant-context — that's
    /// also why this reaches into ProcurementService/InventoryMovementService's internal stock-posting
    /// helpers directly instead of calling back into those services (which assume an ambient tenant).
    /// </summary>
    private async Task ReleaseAsync(ApprovalRequest req, string? rejectReason, CancellationToken ct)
    {
        try
        {
            await using var db = factory.Create(req.TenantId);
            var approved = req.Status == "approved";
            var now = DateTime.UtcNow;

            if (req.DocType == DocPurchaseOrder)
            {
                var po = await db.PurchaseOrders.FirstOrDefaultAsync(p => p.Id == req.DocId, ct);
                if (po is not null && po.Status == "pending")
                {
                    if (approved) { po.Status = "approved"; po.ApprovedAt = now; }
                    else { po.Status = "rejected"; po.RejectedAt = now; po.RejectReason = rejectReason; }
                    await db.SaveChangesAsync(ct);
                }
            }
            else if (req.DocType == DocGrn)
            {
                var grn = await db.GoodsReceivedNotes.Include(g => g.Lines).FirstOrDefaultAsync(g => g.Id == req.DocId, ct);
                if (grn is not null && grn.Status == "pending")
                {
                    if (approved)
                    {
                        await ProcurementService.ApplyGrnToStockAsync(db, grn, ct);
                        grn.Status = "approved"; grn.PostedAt = now; grn.ApprovedAt = now;
                    }
                    else { grn.Status = "rejected"; grn.RejectedAt = now; grn.RejectReason = rejectReason; }
                    await db.SaveChangesAsync(ct);
                }
            }
            else if (req.DocType == DocTransfer)
            {
                var t = await db.StockTransfers.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == req.DocId, ct);
                if (t is not null && t.Status == "pending")
                {
                    if (approved)
                    {
                        await InventoryMovementService.DispatchStockAsync(db, t, ct);
                        t.Status = "dispatched"; t.DispatchedAt = now; t.ApprovedAt = now;
                    }
                    else { t.Status = "rejected"; t.RejectedAt = now; t.RejectReason = rejectReason; }
                    await db.SaveChangesAsync(ct);
                }
            }
            else if (req.DocType == DocWastage)
            {
                var w = await db.WastageNotes.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == req.DocId, ct);
                if (w is not null && w.Status == "pending")
                {
                    if (approved)
                    {
                        await InventoryMovementService.ApplyWastageToStockAsync(db, w, ct);
                        w.Status = "approved"; w.ApprovedAt = now;
                    }
                    else { w.Status = "rejected"; w.RejectedAt = now; w.RejectReason = rejectReason; }
                    await db.SaveChangesAsync(ct);
                }
            }
            else if (req.DocType == DocRequestNote)
            {
                var rn = await db.RequestNotes.FirstOrDefaultAsync(x => x.Id == req.DocId, ct);
                if (rn is not null && rn.Status == "pending")
                {
                    if (approved) { rn.Status = "approved"; rn.ApprovedAt = now; }
                    else { rn.Status = "rejected"; rn.RejectedAt = now; rn.RejectReason = rejectReason; }
                    await db.SaveChangesAsync(ct);
                }
            }
            else if (req.DocType == DocStockAdjustment)
            {
                var adj = await db.StockAdjustments.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == req.DocId, ct);
                if (adj is not null && adj.Status == "pending")
                {
                    if (approved)
                    {
                        await InventoryMovementService.ApplyAdjustmentToStockAsync(db, adj, ct);
                        adj.Status = "approved"; adj.ApprovedAt = now;
                    }
                    else { adj.Status = "rejected"; adj.RejectedAt = now; adj.RejectReason = rejectReason; }
                    await db.SaveChangesAsync(ct);
                }
            }
        }
        catch (Exception ex) { log.LogError(ex, "Releasing {Status} {DocType} {DocId} failed", req.Status, req.DocType, req.DocId); }
    }
}
