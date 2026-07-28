using System.Text.Json;
using Hms.Api.Domain;
using Hms.Api.Features.Approvals;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.RequestNotes;

/// <summary>
/// Request Notes are a requisition raised by one location against another, upstream of
/// actual fulfillment — "po" mode is fulfilled by raising a Purchase Order, "transfer" mode
/// by raising a Transfer of Goods. The document itself never touches stock; approving it
/// only authorises it to be picked up downstream. Same draft → submit → pending/approved →
/// approve/reject → remove workflow as stock adjustments and wastage notes.
/// </summary>
public class RequestNoteService(ITenantDbContextFactory factory, ApprovalService approvals)
{
    public async Task<RequestNote> CreateDraftAsync(RequestNoteDraftInput input, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var settings = await SettingsAsync(db, ct);
        var note = new RequestNote
        {
            RequestNumber = await NextNumberAsync(db, "REQ", settings.RequestNotePrefix, ct),
            Status = "draft",
        };
        await PopulateDraftAsync(db, note, input, ct);
        db.RequestNotes.Add(note);
        await db.SaveChangesAsync(ct);
        return note;
    }

    /// <summary>Edit a DRAFT request note: replace its lines and header fields.</summary>
    public async Task<RequestNote> EditDraftAsync(Guid id, RequestNoteDraftInput input, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var note = await db.RequestNotes.Include(r => r.Lines).FirstOrDefaultAsync(r => r.Id == id, ct)
            ?? throw new InvalidOperationException("Request note not found");
        if (note.Status != "draft") throw new InvalidOperationException($"Only a draft request note can be edited (is {note.Status})");
        db.RequestNoteLines.RemoveRange(note.Lines);
        note.Lines.Clear();
        await PopulateDraftAsync(db, note, input, ct);
        await db.SaveChangesAsync(ct);
        return note;
    }

    /// <summary>
    /// Submit a draft for processing. A configured multi-level <see cref="ApprovalRule"/> (#approvals)
    /// takes priority — if one matches, it blocks on that rule chain instead. Otherwise falls back to
    /// the simple org-setting: resolves straight to "approved" if no approval policy applies, otherwise
    /// to "pending", where it waits for ApproveAsync or RejectAsync. Never touches stock either way —
    /// there's nothing to post.
    /// </summary>
    public async Task<RequestNote> SubmitAsync(Guid id, Guid? requestedBy, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var note = await db.RequestNotes.FirstOrDefaultAsync(r => r.Id == id, ct)
            ?? throw new InvalidOperationException("Request note not found");
        if (note.Status != "draft") throw new InvalidOperationException($"Only a draft request note can be submitted (is {note.Status})");
        note.SubmittedAt = DateTime.UtcNow;

        var summary = JsonSerializer.Serialize(new { note.RequestNumber, note.Mode });
        var ruleResult = await approvals.SubmitAsync(ApprovalService.DocRequestNote, note.Id, note.RequestNumber,
            0m, summary, note.FromLocationId, requestedBy, null, ct);
        if (ruleResult.Required)
        {
            note.Status = "pending";
            await db.SaveChangesAsync(ct);
            return note;
        }

        var settings = await SettingsAsync(db, ct);
        if (settings.RequireRequestNoteApproval)
        {
            note.Status = "pending";
        }
        else
        {
            note.Status = "approved";
            note.ApprovedAt = DateTime.UtcNow;
        }
        await db.SaveChangesAsync(ct);
        return note;
    }

    /// <summary>
    /// Approve a request note awaiting authorisation. If a multi-level <see cref="ApprovalRule"/> is
    /// driving this note (an open <see cref="ApprovalRequest"/> exists), this routes through the same
    /// decision engine as the Approval-rules inbox — gated to that request's current-level approvers
    /// rather than anyone with supply-chain access.
    /// </summary>
    public async Task<RequestNote> ApproveAsync(Guid id, Guid? approverId, int approverRole, string approverName, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var requestId = await db.ApprovalRequests
            .Where(r => r.DocType == ApprovalService.DocRequestNote && r.DocId == id && r.Status == "pending")
            .Select(r => (Guid?)r.Id).FirstOrDefaultAsync(ct);
        if (requestId is Guid rid)
        {
            var result = await approvals.DecideInAppAsync(rid, approverId ?? Guid.Empty, approverRole, approverName, "approve", null, ct);
            if (!result.Ok) throw new InvalidOperationException(result.Error ?? "You can't approve this request note.");
            return await db.RequestNotes.FirstAsync(r => r.Id == id, ct);
        }

        var note = await db.RequestNotes.FirstOrDefaultAsync(r => r.Id == id, ct)
            ?? throw new InvalidOperationException("Request note not found");
        if (note.Status != "pending") throw new InvalidOperationException("Request note is not awaiting approval");
        note.Status = "approved"; note.ApprovedBy = approverId; note.ApprovedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return note;
    }

