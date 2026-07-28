using FluentAssertions;
using Hms.Api.Features.Procurement;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Hms.Api.IntegrationTests;

[Collection("pg")]
public class ProcurementTests(PostgresFixture fx) : IAsyncLifetime
{
    public Task InitializeAsync() => fx.ResetAsync();
    public Task DisposeAsync() => CleanupAsync();

    private async Task CleanupAsync()
    {
        await using var db = fx.NewTenantContext();
        // procurement tables aren't in ResetAsync — clear them so supplier codes stay unique.
        await db.Database.ExecuteSqlRawAsync(
            "TRUNCATE suppliers, purchase_orders, purchase_order_lines, goods_received_notes, grn_lines, document_counters CASCADE;");
    }

    private ProcurementService Svc() => new(fx.Factory());

    [Fact]
    public async Task Po_to_grn_increases_stock_and_recomputes_average_cost()
    {
        var svc = Svc();
        var supplier = await svc.CreateSupplierAsync(new SupplierInput("SUP1", "Acme Foods", null, null, null, null, 7), default);

        // PO: 100 units @ cost 300 (current avg is 350 from seed, qty 50).
        var po = await svc.CreatePoAsync(new CreatePoInput(fx.LocationId, supplier.Id, null, null,
            new() { new PoLineInput(fx.ChickenKottuId, 100, 300m) }), default);
        po.Status.Should().Be("draft");
        po.TotalAmount.Should().Be(30000m);

        await svc.ReceiveAsync(new ReceiveInput(fx.LocationId, supplier.Id, po.Id, null,
            new() { new GrnLineInput(fx.ChickenKottuId, 100, 300m, "B1", null) }), default);

        await using var db = fx.NewTenantContext();
        var stock = await db.ProductStocks.IgnoreQueryFilters()
            .FirstAsync(s => s.ProductId == fx.ChickenKottuId && s.LocationId == fx.LocationId);
        stock.QuantityOnHand.Should().Be(150m);                       // 50 + 100
        // weighted avg = (50*350 + 100*300) / 150 = 47500/150 = 316.6667
        stock.AverageCost.Should().Be(316.6667m);

        var poAfter = await db.PurchaseOrders.IgnoreQueryFilters().Include(p => p.Lines).FirstAsync(p => p.Id == po.Id);
        poAfter.Status.Should().Be("received");
        poAfter.Lines.Single().QuantityReceived.Should().Be(100m);
    }

    [Fact]
    public async Task Partial_receipt_marks_po_partially_received()
    {
        var svc = Svc();
        var supplier = await svc.CreateSupplierAsync(new SupplierInput("SUP2", "Beta", null, null, null, null, null), default);
        var po = await svc.CreatePoAsync(new CreatePoInput(fx.LocationId, supplier.Id, null, null,
            new() { new PoLineInput(fx.ChickenKottuId, 100, 300m) }), default);

        await svc.ReceiveAsync(new ReceiveInput(fx.LocationId, supplier.Id, po.Id, null,
            new() { new GrnLineInput(fx.ChickenKottuId, 40, 300m, null, null) }), default);

        var after = await svc.GetPoAsync(po.Id, default);
        after!.Status.Should().Be("partially_received");
        after.Lines.Single().QuantityReceived.Should().Be(40m);
    }

    [Fact]
    public async Task Standalone_grn_without_po_still_receives_stock()
    {
        var svc = Svc();
        var supplier = await svc.CreateSupplierAsync(new SupplierInput("SUP3", "Gamma", null, null, null, null, null), default);
        var grn = await svc.ReceiveAsync(new ReceiveInput(fx.LocationId, supplier.Id, null, "adhoc",
            new() { new GrnLineInput(fx.LionLagerId, 24, 200m, null, null) }), default);

        grn.GrnNumber.Should().StartWith("GRN-");
        await using var db = fx.NewTenantContext();
        var stock = await db.ProductStocks.IgnoreQueryFilters().FirstAsync(s => s.ProductId == fx.LionLagerId && s.LocationId == fx.LocationId);
        stock.QuantityOnHand.Should().Be(74m);   // 50 + 24
    }

