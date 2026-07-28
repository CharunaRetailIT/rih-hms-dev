using System.Text.Json;
using Hms.Api.Domain;
using Hms.Api.Features.Approvals;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.Procurement;

/// <summary>
/// Supply-chain step 5: suppliers, purchase orders, goods-received notes.
/// Posting a GRN increments stock and recomputes the weighted-average cost —
/// the inbound counterpart to the POS stock decrement (R1).
/// </summary>
public class ProcurementService(ITenantDbContextFactory factory, ApprovalService approvals)
{
    // ── suppliers ──────────────────────────────────────────────────────────
    public async Task<Supplier> CreateSupplierAsync(SupplierInput i, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        // Auto-generate a code from the configured supplier prefix when none is given.
        var code = string.IsNullOrWhiteSpace(i.Code)
            ? await NextNumberAsync(db, "SUP", (await LoadSettingsAsync(db, ct)).SupplierPrefix, ct)
            : i.Code.Trim();
        if (await db.Suppliers.AnyAsync(s => s.Code == code, ct))
            throw new InvalidOperationException($"Supplier code '{code}' already exists");
        var s = new Supplier
        {
            Code = code, Name = i.Name.Trim(), ContactName = i.ContactName,
            Phone = i.Phone, Email = i.Email, Address = i.Address,
            PaymentTermsDays = i.PaymentTermsDays ?? 0, IsActive = true,
            VatRegistrationNumber = i.VatRegistrationNumber,
            IsVatRegistered = i.IsVatRegistered ?? !string.IsNullOrWhiteSpace(i.VatRegistrationNumber),
        };
        db.Suppliers.Add(s);
        await db.SaveChangesAsync(ct);
        return s;
    }

    public async Task<List<Supplier>> ListSuppliersAsync(CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        return await db.Suppliers.AsNoTracking().Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync(ct);
    }

    // ── purchase orders ────────────────────────────────────────────────────
    public async Task<PurchaseOrder> CreatePoAsync(CreatePoInput input, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var settings = await LoadSettingsAsync(db, ct);
        var po = new PurchaseOrder
        {
            Id = Guid.NewGuid(),   // assigned up front so request notes can link to it in the same save
            PoNumber = await NextNumberAsync(db, "PO", settings.PoPrefix, ct),
            Status = "draft", OrderDate = DateOnly.FromDateTime(DateTime.UtcNow),
        };
        await PopulatePoAsync(db, po, input, ct);
        db.PurchaseOrders.Add(po);

        if (input.RequestNoteIds is { Count: > 0 } ids)
            await LinkRequestNotesAsync(db, po, ids, ct);

        await db.SaveChangesAsync(ct);
        return po;
    }

    /// <summary>
    /// Bundles approved "po"-mode request notes into the PO being created: every note must be
    /// approved, in "po" mode, not already fulfilled, and raised against the same location the
    /// PO is buying for. All get marked "fulfilled" and linked to the new PO.
    /// </summary>
    private static async Task LinkRequestNotesAsync(TenantDbContext db, PurchaseOrder po, List<Guid> requestNoteIds, CancellationToken ct)
    {
        var notes = await db.RequestNotes.Where(r => requestNoteIds.Contains(r.Id)).ToListAsync(ct);
        if (notes.Count != requestNoteIds.Distinct().Count())
            throw new InvalidOperationException("One or more request notes could not be found");
        foreach (var n in notes)
        {
            if (n.Mode != "po") throw new InvalidOperationException($"{n.RequestNumber} is not a PO-mode request note");
            if (n.Status != "approved") throw new InvalidOperationException($"{n.RequestNumber} is not approved (is {n.Status})");
            if (n.FromLocationId != po.LocationId) throw new InvalidOperationException($"{n.RequestNumber} was raised against a different location");
        }
        foreach (var n in notes) { n.Status = "fulfilled"; n.PurchaseOrderId = po.Id; }
    }

    /// <summary>Edit a DRAFT PO: replace its header/lines and recompute totals.</summary>
    public async Task<PurchaseOrder> EditPoAsync(Guid id, CreatePoInput input, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var po = await db.PurchaseOrders.Include(p => p.Lines).FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new InvalidOperationException("Purchase order not found");
        if (po.Status != "draft") throw new InvalidOperationException($"Only a draft PO can be edited (is {po.Status})");
        db.PurchaseOrderLines.RemoveRange(po.Lines);
        await PopulatePoAsync(db, po, input, ct);   // clears + rebuilds lines and totals
        await db.SaveChangesAsync(ct);
        return po;
    }

    /// <summary>
    /// Submit a draft for processing. A configured multi-level <see cref="ApprovalRule"/> (#approvals)
    /// takes priority — if one matches this PO's amount/outlet, the PO blocks on that rule chain
    /// instead. Otherwise falls back to the simple org-setting: straight to "approved" if no policy
    /// applies (or the PO's total crossing the configured threshold) — else "pending", where it waits
    /// for <see cref="ApprovePoAsync"/> or <see cref="RejectPoAsync"/>.
    /// </summary>
    public async Task<PurchaseOrder> SubmitPoAsync(Guid id, Guid? requestedBy, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var po = await db.PurchaseOrders.FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new InvalidOperationException("Purchase order not found");
        if (po.Status != "draft") throw new InvalidOperationException($"Only a draft PO can be submitted (is {po.Status})");
        po.SubmittedAt = DateTime.UtcNow;

        var summary = JsonSerializer.Serialize(new { po.PoNumber, total = po.TotalAmount });
        var ruleResult = await approvals.SubmitAsync(ApprovalService.DocPurchaseOrder, po.Id, po.PoNumber,
            po.TotalAmount, summary, po.LocationId, requestedBy, null, ct);
        if (ruleResult.Required)
        {
            po.Status = "pending";
            await db.SaveChangesAsync(ct);
            return po;
        }

        var settings = await LoadSettingsAsync(db, ct);
        var requireApproval = settings.RequirePoApproval
            || (settings.PoApprovalThreshold > 0 && po.TotalAmount >= settings.PoApprovalThreshold);
        po.Status = requireApproval ? "pending" : "approved";
        await db.SaveChangesAsync(ct);
        return po;
    }

