namespace Hms.Api.Infrastructure;

/// <summary>
/// Per-request tenant context. Populated by <see cref="TenantMiddleware"/>
/// from the JWT <c>tenant_id</c> claim, or from <c>X-Tenant-Id</c> header
/// in development.
/// </summary>
public interface ITenantContext
{
    Guid? TenantId { get; }
    bool IsSet { get; }
    Guid TenantIdOrThrow();
    void Set(Guid tenantId);

    // Current authenticated user (for the audit log). Populated by TenantMiddleware.
    Guid? UserId { get; }
    string? UserName { get; }
    string? UserRole { get; }
    void SetUser(Guid? userId, string? userName, string? userRole);
}

public class TenantContext : ITenantContext
{
    public Guid? TenantId { get; private set; }
    public bool IsSet => TenantId.HasValue;

    public Guid? UserId { get; private set; }
    public string? UserName { get; private set; }
    public string? UserRole { get; private set; }

    public void SetUser(Guid? userId, string? userName, string? userRole)
    {
        UserId = userId; UserName = userName; UserRole = userRole;
    }

    public void Set(Guid tenantId)
    {
        if (TenantId.HasValue && TenantId.Value != tenantId)
            throw new InvalidOperationException("Tenant context already set to a different tenant.");
        TenantId = tenantId;
    }

    public Guid TenantIdOrThrow() =>
        TenantId ?? throw new InvalidOperationException(
            "No tenant context for this request. Endpoint must either require [Authorize] or accept X-Tenant-Id header in dev.");
}