    // ── negatives ──
    [Fact]
    public async Task Duplicate_supplier_code_throws()
    {
        var svc = Svc();
        await svc.CreateSupplierAsync(new SupplierInput("DUP", "One", null, null, null, null, null), default);
        var act = () => svc.CreateSupplierAsync(new SupplierInput("DUP", "Two", null, null, null, null, null), default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*already exists*");
    }

    [Fact]
    public async Task Po_with_no_lines_throws()
    {
        var svc = Svc();
        var s = await svc.CreateSupplierAsync(new SupplierInput("SUP4", "Delta", null, null, null, null, null), default);
        var act = () => svc.CreatePoAsync(new CreatePoInput(fx.LocationId, s.Id, null, null, new()), default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*at least one line*");
    }

    [Fact]
    public async Task Vat_registered_supplier_captures_input_vat_on_po_and_grn()
    {
        var svc = Svc();
        // VAT-registered supplier → input VAT (18%) captured; stock cost stays ex-VAT.
        var sup = await svc.CreateSupplierAsync(new SupplierInput("VATSUP", "VAT Foods", null, null, null, null, 0,
            "123456789-7000", true), default);
        sup.IsVatRegistered.Should().BeTrue();

        var po = await svc.CreatePoAsync(new CreatePoInput(fx.LocationId, sup.Id, null, null,
            new() { new PoLineInput(fx.ChickenKottuId, 100, 300m) }), default);   // 30,000 ex-VAT
        po.SubtotalAmount.Should().Be(30000m);
        po.TaxAmount.Should().Be(5400m);        // 18% of 30,000
        po.TotalAmount.Should().Be(35400m);     // VAT-inclusive

        await svc.ReceiveAsync(new ReceiveInput(fx.LocationId, sup.Id, po.Id, null,
            new() { new GrnLineInput(fx.ChickenKottuId, 100, 300m, null, null) }), default);

        await using var db = fx.NewTenantContext();
        var grn = await db.GoodsReceivedNotes.IgnoreQueryFilters().OrderByDescending(g => g.CreatedAt).FirstAsync();
        grn.TotalCost.Should().Be(30000m);      // ex-VAT goods value
        grn.TaxAmount.Should().Be(5400m);        // reclaimable input VAT
        // stock avg cost must NOT include the VAT
        var stock = await db.ProductStocks.IgnoreQueryFilters().FirstAsync(s => s.ProductId == fx.ChickenKottuId && s.LocationId == fx.LocationId);
        stock.AverageCost.Should().Be(316.6667m);  // (50*350 + 100*300)/150 — VAT excluded
    }

    [Fact]
    public async Task Non_vat_supplier_captures_no_input_vat()
    {
        var svc = Svc();
        var sup = await svc.CreateSupplierAsync(new SupplierInput("PLAIN", "Plain Foods", null, null, null, null, 0,
            null, false), default);
        sup.IsVatRegistered.Should().BeFalse();
        var po = await svc.CreatePoAsync(new CreatePoInput(fx.LocationId, sup.Id, null, null,
            new() { new PoLineInput(fx.ChickenKottuId, 10, 300m) }), default);
        po.TaxAmount.Should().Be(0m);
        po.TotalAmount.Should().Be(3000m);
    }

    [Fact]
    public async Task Po_with_unknown_supplier_throws()
    {
        var svc = Svc();
        var act = () => svc.CreatePoAsync(new CreatePoInput(fx.LocationId, Guid.NewGuid(), null, null,
            new() { new PoLineInput(fx.ChickenKottuId, 1, 1m) }), default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Supplier not found*");
    }

    // ── PO landed cost (line + header discount, other charges), currency, units ──
    [Fact]
    public async Task Po_landed_cost_applies_line_and_header_discounts_plus_charges()
    {
        var svc = Svc();
        var sup = await svc.CreateSupplierAsync(new SupplierInput("LAND", "Landed", null, null, null, null, null), default);
        // line: 10 @ 300, line discount 200 → goods 2800. header discount 100, freight 50.
        var po = await svc.CreatePoAsync(new CreatePoInput(fx.LocationId, sup.Id, null, null,
            new() { new PoLineInput(fx.ChickenKottuId, 10, 300m, DiscountAmount: 200m) },
            DiscountAmount: 100m, OtherCharges: 50m), default);

        po.SubtotalAmount.Should().Be(2800m);          // 10*300 − 200
        po.OtherCharges.Should().Be(50m);
        po.DiscountAmount.Should().Be(100m);
        po.TaxAmount.Should().Be(0m);                  // non-VAT supplier
        po.TotalAmount.Should().Be(2750m);             // 2800 − 100 + 50
    }

    [Fact]
    public async Task Po_in_foreign_currency_records_rate()
    {
        var svc = Svc();
        var sup = await svc.CreateSupplierAsync(new SupplierInput("FX", "Importer", null, null, null, null, null), default);
        var po = await svc.CreatePoAsync(new CreatePoInput(fx.LocationId, sup.Id, null, null,
            new() { new PoLineInput(fx.ChickenKottuId, 1, 100m) }, CurrencyCode: "usd", CurrencyRate: 300m), default);
        po.CurrencyCode.Should().Be("USD");
        po.CurrencyRate.Should().Be(300m);
    }

    [Fact]
    public async Task Po_line_unit_must_convert_to_stock_unit()
    {
        var svc = Svc();
        var sup = await svc.CreateSupplierAsync(new SupplierInput("POU", "Units", null, null, null, null, null), default);
        // Rice is mass (kg); ordering in a volume unit (L) is rejected.
        var act = () => svc.CreatePoAsync(new CreatePoInput(fx.LocationId, sup.Id, null, null,
            new() { new PoLineInput(fx.RiceId, 1, 200m, UnitId: fx.LitreUnitId) }), default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*can't be converted*");
    }

    [Fact]
    public async Task Po_header_discount_cannot_exceed_value()
    {
        var svc = Svc();
        var sup = await svc.CreateSupplierAsync(new SupplierInput("POD", "Disc", null, null, null, null, null), default);
        var act = () => svc.CreatePoAsync(new CreatePoInput(fx.LocationId, sup.Id, null, null,
            new() { new PoLineInput(fx.ChickenKottuId, 1, 100m) }, DiscountAmount: 9999m), default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*exceeds*");
    }

    // ── PO lifecycle (send / edit / cancel) + approval (maker-checker) ──
    [Fact]
    public async Task Po_send_then_cannot_edit()
    {
        var svc = Svc();
        var sup = await svc.CreateSupplierAsync(new SupplierInput("SND", "Send", null, null, null, null, null), default);
        var po = await svc.CreatePoAsync(new CreatePoInput(fx.LocationId, sup.Id, null, null,
            new() { new PoLineInput(fx.ChickenKottuId, 1, 100m) }), default);
        po.Status.Should().Be("draft");

        var sent = await svc.SendPoAsync(po.Id, default);
        sent.Status.Should().Be("sent");
        sent.SentAt.Should().NotBeNull();

        var act = () => svc.EditPoAsync(po.Id, new CreatePoInput(fx.LocationId, sup.Id, null, null,
            new() { new PoLineInput(fx.ChickenKottuId, 2, 100m) }), default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Only a draft*");
    }

    [Fact]
    public async Task Po_edit_replaces_lines_and_recomputes_total()
    {
        var svc = Svc();
        var sup = await svc.CreateSupplierAsync(new SupplierInput("EDT", "Edit", null, null, null, null, null), default);
        var po = await svc.CreatePoAsync(new CreatePoInput(fx.LocationId, sup.Id, null, null,
            new() { new PoLineInput(fx.ChickenKottuId, 1, 100m) }), default);
        po.TotalAmount.Should().Be(100m);

        var edited = await svc.EditPoAsync(po.Id, new CreatePoInput(fx.LocationId, sup.Id, null, null,
            new() { new PoLineInput(fx.ChickenKottuId, 3, 100m), new PoLineInput(fx.LionLagerId, 2, 50m) }), default);
        edited.Lines.Count.Should().Be(2);
        edited.TotalAmount.Should().Be(400m);   // 3*100 + 2*50
    }

    [Fact]
    public async Task Po_cannot_be_cancelled_after_a_receipt()
    {
        var svc = Svc();
        var sup = await svc.CreateSupplierAsync(new SupplierInput("CXL", "Cancel", null, null, null, null, null), default);
        var po = await svc.CreatePoAsync(new CreatePoInput(fx.LocationId, sup.Id, null, null,
            new() { new PoLineInput(fx.ChickenKottuId, 10, 100m) }), default);

        // a clean draft cancels fine
        var po2 = await svc.CreatePoAsync(new CreatePoInput(fx.LocationId, sup.Id, null, null,
            new() { new PoLineInput(fx.ChickenKottuId, 1, 100m) }), default);
        (await svc.CancelPoAsync(po2.Id, "dup", default)).Status.Should().Be("cancelled");

        // one with a receipt cannot
        await svc.ReceiveAsync(new ReceiveInput(fx.LocationId, sup.Id, po.Id, null,
            new() { new GrnLineInput(fx.ChickenKottuId, 4, 100m, null, null) }), default);
        var act = () => svc.CancelPoAsync(po.Id, "x", default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*receipts*");
    }

    [Fact]
    public async Task Approval_required_blocks_send_until_approved()
    {
        await using (var db = fx.NewTenantContext())
        {
            var s = await db.OrgSettings.FirstAsync();
            s.RequirePoApproval = true;
            await db.SaveChangesAsync();
        }
        try
        {
            var svc = Svc();
            var sup = await svc.CreateSupplierAsync(new SupplierInput("APP", "Approve", null, null, null, null, null), default);
            var po = await svc.CreatePoAsync(new CreatePoInput(fx.LocationId, sup.Id, null, null,
                new() { new PoLineInput(fx.ChickenKottuId, 1, 100m) }), default);
            po.ApprovalStatus.Should().Be("pending");

            var blocked = () => svc.SendPoAsync(po.Id, default);
            await blocked.Should().ThrowAsync<InvalidOperationException>().WithMessage("*approved*");

            var approverId = Guid.NewGuid();
            var ap = await svc.ApprovePoAsync(po.Id, approverId, default);
            ap.ApprovalStatus.Should().Be("approved");
            ap.ApprovedBy.Should().Be(approverId);

            (await svc.SendPoAsync(po.Id, default)).Status.Should().Be("sent");
        }
        finally
        {
            await using var db = fx.NewTenantContext();
            var s = await db.OrgSettings.FirstAsync();
            s.RequirePoApproval = false;
            await db.SaveChangesAsync();
        }
    }

    // ── GRN receive enhancements (units, free, discount, invoice, void) ──
    [Fact]
    public async Task Receive_in_grams_converts_to_kilograms_of_stock()
    {
        var svc = Svc();
        var sup = await svc.CreateSupplierAsync(new SupplierInput("GSUP", "Grams", null, null, null, null, null), default);
        // Rice is stocked in KG @ 200 (qty 50). Receive 2000 g @ 0.25/g.
        await svc.ReceiveAsync(new ReceiveInput(fx.LocationId, sup.Id, null, null,
            new() { new GrnLineInput(fx.RiceId, 2000, 0.25m, null, null, UnitId: fx.GramUnitId) }), default);

        await using var db = fx.NewTenantContext();
        var stock = await db.ProductStocks.IgnoreQueryFilters().FirstAsync(s => s.ProductId == fx.RiceId && s.LocationId == fx.LocationId);
        stock.QuantityOnHand.Should().Be(52m);                 // 50 kg + (2000 g = 2 kg)
        stock.AverageCost.Should().Be(201.9231m);              // (50*200 + 500) / 52
    }

    [Fact]
    public async Task Free_items_add_stock_but_dilute_average_cost()
    {
        var svc = Svc();
        var sup = await svc.CreateSupplierAsync(new SupplierInput("FSUP", "Free", null, null, null, null, null), default);
        // Buy 10 kg @ 200, get 2 free.
        await svc.ReceiveAsync(new ReceiveInput(fx.LocationId, sup.Id, null, null,
            new() { new GrnLineInput(fx.RiceId, 10, 200m, null, null, FreeQuantity: 2) }), default);

        await using var db = fx.NewTenantContext();
        var stock = await db.ProductStocks.IgnoreQueryFilters().FirstAsync(s => s.ProductId == fx.RiceId && s.LocationId == fx.LocationId);
        stock.QuantityOnHand.Should().Be(62m);                 // 50 + 12 (incl 2 free)
        stock.AverageCost.Should().Be(193.5484m);              // (50*200 + 2000) / 62
    }

    [Fact]
    public async Task Line_discount_lowers_landed_cost()
    {
        var svc = Svc();
        var sup = await svc.CreateSupplierAsync(new SupplierInput("DSUP", "Disc", null, null, null, null, null), default);
        await svc.ReceiveAsync(new ReceiveInput(fx.LocationId, sup.Id, null, null,
            new() { new GrnLineInput(fx.RiceId, 10, 200m, null, null, DiscountAmount: 500) }), default);

        await using var db = fx.NewTenantContext();
        var stock = await db.ProductStocks.IgnoreQueryFilters().FirstAsync(s => s.ProductId == fx.RiceId && s.LocationId == fx.LocationId);
        stock.QuantityOnHand.Should().Be(60m);
        stock.AverageCost.Should().Be(191.6667m);              // (50*200 + 1500) / 60
    }

    [Fact]
    public async Task Supplier_invoice_number_is_captured()
    {
        var svc = Svc();
        var sup = await svc.CreateSupplierAsync(new SupplierInput("ISUP", "Inv", null, null, null, null, null), default);
        var grn = await svc.ReceiveAsync(new ReceiveInput(fx.LocationId, sup.Id, null, null,
            new() { new GrnLineInput(fx.RiceId, 1, 200m, null, null) }, SupplierInvoiceNo: "SINV-9001"), default);
        grn.SupplierInvoiceNo.Should().Be("SINV-9001");
    }

    [Fact]
    public async Task Void_grn_reverses_the_stock_it_added()
    {
        var svc = Svc();
        var sup = await svc.CreateSupplierAsync(new SupplierInput("VSUP", "Void", null, null, null, null, null), default);
        var grn = await svc.ReceiveAsync(new ReceiveInput(fx.LocationId, sup.Id, null, null,
            new() { new GrnLineInput(fx.RiceId, 10, 200m, null, null) }), default);
        (await StockQty(fx.RiceId)).Should().Be(60m);

        var v = await svc.VoidGrnAsync(grn.Id, "wrong delivery", default);
        v.Status.Should().Be("void");
        (await StockQty(fx.RiceId)).Should().Be(50m);          // reversed
    }

    [Fact]
    public async Task Receive_unit_must_be_compatible_with_stock_unit()
    {
        var svc = Svc();
        var sup = await svc.CreateSupplierAsync(new SupplierInput("XSUP", "Bad", null, null, null, null, null), default);
        // Rice is mass (kg); a volume unit (L) can't convert.
        var act = () => svc.ReceiveAsync(new ReceiveInput(fx.LocationId, sup.Id, null, null,
            new() { new GrnLineInput(fx.RiceId, 1, 200m, null, null, UnitId: fx.LitreUnitId) }), default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*can't be converted*");
    }

    // ── supplier returns (PRN) ──
    [Fact]
    public async Task Return_to_supplier_decrements_stock_and_reverses_input_vat()
    {
        var svc = Svc();
        var sup = await svc.CreateSupplierAsync(new SupplierInput("RET", "Returns", null, null, null, null, 0, "123456789-7000", true), default);
        // receive 10 kg rice @ 200 → 60 on hand
        await svc.ReceiveAsync(new ReceiveInput(fx.LocationId, sup.Id, null, null,
            new() { new GrnLineInput(fx.RiceId, 10, 200m, null, null) }), default);
        (await StockQty(fx.RiceId)).Should().Be(60m);

        var prn = await svc.ReturnToSupplierAsync(new PurchaseReturnInput(fx.LocationId, sup.Id, null, "damaged",
            new() { new PrnLineInput(fx.RiceId, 4, 200m) }), default);

        prn.PrnNumber.Should().StartWith("PRN-");
        prn.TotalCost.Should().Be(800m);          // 4 × 200
        prn.TaxAmount.Should().Be(144m);          // 18% input VAT reversed
        (await StockQty(fx.RiceId)).Should().Be(56m);   // 60 − 4
    }

    [Fact]
    public async Task Void_return_puts_the_stock_back()
    {
        var svc = Svc();
        var sup = await svc.CreateSupplierAsync(new SupplierInput("RETV", "RetVoid", null, null, null, null, null), default);
        await svc.ReceiveAsync(new ReceiveInput(fx.LocationId, sup.Id, null, null,
            new() { new GrnLineInput(fx.RiceId, 10, 200m, null, null) }), default);
        var prn = await svc.ReturnToSupplierAsync(new PurchaseReturnInput(fx.LocationId, sup.Id, null, null,
            new() { new PrnLineInput(fx.RiceId, 4, 200m) }), default);
        (await StockQty(fx.RiceId)).Should().Be(56m);

        var v = await svc.VoidPurchaseReturnAsync(prn.Id, "mistake", default);
        v.Status.Should().Be("void");
        (await StockQty(fx.RiceId)).Should().Be(60m);
    }

    [Fact]
    public async Task Cannot_return_more_than_on_hand()
    {
        var svc = Svc();
        var sup = await svc.CreateSupplierAsync(new SupplierInput("RETX", "RetBad", null, null, null, null, null), default);
        var act = () => svc.ReturnToSupplierAsync(new PurchaseReturnInput(fx.LocationId, sup.Id, null, null,
            new() { new PrnLineInput(fx.RiceId, 9999, 200m) }), default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Insufficient*");
    }

    private async Task<decimal> StockQty(System.Guid product)
    {
        await using var db = fx.NewTenantContext();
        var s = await db.ProductStocks.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.ProductId == product && x.LocationId == fx.LocationId);
        return s?.QuantityOnHand ?? 0m;
    }

    [Fact]
    public async Task Configured_prefixes_apply_to_po_and_supplier_autocode()
    {
        await using (var db = fx.NewTenantContext())
        {
            var s = await db.OrgSettings.FirstAsync();
            s.PoPrefix = "PUR"; s.SupplierPrefix = "VEND";
            await db.SaveChangesAsync();
        }
        try
        {
            var svc = Svc();
            // No code supplied → auto-generated with the configured supplier prefix.
            var sup = await svc.CreateSupplierAsync(new SupplierInput(null, "Auto Coded", null, null, null, null, null), default);
            sup.Code.Should().StartWith("VEND-");

            var po = await svc.CreatePoAsync(new CreatePoInput(fx.LocationId, sup.Id, null, null,
                new() { new PoLineInput(fx.ChickenKottuId, 1, 100m) }), default);
            po.PoNumber.Should().StartWith("PUR-");
        }
        finally
        {
            await using var db = fx.NewTenantContext();
            var s = await db.OrgSettings.FirstAsync();
            s.PoPrefix = "PO"; s.SupplierPrefix = "SUP";
            await db.SaveChangesAsync();
        }
    }
}
