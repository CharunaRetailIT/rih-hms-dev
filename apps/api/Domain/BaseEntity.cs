namespace Hms.Api.Domain;

/// <summary>
/// Base class for all tenant-scoped entities. Provides the audit columns
/// our v2 spec mandates: uuid v7 ID, timestamps, actor refs, soft delete,
/// and the tenant_id every row carries.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
}

/// <summary>
/// Control-plane (hms_control DB) entities — NOT tenant-scoped.
/// Tenants, subscriptions, billing records live here.
/// </summary>
public abstract class ControlEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
}
