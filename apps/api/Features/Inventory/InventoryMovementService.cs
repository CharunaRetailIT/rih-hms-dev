using System.Text.Json;
using Hms.Api.Domain;
using Hms.Api.Features.Approvals;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.Inventory;

/// <summary>
/// Supply chain step 6: inter-location transfers (+ returns), wastage, and
/// stock adjustments. All movements go through the same weighted-average-cost
/// engine introduced in step 5. Cost moves WITH goods on transfer; issues
/// (wastage, transfer-out, negative adjustment) leave the average untouched.
/// </summary>
public class InventoryMovementService(ITenantDbContextFactory factory, ApprovalService approvals)
{
    // ── transfers ──────────────────────────────────────────────────────────
    public async Task<StockTransfer> CreateTransferAsync(CreateTransferInput input, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var settings = await SettingsAsync(db, ct);
        var t = new StockTransfer
        {
            Id = Guid.NewGuid(),   // assigned up front so request notes can link to it in the same save
            // One shared counter so transfer + return numbers never collide; the
            // is_return flag distinguishes them. Both use the configured prefix.
            TransferNumber = await NextNumberAsync(db, "TRF", settings.TransferPrefix, ct),
            Status = "draft",
        };
        await PopulateTransferAsync(db, t, input, ct);
        db.StockTransfers.Add(t);

        if (input.RequestNoteIds is { Count: > 0 } ids)
            await LinkRequestNotesAsync(db, t, ids, ct);

        await db.SaveChangesAsync(ct);
        return t;
    }

    /// <summary>
    /// Bundles approved "transfer"-mode request notes into the TOG being created: every note must
    /// be approved, in "transfer" mode, and raised for exactly this From/To location pair (a
    /// transfer only moves stock between the two locations it's actually between, unlike a PO's
    /// single buying location). All get marked "fulfilled" and linked to the new transfer.
    /// </summary>
    private static async Task LinkRequestNotesAsync(TenantDbContext db, StockTransfer t, List<Guid> requestNoteIds, CancellationToken ct)
    {
        var notes = await db.RequestNotes.Where(r => requestNoteIds.Contains(r.Id)).ToListAsync(ct);
        if (notes.Count != requestNoteIds.Distinct().Count())
            throw new InvalidOperationException("One or more request notes could not be found");
        foreach (var n in notes)
        {
            if (n.Mode != "transfer") throw new InvalidOperationException($"{n.RequestNumber} is not a Transfer-mode request note");
            if (n.Status != "approved") throw new InvalidOperationException($"{n.RequestNumber} is not approved (is {n.Status})");
            if (n.FromLocationId != t.FromLocationId || n.ToLocationId != t.ToLocationId)
                throw new InvalidOperationException($"{n.RequestNumber} was raised for a different From/To location pair");
        }
        foreach (var n in notes) { n.Status = "fulfilled"; n.TransferId = t.Id; }
    }

    /// <summary>Edit a DRAFT transfer: replace its header/lines. Request-note bundling only happens on create.</summary>
    public async Task<StockTransfer> EditTransferAsync(Guid id, CreateTransferInput input, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var t = await db.StockTransfers.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new InvalidOperationException("Transfer not found");
        if (t.Status != "draft") throw new InvalidOperationException($"Only a draft transfer can be edited (is {t.Status})");
        db.StockTransferLines.RemoveRange(t.Lines);
        t.Lines.Clear();
        await PopulateTransferAsync(db, t, input, ct);
        await db.SaveChangesAsync(ct);
        return t;
    }

