namespace Hms.Api.Features.Billing;

/// <summary>
/// Payment seam (#110). A real gateway (PayHere/Stripe) plugs in behind this; until then
/// the manual provider records the intent and succeeds, so self-serve purchases are
/// architecturally honest (charge happens here) without faking it in the UI.
/// </summary>
public interface IPaymentProvider
{
    string Name { get; }
    Task<PaymentResult> ChargeAsync(Guid tenantId, decimal amount, string currency, string description, CancellationToken ct);
}

public record PaymentResult(bool Success, string? Reference, string? Error);

public sealed class ManualPaymentProvider : IPaymentProvider
{
    public string Name => "manual";
    public Task<PaymentResult> ChargeAsync(Guid tenantId, decimal amount, string currency, string description, CancellationToken ct)
        => Task.FromResult(new PaymentResult(true, "manual-" + Guid.NewGuid().ToString("N")[..10], null));
}
