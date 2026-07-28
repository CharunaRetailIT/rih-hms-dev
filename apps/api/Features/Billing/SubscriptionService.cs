using Hms.Api.Domain;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Hms.Api.Features.Billing;

/// <summary>
/// Subscription engine (#109 Phase B). Creates/maintains a tenant's control-plane
/// subscription, and PROJECTS its plan + add-on quantities into the tenant's org_settings
/// as enforced limits. Everything is catalog-driven — plan limits/prices are read from
/// control.plans/addons (RIT-editable), never hardcoded. The only intrinsic knowledge is
/// what each add-on CODE means (tab_device → seats, guest_qr → flag, extra_location → +outlets).
/// </summary>
public class SubscriptionService(ControlDbContext control, ITenantDbContextFactory factory, IPaymentProvider payment, IOptions<PayHereOptions> payHere, IOptions<BillingOptions> billing)
{
    public const int TrialDays = 14;   // free-trial length (matches the signup messaging)

    // Well-known add-on codes whose effect the runtime must understand (behaviour, not pricing).
    public const string AddonTabDevice = "tab_device";
    public const string AddonGuestQr = "guest_qr";
    public const string AddonExtraLocation = "extra_location";
    public const string AddonEreceiptEmail = "ereceipt_email";   // e-receipts: email only (#79)
    public const string AddonEreceiptAll = "ereceipt_all";       // e-receipts: email + SMS + WhatsApp (#79)

    /// <summary>RIT's own billing taxes that apply to a tenant in <paramref name="countryCode"/>:
    /// domestic taxes when the tenant is in RIT's home country, export taxes when abroad, plus
    /// any 'all'-scoped tax. (SL VAT is domestic-only → a foreign tenant is zero-rated.)</summary>
    public async Task<List<BillingTax>> ApplicableTaxesAsync(string? countryCode, CancellationToken ct)
    {
        var home = billing.Value.HomeCountry;
        var isDomestic = string.Equals(countryCode ?? home, home, StringComparison.OrdinalIgnoreCase);
        var all = await control.BillingTaxes.AsNoTracking().Where(t => t.IsActive && !t.IsDeleted)
            .OrderBy(t => t.SortOrder).ToListAsync(ct);
        return all.Where(t => t.Scope == "all" || (t.Scope == "domestic" && isDomestic) || (t.Scope == "export" && !isDomestic)).ToList();
    }

