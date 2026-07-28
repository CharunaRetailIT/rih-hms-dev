using System.Text.Json;
using Hms.Api.Domain;
using Hms.Api.Features.Aggregators.PickMe;
using Hms.Api.Features.Orders;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.Aggregators;

/// <summary>
/// Delivery-aggregator integration (Uber Eats / PickMe). No public sandbox is
/// available for either, so this is mock-ready: a simulator injects realistic
/// orders that flow through the SAME path a real webhook would. Swapping in the
/// real Uber Eats / PickMe API later means implementing the parse/verify/send
/// adapter — the ingestion + outbox stay identical.
///
/// Flow (per docs/integration/uber-eats.md):
///   webhook/simulate → normalise → idempotency check → create order
///   (order_source, external id, delivery fields, prepaid) → KOT → settle prepaid
///   → enqueue "accept" callback to the outbox.
/// </summary>
public class AggregatorService(
    ITenantDbContextFactory factory, OrderService orders,
    PickMeClient pickme, ISecretProtector prot, ILogger<AggregatorService> logger,
    Hms.Api.Features.Realtime.RealtimeBus bus)
{
    public static readonly string[] Supported = { "ubereats", "pickme" };

    /// <summary>A normalised aggregator order — what both Uber and PickMe map down to.</summary>
    public record NormalisedOrder(
        string ExternalOrderId, string CustomerName, string DeliveryAddress,
        string DeliveryPhone, string? DeliveryNotes, List<NormalisedLine> Lines, string RawJson);
    public record NormalisedLine(string Sku, decimal Quantity, string? Notes);

    /// <summary>
    /// DEV simulator — builds a realistic order from the tenant's real menu and
    /// runs it through Ingest, exactly as a webhook would. One-click test order.
    /// </summary>
    public async Task<Order> SimulateAsync(string aggregator, Guid locationId, int seed, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var menu = await db.Products.AsNoTracking()
            .Where(p => p.IsActive && p.IsSold).OrderBy(p => p.Sku).ToListAsync(ct);
        if (menu.Count == 0) throw new InvalidOperationException("No products to build a test order from");

        // Deterministic-ish pick from the menu (seed varies the basket).
        var pick = new[] { menu[seed % menu.Count], menu[(seed + 3) % menu.Count] }.Distinct().ToList();
        var lines = pick.Select((p, i) => new NormalisedLine(p.Sku, (i % 2) + 1, i == 0 ? "Extra spicy" : null)).ToList();

        var ext = $"{(aggregator == "ubereats" ? "UE" : "PM")}-{seed:D5}";
        var who = aggregator == "ubereats" ? "Nimal P. (Uber Eats)" : "Kasun S. (PickMe)";
        var addr = aggregator == "ubereats" ? "42 Marine Drive, Colombo 03" : "7 Temple Rd, Nugegoda";
        var normalised = new NormalisedOrder(ext, who, addr, "+9477" + (1000000 + seed),
            "Leave at reception", lines,
            JsonSerializer.Serialize(new { source = aggregator, ext, items = lines }));

        return await IngestAsync(aggregator, locationId, normalised, ct);
    }

    /// <summary>
    /// Ingest a normalised order. It lands as PENDING acceptance — the order
    /// shows in the merchant's incoming queue; the kitchen is NOT fired and no
    /// money is taken until the merchant accepts. Idempotent on (source, ext id).
    /// </summary>
    public async Task<Order> IngestAsync(string aggregator, Guid locationId, NormalisedOrder n, CancellationToken ct)
    {
        await using (var db = await factory.CreateForCurrentAsync(ct))
        {
            var existing = await db.Orders.AsNoTracking()
                .FirstOrDefaultAsync(o => o.OrderSource == aggregator && o.ExternalOrderId == n.ExternalOrderId, ct);
            if (existing is not null) return existing;
        }

        var created = await orders.CreateAsync(new CreateOrderInput(
            LocationId: locationId, OrderType: "delivery", OrderSource: aggregator,
            ExternalOrderId: n.ExternalOrderId, TableLabel: null, Covers: null, CashierId: null,
            CustomerName: n.CustomerName, DeliveryAddress: n.DeliveryAddress,
            DeliveryPhone: n.DeliveryPhone, DeliveryNotes: n.DeliveryNotes, AggregatorPayload: n.RawJson), ct);

        await using (var db = await factory.CreateForCurrentAsync(ct))
        {
            foreach (var line in n.Lines)
            {
                var product = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Sku == line.Sku, ct);
                if (product is null) continue;
                await orders.AddItemAsync(created.Id, new AddItemInput(product.Id, line.Quantity, "kitchen", line.Notes), ct);
            }
            // Mark as pending acceptance (the incoming queue).
            var order = await db.Orders.FirstAsync(o => o.Id == created.Id, ct);
            order.AggregatorStatus = "pending";
            await db.SaveChangesAsync(ct);

            // Push a live signal: a new delivery order needs acceptance. The
            // notification bell + delivery board react instantly (no poll).
            bus.Publish(db.TenantId, "notifications");
            bus.Publish(db.TenantId, "orders");
        }

        return (await orders.GetAsync(created.Id, ct))!;
    }

    /// <summary>
    /// Merchant ACCEPTS an incoming order with a prep/waiting time. This is when
    /// the kitchen fires and payment is recorded (aggregator already collected it).
    /// Pushes the "accepted + prep time" callback to the aggregator.
    /// </summary>
    public async Task<Order> AcceptAsync(Guid orderId, int? prepMinutes, CancellationToken ct)
    {
        string aggregator, ext;
        int prep;
        await using (var db = await factory.CreateForCurrentAsync(ct))
        {
            var o = await db.Orders.FirstOrDefaultAsync(x => x.Id == orderId, ct)
                ?? throw new InvalidOperationException("Order not found");
            if (o.AggregatorStatus != "pending") throw new InvalidOperationException($"Order is already {o.AggregatorStatus}");
            var loc = await db.Locations.AsNoTracking().FirstAsync(l => l.Id == o.LocationId, ct);
            prep = prepMinutes ?? loc.DefaultPrepMinutes;
            aggregator = o.OrderSource; ext = o.ExternalOrderId!;
            o.AggregatorStatus = "preparing";
            o.PrepMinutes = prep;
            o.AcceptedAt = DateTime.UtcNow;
            o.PromisedTime = DateTime.UtcNow.AddMinutes(prep);
            await db.SaveChangesAsync(ct);
        }

        // Fire the KOT (R2 — ticket already carries order_source = uber/pickme), then
        // record the prepaid payment + decrement stock via settle.
        await orders.ConfirmAsync(orderId, ct);
        var current = await orders.GetAsync(orderId, ct);
        var payType = aggregator == "ubereats" ? "ubereats_prepaid" : "pickme_prepaid";
        await orders.SettleAsync(orderId, new SettleInput(
            new() { new PaymentInput(payType, current!.TotalAmount, ext) }, current.CustomerName), ct);

        await EnqueueAsync(aggregator, ext, "accept", new { status = "accepted", prepMinutes = prep }, ct);
        return (await orders.GetAsync(orderId, ct))!;
    }

    /// <summary>Kitchen marks the order ready → pushes "ready_for_pickup" to the aggregator.</summary>
    public async Task<Order> SetReadyAsync(Guid orderId, CancellationToken ct) =>
        await TransitionAsync(orderId, "ready", "ready_for_pickup", o => o.ReadyAt = DateTime.UtcNow, ct);

    /// <summary>Driver collected → pushes "picked_up" to the aggregator.</summary>
    public async Task<Order> SetPickedUpAsync(Guid orderId, CancellationToken ct) =>
        await TransitionAsync(orderId, "picked_up", "picked_up", o => o.PickedUpAt = DateTime.UtcNow, ct);

    public async Task<Order> RejectAsync(Guid orderId, string? reason, CancellationToken ct)
    {
        string aggregator, ext;
        await using (var db = await factory.CreateForCurrentAsync(ct))
        {
            var o = await db.Orders.FirstOrDefaultAsync(x => x.Id == orderId, ct)
                ?? throw new InvalidOperationException("Order not found");
            aggregator = o.OrderSource; ext = o.ExternalOrderId!;
            o.AggregatorStatus = "rejected";
            await db.SaveChangesAsync(ct);
        }
        await orders.VoidAsync(orderId, reason ?? "Rejected", ct);
        await EnqueueAsync(aggregator, ext, "reject", new { status = "rejected", reason }, ct);
        return (await orders.GetAsync(orderId, ct))!;
    }

    private async Task<Order> TransitionAsync(Guid orderId, string aggStatus, string callbackStatus, Action<Order> stamp, CancellationToken ct)
    {
        string aggregator, ext;
        await using (var db = await factory.CreateForCurrentAsync(ct))
        {
            var o = await db.Orders.FirstOrDefaultAsync(x => x.Id == orderId, ct)
                ?? throw new InvalidOperationException("Order not found");
            aggregator = o.OrderSource; ext = o.ExternalOrderId!;
            o.AggregatorStatus = aggStatus;
            stamp(o);
            await db.SaveChangesAsync(ct);
        }
        await EnqueueAsync(aggregator, ext, "status", new { status = callbackStatus }, ct);
        return (await orders.GetAsync(orderId, ct))!;
    }

    /// <summary>
    /// 86 an item: set its online availability and push a menu-availability update
    /// to every aggregator the merchant has enabled. Stock-driven auto-86 can call
    /// this from settle when quantity hits zero (enhancement).
    /// </summary>
    public async Task SetItemAvailabilityAsync(Guid productId, bool available, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == productId, ct)
            ?? throw new InvalidOperationException("Product not found");
        product.IsAvailableOnline = available;
        await db.SaveChangesAsync(ct);
        var sku = product.Sku; var price = product.BasePrice;

        var enabled = await db.AggregatorCredentials.AsNoTracking().Where(c => c.IsEnabled).Select(c => c.Aggregator).ToListAsync(ct);
        foreach (var agg in enabled)
            await EnqueueAsync(agg, sku, "menu_item", new { sku, price, available }, ct);

        // PickMe has a real menu API — push price + availability live to each keyed
        // outlet (Uber stays on the outbox/mock until its adapter lands).
        await PushPickMeAvailabilityAsync(db, sku, price, available, ct);
    }

    /// <summary>Live-push a product's price + availability to every enabled, keyed PickMe outlet.</summary>
    private async Task PushPickMeAvailabilityAsync(TenantDbContext db, string sku, decimal price, bool available, CancellationToken ct)
    {
        var cred = await db.AggregatorCredentials.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Aggregator == "pickme" && c.IsEnabled, ct);
        if (cred is null) return;
        var baseUrl = PickMeClient.ResolveBaseUrl(cred.Environment, cred.BaseUrl);
        var maps = await db.LocationAggregatorMaps.AsNoTracking()
            .Where(m => m.Aggregator == "pickme" && m.IsEnabled && m.ApiKeyEnc != null).ToListAsync(ct);
        var ms = available ? PickMeClient.StatusAvailable : PickMeClient.StatusUnavailable;
        foreach (var m in maps)
        {
            var key = prot.Decrypt(m.ApiKeyEnc);
            if (string.IsNullOrEmpty(key)) continue;
            try { await pickme.UpdateItemByRefAsync(baseUrl, key, sku, price, ms, ct); }
            catch (Exception ex) { logger.LogWarning(ex, "PickMe availability push failed for {Sku} @ outlet {Loc}", sku, m.LocationId); }
        }
    }

    public async Task EnqueueAsync(string aggregator, string externalOrderId, string operation, object payload, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        db.AggregatorOutbox.Add(new AggregatorOutbox
        {
            Id = Guid.NewGuid(), TenantId = db.TenantId, Aggregator = aggregator,
            ExternalOrderId = externalOrderId, Operation = operation,
            PayloadJson = JsonSerializer.Serialize(payload), Status = "pending",
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// DEV outbox processor — marks pending callbacks sent (mock). In prod a
    /// Hangfire worker POSTs each to the aggregator with retry + HMAC signing.
    /// </summary>
    public async Task<int> ProcessOutboxAsync(CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var pending = await db.AggregatorOutbox.Where(o => o.Status == "pending").ToListAsync(ct);
        foreach (var o in pending) { o.Status = "sent"; o.SentAt = DateTime.UtcNow; o.Attempts++; o.UpdatedAt = DateTime.UtcNow; }
        await db.SaveChangesAsync(ct);
        return pending.Count;
    }
}
