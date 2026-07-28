using Hms.Api.Features.Aggregators;
using Hms.Api.Features.Approvals;
using Hms.Api.Features.Availability;
using Hms.Api.Features.Auth;
using Hms.Api.Features.Guest;
using Hms.Api.Features.Health;
using Hms.Api.Features.Printing;
using Hms.Api.Features.Inventory;
using Hms.Api.Features.RequestNotes;
using Hms.Api.Features.Modifiers;
using Hms.Api.Features.Kitchen;
using Hms.Api.Features.Accounting;
using Hms.Api.Features.Fx;
using Hms.Api.Features.Catering;
using Hms.Api.Features.Notifications;
using Hms.Api.Features.Tab;
using Hms.Api.Features.Billing;
using Hms.Api.Features.Orders;
using Hms.Api.Features.Realtime;
using Hms.Api.Features.Pos;
using Hms.Api.Features.Procurement;
using Hms.Api.Features.Replenishment;
using Hms.Api.Features.Production;
using Hms.Api.Features.Products;
using Hms.Api.Features.Locations;
using Hms.Api.Features.Departments;
using Hms.Api.Features.Suppliers;
using Hms.Api.Features.ServingUnits;
using Hms.Api.Features.PriceLevels;
using Hms.Api.Features.KitchenStations;
using Hms.Api.Features.ChargeTypes;
using Hms.Api.Features.Charges;
using Hms.Api.Features.UnitsOfMeasure;
using Hms.Api.Features.UnitConversions;
using Hms.Api.Features.Promotions;
using Hms.Api.Features.Tables;
using Hms.Api.Features.Floors;
using Hms.Api.Features.Push;
using Hms.Api.Features.Customers;
using Hms.Api.Features.Loyalty;
using Hms.Api.Features.Permissions;
using Hms.Api.Features.Reports;
using Hms.Api.Features.Audit;
using Hms.Api.Features.StockCounts;
using Hms.Api.Features.Settings;
using Hms.Api.Features.Shifts;
using Hms.Api.Features.Users;
using Hms.Api.Features.Tenants;
using System.Text;
using Hms.Api.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Gitignored per-developer overrides (secrets that shouldn't hit source control,
// e.g. sandbox payment gateway creds) — optional, loaded last so it wins.
builder.Configuration.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);

// QuestPDF community licence (free for our scale) — required before any PDF render (#79 e-receipt PDF).
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

// ---------- Logging ----------
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

// ---------- Services ----------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "RIT HMS API", Version = "v1" });
});

// Control plane DB (hms_control) — tenants, subscriptions, control records
builder.Services.AddDbContext<ControlDbContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("ControlDb")
            ?? throw new InvalidOperationException("ControlDb connection string missing"))
        .UseSnakeCaseNamingConvention());

// Per-tenant DB factory — created per-request based on tenant_id from JWT
builder.Services.AddScoped<ITenantDbContextFactory, TenantDbContextFactory>();
builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddScoped<LocationScope>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<Hms.Api.Features.Approvals.ApprovalService>();   // #approvals engine
builder.Services.AddScoped<AggregatorService>();
builder.Services.AddScoped<ProcurementService>();
builder.Services.AddScoped<ProductionService>();
builder.Services.AddScoped<ShiftService>();
builder.Services.AddScoped<Hms.Api.Features.Permissions.PermissionService>();
builder.Services.AddScoped<Hms.Api.Features.Audit.AuditService>();
builder.Services.AddScoped<Hms.Api.Features.StockCounts.StockCountService>();
builder.Services.AddScoped<CateringService>();
// Payment seam (#110): auto-select PayHere when BOTH credential pairs are configured
// (App ID/Secret for charging + Merchant ID/Secret for card capture), else the manual stub.
// Secrets come from /opt/hms/hms.env (PayHere__*), never from code.
builder.Services.Configure<Hms.Api.Features.Billing.PayHereOptions>(builder.Configuration.GetSection("PayHere"));
builder.Services.Configure<Hms.Api.Features.Billing.BillingOptions>(builder.Configuration.GetSection("Billing"));

// Email seam (#79): Resend when an API key is configured (/opt/hms/hms.env Resend__*), else log-only.
builder.Services.Configure<Hms.Api.Features.Email.ResendOptions>(builder.Configuration.GetSection("Resend"));
builder.Services.AddHttpClient("resend", c => c.Timeout = TimeSpan.FromSeconds(20));
builder.Services.AddSingleton<Hms.Api.Features.Email.IEmailSender>(sp =>
{
    var o = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<Hms.Api.Features.Email.ResendOptions>>();
    return string.IsNullOrWhiteSpace(o.Value.ApiKey)
        ? new Hms.Api.Features.Email.LogOnlyEmailSender(sp.GetRequiredService<ILogger<Hms.Api.Features.Email.LogOnlyEmailSender>>())
        : new Hms.Api.Features.Email.ResendEmailSender(sp.GetRequiredService<IHttpClientFactory>(), o, sp.GetRequiredService<ILogger<Hms.Api.Features.Email.ResendEmailSender>>());
});

