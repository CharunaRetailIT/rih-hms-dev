using Hms.Api.Domain;
using Hms.Api.Features.Aggregators;
using Hms.Api.Features.Aggregators.PickMe;
using Hms.Api.Features.Orders;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using Xunit;

namespace Hms.Api.IntegrationTests;

/// <summary>
/// Spins up ephemeral test databases (control + one tenant), applies the REAL
/// SQL migrations, seeds minimal master data, and wires the actual services
/// (OrderService, AggregatorService) against them. Tests exercise the genuine
/// business logic end-to-end on real Postgres — no mocks.
///
/// Connection: env HMS_TEST_PG (e.g. "Host=localhost;Username=postgres;Password=postgres")
/// or defaults to a local trust connection. Set via CI's postgres service.
/// </summary>
public sealed class PostgresFixture : IAsyncLifetime
{
    private const string ControlDb = "hms_it_control";
    private const string TenantDb = "hms_it_tenant";
    private string _adminConn = default!;   // connects to 'postgres' db for CREATE DATABASE
    private string _controlConn = default!;
    private string _tenantTemplate = default!;
    public Guid TenantId { get; private set; }
    public Guid LocationId { get; private set; }

    /// <summary>Test control-plane connection string (for WebApplicationFactory-hosted HTTP tests).</summary>
    public string ControlConn => _controlConn;
    /// <summary>Test tenant connection template with the {tenant_db} placeholder.</summary>
    public string TenantTemplate => _tenantTemplate;

    private string Base =>
        Environment.GetEnvironmentVariable("HMS_TEST_PG")
        ?? "Host=localhost;Username=postgres;Password=postgres";

