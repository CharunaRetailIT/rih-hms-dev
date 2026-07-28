using System.Security.Claims;
using Hms.Api.Domain;
using Hms.Api.Features.Auth;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.Permissions;

/// <summary>
/// Per-role POS limits (#71). Owner bypasses everything; a role with no stored
/// row falls back to permissive defaults, so enforcement only bites once an owner
/// configures it. The acting role comes from the JWT "role" claim.
/// </summary>
public class PermissionService(ITenantDbContextFactory factory)
{
    public static readonly IReadOnlyDictionary<string, int> RoleIds =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        { ["Owner"] = 0, ["Manager"] = 1, ["Cashier"] = 2, ["Kitchen"] = 3, ["Accountant"] = 4 };

    /// <summary>The caller's role id from the "role" claim (defaults to Cashier=2).</summary>
    public static int RoleIdOf(ClaimsPrincipal user)
    {
        var r = user.FindFirst("role")?.Value ?? user.FindFirst(ClaimTypes.Role)?.Value ?? "Cashier";
        return RoleIds.TryGetValue(r, out var id) ? id : 2;
    }

    public static bool IsOwner(ClaimsPrincipal user) => RoleIdOf(user) == 0;

    /// <summary>Effective limits for a role — the stored row, or permissive defaults.</summary>
    public async Task<RolePermission> GetForRoleAsync(int role, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        return await db.RolePermissions.AsNoTracking().FirstOrDefaultAsync(p => p.Role == role, ct)
            ?? new RolePermission { Role = role };
    }

    /// <summary>The configurable roles (Manager/Cashier/Kitchen/Accountant) merged with defaults.</summary>
    public async Task<List<RolePermission>> ListAsync(CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var stored = await db.RolePermissions.AsNoTracking().ToDictionaryAsync(p => p.Role, ct);
        return new[] { 1, 2, 3, 4 }
            .Select(r => stored.TryGetValue(r, out var p) ? p : new RolePermission { Role = r })
            .ToList();
    }

    // ── Function-level (per-screen) access (#71 follow-on) ──────────────────────
    /// <summary>Screens a role is explicitly denied (absence = visible).</summary>
    public async Task<List<string>> DeniedScreensAsync(int role, CancellationToken ct)
    {
        if (role == 0) return new();   // Owner sees everything
        await using var db = await factory.CreateForCurrentAsync(ct);
        return await db.RoleScreenAccess.AsNoTracking().Where(s => s.Role == role && !s.Allowed)
            .Select(s => s.Screen).ToListAsync(ct);
    }

    /// <summary>The full stored access matrix (role → screen → allowed).</summary>
    public async Task<List<RoleScreenAccess>> ScreenMatrixAsync(CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        return await db.RoleScreenAccess.AsNoTracking().ToListAsync(ct);
    }

    public async Task SetScreenAccessAsync(int role, string screen, bool allowed, CancellationToken ct)
    {
        screen = screen.Trim();
        await using var db = await factory.CreateForCurrentAsync(ct);
        var row = await db.RoleScreenAccess.FirstOrDefaultAsync(s => s.Role == role && s.Screen == screen, ct);
        if (row is null) { row = new RoleScreenAccess { Role = role, Screen = screen }; db.RoleScreenAccess.Add(row); }
        row.Allowed = allowed; row.IsDeleted = false;
        await db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Manager-PIN override (#71b): find an active Owner/Manager whose PIN matches and
    /// whose role actually permits the blocked action. Owner approves anything; a
    /// Manager only if their role's limits cover it. Returns the approver or null.
    /// </summary>
    public async Task<User?> ResolveApproverAsync(string? pin, Func<RolePermission, bool> allows, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(pin)) return null;
        await using var db = await factory.CreateForCurrentAsync(ct);
        var candidates = await db.Users.AsNoTracking()
            .Where(u => u.IsActive && !u.IsDeleted && u.PasscodeHash != null
                     && (u.Role == UserRole.Owner || u.Role == UserRole.Manager))
            .ToListAsync(ct);
        foreach (var u in candidates)
        {
            if (!PinHasher.Verify(pin, u.PasscodeHash)) continue;
            if (u.Role == UserRole.Owner) return u;             // owner overrides anything
            if (allows(await GetForRoleAsync((int)u.Role, ct))) return u;
        }
        return null;
    }

    public async Task<RolePermission> UpsertAsync(RolePermissionInput i, CancellationToken ct)
    {
        if (i.Role is < 1 or > 4) throw new InvalidOperationException("Role must be Manager, Cashier, Kitchen or Accountant");
        await using var db = await factory.CreateForCurrentAsync(ct);
        var p = await db.RolePermissions.FirstOrDefaultAsync(x => x.Role == i.Role, ct);
        if (p is null) { p = new RolePermission { Role = i.Role }; db.RolePermissions.Add(p); }
        p.MaxDiscountPercent = Math.Clamp(i.MaxDiscountPercent, 0, 100);
        p.CanApplyDiscount = i.CanApplyDiscount;
        p.CanVoid = i.CanVoid;
        p.CanComp = i.CanComp;
        await db.SaveChangesAsync(ct);
        return p;
    }
}

public record RolePermissionInput(int Role, decimal MaxDiscountPercent, bool CanApplyDiscount, bool CanVoid, bool CanComp);