// SMS seam (#79): Sender RT when a gateway is configured (/opt/hms/hms.env Sms__*), else log-only.
builder.Services.Configure<Hms.Api.Features.Sms.SmsOptions>(builder.Configuration.GetSection("Sms"));
builder.Services.AddHttpClient("sms", c => c.Timeout = TimeSpan.FromSeconds(35));
builder.Services.AddSingleton<Hms.Api.Features.Sms.ISmsSender>(sp =>
{
    var o = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<Hms.Api.Features.Sms.SmsOptions>>();
    return o.Value.Configured
        ? new Hms.Api.Features.Sms.SenderRtSmsSender(sp.GetRequiredService<IHttpClientFactory>(), o, sp.GetRequiredService<ILogger<Hms.Api.Features.Sms.SenderRtSmsSender>>())
        : new Hms.Api.Features.Sms.LogOnlySmsSender(sp.GetRequiredService<ILogger<Hms.Api.Features.Sms.LogOnlySmsSender>>());
});

// Web Push seam (#floor-push): VAPID-signed push when keys are configured (Vapid__* /
// /opt/hms/hms.env), else log-only. Drives floor-scoped "new guest order" notifications
// reaching a steward even when their tab/app is closed.
builder.Services.Configure<Hms.Api.Features.Push.VapidOptions>(builder.Configuration.GetSection("Vapid"));
builder.Services.AddSingleton<Hms.Api.Features.Push.IPushSender>(sp =>
{
    var o = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<Hms.Api.Features.Push.VapidOptions>>();
    return string.IsNullOrWhiteSpace(o.Value.PublicKey) || string.IsNullOrWhiteSpace(o.Value.PrivateKey)
        ? new Hms.Api.Features.Push.LogOnlyPushSender(sp.GetRequiredService<ILogger<Hms.Api.Features.Push.LogOnlyPushSender>>())
        : new Hms.Api.Features.Push.VapidPushSender(o, sp.GetRequiredService<ILogger<Hms.Api.Features.Push.VapidPushSender>>());
});

