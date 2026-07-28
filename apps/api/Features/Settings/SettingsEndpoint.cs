using Hms.Api.Domain;
using Hms.Api.Features.Orders;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.Settings;

/// <summary>
/// ERP dashboard configuration: tax/charge rules (CRUD), org settings
/// (VAT reg no, invoice prefix), and the VAT-return summary for govt filing.
/// Nothing about tax is hardcoded — it all lives here.
/// </summary>
public static class SettingsEndpoint
{
    public static IEndpointRouteBuilder MapSettingsEndpoints(this IEndpointRouteBuilder app)
    {
        var org = app.MapGroup("/api/v1/settings").WithTags("Settings").RequireAuthorization("BackOffice");
        var vat = app.MapGroup("/api/v1/reports/vat").WithTags("VAT report").RequireAuthorization("BackOffice");
        var sales = app.MapGroup("/api/v1/reports/sales").WithTags("Sales report").RequireAuthorization("BackOffice");
        var outlets = app.MapGroup("/api/v1/reports/outlets").WithTags("Outlet report").RequireAuthorization("BackOffice");

        // Tax/charge rules CRUD now lives in Features/ChargeTypes + Features/Charges.

        // ---- Org settings ----
        org.MapGet("", async (ITenantDbContextFactory f, CancellationToken ct) =>
        {
            await using var db = await f.CreateForCurrentAsync(ct);
            var s = await db.OrgSettings.AsNoTracking().FirstOrDefaultAsync(ct);
            return Results.Ok(s ?? new OrgSettings { TenantId = db.TenantId });
        }).WithName("Settings.Get");

        org.MapPut("", async (OrgSettingsInput i, ITenantDbContextFactory f, System.Security.Claims.ClaimsPrincipal user, CancellationToken ct) =>
        {
            await using var db = await f.CreateForCurrentAsync(ct);
            var s = await db.OrgSettings.FirstOrDefaultAsync(ct);
            if (s is null) { s = new OrgSettings { TenantId = db.TenantId, CreatedAt = DateTime.UtcNow }; db.OrgSettings.Add(s); }
            // Business identity (legal name, tax registrations, base currency) is OWNER-ONLY — an
            // Admin (or Manager/Accountant) can edit everything else here but not the legal identity.
            if (user.IsInRole("Owner"))
            {
                s.LegalName = i.LegalName; s.VatRegistrationNumber = i.VatRegistrationNumber;
                s.BusinessRegistrationNo = i.BusinessRegistrationNo; s.SvatNumber = i.SvatNumber;
                if (!string.IsNullOrWhiteSpace(i.BaseCurrency)) s.BaseCurrency = i.BaseCurrency.Trim().ToUpperInvariant();
            }
            s.VatEnabled = i.VatEnabled ?? s.VatEnabled;
            if (!string.IsNullOrWhiteSpace(i.TaxLabel)) s.TaxLabel = i.TaxLabel.Trim();
            s.TaxInvoiceFooter = i.TaxInvoiceFooter;
            s.VatFilingFrequency = i.VatFilingFrequency ?? s.VatFilingFrequency;
            s.LogoUrl = i.LogoUrl;
            // Document prefixes (only overwrite when a non-empty value is supplied).
            if (!string.IsNullOrWhiteSpace(i.InvoicePrefix))    s.InvoicePrefix = i.InvoicePrefix.Trim();
            if (!string.IsNullOrWhiteSpace(i.OrderPrefix))      s.OrderPrefix = i.OrderPrefix.Trim();
            if (!string.IsNullOrWhiteSpace(i.PoPrefix))         s.PoPrefix = i.PoPrefix.Trim();
            if (!string.IsNullOrWhiteSpace(i.GrnPrefix))        s.GrnPrefix = i.GrnPrefix.Trim();
            if (!string.IsNullOrWhiteSpace(i.SupplierPrefix))   s.SupplierPrefix = i.SupplierPrefix.Trim();
            if (!string.IsNullOrWhiteSpace(i.TransferPrefix))   s.TransferPrefix = i.TransferPrefix.Trim();
            if (!string.IsNullOrWhiteSpace(i.WastagePrefix))    s.WastagePrefix = i.WastagePrefix.Trim();
            if (!string.IsNullOrWhiteSpace(i.AdjustmentPrefix)) s.AdjustmentPrefix = i.AdjustmentPrefix.Trim();
            if (!string.IsNullOrWhiteSpace(i.RequestNotePrefix)) s.RequestNotePrefix = i.RequestNotePrefix.Trim();
            if (!string.IsNullOrWhiteSpace(i.ProductCodePrefix)) s.ProductCodePrefix = i.ProductCodePrefix.Trim().ToUpperInvariant();
            s.AllowDiscountBelowCost = i.AllowDiscountBelowCost ?? s.AllowDiscountBelowCost;
            // Bill branding
            s.BillHeader = i.BillHeader; s.BillFooter = i.BillFooter;
            s.ShowVatOnBills = i.ShowVatOnBills ?? s.ShowVatOnBills;
            s.RequirePoApproval = i.RequirePoApproval ?? s.RequirePoApproval;
            s.PoApprovalThreshold = i.PoApprovalThreshold ?? s.PoApprovalThreshold;
            s.RequireGrnApproval = i.RequireGrnApproval ?? s.RequireGrnApproval;
            s.GrnApprovalThreshold = i.GrnApprovalThreshold ?? s.GrnApprovalThreshold;
            s.RequireStockAdjustmentApproval = i.RequireStockAdjustmentApproval ?? s.RequireStockAdjustmentApproval;
            s.StockAdjustmentApprovalThreshold = i.StockAdjustmentApprovalThreshold ?? s.StockAdjustmentApprovalThreshold;
            s.RequireWastageApproval = i.RequireWastageApproval ?? s.RequireWastageApproval;
            s.WastageApprovalThreshold = i.WastageApprovalThreshold ?? s.WastageApprovalThreshold;
            s.RequireRequestNoteApproval = i.RequireRequestNoteApproval ?? s.RequireRequestNoteApproval;
            s.RequireTransferApproval = i.RequireTransferApproval ?? s.RequireTransferApproval;
            s.TransferApprovalThreshold = i.TransferApprovalThreshold ?? s.TransferApprovalThreshold;
            s.RequireOpenShiftForBilling = i.RequireOpenShiftForBilling ?? s.RequireOpenShiftForBilling;
            s.LoyaltyEnabled = i.LoyaltyEnabled ?? s.LoyaltyEnabled;
            s.LoyaltyEarnRate = i.LoyaltyEarnRate ?? s.LoyaltyEarnRate;
            s.LoyaltyRedeemValue = i.LoyaltyRedeemValue ?? s.LoyaltyRedeemValue;
            s.LoyaltyExpiryDays = i.LoyaltyExpiryDays ?? s.LoyaltyExpiryDays;
            s.KdsEnabled = i.KdsEnabled ?? s.KdsEnabled;
            s.KotAutoPrint = i.KotAutoPrint ?? s.KotAutoPrint;
            s.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
            return Results.Ok(s);
        }).WithName("Settings.Update");

        // ---- Month-end period lock (#77) ----
        // Close (move the lock forward) — BackOffice. Reopen (move it back / clear)
        // is Owner-only and audited, since it re-opens closed figures.
        org.MapPost("/period/close", async (PeriodInput body, ITenantDbContextFactory f, Hms.Api.Features.Audit.AuditService audit, CancellationToken ct) =>
        {
            if (body.Through is not DateOnly through) return Results.BadRequest(new { error = "A close-through date is required" });
            await using var db = await f.CreateForCurrentAsync(ct);
            var s = await db.OrgSettings.FirstOrDefaultAsync(ct);
            if (s is null) { s = new OrgSettings { TenantId = db.TenantId }; db.OrgSettings.Add(s); }
            if (s.BooksLockedThrough is DateOnly cur && through < cur)
                return Results.BadRequest(new { error = $"Books are already closed through {cur:yyyy-MM-dd}; use reopen to move it back" });
            s.BooksLockedThrough = through; s.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
            await audit.LogAsync("period.close", "period", null, $"Closed books through {through:yyyy-MM-dd}", ct);
            return Results.Ok(new { lockedThrough = through });
        }).WithName("Period.Close");

        app.MapPost("/api/v1/settings/period/reopen", async (PeriodInput body, ITenantDbContextFactory f, Hms.Api.Features.Audit.AuditService audit, CancellationToken ct) =>
        {
            await using var db = await f.CreateForCurrentAsync(ct);
            var s = await db.OrgSettings.FirstOrDefaultAsync(ct);
            if (s is null) return Results.Ok(new { lockedThrough = (DateOnly?)null });
            var prev = s.BooksLockedThrough;
            s.BooksLockedThrough = body.Through; s.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
            await audit.LogAsync("period.reopen", "period", null,
                $"Reopened books — was {(prev is { } p ? p.ToString("yyyy-MM-dd") : "open")}, now {(body.Through is { } t ? t.ToString("yyyy-MM-dd") : "open")}", ct);
            return Results.Ok(new { lockedThrough = s.BooksLockedThrough });
        }).WithName("Period.Reopen").WithTags("Settings").RequireAuthorization("Owners");

        // ---- VAT return summary (govt filing) ----
        vat.MapGet("/summary", async (ITenantDbContextFactory f, CancellationToken ct, DateTime? from, DateTime? to) =>
        {
            await using var db = await f.CreateForCurrentAsync(ct);
            var fromD = from is { } ff ? OrderService.AsUtc(ff) : new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var toD = to is { } tt ? OrderService.AsUtc(tt) : fromD.AddMonths(1);

            // Output tax = sum of VAT-type charges on settled, tax-invoice orders in the period.
            var settled = db.Orders.AsNoTracking().Where(o => o.Status == "settled"
                && o.IsTaxInvoice && o.SettledAt >= fromD && o.SettledAt < toD);

            var netSales = await settled.SumAsync(o => (decimal?)o.SubtotalAmount, ct) ?? 0m;
            var serviceCharge = await settled.SumAsync(o => (decimal?)o.ServiceChargeAmount, ct) ?? 0m;
            var settledIds = settled.Select(o => o.Id);

            // Tax is per-product now (a product may carry several Tax-type charges),
            // snapshotted per line in OrderItemCharge — every row there is a tax by
            // construction (that's the only thing that generates them). Bill-level
            // charges (Service Charge, Levy, ...) still snapshot to OrderCharge.
            var itemCharges = await db.OrderItemCharges.AsNoTracking()
                .Where(oic => db.OrderItems.Any(oi => oi.Id == oic.OrderItemId && settledIds.Contains(oi.OrderId)))
                .GroupBy(oic => new { oic.Code, oic.Description })
                .Select(grp => new {
                    grp.Key.Code, Name = grp.Key.Description, ChargeType = "TAX",
                    RatePercent = grp.Max(x => x.Percentage) ?? 0,
                    BaseAmount = grp.Sum(x => x.BaseAmount), ChargeAmount = grp.Sum(x => x.ChargeAmount),
                })
                .ToListAsync(ct);

            var billCharges = await db.OrderCharges.AsNoTracking()
                .Where(oc => settledIds.Contains(oc.OrderId))
                .GroupBy(oc => new { oc.Code, oc.Name, oc.ChargeType, oc.RatePercent })
                .Select(grp => new {
                    grp.Key.Code, grp.Key.Name, grp.Key.ChargeType, grp.Key.RatePercent,
                    BaseAmount = grp.Sum(x => x.BaseAmount), ChargeAmount = grp.Sum(x => x.ChargeAmount),
                })
                .ToListAsync(ct);

            var charges = itemCharges.Concat(billCharges).ToList();
            var outputVat = itemCharges.Sum(c => c.ChargeAmount);
            var invoiceCount = await settled.CountAsync(ct);

            // Input VAT = reclaimable VAT on goods received (approved/posted GRNs) in the period.
            var grns = db.GoodsReceivedNotes.AsNoTracking()
                .Where(g => g.Status == "approved" && g.PostedAt >= fromD && g.PostedAt < toD);
            var grnVat = await grns.SumAsync(g => (decimal?)g.TaxAmount, ct) ?? 0m;
            var grnExVat = await grns.SumAsync(g => (decimal?)g.TotalCost, ct) ?? 0m;
            var grnCount = await grns.CountAsync(ct);

            // Supplier returns (PRN) reverse reclaimable input VAT in the period.
            var prns = db.PurchaseReturns.AsNoTracking()
                .Where(r => r.Status == "posted" && r.PostedAt >= fromD && r.PostedAt < toD);
            var returnVat = await prns.SumAsync(r => (decimal?)r.TaxAmount, ct) ?? 0m;
            var returnsExVat = await prns.SumAsync(r => (decimal?)r.TotalCost, ct) ?? 0m;

            var inputVat = grnVat - returnVat;
            var purchasesExVat = grnExVat - returnsExVat;

            return Results.Ok(new {
                periodFrom = fromD, periodTo = toD,
                invoiceCount, netSales, serviceCharge, outputVat,
                grnCount, purchasesExVat, inputVat, returnVat, returnsExVat,
                netVatPayable = outputVat - inputVat,   // what you remit (or reclaim if negative)
                charges,
            });
        }).WithName("Reports.VatSummary").WithSummary("VAT return: output VAT (sales) − input VAT (purchases) = net payable.");

        // ---- Sales summary (KPIs, by-day, by-source, top items) ----
        sales.MapGet("/summary", async (OrderService orders, CancellationToken ct,
            DateTime? from, DateTime? to, Guid? locationId) =>
            Results.Ok(await orders.SalesSummaryAsync(from, to, locationId, ct)))
            .WithName("Reports.SalesSummary")
            .WithSummary("Settled-sales summary for a period: KPIs, per-day, per-source, top items.");

        // ---- Consolidated HQ report (per-outlet sales + stock value) ----
        outlets.MapGet("/summary", async (OrderService orders, CancellationToken ct, DateTime? from, DateTime? to) =>
            Results.Ok(await orders.OutletSummaryAsync(from, to, ct)))
            .WithName("Reports.OutletSummary")
            .WithSummary("HQ rollup: each outlet's settled sales + current stock value, with group totals.");

        return app;
    }
}

