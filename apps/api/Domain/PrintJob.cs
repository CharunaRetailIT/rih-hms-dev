namespace Hms.Api.Domain;

/// <summary>A queued print job for the venue Print Agent (#16). KOT chit or bill.</summary>
public class PrintJob : BaseEntity
{
    public Guid LocationId { get; set; }
    public string Kind { get; set; } = "bill";      // kot | bill
    public Guid? OrderId { get; set; }
    public string? PrinterName { get; set; }
    public string Payload { get; set; } = default!;  // receipt/ticket HTML (or ESC/POS)
    public string Status { get; set; } = "queued";   // queued | printed | failed
    public int Attempts { get; set; }
    public string? Error { get; set; }
    public DateTime? PrintedAt { get; set; }
}