// FCM seam (#floor-push, Phase 4): the handheld app's mobile sibling of the web push seam
// above. Real sender when a Firebase service-account key is configured (Firebase__* /
// /opt/hms/hms.env), else log-only.
builder.Services.Configure<Hms.Api.Features.Push.FirebaseOptions>(builder.Configuration.GetSection("Firebase"));
builder.Services.AddSingleton<Hms.Api.Features.Push.IFcmSender>(sp =>
{
    var o = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<Hms.Api.Features.Push.FirebaseOptions>>();
    if (!string.IsNullOrWhiteSpace(o.Value.ServiceAccountKeyPath) && File.Exists(o.Value.ServiceAccountKeyPath))
        return new Hms.Api.Features.Push.FirebaseFcmSender(o, sp.GetRequiredService<ILogger<Hms.Api.Features.Push.FirebaseFcmSender>>());
    return new Hms.Api.Features.Push.LogOnlyFcmSender(sp.GetRequiredService<ILogger<Hms.Api.Features.Push.LogOnlyFcmSender>>());
});
builder.Services.AddHttpClient("payhere", c => c.Timeout = TimeSpan.FromSeconds(30));
builder.Services.AddScoped<Hms.Api.Features.Billing.IPaymentProvider>(sp =>
{
    var opt = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<Hms.Api.Features.Billing.PayHereOptions>>();
    if (opt.Value.IsFullyConfigured)
        return new Hms.Api.Features.Billing.PayHerePaymentProvider(
            sp.GetRequiredService<IHttpClientFactory>(), opt,
            sp.GetRequiredService<Hms.Api.Infrastructure.ControlDbContext>(),
            sp.GetRequiredService<ILogger<Hms.Api.Features.Billing.PayHerePaymentProvider>>());
    return new Hms.Api.Features.Billing.ManualPaymentProvider();
});
builder.Services.AddScoped<Hms.Api.Features.Billing.SubscriptionService>();
builder.Services.AddScoped<Hms.Api.Features.Availability.AvailabilityService>();
builder.Services.AddHostedService<Hms.Api.Features.Billing.SubscriptionRenewalPoller>();   // recurring billing (gated on a live gateway)
builder.Services.AddScoped<Hms.Api.Features.Accounting.AccountingService>();
builder.Services.AddScoped<ModifierService>();
builder.Services.AddScoped<InventoryMovementService>();
builder.Services.AddScoped<RequestNoteService>();
builder.Services.AddScoped<Hms.Api.Features.Replenishment.ReplenishmentService>();
builder.Services.AddScoped<Hms.Api.Features.Tenants.ProvisioningService>();
builder.Services.AddScoped<Hms.Api.Features.Products.ProductCategoriesService>();
builder.Services.AddScoped<Hms.Api.Features.UnitsOfMeasure.UnitsOfMeasureService>();
builder.Services.AddScoped<Hms.Api.Features.UnitConversions.UnitConversionsService>();
builder.Services.AddScoped<Hms.Api.Features.Locations.LocationsService>();
builder.Services.AddScoped<Hms.Api.Features.Departments.DepartmentsService>();
builder.Services.AddScoped<Hms.Api.Features.Suppliers.SuppliersService>();
builder.Services.AddScoped<Hms.Api.Features.ServingUnits.ServingUnitsService>();
builder.Services.AddScoped<Hms.Api.Features.PriceLevels.PriceLevelsService>();
builder.Services.AddScoped<Hms.Api.Features.KitchenStations.KitchenStationsService>();
builder.Services.AddScoped<Hms.Api.Features.Products.ProductsService>();
builder.Services.AddScoped<Hms.Api.Features.ChargeTypes.ChargeTypesService>();
builder.Services.AddScoped<Hms.Api.Features.Charges.ChargesService>();
builder.Services.AddScoped<Hms.Api.Features.Loyalty.LoyaltyCardSchemeService>();
builder.Services.AddSingleton<ISecretProtector, AesGcmSecretProtector>();
builder.Services.AddSingleton<Hms.Api.Features.Kitchen.KitchenBroadcaster>();
builder.Services.AddSingleton<Hms.Api.Features.Realtime.RealtimeBus>();
builder.Services.AddHttpClient<Hms.Api.Features.Aggregators.PickMe.PickMeClient>(c => c.Timeout = TimeSpan.FromSeconds(20));
builder.Services.AddScoped<Hms.Api.Features.Aggregators.PickMe.PickMeService>();
builder.Services.AddHostedService<Hms.Api.Features.Aggregators.PickMe.PickMePoller>();

// CORS for local dev — the Next.js web app (fixed :3000) and the Flutter handheld's
// `flutter run -d chrome` dev server (a random localhost port picked fresh each run).
builder.Services.AddCors(o => o.AddPolicy("dev",
    p => p.SetIsOriginAllowed(origin =>
            Uri.TryCreate(origin, UriKind.Absolute, out var u) &&
            (u.Host == "localhost" || u.Host == "127.0.0.1"))
          .AllowAnyHeader()
          .AllowAnyMethod()
          .AllowCredentials()));

// ---------- AuthN / AuthZ ----------
// The browser sends a Bearer JWT minted by /api/v1/auth/exchange. We validate it
// against the same Jwt config that signed it. A policy scheme forwards to the JWT
// handler when an Authorization header is present, otherwise to a DEV-ONLY scheme
// that accepts the X-Tenant-Id header (no-op in production). Every endpoint then
// requires an authenticated principal via the fallback policy unless it opts out
// with .AllowAnonymous().
var jwtCfg = builder.Configuration.GetSection("Jwt");

// Fail fast rather than boot an insecure server in Production (no-op in dev/test).
SecurityStartup.AssertProductionSecrets(
    builder.Environment.IsProduction(),
    jwtCfg["SigningKey"],
    builder.Configuration["Secrets:MasterKey"]);

builder.Services.AddAuthentication("smart")
    .AddPolicyScheme("smart", "JWT bearer, or X-Tenant-Id in dev", o =>
    {
        o.ForwardDefaultSelector = ctx =>
            ctx.Request.Headers.Authorization.ToString()
               .StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                ? JwtBearerDefaults.AuthenticationScheme
                : DevHeaderAuthHandler.SchemeName;
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, o =>
    {
        // Keep claim names verbatim ("role", "sub", "email") instead of remapping
        // them to long WS-* URIs — so RoleClaimType="role" and the sub lookup work.
        o.MapInboundClaims = false;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtCfg["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtCfg["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtCfg["SigningKey"]
                    ?? throw new InvalidOperationException("Jwt:SigningKey missing"))),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            RoleClaimType = "role",   // the "role" claim (Owner/Manager/…) drives RequireRole
        };
    })
    .AddScheme<AuthenticationSchemeOptions, DevHeaderAuthHandler>(DevHeaderAuthHandler.SchemeName, _ => { });

