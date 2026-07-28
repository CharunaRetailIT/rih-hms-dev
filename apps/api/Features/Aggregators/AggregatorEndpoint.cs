using System.Security.Cryptography;
using System.Text;
using Hms.Api.Domain;
using Hms.Api.Features.Aggregators.PickMe;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.Aggregators;

public static class AggregatorEndpoint
{
    public static IEndpointRouteBuilder MapAggregatorEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/v1/aggregator").WithTags("Aggregators").RequireAuthorization("Operations");

        // ─── Merchant credential config (dashboard) — per tenant, encrypted at rest ───
        g.MapGet("/credentials", async (ITenantDbContextFactory f, ISecretProtector prot, CancellationToken ct) =>
        {
            await using var db = await f.CreateForCurrentAsync(ct);
            var creds = await db.AggregatorCredentials.AsNoTracking().ToListAsync(ct);
            var stores = await db.LocationAggregatorMaps.AsNoTracking().ToListAsync(ct);
            var locs = await db.Locations.AsNoTracking().Where(l => !l.IsDeleted)
                .Select(l => new { l.Id, l.Code, l.Name }).ToListAsync(ct);

            var result = AggregatorService.Supported.Select(agg =>
            {
                var c = creds.FirstOrDefault(x => x.Aggregator == agg);
                return new
                {
                    aggregator = agg,
                    isEnabled = c?.IsEnabled ?? false,
                    environment = c?.Environment ?? "sandbox",
                    clientId = c?.ClientId,
                    // secrets are NEVER returned — only whether they're set + a masked hint
                    hasClientSecret = !string.IsNullOrEmpty(c?.ClientSecretEnc),
                    clientSecretHint = prot.Mask(c?.ClientSecretEnc),
                    hasWebhookSecret = !string.IsNullOrEmpty(c?.WebhookSecretEnc),
                    baseUrl = c?.BaseUrl,
                    stores = locs.Select(l =>
                    {
                        var m = stores.FirstOrDefault(s => s.Aggregator == agg && s.LocationId == l.Id);
                        return new {
                            locationId = l.Id, l.Code, l.Name, externalStoreId = m?.ExternalStoreId,
                            isEnabled = m?.IsEnabled ?? false,
                            hasApiKey = !string.IsNullOrEmpty(m?.ApiKeyEnc),   // PickMe per-outlet key (never returned)
                            lastPolledAt = m?.LastPolledAt,
                        };
                    }),
                };
            });
            return Results.Ok(result);
        }).WithName("Aggregator.GetCredentials").WithSummary("Merchant's aggregator config (secrets masked).");