    /// <summary>
    /// Approve a pending PO. If a multi-level <see cref="ApprovalRule"/> is actually driving this PO
    /// (an open <see cref="ApprovalRequest"/> exists), this routes through the same decision engine as
    /// the Approval-rules inbox — so it's gated to that request's current-level approvers (by role or
    /// by named user, whichever the rule specifies) rather than anyone with supply-chain access. Only
    /// falls back to a plain status flip for the simple org-setting threshold path (no rule matched).
    /// </summary>
    public async Task<PurchaseOrder> ApprovePoAsync(Guid id, Guid? approverId, int approverRole, string approverName, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var po = await db.PurchaseOrders.FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new InvalidOperationException("Purchase order not found");
        if (po.Status != "pending") throw new InvalidOperationException("PO is not awaiting approval");

        var requestId = await db.ApprovalRequests
            .Where(r => r.DocType == ApprovalService.DocPurchaseOrder && r.DocId == id && r.Status == "pending")
            .Select(r => (Guid?)r.Id).FirstOrDefaultAsync(ct);
        if (requestId is Guid rid)
        {
            var result = await approvals.DecideInAppAsync(rid, approverId ?? Guid.Empty, approverRole, approverName, "approve", null, ct);
            if (!result.Ok) throw new InvalidOperationException(result.Error ?? "You can't approve this purchase order.");
            return await db.PurchaseOrders.FirstAsync(p => p.Id == id, ct);
        }

        po.Status = "approved"; po.ApprovedBy = approverId; po.ApprovedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return po;
    }

    public async Task<PurchaseOrder> RejectPoAsync(Guid id, Guid? rejecterId, int approverRole, string approverName, string? reason, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var po = await db.PurchaseOrders.FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new InvalidOperationException("Purchase order not found");
        if (po.Status != "pending") throw new InvalidOperationException("PO is not awaiting approval");

        var requestId = await db.ApprovalRequests
            .Where(r => r.DocType == ApprovalService.DocPurchaseOrder && r.DocId == id && r.Status == "pending")
            .Select(r => (Guid?)r.Id).FirstOrDefaultAsync(ct);
        if (requestId is Guid rid)
        {
            var result = await approvals.DecideInAppAsync(rid, rejecterId ?? Guid.Empty, approverRole, approverName, "reject", reason, ct);
            if (!result.Ok) throw new InvalidOperationException(result.Error ?? "You can't reject this purchase order.");
            return await db.PurchaseOrders.FirstAsync(p => p.Id == id, ct);
        }

        po.Status = "rejected"; po.RejectedBy = rejecterId; po.RejectedAt = DateTime.UtcNow; po.RejectReason = reason;
        await db.SaveChangesAsync(ct);
        return po;
    }

    /// <summary>Soft-delete a PO that never made it to receiving. Blocked once anything has been received.</summary>
    public async Task RemovePoAsync(Guid id, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var po = await db.PurchaseOrders.Include(p => p.Lines).FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new InvalidOperationException("Purchase order not found");
        if (po.Status is "partially_received" or "received") throw new InvalidOperationException($"Cannot remove a {po.Status} PO");
        if (po.Lines.Any(l => l.QuantityReceived > 0)) throw new InvalidOperationException("Cannot remove a PO that already has receipts");
        po.IsDeleted = true;
        await db.SaveChangesAsync(ct);
    }