builder.Services.AddAuthorization(o =>
{
    // Default: any authenticated user (used by product/category/location reads
    // that every role needs).
    o.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    // Role bundles. "Admin" is an owner-level role (full access EXCEPT removing/altering the owner
    // and editing business identity — those carve-outs are enforced in the relevant handlers).
    o.AddPolicy("Owners",      p => p.RequireRole("Owner", "Admin"));
    o.AddPolicy("BackOffice",  p => p.RequireRole("Owner", "Admin", "Manager", "Accountant")); // reports, settings, tax
    o.AddPolicy("SupplyChain", p => p.RequireRole("Owner", "Admin", "Manager"));               // procurement, inventory, production
    o.AddPolicy("Operations",  p => p.RequireRole("Owner", "Admin", "Manager", "Cashier"));    // POS, orders, shifts, delivery
    o.AddPolicy("KitchenView", p => p.RequireRole("Owner", "Admin", "Manager", "Cashier", "Kitchen"));
    // Guest QR (#108): a scoped "Guest" JWT can ONLY place an order for its own table (the table
    // is baked into the token). Staff roles can also hit it (e.g. to test). Nothing else.
    o.AddPolicy("GuestOrders", p => p.RequireRole("Guest", "Owner", "Admin", "Manager", "Cashier"));

    // Platform admin (cross-tenant control-plane ops, e.g. listing all tenants).
    // Allowlist of emails from config (Platform:AdminEmails) — empty ⇒ nobody.
    var adminEmails = builder.Configuration.GetSection("Platform:AdminEmails").Get<string[]>() ?? Array.Empty<string>();
    o.AddPolicy("PlatformAdmin", p => p.RequireAssertion(ctx =>
        adminEmails.Length > 0 &&
        ctx.User.FindFirst("email")?.Value is { } email &&
        adminEmails.Contains(email, StringComparer.OrdinalIgnoreCase)));
});

var app = builder.Build();

// ---------- Pipeline ----------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("dev");
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Tenant resolution middleware — reads the tenant_id claim populated by the
// authentication step above (JWT in prod, X-Tenant-Id dev scheme locally).
app.UseMiddleware<TenantMiddleware>();

// ---------- Endpoints ----------
app.MapHealthEndpoints();
app.MapTenantEndpoints();
app.MapAuthEndpoints();
app.MapProductEndpoints();
app.MapProductCategoriesEndpoints();
app.MapUnitsOfMeasureEndpoints();
app.MapUnitConversionsEndpoints();
app.MapLocationEndpoints();
app.MapDepartmentEndpoints();
app.MapSupplierEndpoints();
app.MapServingUnitEndpoints();
app.MapPriceLevelEndpoints();
app.MapKitchenStationEndpoints();
app.MapChargeTypeEndpoints();
app.MapChargeEndpoints();
app.MapUserEndpoints();
app.MapOrderEndpoints();
app.MapApprovalEndpoints();   // #approvals — config, inbox, public link
app.MapKitchenEndpoints();
app.MapSettingsEndpoints();
app.MapAggregatorEndpoints();
app.MapProcurementEndpoints();
app.MapProductionEndpoints();
app.MapShiftEndpoints();
app.MapModifierEndpoints();
app.MapInventoryEndpoints();
app.MapRequestNotesEndpoints();
app.MapReplenishmentEndpoints();   // #replenishment — worksheet + draft transfers/POs
app.MapPromotionEndpoints();
app.MapTableEndpoints();
app.MapFloorEndpoints();
app.MapPushEndpoints();
app.MapCustomerEndpoints();
app.MapLoyaltyCardSchemeEndpoints();
app.MapPermissionEndpoints();
app.MapReportEndpoints();
app.MapTransactionEndpoints();
app.MapPrintEndpoints();
app.MapAuditEndpoints();
app.MapStockCountEndpoints();
app.MapPosConfigEndpoints();
app.MapCateringEndpoints();
app.MapNotificationEndpoints();
app.MapTabDeviceEndpoints();
app.MapTabSessionEndpoints();
app.MapBillingEndpoints();
app.MapAvailabilityEndpoints();
app.MapGuestEndpoints();
app.MapRealtimeEndpoints();
app.MapAccountingEndpoints();
app.MapFxEndpoints();

app.MapGet("/", () => Results.Ok(new
{
    name = "RIT HMS API",
    version = "0.1.0",
    docs = "/swagger",
    health = "/health"
})).AllowAnonymous();

app.Run();

// Exposed for WebApplicationFactory-based integration tests.
public partial class Program { }
