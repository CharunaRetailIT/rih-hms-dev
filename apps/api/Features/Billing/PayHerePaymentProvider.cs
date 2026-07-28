using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Hms.Api.Features.Billing;

/// <summary>
/// PayHere implementation of the payment seam (#110). Bills a previously-tokenized card
/// (captured via the preapproval flow → <see cref="Subscription.CustomerToken"/>) using
/// PayHere's OAuth + Charging API, server-to-server. Selected only when BOTH credential
/// pairs are configured (see Program.cs); otherwise the manual stub stays in place so the
/// self-serve flow keeps working.
/// </summary>
public sealed class PayHerePaymentProvider(
    IHttpClientFactory http, IOptions<PayHereOptions> opt, ControlDbContext control, ILogger<PayHerePaymentProvider> log)
    : IPaymentProvider
{
    private readonly PayHereOptions _o = opt.Value;
    public string Name => "payhere";

    public async Task<PaymentResult> ChargeAsync(Guid tenantId, decimal amount, string currency, string description, CancellationToken ct)
    {
        if (amount <= 0) return new PaymentResult(true, "payhere-zero", null);   // downgrade / nothing to bill

        var sub = await control.Subscriptions.AsNoTracking()
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && !s.IsDeleted, ct);
        if (sub is null || string.IsNullOrWhiteSpace(sub.CustomerToken))
            return new PaymentResult(false, null, "NO_PAYMENT_METHOD");          // UI prompts owner to add a card

        var token = await GetAccessTokenAsync(ct);
        if (token is null) return new PaymentResult(false, null, "PayHere authorization failed");

        var orderId = $"SUB-{tenantId.ToString("N")[..8]}-{DateTime.UtcNow.Ticks}";   // unique, traceable
        var body = new
        {
            type = "PAYMENT",
            order_id = orderId,
            items = Trunc(description, 100),
            currency,
            amount = decimal.Round(amount, 2),
            customer_token = sub.CustomerToken,
        };

        var client = http.CreateClient("payhere");
        using var req = new HttpRequestMessage(HttpMethod.Post, $"{_o.ApiBase}/merchant/v1/payment/charge")
        {
            Content = JsonContent.Create(body),
        };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        string json;
        try
        {
            using var resp = await client.SendAsync(req, ct);
            json = await resp.Content.ReadAsStringAsync(ct);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "PayHere charge transport error for {Tenant}", tenantId);
            return new PaymentResult(false, null, "PayHere unreachable");
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var status = root.TryGetProperty("status", out var st) && st.ValueKind == JsonValueKind.Number ? st.GetInt32() : -99;
            if (status == 1)
            {
                string? pid = root.TryGetProperty("data", out var d) && d.TryGetProperty("payment_id", out var p) ? p.ToString() : null;
                return new PaymentResult(true, $"payhere-{pid ?? orderId}", null);
            }
            var msg = root.TryGetProperty("msg", out var m) ? m.GetString() : null;
            log.LogWarning("PayHere charge declined for {Tenant}: status={Status} msg={Msg}", tenantId, status, msg);
            return new PaymentResult(false, null, msg ?? "Card charge was declined");
        }
        catch (Exception ex)
        {
            log.LogError(ex, "PayHere charge parse error: {Body}", Trunc(json, 300));
            return new PaymentResult(false, null, "PayHere returned an unexpected response");
        }
    }

    /// <summary>OAuth client-credentials token (cached ~10 min by PayHere; we fetch per charge — charges are rare).</summary>
    public async Task<string?> GetAccessTokenAsync(CancellationToken ct)
    {
        var client = http.CreateClient("payhere");
        var basic = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{_o.AppId}:{_o.AppSecret}"));
        using var req = new HttpRequestMessage(HttpMethod.Post, $"{_o.ApiBase}/merchant/v1/oauth/token")
        {
            Content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("grant_type", "client_credentials") }),
        };
        req.Headers.Authorization = new AuthenticationHeaderValue("Basic", basic);
        try
        {
            using var resp = await client.SendAsync(req, ct);
            if (!resp.IsSuccessStatusCode)
            {
                log.LogWarning("PayHere OAuth failed: {Code}", resp.StatusCode);
                return null;
            }
            using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync(ct));
            return doc.RootElement.TryGetProperty("access_token", out var t) ? t.GetString() : null;
        }
        catch (Exception ex)
        {
            log.LogError(ex, "PayHere OAuth transport error");
            return null;
        }
    }

    private static string Trunc(string s, int n) => string.IsNullOrEmpty(s) || s.Length <= n ? s : s[..n];
}
