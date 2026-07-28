using Hms.Api.Domain;
using Hms.Api.Features.Auth;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.Users;

/// <summary>
/// Tenant staff management (Owner-only). Users created here can sign in via the
/// magic link with the role they're given. Guards prevent removing or demoting
/// the last owner so a tenant can't lock itself out.
/// </summary>
public static class UsersEndpoint
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/v1/users").WithTags("Users").RequireAuthorization("Owners");

        g.MapGet("", async (ITenantDbContextFactory f, CancellationToken ct) =>
        {
            await using var db = await f.CreateForCurrentAsync(ct);
            return Results.Ok(await db.Users.AsNoTracking().OrderBy(u => u.DisplayName)
                .Select(u => new { u.Id, u.Email, u.Username, u.DisplayName, Role = (int)u.Role, u.HomeLocationId, u.IsActive, u.IsServer, u.LastLoginAt, u.PhoneE164, hasPin = u.PasscodeHash != null })
                .ToListAsync(ct));
        }).WithName("Users.List");

        g.MapGet("/paged", async (ITenantDbContextFactory f, CancellationToken ct,
            int pageNumber = 1, int pageSize = 10, string? search = null, int? role = null, bool? isActive = null) =>
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            await using var db = await f.CreateForCurrentAsync(ct);
            var query = db.Users.AsNoTracking().AsQueryable();

            if (role is int r) query = query.Where(u => (int)u.Role == r);
            if (isActive is bool active) query = query.Where(u => u.IsActive == active);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = $"%{search.Trim()}%";
                query = query.Where(u =>
                    EF.Functions.ILike(u.DisplayName, s) ||
                    (u.Email != null && EF.Functions.ILike(u.Email, s)) ||
                    (u.Username != null && EF.Functions.ILike(u.Username, s)));
            }

            var totalCount = await query.CountAsync(ct);

            var data = await query
                .OrderBy(u => u.DisplayName)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(u => new UserDto(u.Id, u.Email, u.Username, u.DisplayName, (int)u.Role, u.HomeLocationId,
                    u.IsActive, u.IsServer, u.LastLoginAt, u.PhoneE164, u.PasscodeHash != null))
                .ToListAsync(ct);

            return Results.Ok(new PagedUserResult(
                data,
                new PaginationMeta(totalCount, pageNumber, pageSize, (int)Math.Ceiling(totalCount / (double)pageSize))));
        }).WithName("Users.ListPaged");

        g.MapPost("", async (CreateUserInput i, ITenantDbContextFactory f, System.Security.Claims.ClaimsPrincipal caller, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(i.DisplayName)) return Results.BadRequest(new { error = "Name is required" });
            if (i.Role is < 0 or > 5) return Results.BadRequest(new { error = "Invalid role" });
            if (i.Role == (int)UserRole.Owner && !caller.IsInRole("Owner"))
                return Results.Json(new { error = "Only an owner can create another owner." }, statusCode: StatusCodes.Status403Forbidden);
            var email = string.IsNullOrWhiteSpace(i.Email) ? null : i.Email.Trim().ToLowerInvariant();
            var username = string.IsNullOrWhiteSpace(i.Username) ? null : i.Username.Trim().ToLowerInvariant();
            var hasPin = !string.IsNullOrWhiteSpace(i.Pin);
            // A staff user normally needs a way to sign in (email magic-link, or
            // username+PIN). The exception is a server-only record (#76): a waiter
            // who's only ever attributed to bills and never logs in — name + flag.
            if (email is null && !hasPin && !i.IsServer)
                return Results.BadRequest(new { error = "Give the user an email (magic-link) or a username + PIN to sign in with — or mark them as a server-only record" });
            if (hasPin && !PinHasher.IsValidFormat(i.Pin))
                return Results.BadRequest(new { error = "PIN must be 4–8 digits" });

            await using var db = await f.CreateForCurrentAsync(ct);
            if (email is not null && await db.Users.IgnoreQueryFilters().AnyAsync(u => u.Email == email && u.TenantId == db.TenantId, ct))
                return Results.Conflict(new { error = "A user with that email already exists" });
            if (username is not null && await db.Users.IgnoreQueryFilters().AnyAsync(u => u.Username == username && u.TenantId == db.TenantId, ct))
                return Results.Conflict(new { error = "That username is already taken" });
            // No username given → auto-generate a unique handle from the name (editable later),
            // so PIN sign-in always works. Server-only (never-logs-in) records skip this.
            if (username is null && !i.IsServer)
                username = await GenerateUsernameAsync(db, i.DisplayName, ct);

            // Plan/subscription enforcement (#109): cap login users at the licensed limit (0 = unlimited).
            // Server-only records (bill attribution, no login) don't consume a seat.
            var userLimit = (await db.OrgSettings.AsNoTracking().Select(o => (int?)o.UserLimit).FirstOrDefaultAsync(ct)) ?? 0;
            if (userLimit > 0 && !i.IsServer && await db.Users.CountAsync(x => x.IsActive && !x.IsServer, ct) >= userLimit)
                return Results.Json(new { error = $"Your plan includes {userLimit} user(s). Upgrade your plan to add more." }, statusCode: StatusCodes.Status403Forbidden);

            var u = new User
            {
                Email = email, Username = username, DisplayName = i.DisplayName.Trim(),
                Role = (UserRole)i.Role, PhoneE164 = i.PhoneE164, IsActive = true, IsServer = i.IsServer,
                HomeLocationId = i.HomeLocationId,
                PasscodeHash = hasPin ? PinHasher.Hash(i.Pin!) : null,
            };
            db.Users.Add(u);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/v1/users/{u.Id}", new { u.Id, u.Email, u.Username, u.DisplayName, Role = (int)u.Role, u.IsActive, u.IsServer, hasPin = u.PasscodeHash != null });
        }).WithName("Users.Create").WithSummary("Add a staff user (email and/or username + PIN).");

        // Set or clear a staff login PIN (empty pin clears it). Owner-only.
        g.MapPut("/{id:guid}/pin", async (Guid id, SetPinInput body, ITenantDbContextFactory f, System.Security.Claims.ClaimsPrincipal caller, CancellationToken ct) =>
        {
            await using var db = await f.CreateForCurrentAsync(ct);
            var u = await db.Users.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (u is null) return Results.NotFound();
            if (u.Role == UserRole.Owner && !caller.IsInRole("Owner")) return Results.Json(new { error = "Only an owner can change the owner's PIN." }, statusCode: StatusCodes.Status403Forbidden);
            var pin = body.Pin?.Trim();
            if (string.IsNullOrEmpty(pin)) { u.PasscodeHash = null; }
            else
            {
                if (!PinHasher.IsValidFormat(pin)) return Results.BadRequest(new { error = "PIN must be 4–8 digits" });
                u.PasscodeHash = PinHasher.Hash(pin);
                // A PIN is useless without a username to sign in with — give them one if missing.
                if (string.IsNullOrWhiteSpace(u.Username)) u.Username = await GenerateUsernameAsync(db, u.DisplayName, ct);
            }
            u.PinFailedCount = 0; u.PinLockedUntil = null;
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { u.Id, u.Username, hasPin = u.PasscodeHash != null });
        }).WithName("Users.SetPin").WithSummary("Set or clear a staff PIN for POS login.");

        g.MapPut("/{id:guid}", async (Guid id, UpdateUserInput i, ITenantDbContextFactory f, System.Security.Claims.ClaimsPrincipal caller, CancellationToken ct) =>
        {
            await using var db = await f.CreateForCurrentAsync(ct);
            var u = await db.Users.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (u is null) return Results.NotFound();
            // Owner protection (#admin): an Admin (or anyone non-owner) cannot alter the owner account.
            if (u.Role == UserRole.Owner && !caller.IsInRole("Owner"))
                return Results.Json(new { error = "Only an owner can change the owner account." }, statusCode: StatusCodes.Status403Forbidden);

            if (i.Role is int r)
            {
                if (r is < 0 or > 5) return Results.BadRequest(new { error = "Invalid role" });
                if ((UserRole)r == UserRole.Owner && !caller.IsInRole("Owner"))
                    return Results.Json(new { error = "Only an owner can grant the Owner role." }, statusCode: StatusCodes.Status403Forbidden);
                if (u.Role == UserRole.Owner && (UserRole)r != UserRole.Owner && !await HasAnotherActiveOwner(db, u.Id, ct))
                    return Results.BadRequest(new { error = "Cannot demote the last owner" });
                u.Role = (UserRole)r;
            }
            if (!string.IsNullOrWhiteSpace(i.DisplayName)) u.DisplayName = i.DisplayName.Trim();
            // Edit the sign-in identifiers (unique per tenant). Only when supplied & non-empty so
            // partial payloads (e.g. a role-only change) never wipe them; clearing isn't via this path.
            if (!string.IsNullOrWhiteSpace(i.Email))
            {
                var em = i.Email.Trim().ToLowerInvariant();
                if (!System.Text.RegularExpressions.Regex.IsMatch(em, @"^[^\s@]+@[^\s@]+\.[^\s@]+$"))
                    return Results.BadRequest(new { error = "Enter a valid email address" });
                if (await db.Users.IgnoreQueryFilters().AnyAsync(x => x.Email == em && x.TenantId == db.TenantId && x.Id != id, ct))
                    return Results.Conflict(new { error = "A user with that email already exists" });
                u.Email = em;
            }
            if (!string.IsNullOrWhiteSpace(i.Username))
            {
                var un = i.Username.Trim().ToLowerInvariant();
                if (await db.Users.IgnoreQueryFilters().AnyAsync(x => x.Username == un && x.TenantId == db.TenantId && x.Id != id, ct))
                    return Results.Conflict(new { error = "That username is already taken" });
                u.Username = un;
            }
            if (i.PhoneE164 is not null) u.PhoneE164 = i.PhoneE164;
            if (i.IsServer is bool srv) u.IsServer = srv;
            if (i.IsActive is bool active)
            {
                if (!active && u.Role == UserRole.Owner && !await HasAnotherActiveOwner(db, u.Id, ct))
                    return Results.BadRequest(new { error = "Cannot deactivate the last active owner" });
                u.IsActive = active;
            }
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { u.Id, u.Email, u.Username, u.DisplayName, Role = (int)u.Role, u.HomeLocationId, u.IsActive, u.IsServer });
        }).WithName("Users.Update").WithSummary("Update a user's name, email, username, role, phone, server flag, or active state.");

        // Assign (or clear) a user's home outlet. null ⇒ head-office / all-access.
        // Dedicated endpoint so it's unambiguous vs the partial Update payload.
        g.MapPut("/{id:guid}/home-location", async (Guid id, HomeLocationInput body, ITenantDbContextFactory f, CancellationToken ct) =>
        {
            await using var db = await f.CreateForCurrentAsync(ct);
            var u = await db.Users.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (u is null) return Results.NotFound();
            if (body.LocationId is Guid lid && !await db.Locations.AnyAsync(l => l.Id == lid, ct))
                return Results.BadRequest(new { error = "Unknown outlet" });
            u.HomeLocationId = body.LocationId;
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { u.Id, u.HomeLocationId });
        }).WithName("Users.SetHomeLocation").WithSummary("Pin a user to an outlet (null = head office / all access).");

        // Which floors a steward covers — drives floor-scoped notification routing for
        // guest QR orders. A steward with no floors assigned is treated as "all floors"
        // (same convention as null homeLocationId = all outlets) until this is adopted.
        g.MapGet("/{id:guid}/floors", async (Guid id, ITenantDbContextFactory f, CancellationToken ct) =>
        {
            await using var db = await f.CreateForCurrentAsync(ct);
            var floorIds = await db.UserFloors.AsNoTracking().Where(x => x.UserId == id).Select(x => x.FloorId).ToListAsync(ct);
            return Results.Ok(floorIds);
        }).WithName("Users.GetFloors");

        g.MapPut("/{id:guid}/floors", async (Guid id, FloorIdsInput body, ITenantDbContextFactory f, CancellationToken ct) =>
        {
            await using var db = await f.CreateForCurrentAsync(ct);
            if (!await db.Users.AnyAsync(x => x.Id == id, ct)) return Results.NotFound();
            var existing = await db.UserFloors.Where(x => x.UserId == id).ToListAsync(ct);
            db.UserFloors.RemoveRange(existing);
            foreach (var floorId in body.FloorIds.Distinct())
                db.UserFloors.Add(new UserFloor { UserId = id, FloorId = floorId });
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { userId = id, floorIds = body.FloorIds.Distinct() });
        }).WithName("Users.SetFloors").WithSummary("Replace the set of floors a steward covers.");

        // Servers for the POS "served by" dropdown — readable by any signed-in user
        // (the cashier needs it). Active users flagged is_server. #76
        app.MapGet("/api/v1/servers", async (ITenantDbContextFactory f, CancellationToken ct) =>
        {
            await using var db = await f.CreateForCurrentAsync(ct);
            return Results.Ok(await db.Users.AsNoTracking().Where(u => u.IsServer && u.IsActive)
                .OrderBy(u => u.DisplayName)
                .Select(u => new { u.Id, name = u.DisplayName })
                .ToListAsync(ct));
        }).WithName("Servers.List").WithTags("Users").RequireAuthorization();

        g.MapDelete("/{id:guid}", async (Guid id, ITenantDbContextFactory f, System.Security.Claims.ClaimsPrincipal caller, CancellationToken ct) =>
        {
            await using var db = await f.CreateForCurrentAsync(ct);
            var u = await db.Users.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (u is null) return Results.NotFound();
            // Owner protection (#admin): only an owner can remove an owner.
            if (u.Role == UserRole.Owner && !caller.IsInRole("Owner"))
                return Results.Json(new { error = "Only an owner can remove the owner account." }, statusCode: StatusCodes.Status403Forbidden);
            if (u.Role == UserRole.Owner && !await HasAnotherActiveOwner(db, u.Id, ct))
                return Results.BadRequest(new { error = "Cannot remove the last owner" });
            u.IsDeleted = true; u.IsActive = false;
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        }).WithName("Users.Delete").WithSummary("Deactivate + soft-delete a user.");

        // ── Self-service: a steward manages which floor(s) THEY cover (#floor-push) ──
        // Deliberately outside the Owners-only group above — any signed-in staff member can
        // set their own coverage (no admin action needed); it's a no-op for notification
        // routing unless they're actually a steward (IsServer), so it's harmless either way.
        var me = app.MapGroup("/api/v1/me").WithTags("Users").RequireAuthorization();

        me.MapGet("/floors", async (ITenantDbContextFactory f, System.Security.Claims.ClaimsPrincipal caller, CancellationToken ct) =>
        {
            var userId = CurrentUserId(caller);
            if (userId is null) return Results.Unauthorized();
            await using var db = await f.CreateForCurrentAsync(ct);
            var floorIds = await db.UserFloors.AsNoTracking().Where(x => x.UserId == userId).Select(x => x.FloorId).ToListAsync(ct);
            return Results.Ok(floorIds);
        }).WithName("Me.GetFloors");

        me.MapPut("/floors", async (FloorIdsInput body, ITenantDbContextFactory f, System.Security.Claims.ClaimsPrincipal caller, CancellationToken ct) =>
        {
            var userId = CurrentUserId(caller);
            if (userId is null) return Results.Unauthorized();
            await using var db = await f.CreateForCurrentAsync(ct);
            var existing = await db.UserFloors.Where(x => x.UserId == userId).ToListAsync(ct);
            db.UserFloors.RemoveRange(existing);
            foreach (var floorId in body.FloorIds.Distinct())
                db.UserFloors.Add(new UserFloor { UserId = userId.Value, FloorId = floorId });
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { floorIds = body.FloorIds.Distinct() });
        }).WithName("Me.SetFloors").WithSummary("A steward sets which floors they personally cover.");

        return app;
    }

    private static Guid? CurrentUserId(System.Security.Claims.ClaimsPrincipal user)
    {
        var raw = user.FindFirst("sub")?.Value ?? user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(raw, out var g) ? g : null;
    }

    private static Task<bool> HasAnotherActiveOwner(TenantDbContext db, Guid excludeId, CancellationToken ct) =>
        db.Users.AnyAsync(u => u.Id != excludeId && u.Role == UserRole.Owner && u.IsActive, ct);

    /// Build a unique, space-free username from a display name (e.g. "Stuart 1" → "stuart1",
    /// next collision → "stuart12"). Used when a user is created / PIN-set without one.
    private static async Task<string> GenerateUsernameAsync(TenantDbContext db, string displayName, CancellationToken ct)
    {
        var baseUn = System.Text.RegularExpressions.Regex.Replace((displayName ?? "").ToLowerInvariant(), "[^a-z0-9]", "");
        if (baseUn.Length < 2) baseUn = "user";
        var un = baseUn; var n = 1;
        while (await db.Users.IgnoreQueryFilters().AnyAsync(u => u.Username == un && u.TenantId == db.TenantId, ct))
            un = $"{baseUn}{++n}";
        return un;
    }
}

public record CreateUserInput(string? Email, string DisplayName, int Role, string? PhoneE164, string? Pin = null, string? Username = null, bool IsServer = false, Guid? HomeLocationId = null);
public record UpdateUserInput(string? DisplayName, int? Role, bool? IsActive, string? PhoneE164, bool? IsServer = null, string? Email = null, string? Username = null);
public record SetPinInput(string? Pin);
public record HomeLocationInput(Guid? LocationId);
public record FloorIdsInput(List<Guid> FloorIds);
