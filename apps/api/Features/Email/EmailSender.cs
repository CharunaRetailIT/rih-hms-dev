using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace Hms.Api.Features.Email;

/// <summary>An optional file attachment for an email (e.g. the e-receipt PDF). #79</summary>
public sealed record EmailAttachment(string Filename, byte[] Content, string ContentType = "application/pdf");

/// <summary>Transactional email seam — magic-link sign-in + e-receipts/invoices with PDF attachments (#79).</summary>
public interface IEmailSender
{
    Task<bool> SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct, IReadOnlyList<EmailAttachment>? attachments = null);
}

public sealed class ResendOptions
{
    public string? ApiKey { get; set; }
    public string FromEmail { get; set; } = "noreply@hms.retailit.lk";
    public string FromName { get; set; } = "RIT HMS";
}

/// <summary>Sends via Resend (https://resend.com). The sending domain must be verified in Resend
/// (DNS) for delivery to arbitrary recipients; until then sends may bounce / be owner-only.</summary>
public sealed class ResendEmailSender(IHttpClientFactory http, IOptions<ResendOptions> opt, ILogger<ResendEmailSender> log) : IEmailSender
{
    private readonly ResendOptions _o = opt.Value;

    public async Task<bool> SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct, IReadOnlyList<EmailAttachment>? attachments = null)
    {
        var from = string.IsNullOrWhiteSpace(_o.FromName) ? _o.FromEmail : $"{_o.FromName} <{_o.FromEmail}>";
        var client = http.CreateClient("resend");
        var payload = new Dictionary<string, object?> { ["from"] = from, ["to"] = new[] { toEmail }, ["subject"] = subject, ["html"] = htmlBody };
        if (attachments is { Count: > 0 })
            payload["attachments"] = attachments.Select(a => new { filename = a.Filename, content = Convert.ToBase64String(a.Content), content_type = a.ContentType }).ToList();
        using var req = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails")
        {
            Content = JsonContent.Create(payload),
        };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _o.ApiKey);
        try
        {
            using var resp = await client.SendAsync(req, ct);
            if (resp.IsSuccessStatusCode) return true;
            log.LogWarning("Resend send failed ({Code}): {Body}", resp.StatusCode,
                (await resp.Content.ReadAsStringAsync(ct)) is { Length: > 0 } b ? b[..Math.Min(300, b.Length)] : "");
            return false;
        }
        catch (Exception ex) { log.LogError(ex, "Resend transport error sending to {To}", toEmail); return false; }
    }
}

/// <summary>Fallback when no email provider is configured: logs the message (dev / pre-Resend).</summary>
public sealed class LogOnlyEmailSender(ILogger<LogOnlyEmailSender> log) : IEmailSender
{
    public Task<bool> SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct, IReadOnlyList<EmailAttachment>? attachments = null)
    {
        log.LogInformation("EMAIL (no provider) → {To} | {Subject} | {Attachments} attachment(s)", toEmail, subject, attachments?.Count ?? 0);
        return Task.FromResult(true);
    }
}
