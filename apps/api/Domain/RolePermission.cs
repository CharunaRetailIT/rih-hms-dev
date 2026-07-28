namespace Hms.Api.Domain;

/// <summary>
/// Per-role POS limits (#71). Owner (role 0) bypasses; a role with no row uses
/// permissive defaults so behaviour is unchanged until an owner tightens it.
/// </summary>
public class RolePermission : BaseEntity
{
    public int Role { get; set; }
    public decimal MaxDiscountPercent { get; set; } = 100m;
    public bool CanApplyDiscount { get; set; } = true;
    public bool CanVoid { get; set; } = true;
    public bool CanComp { get; set; } = true;   // comp / no-sale drawer open
}

/// <summary>Per-role screen/function visibility (#71). Absence = visible by default.</summary>
public class RoleScreenAccess : BaseEntity
{
    public int Role { get; set; }
    public string Screen { get; set; } = default!;   // nav key, e.g. "/reports"
    public bool Allowed { get; set; } = true;
}