    /// <summary>Builds a PO's header + lines + totals + approval state from input (shared by create + edit).</summary>
    private async Task PopulatePoAsync(TenantDbContext db, PurchaseOrder po, CreatePoInput input, CancellationToken ct)
    {
        var supplier = await db.Suppliers.AsNoTracking().FirstOrDefaultAsync(s => s.Id == input.SupplierId, ct)
            ?? throw new InvalidOperationException("Supplier not found");
        if (input.Lines is null || input.Lines.Count == 0)
            throw new InvalidOperationException("A purchase order needs at least one line");
        var (locationRate, locationFlat) = input.ExcludeTax
            ? (0m, 0m)
            : await LoadLocationTaxRateAsync(db, input.LocationId, ct);
        var units = await db.UnitsOfMeasure.AsNoTracking().ToDictionaryAsync(u => u.Id, ct);

        po.LocationId = input.LocationId; po.SupplierId = input.SupplierId;
        po.ExpectedDate = input.ExpectedDate; po.Notes = input.Notes;
        po.DiscountAmount = input.DiscountAmount < 0 ? 0m : input.DiscountAmount;
        po.Deductions = input.Deductions < 0 ? 0m : input.Deductions;
        po.OtherCharges = input.OtherCharges < 0 ? 0m : input.OtherCharges;
        po.ExcludeTax = input.ExcludeTax;
        po.CurrencyCode = string.IsNullOrWhiteSpace(input.CurrencyCode) ? "LKR" : input.CurrencyCode!.Trim().ToUpperInvariant();
        po.CurrencyRate = input.CurrencyRate is > 0 ? input.CurrencyRate!.Value : 1m;
        po.DeliveryLocationId = input.DeliveryLocationId is Guid dl && dl != input.LocationId ? dl : null;
        po.DeliveryAddress = input.DeliveryAddress; po.ReferenceNo = input.ReferenceNo;
        po.PaymentTermsDays = input.PaymentTermsDays ?? supplier.PaymentTermsDays;
        po.PaymentMethod = input.PaymentMethod;

        po.Lines.Clear();
        var anyLineTaxed = false;
        foreach (var l in input.Lines)
        {
            var p = await db.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == l.ProductId, ct)
                ?? throw new InvalidOperationException($"Product {l.ProductId} not found");
            if (l.Quantity <= 0) throw new InvalidOperationException("Line quantity must be > 0");
            var discount = l.DiscountAmount < 0 ? 0m : l.DiscountAmount;

            units.TryGetValue(p.UnitOfMeasureId, out var stockUnit);
            var lineUnit = l.UnitId is Guid uid && units.TryGetValue(uid, out var u) ? u : stockUnit;
            if (lineUnit is not null && stockUnit is not null && !Convertible(lineUnit, stockUnit))
                throw new InvalidOperationException(
                    $"Unit '{lineUnit.Symbol ?? lineUnit.Code}' can't be converted to '{stockUnit.Symbol ?? stockUnit.Code}' for {p.Name}");

            // Tax is header-level now (the buying location's tax-type Charges — see
            // LoadLocationTaxRateAsync), not sourced from the product's own charge
            // assignments. If the product's listed cost is tax-inclusive, back the
            // rate out of it so the cost price actually shown/stored is ex-tax.
            var unitCost = p.IsTaxInclusive && locationRate > 0
                ? Math.Round(l.UnitCost / (1 + locationRate / 100m), 4)
                : l.UnitCost;
            var goodsValue = Math.Round(l.Quantity * unitCost - discount, 4);
            if (goodsValue < 0) throw new InvalidOperationException($"Discount exceeds the line value for {p.Name}");
            var applyTax = !input.ExcludeTax && supplier.IsVatRegistered && p.IsTaxable && p.TaxClass == "standard";
            if (applyTax) anyLineTaxed = true;
            po.Lines.Add(new PurchaseOrderLine
            {
                ProductId = p.Id, Sku = p.Sku, ProductName = p.Name,
                QuantityOrdered = l.Quantity, UnitCost = unitCost, LineTotal = goodsValue,
                TaxRate = applyTax ? locationRate : 0m, TaxAmount = applyTax ? Math.Round(goodsValue * locationRate / 100m, 4) : 0m,
                UnitId = lineUnit?.Id, UnitSymbol = lineUnit?.Symbol ?? lineUnit?.Code, DiscountAmount = discount,
            });
        }
        po.SubtotalAmount = po.Lines.Sum(x => x.LineTotal);
        if (po.SubtotalAmount - po.DiscountAmount - po.Deductions + po.OtherCharges < 0)
            throw new InvalidOperationException("Header discount/deductions exceed the order value");
        // Flat tax-type charges (e.g. a fixed levy) apply once at header level, not per line.
        po.TaxAmount = po.Lines.Sum(x => x.TaxAmount) + (anyLineTaxed ? locationFlat : 0m);
        po.TotalAmount = po.SubtotalAmount - po.DiscountAmount - po.Deductions + po.OtherCharges + po.TaxAmount;
    }

    /// <summary>
    /// Combined percentage rate + flat amount for every active Charge under a
    /// tax-type Charge Type (Code OR Name contains "vat" or "tax" — Charge Type
    /// codes are often short arbitrary strings like "C0001", so the human-readable
    /// Name has to be checked too; OrderService's VAT-exemption filter only checks
    /// Code, which is why it silently misses codes like that — checking Name here
    /// avoids the same gap). Unlike POS bills, this ignores AppliesPerProduct /
    /// ProductCharge assignment entirely — PO/GRN tax is a flat, location-level
    /// add-on to the subtotal, not sourced from which products carry which charge.
    /// A VAT-exempt location (e.g. duty-free) drops out entirely — mirrors the
    /// same per-location VAT exemption POS bills already respect.
    /// </summary>
    private static async Task<(decimal Rate, decimal Flat)> LoadLocationTaxRateAsync(
        TenantDbContext db, Guid locationId, CancellationToken ct)
    {
        var vatExempt = await db.Locations.AsNoTracking()
            .Where(l => l.Id == locationId).Select(l => l.VatExempt).FirstOrDefaultAsync(ct);
        if (vatExempt) return (0m, 0m);

        var rows = await db.Charges.AsNoTracking()
            .Include(c => c.ChargeType)
            .Where(c => c.IsActive
                     && (EF.Functions.ILike(c.ChargeType!.Code, "%vat%") || EF.Functions.ILike(c.ChargeType.Code, "%tax%")
                      || EF.Functions.ILike(c.ChargeType.Name, "%vat%") || EF.Functions.ILike(c.ChargeType.Name, "%tax%")))
            .Select(c => new { c.Percentage, c.Amount })
            .ToListAsync(ct);
        var rate = rows.Sum(r => r.Percentage ?? 0);
        var flat = rows.Where(r => r.Percentage is null).Sum(r => r.Amount ?? 0);
        return (rate, flat);
    }

    /// <summary>Public wrapper so the PO/GRN forms can preview a location's combined tax
    /// rate before submitting (drives the tax-inclusive cost-price back-out on the client).</summary>
    public async Task<(decimal Rate, decimal Flat)> GetLocationTaxRateAsync(Guid locationId, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        return await LoadLocationTaxRateAsync(db, locationId, ct);
    }

    public async Task<PurchaseOrder?> GetPoAsync(Guid id, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        return await db.PurchaseOrders.Include(p => p.Lines).FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<List<PurchaseOrder>> ListPosAsync(CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        return await db.PurchaseOrders.AsNoTracking().OrderByDescending(p => p.CreatedAt).Take(100).ToListAsync(ct);
    }

    public async Task<PagedPoResult> GetPoPagedAsync(
        int pageNumber, int pageSize, string? search,
        Guid? supplierId, Guid? locationId, string? status, CancellationToken ct)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        await using var db = await factory.CreateForCurrentAsync(ct);

        var query =
            from p in db.PurchaseOrders.AsNoTracking()
            join s in db.Suppliers.AsNoTracking() on p.SupplierId equals s.Id
            join l in db.Locations.AsNoTracking() on p.LocationId equals l.Id
            select new { p, SupplierName = s.Name, LocationCode = l.Code, LocationName = l.Name };

        if (supplierId.HasValue) query = query.Where(x => x.p.SupplierId == supplierId.Value);
        if (locationId.HasValue) query = query.Where(x => x.p.LocationId == locationId.Value);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.p.Status == status);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s2 = $"%{search.Trim()}%";
            query = query.Where(x => EF.Functions.ILike(x.p.PoNumber, s2) || EF.Functions.ILike(x.SupplierName, s2));
        }

        var totalCount = await query.CountAsync(ct);

        var data = await query
            .OrderByDescending(x => x.p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .Select(x => new PoListDto(
                x.p.Id, x.p.PoNumber, x.p.SupplierId, x.SupplierName,
                x.p.LocationId, x.LocationCode, x.LocationName,
                x.p.Status, x.p.OrderDate, x.p.ExpectedDate, x.p.TotalAmount))
            .ToListAsync(ct);

        return new PagedPoResult(
            data,
            new PaginationMeta(totalCount, pageNumber, pageSize, (int)Math.Ceiling(totalCount / (double)pageSize)));
    }

    // ── goods received ─────────────────────────────────────────────────────
    /// <summary>
    /// Build a GRN from the received lines as a draft — no stock impact yet.
    /// <see cref="SubmitGrnAsync"/> is what starts it toward posting. A GRN can
    /// stand alone (no PO) for ad-hoc receipts.
    /// </summary>
    public async Task<GoodsReceivedNote> ReceiveAsync(ReceiveInput input, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);

        var supplier = await db.Suppliers.AsNoTracking().FirstOrDefaultAsync(s => s.Id == input.SupplierId, ct)
            ?? throw new InvalidOperationException("Supplier not found");
        if (input.Lines is null || input.Lines.Count == 0)
            throw new InvalidOperationException("A GRN needs at least one line");

        var (locationRate, locationFlat) = await LoadLocationTaxRateAsync(db, input.LocationId, ct);
        var settings = await LoadSettingsAsync(db, ct);

        var units = await db.UnitsOfMeasure.AsNoTracking().ToDictionaryAsync(u => u.Id, ct);
        var grn = new GoodsReceivedNote
        {
            LocationId = input.LocationId, SupplierId = input.SupplierId,
            PurchaseOrderId = input.PurchaseOrderId,
            GrnNumber = await NextNumberAsync(db, "GRN", settings.GrnPrefix, ct),
            Status = "draft", ReceivedDate = input.ReceivedDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            SupplierInvoiceNo = input.SupplierInvoiceNo, Notes = input.Notes,
            CurrencyCode = string.IsNullOrWhiteSpace(input.CurrencyCode) ? "LKR" : input.CurrencyCode.ToUpperInvariant(),
            CurrencyRate = input.CurrencyRate is > 0 ? input.CurrencyRate.Value : 1,
            PaymentTermsDays = input.PaymentTermsDays, PaymentMethod = input.PaymentMethod,
            DiscountAmount = input.DiscountAmount < 0 ? 0 : input.DiscountAmount,
            OtherCharges = input.OtherCharges < 0 ? 0 : input.OtherCharges,
            ReferenceNo = input.ReferenceNo,
        };

        var anyLineTaxed = false;
        foreach (var l in input.Lines)
        {
            var p = await db.Products.FirstOrDefaultAsync(x => x.Id == l.ProductId, ct)
                ?? throw new InvalidOperationException($"Product {l.ProductId} not found");
            if (l.Quantity <= 0) throw new InvalidOperationException("Received quantity must be > 0");
            var free = l.FreeQuantity < 0 ? 0m : l.FreeQuantity;
            var discount = l.DiscountAmount < 0 ? 0m : l.DiscountAmount;

            // Convert the received (purchasing) unit to the product's stock unit.
            units.TryGetValue(p.UnitOfMeasureId, out var stockUnit);
            var recvUnit = l.UnitId is Guid uid && units.TryGetValue(uid, out var ru) ? ru : stockUnit;
            if (recvUnit is not null && stockUnit is not null && !Convertible(recvUnit, stockUnit))
                throw new InvalidOperationException(
                    $"Unit '{recvUnit.Symbol ?? recvUnit.Code}' can't be converted to '{stockUnit.Symbol ?? stockUnit.Code}' for {p.Name}");
            var factor = recvUnit is not null && stockUnit is not null && stockUnit.FactorToBase != 0
                ? recvUnit.FactorToBase / stockUnit.FactorToBase : 1m;

            // Tax is header-level now (the receiving location's tax-type Charges — see
            // LoadLocationTaxRateAsync), not sourced from the product's own charge
            // assignments. If the product's listed cost is tax-inclusive, back the
            // rate out of it so the cost price actually shown/stored, and what stock
            // costing uses, is ex-tax.
            var unitCost = p.IsTaxInclusive && locationRate > 0
                ? Math.Round(l.UnitCost / (1 + locationRate / 100m), 4)
                : l.UnitCost;
            var goodsValue = Math.Round(l.Quantity * unitCost - discount, 4);
            if (goodsValue < 0) throw new InvalidOperationException($"Discount exceeds the line value for {p.Name}");
            // Input VAT (reclaimable), not added to stock cost.
            var applyTax = supplier.IsVatRegistered && p.IsTaxable && p.TaxClass == "standard";
            if (applyTax) anyLineTaxed = true;
            var stockQty = (l.Quantity + free) * factor;  // total units added to stock (incl free)

            grn.Lines.Add(new GrnLine
            {
                ProductId = p.Id, Sku = p.Sku, ProductName = p.Name,
                QuantityReceived = l.Quantity, UnitCost = unitCost, LineTotal = goodsValue,
                TaxRate = applyTax ? locationRate : 0m, TaxAmount = applyTax ? Math.Round(goodsValue * locationRate / 100m, 4) : 0m,
                BatchNo = l.BatchNo, ExpiryDate = l.ExpiryDate,
                UnitId = recvUnit?.Id, UnitSymbol = recvUnit?.Symbol ?? recvUnit?.Code,
                FreeQuantity = free, DiscountAmount = discount, StockQuantity = stockQty,
            });
        }
        grn.TotalCost = grn.Lines.Sum(x => x.LineTotal);   // ex-VAT goods value
        // Flat tax-type charges (e.g. a fixed levy) apply once at header level, not per line.
        grn.TaxAmount = grn.Lines.Sum(x => x.TaxAmount) + (anyLineTaxed ? locationFlat : 0m);   // input VAT (reclaimable)

        db.GoodsReceivedNotes.Add(grn);
        await db.SaveChangesAsync(ct);
        return grn;
    }

    /// <summary>
    /// Submit a draft GRN for processing. A configured multi-level <see cref="ApprovalRule"/>
    /// (#approvals) takes priority — if one matches this GRN's net amount/outlet, the GRN blocks on
    /// that rule chain instead. Otherwise falls back to the simple org-setting: straight to "approved"
    /// — which posts it (stock + weighted-avg cost + PO advance) — if no policy applies (or the GRN's
    /// net amount crossing the configured threshold); else "pending", where it waits for
    /// <see cref="ApproveGrnAsync"/> or <see cref="RejectGrnAsync"/>.
    /// </summary>
    public async Task<GoodsReceivedNote> SubmitGrnAsync(Guid id, Guid? requestedBy, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var grn = await db.GoodsReceivedNotes.Include(g => g.Lines).FirstOrDefaultAsync(g => g.Id == id, ct)
            ?? throw new InvalidOperationException("GRN not found");
        if (grn.Status != "draft") throw new InvalidOperationException($"Only a draft GRN can be submitted (is {grn.Status})");

        var netAmount = grn.TotalCost - grn.DiscountAmount + grn.OtherCharges + grn.TaxAmount;
        grn.SubmittedAt = DateTime.UtcNow;

        var summary = JsonSerializer.Serialize(new { grn.GrnNumber, total = netAmount });
        var ruleResult = await approvals.SubmitAsync(ApprovalService.DocGrn, grn.Id, grn.GrnNumber,
            netAmount, summary, grn.LocationId, requestedBy, null, ct);
        if (ruleResult.Required)
        {
            grn.Status = "pending";
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return grn;
        }

        var settings = await LoadSettingsAsync(db, ct);
        var requireApproval = settings.RequireGrnApproval
            || (settings.GrnApprovalThreshold > 0 && netAmount >= settings.GrnApprovalThreshold);

        if (requireApproval)
        {
            grn.Status = "pending";
        }
        else
        {
            await ApplyGrnToStockAsync(db, grn, ct);
            grn.Status = "approved"; grn.PostedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return grn;
    }

    /// <summary>
    /// Applies a GRN's already-computed lines to stock (weighted-average cost) and
    /// advances its linked PO's received quantities + status. The whole goods value
    /// is spread over the total units added (paid + free), so free stock correctly
    /// dilutes avg cost. Shared by immediate posting (ReceiveAsync) and deferred
    /// posting on approval (ApproveGrnAsync) — both call this within an open transaction. Internal
    /// (not private) so <see cref="Hms.Api.Features.Approvals.ApprovalService"/> can post the same
    /// way when a multi-level approval rule's final level clears (#approvals).
    /// </summary>
    internal static async Task ApplyGrnToStockAsync(TenantDbContext db, GoodsReceivedNote grn, CancellationToken ct)
    {
        foreach (var gl in grn.Lines.Where(l => !l.IsDeleted))
        {
            var stock = await db.ProductStocks.FirstOrDefaultAsync(
                s => s.ProductId == gl.ProductId && s.LocationId == grn.LocationId, ct);
            if (stock is null)
            {
                stock = new ProductStock
                {
                    ProductId = gl.ProductId, LocationId = grn.LocationId,
                    QuantityOnHand = 0, AverageCost = 0,
                };
                db.ProductStocks.Add(stock);
            }
            var oldQty = stock.QuantityOnHand;
            var newQty = oldQty + gl.StockQuantity;
            stock.AverageCost = newQty > 0
                ? Math.Round((oldQty * stock.AverageCost + gl.LineTotal) / newQty, 4)
                : (gl.StockQuantity > 0 ? Math.Round(gl.LineTotal / gl.StockQuantity, 4) : gl.UnitCost);
            stock.QuantityOnHand = newQty;
            stock.LastReceivedAt = DateTime.UtcNow;
        }

        if (grn.PurchaseOrderId is { } poId)
        {
            var po = await db.PurchaseOrders.Include(p => p.Lines).FirstOrDefaultAsync(p => p.Id == poId, ct);
            if (po is not null)
            {
                foreach (var gl in grn.Lines)
                {
                    var pol = po.Lines.FirstOrDefault(x => x.ProductId == gl.ProductId);
                    if (pol is not null) pol.QuantityReceived += gl.QuantityReceived;
                }
                po.Status = po.Lines.All(x => x.QuantityReceived >= x.QuantityOrdered) ? "received" : "partially_received";
            }
        }
    }

    /// <summary>
    /// Approve a GRN awaiting authorisation — this is what actually posts it (stock + weighted-avg
    /// cost + PO advance). If a multi-level <see cref="ApprovalRule"/> is actually driving this GRN
    /// (an open <see cref="ApprovalRequest"/> exists), this routes through the same decision engine as
    /// the Approval-rules inbox — gated to that request's current-level approvers (by role or named
    /// user) rather than anyone with supply-chain access — and the engine itself posts stock once the
    /// final level clears. Only falls back to posting here directly for the simple org-setting
    /// threshold path (no rule matched).
    /// </summary>
    public async Task<GoodsReceivedNote> ApproveGrnAsync(Guid id, Guid? approverId, int approverRole, string approverName, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var requestId = await db.ApprovalRequests
            .Where(r => r.DocType == ApprovalService.DocGrn && r.DocId == id && r.Status == "pending")
            .Select(r => (Guid?)r.Id).FirstOrDefaultAsync(ct);
        if (requestId is Guid rid)
        {
            var result = await approvals.DecideInAppAsync(rid, approverId ?? Guid.Empty, approverRole, approverName, "approve", null, ct);
            if (!result.Ok) throw new InvalidOperationException(result.Error ?? "You can't approve this GRN.");
            return await db.GoodsReceivedNotes.Include(g => g.Lines).FirstAsync(g => g.Id == id, ct);
        }

        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var grn = await db.GoodsReceivedNotes.Include(g => g.Lines).FirstOrDefaultAsync(g => g.Id == id, ct)
            ?? throw new InvalidOperationException("GRN not found");
        if (grn.Status != "pending") throw new InvalidOperationException("GRN is not awaiting approval");

        await ApplyGrnToStockAsync(db, grn, ct);
        grn.Status = "approved"; grn.PostedAt = DateTime.UtcNow;
        grn.ApprovedBy = approverId; grn.ApprovedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return grn;
    }

    /// <summary>Reject a GRN awaiting authorisation. It never touched stock, so there's nothing to reverse.</summary>
    public async Task<GoodsReceivedNote> RejectGrnAsync(Guid id, Guid? rejecterId, int approverRole, string approverName, string? reason, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var grn = await db.GoodsReceivedNotes.FirstOrDefaultAsync(g => g.Id == id, ct)
            ?? throw new InvalidOperationException("GRN not found");
        if (grn.Status != "pending") throw new InvalidOperationException("GRN is not awaiting approval");

        var requestId = await db.ApprovalRequests
            .Where(r => r.DocType == ApprovalService.DocGrn && r.DocId == id && r.Status == "pending")
            .Select(r => (Guid?)r.Id).FirstOrDefaultAsync(ct);
        if (requestId is Guid rid)
        {
            var result = await approvals.DecideInAppAsync(rid, rejecterId ?? Guid.Empty, approverRole, approverName, "reject", reason, ct);
            if (!result.Ok) throw new InvalidOperationException(result.Error ?? "You can't reject this GRN.");
            return await db.GoodsReceivedNotes.FirstAsync(g => g.Id == id, ct);
        }

        grn.Status = "rejected"; grn.RejectedBy = rejecterId; grn.RejectedAt = DateTime.UtcNow; grn.RejectReason = reason;
        await db.SaveChangesAsync(ct);
        return grn;
    }

    /// <summary>Soft-delete a GRN that never posted to stock. Blocked once approved (use Void instead) or already void.</summary>
    public async Task RemoveGrnAsync(Guid id, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var grn = await db.GoodsReceivedNotes.FirstOrDefaultAsync(g => g.Id == id, ct)
            ?? throw new InvalidOperationException("GRN not found");
        if (grn.Status is "approved" or "void") throw new InvalidOperationException($"Cannot remove a {grn.Status} GRN — void it instead");
        grn.IsDeleted = true;
        await db.SaveChangesAsync(ct);
    }

    public async Task<GoodsReceivedNote?> GetGrnAsync(Guid id, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        return await db.GoodsReceivedNotes.Include(g => g.Lines).FirstOrDefaultAsync(g => g.Id == id, ct);
    }

    public async Task<PagedGrnResult> GetGrnPagedAsync(
        int pageNumber, int pageSize, string? search,
        Guid? supplierId, Guid? locationId, string? status,
        DateOnly? dateFrom, DateOnly? dateTo, CancellationToken ct)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        await using var db = await factory.CreateForCurrentAsync(ct);

        var query =
            from g in db.GoodsReceivedNotes.AsNoTracking()
            join s in db.Suppliers.AsNoTracking() on g.SupplierId equals s.Id
            join l in db.Locations.AsNoTracking() on g.LocationId equals l.Id
            join po in db.PurchaseOrders.AsNoTracking() on g.PurchaseOrderId equals po.Id into poJoin
            from po in poJoin.DefaultIfEmpty()
            select new { g, SupplierName = s.Name, LocationCode = l.Code, LocationName = l.Name, PoNumber = po != null ? po.PoNumber : null };

        if (supplierId.HasValue) query = query.Where(x => x.g.SupplierId == supplierId.Value);
        if (locationId.HasValue) query = query.Where(x => x.g.LocationId == locationId.Value);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.g.Status == status);
        if (dateFrom.HasValue) query = query.Where(x => x.g.ReceivedDate >= dateFrom.Value);
        if (dateTo.HasValue) query = query.Where(x => x.g.ReceivedDate <= dateTo.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s2 = $"%{search.Trim()}%";
            query = query.Where(x =>
                EF.Functions.ILike(x.g.GrnNumber, s2) ||
                EF.Functions.ILike(x.SupplierName, s2) ||
                EF.Functions.ILike(x.g.SupplierInvoiceNo ?? "", s2) ||
                EF.Functions.ILike(x.g.ReferenceNo ?? "", s2));
        }

        var totalCount = await query.CountAsync(ct);

        var data = await query
            .OrderByDescending(x => x.g.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new GrnListDto(
                x.g.Id, x.g.GrnNumber, x.g.SupplierId, x.SupplierName,
                x.g.LocationId, x.LocationCode, x.LocationName,
                x.g.PurchaseOrderId, x.PoNumber,
                x.g.Status, x.g.ReceivedDate,
                x.g.TotalCost, x.g.TaxAmount, x.g.DiscountAmount, x.g.OtherCharges,
                x.g.TotalCost - x.g.DiscountAmount + x.g.OtherCharges + x.g.TaxAmount,
                x.g.SupplierInvoiceNo, x.g.ReferenceNo, x.g.PostedAt,
                x.g.ApprovedAt, x.g.RejectedAt, x.g.RejectReason, new List<string>()))
            .ToListAsync(ct);

        // Trace each GRN's PO back to whichever request notes were bundled into it (if any) —
        // a small follow-up lookup rather than a correlated subquery in the main projection.
        var poIds = data.Where(d => d.PurchaseOrderId.HasValue).Select(d => d.PurchaseOrderId!.Value).Distinct().ToList();
        if (poIds.Count > 0)
        {
            var byPo = (await db.RequestNotes.AsNoTracking()
                    .Where(r => r.PurchaseOrderId.HasValue && poIds.Contains(r.PurchaseOrderId.Value))
                    .Select(r => new { r.PurchaseOrderId, r.RequestNumber })
                    .ToListAsync(ct))
                .GroupBy(r => r.PurchaseOrderId!.Value)
                .ToDictionary(g => g.Key, g => g.Select(x => x.RequestNumber).ToList());
            data = data.Select(d => d.PurchaseOrderId.HasValue && byPo.TryGetValue(d.PurchaseOrderId.Value, out var nums)
                ? d with { FulfilledRequestNoteNumbers = nums }
                : d).ToList();
        }

        return new PagedGrnResult(
            data,
            new PaginationMeta(totalCount, pageNumber, pageSize, (int)Math.Ceiling(totalCount / (double)pageSize)));
    }

    /// <summary>Void a posted GRN: reverse the stock it added and back out the PO receipt.</summary>
    public async Task<GoodsReceivedNote> VoidGrnAsync(Guid id, string? reason, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var grn = await db.GoodsReceivedNotes.Include(g => g.Lines).FirstOrDefaultAsync(g => g.Id == id, ct)
            ?? throw new InvalidOperationException("GRN not found");
        if (grn.Status != "approved") throw new InvalidOperationException($"Only an approved GRN can be voided (is {grn.Status})");
        await PeriodGuard.EnsureOpenAsync(db, grn.PostedAt ?? grn.CreatedAt, ct);   // #77: can't reverse a closed-period receipt

        foreach (var gl in grn.Lines.Where(l => !l.IsDeleted))
        {
            var stock = await db.ProductStocks.FirstOrDefaultAsync(
                s => s.ProductId == gl.ProductId && s.LocationId == grn.LocationId, ct);
            // Reverse the quantity that was added (incl free). Avg cost left as-is.
            if (stock is not null) stock.QuantityOnHand -= gl.StockQuantity != 0 ? gl.StockQuantity : gl.QuantityReceived;
        }

        if (grn.PurchaseOrderId is { } poId)
        {
            var po = await db.PurchaseOrders.Include(p => p.Lines).FirstOrDefaultAsync(p => p.Id == poId, ct);
            if (po is not null)
            {
                foreach (var gl in grn.Lines)
                {
                    var pol = po.Lines.FirstOrDefault(x => x.ProductId == gl.ProductId);
                    if (pol is not null) pol.QuantityReceived = Math.Max(0, pol.QuantityReceived - gl.QuantityReceived);
                }
                po.Status = po.Lines.Any(x => x.QuantityReceived > 0)
                    ? (po.Lines.All(x => x.QuantityReceived >= x.QuantityOrdered) ? "received" : "partially_received")
                    : "draft";
            }
        }

        grn.Status = "void"; grn.VoidedAt = DateTime.UtcNow; grn.VoidReason = reason;
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return grn;
    }

    // ── supplier returns / debit notes (PRN) ───────────────────────────────
    /// <summary>
    /// Return received goods to a supplier: decrements stock at the location
    /// (weighted-avg unchanged) and reverses the reclaimable input VAT. Atomic.
    /// </summary>
    public async Task<PurchaseReturn> ReturnToSupplierAsync(PurchaseReturnInput input, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        await using var tx = await db.Database.BeginTransactionAsync(ct);

        var supplier = await db.Suppliers.AsNoTracking().FirstOrDefaultAsync(s => s.Id == input.SupplierId, ct)
            ?? throw new InvalidOperationException("Supplier not found");
        if (input.Lines is null || input.Lines.Count == 0)
            throw new InvalidOperationException("A return needs at least one line");

        var (locationRate, locationFlat) = await LoadLocationTaxRateAsync(db, input.LocationId, ct);
        var units = await db.UnitsOfMeasure.AsNoTracking().ToDictionaryAsync(u => u.Id, ct);
        var prn = new PurchaseReturn
        {
            LocationId = input.LocationId, SupplierId = input.SupplierId, GrnId = input.GrnId,
            PrnNumber = await NextNumberAsync(db, "PRN", "PRN", ct),
            Status = "posted", ReturnDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Reason = input.Reason, SupplierCreditNo = input.SupplierCreditNo, Notes = input.Notes, PostedAt = DateTime.UtcNow,
        };

        var anyLineTaxed = false;
        foreach (var l in input.Lines)
        {
            var p = await db.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == l.ProductId, ct)
                ?? throw new InvalidOperationException($"Product {l.ProductId} not found");
            if (l.Quantity <= 0) throw new InvalidOperationException("Return quantity must be > 0");

            units.TryGetValue(p.UnitOfMeasureId, out var stockUnit);
            var retUnit = l.UnitId is Guid uid && units.TryGetValue(uid, out var ru) ? ru : stockUnit;
            if (retUnit is not null && stockUnit is not null && !Convertible(retUnit, stockUnit))
                throw new InvalidOperationException(
                    $"Unit '{retUnit.Symbol ?? retUnit.Code}' can't be converted to '{stockUnit.Symbol ?? stockUnit.Code}' for {p.Name}");
            var factor = retUnit is not null && stockUnit is not null && stockUnit.FactorToBase != 0
                ? retUnit.FactorToBase / stockUnit.FactorToBase : 1m;
            var stockQty = l.Quantity * factor;

            var stock = await db.ProductStocks.FirstOrDefaultAsync(
                s => s.ProductId == p.Id && s.LocationId == input.LocationId, ct);
            if (stock is null || stock.QuantityOnHand < stockQty)
                throw new InvalidOperationException($"Insufficient {p.Name} on hand to return (need {stockQty})");
            var unitCost = l.UnitCost > 0 ? l.UnitCost : stock.AverageCost;   // credit value (defaults to avg cost)
            stock.QuantityOnHand -= stockQty;                                 // issue back — avg unchanged

            var goodsValue = Math.Round(l.Quantity * unitCost, 4);
            var applyTax = supplier.IsVatRegistered && p.IsTaxable && p.TaxClass == "standard";
            if (applyTax) anyLineTaxed = true;
            prn.Lines.Add(new PurchaseReturnLine
            {
                ProductId = p.Id, Sku = p.Sku, ProductName = p.Name,
                Quantity = l.Quantity, UnitId = retUnit?.Id, UnitSymbol = retUnit?.Symbol ?? retUnit?.Code,
                UnitCost = unitCost, LineTotal = goodsValue, TaxRate = applyTax ? locationRate : 0m,
                TaxAmount = applyTax ? Math.Round(goodsValue * locationRate / 100m, 4) : 0m, StockQuantity = stockQty, BatchNo = l.BatchNo,
            });
        }
        prn.TotalCost = prn.Lines.Sum(x => x.LineTotal);
        prn.TaxAmount = prn.Lines.Sum(x => x.TaxAmount) + (anyLineTaxed ? locationFlat : 0m);
        db.PurchaseReturns.Add(prn);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return prn;
    }

    public async Task<PurchaseReturn> VoidPurchaseReturnAsync(Guid id, string? reason, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var prn = await db.PurchaseReturns.Include(r => r.Lines).FirstOrDefaultAsync(r => r.Id == id, ct)
            ?? throw new InvalidOperationException("Purchase return not found");
        if (prn.Status == "void") throw new InvalidOperationException("Return is already void");
        await PeriodGuard.EnsureOpenAsync(db, prn.PostedAt ?? prn.CreatedAt, ct);   // #77
        foreach (var l in prn.Lines.Where(x => !x.IsDeleted))
        {
            var stock = await db.ProductStocks.FirstOrDefaultAsync(
                s => s.ProductId == l.ProductId && s.LocationId == prn.LocationId, ct);
            if (stock is not null) stock.QuantityOnHand += l.StockQuantity;   // put the goods back
        }
        prn.Status = "void"; prn.VoidedAt = DateTime.UtcNow; prn.VoidReason = reason;
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return prn;
    }

    public async Task<List<PurchaseReturn>> ListReturnsAsync(CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        return await db.PurchaseReturns.AsNoTracking().OrderByDescending(r => r.CreatedAt).Take(100).ToListAsync(ct);
    }

    public async Task<PurchaseReturn?> GetReturnAsync(Guid id, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        return await db.PurchaseReturns.Include(r => r.Lines).FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    /// <summary>Convertible if same unit, or a different unit in the same mass/volume dimension.</summary>
    private static bool Convertible(UnitOfMeasure recvUnit, UnitOfMeasure stockUnit)
    {
        if (recvUnit.Id == stockUnit.Id) return true;
        if (recvUnit.Dimension != stockUnit.Dimension) return false;
        return recvUnit.Dimension is "mass" or "volume";
    }

    /// <summary>Org settings (with built-in defaults if the tenant hasn't configured them).</summary>
    internal static async Task<OrgSettings> LoadSettingsAsync(TenantDbContext db, CancellationToken ct)
        => await db.OrgSettings.AsNoTracking().FirstOrDefaultAsync(ct) ?? new OrgSettings();

    /// <summary>
    /// Next document number: counterKey keeps the running sequence stable;
    /// prefix is the admin-configured display prefix (e.g. PO → "PUR").
    /// </summary>
    internal static async Task<string> NextNumberAsync(TenantDbContext db, string counterKey, string prefix, CancellationToken ct)
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

public record SupplierInput(string? Code, string Name, string? ContactName, string? Phone, string? Email, string? Address, int? PaymentTermsDays,
    string? VatRegistrationNumber = null, bool? IsVatRegistered = null);
public record CreatePoInput(Guid LocationId, Guid SupplierId, DateOnly? ExpectedDate, string? Notes, List<PoLineInput> Lines,
    decimal DiscountAmount = 0, decimal OtherCharges = 0, string? CurrencyCode = null, decimal? CurrencyRate = null,
    Guid? DeliveryLocationId = null, string? DeliveryAddress = null, string? ReferenceNo = null,
    int? PaymentTermsDays = null, string? PaymentMethod = null, decimal Deductions = 0, bool ExcludeTax = false,
    List<Guid>? RequestNoteIds = null);
public record PoLineInput(Guid ProductId, decimal Quantity, decimal UnitCost, Guid? UnitId = null, decimal DiscountAmount = 0);
public record ReceiveInput(Guid LocationId, Guid SupplierId, Guid? PurchaseOrderId, string? Notes, List<GrnLineInput> Lines,
    string? SupplierInvoiceNo = null, DateOnly? ReceivedDate = null, string? CurrencyCode = null, decimal? CurrencyRate = null,
    int? PaymentTermsDays = null, string? PaymentMethod = null, decimal DiscountAmount = 0, decimal OtherCharges = 0,
    string? ReferenceNo = null);
public record GrnLineInput(Guid ProductId, decimal Quantity, decimal UnitCost, string? BatchNo, DateOnly? ExpiryDate,
    Guid? UnitId = null, decimal FreeQuantity = 0, decimal DiscountAmount = 0);
public record VoidGrnInput(string? Reason);
public record PurchaseReturnInput(Guid LocationId, Guid SupplierId, Guid? GrnId, string? Reason, List<PrnLineInput> Lines,
    string? SupplierCreditNo = null, string? Notes = null);
public record PrnLineInput(Guid ProductId, decimal Quantity, decimal UnitCost = 0, Guid? UnitId = null, string? BatchNo = null);