        g.MapPut("/credentials/{name}", async (string name, CredentialInput body,
            ITenantDbContextFactory f, ISecretProtector prot, CancellationToken ct) =>
        {
            if (!AggregatorService.Supported.Contains(name)) return Results.BadRequest(new { error = "Unknown aggregator" });
            await using var db = await f.CreateForCurrentAsync(ct);
            var c = await db.AggregatorCredentials.FirstOrDefaultAsync(x => x.Aggregator == name, ct);
            if (c is null)
            {
                c = new AggregatorCredential { Id = Guid.NewGuid(), TenantId = db.TenantId, Aggregator = name, CreatedAt = DateTime.UtcNow };
                db.AggregatorCredentials.Add(c);
            }
            c.ClientId = body.ClientId ?? c.ClientId;
            // Only overwrite a secret if a new non-empty value is supplied (so the UI
            // can submit without re-typing existing secrets).
            if (!string.IsNullOrWhiteSpace(body.ClientSecret)) c.ClientSecretEnc = prot.Encrypt(body.ClientSecret);
            if (!string.IsNullOrWhiteSpace(body.WebhookSecret)) c.WebhookSecretEnc = prot.Encrypt(body.WebhookSecret);
            c.Environment = body.Environment ?? c.Environment;
            c.BaseUrl = body.BaseUrl ?? c.BaseUrl;
            c.IsEnabled = body.IsEnabled ?? c.IsEnabled;
            c.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { c.Aggregator, c.IsEnabled, c.Environment, hasClientSecret = c.ClientSecretEnc != null });
        }).WithName("Aggregator.SetCredentials").WithSummary("Set/update a merchant's aggregator keys (encrypted at rest).");

        g.MapPut("/credentials/{name}/stores/{locationId:guid}", async (string name, Guid locationId, StoreMapInput body,
            ITenantDbContextFactory f, ISecretProtector prot, CancellationToken ct) =>
        {
            if (!AggregatorService.Supported.Contains(name)) return Results.BadRequest(new { error = "Unknown aggregator" });
            await using var db = await f.CreateForCurrentAsync(ct);
            var m = await db.LocationAggregatorMaps.FirstOrDefaultAsync(x => x.Aggregator == name && x.LocationId == locationId, ct);
            if (m is null)
            {
                m = new LocationAggregatorMap { Id = Guid.NewGuid(), TenantId = db.TenantId, Aggregator = name, LocationId = locationId, CreatedAt = DateTime.UtcNow };
                db.LocationAggregatorMaps.Add(m);
            }
            m.ExternalStoreId = body.ExternalStoreId;
            // PickMe per-outlet X-API-KEY — only overwrite when a new value is sent (encrypted at rest).
            if (!string.IsNullOrWhiteSpace(body.ApiKey)) m.ApiKeyEnc = prot.Encrypt(body.ApiKey);
            m.IsEnabled = body.IsEnabled ?? m.IsEnabled;
            m.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { m.LocationId, m.ExternalStoreId, m.IsEnabled, hasApiKey = !string.IsNullOrEmpty(m.ApiKeyEnc) });
        }).WithName("Aggregator.SetStoreMap").WithSummary("Map an outlet to its aggregator store id + PickMe X-API-KEY (encrypted).");

        // ─── PickMe (real API) — poll orders + preview the menu for ref_id mapping ───
        g.MapPost("/pickme/poll", async (PickMeService svc, CancellationToken ct) =>
        {
            try { return Results.Ok(new { ingested = await svc.PollAsync(ct) }); }
            catch (PickMeApiException ex) { return Results.BadRequest(new { error = ex.Message, code = ex.Code }); }
        }).WithName("Aggregator.PickMePoll").WithSummary("Poll PickMe /joblist now and ingest any new orders.");

        g.MapGet("/pickme/menu", async (Guid? locationId, PickMeService svc, CancellationToken ct) =>
        {
            try
            {
                var menu = await svc.GetMenuAsync(locationId, ct);
                return menu is null ? Results.BadRequest(new { error = "PickMe is not configured (or no API key) for this outlet" }) : Results.Ok(menu);
            }
            catch (PickMeApiException ex) { return Results.BadRequest(new { error = ex.Message, code = ex.Code }); }
        }).WithName("Aggregator.PickMeMenu").WithSummary("Fetch the outlet's live PickMe menu (id / ref_id / price / availability).");

        // DEV simulator — inject a realistic Uber Eats / PickMe order for testing.
        g.MapPost("/{name}/simulate", async (string name, SimulateInput body, AggregatorService svc,
            ITenantDbContextFactory f, CancellationToken ct) =>
        {
            if (!AggregatorService.Supported.Contains(name)) return Results.BadRequest(new { error = "Unknown aggregator" });
            Guid locationId = body.LocationId ?? await ResolveDefaultLocation(f, ct);
            try
            {
                var order = await svc.SimulateAsync(name, locationId, body.Seed ?? Random.Shared.Next(1, 99999), ct);
                return Results.Ok(new { order.Id, order.OrderNumber, order.OrderSource, order.ExternalOrderId,
                    order.Status, order.TotalAmount, order.CustomerName, order.DeliveryAddress });
            }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Aggregator.Simulate").WithSummary("DEV: inject a test order as if it came from the aggregator webhook.");

        // Real webhook receiver — HMAC verified against the MERCHANT's stored
        // webhook secret (decrypted from the DB), NOT an env value.
        g.MapPost("/{name}/webhook", async (string name, HttpRequest req, AggregatorService svc,
            ISecretProtector prot, ITenantDbContextFactory f, CancellationToken ct) =>
        {
            if (!AggregatorService.Supported.Contains(name)) return Results.BadRequest(new { error = "Unknown aggregator" });

            using var reader = new StreamReader(req.Body);
            var raw = await reader.ReadToEndAsync(ct);

            // NOTE on tenant resolution: a real public webhook has no tenant context.
            // We resolve the tenant by matching the payload's store_id against
            // location_aggregator_map.external_store_id. In dev the simulator runs
            // with X-Tenant-Id, so we load that tenant's credential here.
            await using var db = await f.CreateForCurrentAsync(ct);
            var cred = await db.AggregatorCredentials.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Aggregator == name && c.IsEnabled, ct);
            if (cred is null) return Results.BadRequest(new { error = $"{name} not configured for this merchant" });

            var secret = prot.Decrypt(cred.WebhookSecretEnc);
            if (!string.IsNullOrEmpty(secret))
            {
                var sig = req.Headers["X-Signature"].ToString();
                if (!VerifyHmac(raw, secret, sig)) return Results.Unauthorized();
            }
            // Real adapters parse the aggregator-specific JSON → NormalisedOrder, then
            // call svc.IngestAsync(name, locationId, normalised, ct). The simulate
            // endpoint exercises that exact ingestion path today.
            return Results.Accepted();
        }).WithName("Aggregator.Webhook").WithSummary("Production webhook — HMAC against the merchant's stored secret (DB, not env).")
          .AllowAnonymous();   // public callback from Uber/PickMe — authenticated by HMAC, not our JWT

        // Incoming queue — orders awaiting merchant acceptance.
        g.MapGet("/incoming", async (ITenantDbContextFactory f, CancellationToken ct) =>
        {
            await using var db = await f.CreateForCurrentAsync(ct);
            var list = await db.Orders.Include(o => o.Items).AsNoTracking()
                .Where(o => o.AggregatorStatus == "pending")
                .OrderBy(o => o.OpenedAt)
                .Select(o => new {
                    o.Id, o.OrderNumber, o.OrderSource, o.ExternalOrderId, o.CustomerName,
                    o.DeliveryAddress, o.DeliveryPhone, o.DeliveryNotes, o.TotalAmount, o.OpenedAt,
                    items = o.Items.Where(i => !i.IsDeleted).Select(i => new { i.ProductName, i.Quantity, i.Notes }),
                }).ToListAsync(ct);
            return Results.Ok(list);
        }).WithName("Aggregator.Incoming").WithSummary("Orders awaiting accept/reject.");

        // Lifecycle transitions
        g.MapPost("/orders/{id:guid}/accept", async (Guid id, AcceptInput? body, AggregatorService svc, CancellationToken ct) =>
        {
            try { return Results.Ok(AggDto(await svc.AcceptAsync(id, body?.PrepMinutes, ct))); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Aggregator.Accept").WithSummary("Accept an order with a prep/waiting time; fires KOT + pushes accept.");

        g.MapPost("/orders/{id:guid}/ready", async (Guid id, AggregatorService svc, CancellationToken ct) =>
        {
            try { return Results.Ok(AggDto(await svc.SetReadyAsync(id, ct))); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Aggregator.Ready").WithSummary("Mark ready → pushes ready_for_pickup to the aggregator.");

        g.MapPost("/orders/{id:guid}/pickup", async (Guid id, AggregatorService svc, CancellationToken ct) =>
        {
            try { return Results.Ok(AggDto(await svc.SetPickedUpAsync(id, ct))); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Aggregator.Pickup");

        g.MapPost("/orders/{id:guid}/reject", async (Guid id, RejectInput? body, AggregatorService svc, CancellationToken ct) =>
        {
            try { return Results.Ok(AggDto(await svc.RejectAsync(id, body?.Reason, ct))); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Aggregator.Reject");

        // 86 an item — set availability and sync to the aggregator menus.
        g.MapPost("/availability", async (AvailabilityInput body, AggregatorService svc, CancellationToken ct) =>
        {
            try { await svc.SetItemAvailabilityAsync(body.ProductId, body.Available, ct); return Results.Ok(new { ok = true }); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Aggregator.SetAvailability").WithSummary("Mark a product available/unavailable → syncs to Uber/PickMe menus.");

        // List aggregator (delivery) orders with their lifecycle state.
        g.MapGet("/orders", async (ITenantDbContextFactory f, CancellationToken ct) =>
        {
            await using var db = await f.CreateForCurrentAsync(ct);
            var list = await db.Orders.AsNoTracking()
                .Where(o => o.OrderSource == "ubereats" || o.OrderSource == "pickme")
                .OrderByDescending(o => o.OpenedAt)
                .Select(o => new { o.Id, o.OrderNumber, o.OrderSource, o.ExternalOrderId, o.Status,
                    o.AggregatorStatus, o.PrepMinutes, o.PromisedTime,
                    o.CustomerName, o.DeliveryAddress, o.TotalAmount, o.OpenedAt, o.InvoiceNumber })
                .Take(100).ToListAsync(ct);
            return Results.Ok(list);
        }).WithName("Aggregator.ListOrders");

        // Outbox view + mock-process.
        g.MapGet("/outbox", async (ITenantDbContextFactory f, CancellationToken ct) =>
        {
            await using var db = await f.CreateForCurrentAsync(ct);
            return Results.Ok(await db.AggregatorOutbox.AsNoTracking()
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new { o.Id, o.Aggregator, o.ExternalOrderId, o.Operation, o.Status, o.Attempts, o.CreatedAt, o.SentAt })
                .Take(100).ToListAsync(ct));
        }).WithName("Aggregator.Outbox");

        g.MapPost("/outbox/process", async (AggregatorService svc, CancellationToken ct) =>
            Results.Ok(new { sent = await svc.ProcessOutboxAsync(ct) }))
            .WithName("Aggregator.ProcessOutbox").WithSummary("DEV: mark pending callbacks sent (prod = Hangfire worker).");

        return app;
    }

    private static object AggDto(Hms.Api.Domain.Order o) => new
    {
        o.Id, o.OrderNumber, o.OrderSource, o.ExternalOrderId, o.Status,
        o.AggregatorStatus, o.PrepMinutes, o.PromisedTime, o.CustomerName,
        o.DeliveryAddress, o.TotalAmount, o.InvoiceNumber,
    };

    private static async Task<Guid> ResolveDefaultLocation(ITenantDbContextFactory f, CancellationToken ct)
    {
        await using var db = await f.CreateForCurrentAsync(ct);
        var loc = await db.Locations.AsNoTracking().Where(l => l.CanSell).OrderBy(l => l.Code).FirstOrDefaultAsync(ct)
            ?? await db.Locations.AsNoTracking().OrderBy(l => l.Code).FirstAsync(ct);
        return loc.Id;
    }

    private static bool VerifyHmac(string body, string secret, string providedSig)
    {
        using var h = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var computed = Convert.ToHexString(h.ComputeHash(Encoding.UTF8.GetBytes(body))).ToLowerInvariant();
        var provided = providedSig.Replace("sha256=", "").Trim().ToLowerInvariant();
        return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(computed), Encoding.UTF8.GetBytes(provided));
    }
}

public record SimulateInput(Guid? LocationId, int? Seed);
public record CredentialInput(string? ClientId, string? ClientSecret, string? WebhookSecret,
    string? Environment, string? BaseUrl, bool? IsEnabled);
public record StoreMapInput(string? ExternalStoreId, bool? IsEnabled, string? ApiKey = null);
public record AcceptInput(int? PrepMinutes);
public record RejectInput(string? Reason);
public record AvailabilityInput(Guid ProductId, bool Available);
