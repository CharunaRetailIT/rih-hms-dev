using Hms.Api.Domain;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Hms.Api.Features.Billing;

/// <summary>
/// SaaS commercial backbone — Phase A: the pricing catalog (#109). RIT (platform admin)
/// defines plans + add-on prices; the catalog is publicly readable so signup and the
/// owner's self-serve "buy" screens can show pricing. Subscriptions + entitlement
/// projection + enforcement come in Phase B.
/// </summary>
public static class BillingEndpoint
{
    public static IEndpointRouteBuilder MapBillingEndpoints(this IEndpointRouteBuilder app)
    {
        // ── Public catalog (signup + owner self-serve read it) ──
        // Pass ?country=XX to get the RIT billing taxes that apply to that country (SL VAT for
        // domestic, nothing for export). The UI shows the tax line on its price estimate.
        app.MapGet("/api/v1/billing/catalog", async (ControlDbContext db, SubscriptionService svc, string? country,
            IOptions<PayHereOptions> ph, IOptions<BillingOptions> bo, CancellationToken ct) =>
        {
            var plans = await db.Plans.AsNoTracking().Where(p => p.IsActive && !p.IsDeleted)
                .OrderBy(p => p.SortOrder)
                .Select(p => new { p.Code, p.Name, p.MonthlyPrice, p.Currency, p.IncludedLocations, p.IncludedUsers, p.MaxLocations, p.Features })
                .ToListAsync(ct);
            var addons = await db.Addons.AsNoTracking().Where(a => a.IsActive && !a.IsDeleted)
                .OrderBy(a => a.Name)
                .Select(a => new { a.Code, a.Name, a.Unit, a.UnitPrice, a.Currency })
                .ToListAsync(ct);
            var taxes = (await svc.ApplicableTaxesAsync(country, ct)).Select(t => new { t.Code, t.Name, t.RatePercent });
            // Drives the signup card step: capture a card up-front only if RIT's toggle is on AND the gateway is live.
            var toggle = await db.PlatformSettings.AsNoTracking().FirstOrDefaultAsync(s => s.Key == "require_card_at_signup", ct);
            var requireToggle = toggle is null ? bo.Value.RequireCardAtSignup : toggle.Value != "false";
            var requireCardAtSignup = requireToggle && ph.Value.IsFullyConfigured;
            return Results.Ok(new { plans, addons, taxes, gatewayReady = ph.Value.IsFullyConfigured, requireCardAtSignup, trialDays = SubscriptionService.TrialDays });
        }).WithName("Billing.Catalog").WithTags("Billing").AllowAnonymous();

        // ── RIT platform-admin: manage the catalog ──
        var admin = app.MapGroup("/api/v1/admin/billing").WithTags("Platform admin").RequireAuthorization("PlatformAdmin");

        admin.MapGet("/plans", async (ControlDbContext db, CancellationToken ct) =>
            Results.Ok(await db.Plans.AsNoTracking().Where(p => !p.IsDeleted).OrderBy(p => p.SortOrder)
                .Select(p => new { p.Id, p.Code, p.Name, p.MonthlyPrice, p.Currency, p.IncludedLocations, p.IncludedUsers, p.MaxLocations, p.SortOrder, p.IsActive }).ToListAsync(ct)))
            .WithName("Admin.Plans.List");

        admin.MapPut("/plans", async (PlanInput i, ControlDbContext db, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(i.Code) || string.IsNullOrWhiteSpace(i.Name))
                return Results.BadRequest(new { error = "Code and name are required" });
            var code = i.Code.Trim().ToLowerInvariant();
            var p = await db.Plans.FirstOrDefaultAsync(x => x.Code == code, ct);
            if (p is null) { p = new Plan { Code = code }; db.Plans.Add(p); }
            p.Name = i.Name.Trim();
            p.MonthlyPrice = Math.Max(0, i.MonthlyPrice);
            if (!string.IsNullOrWhiteSpace(i.Currency)) p.Currency = i.Currency.Trim().ToUpperInvariant();
            p.IncludedLocations = Math.Max(0, i.IncludedLocations);
            p.IncludedUsers = Math.Max(0, i.IncludedUsers);
            p.MaxLocations = Math.Max(0, i.MaxLocations);
            p.SortOrder = i.SortOrder;
            p.IsActive = i.IsActive;
            if (i.Features is not null) p.Features = i.Features.ToList();
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { p.Id, p.Code, p.Name, p.MonthlyPrice, p.Currency, p.IncludedLocations, p.IncludedUsers, p.SortOrder, p.IsActive });
        }).WithName("Admin.Plans.Upsert");

        admin.MapGet("/addons", async (ControlDbContext db, CancellationToken ct) =>
            Results.Ok(await db.Addons.AsNoTracking().Where(a => !a.IsDeleted).OrderBy(a => a.Name)
                .Select(a => new { a.Id, a.Code, a.Name, a.Unit, a.UnitPrice, a.Currency, a.IsActive }).ToListAsync(ct)))
            .WithName("Admin.Addons.List");

        admin.MapPut("/addons", async (AddonInput i, ControlDbContext db, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(i.Code) || string.IsNullOrWhiteSpace(i.Name))
                return Results.BadRequest(new { error = "Code and name are required" });
            var code = i.Code.Trim().ToLowerInvariant();
            var a = await db.Addons.FirstOrDefaultAsync(x => x.Code == code, ct);
            if (a is null) { a = new Addon { Code = code }; db.Addons.Add(a); }
            a.Name = i.Name.Trim();
            if (!string.IsNullOrWhiteSpace(i.Unit)) a.Unit = i.Unit.Trim();
            a.UnitPrice = Math.Max(0, i.UnitPrice);
            if (!string.IsNullOrWhiteSpace(i.Currency)) a.Currency = i.Currency.Trim().ToUpperInvariant();
            a.IsActive = i.IsActive;
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { a.Id, a.Code, a.Name, a.Unit, a.UnitPrice, a.Currency, a.IsActive });
        }).WithName("Admin.Addons.Upsert");

        // ── RIT platform-admin: manage the subscription-billing taxes (RIT's own VAT, 1 or many) ──
        admin.MapGet("/taxes", async (ControlDbContext db, CancellationToken ct) =>
            Results.Ok(await db.BillingTaxes.AsNoTracking().Where(t => !t.IsDeleted).OrderBy(t => t.SortOrder)
                .Select(t => new { t.Id, t.Code, t.Name, t.RatePercent, t.Scope, t.SortOrder, t.IsActive }).ToListAsync(ct)))
            .WithName("Admin.Taxes.List");

        admin.MapPut("/taxes", async (BillingTaxInput i, ControlDbContext db, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(i.Code) || string.IsNullOrWhiteSpace(i.Name))
                return Results.BadRequest(new { error = "Code and name are required" });
            var scope = (i.Scope ?? "domestic").Trim().ToLowerInvariant();
            if (scope is not ("domestic" or "export" or "all"))
                return Results.BadRequest(new { error = "Scope must be domestic, export or all" });
            var code = i.Code.Trim().ToLowerInvariant();
            var t = await db.BillingTaxes.FirstOrDefaultAsync(x => x.Code == code, ct);
            if (t is null) { t = new BillingTax { Code = code }; db.BillingTaxes.Add(t); }
            t.Name = i.Name.Trim();
            t.RatePercent = Math.Clamp(i.RatePercent, 0, 100);
            t.Scope = scope;
            t.SortOrder = i.SortOrder;
            t.IsActive = i.IsActive;
            t.IsDeleted = false;
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { t.Id, t.Code, t.Name, t.RatePercent, t.Scope, t.SortOrder, t.IsActive });
        }).WithName("Admin.Taxes.Upsert");

        admin.MapDelete("/taxes/{code}", async (string code, ControlDbContext db, CancellationToken ct) =>
        {
            var t = await db.BillingTaxes.FirstOrDefaultAsync(x => x.Code == code.ToLowerInvariant(), ct);
            if (t is null) return Results.NotFound();
            t.IsDeleted = true; t.IsActive = false;
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        }).WithName("Admin.Taxes.Delete");

        // ── RIT platform-admin: platform settings (e.g. card-required-at-signup toggle, A/B without redeploy) ──
        admin.MapGet("/settings", async (ControlDbContext db, IOptions<BillingOptions> bo, CancellationToken ct) =>
        {
            var t = await db.PlatformSettings.AsNoTracking().FirstOrDefaultAsync(x => x.Key == "require_card_at_signup", ct);
            return Results.Ok(new { requireCardAtSignup = t is null ? bo.Value.RequireCardAtSignup : t.Value != "false" });
        }).WithName("Admin.PlatformSettings.Get");

        admin.MapPut("/settings", async (PlatformSettingsInput i, ControlDbContext db, CancellationToken ct) =>
        {
            const string key = "require_card_at_signup";
            var t = await db.PlatformSettings.FirstOrDefaultAsync(x => x.Key == key, ct);
            if (t is null) { t = new PlatformSetting { Key = key }; db.PlatformSettings.Add(t); }
            t.Value = i.RequireCardAtSignup ? "true" : "false";
            t.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { requireCardAtSignup = i.RequireCardAtSignup });
        }).WithName("Admin.PlatformSettings.Set");

        // ── Tenant subscription (owner self-serve) ──
        var sub = app.MapGroup("/api/v1/billing/subscription").WithTags("Billing");

        sub.MapGet("", async (ITenantContext tc, SubscriptionService svc, CancellationToken ct) =>
            Results.Ok(await svc.GetViewAsync(tc.TenantIdOrThrow(), ct)))
            .WithName("Billing.Subscription.Get").RequireAuthorization("BackOffice");

        // Owner buys/changes plan + add-on quantities on the go (self-serve). Charges via the payment seam, then projects.
        sub.MapPut("", async (SetSubscriptionInput i, ITenantContext tc, SubscriptionService svc, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(i.Plan)) return Results.BadRequest(new { error = "A plan is required" });
            try
            {
                await svc.SetAsync(tc.TenantIdOrThrow(), i.Plan.Trim().ToLowerInvariant(), i.Addons ?? new Dictionary<string, int>(), ct);
                return Results.Ok(await svc.GetViewAsync(tc.TenantIdOrThrow(), ct));
            }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Billing.Subscription.Set").RequireAuthorization("Owners");

        // ── PayHere: capture a card (preapproval) + receive the tokenization result ──
        // The owner adds/updates the card that recurring + mid-cycle charges will bill.
        // We build the signed preapproval form server-side (merchant_secret never leaves the server);
        // the browser auto-submits it to PayHere; PayHere tokenizes and calls our notify_url.
        app.MapPost("/api/v1/billing/payhere/preapproval", async (PreapprovalInput i, ITenantContext tc, SubscriptionService svc,
            IOptions<PayHereOptions> opt, CancellationToken ct) =>
        {
            var o = opt.Value;
            if (!o.HasCheckoutCreds)
                return Results.Json(new { error = "PayHere checkout is not configured yet (missing Merchant ID/Secret)." }, statusCode: 503);
            var tenantId = tc.TenantIdOrThrow();
            await svc.EnsureAsync(tenantId, ct);   // guarantee a subscription row exists to attach the token to
            return Results.Ok(BuildPreapproval(o, tenantId, i));
        }).WithName("Billing.PayHere.Preapproval").WithTags("Billing").RequireAuthorization("Owners");

        // Anonymous variant used during signup (no JWT yet): capture a card for the just-created tenant.
        // Safe-gated: only a trialing subscription with NO card yet can be targeted.
        app.MapPost("/api/v1/billing/payhere/preapproval/signup", async (SignupPreapprovalInput i, ControlDbContext db,
            IOptions<PayHereOptions> opt, CancellationToken ct) =>
        {
            var o = opt.Value;
            if (!o.HasCheckoutCreds) return Results.Json(new { error = "PayHere not configured." }, statusCode: 503);
            if (!Guid.TryParse(i.TenantId, out var tenantId)) return Results.BadRequest(new { error = "tenantId required" });
            var sub = await db.Subscriptions.AsNoTracking().FirstOrDefaultAsync(s => s.TenantId == tenantId && !s.IsDeleted, ct);
            if (sub is null || sub.Status != "trialing" || !string.IsNullOrWhiteSpace(sub.CustomerToken))
                return Results.BadRequest(new { error = "Not eligible for signup card capture" });
            return Results.Ok(BuildPreapproval(o, tenantId, new PreapprovalInput(i.FirstName, i.LastName, i.Email, i.Phone, null, null, null)));
        }).WithName("Billing.PayHere.Preapproval.Signup").WithTags("Billing").AllowAnonymous();

        // Lightweight trial status for the in-app banner (any authenticated tenant user).
        app.MapGet("/api/v1/billing/trial", async (ITenantContext tc, SubscriptionService svc, CancellationToken ct) =>
        {
            var sub = await svc.EnsureAsync(tc.TenantIdOrThrow(), ct);
            int? days = sub.Status == "trialing" && sub.CurrentPeriodEnd is DateTime te
                ? Math.Max(0, (int)Math.Ceiling((te - DateTime.UtcNow).TotalDays)) : null;
            return Results.Ok(new { sub.Status, trialEndsAt = sub.CurrentPeriodEnd, daysRemaining = days });
        }).WithName("Billing.Trial").WithTags("Billing").RequireAuthorization();

        // Server-to-server callback from PayHere (no user session). Verify the signature, then
        // store the customer_token on the tenant's subscription. Must be anonymous + tolerant.
        app.MapPost("/api/v1/billing/payhere/notify", async (HttpRequest http, ControlDbContext db,
            IOptions<PayHereOptions> opt, ILoggerFactory lf, CancellationToken ct) =>
        {
            var log = lf.CreateLogger("PayHere.Notify");
            if (!http.HasFormContentType) return Results.Ok();
            var f = await http.ReadFormAsync(ct);
            string F(string k) => f.TryGetValue(k, out var v) ? v.ToString() : string.Empty;

            var o = opt.Value;
            var orderId = F("order_id");
            if (!o.VerifyNotifySig(orderId, F("payhere_amount"), F("payhere_currency"), F("status_code"), F("md5sig")))
            {
                log.LogWarning("PayHere notify signature mismatch for order {Order}", orderId);
                return Results.Ok();   // ack so PayHere stops retrying, but do nothing
            }
            // status_code 2 = success. Only act on a successful tokenization that returned a token.
            var token = F("customer_token");
            if (F("status_code") != "2" || string.IsNullOrWhiteSpace(token)) return Results.Ok();

            if (!Guid.TryParse(F("custom_1"), out var tenantId))
            {
                log.LogWarning("PayHere notify missing/invalid tenant in custom_1 for order {Order}", orderId);
                return Results.Ok();
            }
            var s = await db.Subscriptions.FirstOrDefaultAsync(x => x.TenantId == tenantId && !x.IsDeleted, ct);
            if (s is null) return Results.Ok();
            s.CustomerToken = token;
            s.CardBrand = string.IsNullOrWhiteSpace(F("method")) ? F("card_holder_name") : F("method");
            var cardNo = F("card_no");
            s.CardLast4 = cardNo.Length >= 4 ? cardNo[^4..] : null;
            s.PaymentMethodUpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
            log.LogInformation("PayHere card saved for tenant {Tenant} (****{Last4})", tenantId, s.CardLast4);
            return Results.Ok();
        }).WithName("Billing.PayHere.Notify").WithTags("Billing").AllowAnonymous();

        // ── RIT platform-admin: sync/backfill a tenant's entitlements + list all subscriptions ──
        app.MapPost("/api/v1/admin/tenants/{tenantId:guid}/subscription/sync", async (Guid tenantId, SubscriptionService svc, CancellationToken ct) =>
        {
            await svc.EnsureAsync(tenantId, ct);
            await svc.ProjectAsync(tenantId, ct);
            return Results.Ok(await svc.GetViewAsync(tenantId, ct));
        }).WithName("Admin.Subscription.Sync").WithTags("Platform admin").RequireAuthorization("PlatformAdmin");

        app.MapGet("/api/v1/admin/subscriptions", async (ControlDbContext db, CancellationToken ct) =>
        {
            var subs = await db.Subscriptions.AsNoTracking().Where(s => !s.IsDeleted)
                .Select(s => new { s.TenantId, s.Plan, s.Status, s.Provider, s.CurrentPeriodEnd }).ToListAsync(ct);
            return Results.Ok(subs);
        }).WithName("Admin.Subscriptions.List").WithTags("Platform admin").RequireAuthorization("PlatformAdmin");

        return app;
    }

    private static string? Clean(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    /// <summary>Build the signed PayHere preapproval payload (form fields + sandbox flag). custom_1 carries
    /// the tenant id so the notify webhook maps the returned token to the right tenant.</summary>
    private static object BuildPreapproval(PayHereOptions o, Guid tenantId, PreapprovalInput i)
    {
        const string currency = "LKR";   // nominal tokenization context (no charge)
        const decimal amount = 10.00m;   // PayHere default preapproval amount (LKR); enters the hash only
        var orderId = $"PA-{tenantId.ToString("N")[..8]}-{DateTime.UtcNow.Ticks}";
        var domain = string.IsNullOrWhiteSpace(o.AllowedDomain) ? "hms.retailit.lk" : o.AllowedDomain;
        var fields = new Dictionary<string, string>
        {
            ["merchant_id"] = o.MerchantId!,
            ["return_url"] = $"https://{domain}/settings?billing=card_added",
            ["cancel_url"] = $"https://{domain}/settings?billing=card_cancelled",
            ["notify_url"] = $"https://{domain}/api/v1/billing/payhere/notify",
            ["order_id"] = orderId,
            ["items"] = "RIT HMS — save card for subscription billing",
            ["currency"] = currency,
            ["amount"] = amount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
            ["first_name"] = Clean(i.FirstName) ?? "Account",
            ["last_name"] = Clean(i.LastName) ?? "Owner",
            ["email"] = Clean(i.Email) ?? "owner@" + domain,
            ["phone"] = Clean(i.Phone) ?? "0000000000",
            ["address"] = Clean(i.Address) ?? "N/A",
            ["city"] = Clean(i.City) ?? "Colombo",
            ["country"] = Clean(i.Country) ?? "Sri Lanka",
            ["custom_1"] = tenantId.ToString(),
            ["hash"] = o.CheckoutHash(orderId, amount, currency),
        };
        // actionUrl = redirect fallback; sandbox flag drives the on-domain JS SDK popup (payhere.startPayment).
        return new { actionUrl = $"{o.ApiBase}/pay/preapprove", fields, sandbox = !o.IsLive };
    }
}

public record PreapprovalInput(string? FirstName, string? LastName, string? Email, string? Phone,
    string? Address, string? City, string? Country);
public record SignupPreapprovalInput(string TenantId, string? FirstName, string? LastName, string? Email, string? Phone);

public record SetSubscriptionInput(string Plan, Dictionary<string, int>? Addons);

public record PlanInput(string Code, string Name, decimal MonthlyPrice, string? Currency,
    int IncludedLocations, int IncludedUsers, int MaxLocations = 0, int SortOrder = 0, bool IsActive = true, string[]? Features = null);
public record AddonInput(string Code, string Name, string Unit, decimal UnitPrice, string? Currency, bool IsActive = true);
public record BillingTaxInput(string Code, string Name, decimal RatePercent, string? Scope, int SortOrder = 0, bool IsActive = true);
public record PlatformSettingsInput(bool RequireCardAtSignup);
