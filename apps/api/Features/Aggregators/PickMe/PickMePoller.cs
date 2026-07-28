using Hms.Api.Domain;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.Aggregators.PickMe;

/// <summary>
/// Background poller for PickMe orders. PickMe has no webhook — the merchant POS
/// must pull /joblist — so this hosted service sweeps every active tenant on a
/// timer and ingests new jobs for each enabled, keyed outlet.
///
/// OFF by default (Aggregators:PickMe:PollingEnabled=false) so it never runs in
/// tests/dev or hits the network unasked. Enable in production via config/env.
///
/// Single-instance: each running API process polls. For a multi-instance
/// deployment add leader election (or move polling to a dedicated worker) so a
/// tenant isn't polled N times and burns the 30-calls/min budget.
/// </summary>
public sealed class PickMePoller(IServiceScopeFactory scopes, IConfiguration config, ILogger<PickMePoller> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        if (!config.GetValue("Aggregators:PickMe:PollingEnabled", false))
        {
            logger.LogInformation("PickMe poller disabled (Aggregators:PickMe:PollingEnabled=false).");
            return;
        }
        var seconds = Math.Max(20, config.GetValue("Aggregators:PickMe:PollSeconds", 60));
        logger.LogInformation("PickMe poller started — sweeping active tenants every {Seconds}s.", seconds);

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(seconds));
        do
        {
            try { await PollAllTenantsAsync(ct); }
            catch (OperationCanceledException) { break; }
            catch (Exception ex) { logger.LogError(ex, "PickMe poll cycle failed."); }
        }
        while (await timer.WaitForNextTickAsync(ct));
    }

    private async Task PollAllTenantsAsync(CancellationToken ct)
    {
        List<Guid> tenantIds;
        using (var scope = scopes.CreateScope())
        {
            var control = scope.ServiceProvider.GetRequiredService<ControlDbContext>();
            tenantIds = await control.Tenants.AsNoTracking()
                .Where(t => t.Status == TenantStatus.Active
                         || t.Status == TenantStatus.Trialing
                         || t.Status == TenantStatus.PastDue)
                .Select(t => t.Id).ToListAsync(ct);
        }

        foreach (var tid in tenantIds)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                using var scope = scopes.CreateScope();
                scope.ServiceProvider.GetRequiredService<ITenantContext>().Set(tid);
                var svc = scope.ServiceProvider.GetRequiredService<PickMeService>();
                var n = await svc.PollAsync(ct);
                if (n > 0) logger.LogInformation("PickMe: ingested {N} new order(s) for tenant {Tid}.", n, tid);
            }
            catch (Exception ex) { logger.LogWarning(ex, "PickMe poll failed for tenant {Tid}.", tid); }
        }
    }
}
