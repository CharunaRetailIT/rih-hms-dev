using Microsoft.Extensions.Options;

namespace Hms.Api.Features.Billing;

/// <summary>
/// Recurring-billing worker (#110): once an hour, bills any subscription whose period has
/// ended via <see cref="SubscriptionService.RenewDueAsync"/>. It is a strict no-op unless a
/// real gateway is configured (both PayHere credential pairs present), so it never auto-bills
/// while the system is on the manual stub.
/// </summary>
public sealed class SubscriptionRenewalPoller(
    IServiceScopeFactory scopes, IOptions<PayHereOptions> opt, ILogger<SubscriptionRenewalPoller> log)
    : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Let the app finish starting before the first tick.
        try { await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken); } catch (OperationCanceledException) { return; }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (opt.Value.IsFullyConfigured)
                {
                    using var scope = scopes.CreateScope();
                    var svc = scope.ServiceProvider.GetRequiredService<SubscriptionService>();
                    var (charged, failed) = await svc.RenewDueAsync(stoppingToken);
                    if (charged + failed > 0)
                        log.LogInformation("Subscription renewal tick: {Charged} charged, {Failed} past_due", charged, failed);
                }
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex) { log.LogError(ex, "Subscription renewal tick failed"); }

            try { await Task.Delay(Interval, stoppingToken); } catch (OperationCanceledException) { break; }
        }
    }
}
