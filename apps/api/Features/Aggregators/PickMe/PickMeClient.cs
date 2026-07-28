using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hms.Api.Features.Aggregators.PickMe;

/// <summary>A PickMe API error (e.g. PMDP-20019 invalid outlet, PMDP-20054 bad item).</summary>
public sealed class PickMeApiException(string code, string message, int httpStatus)
    : Exception($"PickMe {code}: {message}")
{
    public string Code { get; } = code;
    public int HttpStatus { get; } = httpStatus;
}

/// <summary>
/// Typed HTTP client for the PickMe POS API v1.4.7. Stateless: base URL + the
/// outlet's X-API-KEY are passed per call, because PickMe scopes one key to one
/// outlet (a tenant with several branches has several keys). Rate limit is 30
/// calls/min/outlet — callers keep well under it (poll interval ≫ 2s).
/// </summary>
public sealed class PickMeClient(HttpClient http, ILogger<PickMeClient> logger)
{
    public const string SandboxBase = "https://api.stage-mytaxi.com";
    public const string LiveBase = "https://api.pickme.lk";

    // merchant_status values for the update endpoints.
    public const int StatusUnavailable = 0;
    public const int StatusAvailable = 1;
    public const int StatusSoldOut = 2;

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
    };

    /// <summary>Sandbox unless the credential says "live"/"production"; an explicit override wins.</summary>
    public static string ResolveBaseUrl(string? environment, string? overrideUrl)
    {
        if (!string.IsNullOrWhiteSpace(overrideUrl)) return overrideUrl.TrimEnd('/');
        var isLive = string.Equals(environment, "live", StringComparison.OrdinalIgnoreCase)
                  || string.Equals(environment, "production", StringComparison.OrdinalIgnoreCase);
        return isLive ? LiveBase : SandboxBase;
    }

    public Task<PickMeJobListResponse> GetJobListAsync(string baseUrl, string apiKey, int page, int hours, CancellationToken ct) =>
        SendAsync<PickMeJobListResponse>(HttpMethod.Get, baseUrl, apiKey,
            $"/pickme/pos/v1/joblist?page={page}&hours={hours}", null, ct);

    public Task<PickMeItemListResponse> GetOutletItemsAsync(string baseUrl, string apiKey, CancellationToken ct) =>
        SendAsync<PickMeItemListResponse>(HttpMethod.Get, baseUrl, apiKey, "/pickme/pos/v1/outlet/items", null, ct);

    /// <summary>Update an item's price + availability by the merchant Ref ID (= our SKU).</summary>
    public Task UpdateItemByRefAsync(string baseUrl, string apiKey, string refId, decimal price, int merchantStatus, CancellationToken ct) =>
        SendAsync<JsonElement>(HttpMethod.Post, baseUrl, apiKey,
            $"/pickme/pos/v1/update/item/ref/{Uri.EscapeDataString(refId)}",
            new { price, merchant_status = merchantStatus }, ct);

    /// <summary>Update an item's price + availability by the PickMe item id.</summary>
    public Task UpdateItemAsync(string baseUrl, string apiKey, long itemId, decimal price, int merchantStatus, CancellationToken ct) =>
        SendAsync<JsonElement>(HttpMethod.Post, baseUrl, apiKey,
            $"/pickme/pos/v1/update/item/{itemId}",
            new { price, merchant_status = merchantStatus }, ct);

    private async Task<T> SendAsync<T>(HttpMethod method, string baseUrl, string apiKey, string path, object? body, CancellationToken ct)
    {
        using var req = new HttpRequestMessage(method, $"{baseUrl.TrimEnd('/')}{path}");
        req.Headers.TryAddWithoutValidation("X-API-KEY", apiKey);
        req.Headers.TryAddWithoutValidation("Accept", "application/json");
        if (body is not null) req.Content = JsonContent.Create(body, options: Json);

        using var res = await http.SendAsync(req, ct);
        var raw = await res.Content.ReadAsStringAsync(ct);
        if (!res.IsSuccessStatusCode)
        {
            var (code, msg) = ParseError(raw);
            logger.LogWarning("PickMe {Method} {Path} -> {Status} {Code} {Msg}", method, path, (int)res.StatusCode, code, msg);
            throw new PickMeApiException(code, msg, (int)res.StatusCode);
        }
        return JsonSerializer.Deserialize<T>(raw, Json)!;
    }

    // Errors come in two shapes: a bare array [{message,code}] (joblist) or
    // { "errors": [{code,message}] } (items/update). Tolerate both.
    private static (string code, string message) ParseError(string raw)
    {
        try
        {
            using var doc = JsonDocument.Parse(raw);
            JsonElement err;
            if (doc.RootElement.ValueKind == JsonValueKind.Array && doc.RootElement.GetArrayLength() > 0)
                err = doc.RootElement[0];
            else if (doc.RootElement.TryGetProperty("errors", out var arr) && arr.ValueKind == JsonValueKind.Array && arr.GetArrayLength() > 0)
                err = arr[0];
            else return ("PMDP-UNKNOWN", raw.Length > 200 ? raw[..200] : raw);

            var code = err.TryGetProperty("code", out var c) ? c.GetString() ?? "PMDP-UNKNOWN" : "PMDP-UNKNOWN";
            var msg = err.TryGetProperty("message", out var m) ? m.GetString() ?? "" : "";
            return (code, msg);
        }
        catch { return ("PMDP-UNKNOWN", raw.Length > 200 ? raw[..200] : raw); }
    }
}
