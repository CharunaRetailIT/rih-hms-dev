using Hms.Api.Domain;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.Pos;

/// <summary>
/// POS-depth master data (#76): tour operators and tender currencies. Lists are
/// readable by any authenticated user (the POS terminal needs them to attach to a
/// bill); create/update/delete are back-office only. (Stewards/servers are not a
/// separate master — they're users flagged is_server; see /api/v1/servers and the
/// Team screen.)
/// </summary>
public static class PosConfigEndpoint
{
    public static IEndpointRouteBuilder MapPosConfigEndpoints(this IEndpointRouteBuilder app)
    {
        // ── Tour operator companies (Tour Agent Company) ───────────────────────
        var tc = app.MapGroup("/api/v1/tour-operator-companies").WithTags("POS Config");

        tc.MapGet("", async (ITenantDbContextFactory f, CancellationToken ct, bool? all) =>
        {
            await using var db = await f.CreateForCurrentAsync(ct);
            var q = db.TourOperatorCompanies.AsNoTracking();
            if (all != true) q = q.Where(x => x.IsActive);
            return Results.Ok(await q.OrderBy(x => x.Name).Select(x => new
            {
                x.Id, x.Code, x.Name, x.Address1, x.Address2, x.CountryCode, x.Mobile, x.Telephone,
                x.FaxNo, x.Email, x.WebAddress, x.ContactPerson, x.CommissionPercent, x.CommissionAmount, x.IsActive,
            }).ToListAsync(ct));
        }).WithName("TourOperatorCompanies.List").RequireAuthorization();

        tc.MapGet("/paged", async (ITenantDbContextFactory f, CancellationToken ct,
            int pageNumber = 1, int pageSize = 10, string? search = null, bool? isActive = null) =>
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            await using var db = await f.CreateForCurrentAsync(ct);
            var query = db.TourOperatorCompanies.AsNoTracking().AsQueryable();

            if (isActive is bool active) query = query.Where(x => x.IsActive == active);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = $"%{search.Trim()}%";
                query = query.Where(x => EF.Functions.ILike(x.Code, s) || EF.Functions.ILike(x.Name, s));
            }

            var totalCount = await query.CountAsync(ct);
            var data = await query.OrderBy(x => x.Name)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(x => new TourOperatorCompanyDto(
                    x.Id, x.Code, x.Name, x.Address1, x.Address2, x.CountryCode, x.Mobile, x.Telephone,
                    x.FaxNo, x.Email, x.WebAddress, x.ContactPerson, x.CommissionPercent, x.CommissionAmount, x.IsActive))
                .ToListAsync(ct);

            return Results.Ok(new PagedTourOperatorCompanyResult(
                data, new PaginationMeta(totalCount, pageNumber, pageSize, (int)Math.Ceiling(totalCount / (double)pageSize))));
        }).WithName("TourOperatorCompanies.ListPaged").RequireAuthorization();

        tc.MapPost("", async (TourOperatorCompanyInput body, ITenantDbContextFactory f, CancellationToken ct) =>
        {
            await using var db = await f.CreateForCurrentAsync(ct);
            try
            {
                var commissionPercent = Math.Clamp(body.CommissionPercent ?? 0m, 0m, 100m);
                var commissionAmount = Math.Max(0, body.CommissionAmount ?? 0m);
                if (commissionPercent > 0 && commissionAmount > 0)
                    return Results.BadRequest(new { error = "Set either a commission percentage or a flat amount, not both." });

                TourOperatorCompany row;
                if (body.Id is Guid id)
                    row = await db.TourOperatorCompanies.FirstOrDefaultAsync(x => x.Id == id, ct)
                          ?? throw new InvalidOperationException("Tour agent company not found");
                else { row = new TourOperatorCompany(); db.TourOperatorCompanies.Add(row); }
                row.Code = (body.Code ?? "").Trim();
                row.Name = (body.Name ?? "").Trim();
                row.Address1 = Trim(body.Address1);
                row.Address2 = Trim(body.Address2);
                row.CountryCode = Trim(body.CountryCode)?.ToUpperInvariant();
                row.Mobile = Trim(body.Mobile);
                row.Telephone = Trim(body.Telephone);
                row.FaxNo = Trim(body.FaxNo);
                row.Email = Trim(body.Email);
                row.WebAddress = Trim(body.WebAddress);
                row.ContactPerson = Trim(body.ContactPerson);
                row.CommissionPercent = commissionPercent;
                row.CommissionAmount = commissionAmount;
                row.IsActive = body.IsActive ?? true;
                if (row.Code == "" || row.Name == "") return Results.BadRequest(new { error = "Code and name are required" });
                await db.SaveChangesAsync(ct);
                return Results.Ok(new
                {
                    row.Id, row.Code, row.Name, row.Address1, row.Address2, row.CountryCode, row.Mobile, row.Telephone,
                    row.FaxNo, row.Email, row.WebAddress, row.ContactPerson, row.CommissionPercent, row.CommissionAmount, row.IsActive,
                });
            }
            catch (DbUpdateException) { return Results.BadRequest(new { error = "A tour agent company with that code already exists" }); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("TourOperatorCompanies.Upsert").RequireAuthorization("BackOffice");

        tc.MapDelete("/{id:guid}", async (Guid id, ITenantDbContextFactory f, CancellationToken ct) =>
        {
            await using var db = await f.CreateForCurrentAsync(ct);
            var row = await db.TourOperatorCompanies.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (row is null) return Results.NotFound();
            if (await db.TourOperators.AnyAsync(x => x.CompanyId == id, ct))
                return Results.BadRequest(new { error = "Cannot remove a company that still has tour operators linked to it." });
            row.IsDeleted = true; await db.SaveChangesAsync(ct);
            return Results.NoContent();
        }).WithName("TourOperatorCompanies.Delete").RequireAuthorization("BackOffice");

        // ── Tour operators (Tour Agent) ─────────────────────────────────────────
        var t = app.MapGroup("/api/v1/tour-operators").WithTags("POS Config");

        t.MapGet("", async (ITenantDbContextFactory f, CancellationToken ct, bool? all) =>
        {
            await using var db = await f.CreateForCurrentAsync(ct);
            var q = db.TourOperators.AsNoTracking();
            if (all != true) q = q.Where(x => x.IsActive);
            return Results.Ok(await q.OrderBy(x => x.Name).Select(x => new
            {
                x.Id, x.Code, x.Name, x.CommissionPercent, x.Kind, x.IsActive,
                x.CompanyId, x.Title, x.Nic, x.Address1, x.Address2, x.Address3, x.CountryCode, x.Mobile, x.Email, x.Amount, x.Remarks,
            }).ToListAsync(ct));
        }).WithName("TourOperators.List").RequireAuthorization();

        t.MapGet("/paged", async (ITenantDbContextFactory f, CancellationToken ct,
            int pageNumber = 1, int pageSize = 10, string? search = null, bool? isActive = null, string? kind = null) =>
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            await using var db = await f.CreateForCurrentAsync(ct);
            var query = db.TourOperators.AsNoTracking().AsQueryable();

            if (isActive is bool active) query = query.Where(x => x.IsActive == active);
            if (!string.IsNullOrWhiteSpace(kind)) query = query.Where(x => x.Kind == kind);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = $"%{search.Trim()}%";
                query = query.Where(x => EF.Functions.ILike(x.Code, s) || EF.Functions.ILike(x.Name, s));
            }

            var totalCount = await query.CountAsync(ct);
            var data = await query.OrderBy(x => x.Name)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(x => new TourOperatorDto(
                    x.Id, x.Code, x.Name, x.CommissionPercent, x.Kind, x.IsActive,
                    x.CompanyId, x.Title, x.Nic, x.Address1, x.Address2, x.Address3, x.CountryCode, x.Mobile, x.Email, x.Amount, x.Remarks))
                .ToListAsync(ct);

            return Results.Ok(new PagedTourOperatorResult(
                data, new PaginationMeta(totalCount, pageNumber, pageSize, (int)Math.Ceiling(totalCount / (double)pageSize))));
        }).WithName("TourOperators.ListPaged").RequireAuthorization();

        t.MapPost("", async (TourOperatorInput body, ITenantDbContextFactory f, CancellationToken ct) =>
        {
            await using var db = await f.CreateForCurrentAsync(ct);
            try
            {
                var commissionPercent = Math.Clamp(body.CommissionPercent ?? 0m, 0m, 100m);
                var amount = Math.Max(0, body.Amount ?? 0m);
                if (commissionPercent > 0 && amount > 0)
                    return Results.BadRequest(new { error = "Set either a commission percentage or a flat amount, not both." });

                TourOperator row;
                if (body.Id is Guid id)
                    row = await db.TourOperators.FirstOrDefaultAsync(x => x.Id == id, ct)
                          ?? throw new InvalidOperationException("Tour operator not found");
                else { row = new TourOperator(); db.TourOperators.Add(row); }
                row.Code = (body.Code ?? "").Trim();
                row.Name = (body.Name ?? "").Trim();
                row.CommissionPercent = commissionPercent;
                row.Kind = body.Kind == "individual" ? "individual" : "company";
                row.IsActive = body.IsActive ?? true;
                row.CompanyId = body.CompanyId;
                row.Title = Trim(body.Title);
                row.Nic = Trim(body.Nic);
                row.Address1 = Trim(body.Address1);
                row.Address2 = Trim(body.Address2);
                row.Address3 = Trim(body.Address3);
                row.CountryCode = Trim(body.CountryCode)?.ToUpperInvariant();
                row.Mobile = Trim(body.Mobile);
                row.Email = Trim(body.Email);
                row.Amount = amount;
                row.Remarks = Trim(body.Remarks);
                if (row.Code == "" || row.Name == "") return Results.BadRequest(new { error = "Code and name are required" });
                if (row.CompanyId is Guid cid && !await db.TourOperatorCompanies.AnyAsync(x => x.Id == cid, ct))
                    return Results.BadRequest(new { error = "Selected tour agent company was not found" });
                await db.SaveChangesAsync(ct);
                return Results.Ok(new
                {
                    row.Id, row.Code, row.Name, row.CommissionPercent, row.Kind, row.IsActive,
                    row.CompanyId, row.Title, row.Nic, row.Address1, row.Address2, row.Address3, row.CountryCode, row.Mobile, row.Email, row.Amount, row.Remarks,
                });
            }
            catch (DbUpdateException) { return Results.BadRequest(new { error = "A tour operator with that code already exists" }); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("TourOperators.Upsert").RequireAuthorization("BackOffice");

        t.MapDelete("/{id:guid}", async (Guid id, ITenantDbContextFactory f, CancellationToken ct) =>
        {
            await using var db = await f.CreateForCurrentAsync(ct);
            var row = await db.TourOperators.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (row is null) return Results.NotFound();
            row.IsDeleted = true; await db.SaveChangesAsync(ct);
            return Results.NoContent();
        }).WithName("TourOperators.Delete").RequireAuthorization("BackOffice");

        // ── Currencies ────────────────────────────────────────────────────────
        var c = app.MapGroup("/api/v1/currencies").WithTags("POS Config");

        c.MapGet("", async (ITenantDbContextFactory f, CancellationToken ct, bool? all) =>
        {
            await using var db = await f.CreateForCurrentAsync(ct);
            var q = db.Currencies.AsNoTracking();
            if (all != true) q = q.Where(x => x.IsActive);
            return Results.Ok(await q.OrderByDescending(x => x.IsBase).ThenBy(x => x.Code)
                .Select(x => new { x.Id, x.Code, x.Name, x.Symbol, x.RateToBase, x.IsBase, x.IsActive }).ToListAsync(ct));
        }).WithName("Currencies.List").RequireAuthorization();

        c.MapPost("", async (CurrencyInput body, ITenantDbContextFactory f, CancellationToken ct) =>
        {
            await using var db = await f.CreateForCurrentAsync(ct);
            try
            {
                Currency row;
                if (body.Id is Guid id)
                    row = await db.Currencies.FirstOrDefaultAsync(x => x.Id == id, ct)
                          ?? throw new InvalidOperationException("Currency not found");
                else { row = new Currency(); db.Currencies.Add(row); }
                row.Code = (body.Code ?? "").Trim().ToUpperInvariant();
                row.Name = (body.Name ?? "").Trim();
                row.Symbol = string.IsNullOrWhiteSpace(body.Symbol) ? null : body.Symbol!.Trim();
                row.IsBase = body.IsBase ?? false;
                row.RateToBase = row.IsBase ? 1m : (body.RateToBase ?? 0m);
                row.IsActive = body.IsActive ?? true;
                if (row.Code.Length is < 2 or > 3 || row.Name == "")
                    return Results.BadRequest(new { error = "A 2–3 letter code and a name are required" });
                if (!row.IsBase && row.RateToBase <= 0)
                    return Results.BadRequest(new { error = "Rate to base must be greater than zero" });
                await db.SaveChangesAsync(ct);
                return Results.Ok(new { row.Id, row.Code, row.Name, row.Symbol, row.RateToBase, row.IsBase, row.IsActive });
            }
            catch (DbUpdateException) { return Results.BadRequest(new { error = "That currency code already exists" }); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Currencies.Upsert").RequireAuthorization("BackOffice");

        c.MapDelete("/{id:guid}", async (Guid id, ITenantDbContextFactory f, CancellationToken ct) =>
        {
            await using var db = await f.CreateForCurrentAsync(ct);
            var row = await db.Currencies.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (row is null) return Results.NotFound();
            if (row.IsBase) return Results.BadRequest(new { error = "Cannot delete the base currency" });
            row.IsDeleted = true; await db.SaveChangesAsync(ct);
            return Results.NoContent();
        }).WithName("Currencies.Delete").RequireAuthorization("BackOffice");

        return app;
    }

    private static string? Trim(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}

public record TourOperatorCompanyInput(
    Guid? Id, string? Code, string? Name, string? Address1, string? Address2, string? Mobile, string? Telephone,
    string? FaxNo, string? Email, string? WebAddress, string? ContactPerson, decimal? CommissionAmount, bool? IsActive,
    decimal? CommissionPercent = null, string? CountryCode = null);

public record TourOperatorInput(
    Guid? Id, string? Code, string? Name, decimal? CommissionPercent, bool? IsActive, string? Kind = null,
    Guid? CompanyId = null, string? Title = null, string? Nic = null, string? Address1 = null, string? Address2 = null,
    string? Address3 = null, string? Mobile = null, string? Email = null, decimal? Amount = null, string? Remarks = null,
    string? CountryCode = null);

public record CurrencyInput(Guid? Id, string? Code, string? Name, string? Symbol, decimal? RateToBase, bool? IsBase, bool? IsActive);
