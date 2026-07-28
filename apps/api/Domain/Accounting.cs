namespace Hms.Api.Domain;

/// <summary>
/// Chart-of-accounts account (#73). System accounts (is_system) are the ones the
/// posting engine references by well-known code; the rest are user-defined.
/// </summary>
public class GlAccount : BaseEntity
{
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string AccountType { get; set; } = "expense";   // asset | liability | equity | income | expense
    public bool IsSystem { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// A balanced double-entry journal document (#73): Σ debit == Σ credit across its
/// lines. Auto-generated entries carry a source + source_ref (idempotency key);
/// manual entries are typed by the accountant.
/// </summary>
public class GlJournalEntry : BaseEntity
{
    public string EntryNo { get; set; } = default!;
    public DateOnly EntryDate { get; set; }
    public string? Memo { get; set; }
    public string Source { get; set; } = "manual";         // sales | purchase | expense | payment | manual
    public string? SourceRef { get; set; }
    public Guid? LocationId { get; set; }
    public string Status { get; set; } = "posted";         // draft | posted | void
    public DateTime? PostedAt { get; set; }
    public List<GlJournalLine> Lines { get; set; } = new();
}

/// <summary>One debit/credit line of a journal entry. Plain entity (no full audit cols).</summary>
public class GlJournalLine
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid EntryId { get; set; }
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; } = default!;     // snapshot
    public string AccountName { get; set; } = default!;     // snapshot
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string? LineMemo { get; set; }
    public int Sort { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsDeleted { get; set; }
}

/// <summary>Operating / petty-cash expense (#73). Posting it creates a journal (Dr expense, Cr cash/bank).</summary>
public class GlExpense : BaseEntity
{
    public string ExpenseNo { get; set; } = default!;
    public DateOnly ExpenseDate { get; set; }
    public Guid AccountId { get; set; }                    // expense account debited
    public decimal Amount { get; set; }
    public string? Payee { get; set; }
    public Guid PaymentAccountId { get; set; }             // cash/bank credited
    public string? PaymentMethod { get; set; }
    public string? Memo { get; set; }
    public Guid? LocationId { get; set; }
    public Guid? JournalEntryId { get; set; }
}

/// <summary>Payment to a supplier against AP (#73): Dr Accounts Payable, Cr cash/bank.</summary>
public class ApPayment : BaseEntity
{
    public string PaymentNo { get; set; } = default!;
    public Guid SupplierId { get; set; }
    public DateOnly PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public Guid PaymentAccountId { get; set; }
    public string? Reference { get; set; }
    public string? Memo { get; set; }
    public Guid? JournalEntryId { get; set; }
}
