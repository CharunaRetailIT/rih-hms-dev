using System.Text.Json;
using Hms.Api.Features.Orders;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.Aggregators.PickMe;

/// <summary>
/// PickMe order intake. PickMe is PULL-based: we poll /joblist and mirror each
/// job into our system. There is no accept/decline API (the merchant accepts in
/// the PickMe Merchant App), so any job we see is already confirmed — we create
/// it, fire the KOT, record the prepaid total, and then just track the driver
/// lifecycle on subsequent polls. Idempotent on pickme_job_id.
///
/// Runs in the CURRENT tenant context: the background poller sets the tenant per
/// scope, the manual endpoint uses the caller's JWT.
/// </summary>
public sealed class PickMeService(
    ITenantDbContextFactory factory,
    OrderService orders,
    PickMeClient client,
    ISecretProtector prot,
    ILogger<PickMeService> logger)
{
    private const int PollHours = 2;   // overlap window; dedup on job id absorbs repeats
    private const int MaxPages = 10;   // safety bound vs the 30-calls/min limit

    // PickMe statuses that mean the job is dead → void our order, never create one.
    private static readonly HashSet<string> CancelStatuses = new(StringComparer.OrdinalIgnoreCase)
    { "Job Declined", "Job Timed Out", "Job Cancelled" };

    public static bool IsCancelled(string? status) => status is not null && CancelStatuses.Contains(status.Trim());
    public static bool IsPickup(string? deliveryMode) => string.Equals(deliveryMode?.Trim(), "PickUp", StringComparison.OrdinalIgnoreCase);
    public static string MapOrderType(string? deliveryMode) => IsPickup(deliveryMode) ? "takeaway" : "delivery";
    public static decimal UnitPrice(PickMeOrderItem it) => it.Qty > 0 ? Math.Round(it.Total / it.Qty, 2) : it.Total;

    /// <summary>Fold special instructions + chosen options into one kitchen note.</summary>
    public static string? BuildItemNotes(PickMeOrderItem it)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(it.SpIns)) parts.Add(it.SpIns!.Trim());
        foreach (var opt in it.Options ?? new())
        {
            var chosen = string.Join(", ", (opt.Items ?? new()).Select(x => x.Name).Where(n => !string.IsNullOrWhiteSpace(n)));
            if (!string.IsNullOrWhiteSpace(chosen)) parts.Add($"{opt.Name}: {chosen}");
            else if (!string.IsNullOrWhiteSpace(opt.Name)) parts.Add(opt.Name!);
        }
        return parts.Count == 0 ? null : string.Join(" · ", parts);
    }

    /// <summary>Fetch an outlet's live PickMe menu (for product↔ref_id mapping). Null if unconfigured.</summary>
    public async Task<PickMeItemListResponse?> GetMenuAsync(Guid? locationId, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var cred = await db.AggregatorCredentials.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Aggregator == "pickme" && c.IsEnabled, ct);
        if (cred is null) return null;
        var baseUrl = PickMeClient.ResolveBaseUrl(cred.Environment, cred.BaseUrl);
        var q = db.LocationAggregatorMaps.AsNoTracking()
            .Where(m => m.Aggregator == "pickme" && m.IsEnabled && m.ApiKeyEnc != null);
        var map = locationId is { } lid ? await q.FirstOrDefaultAsync(m => m.LocationId == lid, ct) : await q.FirstOrDefaultAsync(ct);
        if (map is null) return null;
        var key = prot.Decrypt(map.ApiKeyEnc);
        return string.IsNullOrEmpty(key) ? null : await client.GetOutletItemsAsync(baseUrl, key, ct);
    }

    /// <summary>Poll every enabled PickMe outlet for the current tenant; returns new orders ingested.</summary>
    public async Task<int> PollAsync(CancellationToken ct)
    {
        string baseUrl;
        List<(Guid Id, Guid LocationId, string Key)> outlets = new();
        await using (var db = await factory.CreateForCurrentAsync(ct))
        {
            var cred = await db.AggregatorCredentials.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Aggregator == "pickme" && c.IsEnabled, ct);
            if (cred is null) return 0;
            baseUrl = PickMeClient.ResolveBaseUrl(cred.Environment, cred.BaseUrl);

            var maps = await db.LocationAggregatorMaps.AsNoTracking()
                .Where(m => m.Aggregator == "pickme" && m.IsEnabled && m.ApiKeyEnc != null).ToListAsync(ct);
            foreach (var m in maps)
            {
                var key = prot.Decrypt(m.ApiKeyEnc);
                if (!string.IsNullOrEmpty(key)) outlets.Add((m.Id, m.LocationId, key));
            }
        }
        if (outlets.Count == 0) return 0;

        var ingested = 0;
        foreach (var (mapId, locationId, key) in outlets)
        {
            try { ingested += await PollOutletAsync(locationId, baseUrl, key, ct); }
            catch (Exception ex) { logger.LogError(ex, "PickMe poll failed for outlet {Loc}", locationId); }

            await using var w = await factory.CreateForCurrentAsync(ct);
            var m2 = await w.LocationAggregatorMaps.FirstOrDefaultAsync(x => x.Id == mapId, ct);
            if (m2 is not null) { m2.LastPolledAt = DateTime.UtcNow; await w.SaveChangesAsync(ct); }
        }
        return ingested;
    }

    private async Task<int> PollOutletAsync(Guid locationId, string baseUrl, string key, CancellationToken ct)
    {
        int page = 1, ingested = 0;
        while (page <= MaxPages)
        {
            var resp = await client.GetJobListAsync(baseUrl, key, page, PollHours, ct);
            var jobs = resp.Data ?? new();
            foreach (var job in jobs)
            {
                try { if (await IngestJobAsync(locationId, job, ct)) ingested++; }
                catch (Exception ex) { logger.LogError(ex, "PickMe ingest failed for job {Job}", job.PickmeJobId); }
            }
            var total = resp.Params?.Pagination?.TotalRecords ?? jobs.Count;
            var size = resp.Params?.Pagination?.Size ?? 25;
            if (jobs.Count == 0 || size <= 0 || page * size >= total) break;
            page++;
        }
        return ingested;
    }

    /// <summary>
    /// Create-or-update one PickMe job. Returns true if a NEW order was created.
    /// Public so the mapping can be exercised directly (tests / manual ingest).
    /// </summary>
    public async Task<bool> IngestJobAsync(Guid locationId, PickMeJob job, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(job.PickmeJobId)) return false;
        var status = job.Status?.Name?.Trim() ?? "Merchant Confirmed";

        await using (var db = await factory.CreateForCurrentAsync(ct))
        {
            var existing = await db.Orders
                .FirstOrDefaultAsync(o => o.OrderSource == "pickme" && o.ExternalOrderId == job.PickmeJobId, ct);
            if (existing is not null)
            {
                // Mirror status changes; void if PickMe killed the job. The void is
                // best-effort — a cancel can arrive after we've already settled the
                // prepaid total, and that may be un-voidable; we still record the
                // cancel so the floor sees it.
                if (IsCancelled(status) && existing.Status != "void")
                {
                    try { await orders.VoidAsync(existing.Id, $"PickMe: {status}", ct); }
                    catch (Exception ex) { logger.LogWarning(ex, "PickMe cancel: could not void {Job} (status {S})", job.PickmeJobId, existing.Status); }
                    existing.AggregatorStatus = status;
                    await db.SaveChangesAsync(ct);
                }
                else if (existing.AggregatorStatus != status)
                {
                    existing.AggregatorStatus = status;
                    if (status is "Order Ready" or "Prep time expired") existing.ReadyAt ??= DateTime.UtcNow;
                    if (status is "Order Picked Up" or "Job Ended" or "Job Completed") existing.PickedUpAt ??= DateTime.UtcNow;
                    await db.SaveChangesAsync(ct);
                }
                return false;
            }
        }

        // New job. Don't materialise an already-dead one.
        if (IsCancelled(status)) return false;

        var phone = job.Customer?.ContactNumber;
        var created = await orders.CreateAsync(new CreateOrderInput(
            LocationId: locationId, OrderType: MapOrderType(job.DeliveryMode), OrderSource: "pickme",
            ExternalOrderId: job.PickmeJobId, TableLabel: null, Covers: null, CashierId: null,
            CustomerName: "PickMe customer", DeliveryAddress: job.Customer?.Location?.Address,
            DeliveryPhone: phone, DeliveryNotes: job.Order?.DeliveryNote,
            AggregatorPayload: JsonSerializer.Serialize(job)), ct);

        await using (var db = await factory.CreateForCurrentAsync(ct))
        {
            foreach (var it in job.Order?.Items ?? new())
            {
                var notes = BuildItemNotes(it);
                var product = string.IsNullOrWhiteSpace(it.RefId) ? null
                    : await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Sku == it.RefId, ct);
                if (product is not null)
                    await orders.AddItemAsync(created.Id, new AddItemInput(product.Id, it.Qty, "kitchen", notes), ct);
                else
                {
                    // Unmapped item (merchant hasn't set PickMe ref_id = our SKU): keep it
                    // anyway as a custom line so the order/total isn't silently wrong.
                    var name = string.IsNullOrWhiteSpace(it.Name) ? "PickMe item" : it.Name!;
                    if (!string.IsNullOrWhiteSpace(notes)) name = $"{name} ({notes})";
                    await orders.AddCustomItemAsync(created.Id, name, UnitPrice(it), it.Qty, "kitchen", ct);
                }
            }
        }

        // Fire the KOT, then record the prepaid total (PickMe already collected it).
        // Both are best-effort: a failure here (e.g. no open shift) must not stop us
        // mirroring the order onto the board.
        try { await orders.ConfirmAsync(created.Id, ct); }
        catch (Exception ex) { logger.LogWarning(ex, "PickMe confirm/KOT failed for {Job}", job.PickmeJobId); }

        var current = await orders.GetAsync(created.Id, ct);
        if (current is not null)
        {
            try
            {
                await orders.SettleAsync(created.Id, new SettleInput(
                    new() { new PaymentInput("pickme_prepaid", current.TotalAmount, job.PickmeJobId) }, current.CustomerName), ct);
            }
            catch (Exception ex) { logger.LogWarning(ex, "PickMe prepaid settle failed for {Job} (left unsettled)", job.PickmeJobId); }
        }

        await using (var db = await factory.CreateForCurrentAsync(ct))
        {
            var o = await db.Orders.FirstOrDefaultAsync(x => x.Id == created.Id, ct);
            if (o is not null) { o.AggregatorStatus = status; await db.SaveChangesAsync(ct); }
        }
        return true;
    }
}
