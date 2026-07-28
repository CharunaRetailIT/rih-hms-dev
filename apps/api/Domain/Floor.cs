namespace Hms.Api.Domain;

/// <summary>
/// A physical floor/section of an outlet (e.g. Ground Floor, Rooftop, Garden). Tables are
/// optionally tagged with a floor; stewards are optionally assigned to floors via
/// <see cref="UserFloor"/> so floor-scoped notifications (guest QR orders) reach the
/// right waiter instead of the whole tenant.
/// </summary>
public class Floor : BaseEntity
{
    public Guid LocationId { get; set; }
    public string Name { get; set; } = default!;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Assigns a steward (User) to a floor they cover. Many-to-many: a floor can have several
/// stewards, and a steward can cover more than one floor.
/// </summary>
public class UserFloor : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid FloorId { get; set; }
}
