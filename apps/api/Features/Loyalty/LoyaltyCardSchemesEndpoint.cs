namespace Hms.Api.Features.Loyalty;

/// <summary>
/// Loyalty Card Schemes (#master-data-loyalty): the "Card Type" master from the old
/// system — Discount / Points / Promotion schemes a customer enrolls into. Lists are
/// readable by any authenticated user (POS + loyalty-customer picker); create/update/
/// delete are back-office only.
/// </summary>
public static class LoyaltyCardSchemesEndpoint
{
    public static IEndpointRouteBuilder MapLoyaltyCardSchemeEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/v1/loyalty/card-schemes").WithTags("Loyalty");

        g.MapGet("", async (LoyaltyCardSchemeService svc, CancellationToken ct, bool? all) =>
            Results.Ok(await svc.ListAsync(all == true, ct))
        ).RequireAuthorization("Operations").WithName("Loyalty.CardSchemes.List");

        g.MapGet("/paged", async (
            LoyaltyCardSchemeService svc, CancellationToken ct,
            int pageNumber = 1, int pageSize = 10, string? search = null, string? type = null, bool? isActive = null) =>
            Results.Ok(await svc.GetPagedAsync(pageNumber, pageSize, search, type, isActive, ct))
        ).RequireAuthorization("Operations").WithName("Loyalty.CardSchemes.Paged");

        g.MapPost("", async (LoyaltyCardSchemeInput body, LoyaltyCardSchemeService svc, CancellationToken ct) =>
        {
            try { return Results.Ok(await svc.UpsertAsync(body, ct)); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).RequireAuthorization("BackOffice").WithName("Loyalty.CardSchemes.Upsert");

        g.MapDelete("/{id:guid}", async (Guid id, LoyaltyCardSchemeService svc, CancellationToken ct) =>
        {
            try { await svc.DeleteAsync(id, ct); return Results.NoContent(); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).RequireAuthorization("BackOffice").WithName("Loyalty.CardSchemes.Delete");

        return app;
    }
}