public record OrgSettingsInput(string? LegalName, string? VatRegistrationNumber, string? BusinessRegistrationNo,
    bool? VatEnabled, string? InvoicePrefix, string? TaxInvoiceFooter, string? VatFilingFrequency,
    string? SvatNumber = null, string? OrderPrefix = null, string? PoPrefix = null, string? GrnPrefix = null,
    string? SupplierPrefix = null, string? TransferPrefix = null, string? WastagePrefix = null,
    string? AdjustmentPrefix = null, string? RequestNotePrefix = null, string? BillHeader = null, string? BillFooter = null, bool? ShowVatOnBills = null,
    bool? RequirePoApproval = null, decimal? PoApprovalThreshold = null,
    bool? RequireGrnApproval = null, decimal? GrnApprovalThreshold = null,
    bool? RequireStockAdjustmentApproval = null, decimal? StockAdjustmentApprovalThreshold = null,
    bool? RequireWastageApproval = null, decimal? WastageApprovalThreshold = null,
    bool? RequireRequestNoteApproval = null,
    bool? RequireTransferApproval = null, decimal? TransferApprovalThreshold = null,
    bool? RequireOpenShiftForBilling = null,
    bool? LoyaltyEnabled = null, decimal? LoyaltyEarnRate = null, decimal? LoyaltyRedeemValue = null,
    int? LoyaltyExpiryDays = null, bool? KdsEnabled = null, bool? KotAutoPrint = null,
    string? LogoUrl = null, string? BaseCurrency = null, string? TaxLabel = null, string? ProductCodePrefix = null,
    bool? AllowDiscountBelowCost = null);
public record PeriodInput(DateOnly? Through);
