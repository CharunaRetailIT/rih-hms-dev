using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Hms.Api.Features.Sms;

/// <summary>Transactional SMS seam — e-receipts now (#79). Mirrors IEmailSender.</summary>
public interface ISmsSender
{
    Task<bool> SendAsync(string toPhone, string message, CancellationToken ct);
}

/// <summary>
/// Sender RT gateway config (https://senderrt.com). Bound from /opt/hms/hms.env `Sms__*`.
/// Ported from the RIT time-log Sender RT integration: token endpoint issues a short-lived
/// Bearer token, then the send endpoint takes the token + a MobileNos[] payload.
/// </summary>
public sealed class SmsOptions
{
    public string Provider { get; set; } = "sender";                                            // "sender" (Sender RT) | "off"
    public string TokenUrl { get; set; } = "https://externalapi.senderrt.com/api/202210/Token";
    public string SendUrl { get; set; } = "https://externalapi.senderrt.com/api/sms/Send";
    public int AccountId { get; set; }                                                            // Sender RT account / CompanyID
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? ApiKey { get; set; }                                                           // = Sender RT password
    public string? Mask { get; set; }                                                             // approved sender mask
    public bool Configured => !string.IsNullOrWhiteSpace(ApiKey) && !string.IsNullOrWhiteSpace(UserName) && AccountId > 0;
}

/// <summary>Sends via Sender RT. Caches the Bearer token until shortly before it expires.</summary>
public sealed class SenderRtSmsSender(IHttpClientFactory http, IOptions<SmsOptions> opt, ILogger<SenderRtSmsSender> log) : ISmsSender
{
    private readonly SmsOptions _o = opt.Value;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);
    private string? _token;
    private DateTime _tokenExpiresUtc = DateTime.MinValue;

    // SMS gateways reject formatted/local-prefixed numbers — reduce to the 9-digit subscriber number.
    private static string NormalizeMobile(string to)
    {
        var d = new string(to.Where(char.IsDigit).ToArray());
        if (d.StartsWith("94") && d.Length == 11) d = d[2..];
        else if (d.StartsWith("0") && d.Length == 10) d = d[1..];
        return d;
    }

    private async Task<string> GetTokenAsync(HttpClient client, CancellationToken ct)
    {
        if (_token is not null && _tokenExpiresUtc > DateTime.UtcNow) return _token;
        await _tokenLock.WaitAsync(ct);
        try
        {
            if (_token is not null && _tokenExpiresUtc > DateTime.UtcNow) return _token;
            using var req = new HttpRequestMessage(HttpMethod.Post, _o.TokenUrl)
            {
                Content = JsonContent.Create(new { id = _o.AccountId, userName = _o.UserName, password = _o.ApiKey, email = _o.Email }),
            };
            using var resp = await client.SendAsync(req, ct);
            var text = await resp.Content.ReadAsStringAsync(ct);
            if (!resp.IsSuccessStatusCode) throw new InvalidOperationException($"Sender RT auth failed ({(int)resp.StatusCode})");
            using var doc = JsonDocument.Parse(text);
            if (!doc.RootElement.TryGetProperty("tokens", out var tokens) || tokens.GetArrayLength() == 0)
                throw new InvalidOperationException("Sender RT returned no tokens");
            // Pick the longest-lived token.
            JsonElement best = default; double bestHours = -1;
            foreach (var t in tokens.EnumerateArray())
            {
                var hrs = t.TryGetProperty("expiryHours", out var eh) && eh.ValueKind == JsonValueKind.Number ? eh.GetDouble() : 1;
                if (hrs > bestHours) { bestHours = hrs; best = t; }
            }
            _token = best.GetProperty("token").GetString();
            _tokenExpiresUtc = DateTime.UtcNow.AddHours(Math.Max(0.1, bestHours - 0.5));   // refresh half-an-hour early
            return _token!;
        }
        finally { _tokenLock.Release(); }
    }

    public async Task<bool> SendAsync(string toPhone, string message, CancellationToken ct)
    {
        if (!_o.Configured) { log.LogWarning("SMS not configured — skipping send to {To}", toPhone); return false; }
        var client = http.CreateClient("sms");
        try
        {
            var token = await GetTokenAsync(client, ct);
            var body = new
            {
                CompanyID = _o.AccountId,
                Message = message,
                MobileNos = new[] { new { Mobile = NormalizeMobile(toPhone) } },
                Type = "sms",
                TransactionId = $"hms-{Guid.NewGuid():N}"[..20],
                Mask = _o.Mask ?? "",
                AttchmentFile = Array.Empty<object>(),
            };
            using var req = new HttpRequestMessage(HttpMethod.Post, _o.SendUrl) { Content = JsonContent.Create(body) };
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            using var resp = await client.SendAsync(req, ct);
            var text = await resp.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(text) ? "{}" : text);
            var root = doc.RootElement;
            var ok = (root.TryGetProperty("result", out var r) && r.ValueKind == JsonValueKind.Number && r.GetInt32() == 1)
                  || (root.TryGetProperty("responsCode", out var rc) && rc.ValueKind == JsonValueKind.Number && rc.GetInt32() == 200);
            if (!ok) log.LogWarning("Sender RT send failed: {Body}", text[..Math.Min(300, text.Length)]);
            return ok;
        }
        catch (Exception ex) { log.LogError(ex, "Sender RT transport error sending to {To}", toPhone); return false; }
    }
}

/// <summary>Fallback when no SMS provider is configured: logs the message (dev / pre-gateway).</summary>
public sealed class LogOnlySmsSender(ILogger<LogOnlySmsSender> log) : ISmsSender
{
    public Task<bool> SendAsync(string toPhone, string message, CancellationToken ct)
    {
        log.LogInformation("SMS (no provider) → {To} | {Msg}", toPhone, message[..Math.Min(80, message.Length)]);
        return Task.FromResult(true);
    }
}
