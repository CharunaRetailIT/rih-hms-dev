using System.Text.Json;

namespace Hms.Api.Features.Fx;

/// <summary>
/// Live foreign-exchange reference rates from a trustworthy public source
/// (the Exchange Rate API — open.er-api.com — which aggregates central-bank
/// and commercial feeds and supports LKR). Used to seed per-outlet currency
/// prices; we never auto-apply a rate to live billing without review.
/// </summary>
public static class FxEndpoint
{
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(8) };

    public static IEndpointRouteBuilder MapFxEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/fx/rate", async (string? from, string? to, CancellationToken ct) =>
        {
            var b = (from ?? "USD").Trim().ToUpperInvariant();
            var q = (to ?? "LKR").Trim().ToUpperInvariant();
            if (b.Length != 3 || q.Length != 3) return Results.BadRequest(new { error = "from/to must be 3-letter currency codes" });
            if (b == q) return Results.Ok(new { from = b, to = q, rate = 1m, asOf = (string?)null, source = "identity" });
            try
            {
                using var resp = await Http.GetAsync($"https://open.er-api.com/v6/latest/{b}", ct);
                if (!resp.IsSuccessStatusCode) return Results.Json(new { error = "FX provider unavailable" }, statusCode: 502);
                using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync(ct));
                var root = doc.RootElement;
                if (root.TryGetProperty("result", out var res) && res.GetString() != "success")
                    return Results.Json(new { error = "FX provider returned an error" }, statusCode: 502);
                if (!root.GetProperty("rates").TryGetProperty(q, out var rateEl))
                    return Results.BadRequest(new { error = $"No published rate for {q}." });
                var rate = rateEl.GetDecimal();
                var asOf = root.TryGetProperty("time_last_update_utc", out var t) ? t.GetString() : null;
                return Results.Ok(new { from = b, to = q, rate, asOf, source = "Exchange Rate API (open.er-api.com)" });
            }
            catch (Exception)
            {
                return Results.Json(new { error = "Could not reach the FX provider. Try again shortly." }, statusCode: 502);
            }
        }).WithName("Fx.Rate").RequireAuthorization()
          .WithSummary("Latest reference rate: how many 'to' units per 1 'from' (e.g. from=USD&to=LKR).");

        return app;
    }
}
