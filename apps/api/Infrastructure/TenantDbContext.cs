using Hms.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Infrastructure;

/// <summary>
/// Per-tenant data context. Created per-request via <see cref="ITenantDbContextFactory"/>,
/// pointed at <c>hms_tenant_&lt;id&gt;</c>. Tenant isolation is layered: DB-per-tenant
/// physically, the EF query filter on TenantId below, and Postgres Row-Level
/// Security — the connection sets <c>app.tenant_id</c> on open (see
/// <see cref="TenantGucConnectionInterceptor"/>) and the tables FORCE RLS
/// (migration 0051), so the policy holds even if the EF filter is bypassed.
/// </summary>
public class TenantDbContext : DbContext
{
    private readonly Guid _tenantId;

    public TenantDbContext(DbContextOptions<TenantDbContext> options, Guid tenantId)
        : base(options)
    {
        _tenantId = tenantId;
    }

    /// <summary>The tenant this context is scoped to. Used to stamp non-BaseEntity rows.</summary>
    public Guid TenantId => _tenantId;

    public DbSet<User> Users => Set<User>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<UnitOfMeasure> UnitsOfMeasure => Set<UnitOfMeasure>();
    public DbSet<Tax> Taxes => Set<Tax>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<KitchenStation> KitchenStations => Set<KitchenStation>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<PriceLevel> PriceLevels => Set<PriceLevel>();
    public DbSet<ProductPrice> ProductPrices => Set<ProductPrice>();
    public DbSet<Promotion> Promotions => Set<Promotion>();
    public DbSet<PromotionLine> PromotionLines => Set<PromotionLine>();
    public DbSet<OrderPromotion> OrderPromotions => Set<OrderPromotion>();
    public DbSet<RestaurantTable> RestaurantTables => Set<RestaurantTable>();
    public DbSet<Floor> Floors => Set<Floor>();
    public DbSet<UserFloor> UserFloors => Set<UserFloor>();
    public DbSet<PushSubscription> PushSubscriptions => Set<PushSubscription>();
    public DbSet<DeviceToken> DeviceTokens => Set<DeviceToken>();
    public DbSet<TabDevice> TabDevices => Set<TabDevice>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<ProductStock> ProductStocks => Set<ProductStock>();
    public DbSet<ProductAvailabilityOverride> ProductAvailabilityOverrides => Set<ProductAvailabilityOverride>();
    public DbSet<ProductReplenishmentLevel> ProductReplenishmentLevels => Set<ProductReplenishmentLevel>();
    public DbSet<ApprovalRule> ApprovalRules => Set<ApprovalRule>();
    public DbSet<ApprovalRuleStep> ApprovalRuleSteps => Set<ApprovalRuleStep>();
    public DbSet<ApprovalRequest> ApprovalRequests => Set<ApprovalRequest>();
    public DbSet<ApprovalAction> ApprovalActions => Set<ApprovalAction>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<KitchenTicket> KitchenTickets => Set<KitchenTicket>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<OrderCharge> OrderCharges => Set<OrderCharge>();
    public DbSet<OrgSettings> OrgSettings => Set<OrgSettings>();
    public DbSet<InvoiceSeries> InvoiceSeries => Set<InvoiceSeries>();
    public DbSet<AggregatorOutbox> AggregatorOutbox => Set<AggregatorOutbox>();
    public DbSet<AggregatorCredential> AggregatorCredentials => Set<AggregatorCredential>();
    public DbSet<LocationAggregatorMap> LocationAggregatorMaps => Set<LocationAggregatorMap>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderLine> PurchaseOrderLines => Set<PurchaseOrderLine>();
    public DbSet<GoodsReceivedNote> GoodsReceivedNotes => Set<GoodsReceivedNote>();
    public DbSet<GrnLine> GrnLines => Set<GrnLine>();
    public DbSet<PurchaseReturn> PurchaseReturns => Set<PurchaseReturn>();
    public DbSet<PurchaseReturnLine> PurchaseReturnLines => Set<PurchaseReturnLine>();
    public DbSet<ModifierGroup> ModifierGroups => Set<ModifierGroup>();
    public DbSet<ModifierItem> ModifierItems => Set<ModifierItem>();
    public DbSet<ProductModifierGroup> ProductModifierGroups => Set<ProductModifierGroup>();
    public DbSet<OrderItemModifier> OrderItemModifiers => Set<OrderItemModifier>();
    public DbSet<StockTransfer> StockTransfers => Set<StockTransfer>();
    public DbSet<StockTransferLine> StockTransferLines => Set<StockTransferLine>();
    public DbSet<WastageNote> WastageNotes => Set<WastageNote>();
    public DbSet<WastageLine> WastageLines => Set<WastageLine>();
    public DbSet<StockAdjustment> StockAdjustments => Set<StockAdjustment>();
    public DbSet<StockAdjustmentLine> StockAdjustmentLines => Set<StockAdjustmentLine>();
    public DbSet<RequestNote> RequestNotes => Set<RequestNote>();
    public DbSet<RequestNoteLine> RequestNoteLines => Set<RequestNoteLine>();
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<RecipeLine> RecipeLines => Set<RecipeLine>();
    public DbSet<ProductionOrder> ProductionOrders => Set<ProductionOrder>();
    public DbSet<ProductionConsumption> ProductionConsumptions => Set<ProductionConsumption>();
    public DbSet<Shift> Shifts => Set<Shift>();
    public DbSet<CustomerCategory> CustomerCategories => Set<CustomerCategory>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerPayment> CustomerPayments => Set<CustomerPayment>();
    public DbSet<CustomerProductPrice> CustomerProductPrices => Set<CustomerProductPrice>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<RoleScreenAccess> RoleScreenAccess => Set<RoleScreenAccess>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<StockCount> StockCounts => Set<StockCount>();
    public DbSet<StockCountLine> StockCountLines => Set<StockCountLine>();
    public DbSet<LoyaltyTransaction> LoyaltyTransactions => Set<LoyaltyTransaction>();
    public DbSet<LoyaltyTier> LoyaltyTiers => Set<LoyaltyTier>();
    public DbSet<LoyaltyCardScheme> LoyaltyCardSchemes => Set<LoyaltyCardScheme>();
    public DbSet<LoyaltyCardSchemeTier> LoyaltyCardSchemeTiers => Set<LoyaltyCardSchemeTier>();
    public DbSet<SalesBudget> SalesBudgets => Set<SalesBudget>();
    public DbSet<PrintJob> PrintJobs => Set<PrintJob>();
    public DbSet<TourOperatorCompany> TourOperatorCompanies => Set<TourOperatorCompany>();
    public DbSet<TourOperator> TourOperators => Set<TourOperator>();
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<GlAccount> GlAccounts => Set<GlAccount>();
    public DbSet<GlJournalEntry> GlJournalEntries => Set<GlJournalEntry>();
    public DbSet<GlJournalLine> GlJournalLines => Set<GlJournalLine>();
    public DbSet<GlExpense> GlExpenses => Set<GlExpense>();
    public DbSet<ApPayment> ApPayments => Set<ApPayment>();
    public DbSet<EventHall> EventHalls => Set<EventHall>();
    public DbSet<CateringPackage> CateringPackages => Set<CateringPackage>();
    public DbSet<CateringEvent> CateringEvents => Set<CateringEvent>();
    public DbSet<CateringEventItem> CateringEventItems => Set<CateringEventItem>();
    public DbSet<CateringEventPayment> CateringEventPayments => Set<CateringEventPayment>();