    private async Task<string?> TenantCountryAsync(Guid tenantId, CancellationToken ct)
        => (await control.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tenantId, ct))?.CountryCode;

    /// <summary>Get the tenant's subscription, creating a trialing one (from the tenant's plan, or the
    /// lowest-sort active plan in the catalog) if none exists. Never hardcodes a plan code.</summary>
    public async Task<Subscription> EnsureAsync(Guid tenantId, CancellationToken ct)
    {
        var sub = await control.Subscriptions.FirstOrDefaultAsync(s => s.TenantId == tenantId && !s.IsDeleted, ct);
        if (sub is not null) return sub;

        var tenant = await control.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tenantId, ct)
            ?? throw new InvalidOperationException("Tenant not found");
        var plan = await control.Plans.FirstOrDefaultAsync(p => p.Code == tenant.Plan && p.IsActive && !p.IsDeleted, ct)
            ?? await control.Plans.Where(p => p.IsActive && !p.IsDeleted).OrderBy(p => p.SortOrder).FirstOrDefaultAsync(ct);

        sub = new Subscription
        {
            TenantId = tenantId, Provider = "manual", Plan = plan?.Code ?? tenant.Plan,
            Status = "trialing", CurrentPeriodStart = DateTime.UtcNow, CurrentPeriodEnd = DateTime.UtcNow.AddDays(TrialDays),
        };
        control.Subscriptions.Add(sub);
        await control.SaveChangesAsync(ct);
        if (plan is not null)
        {
            control.SubscriptionItems.Add(new SubscriptionItem { SubscriptionId = sub.Id, ItemType = "plan", ItemCode = plan.Code, Quantity = 1, UnitPrice = plan.MonthlyPrice, Currency = plan.Currency });
            await control.SaveChangesAsync(ct);
        }
        return sub;
    }

    /// <summary>Create the initial trialing subscription at signup (plan + chosen add-ons) and project
    /// entitlements. No charge — it's a trial. Idempotent: re-projects if a subscription already exists.</summary>
    public async Task InitializeAsync(Guid tenantId, string? requestedPlan, IDictionary<string, int>? addons, CancellationToken ct)
    {
        if (await control.Subscriptions.AnyAsync(s => s.TenantId == tenantId && !s.IsDeleted, ct))
        {
            await ProjectAsync(tenantId, ct);
            return;
        }
        var plan = await control.Plans.FirstOrDefaultAsync(p => p.Code == requestedPlan && p.IsActive && !p.IsDeleted, ct)
            ?? await control.Plans.Where(p => p.IsActive && !p.IsDeleted).OrderBy(p => p.SortOrder).FirstOrDefaultAsync(ct);

        var sub = new Subscription
        {
            TenantId = tenantId, Provider = "manual", Plan = plan?.Code ?? requestedPlan ?? "lite",
            Status = "trialing", CurrentPeriodStart = DateTime.UtcNow, CurrentPeriodEnd = DateTime.UtcNow.AddDays(TrialDays),
        };
        control.Subscriptions.Add(sub);
        await control.SaveChangesAsync(ct);

        if (plan is not null)
            control.SubscriptionItems.Add(new SubscriptionItem { SubscriptionId = sub.Id, ItemType = "plan", ItemCode = plan.Code, Quantity = 1, UnitPrice = plan.MonthlyPrice, Currency = plan.Currency });
        if (addons is { Count: > 0 })
        {
            var catalog = (await control.Addons.Where(a => a.IsActive && !a.IsDeleted).ToListAsync(ct)).ToDictionary(a => a.Code);
            foreach (var (code, qty) in addons)
            {
                if (qty <= 0 || !catalog.TryGetValue(code, out var a)) continue;
                control.SubscriptionItems.Add(new SubscriptionItem { SubscriptionId = sub.Id, ItemType = "addon", ItemCode = code, Quantity = qty, UnitPrice = a.UnitPrice, Currency = a.Currency });
            }
        }
        await control.SaveChangesAsync(ct);
        await ProjectAsync(tenantId, ct);
    }

    /// <summary>Set the plan + add-on quantities (owner self-serve or RIT). Validates against the live
    /// catalog, charges via the payment seam, snapshots prices into line-items, then re-projects.</summary>
    public async Task<Subscription> SetAsync(Guid tenantId, string planCode, IDictionary<string, int> addonQty, CancellationToken ct)
    {
        var sub = await EnsureAsync(tenantId, ct);
        var plan = await control.Plans.FirstOrDefaultAsync(p => p.Code == planCode && p.IsActive && !p.IsDeleted, ct)
            ?? throw new InvalidOperationException($"Unknown or inactive plan '{planCode}'");
        var addons = (await control.Addons.Where(a => a.IsActive && !a.IsDeleted).ToListAsync(ct)).ToDictionary(a => a.Code);
        foreach (var code in addonQty.Keys)
            if (!addons.ContainsKey(code)) throw new InvalidOperationException($"Unknown or inactive add-on '{code}'");

        // Monthly total (flat add-ons priced once; per-unit add-ons × qty).
        decimal total = plan.MonthlyPrice;
        foreach (var (code, qty) in addonQty)
        {
            if (qty <= 0) continue;
            var a = addons[code];
            total += a.Unit == "flat_month" ? a.UnitPrice : a.UnitPrice * qty;
        }

        // RIT billing tax (e.g. SL VAT) on top, by the tenant's country — foreign = export (zero-rated).
        var taxes = await ApplicableTaxesAsync(await TenantCountryAsync(tenantId, ct), ct);
        decimal taxTotal = taxes.Sum(t => Math.Round(total * t.RatePercent / 100m, 2));
        decimal grand = total + taxTotal;

        var pay = await payment.ChargeAsync(tenantId, grand, plan.Currency, $"RIT HMS — {plan.Name} + add-ons", ct);
        if (!pay.Success) throw new InvalidOperationException(pay.Error ?? "Payment failed");

        // Replace line-items with the new basket (price snapshots from the live catalog).
        var existing = await control.SubscriptionItems.Where(i => i.SubscriptionId == sub.Id).ToListAsync(ct);
        control.SubscriptionItems.RemoveRange(existing);
        control.SubscriptionItems.Add(new SubscriptionItem { SubscriptionId = sub.Id, ItemType = "plan", ItemCode = plan.Code, Quantity = 1, UnitPrice = plan.MonthlyPrice, Currency = plan.Currency });
        foreach (var (code, qty) in addonQty)
        {
            if (qty <= 0) continue;
            var a = addons[code];
            control.SubscriptionItems.Add(new SubscriptionItem { SubscriptionId = sub.Id, ItemType = "addon", ItemCode = code, Quantity = qty, UnitPrice = a.UnitPrice, Currency = a.Currency });
        }
        sub.Plan = plan.Code;
        sub.Provider = payment.Name;
        if (sub.Status == "cancelled") { sub.Status = "active"; sub.CancelledAt = null; }
        await control.SaveChangesAsync(ct);

        await ProjectAsync(tenantId, ct);
        return sub;
    }

    /// <summary>Compute entitlements from the subscription + live catalog and write them into the
    /// tenant's org_settings (the runtime-enforced projection).</summary>
    public async Task ProjectAsync(Guid tenantId, CancellationToken ct)
    {
        var sub = await control.Subscriptions.AsNoTracking().FirstOrDefaultAsync(s => s.TenantId == tenantId && !s.IsDeleted, ct);
        if (sub is null) return;
        var items = await control.SubscriptionItems.AsNoTracking().Where(i => i.SubscriptionId == sub.Id && !i.IsDeleted).ToListAsync(ct);
        var plan = await control.Plans.AsNoTracking().FirstOrDefaultAsync(p => p.Code == sub.Plan, ct);

        int includedLocations = plan?.IncludedLocations ?? 0;
        int includedUsers = plan?.IncludedUsers ?? 0;
        int extraLocations = items.Where(i => i.ItemCode == AddonExtraLocation).Sum(i => i.Quantity);
        int tabSeats = items.Where(i => i.ItemCode == AddonTabDevice).Sum(i => i.Quantity);
        bool guestQr = items.Any(i => i.ItemCode == AddonGuestQr && i.Quantity >= 1);

        // E-receipts (#79): the all-channels tier wins over email-only; quota = that add-on's IncludedQty.
        var addonCatalog = await control.Addons.AsNoTracking().Where(a => !a.IsDeleted).ToListAsync(ct);
        string erChannels = ""; int erQuota = 0;
        if (items.Any(i => i.ItemCode == AddonEreceiptAll && i.Quantity >= 1))
        { erChannels = "email,sms,whatsapp"; erQuota = addonCatalog.FirstOrDefault(a => a.Code == AddonEreceiptAll)?.IncludedQty ?? 0; }
        else if (items.Any(i => i.ItemCode == AddonEreceiptEmail && i.Quantity >= 1))
        { erChannels = "email"; erQuota = addonCatalog.FirstOrDefault(a => a.Code == AddonEreceiptEmail)?.IncludedQty ?? 0; }

        await using var tdb = factory.Create(tenantId);
        var os = await tdb.OrgSettings.FirstOrDefaultAsync(ct);
        if (os is null) { os = new OrgSettings { TenantId = tenantId, CreatedAt = DateTime.UtcNow }; tdb.OrgSettings.Add(os); }
        os.PlanCode = sub.Plan;
        // #6 tier ceiling: a plan's MaxLocations is a HARD cap on total outlets (0 = unlimited).
        // Falls back to included + purchased add-ons for any plan that hasn't set a cap.
        int maxLocations = plan?.MaxLocations ?? 0;
        os.LocationLimit = maxLocations > 0 ? maxLocations : (includedLocations + extraLocations);
        os.UserLimit = includedUsers;
        os.TabDeviceLimit = tabSeats;
        os.GuestQrEnabled = guestQr;
        os.EReceiptChannels = erChannels;     // usage meter (used / period) is left untouched — only the entitlement projects
        os.EReceiptQuota = erQuota;
        os.UpdatedAt = DateTime.UtcNow;
        await tdb.SaveChangesAsync(ct);
    }

    /// <summary>Bill every subscription whose period has ended (recurring billing, #110). Charges the
    /// saved card via the payment seam; on success advances the period a month, on failure marks
    /// past_due. No-op unless a real gateway is configured — manual/stub mode never auto-bills.</summary>
    public async Task<(int charged, int failed)> RenewDueAsync(CancellationToken ct)
    {
        if (!payHere.Value.IsFullyConfigured) return (0, 0);
        var now = DateTime.UtcNow;
        var due = await control.Subscriptions
            .Where(s => !s.IsDeleted
                && (s.Status == "trialing" || s.Status == "active" || s.Status == "past_due")
                && s.CurrentPeriodEnd != null && s.CurrentPeriodEnd <= now)
            .ToListAsync(ct);

        int charged = 0, failed = 0;
        foreach (var sub in due)
        {
            var items = await control.SubscriptionItems.AsNoTracking()
                .Where(i => i.SubscriptionId == sub.Id && !i.IsDeleted).ToListAsync(ct);
            decimal total = items.Sum(i => i.ItemType == "plan" ? i.UnitPrice : i.UnitPrice * (i.Quantity <= 0 ? 1 : i.Quantity));
            var currency = items.FirstOrDefault()?.Currency ?? "LKR";

            if (total <= 0)   // free plan — just roll the period forward
            {
                sub.Status = "active"; sub.CurrentPeriodStart = now; sub.CurrentPeriodEnd = now.AddMonths(1);
                charged++; await control.SaveChangesAsync(ct); continue;
            }
            var taxes = await ApplicableTaxesAsync(await TenantCountryAsync(sub.TenantId, ct), ct);
            decimal grand = total + taxes.Sum(t => Math.Round(total * t.RatePercent / 100m, 2));
            var pay = await payment.ChargeAsync(sub.TenantId, grand, currency, $"RIT HMS — {sub.Plan} monthly renewal", ct);
            if (pay.Success)
            {
                sub.Status = "active"; sub.CurrentPeriodStart = now; sub.CurrentPeriodEnd = now.AddMonths(1); charged++;
            }
            else { sub.Status = "past_due"; failed++; }
            await control.SaveChangesAsync(ct);
        }
        return (charged, failed);
    }

    /// <summary>Subscription view for the owner / RIT: header + priced line-items + monthly total.</summary>
    public async Task<object> GetViewAsync(Guid tenantId, CancellationToken ct)
    {
        var sub = await EnsureAsync(tenantId, ct);
        var items = await control.SubscriptionItems.AsNoTracking().Where(i => i.SubscriptionId == sub.Id && !i.IsDeleted).ToListAsync(ct);
        var lines = items.Select(i => new { i.ItemType, i.ItemCode, i.Quantity, i.UnitPrice, i.Currency,
            lineTotal = i.ItemType == "addon" && items.Any() && i.UnitPrice >= 0 ? i.UnitPrice * (i.Quantity <= 0 ? 1 : i.Quantity) : i.UnitPrice }).ToList();
        decimal total = items.Sum(i => i.ItemType == "plan" ? i.UnitPrice : i.UnitPrice * (i.Quantity <= 0 ? 1 : i.Quantity));

        // RIT billing tax by the tenant's country (SL VAT for domestic, export-zero-rated for foreign).
        var taxes = await ApplicableTaxesAsync(await TenantCountryAsync(tenantId, ct), ct);
        var taxLines = taxes.Select(t => new { t.Code, t.Name, t.RatePercent, amount = Math.Round(total * t.RatePercent / 100m, 2) }).ToList();
        decimal taxTotal = taxLines.Sum(t => t.amount);

        int? daysRemaining = sub.Status == "trialing" && sub.CurrentPeriodEnd is DateTime te
            ? Math.Max(0, (int)Math.Ceiling((te - DateTime.UtcNow).TotalDays)) : null;
        return new
        {
            sub.TenantId, plan = sub.Plan, sub.Status, sub.Provider,
            sub.CurrentPeriodStart, sub.CurrentPeriodEnd, daysRemaining,
            items = lines, monthlyTotal = total,
            taxes = taxLines, taxTotal, grandTotal = total + taxTotal,
            currency = items.FirstOrDefault()?.Currency ?? "LKR",
            // Payment method (PayHere #110): drives the owner "card on file" UI.
            gatewayReady = payHere.Value.IsFullyConfigured,
            hasPaymentMethod = !string.IsNullOrWhiteSpace(sub.CustomerToken),
            cardBrand = sub.CardBrand, cardLast4 = sub.CardLast4,
            paymentMethodUpdatedAt = sub.PaymentMethodUpdatedAt,
        };
    }
}