    public async Task<RequestNote> RejectAsync(Guid id, Guid? rejecterId, int approverRole, string approverName, string? reason, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var note = await db.RequestNotes.FirstOrDefaultAsync(r => r.Id == id, ct)
            ?? throw new InvalidOperationException("Request note not found");
        if (note.Status != "pending") throw new InvalidOperationException("Request note is not awaiting approval");

        var requestId = await db.ApprovalRequests
            .Where(r => r.DocType == ApprovalService.DocRequestNote && r.DocId == id && r.Status == "pending")
            .Select(r => (Guid?)r.Id).FirstOrDefaultAsync(ct);
        if (requestId is Guid rid)
        {
            var result = await approvals.DecideInAppAsync(rid, rejecterId ?? Guid.Empty, approverRole, approverName, "reject", reason, ct);
            if (!result.Ok) throw new InvalidOperationException(result.Error ?? "You can't reject this request note.");
            return await db.RequestNotes.FirstAsync(r => r.Id == id, ct);
        }

        note.Status = "rejected"; note.RejectedBy = rejecterId; note.RejectedAt = DateTime.UtcNow; note.RejectReason = reason;
        await db.SaveChangesAsync(ct);
        return note;
    }

    /// <summary>Soft-delete a request note. Blocked once approved — it may already be in use downstream.</summary>
    public async Task RemoveAsync(Guid id, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var note = await db.RequestNotes.FirstOrDefaultAsync(r => r.Id == id, ct)
            ?? throw new InvalidOperationException("Request note not found");
        if (note.Status is "approved" or "fulfilled") throw new InvalidOperationException($"Cannot remove a {note.Status} request note");
        note.IsDeleted = true;
        await db.SaveChangesAsync(ct);
    }

    public async Task<RequestNote?> GetAsync(Guid id, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        return await db.RequestNotes.Include(r => r.Lines).FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<PagedRequestNoteResult> GetPagedAsync(
        int pageNumber, int pageSize, string? search, Guid? fromLocationId, Guid? toLocationId,
        string? mode, string? status, DateOnly? dateFrom, DateOnly? dateTo, CancellationToken ct)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        await using var db = await factory.CreateForCurrentAsync(ct);

        var query =
            from r in db.RequestNotes.AsNoTracking()
            join fl in db.Locations.AsNoTracking() on r.FromLocationId equals fl.Id
            join tl in db.Locations.AsNoTracking() on r.ToLocationId equals tl.Id
            select new { r, FromCode = fl.Code, FromName = fl.Name, ToCode = tl.Code, ToName = tl.Name };

        if (fromLocationId.HasValue) query = query.Where(x => x.r.FromLocationId == fromLocationId.Value);
        if (toLocationId.HasValue) query = query.Where(x => x.r.ToLocationId == toLocationId.Value);
        if (!string.IsNullOrWhiteSpace(mode)) query = query.Where(x => x.r.Mode == mode);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.r.Status == status);
        if (dateFrom.HasValue) query = query.Where(x => DateOnly.FromDateTime(x.r.CreatedAt) >= dateFrom.Value);
        if (dateTo.HasValue) query = query.Where(x => DateOnly.FromDateTime(x.r.CreatedAt) <= dateTo.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s2 = $"%{search.Trim()}%";
            query = query.Where(x => EF.Functions.ILike(x.r.RequestNumber, s2) || EF.Functions.ILike(x.r.CommonRemark ?? "", s2));
        }

        var totalCount = await query.CountAsync(ct);

