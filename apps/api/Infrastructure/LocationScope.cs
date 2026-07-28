namespace Hms.Api.Infrastructure;

/// <summary>
/// Per-request location scope (#scoping-P2). A non-Owner user pinned to a
/// <c>home_location_id</c> is clamped to that outlet on the SERVER — the P1 UI
/// gating is now backed by real enforcement, so a raw API call can't reach
/// another branch's data.
///
/// Populated by <see cref="TenantMiddleware"/> from the JWT (role + home_location_id
/// claims). Owner, or any user with no home location, is all-access (not pinned).
/// </summary>
public class LocationScope
{
    public Guid? HomeLocationId { get; private set; }

    /// <summary>True when this request must be confined to <see cref="HomeLocationId"/>.</summary>
    public bool IsPinned { get; private set; }

    public void Set(string? role, Guid? homeLocationId)
    {
        HomeLocationId = homeLocationId;
        IsPinned = homeLocationId.HasValue
            && !string.Equals(role, "Owner", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Resolve an optional (query/route) locationId. For a pinned user a null or
    /// matching value collapses to the home location; any other value is forbidden.
    /// For an all-access user the requested value passes through unchanged.
    /// </summary>
    public Guid? Clamp(Guid? requested)
    {
        if (!IsPinned) return requested;
        if (requested is null || requested == HomeLocationId) return HomeLocationId;
        throw new LocationForbiddenException();
    }

    /// <summary>Assert a required (body) locationId is allowed for this user.</summary>
    public void Assert(Guid requested)
    {
        if (IsPinned && requested != HomeLocationId) throw new LocationForbiddenException();
    }
}

/// <summary>Thrown when a pinned user references an outlet other than their own. Mapped to 403 by TenantMiddleware.</summary>
public sealed class LocationForbiddenException() : Exception("You don't have access to that outlet.");