    /// <summary>Builds/rebuilds a transfer's header + lines from input (shared by create + edit). No stock impact yet;
    /// lines are costed at the sender's CURRENT avg cost as a preview (used for the approval threshold check and
    /// display) — dispatch re-costs against the LIVE avg cost at that time, which may have moved since.</summary>
    private static async Task PopulateTransferAsync(TenantDbContext db, StockTransfer t, CreateTransferInput input, CancellationToken ct)
    {
        if (input.FromLocationId == input.ToLocationId)
            throw new InvalidOperationException("Transfer source and destination must differ");
        if (input.Lines is null || input.Lines.Count == 0)
            throw new InvalidOperationException("A transfer needs at least one line");
        foreach (var locId in new[] { input.FromLocationId, input.ToLocationId })
            if (!await db.Locations.AnyAsync(l => l.Id == locId, ct))
                throw new InvalidOperationException("Location not found");

        t.FromLocationId = input.FromLocationId; t.ToLocationId = input.ToLocationId;
        t.IsReturn = input.IsReturn; t.Notes = input.Notes;
        t.TransferDate = input.TransferDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        t.ReferenceNo = input.ReferenceNo;

        decimal total = 0;
        foreach (var l in input.Lines)
        {
            var p = await db.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == l.ProductId, ct)
                ?? throw new InvalidOperationException($"Product {l.ProductId} not found");
            if (l.Quantity <= 0) throw new InvalidOperationException("Line quantity must be > 0");
            var stock = await db.ProductStocks.AsNoTracking().FirstOrDefaultAsync(
                s => s.ProductId == p.Id && s.LocationId == input.FromLocationId, ct);
            var cost = stock?.AverageCost ?? p.CostPrice;
            var lineTotal = l.Quantity * cost;
            total += lineTotal;
            t.Lines.Add(new StockTransferLine { ProductId = p.Id, Sku = p.Sku, ProductName = p.Name, Quantity = l.Quantity, UnitCost = cost, LineTotal = lineTotal });
        }
        t.TotalCost = total;
    }

    /// <summary>
    /// Submit a draft for processing. A configured multi-level <see cref="ApprovalRule"/> (#approvals)
    /// takes priority — if one matches this transfer's total cost/outlet, it blocks on that rule chain
    /// instead. Otherwise falls back to the simple org-setting: straight to "dispatched" (and decrements
    /// sender stock) if no policy applies (or the transfer's total cost crossing the configured
    /// threshold) — else "pending", where it waits for <see cref="ApproveTransferAsync"/> or
    /// <see cref="RejectTransferAsync"/>.
    /// </summary>
    public async Task<StockTransfer> SubmitTransferAsync(Guid id, Guid? requestedBy, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var t = await db.StockTransfers.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new InvalidOperationException("Transfer not found");
        if (t.Status != "draft") throw new InvalidOperationException($"Only a draft transfer can be submitted (is {t.Status})");
        t.SubmittedAt = DateTime.UtcNow;

        var summary = JsonSerializer.Serialize(new { t.TransferNumber, total = t.TotalCost });
        var ruleResult = await approvals.SubmitAsync(ApprovalService.DocTransfer, t.Id, t.TransferNumber,
            t.TotalCost, summary, t.FromLocationId, requestedBy, null, ct);
        if (ruleResult.Required)
        {
            t.Status = "pending";
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return t;
        }

        var settings = await SettingsAsync(db, ct);
        var requireApproval = settings.RequireTransferApproval
            || (settings.TransferApprovalThreshold > 0 && t.TotalCost >= settings.TransferApprovalThreshold);

        if (requireApproval)
        {
            t.Status = "pending";
        }
        else
        {
            await DispatchStockAsync(db, t, ct);
            t.Status = "dispatched"; t.DispatchedAt = DateTime.UtcNow; t.ApprovedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return t;
    }

    /// <summary>
    /// Approve a transfer awaiting authorisation — this is what actually dispatches it (decrements
    /// sender stock). If a multi-level <see cref="ApprovalRule"/> is actually driving this transfer
    /// (an open <see cref="ApprovalRequest"/> exists), this routes through the same decision engine as
    /// the Approval-rules inbox — gated to that request's current-level approvers (by role or named
    /// user) rather than anyone with supply-chain access. Only falls back to dispatching here directly
    /// for the simple org-setting threshold path (no rule matched).
    /// </summary>
    public async Task<StockTransfer> ApproveTransferAsync(Guid id, Guid? approverId, int approverRole, string approverName, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var requestId = await db.ApprovalRequests
            .Where(r => r.DocType == ApprovalService.DocTransfer && r.DocId == id && r.Status == "pending")
            .Select(r => (Guid?)r.Id).FirstOrDefaultAsync(ct);
        if (requestId is Guid rid)
        {
            var result = await approvals.DecideInAppAsync(rid, approverId ?? Guid.Empty, approverRole, approverName, "approve", null, ct);
            if (!result.Ok) throw new InvalidOperationException(result.Error ?? "You can't approve this transfer.");
            return await db.StockTransfers.Include(x => x.Lines).FirstAsync(x => x.Id == id, ct);
        }

        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var t = await db.StockTransfers.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new InvalidOperationException("Transfer not found");
        if (t.Status != "pending") throw new InvalidOperationException("Transfer is not awaiting approval");

        await DispatchStockAsync(db, t, ct);
        t.Status = "dispatched"; t.DispatchedAt = DateTime.UtcNow;
        t.ApprovedBy = approverId; t.ApprovedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return t;
    }

    /// <summary>Reject a transfer awaiting authorisation. It never touched stock, so there's nothing to reverse — final, cannot be edited.</summary>
    public async Task<StockTransfer> RejectTransferAsync(Guid id, Guid? rejecterId, int approverRole, string approverName, string? reason, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var t = await db.StockTransfers.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new InvalidOperationException("Transfer not found");
        if (t.Status != "pending") throw new InvalidOperationException("Transfer is not awaiting approval");

        var requestId = await db.ApprovalRequests
            .Where(r => r.DocType == ApprovalService.DocTransfer && r.DocId == id && r.Status == "pending")
            .Select(r => (Guid?)r.Id).FirstOrDefaultAsync(ct);
        if (requestId is Guid rid)
        {
            var result = await approvals.DecideInAppAsync(rid, rejecterId ?? Guid.Empty, approverRole, approverName, "reject", reason, ct);
            if (!result.Ok) throw new InvalidOperationException(result.Error ?? "You can't reject this transfer.");
            return await db.StockTransfers.FirstAsync(x => x.Id == id, ct);
        }

        t.Status = "rejected"; t.RejectedBy = rejecterId; t.RejectedAt = DateTime.UtcNow; t.RejectReason = reason;
        await db.SaveChangesAsync(ct);
        return t;
    }

    /// <summary>Decrement sender stock against LIVE on-hand/avg cost (not the draft-time preview snapshot, which may be
    /// stale). Shared by the no-approval-required submit path and the explicit approve path. Internal (not private)
    /// so <see cref="Hms.Api.Features.Approvals.ApprovalService"/> can dispatch the same way when a multi-level
    /// approval rule's final level clears (#approvals).</summary>
    internal static async Task DispatchStockAsync(TenantDbContext db, StockTransfer t, CancellationToken ct)
    {
        decimal total = 0;
        foreach (var line in t.Lines.Where(l => !l.IsDeleted))
        {
            var stock = await db.ProductStocks.FirstOrDefaultAsync(
                s => s.ProductId == line.ProductId && s.LocationId == t.FromLocationId, ct);
            if (stock is null || stock.QuantityOnHand < line.Quantity)
                throw new InvalidOperationException($"Insufficient stock to dispatch {line.ProductName} from source");
            line.UnitCost = stock.AverageCost;
            line.LineTotal = line.Quantity * line.UnitCost;
            total += line.LineTotal;
            stock.QuantityOnHand -= line.Quantity;
        }
        t.TotalCost = total;
    }

    /// <summary>Soft-delete a transfer that hasn't dispatched (draft, pending, or rejected — none have touched stock).</summary>
    public async Task RemoveTransferAsync(Guid id, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var t = await db.StockTransfers.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new InvalidOperationException("Transfer not found");
        if (t.Status is "dispatched" or "received") throw new InvalidOperationException($"Cannot remove a {t.Status} transfer");
        t.IsDeleted = true;
        await db.SaveChangesAsync(ct);
    }

    /// <summary>Receive: increment destination stock at the dispatched cost (weighted avg).</summary>
    public async Task<StockTransfer> ReceiveTransferAsync(Guid id, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var t = await db.StockTransfers.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new InvalidOperationException("Transfer not found");
        if (t.Status != "dispatched") throw new InvalidOperationException($"Cannot receive a {t.Status} transfer");

        foreach (var line in t.Lines)
            await IncrementAsync(db, line.ProductId, t.ToLocationId, line.Quantity, line.UnitCost, ct);
        t.Status = "received";
        t.ReceivedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return t;
    }

    // ── wastage documents (draft → submit → pending/approved → approve/reject) ──
    // Mirrors the stock-adjustment document workflow below: nothing touches ProductStock
    // until the document is actually approved. Replaces the old always-immediate
    // RecordWastageAsync, which posted straight to stock from the Inventory page's modal
    // (now removed) — existing "posted" rows from that path are left as historical records.
    public async Task<WastageNote> CreateWastageDraftAsync(WastageDraftInput input, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var settings = await SettingsAsync(db, ct);
        var note = new WastageNote
        {
            WastageNumber = await NextNumberAsync(db, "WST", settings.WastagePrefix, ct),
            Status = "draft",
        };
        await PopulateWastageDraftAsync(db, note, input, ct);
        db.WastageNotes.Add(note);
        await db.SaveChangesAsync(ct);
        return note;
    }

    /// <summary>Edit a DRAFT wastage note: replace its lines and recompute the preview total.</summary>
    public async Task<WastageNote> EditWastageDraftAsync(Guid id, WastageDraftInput input, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var note = await db.WastageNotes.Include(w => w.Lines).FirstOrDefaultAsync(w => w.Id == id, ct)
            ?? throw new InvalidOperationException("Wastage note not found");
        if (note.Status != "draft") throw new InvalidOperationException($"Only a draft wastage note can be edited (is {note.Status})");
        db.WastageLines.RemoveRange(note.Lines);
        note.Lines.Clear();
        await PopulateWastageDraftAsync(db, note, input, ct);
        await db.SaveChangesAsync(ct);
        return note;
    }

    /// <summary>
    /// Submit a draft for processing. A configured multi-level <see cref="ApprovalRule"/> (#approvals)
    /// takes priority — if one matches, it blocks on that rule chain instead. Otherwise falls back to
    /// the simple org-setting: straight to "approved" (and posts to stock) if no policy applies,
    /// otherwise to "pending", where it waits for ApproveWastageAsync or RejectWastageAsync.
    /// </summary>
    public async Task<WastageNote> SubmitWastageAsync(Guid id, Guid? requestedBy, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var note = await db.WastageNotes.Include(w => w.Lines).FirstOrDefaultAsync(w => w.Id == id, ct)
            ?? throw new InvalidOperationException("Wastage note not found");
        if (note.Status != "draft") throw new InvalidOperationException($"Only a draft wastage note can be submitted (is {note.Status})");
        note.SubmittedAt = DateTime.UtcNow;

        var summary = JsonSerializer.Serialize(new { note.WastageNumber, total = note.TotalCost });
        var ruleResult = await approvals.SubmitAsync(ApprovalService.DocWastage, note.Id, note.WastageNumber,
            note.TotalCost, summary, note.LocationId, requestedBy, null, ct);
        if (ruleResult.Required)
        {
            note.Status = "pending";
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return note;
        }

        var settings = await SettingsAsync(db, ct);
        var requireApproval = settings.RequireWastageApproval
            || (settings.WastageApprovalThreshold > 0 && note.TotalCost >= settings.WastageApprovalThreshold);

        if (requireApproval)
        {
            note.Status = "pending";
        }
        else
        {
            await ApplyWastageToStockAsync(db, note, ct);
            note.Status = "approved"; note.ApprovedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return note;
    }

    /// <summary>
    /// Approve a wastage note awaiting authorisation — this is what actually posts it to stock. If a
    /// multi-level <see cref="ApprovalRule"/> is driving this note (an open <see cref="ApprovalRequest"/>
    /// exists), this routes through the same decision engine as the Approval-rules inbox — gated to
    /// that request's current-level approvers rather than anyone with supply-chain access.
    /// </summary>
    public async Task<WastageNote> ApproveWastageAsync(Guid id, Guid? approverId, int approverRole, string approverName, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var requestId = await db.ApprovalRequests
            .Where(r => r.DocType == ApprovalService.DocWastage && r.DocId == id && r.Status == "pending")
            .Select(r => (Guid?)r.Id).FirstOrDefaultAsync(ct);
        if (requestId is Guid rid)
        {
            var result = await approvals.DecideInAppAsync(rid, approverId ?? Guid.Empty, approverRole, approverName, "approve", null, ct);
            if (!result.Ok) throw new InvalidOperationException(result.Error ?? "You can't approve this wastage note.");
            return await db.WastageNotes.Include(w => w.Lines).FirstAsync(w => w.Id == id, ct);
        }

        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var note = await db.WastageNotes.Include(w => w.Lines).FirstOrDefaultAsync(w => w.Id == id, ct)
            ?? throw new InvalidOperationException("Wastage note not found");
        if (note.Status != "pending") throw new InvalidOperationException("Wastage note is not awaiting approval");

        await ApplyWastageToStockAsync(db, note, ct);
        note.Status = "approved"; note.ApprovedBy = approverId; note.ApprovedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return note;
    }

    /// <summary>Reject a wastage note awaiting authorisation. It never touched stock, so there's nothing to reverse.</summary>
    public async Task<WastageNote> RejectWastageAsync(Guid id, Guid? rejecterId, int approverRole, string approverName, string? reason, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var note = await db.WastageNotes.FirstOrDefaultAsync(w => w.Id == id, ct)
            ?? throw new InvalidOperationException("Wastage note not found");
        if (note.Status != "pending") throw new InvalidOperationException("Wastage note is not awaiting approval");

        var requestId = await db.ApprovalRequests
            .Where(r => r.DocType == ApprovalService.DocWastage && r.DocId == id && r.Status == "pending")
            .Select(r => (Guid?)r.Id).FirstOrDefaultAsync(ct);
        if (requestId is Guid rid)
        {
            var result = await approvals.DecideInAppAsync(rid, rejecterId ?? Guid.Empty, approverRole, approverName, "reject", reason, ct);
            if (!result.Ok) throw new InvalidOperationException(result.Error ?? "You can't reject this wastage note.");
            return await db.WastageNotes.FirstAsync(w => w.Id == id, ct);
        }

        note.Status = "rejected"; note.RejectedBy = rejecterId; note.RejectedAt = DateTime.UtcNow; note.RejectReason = reason;
        await db.SaveChangesAsync(ct);
        return note;
    }

    /// <summary>Soft-delete a wastage note that never posted to stock. Blocked once approved (or legacy "posted").</summary>
    public async Task RemoveWastageAsync(Guid id, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var note = await db.WastageNotes.FirstOrDefaultAsync(w => w.Id == id, ct)
            ?? throw new InvalidOperationException("Wastage note not found");
        if (note.Status is "approved" or "posted") throw new InvalidOperationException($"Cannot remove a {note.Status} wastage note");
        note.IsDeleted = true;
        await db.SaveChangesAsync(ct);
    }

    public async Task<WastageNote?> GetWastageAsync(Guid id, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        return await db.WastageNotes.Include(w => w.Lines).FirstOrDefaultAsync(w => w.Id == id, ct);
    }

    public async Task<PagedWastageResult> GetWastagePagedAsync(
        int pageNumber, int pageSize, string? search, Guid? locationId, string? status, string? reason,
        DateOnly? dateFrom, DateOnly? dateTo, CancellationToken ct)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        await using var db = await factory.CreateForCurrentAsync(ct);

        var query =
            from w in db.WastageNotes.AsNoTracking()
            join l in db.Locations.AsNoTracking() on w.LocationId equals l.Id
            select new { w, LocationCode = l.Code, LocationName = l.Name };

        if (locationId.HasValue) query = query.Where(x => x.w.LocationId == locationId.Value);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.w.Status == status);
        if (!string.IsNullOrWhiteSpace(reason)) query = query.Where(x => x.w.Reason == reason);
        if (dateFrom.HasValue) query = query.Where(x => DateOnly.FromDateTime(x.w.CreatedAt) >= dateFrom.Value);
        if (dateTo.HasValue) query = query.Where(x => DateOnly.FromDateTime(x.w.CreatedAt) <= dateTo.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s2 = $"%{search.Trim()}%";
            query = query.Where(x => EF.Functions.ILike(x.w.WastageNumber, s2) || EF.Functions.ILike(x.w.Notes ?? "", s2));
        }

        var totalCount = await query.CountAsync(ct);

        var data = await query
            .OrderByDescending(x => x.w.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new WastageListDto(
                x.w.Id, x.w.WastageNumber, x.w.LocationId, x.LocationCode, x.LocationName,
                x.w.Reason, x.w.Status, x.w.TotalCost, x.w.Notes,
                x.w.SubmittedAt, x.w.ApprovedAt, x.w.RejectedAt, x.w.RejectReason, x.w.CreatedAt))
            .ToListAsync(ct);

        return new PagedWastageResult(
            data,
            new PaginationMeta(totalCount, pageNumber, pageSize, (int)Math.Ceiling(totalCount / (double)pageSize)));
    }

    /// <summary>Builds/rebuilds a draft wastage note's header + lines from input (shared by create + edit). No stock impact yet.</summary>
    private async Task PopulateWastageDraftAsync(TenantDbContext db, WastageNote note, WastageDraftInput input, CancellationToken ct)
    {
        if (!await db.Locations.AnyAsync(l => l.Id == input.LocationId, ct))
            throw new InvalidOperationException("Location not found");
        if (input.Lines is null || input.Lines.Count == 0)
            throw new InvalidOperationException("A wastage note needs at least one line");

        note.LocationId = input.LocationId;
        note.Reason = input.Reason ?? "spoilage";
        note.Notes = input.Notes;

        decimal total = 0;
        foreach (var l in input.Lines)
        {
            var p = await db.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == l.ProductId, ct)
                ?? throw new InvalidOperationException($"Product {l.ProductId} not found");
            if (l.Quantity <= 0) throw new InvalidOperationException("Wastage quantity must be > 0");

            var stock = await db.ProductStocks.AsNoTracking().FirstOrDefaultAsync(
                s => s.ProductId == p.Id && s.LocationId == input.LocationId, ct);
            var current = stock?.QuantityOnHand ?? 0;
            var cost = stock?.AverageCost ?? p.CostPrice;
            var lineTotal = l.Quantity * cost;
            total += lineTotal;
            note.Lines.Add(new WastageLine
            {
                ProductId = p.Id, Sku = p.Sku, ProductName = p.Name, Quantity = l.Quantity,
                CurrentStock = current, NewStock = current - l.Quantity, UnitCost = cost, LineTotal = lineTotal,
            });
        }
        note.TotalCost = total;
    }

    /// <summary>
    /// Applies a wastage note's lines to stock against LIVE on-hand quantities (not the draft-time
    /// snapshot, which may be stale by the time this posts). Always a straight issue — average
    /// cost is left untouched. Each line's CurrentStock/NewStock/LineTotal are overwritten with
    /// what was actually applied, for an accurate audit trail.
    /// </summary>
    internal static async Task ApplyWastageToStockAsync(TenantDbContext db, WastageNote note, CancellationToken ct)
    {
        decimal total = 0;
        foreach (var line in note.Lines.Where(l => !l.IsDeleted))
        {
            var stock = await db.ProductStocks.FirstOrDefaultAsync(
                s => s.ProductId == line.ProductId && s.LocationId == note.LocationId, ct);
            var liveCurrent = stock?.QuantityOnHand ?? 0;
            var cost = stock?.AverageCost ?? line.UnitCost;
            if (stock is not null) stock.QuantityOnHand -= line.Quantity;   // issue — avg unchanged

            line.CurrentStock = liveCurrent;
            line.NewStock = liveCurrent - line.Quantity;
            line.UnitCost = cost;
            line.LineTotal = line.Quantity * cost;
            total += line.LineTotal;
        }
        note.TotalCost = total;
    }

    // ── stock adjustment documents (draft → submit → pending/approved → approve/reject) ──
    // Mirrors the wastage document workflow above: nothing touches ProductStock until the
    // document is actually approved (either automatically on submit, or via a separate approval
    // step gated by RequireStockAdjustmentApproval / the value threshold). Replaces the old
    // always-immediate AdjustAsync, which posted straight to stock from the Inventory page's
    // Adjust Stock modal (now removed).
    public async Task<StockAdjustment> CreateAdjustmentDraftAsync(AdjustmentDraftInput input, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var settings = await SettingsAsync(db, ct);
        var adj = new StockAdjustment
        {
            AdjustmentNumber = await NextNumberAsync(db, "ADJ", settings.AdjustmentPrefix, ct),
            Status = "draft",
        };
        await PopulateAdjustmentDraftAsync(db, adj, input, ct);
        db.StockAdjustments.Add(adj);
        await db.SaveChangesAsync(ct);
        return adj;
    }

    /// <summary>Edit a DRAFT adjustment: replace its lines and recompute the preview totals.</summary>
    public async Task<StockAdjustment> EditAdjustmentDraftAsync(Guid id, AdjustmentDraftInput input, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var adj = await db.StockAdjustments.Include(a => a.Lines).FirstOrDefaultAsync(a => a.Id == id, ct)
            ?? throw new InvalidOperationException("Stock adjustment not found");
        if (adj.Status != "draft") throw new InvalidOperationException($"Only a draft adjustment can be edited (is {adj.Status})");
        db.StockAdjustmentLines.RemoveRange(adj.Lines);
        adj.Lines.Clear();
        await PopulateAdjustmentDraftAsync(db, adj, input, ct);
        await db.SaveChangesAsync(ct);
        return adj;
    }

    /// <summary>
    /// Submit a draft for processing. A configured multi-level <see cref="ApprovalRule"/> (#approvals)
    /// takes priority — if one matches, it blocks on that rule chain instead. Otherwise falls back to
    /// the simple org-setting: straight to "approved" (and posts to stock) if no policy applies (org
    /// setting, or the adjustment's absolute value crossing the configured threshold) — otherwise to
    /// "pending", where it waits for ApproveAdjustmentAsync or RejectAdjustmentAsync.
    /// </summary>
    public async Task<StockAdjustment> SubmitAdjustmentAsync(Guid id, Guid? requestedBy, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var adj = await db.StockAdjustments.Include(a => a.Lines).FirstOrDefaultAsync(a => a.Id == id, ct)
            ?? throw new InvalidOperationException("Stock adjustment not found");
        if (adj.Status != "draft") throw new InvalidOperationException($"Only a draft adjustment can be submitted (is {adj.Status})");
        adj.SubmittedAt = DateTime.UtcNow;

        var summary = JsonSerializer.Serialize(new { adj.AdjustmentNumber, total = adj.TotalValue });
        var ruleResult = await approvals.SubmitAsync(ApprovalService.DocStockAdjustment, adj.Id, adj.AdjustmentNumber,
            Math.Abs(adj.TotalValue), summary, adj.LocationId, requestedBy, null, ct);
        if (ruleResult.Required)
        {
            adj.Status = "pending";
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return adj;
        }

        var settings = await SettingsAsync(db, ct);
        var requireApproval = settings.RequireStockAdjustmentApproval
            || (settings.StockAdjustmentApprovalThreshold > 0 && Math.Abs(adj.TotalValue) >= settings.StockAdjustmentApprovalThreshold);

        if (requireApproval)
        {
            adj.Status = "pending";
        }
        else
        {
            await ApplyAdjustmentToStockAsync(db, adj, ct);
            adj.Status = "approved"; adj.ApprovedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return adj;
    }

    /// <summary>
    /// Approve an adjustment awaiting authorisation — this is what actually posts it to stock. If a
    /// multi-level <see cref="ApprovalRule"/> is driving this adjustment (an open
    /// <see cref="ApprovalRequest"/> exists), this routes through the same decision engine as the
    /// Approval-rules inbox — gated to that request's current-level approvers rather than anyone
    /// with supply-chain access.
    /// </summary>
    public async Task<StockAdjustment> ApproveAdjustmentAsync(Guid id, Guid? approverId, int approverRole, string approverName, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var requestId = await db.ApprovalRequests
            .Where(r => r.DocType == ApprovalService.DocStockAdjustment && r.DocId == id && r.Status == "pending")
            .Select(r => (Guid?)r.Id).FirstOrDefaultAsync(ct);
        if (requestId is Guid rid)
        {
            var result = await approvals.DecideInAppAsync(rid, approverId ?? Guid.Empty, approverRole, approverName, "approve", null, ct);
            if (!result.Ok) throw new InvalidOperationException(result.Error ?? "You can't approve this adjustment.");
            return await db.StockAdjustments.Include(a => a.Lines).FirstAsync(a => a.Id == id, ct);
        }

        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var adj = await db.StockAdjustments.Include(a => a.Lines).FirstOrDefaultAsync(a => a.Id == id, ct)
            ?? throw new InvalidOperationException("Stock adjustment not found");
        if (adj.Status != "pending") throw new InvalidOperationException("Adjustment is not awaiting approval");

        await ApplyAdjustmentToStockAsync(db, adj, ct);
        adj.Status = "approved"; adj.ApprovedBy = approverId; adj.ApprovedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return adj;
    }

    /// <summary>Reject an adjustment awaiting authorisation. It never touched stock, so there's nothing to reverse.</summary>
    public async Task<StockAdjustment> RejectAdjustmentAsync(Guid id, Guid? rejecterId, int approverRole, string approverName, string? reason, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var adj = await db.StockAdjustments.FirstOrDefaultAsync(a => a.Id == id, ct)
            ?? throw new InvalidOperationException("Stock adjustment not found");
        if (adj.Status != "pending") throw new InvalidOperationException("Adjustment is not awaiting approval");

        var requestId = await db.ApprovalRequests
            .Where(r => r.DocType == ApprovalService.DocStockAdjustment && r.DocId == id && r.Status == "pending")
            .Select(r => (Guid?)r.Id).FirstOrDefaultAsync(ct);
        if (requestId is Guid rid)
        {
            var result = await approvals.DecideInAppAsync(rid, rejecterId ?? Guid.Empty, approverRole, approverName, "reject", reason, ct);
            if (!result.Ok) throw new InvalidOperationException(result.Error ?? "You can't reject this adjustment.");
            return await db.StockAdjustments.FirstAsync(a => a.Id == id, ct);
        }

        adj.Status = "rejected"; adj.RejectedBy = rejecterId; adj.RejectedAt = DateTime.UtcNow; adj.RejectReason = reason;
        await db.SaveChangesAsync(ct);
        return adj;
    }

    /// <summary>Soft-delete an adjustment that never posted to stock. Blocked once approved (or legacy "posted") — mirrors RemovePoAsync.</summary>
    public async Task RemoveAdjustmentAsync(Guid id, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var adj = await db.StockAdjustments.FirstOrDefaultAsync(a => a.Id == id, ct)
            ?? throw new InvalidOperationException("Stock adjustment not found");
        if (adj.Status is "approved" or "posted") throw new InvalidOperationException($"Cannot remove a {adj.Status} adjustment");
        adj.IsDeleted = true;
        await db.SaveChangesAsync(ct);
    }

    public async Task<StockAdjustment?> GetAdjustmentAsync(Guid id, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        return await db.StockAdjustments.Include(a => a.Lines).FirstOrDefaultAsync(a => a.Id == id, ct);
    }

    public async Task<PagedAdjustmentResult> GetAdjustmentsPagedAsync(
        int pageNumber, int pageSize, string? search, Guid? locationId, string? status, string? reason,
        DateOnly? dateFrom, DateOnly? dateTo, CancellationToken ct)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        await using var db = await factory.CreateForCurrentAsync(ct);

        var query =
            from a in db.StockAdjustments.AsNoTracking()
            join l in db.Locations.AsNoTracking() on a.LocationId equals l.Id
            select new { a, LocationCode = l.Code, LocationName = l.Name };

        if (locationId.HasValue) query = query.Where(x => x.a.LocationId == locationId.Value);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.a.Status == status);
        if (!string.IsNullOrWhiteSpace(reason)) query = query.Where(x => x.a.Reason == reason);
        if (dateFrom.HasValue) query = query.Where(x => DateOnly.FromDateTime(x.a.CreatedAt) >= dateFrom.Value);
        if (dateTo.HasValue) query = query.Where(x => DateOnly.FromDateTime(x.a.CreatedAt) <= dateTo.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s2 = $"%{search.Trim()}%";
            query = query.Where(x => EF.Functions.ILike(x.a.AdjustmentNumber, s2) || EF.Functions.ILike(x.a.Notes ?? "", s2));
        }

        var totalCount = await query.CountAsync(ct);

        var data = await query
            .OrderByDescending(x => x.a.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new AdjustmentListDto(
                x.a.Id, x.a.AdjustmentNumber, x.a.LocationId, x.LocationCode, x.LocationName,
                x.a.Reason, x.a.Status, x.a.TotalValue, x.a.Notes,
                x.a.SubmittedAt, x.a.ApprovedAt, x.a.RejectedAt, x.a.RejectReason, x.a.CreatedAt))
            .ToListAsync(ct);

        return new PagedAdjustmentResult(
            data,
            new PaginationMeta(totalCount, pageNumber, pageSize, (int)Math.Ceiling(totalCount / (double)pageSize)));
    }

    /// <summary>Builds/rebuilds a draft adjustment's header + lines from input (shared by create + edit). No stock impact yet.</summary>
    private async Task PopulateAdjustmentDraftAsync(TenantDbContext db, StockAdjustment adj, AdjustmentDraftInput input, CancellationToken ct)
    {
        if (!await db.Locations.AnyAsync(l => l.Id == input.LocationId, ct))
            throw new InvalidOperationException("Location not found");
        if (input.Lines is null || input.Lines.Count == 0)
            throw new InvalidOperationException("An adjustment needs at least one line");

        adj.LocationId = input.LocationId;
        adj.Reason = input.Reason ?? "count";
        adj.Notes = input.Notes;

        decimal total = 0;
        foreach (var l in input.Lines)
        {
            var p = await db.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == l.ProductId, ct)
                ?? throw new InvalidOperationException($"Product {l.ProductId} not found");
            if (l.Quantity <= 0) throw new InvalidOperationException("Each line needs a quantity greater than zero");
            if (l.Type is not ("add" or "reduce" or "override"))
                throw new InvalidOperationException("Invalid adjustment type");

            var stock = await db.ProductStocks.AsNoTracking().FirstOrDefaultAsync(
                s => s.ProductId == p.Id && s.LocationId == input.LocationId, ct);
            var current = stock?.QuantityOnHand ?? 0;
            var cost = stock?.AverageCost ?? p.CostPrice;

            // "add"/"reduce" fix the delta itself, unaffected by anything else that happens before
            // posting; "override" instead fixes a target (NewStock) and the delta is recomputed
            // against live stock at posting time — see ApplyAdjustmentToStockAsync.
            var (delta, newStock) = l.Type switch
            {
                "add" => (l.Quantity, current + l.Quantity),
                "reduce" => (-l.Quantity, current - l.Quantity),
                _ => (l.Quantity - current, l.Quantity),
            };
            var lineTotal = delta * cost;
            total += lineTotal;
            adj.Lines.Add(new StockAdjustmentLine
            {
                ProductId = p.Id, Sku = p.Sku, ProductName = p.Name, AdjustmentType = l.Type,
                QuantityDelta = delta, CurrentStock = current, NewStock = newStock, UnitCost = cost, LineTotal = lineTotal,
            });
        }
        adj.TotalValue = total;
    }

    /// <summary>
    /// Applies an adjustment's lines to stock against LIVE on-hand quantities (not the draft-time
    /// snapshot, which may be stale by the time this posts). "add"/"reduce" lines keep their fixed
    /// delta; "override" lines recompute delta = target (NewStock) − live current. Each line's
    /// CurrentStock/NewStock/LineTotal are overwritten with what was actually applied, for an
    /// accurate audit trail.
    /// </summary>
    internal static async Task ApplyAdjustmentToStockAsync(TenantDbContext db, StockAdjustment adj, CancellationToken ct)
    {
        decimal total = 0;
        foreach (var line in adj.Lines.Where(l => !l.IsDeleted))
        {
            var stock = await db.ProductStocks.FirstOrDefaultAsync(
                s => s.ProductId == line.ProductId && s.LocationId == adj.LocationId, ct);
            var liveCurrent = stock?.QuantityOnHand ?? 0;
            var cost = stock?.AverageCost ?? line.UnitCost;
            var delta = line.AdjustmentType == "override" ? line.NewStock - liveCurrent : line.QuantityDelta;

            if (delta > 0)
                await IncrementAsync(db, line.ProductId, adj.LocationId, delta, cost, ct);  // add at current avg → avg unchanged
            else if (delta < 0 && stock is not null)
                stock.QuantityOnHand += delta;                                              // delta is negative

            line.QuantityDelta = delta;
            line.CurrentStock = liveCurrent;
            line.NewStock = liveCurrent + delta;
            line.UnitCost = cost;
            line.LineTotal = delta * cost;
            total += line.LineTotal;
        }
        adj.TotalValue = total;
    }

    public async Task<StockTransfer?> GetTransferAsync(Guid id, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        return await db.StockTransfers.Include(t => t.Lines).FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async Task<PagedTransferResult> GetTransfersPagedAsync(
        int pageNumber, int pageSize, string? search, Guid? fromLocationId, Guid? toLocationId,
        string? status, bool? isReturn, DateOnly? dateFrom, DateOnly? dateTo, CancellationToken ct)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        await using var db = await factory.CreateForCurrentAsync(ct);

        var query =
            from t in db.StockTransfers.AsNoTracking()
            join fl in db.Locations.AsNoTracking() on t.FromLocationId equals fl.Id
            join tl in db.Locations.AsNoTracking() on t.ToLocationId equals tl.Id
            select new { t, FromCode = fl.Code, FromName = fl.Name, ToCode = tl.Code, ToName = tl.Name };

        if (fromLocationId.HasValue) query = query.Where(x => x.t.FromLocationId == fromLocationId.Value);
        if (toLocationId.HasValue) query = query.Where(x => x.t.ToLocationId == toLocationId.Value);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.t.Status == status);
        if (isReturn.HasValue) query = query.Where(x => x.t.IsReturn == isReturn.Value);
        if (dateFrom.HasValue) query = query.Where(x => x.t.TransferDate >= dateFrom.Value);
        if (dateTo.HasValue) query = query.Where(x => x.t.TransferDate <= dateTo.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s2 = $"%{search.Trim()}%";
            query = query.Where(x => EF.Functions.ILike(x.t.TransferNumber, s2) || EF.Functions.ILike(x.t.ReferenceNo ?? "", s2) || EF.Functions.ILike(x.t.Notes ?? "", s2));
        }

        var totalCount = await query.CountAsync(ct);

        var data = await query
            .OrderByDescending(x => x.t.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new TransferListDto(
                x.t.Id, x.t.TransferNumber, x.t.FromLocationId, x.FromCode, x.FromName,
                x.t.ToLocationId, x.ToCode, x.ToName, x.t.IsReturn, x.t.TransferDate, x.t.ReferenceNo,
                x.t.Status, x.t.TotalCost, x.t.DispatchedAt, x.t.ReceivedAt, x.t.CreatedAt))
            .ToListAsync(ct);

        return new PagedTransferResult(
            data,
            new PaginationMeta(totalCount, pageNumber, pageSize, (int)Math.Ceiling(totalCount / (double)pageSize)));
    }

    /// <summary>
    /// Opening-stock import (#master-data-import): SET each product's on-hand at a location to
    /// the given quantity (idempotent — re-running with the same file is a no-op), optionally
    /// stamping the opening average cost for valuation. The net change is captured as one posted
    /// stock-adjustment document so the movement is auditable. Products resolve by Product Code.
    /// </summary>
    public async Task<object> OpeningStockImportAsync(Guid locationId, List<OpeningStockRow> rows, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var products = (await db.Products.AsNoTracking().Where(p => p.IsStocked).ToListAsync(ct))
            .Where(p => !string.IsNullOrWhiteSpace(p.Sku))
            .GroupBy(p => p.Sku!.Trim().ToLowerInvariant()).ToDictionary(g => g.Key, g => g.First());

        int applied = 0; var errors = new List<object>(); var rn = 0;
        var lines = new List<StockAdjustmentLine>();
        foreach (var row in rows)
        {
            rn++;
            if (string.IsNullOrWhiteSpace(row.Sku)) { errors.Add(new { row = rn, sku = row.Sku, error = "Product Code is required" }); continue; }
            if (!products.TryGetValue(row.Sku!.Trim().ToLowerInvariant(), out var p))
            { errors.Add(new { row = rn, sku = row.Sku, error = "No stocked product with that Product Code" }); continue; }
            var target = row.Quantity ?? 0;
            if (target < 0) { errors.Add(new { row = rn, sku = row.Sku, error = "Quantity cannot be negative" }); continue; }
            var stock = await db.ProductStocks.FirstOrDefaultAsync(s => s.ProductId == p.Id && s.LocationId == locationId, ct);
            if (stock is null) { stock = new ProductStock { ProductId = p.Id, LocationId = locationId, QuantityOnHand = 0, AverageCost = 0 }; db.ProductStocks.Add(stock); }
            var delta = target - stock.QuantityOnHand;
            var cost = row.Cost ?? (stock.AverageCost > 0 ? stock.AverageCost : p.CostPrice);
            stock.QuantityOnHand = target;
            stock.AverageCost = cost;
            stock.LastReceivedAt = DateTime.UtcNow;
            if (delta != 0)
                lines.Add(new StockAdjustmentLine { ProductId = p.Id, Sku = p.Sku, ProductName = p.Name, QuantityDelta = delta, UnitCost = cost });
            applied++;
        }
        string? adjNumber = null;
        if (lines.Count > 0)
        {
            adjNumber = await NextNumberAsync(db, "ADJ", (await SettingsAsync(db, ct)).AdjustmentPrefix, ct);
            var adj = new StockAdjustment { LocationId = locationId, AdjustmentNumber = adjNumber, Reason = "opening", Status = "posted", Notes = "Opening stock import", PostedAt = DateTime.UtcNow };
            adj.Lines.AddRange(lines);
            db.StockAdjustments.Add(adj);
        }
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return new { applied, adjustment = adjNumber, errors };
    }

    // ── helpers ──────────────────────────────────────────────────────────────
    private static async Task IncrementAsync(TenantDbContext db, Guid productId, Guid locationId, decimal qty, decimal unitCost, CancellationToken ct)
    {
        // Local-first: a second line for the same product in one document must reuse the
        // row a prior line just added, or we'd duplicate (product_id, location_id).
        var stock = db.ProductStocks.Local.FirstOrDefault(s => s.ProductId == productId && s.LocationId == locationId)
            ?? await db.ProductStocks.FirstOrDefaultAsync(s => s.ProductId == productId && s.LocationId == locationId, ct);
        if (stock is null)
        {
            stock = new ProductStock { ProductId = productId, LocationId = locationId, QuantityOnHand = 0, AverageCost = 0 };
            db.ProductStocks.Add(stock);
        }
        var oldQty = stock.QuantityOnHand;
        var newQty = oldQty + qty;
        stock.AverageCost = newQty > 0 ? Math.Round((oldQty * stock.AverageCost + qty * unitCost) / newQty, 4) : unitCost;
        stock.QuantityOnHand = newQty;
        stock.LastReceivedAt = DateTime.UtcNow;
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

public record CreateTransferInput(Guid FromLocationId, Guid ToLocationId, bool IsReturn, string? Notes, List<TransferLineInput> Lines,
    DateOnly? TransferDate = null, string? ReferenceNo = null, List<Guid>? RequestNoteIds = null);
public record PagedTransferResult(List<TransferListDto> Data, PaginationMeta Pagination);
public record TransferListDto(
    Guid Id, string TransferNumber, Guid FromLocationId, string FromLocationCode, string FromLocationName,
    Guid ToLocationId, string ToLocationCode, string ToLocationName, bool IsReturn, DateOnly TransferDate, string? ReferenceNo,
    string Status, decimal TotalCost, DateTime? DispatchedAt, DateTime? ReceivedAt, DateTime CreatedAt);
public record TransferLineInput(Guid ProductId, decimal Quantity);
public record OpeningStockImportInput(Guid LocationId, List<OpeningStockRow> Lines);
public record OpeningStockRow(string? Sku, decimal? Quantity, decimal? Cost);

public record AdjustmentDraftInput(Guid LocationId, string? Reason, string? Notes, List<AdjustmentDraftLineInput> Lines);
public record AdjustmentDraftLineInput(Guid ProductId, string Type, decimal Quantity);   // Type: add|reduce|override

public record WastageDraftInput(Guid LocationId, string? Reason, string? Notes, List<WastageDraftLineInput> Lines);
public record WastageDraftLineInput(Guid ProductId, decimal Quantity);
public record PagedWastageResult(List<WastageListDto> Data, PaginationMeta Pagination);
public record WastageListDto(
    Guid Id, string WastageNumber, Guid LocationId, string LocationCode, string LocationName,
    string Reason, string Status, decimal TotalCost, string? Notes,
    DateTime? SubmittedAt, DateTime? ApprovedAt, DateTime? RejectedAt, string? RejectReason, DateTime CreatedAt);

public record PaginationMeta(int TotalCount, int PageNumber, int PageSize, int TotalPages);
public record PagedAdjustmentResult(List<AdjustmentListDto> Data, PaginationMeta Pagination);
public record AdjustmentListDto(
    Guid Id, string AdjustmentNumber, Guid LocationId, string LocationCode, string LocationName,
    string Reason, string Status, decimal TotalValue, string? Notes,
    DateTime? SubmittedAt, DateTime? ApprovedAt, DateTime? RejectedAt, string? RejectReason, DateTime CreatedAt);
