namespace Hms.Api.Domain;

/// <summary>
/// A merchant's (tenant's) OAuth credentials for ONE aggregator. Secrets are
/// stored AES-GCM encrypted (the *_Enc columns). Configured from the dashboard,
/// never from env. Each tenant has at most one row per aggregator.
/// </summary>
public class AggregatorCredential
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Aggregator { get; set; } = default!;   // ubereats | pickme
    public string? ClientId { get; set; }
    public string? ClientSecretEnc { get; set; }         // encrypted
    public string? WebhookSecretEnc { get; set; }        // encrypted
    public string Environment { get; set; } = "sandbox"; // sandbox | live
    public string? BaseUrl { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
}

/// <summary>Maps one outlet to its aggregator store/outlet id (per location).</summary>
public class LocationAggregatorMap
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid LocationId { get; set; }
    public string Aggregator { get; set; } = default!;
    public string? ExternalStoreId { get; set; }
    public string? ApiKeyEnc { get; set; }               // PickMe X-API-KEY (one per outlet), encrypted
    public DateTime? LastPolledAt { get; set; }           // joblist poll cursor (observability)
    public bool IsEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