    public async Task InitializeAsync()
    {
        _adminConn = $"{Base};Database=postgres";
        _controlConn = $"{Base};Database={ControlDb}";
        _tenantTemplate = $"{Base};Database={{tenant_db}}";

        await RecreateDatabase(ControlDb);
        await RecreateDatabase(TenantDb);

        var migrations = FindMigrationsDir();
        // Control plane
        await RunSql(_controlConn, File.ReadAllText(Path.Combine(migrations, "0001_control_plane.sql")));
        await RunSql(_controlConn, File.ReadAllText(Path.Combine(migrations, "0026_control_refresh_tokens.sql")));
        // Tenant template (0002..0008)
        var tenantConn = $"{Base};Database={TenantDb}";
        foreach (var f in new[] { "0002_tenant_template.sql", "0003_master_data.sql", "0004_orders.sql",
                     "0005_tax_settings_multioutlet.sql", "0006_aggregators.sql",
                     "0007_aggregator_credentials.sql", "0008_aggregator_lifecycle.sql",
                     "0009_procurement.sql", "0010_inventory_movements.sql",
                     "0011_supplier_vat_input_tax.sql", "0012_org_numbering_branding.sql",
                     "0013_production.sql", "0014_shifts.sql", "0015_uom_conversion.sql",
                     "0016_production_parity.sql", "0017_grn_enhancements.sql",
                     "0018_po_enhancements.sql", "0019_po_approval_setting.sql",
                     "0020_purchase_returns.sql", "0021_modifiers.sql",
                     "0022_kitchen_stations.sql", "0023_product_variants.sql",
                     "0024_price_levels.sql", "0025_product_tax_class.sql",
                     "0027_promotions.sql", "0028_tables_reservations.sql",
                     "0029_billing_requires_shift.sql", "0030_customers.sql",
                     "0031_role_permissions.sql", "0032_activity_log.sql",
                     "0033_stock_counts.sql", "0034_user_pin.sql",
                     "0035_loyalty.sql", "0036_user_username.sql",
                     "0037_kot_workflow.sql", "0038_pickme.sql",
                     "0039_customer_pricing.sql", "0040_loyalty_tiers.sql",
                     "0041_period_lock.sql", "0042_table_layout.sql",
                     "0043_reprint.sql", "0044_role_screen_access.sql",
                     "0045_promo_tail.sql", "0046_advance.sql",
                     "0047_loyalty_card.sql", "0048_sales_budgets.sql",
                     "0049_print_jobs.sql", "0050_pos_depth.sql",
                     "0051_force_rls.sql", "0052_accounting.sql",
                     "0053_sales_budget_group.sql", "0054_catering.sql",
                     "0055_catering_production.sql" })
            await RunSql(tenantConn, File.ReadAllText(Path.Combine(migrations, f)));

        TenantId = Guid.NewGuid();
        await SeedAsync(tenantConn);
        await EnsureRlsRole(tenantConn);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    // ── service wiring ───────────────────────────────────────────────────────
    /// <summary>A fresh tenant DB context factory wired to the test databases.</summary>
    public ITenantDbContextFactory Factory()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:ControlDb"] = _controlConn,
            ["ConnectionStrings:TenantTemplateDb"] = _tenantTemplate,
            ["Secrets:MasterKey"] = "ZGV2LW9ubHktbWFzdGVyLWtleS0zMmJ5dGVzLXJpdC1obXM=",
        }).Build();
        var control = new ControlDbContext(new DbContextOptionsBuilder<ControlDbContext>()
            .UseNpgsql(_controlConn).UseSnakeCaseNamingConvention().Options);
        var tenantCtx = new TenantContext();
        tenantCtx.Set(TenantId);
        return new TenantDbContextFactory(control, tenantCtx, config);
    }

    public (OrderService orders, AggregatorService agg) Services()
    {
        var factory = Factory();
        var orders = new OrderService(factory);
        var pickme = new PickMeClient(new HttpClient(), NullLogger<PickMeClient>.Instance);
        var agg = new AggregatorService(factory, orders, pickme, Protector(), NullLogger<AggregatorService>.Instance, new Hms.Api.Features.Realtime.RealtimeBus());
        return (orders, agg);
    }

    /// <summary>A PickMeService wired to the test tenant (its HTTP client is unused by IngestJobAsync).</summary>
    public PickMeService PickMe()
    {
        var factory = Factory();
        var orders = new OrderService(factory);
        var pickme = new PickMeClient(new HttpClient(), NullLogger<PickMeClient>.Instance);
        return new PickMeService(factory, orders, pickme, Protector(), NullLogger<PickMeService>.Instance);
    }

    public ISecretProtector Protector() =>
        new AesGcmSecretProtector(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        { ["Secrets:MasterKey"] = "ZGV2LW9ubHktbWFzdGVyLWtleS0zMmJ5dGVzLXJpdC1obXM=" }).Build());

    public TenantDbContext NewTenantContext()
    {
        var opts = new DbContextOptionsBuilder<TenantDbContext>()
            .UseNpgsql($"{Base};Database={TenantDb}").UseSnakeCaseNamingConvention()
            .AddInterceptors(new TenantGucConnectionInterceptor(TenantId))
            .Options;
        return new TenantDbContext(opts, TenantId);
    }

    // ── RLS (defence-in-depth) test support ──────────────────────────────────
    /// <summary>A non-superuser login role used to exercise Row-Level Security.</summary>
    public const string RlsRole = "hms_rls_test";

    /// <summary>
    /// Reproduce production for RLS tests: create a NON-superuser login role and
    /// make it OWN every tenant table. The default test connection is the postgres
    /// superuser, which bypasses RLS entirely; this role is subject to it. Because
    /// the role owns the tables, the only reason RLS applies to it is that the
    /// tables FORCE row-level security (migration 0051) — so a test reading through
    /// <see cref="RlsTenantContext"/> guards both the FORCE migration and the GUC
    /// interceptor. Roles are cluster-global; grants/ownership are per-DB and are
    /// (re)applied on each fixture init since the test DB is recreated.
    /// </summary>
    private async Task EnsureRlsRole(string tenantConn)
    {
        await RunSql(tenantConn, $@"
DO $$ BEGIN
  IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = '{RlsRole}') THEN
    CREATE ROLE {RlsRole} LOGIN PASSWORD '{RlsRole}';
  END IF;
END $$;
GRANT USAGE ON SCHEMA public TO {RlsRole};
DO $$
DECLARE n text;
BEGIN
  FOR n IN SELECT tablename    FROM pg_tables    WHERE schemaname = 'public' LOOP
    EXECUTE format('ALTER TABLE public.%I OWNER TO {RlsRole}', n);
  END LOOP;
  FOR n IN SELECT sequencename FROM pg_sequences WHERE schemaname = 'public' LOOP
    EXECUTE format('ALTER SEQUENCE public.%I OWNER TO {RlsRole}', n);
  END LOOP;
END $$;");
    }

    /// <summary>Run raw SQL against the tenant DB as the (superuser) test admin — bypasses RLS.</summary>
    public Task ExecTenantSqlAsAdminAsync(string sql) => RunSql($"{Base};Database={TenantDb}", sql);

    /// <summary>
    /// A tenant context that connects as the non-superuser <see cref="RlsRole"/>
    /// (which owns the tables) with the GUC interceptor wired — i.e. how the app
    /// behaves in production. RLS is live on this connection.
    /// </summary>
    public TenantDbContext RlsTenantContext(Guid tenantId)
    {
        var csb = new NpgsqlConnectionStringBuilder(Base)
        {
            Database = TenantDb,
            Username = RlsRole,
            Password = RlsRole,
        };
        var opts = new DbContextOptionsBuilder<TenantDbContext>()
            .UseNpgsql(csb.ConnectionString).UseSnakeCaseNamingConvention()
            .AddInterceptors(new TenantGucConnectionInterceptor(tenantId))
            .Options;
        return new TenantDbContext(opts, tenantId);
    }

    /// <summary>Reset transactional tables + restore stock to 50 between tests.</summary>
    public async Task ResetAsync()
    {
        await RunSql($"{Base};Database={TenantDb}",
            "TRUNCATE orders, order_items, kitchen_tickets, payments, order_counters, order_charges, invoice_series, aggregator_outbox, " +
            "  suppliers, purchase_orders, purchase_order_lines, goods_received_notes, grn_lines, document_counters, " +
            "  stock_transfers, stock_transfer_lines, wastage_notes, wastage_lines, stock_adjustments, stock_adjustment_lines, " +
            "  recipes, recipe_lines, production_orders, production_consumptions, shifts, " +
            "  purchase_returns, purchase_return_lines, " +
            "  modifier_groups, modifier_items, product_modifier_groups, order_item_modifiers, " +
            "  kitchen_stations, product_variants, price_levels, product_prices, " +
            "  promotions, promotion_lines, order_promotions, restaurant_tables, reservations, " +
            "  customers, customer_categories, customer_payments, customer_product_prices, role_permissions, role_screen_access, activity_log, " +
            "  stock_counts, stock_count_lines, loyalty_transactions, loyalty_tiers, sales_budgets, print_jobs, " +
            "  tour_operators, currencies, " +
            "  gl_journal_lines, gl_journal_entries, gl_accounts, gl_expenses, ap_payments, " +
            "  catering_event_payments, catering_event_items, catering_events, catering_packages, event_halls CASCADE; " +
            // Rebuild a clean stock baseline: every product at MAIN only, qty 50, avg = cost_price.
            // (drops any CK/other-location rows created by transfer tests)
            "DELETE FROM product_stock; " +
            // Drop any outlets created by tests (e.g. the location-CRUD test); keep the
            // two seeded ones (MAIN + CK) so outlet-summary expectations hold.
            "DELETE FROM location_aggregator_map WHERE location_id NOT IN (SELECT id FROM locations WHERE code IN ('MAIN','CK')); " +
            "DELETE FROM locations WHERE code NOT IN ('MAIN','CK'); " +
            "INSERT INTO product_stock (tenant_id, product_id, location_id, quantity_on_hand, average_cost) " +
            "  SELECT p.tenant_id, p.id, l.id, 50, p.cost_price FROM products p, locations l WHERE l.code = 'MAIN'; " +
            "UPDATE products SET is_available_online = true, kitchen_station_code = NULL, tax_class = 'standard', is_taxable = true; " +
            // org_settings isn't truncated (it carries tenant config), so restore the
            // suite default: billing shift-gate OFF. BillingShiftGateTests flips it ON
            // for itself; without this its value would leak into later test classes.
            "UPDATE org_settings SET require_open_shift_for_billing = false, books_locked_through = NULL, loyalty_enabled = false, loyalty_expiry_days = 0; " +
            // Default stations every test can rely on.
            $"INSERT INTO kitchen_stations (tenant_id, code, name, sort_order) VALUES " +
            $"  ('{TenantId}','KITCHEN','Hot Kitchen',0), ('{TenantId}','BAR','Bar',1); " +
            // Price levels: DINEIN (default) + DELIVERY (auto for delivery orders).
            $"INSERT INTO price_levels (id, tenant_id, code, name, is_default, applies_to_order_type, sort_order) VALUES " +
            $"  ('{DineInLevelId}','{TenantId}','DINEIN','Dine-in',true,NULL,0), " +
            $"  ('{DeliveryLevelId}','{TenantId}','DELIVERY','Delivery',false,'delivery',1);");
    }

    // ── helpers ──────────────────────────────────────────────────────────────
    private async Task RecreateDatabase(string name)
    {
        await using var conn = new NpgsqlConnection(_adminConn);
        await conn.OpenAsync();
        await Exec(conn, $"DROP DATABASE IF EXISTS {name} WITH (FORCE)");
        await Exec(conn, $"CREATE DATABASE {name}");
    }

    private static async Task RunSql(string connStr, string sql)
    {
        await using var conn = new NpgsqlConnection(connStr);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        await cmd.ExecuteNonQueryAsync();
    }

    private static async Task Exec(NpgsqlConnection conn, string sql)
    {
        await using var cmd = new NpgsqlCommand(sql, conn);
        await cmd.ExecuteNonQueryAsync();
    }

    private static string FindMigrationsDir()
    {
        var dir = AppContext.BaseDirectory;
        for (var i = 0; i < 12 && dir is not null; i++)
        {
            var candidate = Path.Combine(dir, "db", "postgres", "migrations");
            if (Directory.Exists(candidate)) return candidate;
            dir = Directory.GetParent(dir)?.FullName;
        }
        throw new DirectoryNotFoundException("Could not locate db/postgres/migrations from " + AppContext.BaseDirectory);
    }

    private async Task SeedAsync(string tenantConn)
    {
        // Control: a tenant row pointing at the tenant DB.
        await using (var control = new ControlDbContext(new DbContextOptionsBuilder<ControlDbContext>()
            .UseNpgsql(_controlConn).UseSnakeCaseNamingConvention().Options))
        {
            control.Tenants.Add(new Tenant
            {
                Id = TenantId, Slug = "it", DisplayName = "IT Test",
                DatabaseName = TenantDb, DatabaseHost = "localhost", Status = TenantStatus.Active,
                Plan = "starter", CountryCode = "LK", DefaultCurrency = "LKR", TimeZone = "Asia/Colombo",
            });
            await control.SaveChangesAsync();
        }

        LocationId = Guid.NewGuid();
        CentralKitchenId = Guid.NewGuid();
        var prodChk = Guid.NewGuid();   // Chicken Kottu 900 (stocked, kitchen)
        var prodLion = Guid.NewGuid();  // Lion Lager 450 (stocked, bar/beverage)
        var catFood = Guid.NewGuid();
        var catBev = Guid.NewGuid();
        var uom = Guid.NewGuid();
        var uomKg = Guid.NewGuid();     // mass base-1000 (g)
        var uomG = Guid.NewGuid();      // mass base-1
        var uomL = Guid.NewGuid();      // volume base-1000 (ml) — for dimension-mismatch test
        var prodRice = Guid.NewGuid();  // Raw Rice, stocked in KG @ 200/kg

        // Tenant master data + the SL tax stack (SVC 10 → SSCL 2.5 compound → VAT 18 compound).
        var sql = $@"
INSERT INTO org_settings (tenant_id, legal_name, vat_registration_number, vat_enabled, invoice_prefix, require_open_shift_for_billing)
  VALUES ('{TenantId}', 'IT Test (Pvt) Ltd', '111111111-7000', true, 'INV', false);
INSERT INTO tax_charges (tenant_id, code, name, charge_type, rate_percent, sequence, compound_on_previous, applies_to_takeaway, applies_to_delivery) VALUES
  ('{TenantId}','SVC','Service Charge','service_charge',10.0,1,false,false,false),
  ('{TenantId}','SSCL','SSCL','levy',2.5,2,true,true,true),
  ('{TenantId}','VAT','VAT','vat',18.0,3,true,true,true);
INSERT INTO units_of_measure (id, tenant_id, code, name, symbol, is_base_unit, dimension, factor_to_base) VALUES
  ('{uom}','{TenantId}','EA','Each','ea',true,'count',1),
  ('{uomKg}','{TenantId}','KG','Kilogram','kg',true,'mass',1000),
  ('{uomG}','{TenantId}','G','Gram','g',false,'mass',1),
  ('{uomL}','{TenantId}','L','Litre','L',true,'volume',1000);
INSERT INTO categories (id, tenant_id, code, name, sort_order) VALUES
  ('{catFood}','{TenantId}','FOOD','Food',1),
  ('{catBev}','{TenantId}','BEV','Beverages',2);
INSERT INTO locations (id, tenant_id, code, name, address_line1, city, country_code, currency, location_type, can_sell, can_produce, can_stock, default_prep_minutes) VALUES
  ('{LocationId}','{TenantId}','MAIN','Main','1 St','Colombo','LK','LKR','outlet',true,false,true,20),
  ('{CentralKitchenId}','{TenantId}','CK','Central Kitchen','5 Ind Rd','Colombo','LK','LKR','central_kitchen',false,true,true,20);
INSERT INTO products (id, tenant_id, sku, name, category_id, unit_of_measure_id, base_price, cost_price, sort_order, is_available_online) VALUES
  ('{prodChk}','{TenantId}','KOTTU-CHK','Chicken Kottu','{catFood}','{uom}',900,350,1,true),
  ('{prodLion}','{TenantId}','BEV-LION','Lion Lager','{catBev}','{uom}',450,220,2,true),
  ('{prodRice}','{TenantId}','RICE-RAW','Raw Rice','{catFood}','{uomKg}',0,200,3,false);
INSERT INTO product_stock (tenant_id, product_id, location_id, quantity_on_hand, average_cost) VALUES
  ('{TenantId}','{prodChk}','{LocationId}',50,350),
  ('{TenantId}','{prodLion}','{LocationId}',50,220),
  ('{TenantId}','{prodRice}','{LocationId}',50,200);
";
        await RunSql(tenantConn, sql);
        ChickenKottuId = prodChk;
        LionLagerId = prodLion;
        RiceId = prodRice;
        GramUnitId = uomG;
        LitreUnitId = uomL;
    }

    public Guid CentralKitchenId { get; private set; }
    public Guid ChickenKottuId { get; private set; }
    public Guid LionLagerId { get; private set; }
    public Guid RiceId { get; private set; }        // stocked in KG @ 200/kg
    public Guid GramUnitId { get; private set; }    // mass, factor 1
    public Guid LitreUnitId { get; private set; }   // volume — for dimension-mismatch test
    public Guid DineInLevelId { get; } = Guid.NewGuid();   // default price level
    public Guid DeliveryLevelId { get; } = Guid.NewGuid(); // applies to delivery orders
}

[CollectionDefinition("pg")]
public class PgCollection : ICollectionFixture<PostgresFixture> { }