        var data = await query
            .OrderByDescending(x => x.r.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new RequestNoteListDto(
                x.r.Id, x.r.RequestNumber, x.r.Mode,
                x.r.FromLocationId, x.FromCode, x.FromName,
                x.r.ToLocationId, x.ToCode, x.ToName,
                x.r.ExpectedDeliveryDate, x.r.Status, x.r.CommonRemark,
                x.r.SubmittedAt, x.r.ApprovedAt, x.r.RejectedAt, x.r.RejectReason, x.r.PurchaseOrderId, x.r.TransferId, x.r.CreatedAt))
            .ToListAsync(ct);

        return new PagedRequestNoteResult(
            data,
            new PaginationMeta(totalCount, pageNumber, pageSize, (int)Math.Ceiling(totalCount / (double)pageSize)));
    }

    /// <summary>Builds/rebuilds a draft's header + lines from input (shared by create + edit).</summary>
    private static async Task PopulateDraftAsync(TenantDbContext db, RequestNote note, RequestNoteDraftInput input, CancellationToken ct)
    {
        if (input.Mode is not ("po" or "transfer")) throw new InvalidOperationException("Invalid mode");
        if (input.FromLocationId == input.ToLocationId)
            throw new InvalidOperationException("From and To locations must differ");
        foreach (var locId in new[] { input.FromLocationId, input.ToLocationId })
            if (!await db.Locations.AnyAsync(l => l.Id == locId, ct))
                throw new InvalidOperationException("Location not found");
        if (input.ExpectedDeliveryDate < DateOnly.FromDateTime(DateTime.UtcNow))
            throw new InvalidOperationException("Expected delivery date cannot be in the past");
        if (input.Lines is null || input.Lines.Count == 0)
            throw new InvalidOperationException("A request note needs at least one line");

        note.Mode = input.Mode;
        note.FromLocationId = input.FromLocationId;
        note.ToLocationId = input.ToLocationId;
        note.ExpectedDeliveryDate = input.ExpectedDeliveryDate;
        note.CommonRemark = input.CommonRemark;

        foreach (var l in input.Lines)
        {
            var p = await db.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == l.ProductId, ct)
                ?? throw new InvalidOperationException($"Product {l.ProductId} not found");
            if (l.Quantity <= 0) throw new InvalidOperationException("Each line needs a quantity greater than zero");

            var stock = await db.ProductStocks.AsNoTracking().FirstOrDefaultAsync(
                s => s.ProductId == p.Id && s.LocationId == input.FromLocationId, ct);
            note.Lines.Add(new RequestNoteLine
            {
                ProductId = p.Id, Sku = p.Sku, ProductName = p.Name,
                Sih = stock?.QuantityOnHand ?? 0, Quantity = l.Quantity, Remark = l.Remark,
            });
        }
    }

    private static async Task<OrgSettings> SettingsAsync(TenantDbContext db, CancellationToken ct)
        => await db.OrgSettings.AsNoTracking().FirstOrDefaultAsync(ct) ?? new OrgSettings();

    private static async Task<string> NextNumberAsync(TenantDbContext db, string counterKey, string prefix, CancellationToken ct)
    {
        var rows = await db.Database.SqlQuery<int>($@"
            INSERT INTO document_counters (tenant_id, doc_type, last_value)
            VALUES ({db.TenantId}, {counterKey}, 1)
            ON CONFLICT (tenant_id, doc_type)
            DO UPDATE SET last_value = document_counters.last_value + 1
            RETURNING last_value AS ""Value""").ToListAsync(ct);
        return $"{prefix}-{rows[0]:D4}";
    }
}

public record RequestNoteDraftInput(
    string Mode, Guid FromLocationId, Guid ToLocationId, DateOnly ExpectedDeliveryDate,
    string? CommonRemark, List<RequestNoteDraftLineInput> Lines);
public record RequestNoteDraftLineInput(Guid ProductId, decimal Quantity, string? Remark);

public record PaginationMeta(int TotalCount, int PageNumber, int PageSize, int TotalPages);
public record PagedRequestNoteResult(List<RequestNoteListDto> Data, PaginationMeta Pagination);
public record RequestNoteListDto(
    Guid Id, string RequestNumber, string Mode,
    Guid FromLocationId, string FromLocationCode, string FromLocationName,
    Guid ToLocationId, string ToLocationCode, string ToLocationName,
    DateOnly ExpectedDeliveryDate, string Status, string? CommonRemark,
    DateTime? SubmittedAt, DateTime? ApprovedAt, DateTime? RejectedAt, string? RejectReason, Guid? PurchaseOrderId, Guid? TransferId, DateTime CreatedAt);