    public DbSet<ChargeType> ChargeTypes => Set<ChargeType>();
    public DbSet<Charge> Charges => Set<Charge>();
    public DbSet<ProductCharge> ProductCharges => Set<ProductCharge>();
    public DbSet<OrderItemCharge> OrderItemCharges => Set<OrderItemCharge>();
    public DbSet<UnitConversion> UnitConversions => Set<UnitConversion>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<SupplierGroup> SupplierGroups => Set<SupplierGroup>();
    public DbSet<SupplierType> SupplierTypes => Set<SupplierType>();
    public DbSet<ServingUnit> ServingUnits => Set<ServingUnit>();
    public DbSet<PrinterType> PrinterTypes => Set<PrinterType>();
    public DbSet<ProductSupplier> ProductSuppliers => Set<ProductSupplier>();
    public DbSet<ProductKitchenStation> ProductKitchenStations => Set<ProductKitchenStation>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>(e =>
        {
            e.ToTable("users");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.Email }).IsUnique();   // Postgres allows many NULLs
            e.HasIndex(x => new { x.TenantId, x.Username }).IsUnique();
            e.Property(x => x.Email).HasMaxLength(255);
            e.Property(x => x.Username).HasMaxLength(60);
            e.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
            e.Property(x => x.PhoneE164).HasMaxLength(20);
            e.Property(x => x.Role).HasConversion<int>();
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<Location>(e =>
        {
            e.ToTable("locations");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
            e.Property(x => x.Code).HasMaxLength(20).IsRequired();
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.AddressLine1).HasMaxLength(255).IsRequired();
            e.Property(x => x.AddressLine2).HasMaxLength(255);
            e.Property(x => x.City).HasMaxLength(100).IsRequired();
            e.Property(x => x.CountryCode).HasMaxLength(2).IsRequired();
            e.Property(x => x.TimeZone).HasMaxLength(64).IsRequired();
            e.Property(x => x.Currency).HasMaxLength(3).IsRequired();
            e.Property(x => x.PhoneE164).HasMaxLength(20);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<UnitOfMeasure>(e =>
        {
            e.ToTable("units_of_measure");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
            e.Property(x => x.Code).HasMaxLength(20).IsRequired();
            e.Property(x => x.Name).HasMaxLength(80).IsRequired();
            e.Property(x => x.Symbol).HasMaxLength(10);
            e.Property(x => x.Dimension).HasMaxLength(20).IsRequired();
            e.Property(x => x.FactorToBase).HasPrecision(18, 6);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<Tax>(e =>
        {
            e.ToTable("taxes");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
            e.Property(x => x.Code).HasMaxLength(20).IsRequired();
            e.Property(x => x.Name).HasMaxLength(80).IsRequired();
            e.Property(x => x.RatePercent).HasPrecision(8, 4);
            e.Property(x => x.ApplyOn).HasMaxLength(20).IsRequired();
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<Category>(e =>
        {
            e.ToTable("categories");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
            e.Property(x => x.Code).HasMaxLength(40).IsRequired();
            e.Property(x => x.Name).HasMaxLength(120).IsRequired();
            e.Property(x => x.ColorHex).HasMaxLength(7);
            e.Property(x => x.IconName).HasMaxLength(60);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<Product>(e =>
        {
            e.ToTable("products");

            e.HasKey(x => x.Id);

            e.HasIndex(x => new { x.TenantId, x.Sku })
                .IsUnique();

            e.HasIndex(x => x.LocationId);
            e.HasIndex(x => x.DepartmentId);
            e.HasIndex(x => x.CategoryId);

            e.Property(x => x.Sku).HasMaxLength(40).IsRequired();
            e.Property(x => x.Barcode).HasMaxLength(60);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.NameOnInvoice).HasMaxLength(200);
            e.Property(x => x.RefCode01).HasMaxLength(80).HasColumnName("ref_code_01");
            e.Property(x => x.RefCode02).HasMaxLength(80).HasColumnName("ref_code_02");
            e.Property(x => x.BasePrice).HasPrecision(15, 4);
            e.Property(x => x.CostPrice).HasPrecision(15, 4);
            e.Property(x => x.ReorderLevel).HasPrecision(15, 4);
            e.Property(x => x.ReorderQuantity).HasPrecision(18, 4);
            e.Property(x => x.ParLevel).HasPrecision(18, 4);
            e.Property(x => x.ColorHex).HasMaxLength(7);
            e.Property(x => x.ImageUrl).HasMaxLength(500);
            e.Property(x => x.IsAvailableOnline).HasDefaultValue(true);
            e.Property(x => x.TaxClass).HasMaxLength(16).IsRequired();
            e.Property(x => x.ProductType).HasMaxLength(30).IsRequired();
            e.Property(x => x.DiscountType).HasMaxLength(20).IsRequired();
            e.Property(x => x.DiscountValue).HasPrecision(18, 4);
            e.Property(x => x.MaxDiscountAmount).HasPrecision(18, 4);
            e.Property(x => x.MaxDiscountPercentage).HasPrecision(18, 4);

            e.HasOne<Location>()
                .WithMany()
                .HasForeignKey(x => x.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne<Department>()
                .WithMany()
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });
        b.Entity<ProductAvailabilityOverride>(e =>
        {
            e.ToTable("product_availability_overrides");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.LocationId, x.ProductId }).IsUnique();
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });
        b.Entity<ProductReplenishmentLevel>(e =>
        {
            e.ToTable("product_replenishment_levels");
            e.HasKey(x => x.Id);
            e.Property(x => x.ReorderLevel).HasPrecision(18, 4);
            e.Property(x => x.ParLevel).HasPrecision(18, 4);
            e.HasIndex(x => new { x.TenantId, x.ProductId, x.LocationId });
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });
        // Configurable approval workflows (#approvals)
        b.Entity<ApprovalRule>(e =>
        {
            e.ToTable("approval_rules"); e.HasKey(x => x.Id);
            e.Ignore(x => x.Steps);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });
        b.Entity<ApprovalRuleStep>(e =>
        {
            e.ToTable("approval_rule_steps"); e.HasKey(x => x.Id);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });
        b.Entity<ApprovalRequest>(e =>
        {
            e.ToTable("approval_requests"); e.HasKey(x => x.Id);
            e.Ignore(x => x.Actions);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });
        b.Entity<ApprovalAction>(e =>
        {
            e.ToTable("approval_actions"); e.HasKey(x => x.Id);
            e.HasIndex(x => x.Token).IsUnique();
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });
        b.Entity<KitchenStation>(entity =>
        {
            entity.ToTable("kitchen_stations");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Code)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.Name)
                .HasMaxLength(80)
                .IsRequired();

            entity.Property(x => x.PrinterName)
                .HasMaxLength(120);

            entity.HasIndex(x => new { x.TenantId, x.LocationId, x.Code })
                .IsUnique();

            entity.HasIndex(x => new { x.TenantId, x.LocationId });

            entity.HasOne(x => x.PrinterType)
                .WithMany()
                .HasForeignKey(x => x.PrinterTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<PrinterType>(entity =>
        {
            entity.ToTable("printer_types");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Code)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.Name)
                .HasMaxLength(80)
                .IsRequired();

            entity.HasIndex(x => new { x.TenantId, x.Code })
                .IsUnique();

            entity.HasIndex(x => x.TenantId);
        });

        b.Entity<ProductVariant>(e =>
        {
            e.ToTable("product_variants");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.ProductId, x.Code }).IsUnique();
            e.HasIndex(x => x.ProductId);
            e.Property(x => x.Code).HasMaxLength(20).IsRequired();
            e.Property(x => x.Name).HasMaxLength(60).IsRequired();
            e.Property(x => x.Price).HasPrecision(18, 4);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);

            e.HasOne<ServingUnit>()
                .WithMany()
                .HasForeignKey(x => x.ServingUnitId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        b.Entity<PriceLevel>(e =>
        {
            e.ToTable("price_levels");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
            e.Property(x => x.Code).HasMaxLength(20).IsRequired();
            e.Property(x => x.Name).HasMaxLength(60).IsRequired();
            e.Property(x => x.AppliesToOrderType).HasMaxLength(20);
            e.HasIndex(x => new { x.TenantId, x.LocationId, x.Code })
            .IsUnique();
            e.HasIndex(x => new { x.TenantId, x.LocationId });
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<ProductPrice>(e =>
        {
            e.ToTable("product_prices");

            e.HasKey(x => x.Id);

            e.HasIndex(x => new
            {
                x.TenantId,
                x.LocationId,
                x.ProductId,
                x.ProductVariantId,
                x.PriceLevelId
            }).IsUnique();

            e.HasIndex(x => x.ProductId);
            e.HasIndex(x => x.ProductVariantId);
            e.HasIndex(x => x.PriceLevelId);
            e.HasIndex(x => x.LocationId);

            e.Property(x => x.CostPrice).HasPrecision(18, 4);
            e.Property(x => x.Price).HasPrecision(18, 4);

            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<Promotion>(e =>
        {
            e.ToTable("promotions");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
            e.Property(x => x.Code).HasMaxLength(30).IsRequired();
            e.Property(x => x.Name).HasMaxLength(120).IsRequired();
            e.Property(x => x.PromoType).HasMaxLength(20).IsRequired();
            e.Property(x => x.AppliesToOrderType).HasMaxLength(20);
            e.Property(x => x.DisplayMessage).HasMaxLength(160);
            e.HasMany(x => x.Lines).WithOne().HasForeignKey(l => l.PromotionId);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<PromotionLine>(e =>
        {
            e.ToTable("promotion_lines");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.PromotionId);
            foreach (var p in new[] { "MinQty", "GetQty" }) e.Property(p).HasPrecision(15, 4);
            foreach (var p in new[] { "BillFrom", "BillTo", "DiscountAmount", "BundlePrice" }) e.Property(p).HasPrecision(18, 4);
            e.Property(x => x.DiscountPercent).HasPrecision(8, 4);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<OrderPromotion>(e =>
        {
            e.ToTable("order_promotions");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.OrderId);
            e.Property(x => x.Code).HasMaxLength(30).IsRequired();
            e.Property(x => x.Name).HasMaxLength(120).IsRequired();
            e.Property(x => x.DiscountAmount).HasPrecision(18, 4);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<RestaurantTable>(e =>
        {
            e.ToTable("restaurant_tables");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.LocationId, x.Code }).IsUnique();
            e.HasIndex(x => x.FloorId);
            e.Property(x => x.Code).HasMaxLength(20).IsRequired();
            e.Property(x => x.Name).HasMaxLength(60);
            e.Property(x => x.Area).HasMaxLength(40);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<Floor>(e =>
        {
            e.ToTable("floors");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.LocationId, x.Name }).IsUnique();
            e.Property(x => x.Name).HasMaxLength(60).IsRequired();
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<UserFloor>(e =>
        {
            e.ToTable("user_floors");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.UserId, x.FloorId }).IsUnique();
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<PushSubscription>(e =>
        {
            e.ToTable("push_subscriptions");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.Endpoint }).IsUnique();
            e.HasIndex(x => x.UserId);
            e.Property(x => x.Endpoint).HasMaxLength(500).IsRequired();
            e.Property(x => x.P256dh).HasMaxLength(300).IsRequired();
            e.Property(x => x.Auth).HasMaxLength(300).IsRequired();
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<DeviceToken>(e =>
        {
            e.ToTable("device_tokens");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.Token }).IsUnique();
            e.HasIndex(x => x.UserId);
            e.Property(x => x.Token).HasMaxLength(300).IsRequired();
            e.Property(x => x.Platform).HasMaxLength(10).IsRequired();
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<TabDevice>(e =>
        {
            e.ToTable("tab_devices");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.IsActive });
            e.Property(x => x.Name).HasMaxLength(120).IsRequired();
            e.Property(x => x.TokenHash).HasMaxLength(128).IsRequired();
            e.Property(x => x.Fingerprint).HasMaxLength(200);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<Reservation>(e =>
        {
            e.ToTable("reservations");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.LocationId);
            e.HasIndex(x => x.ReservedAt);
            e.Property(x => x.CustomerName).HasMaxLength(120).IsRequired();
            e.Property(x => x.Phone).HasMaxLength(40);
            e.Property(x => x.Status).HasMaxLength(20).IsRequired();
            e.Property(x => x.Notes).HasMaxLength(300);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<CustomerCategory>(e =>
        {
            e.ToTable("customer_categories");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
            e.Property(x => x.Code).HasMaxLength(40).IsRequired();
            e.Property(x => x.Name).HasMaxLength(120).IsRequired();
            e.Property(x => x.DiscountPercent).HasPrecision(8, 4);
            e.Property(x => x.Notes).HasMaxLength(300);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<Customer>(e =>
        {
            e.ToTable("customers");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
            e.Property(x => x.Code).HasMaxLength(40).IsRequired();
            e.Property(x => x.Name).HasMaxLength(160).IsRequired();
            e.Property(x => x.Phone).HasMaxLength(40);
            e.Property(x => x.Email).HasMaxLength(160);
            e.Property(x => x.Address).HasMaxLength(300);
            e.Property(x => x.TaxNo).HasMaxLength(40);
            e.Property(x => x.DiscountPercent).HasPrecision(8, 4);
            e.Property(x => x.CreditLimit).HasPrecision(15, 4);
            e.Property(x => x.CurrentBalance).HasPrecision(15, 4);
            e.Property(x => x.LoyaltyPoints).HasPrecision(15, 4);
            e.Property(x => x.LoyaltyLifetimePoints).HasPrecision(15, 4);
            e.Property(x => x.Notes).HasMaxLength(300);
            e.HasIndex(x => x.LoyaltyCardSchemeId);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<LoyaltyTransaction>(e =>
        {
            e.ToTable("loyalty_transactions");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.CustomerId);
            e.Property(x => x.TxnType).HasMaxLength(20).IsRequired();
            e.Property(x => x.Points).HasPrecision(15, 4);
            e.Property(x => x.BalanceAfter).HasPrecision(15, 4);
            e.Property(x => x.Note).HasMaxLength(200);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<LoyaltyTier>(e =>
        {
            e.ToTable("loyalty_tiers");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.TenantId);
            e.Property(x => x.Name).HasMaxLength(60).IsRequired();
            e.Property(x => x.MinLifetimePoints).HasPrecision(15, 4);
            e.Property(x => x.EarnMultiplier).HasPrecision(8, 4);
            e.Property(x => x.DiscountPercent).HasPrecision(8, 4);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<LoyaltyCardScheme>(e =>
        {
            e.ToTable("loyalty_card_schemes");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
            e.Property(x => x.Code).HasMaxLength(40).IsRequired();
            e.Property(x => x.Name).HasMaxLength(160).IsRequired();
            e.Property(x => x.Description).HasMaxLength(300);
            e.Property(x => x.Type).HasMaxLength(20).IsRequired();
            e.Property(x => x.DiscountPercent).HasPrecision(8, 4);
            e.HasMany(x => x.Tiers).WithOne().HasForeignKey(x => x.SchemeId).OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<LoyaltyCardSchemeTier>(e =>
        {
            e.ToTable("loyalty_card_scheme_tiers");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.SchemeId);
            e.Property(x => x.BillFromValue).HasPrecision(15, 4);
            e.Property(x => x.BillToValue).HasPrecision(15, 4);
            e.Property(x => x.Increment).HasPrecision(15, 4);
            e.Property(x => x.Points).HasPrecision(15, 4);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<StockCount>(e =>
        {
            e.ToTable("stock_counts");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.LocationId);
            e.Property(x => x.Status).HasMaxLength(20).IsRequired();
            e.Property(x => x.CountNumber).HasMaxLength(30);
            e.Property(x => x.Notes).HasMaxLength(300);
            e.HasMany(x => x.Lines).WithOne().HasForeignKey(l => l.StockCountId);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });
        b.Entity<StockCountLine>(e =>
        {
            e.ToTable("stock_count_lines");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.StockCountId);
            e.Property(x => x.SystemQty).HasPrecision(15, 4);
            e.Property(x => x.CountedQty).HasPrecision(15, 4);
            e.Property(x => x.Variance).HasPrecision(15, 4);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<ActivityLog>(e =>
        {
            e.ToTable("activity_log");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.CreatedAt });
            e.HasIndex(x => new { x.TenantId, x.Action });
            e.Property(x => x.Action).HasMaxLength(60).IsRequired();
            e.Property(x => x.ActorName).HasMaxLength(160);
            e.Property(x => x.ActorRole).HasMaxLength(40);
            e.Property(x => x.EntityType).HasMaxLength(40);
            e.Property(x => x.Summary).HasMaxLength(400);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<RolePermission>(e =>
        {
            e.ToTable("role_permissions");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.Role }).IsUnique();
            e.Property(x => x.MaxDiscountPercent).HasPrecision(8, 4);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<PrintJob>(e =>
        {
            e.ToTable("print_jobs");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.LocationId, x.Status, x.CreatedAt });
            e.Property(x => x.Kind).HasMaxLength(20).IsRequired();
            e.Property(x => x.Status).HasMaxLength(20).IsRequired();
            e.Property(x => x.PrinterName).HasMaxLength(80);
            e.Property(x => x.Error).HasMaxLength(300);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<SalesBudget>(e =>
        {
            e.ToTable("sales_budgets");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.LocationId, x.PeriodMonth });
            e.Property(x => x.Amount).HasPrecision(18, 4);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<RoleScreenAccess>(e =>
        {
            e.ToTable("role_screen_access");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.Role, x.Screen });
            e.Property(x => x.Screen).HasMaxLength(40).IsRequired();
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<CustomerProductPrice>(e =>
        {
            e.ToTable("customer_product_prices");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.CustomerId);
            e.HasIndex(x => x.CategoryId);
            e.Property(x => x.Price).HasPrecision(18, 4);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<CustomerPayment>(e =>
        {
            e.ToTable("customer_payments");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.CustomerId);
            e.Property(x => x.Amount).HasPrecision(15, 4);
            e.Property(x => x.PayType).HasMaxLength(20).IsRequired();
            e.Property(x => x.Reference).HasMaxLength(120);
            e.Property(x => x.Notes).HasMaxLength(300);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<ProductStock>(e =>
        {
            e.ToTable("product_stock");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.ProductId, x.LocationId }).IsUnique();
            e.Property(x => x.QuantityOnHand).HasPrecision(15, 4);
            e.Property(x => x.AverageCost).HasPrecision(15, 4);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<Order>(e =>
        {
            e.ToTable("orders");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.OrderNumber }).IsUnique();
            e.Property(x => x.OrderNumber).HasMaxLength(20).IsRequired();
            e.Property(x => x.OrderType).HasMaxLength(20).IsRequired();
            e.Property(x => x.OrderSource).HasMaxLength(20).IsRequired();
            e.Property(x => x.ExternalOrderId).HasMaxLength(100);
            e.Property(x => x.TableLabel).HasMaxLength(40);
            e.Property(x => x.Status).HasMaxLength(20).IsRequired();
            e.Property(x => x.VoidReason).HasMaxLength(200);
            foreach (var p in new[] { "SubtotalAmount", "DiscountAmount", "PromotionDiscountAmount", "ServiceChargeAmount", "TaxAmount", "TipAmount", "TotalAmount", "TourCommissionAmount" })
                e.Property(p).HasPrecision(15, 4);
            e.HasMany(x => x.Items).WithOne().HasForeignKey(i => i.OrderId);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<OrderItem>(e =>
        {
            e.ToTable("order_items");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.OrderId);
            e.Property(x => x.Sku).HasMaxLength(40).IsRequired();
            e.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
            e.Property(x => x.VariantName).HasMaxLength(60);
            e.Property(x => x.Station).HasMaxLength(20).IsRequired();
            e.Property(x => x.Notes).HasMaxLength(500);
            e.Property(x => x.KotStatus).HasMaxLength(20).IsRequired();
            foreach (var p in new[] { "Quantity", "UnitPrice", "LineSubtotal", "TaxAmount", "LineTotal" })
                e.Property(p).HasPrecision(15, 4);
            e.Property(x => x.TaxRate).HasPrecision(8, 4);
            e.HasMany(x => x.Modifiers).WithOne().HasForeignKey(m => m.OrderItemId);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<KitchenTicket>(e =>
        {
            e.ToTable("kitchen_tickets");
            e.HasKey(x => x.Id);
            e.Property(x => x.TicketNumber).HasMaxLength(20).IsRequired();
            e.Property(x => x.Station).HasMaxLength(20).IsRequired();
            e.Property(x => x.OrderLabel).HasMaxLength(60).IsRequired();
            e.Property(x => x.OrderSource).HasMaxLength(20).IsRequired();
            e.Property(x => x.Status).HasMaxLength(20).IsRequired();
            e.Property(x => x.ItemsJson).HasColumnType("text").IsRequired();
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<Payment>(e =>
        {
            e.ToTable("payments");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.OrderId);
            e.Property(x => x.PayType).HasMaxLength(30).IsRequired();
            e.Property(x => x.Amount).HasPrecision(15, 4);
            e.Property(x => x.Reference).HasMaxLength(100);
            e.Property(x => x.CurrencyCode).HasMaxLength(3);
            e.Property(x => x.FxRate).HasPrecision(18, 8);
            e.Property(x => x.BaseAmount).HasPrecision(15, 4);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<TourOperatorCompany>(e =>
        {
            e.ToTable("tour_operator_companies");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
            e.Property(x => x.Code).HasMaxLength(40).IsRequired();
            e.Property(x => x.Name).HasMaxLength(160).IsRequired();
            e.Property(x => x.CountryCode).HasMaxLength(2);
            e.Property(x => x.CommissionPercent).HasPrecision(8, 4);
            e.Property(x => x.CommissionAmount).HasPrecision(18, 2);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<TourOperator>(e =>
        {
            e.ToTable("tour_operators");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
            e.Property(x => x.Code).HasMaxLength(40).IsRequired();
            e.Property(x => x.Name).HasMaxLength(160).IsRequired();
            e.Property(x => x.CountryCode).HasMaxLength(2);
            e.Property(x => x.CommissionPercent).HasPrecision(8, 4);
            e.Property(x => x.Amount).HasPrecision(18, 2);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<Currency>(e =>
        {
            e.ToTable("currencies");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
            e.Property(x => x.Code).HasMaxLength(3).IsRequired();
            e.Property(x => x.Name).HasMaxLength(60).IsRequired();
            e.Property(x => x.Symbol).HasMaxLength(8);
            e.Property(x => x.RateToBase).HasPrecision(18, 8);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        // ── GL / accounting (#73) ──
        b.Entity<GlAccount>(e =>
        {
            e.ToTable("gl_accounts");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
            e.Property(x => x.Code).HasMaxLength(20).IsRequired();
            e.Property(x => x.Name).HasMaxLength(160).IsRequired();
            e.Property(x => x.AccountType).HasMaxLength(20).IsRequired();
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<GlJournalEntry>(e =>
        {
            e.ToTable("gl_journal_entries");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.EntryNo }).IsUnique();
            e.Property(x => x.EntryNo).HasMaxLength(30).IsRequired();
            e.Property(x => x.Memo).HasMaxLength(300);
            e.Property(x => x.Source).HasMaxLength(20).IsRequired();
            e.Property(x => x.SourceRef).HasMaxLength(60);
            e.Property(x => x.Status).HasMaxLength(20).IsRequired();
            e.HasMany(x => x.Lines).WithOne().HasForeignKey(l => l.EntryId);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<GlJournalLine>(e =>
        {
            e.ToTable("gl_journal_lines");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.EntryId);
            e.Property(x => x.AccountCode).HasMaxLength(20).IsRequired();
            e.Property(x => x.AccountName).HasMaxLength(160).IsRequired();
            e.Property(x => x.Debit).HasPrecision(15, 4);
            e.Property(x => x.Credit).HasPrecision(15, 4);
            e.Property(x => x.LineMemo).HasMaxLength(300);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<GlExpense>(e =>
        {
            e.ToTable("gl_expenses");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.ExpenseNo }).IsUnique();
            e.Property(x => x.ExpenseNo).HasMaxLength(30).IsRequired();
            e.Property(x => x.Amount).HasPrecision(15, 4);
            e.Property(x => x.Payee).HasMaxLength(160);
            e.Property(x => x.PaymentMethod).HasMaxLength(30);
            e.Property(x => x.Memo).HasMaxLength(300);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<ApPayment>(e =>
        {
            e.ToTable("ap_payments");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.PaymentNo }).IsUnique();
            e.Property(x => x.PaymentNo).HasMaxLength(30).IsRequired();
            e.Property(x => x.Amount).HasPrecision(15, 4);
            e.Property(x => x.Reference).HasMaxLength(100);
            e.Property(x => x.Memo).HasMaxLength(300);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        // ── Catering / banquet (#75) ──
        b.Entity<EventHall>(e =>
        {
            e.ToTable("event_halls");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
            e.Property(x => x.Code).HasMaxLength(40).IsRequired();
            e.Property(x => x.Name).HasMaxLength(160).IsRequired();
            e.Property(x => x.Notes).HasMaxLength(300);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<CateringPackage>(e =>
        {
            e.ToTable("catering_packages");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
            e.Property(x => x.Code).HasMaxLength(40).IsRequired();
            e.Property(x => x.Name).HasMaxLength(160).IsRequired();
            e.Property(x => x.PricePerHead).HasPrecision(15, 4);
            e.Property(x => x.Description).HasMaxLength(500);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<CateringEvent>(e =>
        {
            e.ToTable("catering_events");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.EventNo }).IsUnique();
            e.Property(x => x.EventNo).HasMaxLength(30).IsRequired();
            e.Property(x => x.Title).HasMaxLength(200);
            e.Property(x => x.CustomerName).HasMaxLength(160);
            e.Property(x => x.CustomerPhone).HasMaxLength(40);
            e.Property(x => x.Status).HasMaxLength(20).IsRequired();
            e.Property(x => x.DeliveryAddress).HasMaxLength(300);
            e.Property(x => x.Vehicle).HasMaxLength(80);
            e.Property(x => x.Driver).HasMaxLength(120);
            e.Property(x => x.DispatchStatus).HasMaxLength(20);
            e.Property(x => x.Notes).HasMaxLength(500);
            foreach (var p in new[] { "PricePerHead", "PackageTotal", "ExtrasTotal", "DiscountAmount", "ServiceCharge", "TaxAmount", "TotalAmount", "PaidAmount", "FoodCost" })
                e.Property(p).HasPrecision(15, 4);
            e.HasMany(x => x.Items).WithOne().HasForeignKey(i => i.EventId);
            e.HasMany(x => x.Payments).WithOne().HasForeignKey(p => p.EventId);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<CateringEventItem>(e =>
        {
            e.ToTable("catering_event_items");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.EventId);
            e.Property(x => x.Description).HasMaxLength(200).IsRequired();
            foreach (var p in new[] { "Quantity", "UnitPrice", "LineTotal" })
                e.Property(p).HasPrecision(15, 4);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<CateringEventPayment>(e =>
        {
            e.ToTable("catering_event_payments");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.EventId);
            e.Property(x => x.Amount).HasPrecision(15, 4);
            e.Property(x => x.PayType).HasMaxLength(30).IsRequired();
            e.Property(x => x.Kind).HasMaxLength(20).IsRequired();
            e.Property(x => x.Reference).HasMaxLength(100);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<Hms.Api.Features.Orders.OrderCounter>(e =>
        {
            e.ToTable("order_counters");
            e.HasKey(x => new { x.TenantId, x.CounterDate });
            e.HasQueryFilter(x => x.TenantId == _tenantId);
        });

        b.Entity<OrderCharge>(e =>
        {
            e.ToTable("order_charges");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.OrderId);
            e.Property(x => x.Code).HasMaxLength(20).IsRequired();
            e.Property(x => x.Name).HasMaxLength(80).IsRequired();
            e.Property(x => x.ChargeType).HasMaxLength(20).IsRequired();
            e.Property(x => x.RatePercent).HasPrecision(8, 4);
            e.Property(x => x.BaseAmount).HasPrecision(15, 4);
            e.Property(x => x.ChargeAmount).HasPrecision(15, 4);
            e.HasQueryFilter(x => x.TenantId == _tenantId);
        });

        b.Entity<OrgSettings>(e =>
        {
            e.ToTable("org_settings");
            e.HasKey(x => x.TenantId);
            e.Property(x => x.InvoicePrefix).HasMaxLength(10).IsRequired();
            e.Property(x => x.BaseCurrency).HasMaxLength(3).IsRequired();
            e.Property(x => x.VatFilingFrequency).HasMaxLength(20).IsRequired();
            foreach (var p in new[] { "OrderPrefix", "PoPrefix", "GrnPrefix", "SupplierPrefix",
                         "TransferPrefix", "WastagePrefix", "AdjustmentPrefix", "ProductionPrefix", "ShiftPrefix" })
                e.Property(p).HasMaxLength(10).IsRequired();
            e.Property(x => x.SvatNumber).HasMaxLength(40);
            e.Property(x => x.PoApprovalThreshold).HasPrecision(15, 4);
            e.HasQueryFilter(x => x.TenantId == _tenantId);
        });

        b.Entity<InvoiceSeries>(e =>
        {
            e.ToTable("invoice_series");
            e.HasKey(x => new { x.TenantId, x.SeriesYear });
            e.HasQueryFilter(x => x.TenantId == _tenantId);
        });

        b.Entity<AggregatorOutbox>(e =>
        {
            e.ToTable("aggregator_outbox");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.Status });
            e.Property(x => x.Aggregator).HasMaxLength(20).IsRequired();
            e.Property(x => x.ExternalOrderId).HasMaxLength(100).IsRequired();
            e.Property(x => x.Operation).HasMaxLength(40).IsRequired();
            e.Property(x => x.Status).HasMaxLength(20).IsRequired();
            e.Property(x => x.LastError).HasMaxLength(2000);
            e.Property(x => x.PayloadJson).HasColumnType("text").IsRequired();
            e.HasQueryFilter(x => x.TenantId == _tenantId);
        });

        b.Entity<AggregatorCredential>(e =>
        {
            e.ToTable("aggregator_credentials");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.Aggregator }).IsUnique();
            e.Property(x => x.Aggregator).HasMaxLength(20).IsRequired();
            e.Property(x => x.ClientId).HasMaxLength(255);
            e.Property(x => x.ClientSecretEnc).HasColumnType("text");
            e.Property(x => x.WebhookSecretEnc).HasColumnType("text");
            e.Property(x => x.Environment).HasMaxLength(20).IsRequired();
            e.Property(x => x.BaseUrl).HasMaxLength(255);
            e.HasQueryFilter(x => x.TenantId == _tenantId);
        });

        b.Entity<LocationAggregatorMap>(e =>
        {
            e.ToTable("location_aggregator_map");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.LocationId, x.Aggregator }).IsUnique();
            e.Property(x => x.Aggregator).HasMaxLength(20).IsRequired();
            e.Property(x => x.ExternalStoreId).HasMaxLength(120);
            e.Property(x => x.ApiKeyEnc).HasColumnType("text");
            e.HasQueryFilter(x => x.TenantId == _tenantId);
        });

        b.Entity<Supplier>(e =>
        {
            e.ToTable("suppliers");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
            e.Property(x => x.Code).HasMaxLength(30).IsRequired();
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.ContactName).HasMaxLength(120);
            e.Property(x => x.Phone).HasMaxLength(40);
            e.Property(x => x.Email).HasMaxLength(255);
            e.Property(x => x.Address).HasMaxLength(500);
            e.Property(x => x.VatRegistrationNumber).HasMaxLength(40);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<PurchaseOrder>(e =>
        {
            e.ToTable("purchase_orders");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.PoNumber }).IsUnique();
            e.Property(x => x.PoNumber).HasMaxLength(20).IsRequired();
            e.Property(x => x.Status).HasMaxLength(20).IsRequired();
            e.Property(x => x.Notes).HasMaxLength(500);
            e.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
            e.Property(x => x.DeliveryAddress).HasMaxLength(500);
            e.Property(x => x.ReferenceNo).HasMaxLength(50);
            e.Property(x => x.PaymentMethod).HasMaxLength(30);
            e.Property(x => x.RejectReason).HasMaxLength(200);
            foreach (var p in new[] { "SubtotalAmount", "DiscountAmount", "Deductions", "OtherCharges", "TaxAmount", "TotalAmount" })
                e.Property(p).HasPrecision(15, 4);
            e.Property(x => x.CurrencyRate).HasPrecision(18, 6);
            e.HasMany(x => x.Lines).WithOne().HasForeignKey(l => l.PurchaseOrderId);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<PurchaseOrderLine>(e =>
        {
            e.ToTable("purchase_order_lines");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.PurchaseOrderId);
            e.Property(x => x.Sku).HasMaxLength(40).IsRequired();
            e.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
            e.Property(x => x.UnitSymbol).HasMaxLength(10);
            foreach (var p in new[] { "QuantityOrdered", "QuantityReceived", "UnitCost", "LineTotal", "TaxAmount", "DiscountAmount" })
                e.Property(p).HasPrecision(15, 4);
            e.Property(x => x.TaxRate).HasPrecision(8, 4);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<GoodsReceivedNote>(e =>
        {
            e.ToTable("goods_received_notes");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.GrnNumber }).IsUnique();
            e.Property(x => x.GrnNumber).HasMaxLength(20).IsRequired();
            e.Property(x => x.Status).HasMaxLength(20).IsRequired();
            e.Property(x => x.Notes).HasMaxLength(500);
            e.Property(x => x.SupplierInvoiceNo).HasMaxLength(50);
            e.Property(x => x.VoidReason).HasMaxLength(200);
            e.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
            e.Property(x => x.PaymentMethod).HasMaxLength(30);
            e.Property(x => x.ReferenceNo).HasMaxLength(50);
            e.Property(x => x.RejectReason).HasMaxLength(200);
            foreach (var p in new[] { "TotalCost", "TaxAmount", "DiscountAmount", "OtherCharges" })
                e.Property(p).HasPrecision(15, 4);
            e.Property(x => x.CurrencyRate).HasPrecision(18, 6);
            e.HasMany(x => x.Lines).WithOne().HasForeignKey(l => l.GrnId);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<GrnLine>(e =>
        {
            e.ToTable("grn_lines");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.GrnId);
            e.Property(x => x.Sku).HasMaxLength(40).IsRequired();
            e.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
            e.Property(x => x.BatchNo).HasMaxLength(50);
            e.Property(x => x.UnitSymbol).HasMaxLength(10);
            foreach (var p in new[] { "QuantityReceived", "UnitCost", "LineTotal", "TaxAmount",
                         "FreeQuantity", "DiscountAmount", "StockQuantity" })
                e.Property(p).HasPrecision(15, 4);
            e.Property(x => x.TaxRate).HasPrecision(8, 4);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });
        b.Entity<PurchaseReturn>(e =>
        {
            e.ToTable("purchase_returns");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.PrnNumber }).IsUnique();
            e.Property(x => x.PrnNumber).HasMaxLength(20).IsRequired();
            e.Property(x => x.Status).HasMaxLength(20).IsRequired();
            e.Property(x => x.Reason).HasMaxLength(200);
            e.Property(x => x.SupplierCreditNo).HasMaxLength(50);
            e.Property(x => x.VoidReason).HasMaxLength(200);
            e.Property(x => x.Notes).HasMaxLength(500);
            e.Property(x => x.TotalCost).HasPrecision(15, 4);
            e.Property(x => x.TaxAmount).HasPrecision(15, 4);
            e.HasMany(x => x.Lines).WithOne().HasForeignKey(l => l.ReturnId);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });
        b.Entity<PurchaseReturnLine>(e =>
        {
            e.ToTable("purchase_return_lines");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.ReturnId);
            e.Property(x => x.Sku).HasMaxLength(40).IsRequired();
            e.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
            e.Property(x => x.UnitSymbol).HasMaxLength(10);
            e.Property(x => x.BatchNo).HasMaxLength(50);
            foreach (var p in new[] { "Quantity", "UnitCost", "LineTotal", "TaxAmount", "StockQuantity" })
                e.Property(p).HasPrecision(15, 4);
            e.Property(x => x.TaxRate).HasPrecision(8, 4);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });
        b.Entity<ModifierGroup>(e =>
        {
            e.ToTable("modifier_groups");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(120).IsRequired();
            e.HasMany(x => x.Items).WithOne().HasForeignKey(i => i.GroupId);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });
        b.Entity<ModifierItem>(e =>
        {
            e.ToTable("modifier_items");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.GroupId);
            e.Property(x => x.Name).HasMaxLength(120).IsRequired();
            e.Property(x => x.PriceDelta).HasPrecision(15, 4);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });
        b.Entity<ProductModifierGroup>(e =>
        {
            e.ToTable("product_modifier_groups");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.ProductId, x.GroupId }).IsUnique();
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });
        b.Entity<OrderItemModifier>(e =>
        {
            e.ToTable("order_item_modifiers");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.OrderItemId);
            e.Property(x => x.Name).HasMaxLength(120).IsRequired();
            e.Property(x => x.PriceDelta).HasPrecision(15, 4);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<DocumentCounter>(e =>
        {
            e.ToTable("document_counters");
            e.HasKey(x => new { x.TenantId, x.DocType });
            e.Property(x => x.DocType).HasMaxLength(20).IsRequired();
            e.HasQueryFilter(x => x.TenantId == _tenantId);
        });

        b.Entity<StockTransfer>(e =>
        {
            e.ToTable("stock_transfers");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.TransferNumber }).IsUnique();
            e.Property(x => x.TransferNumber).HasMaxLength(20).IsRequired();
            e.Property(x => x.Status).HasMaxLength(20).IsRequired();
            e.Property(x => x.ReferenceNo).HasMaxLength(100);
            e.Property(x => x.Notes).HasMaxLength(500);
            e.Property(x => x.TotalCost).HasPrecision(15, 4);
            e.HasMany(x => x.Lines).WithOne().HasForeignKey(l => l.TransferId);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });
        b.Entity<StockTransferLine>(e =>
        {
            e.ToTable("stock_transfer_lines");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.TransferId);
            e.Property(x => x.Sku).HasMaxLength(40).IsRequired();
            e.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
            foreach (var p in new[] { "Quantity", "UnitCost", "LineTotal" }) e.Property(p).HasPrecision(15, 4);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<WastageNote>(e =>
        {
            e.ToTable("wastage_notes");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.WastageNumber }).IsUnique();
            e.Property(x => x.WastageNumber).HasMaxLength(20).IsRequired();
            e.Property(x => x.Reason).HasMaxLength(30).IsRequired();
            e.Property(x => x.Status).HasMaxLength(20).IsRequired();
            e.Property(x => x.Notes).HasMaxLength(500);
            e.Property(x => x.RejectReason).HasMaxLength(200);
            e.Property(x => x.TotalCost).HasPrecision(15, 4);
            e.HasMany(x => x.Lines).WithOne().HasForeignKey(l => l.WastageId);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });
        b.Entity<WastageLine>(e =>
        {
            e.ToTable("wastage_lines");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.WastageId);
            e.Property(x => x.Sku).HasMaxLength(40).IsRequired();
            e.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
            foreach (var p in new[] { "Quantity", "CurrentStock", "NewStock", "UnitCost", "LineTotal" }) e.Property(p).HasPrecision(15, 4);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<StockAdjustment>(e =>
        {
            e.ToTable("stock_adjustments");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.AdjustmentNumber }).IsUnique();
            e.Property(x => x.AdjustmentNumber).HasMaxLength(20).IsRequired();
            e.Property(x => x.Reason).HasMaxLength(30).IsRequired();
            e.Property(x => x.Status).HasMaxLength(20).IsRequired();
            e.Property(x => x.Notes).HasMaxLength(500);
            e.Property(x => x.RejectReason).HasMaxLength(200);
            e.Property(x => x.TotalValue).HasPrecision(15, 4);
            e.HasMany(x => x.Lines).WithOne().HasForeignKey(l => l.AdjustmentId);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });
        b.Entity<StockAdjustmentLine>(e =>
        {
            e.ToTable("stock_adjustment_lines");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.AdjustmentId);
            e.Property(x => x.Sku).HasMaxLength(40).IsRequired();
            e.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
            e.Property(x => x.AdjustmentType).HasMaxLength(10).IsRequired();
            foreach (var p in new[] { "QuantityDelta", "CurrentStock", "NewStock", "UnitCost", "LineTotal" }) e.Property(p).HasPrecision(15, 4);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<RequestNote>(e =>
        {
            e.ToTable("request_notes");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.RequestNumber }).IsUnique();
            e.Property(x => x.RequestNumber).HasMaxLength(20).IsRequired();
            e.Property(x => x.Mode).HasMaxLength(10).IsRequired();
            e.Property(x => x.Status).HasMaxLength(20).IsRequired();
            e.Property(x => x.CommonRemark).HasMaxLength(500);
            e.Property(x => x.RejectReason).HasMaxLength(200);
            e.HasMany(x => x.Lines).WithOne().HasForeignKey(l => l.RequestId);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });
        b.Entity<RequestNoteLine>(e =>
        {
            e.ToTable("request_note_lines");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.RequestId);
            e.Property(x => x.Sku).HasMaxLength(40).IsRequired();
            e.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
            e.Property(x => x.Remark).HasMaxLength(200);
            foreach (var p in new[] { "Sih", "Quantity" }) e.Property(p).HasPrecision(15, 4);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<Recipe>(e =>
        {
            e.ToTable("recipes");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.ProductId, x.OutputUnitId, x.LocationId }).IsUnique();
            e.HasIndex(x => x.LocationId);
            e.Property(x => x.YieldQuantity).HasPrecision(15, 4);
            e.Property(x => x.Notes).HasMaxLength(500);
            e.Property(x => x.TotalCost).HasPrecision(18, 4);
            e.HasMany(x => x.Lines).WithOne().HasForeignKey(l => l.RecipeId);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });
        b.Entity<RecipeLine>(e =>
        {
            e.ToTable("recipe_lines");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.RecipeId);
            e.Property(x => x.Sku).HasMaxLength(40).IsRequired();
            e.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
            e.Property(x => x.UnitSymbol).HasMaxLength(10);
            e.Property(x => x.Quantity).HasPrecision(15, 4);
            e.Property(x => x.CostPrice).HasPrecision(18, 4);
            e.Property(x => x.LineCost).HasPrecision(18, 4);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });
        b.Entity<ProductionOrder>(e =>
        {
            e.ToTable("production_orders");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.ProductionNumber }).IsUnique();
            e.Property(x => x.ProductionNumber).HasMaxLength(20).IsRequired();
            e.Property(x => x.DocumentNo).HasMaxLength(20);
            e.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
            e.Property(x => x.Status).HasMaxLength(20).IsRequired();
            e.Property(x => x.Notes).HasMaxLength(500);
            e.Property(x => x.VoidReason).HasMaxLength(200);
            foreach (var p in new[] { "Quantity", "TotalInputCost", "UnitCost" }) e.Property(p).HasPrecision(15, 4);
            e.HasMany(x => x.Consumptions).WithOne().HasForeignKey(c => c.ProductionOrderId);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });
        b.Entity<ProductionConsumption>(e =>
        {
            e.ToTable("production_consumptions");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.ProductionOrderId);
            e.Property(x => x.Sku).HasMaxLength(40).IsRequired();
            e.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
            foreach (var p in new[] { "QuantityConsumed", "UnitCost", "LineTotal" }) e.Property(p).HasPrecision(15, 4);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });
        b.Entity<Shift>(e =>
        {
            e.ToTable("shifts");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.ShiftNumber }).IsUnique();
            e.Property(x => x.ShiftNumber).HasMaxLength(20).IsRequired();
            e.Property(x => x.OpenedByName).HasMaxLength(200);
            e.Property(x => x.Status).HasMaxLength(20).IsRequired();
            e.Property(x => x.Notes).HasMaxLength(500);
            foreach (var p in new[] { "OpeningFloat", "DeclaredCash", "ExpectedCash", "CashVariance",
                         "TotalSales", "CashSales", "CardSales", "OtherSales" })
                e.Property(p).HasPrecision(15, 2);
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<ChargeType>(e =>
        {
            e.ToTable("charge_types");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
            e.Property(x => x.Code).HasMaxLength(20).IsRequired();
            e.Property(x => x.Name).HasMaxLength(80).IsRequired();
            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<Charge>(e =>
        {
            e.ToTable("charges");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
            e.HasIndex(x => x.ChargeTypeId);
            e.Property(x => x.Code).HasMaxLength(20).IsRequired();
            e.Property(x => x.Description).HasMaxLength(200).IsRequired();
            e.Property(x => x.Percentage).HasPrecision(8, 4);
            e.Property(x => x.Amount).HasPrecision(15, 4);

            e.HasOne(x => x.ChargeType)
                .WithMany()
                .HasForeignKey(x => x.ChargeTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<ProductCharge>(e =>
        {
            e.ToTable("product_charges");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.ProductId, x.ChargeId }).IsUnique();
            e.HasIndex(x => x.ProductId);
            e.HasIndex(x => x.ChargeId);

            e.HasOne(x => x.Charge)
                .WithMany()
                .HasForeignKey(x => x.ChargeId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<OrderItemCharge>(e =>
        {
            e.ToTable("order_item_charges");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.OrderItemId);
            e.Property(x => x.Code).HasMaxLength(20).IsRequired();
            e.Property(x => x.Description).HasMaxLength(200).IsRequired();
            e.Property(x => x.Percentage).HasPrecision(8, 4);
            e.Property(x => x.Amount).HasPrecision(15, 4);
            e.Property(x => x.BaseAmount).HasPrecision(15, 4);
            e.Property(x => x.ChargeAmount).HasPrecision(15, 4);

            // Navigation (not just a bare FK column) so EF's change tracker sees
            // the dependency and orders the insert after its parent OrderItem when
            // both are added within the same SaveChanges call (new order lines).
            e.HasOne(x => x.OrderItem)
                .WithMany()
                .HasForeignKey(x => x.OrderItemId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasQueryFilter(x => x.TenantId == _tenantId);
        });

        b.Entity<UnitConversion>(entity =>
        {
            entity.ToTable("unit_conversions");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.TenantId).HasColumnName("tenant_id");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.Property(x => x.CreatedBy).HasColumnName("created_by");
            entity.Property(x => x.UpdatedBy).HasColumnName("updated_by");
            entity.Property(x => x.IsDeleted).HasColumnName("is_deleted");

            entity.Property(x => x.UnitOfMeasureId)
                .HasColumnName("unit_of_measure_id");

            entity.Property(x => x.SubUnitOfMeasureId)
                .HasColumnName("sub_unit_of_measure_id");

            entity.Property(x => x.SubUnitValue)
                .HasColumnName("sub_unit_value")
                .HasPrecision(18, 6);

            entity.Property(x => x.BaseUnitValue)
                .HasColumnName("base_unit_value")
                .HasPrecision(18, 6);

            entity.HasOne(x => x.UnitOfMeasure)
                .WithMany()
                .HasForeignKey(x => x.UnitOfMeasureId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.SubUnitOfMeasure)
                .WithMany()
                .HasForeignKey(x => x.SubUnitOfMeasureId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new
            {
                x.TenantId,
                x.UnitOfMeasureId,
                x.SubUnitOfMeasureId
            })
            .IsUnique();
        });

        b.Entity<Department>(entity =>
        {
            entity.ToTable("departments");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.TenantId).HasColumnName("tenant_id");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.Property(x => x.CreatedBy).HasColumnName("created_by");
            entity.Property(x => x.UpdatedBy).HasColumnName("updated_by");
            entity.Property(x => x.IsDeleted).HasColumnName("is_deleted");

            entity.Property(x => x.Code)
                .HasColumnName("code")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.Remark)
                .HasColumnName("remark")
                .HasMaxLength(500);

            entity.Property(x => x.IsActive)
                .HasColumnName("is_active");

            entity.Property(x => x.LocationId)
                .HasColumnName("location_id");

            entity.Property(x => x.DashboardColor)
                .HasColumnName("dashboard_color")
                .HasMaxLength(20);

            entity.HasOne(x => x.Location)
                .WithMany()
                .HasForeignKey(x => x.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.TenantId, x.Code })
                .IsUnique()
                .HasFilter("is_deleted = false");
        });

        b.Entity<SupplierGroup>(entity =>
        {
            entity.ToTable("supplier_groups");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.TenantId).HasColumnName("tenant_id");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.Property(x => x.CreatedBy).HasColumnName("created_by");
            entity.Property(x => x.UpdatedBy).HasColumnName("updated_by");
            entity.Property(x => x.IsDeleted).HasColumnName("is_deleted");

            entity.Property(x => x.Code)
                .HasColumnName("code")
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.Remark)
                .HasColumnName("remark")
                .HasMaxLength(500);

            entity.Property(x => x.IsActive)
                .HasColumnName("is_active");

            entity.HasIndex(x => new { x.TenantId, x.Code })
                .IsUnique();
        });

        b.Entity<SupplierType>(entity =>
        {
            entity.ToTable("supplier_types");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.TenantId).HasColumnName("tenant_id");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.Property(x => x.CreatedBy).HasColumnName("created_by");
            entity.Property(x => x.UpdatedBy).HasColumnName("updated_by");
            entity.Property(x => x.IsDeleted).HasColumnName("is_deleted");

            entity.Property(x => x.Code)
                .HasColumnName("code")
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.Remark)
                .HasColumnName("remark")
                .HasMaxLength(500);

            entity.Property(x => x.IsActive)
                .HasColumnName("is_active");

            entity.HasIndex(x => new { x.TenantId, x.Code })
                .IsUnique();
        });

        b.Entity<ServingUnit>(entity =>
        {
            entity.ToTable("serving_units");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Code).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(60).IsRequired();

            entity.HasIndex(x => new { x.TenantId, x.Code })
                .IsUnique();
        });

        b.Entity<ProductSupplier>(e =>
        {
            e.ToTable("product_suppliers");

            e.HasKey(x => x.Id);

            e.HasIndex(x => new { x.TenantId, x.ProductId, x.SupplierId })
                .IsUnique();

            e.HasIndex(x => x.ProductId);
            e.HasIndex(x => x.SupplierId);

            e.HasOne(x => x.Supplier)
                .WithMany()
                .HasForeignKey(x => x.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });

        b.Entity<ProductKitchenStation>(e =>
        {
            e.ToTable("product_kitchen_stations");

            e.HasKey(x => x.Id);

            e.HasIndex(x => new { x.TenantId, x.ProductId, x.KitchenStationId, x.LocationId })
                .IsUnique();

            e.HasIndex(x => x.ProductId);
            e.HasIndex(x => x.KitchenStationId);
            e.HasIndex(x => x.LocationId);

            e.HasOne(x => x.KitchenStation)
                .WithMany()
                .HasForeignKey(x => x.KitchenStationId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasQueryFilter(x => x.TenantId == _tenantId && !x.IsDeleted);
        });
    }

    public override int SaveChanges()
    {
        StampTenantAndTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        StampTenantAndTimestamps();
        return base.SaveChangesAsync(ct);
    }

    private void StampTenantAndTimestamps()
    {
        var now = DateTime.UtcNow;
        foreach (var e in ChangeTracker.Entries<BaseEntity>())
        {
            if (e.State == EntityState.Added)
            {
                if (e.Entity.Id == Guid.Empty) e.Entity.Id = Guid.NewGuid();
                e.Entity.TenantId = _tenantId;
                e.Entity.CreatedAt = now;
                e.Entity.UpdatedAt = now;
            }
            else if (e.State == EntityState.Modified)
            {
                e.Entity.UpdatedAt = now;
                // Tenant ID must never change after creation.
                e.Property(nameof(BaseEntity.TenantId)).IsModified = false;
                e.Property(nameof(BaseEntity.CreatedAt)).IsModified = false;
            }
        }
    }
}

public interface ITenantDbContextFactory
{
    TenantDbContext Create(Guid tenantId);
    Task<TenantDbContext> CreateForCurrentAsync(CancellationToken ct = default);
}

/// <summary>
/// The request's tenant_id (from a JWT) doesn't resolve to a live tenant — e.g. an
/// old token after the tenant was recreated. Mapped to 401 (re-authenticate), not
/// a 500, so the client clears the stale session and sends the user to log in.
/// </summary>
public sealed class TenantNotFoundException(Guid tenantId)
    : Exception($"Tenant {tenantId} not found");

public class TenantDbContextFactory(
    ControlDbContext control,
    ITenantContext tenantContext,
    IConfiguration config) : ITenantDbContextFactory
{
    public TenantDbContext Create(Guid tenantId)
    {
        var tenant = control.Tenants.AsNoTracking().FirstOrDefault(t => t.Id == tenantId)
            ?? throw new TenantNotFoundException(tenantId);
        return BuildContext(tenant);
    }

    public async Task<TenantDbContext> CreateForCurrentAsync(CancellationToken ct = default)
    {
        var tenantId = tenantContext.TenantIdOrThrow();
        var tenant = await control.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tenantId, ct)
            ?? throw new TenantNotFoundException(tenantId);
        return BuildContext(tenant);
    }

    private TenantDbContext BuildContext(Tenant tenant)
    {
        var template = config.GetConnectionString("TenantTemplateDb")
            ?? throw new InvalidOperationException("TenantTemplateDb connection string missing");
        var connString = template.Replace("{tenant_db}", tenant.DatabaseName);

        var options = new DbContextOptionsBuilder<TenantDbContext>()
            .UseNpgsql(connString)
            .UseSnakeCaseNamingConvention()
            // Defence-in-depth: set app.tenant_id on every connection this context
            // opens so the FORCE'd Row-Level Security policies (migration 0051)
            // scope reads/writes to this tenant — even if the EF query filter is
            // bypassed. See TenantGucConnectionInterceptor.
            .AddInterceptors(new TenantGucConnectionInterceptor(tenant.Id))
            .Options;

        return new TenantDbContext(options, tenant.Id);
    }
}
