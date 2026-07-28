namespace Hms.Api.Domain;

/// <summary>
/// Configurable approval workflow (#approvals). A tenant defines RULES per document type
/// (purchase_order | grn | stock_transfer), optionally gated by a value threshold / outlet,
/// each with one or more ordered LEVELS (steps). When a matching document is submitted, a
/// REQUEST is created and blocks the document until every level approves. Each level produces
/// an ACTION carrying a single-use, time-limited token so the approver can act from an email
/// link WITHOUT logging in (Approve / Reject / Hold, remark mandatory for Reject & Hold).
/// </summary>
public class ApprovalRule : BaseEntity
{
    public string DocType { get; set; } = default!;       // purchase_order | grn | stock_transfer
    public string Name { get; set; } = default!;          // admin label, e.g. "PO over LKR 50k"
    public decimal MinAmount { get; set; }                // 0 = always; else only when the doc total >= this
    public Guid? LocationId { get; set; }                 // scope to one outlet; null = all outlets
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public List<ApprovalRuleStep> Steps { get; set; } = new();
}

/// <summary>One approval level within a rule. The approver is a named USER, a ROLE/group, or an
/// external EMAIL (someone with no system login who only ever uses the link).</summary>
public class ApprovalRuleStep : BaseEntity
{
    public Guid RuleId { get; set; }
    public int Level { get; set; }                        // 1, 2, 3 … (ascending)
    public string ApproverType { get; set; } = "role";    // user | role | email
    public Guid? ApproverUserId { get; set; }             // when type = user
    public int? ApproverRole { get; set; }                // UserRole value when type = role
    public string? ApproverEmail { get; set; }            // when type = email (no login)
    public string ApproverLabel { get; set; } = default!; // display, e.g. "Finance Manager" / "director@x.com"
}

/// <summary>A live approval for one document instance (the PO / GRN / transfer being approved).</summary>
public class ApprovalRequest : BaseEntity
{
    public string DocType { get; set; } = default!;
    public Guid DocId { get; set; }                       // the source PO / GRN / transfer id
    public string DocNumber { get; set; } = default!;     // display number, e.g. "PO-0006"
    public string DocSummary { get; set; } = "{}";        // JSON snapshot (supplier, lines, totals) for the link page
    public decimal Amount { get; set; }
    public Guid RuleId { get; set; }
    public string Status { get; set; } = "pending";       // pending | approved | rejected | on_hold
    public int CurrentLevel { get; set; } = 1;
    public Guid? RequestedBy { get; set; }
    public string? RequestedByName { get; set; }
    public DateTime? DecidedAt { get; set; }
    public List<ApprovalAction> Actions { get; set; } = new();
}

/// <summary>One approver's action at a level — created pending, carries the email-link token, and
/// records the decision + (mandatory-on-reject/hold) remark.</summary>
public class ApprovalAction : BaseEntity
{
    public Guid RequestId { get; set; }
    public int Level { get; set; }
    public string ApproverType { get; set; } = "role";
    public string ApproverLabel { get; set; } = default!;
    public string? ApproverEmail { get; set; }            // where the link was sent (if known)
    public string Token { get; set; } = default!;         // single-use link credential (URL-safe random)
    public DateTime TokenExpiresAt { get; set; }
    public string Status { get; set; } = "pending";       // pending | approved | rejected | on_hold
    public string? Remark { get; set; }
    public string? ActedBy { get; set; }                  // name/email of whoever acted
    public DateTime? ActedAt { get; set; }
}
