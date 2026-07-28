using Hms.Api.Domain;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.Tenants;

public static class TenantsEndpoint
{
    public static IEndpointRouteBuilder MapTenantEndpoints(this IEndpointRouteBuilder app)
    {
        // Signup + slug lookup are pre-auth (login/self-service). The full List is
        // a cross-tenant control-plane read → PlatformAdmin only.
        var g = app.MapGroup("/api/v1/tenants").WithTags("Tenants");

        // List (control plane — PlatformAdmin allowlist, see Program.cs)
        g.MapGet("", async (ControlDbContext db, CancellationToken ct) =>
        {
            var list = await db.Tenants
                .AsNoTracking()
                .OrderBy(t => t.CreatedAt)
                .Select(t => new
                {
                    t.Id,
                    t.Slug,
                    t.DisplayName,
                    t.Status,
                    t.Plan,
                    t.CountryCode,
                    t.DefaultCurrency,
                    t.OwnerEmail,
                    t.CreatedAt
                })
                .ToListAsync(ct);
            return Results.Ok(list);
        })
        .WithName("Tenants.List")
        .WithSummary("List all tenants in the control plane (PlatformAdmin only).")
        .RequireAuthorization("PlatformAdmin");

        // Create (signup) → record the tenant, then auto-provision its DB + baseline.
        g.MapPost("", async (CreateTenantRequest req, ControlDbContext db, ProvisioningService provisioner,
            Hms.Api.Features.Billing.SubscriptionService billing, ILogger<Tenant> logger, CancellationToken ct) =>
        {
            var slug = req.Slug.Trim().ToLowerInvariant();
            if (!System.Text.RegularExpressions.Regex.IsMatch(slug, "^[a-z][a-z0-9]{1,39}$"))
                return Results.BadRequest(new { error = "slug must be 2–40 chars, lowercase letters/digits, starting with a letter" });
            if (await db.Tenants.AnyAsync(t => t.Slug == slug, ct))
                return Results.Conflict(new { error = "slug already taken" });

            var tenant = new Tenant
            {
                Slug = slug,
                DisplayName = req.DisplayName,
                DatabaseName = $"hms_tenant_{slug}",
                DatabaseHost = "localhost",          // dev default — provisioning pipeline picks the server
                Status = TenantStatus.Pending,
                Plan = req.Plan ?? "starter",
                OwnerEmail = req.OwnerEmail,
                CountryCode = req.CountryCode ?? "LK",
                DefaultCurrency = req.DefaultCurrency ?? "LKR",
                TimeZone = req.TimeZone ?? "Asia/Colombo",
                TrialEndsAt = DateTime.UtcNow.AddDays(14)
            };

            db.Tenants.Add(tenant);
            await db.SaveChangesAsync(ct);

            // Provision inline (CREATE DB + migrate + seed). On failure the tenant
            // stays Pending so it can be retried; signup itself still succeeds.
            var provisioned = false;
            try { provisioned = await provisioner.ProvisionAsync(tenant.Id, ct); }
            catch (Exception ex) { logger.LogError(ex, "Inline provisioning failed for {Slug}", slug); }

            // Subscribe: create the trialing subscription (plan + chosen add-ons) and project entitlements.
            if (provisioned)
            {
                try { await billing.InitializeAsync(tenant.Id, tenant.Plan, req.Addons, ct); }
                catch (Exception ex) { logger.LogError(ex, "Subscription init failed for {Slug}", slug); }
            }

            return Results.Created($"/api/v1/tenants/{tenant.Id}", new
            {
                tenant.Id, tenant.Slug, tenant.DisplayName, tenant.Status, tenant.Plan, tenant.OwnerEmail,
                provisioned,
            });
        })
        .WithName("Tenants.Create")
        .WithSummary("Create a tenant (signup) and provision its database + baseline.")
        .AllowAnonymous();

        // Re-run provisioning for a Pending tenant (PlatformAdmin retry).
        g.MapPost("/{id:guid}/provision", async (Guid id, ProvisioningService provisioner, CancellationToken ct) =>
        {
            try { return Results.Ok(new { provisioned = await provisioner.ProvisionAsync(id, ct) }); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        })
        .WithName("Tenants.Provision")
        .WithSummary("Retry provisioning for a tenant (PlatformAdmin).")
        .RequireAuthorization("PlatformAdmin");

        // Lookup by slug
        g.MapGet("/by-slug/{slug}", async (string slug, ControlDbContext db, CancellationToken ct) =>
        {
            var t = await db.Tenants.AsNoTracking().FirstOrDefaultAsync(x => x.Slug == slug.ToLowerInvariant(), ct);
            return t is null
                ? Results.NotFound()
                : Results.Ok(new { t.Id, t.Slug, t.DisplayName, t.Status, t.Plan });
        })
        .WithName("Tenants.GetBySlug")
        .AllowAnonymous();

        return app;
    }
}

public record CreateTenantRequest(
    string Slug,
    string DisplayName,
    string? OwnerEmail,
    string? Plan,
    string? CountryCode,
    string? DefaultCurrency,
    string? TimeZone,
    Dictionary<string, int>? Addons = null   // add-on code → quantity, chosen at signup (#109)
);
