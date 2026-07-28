namespace Hms.Api.Domain;

/// <summary>
/// An outbound status callback to a delivery aggregator (accept / status / cancel).
/// In dev a mock processor marks these sent; in prod a Hangfire worker POSTs them
/// to the aggregator with retry. The outbox pattern guarantees we never lose an
/// "order accepted" callback even if the aggregator is briefly unreachable.
/// </summary>
public class AggregatorOutbox
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Aggregator { get; set; } = default!;     // ubereats | pickme
    public string ExternalOrderId { get; set; } = default!;
    public string Operation { get; set; } = default!;      // accept | status | cancel
    public string PayloadJson { get; set; } = "{}";
    public string Status { get; set; } = "pending";        // pending | sent | failed
    public int Attempts { get; set; }
    public string? LastError { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? SentAt { get; set; }
}
