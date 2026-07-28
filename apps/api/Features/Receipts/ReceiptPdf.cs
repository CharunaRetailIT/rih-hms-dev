using Hms.Api.Domain;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Hms.Api.Features.Receipts;

/// <summary>
/// Renders a clean A4 tax-invoice / receipt PDF for e-receipts (#79), attached to the email.
/// VAT-aware: when the bill is a tax invoice (VAT broken out) it's headed "TAX INVOICE" and shows
/// the supplier's VAT/BR numbers + a separate VAT line; otherwise it's a plain itemised "RECEIPT".
/// </summary>
public static class ReceiptPdf
{
    public static byte[] Build(Order o, OrgSettings? s, string ccy, string bizName, bool isTaxInvoice, byte[]? logo, string? customerName)
    {
        string Money(decimal n) => $"{ccy} {n:N2}";
        var taxLabel = string.IsNullOrWhiteSpace(s?.TaxLabel) ? "VAT" : s!.TaxLabel;
        var title = isTaxInvoice ? "TAX INVOICE" : "RECEIPT";
        var no = o.InvoiceNumber ?? o.OrderNumber;
        var when = (o.SettledAt ?? DateTime.UtcNow).ToString("yyyy-MM-dd HH:mm");
        var items = o.Items.Where(i => !i.IsDeleted).OrderBy(i => i.CreatedAt).ThenBy(i => i.Id).ToList();
        var discount = o.DiscountAmount + o.PromotionDiscountAmount;
        var green = "#15803d";

        return Document.Create(doc =>
        {
            doc.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(36);
                page.DefaultTextStyle(t => t.FontSize(10).FontColor("#222222"));

                // ── Header: logo + business identity (left), invoice meta (right) ──
                page.Header().Column(head =>
                {
                    head.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            if (logo is { Length: > 0 }) c.Item().Height(46).AlignLeft().Image(logo).FitHeight();
                            c.Item().PaddingTop(logo is { Length: > 0 } ? 6 : 0).Text(bizName).FontSize(16).Bold().FontColor(green);
                            if (!string.IsNullOrWhiteSpace(s?.LegalName) && s!.LegalName != bizName) c.Item().Text(s.LegalName!).FontSize(9).FontColor("#666666");
                            if (isTaxInvoice && !string.IsNullOrWhiteSpace(s?.VatRegistrationNumber)) c.Item().Text($"{taxLabel} Reg: {s!.VatRegistrationNumber}").FontSize(9).FontColor("#666666");
                            if (!string.IsNullOrWhiteSpace(s?.BusinessRegistrationNo)) c.Item().Text($"BR: {s!.BusinessRegistrationNo}").FontSize(9).FontColor("#666666");
                        });
                        row.ConstantItem(200).Column(c =>
                        {
                            c.Item().AlignRight().Text(title).FontSize(15).Bold();
                            c.Item().AlignRight().Text(no).FontSize(11).FontColor("#444444");
                            c.Item().AlignRight().Text(when).FontSize(9).FontColor("#888888");
                            if (!string.IsNullOrWhiteSpace(o.TableLabel)) c.Item().AlignRight().Text($"Table {o.TableLabel}").FontSize(9).FontColor("#888888");
                        });
                    });
                    head.Item().PaddingTop(10).LineHorizontal(1).LineColor("#dddddd");
                });

                // ── Body: customer + line items + totals ──
                page.Content().PaddingVertical(12).Column(col =>
                {
                    if (!string.IsNullOrWhiteSpace(customerName))
                        col.Item().PaddingBottom(8).Text($"Billed to: {customerName}").FontSize(10).FontColor("#444444");

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(cd => { cd.RelativeColumn(3); cd.ConstantColumn(45); cd.ConstantColumn(110); });
                        table.Header(h =>
                        {
                            h.Cell().BorderBottom(1).BorderColor("#cccccc").PaddingBottom(4).Text("Item").Bold();
                            h.Cell().BorderBottom(1).BorderColor("#cccccc").PaddingBottom(4).AlignRight().Text("Qty").Bold();
                            h.Cell().BorderBottom(1).BorderColor("#cccccc").PaddingBottom(4).AlignRight().Text("Amount").Bold();
                        });
                        foreach (var i in items)
                        {
                            table.Cell().PaddingVertical(3).Text(i.ProductName);
                            table.Cell().PaddingVertical(3).AlignRight().Text($"{i.Quantity:0.##}");
                            table.Cell().PaddingVertical(3).AlignRight().Text(Money(i.LineSubtotal));
                        }
                    });

                    col.Item().PaddingTop(10).AlignRight().Width(260).Column(t =>
                    {
                        void TLine(string label, decimal v)
                        {
                            t.Item().Row(r =>
                            {
                                r.RelativeItem().Text(label).FontColor("#555555");
                                r.ConstantItem(120).AlignRight().Text(Money(v)).FontColor("#555555");
                            });
                        }
                        TLine("Subtotal", o.SubtotalAmount);
                        if (discount > 0) TLine("Discount", -discount);
                        if (o.ServiceChargeAmount > 0) TLine("Service charge", o.ServiceChargeAmount);
                        if (isTaxInvoice && o.TaxAmount > 0) TLine(taxLabel, o.TaxAmount);
                        t.Item().PaddingTop(6).LineHorizontal(1).LineColor("#dddddd");
                        t.Item().PaddingTop(4).Row(r =>
                        {
                            r.RelativeItem().Text("Total").FontSize(13).Bold();
                            r.ConstantItem(120).AlignRight().Text(Money(o.TotalAmount)).FontSize(13).Bold();
                        });
                    });
                });

                // ── Footer ──
                page.Footer().Column(f =>
                {
                    f.Item().PaddingTop(8).LineHorizontal(1).LineColor("#eeeeee");
                    f.Item().PaddingTop(6).Text(string.IsNullOrWhiteSpace(s?.TaxInvoiceFooter) ? "Thank you for your visit." : s!.TaxInvoiceFooter).FontSize(9).FontColor("#888888");
                    if (!string.IsNullOrWhiteSpace(s?.BillFooter)) f.Item().Text(s!.BillFooter!).FontSize(8).FontColor("#aaaaaa");
                });
            });
        }).GeneratePdf();
    }
}
