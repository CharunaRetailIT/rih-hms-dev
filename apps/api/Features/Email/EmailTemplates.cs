using System.Net;

namespace Hms.Api.Features.Email;

/// <summary>
/// Branded transactional-email templates. Table-based + inline styles for email-client
/// compatibility (Outlook/Gmail/Apple Mail). Reused for magic-link now, receipts/invoices later (#79).
/// </summary>
public static class EmailTemplates
{
    private const string Green = "#15803d";
    private const string Ink = "#0f172a";
    private const string Muted = "#64748b";
    private const string Font = "-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif";

    /// <summary>Wrap inner body HTML in the branded shell (header bar + card + footer).</summary>
    public static string Wrap(string title, string innerHtml) => $@"<!DOCTYPE html>
<html lang=""en""><head><meta charset=""utf-8""><meta name=""viewport"" content=""width=device-width,initial-scale=1""><title>{WebUtility.HtmlEncode(title)}</title></head>
<body style=""margin:0;padding:0;background:#f1f5f9;"">
  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#f1f5f9;padding:32px 12px;font-family:{Font};"">
    <tr><td align=""center"">
      <table role=""presentation"" width=""520"" cellpadding=""0"" cellspacing=""0"" style=""max-width:520px;width:100%;background:#ffffff;border-radius:16px;overflow:hidden;border:1px solid #e2e8f0;"">
        <tr><td style=""background:{Green};padding:22px 32px;"">
          <span style=""color:#ffffff;font-size:20px;font-weight:800;letter-spacing:-0.4px;"">RIT&nbsp;HMS</span>
        </td></tr>
        <tr><td style=""padding:32px;color:{Ink};"">{innerHtml}</td></tr>
        <tr><td style=""padding:18px 32px;background:#f8fafc;border-top:1px solid #e2e8f0;color:#94a3b8;font-size:12px;line-height:18px;"">
          RIT HMS — hospitality management by Retail Information Technologies (Pvt) Ltd.<br>
          You received this email because it was requested for your account.
        </td></tr>
      </table>
    </td></tr>
  </table>
</body></html>";

    /// <summary>Magic-link sign-in email.</summary>
    public static string MagicLink(string tenantName, string link)
    {
        var name = WebUtility.HtmlEncode(tenantName);
        var inner = $@"
<h1 style=""margin:0 0 12px;font-size:22px;font-weight:800;color:{Ink};"">Sign in to RIT&nbsp;HMS</h1>
<p style=""margin:0 0 24px;font-size:15px;line-height:23px;color:{Muted};"">Click the button below to sign in to <strong style=""color:{Ink};"">{name}</strong>. For your security this link expires in <strong>15&nbsp;minutes</strong> and can be used once.</p>
<table role=""presentation"" cellpadding=""0"" cellspacing=""0""><tr><td style=""border-radius:10px;background:{Green};"">
  <a href=""{link}"" style=""display:inline-block;padding:14px 34px;font-size:15px;font-weight:700;color:#ffffff;text-decoration:none;border-radius:10px;"">Sign in &rarr;</a>
</td></tr></table>
<p style=""margin:28px 0 6px;font-size:12px;color:#94a3b8;"">Or paste this link into your browser:</p>
<p style=""margin:0;font-size:12px;word-break:break-all;""><a href=""{link}"" style=""color:{Green};text-decoration:none;"">{link}</a></p>
<hr style=""border:none;border-top:1px solid #e2e8f0;margin:28px 0 16px;"">
<p style=""margin:0;font-size:13px;line-height:20px;color:#94a3b8;"">If you didn&rsquo;t request this, you can safely ignore this email — your account stays secure.</p>";
        return Wrap("Your RIT HMS sign-in link", inner);
    }

    /// <summary>Approval-request email with a no-login link to Approve / Reject / Hold (#approvals).</summary>
    public static string ApprovalLink(string docType, string docNumber, decimal amount, string link)
    {
        var label = docType switch { "purchase_order" => "Purchase Order", "grn" => "Goods Receipt", "stock_transfer" => "Stock Transfer", "wastage_note" => "Wastage Note", "request_note" => "Request Note", "stock_adjustment" => "Stock Adjustment", _ => "Document" };
        var inner = $@"
<h1 style=""margin:0 0 12px;font-size:22px;font-weight:800;color:{Ink};"">Approval needed</h1>
<p style=""margin:0 0 8px;font-size:15px;line-height:23px;color:{Muted};"">A <strong style=""color:{Ink};"">{WebUtility.HtmlEncode(label)}</strong> needs your approval:</p>
<table role=""presentation"" cellpadding=""0"" cellspacing=""0"" style=""margin:0 0 24px;font-size:15px;color:{Ink};"">
  <tr><td style=""padding:2px 16px 2px 0;color:{Muted};"">Reference</td><td style=""font-weight:700;"">{WebUtility.HtmlEncode(docNumber)}</td></tr>
  <tr><td style=""padding:2px 16px 2px 0;color:{Muted};"">Amount</td><td style=""font-weight:700;"">{amount:N2}</td></tr>
</table>
<table role=""presentation"" cellpadding=""0"" cellspacing=""0""><tr><td style=""border-radius:10px;background:{Green};"">
  <a href=""{link}"" style=""display:inline-block;padding:14px 34px;font-size:15px;font-weight:700;color:#ffffff;text-decoration:none;border-radius:10px;"">Review &amp; decide &rarr;</a>
</td></tr></table>
<p style=""margin:24px 0 6px;font-size:12px;color:#94a3b8;"">You can Approve, Reject or Hold from that page (no login needed). A note is required to reject or hold. Link valid for 7 days.</p>
<p style=""margin:0;font-size:12px;word-break:break-all;""><a href=""{link}"" style=""color:{Green};text-decoration:none;"">{link}</a></p>";
        return Wrap($"Approval needed — {docNumber}", inner);
    }
}
