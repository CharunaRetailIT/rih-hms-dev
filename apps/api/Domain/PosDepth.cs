namespace Hms.Api.Domain;

/// <summary>
/// A tour agent company / DMC (e.g. a travel agency) that individual tour operators
/// can belong to. Matches the legacy "Tour Agent Company" master — its own contact
/// details and a default commission amount, separate from an individual agent's cut.
/// </summary>
public class TourOperatorCompany : BaseEntity
{
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Address1 { get; set; }
    public string? Address2 { get; set; }
    public string? CountryCode { get; set; }
    public string? Mobile { get; set; }
    public string? Telephone { get; set; }
    public string? FaxNo { get; set; }
    public string? Email { get; set; }
    public string? WebAddress { get; set; }
    public string? ContactPerson { get; set; }
    public decimal CommissionPercent { get; set; }
    public decimal CommissionAmount { get; set; }   // only one of CommissionPercent / CommissionAmount is set
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// A tour operator / travel agent that brings guests for a commission (#76).
/// At settle the commission is computed off the bill's net (ex-tip) and booked
/// against the operator as a payable for the commission report. An individual
/// agent (Kind = "individual") can optionally belong to a <see cref="TourOperatorCompany"/>.
/// </summary>
public class TourOperator : BaseEntity
{
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public decimal CommissionPercent { get; set; }
    public string Kind { get; set; } = "company";   // company | individual (independent guide/agent)
    public bool IsActive { get; set; } = true;

    public Guid? CompanyId { get; set; }            // individual agent's parent Tour Agent Company, if any
    public string? Title { get; set; }              // Mr / Mrs / ... for an individual agent
    public string? Nic { get; set; }
    public string? Address1 { get; set; }
    public string? Address2 { get; set; }
    public string? Address3 { get; set; }
    public string? CountryCode { get; set; }
    public string? Mobile { get; set; }
    public string? Email { get; set; }
    public decimal Amount { get; set; }             // only one of CommissionPercent / Amount is set
    public string? Remarks { get; set; }
}

/// <summary>
/// An accepted tender currency and its rate to the tenant's base currency (#76).
/// The base currency row has <see cref="IsBase"/> = true and rate 1. A foreign
/// tender's <c>amount × RateToBase</c> is the base-currency value that counts
/// toward settling the bill.
/// </summary>
public class Currency : BaseEntity
{
    public string Code { get; set; } = default!;     // ISO 4217 (LKR, USD …)
    public string Name { get; set; } = default!;
    public string? Symbol { get; set; }
    public decimal RateToBase { get; set; } = 1m;
    public bool IsBase { get; set; }
    public bool IsActive { get; set; } = true;
}
