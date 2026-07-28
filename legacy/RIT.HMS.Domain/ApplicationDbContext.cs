using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RIT.HMS.Domain;
using RIT.HMS.Domain.Promotions;
using System.Security.Claims;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using RIT.HMS.Domain.Common;
using RIT.HMS.Domain.Transactions;
using RIT.HMS.Domain.Configurations;
using RIT.HMS.Domain.Logs;
using RIT.HMS.Domain.Loyalty;
using System.ComponentModel.DataAnnotations.Schema;
using RIT.HMS.Domain.ConnectionManager;
using System.Data.Entity.Infrastructure;
using RIT.HMS.Domain.Journal;
using RIT.HMS.Domain.ViewModels;

namespace RIT.HMS.Domain
{

    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {

            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            return userIdentity;
        }
    }

    // public  class ApplicationDbContext: DbContext IdentityDbContext<ApplicationUser>

    //IdentityDbContext
    public class ApplicationDbContext : DbContext
    {


        public ApplicationDbContext() : base("HMSContext")
        {

        }

        static string owinconnection = "";
        public ApplicationDbContext(string connectionstringname) : base(connectionstringname)
        {
            owinconnection = connectionstringname;
            this.Database.CommandTimeout = 6000;
            //((IObjectContextAdapter)this).ObjectContext.CommandTimeout = 6000;
        }

        public static ApplicationDbContext Create()
        {
            if (string.IsNullOrEmpty(owinconnection))
            {
                owinconnection = "Default";
            }

            return new ApplicationDbContext(owinconnection);

        }
        
        public DbSet<SysGroupOfCompany> SysGroupOfCompanys { get; set; }
        public DbSet<SysCompany> SysCompanies { get; set; }
        public DbSet<SysLocation> SysLocations { get; set; }

        public DbSet<SysLocationType> SysLocationTypes { get; set; }
        public DbSet<SysLocationMapper> SysLocationMappers { get; set; }

        public DbSet<InvGiftVoucherBookCode> InvGiftVoucherBookCodes { get; set; }



        public DbSet<InvGiftVoucherCancel> InvGiftVoucherCancels { get; set; }


        public DbSet<BudgetOutlet> BudgetOutlets { get; set; }
        public DbSet<BudgetItemWise> BudgetItemWises { get; set; }

       // public DbSet<InvGiftVoucherCancel> InvGiftVoucherCancels { get; set; }

        #region User Authentications
        public DbSet<SysUserFunction> SysUserFunctions { get; set; }
        public DbSet<SysUserGroup> SysUserGroups { get; set; }
        public DbSet<SysUserGroupPermission> SysUserGroupPermissions { get; set; }
        public DbSet<SysUserMaster> SysUserMasters { get; set; }
        public DbSet<SysUserPermission> SysUserPermissions { get; set; }

        // for subscriptions

        public DbSet<CompanyUser> CompanyUser { get; set; }

        #endregion

        #region Restaurant
        public DbSet<RstDepartment> RstDepartment { get; set; }
        public DbSet<RstCategory> RstDepartmentCategory { get; set; }
        public DbSet<RstSubCategory> RstDepartmentSubCategory { get; set; }
        public DbSet<RstKotCategory> RstKotCategory { get; set; }
        public DbSet<RstRoomMaster> RstRoomMaster { get; set; }
        public DbSet<RstRoomType> RstRoomType { get; set; }
        public DbSet<RstRoomTypeRate> RstRoomTypeRate { get; set; }
        public DbSet<RstPromotions> RstPromotions { get; set; }
        public DbSet<RstPromotionTypes> RstPromotionTypes { get; set; }
        public DbSet<ServingUnit> ServingUnit { get; set; }

        public DbSet<ChairMaster> ChairMaster { get; set; }
        public DbSet<TableMaster> TableMasters { get; set; }

        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<DeliveryPerson> DeliveryPerson { get; set; }
        public DbSet<UnitConversion> UnitConversion { get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<ProductKitchenMapper> ProductKitchenMapper { get; set; }
        public DbSet<Receipe> Receipe { get; set; }
        public DbSet<KitchenMaster> KitchenMaster { get; set; }

        // taxes
        public DbSet<ProductTax> ProductTax { get; set; }
        public DbSet<LocationTax> LocationTax { get; set; }
        public DbSet<CateringModeTax> CateringModeTax { get; set; }
        public DbSet<PayTypeTax> PayTypeTax { get; set; }
        // end taxes
        public DbSet<ProductServingUnit> ProductServingUnit { get; set; }
        public DbSet<ProductionNoteHeader> ProductionNoteHeader { get; set; }
        public DbSet<ProductionNoteDetail> ProductionNoteDetail { get; set; }
        public DbSet<RstMealType> RstMealType { get; set; }
        public DbSet<InterDepartment> InterDepartment { get; set; }
        public DbSet<RequestNoteHeader> RequestNoteHeader { get; set; }
        public DbSet<RequestNoteDetail> RequestNoteDetail { get; set; }

        public DbSet<InvRequestNotePOTransaction> InvRequestNotePOTransaction { get; set; }

        
        public DbSet<Addons> Addons { get; set; }
        public DbSet<RequestNoteAccptanceHeader> RequestNoteAccptanceHeader { get; set; }
        public DbSet<RequestNoteAcceptanceDetail> RequestNoteAcceptanceDetail { get; set; }
        public DbSet<KOTBOTDescription> KOTBOTDescription { get; set; }
        public DbSet<ProductInstruction> ProductInstruction { get; set; }
        public DbSet<AddonCategoryMaster> AddonCategoryMaster { get; set; }
        public DbSet<CustomProductionNoteHeader> CustomProductionNoteHeader { get; set; }
        public DbSet<CustomProductionNoteDetail> CustomProductionNoteDetail { get; set; }

        public DbSet<TourAgent> TourAgent { get; set; }
        public DbSet<TourAgentCompany> TourAgentCompanies { get; set; }
        //ll

        #endregion

        #region Open
        public DbSet<Customer> Customers { get; set; }
        public DbSet<CustomerCategory> CustomerCategorys { get; set; }
        public DbSet<CustomoerPreviousVisits> CustomoerPreviousVisitss { get; set; }
        public DbSet<CustomerDiscount> CustomerDiscount { get; set; }
        public DbSet<StewardsMaster> StewardsMastesrs { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<EmployeeGroup> EmployeeGroup { get; set; }
        public DbSet<SupplierType> SupplierType { get; set; }
        public DbSet<Event> Event { get; set; }
        public DbSet<EventProduct> EventProduct { get; set; }
        public DbSet<MonthEnd> MonthEnd { get; set; }
        public DbSet<SysYears> SysYears { get; set; }
        public DbSet<tmpMonthEnd> tmpMonthEnd { get; set; }

        #endregion

        #region Inventory

        public DbSet<DocStatus> DocStatus { get; set; }
        public DbSet<DocStatusChangeLog> DocStatusChangeLog { get; set; }
        public DbSet<InvSupplier> InvSupplier { get; set; }
        public DbSet<InvProductMaster> InvProductMaster { get; set; }
        public DbSet<UnitOfMeasure> UnitOfMeasure { get; set; }

        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<SupplierGroup> SupplierGroup { get; set; }

        public DbSet<PurchaseOrderHeader> PurchaseOrderHeader { get; set; }
        public DbSet<PurchaseOrderDetail> PurchaseOrderDetail { get; set; }
        public DbSet<ProductStockMaster> ProductStockMaster { get; set; }
        public DbSet<PurchaseHeader> PurchaseHeader { get; set; }
        public DbSet<PurchaseDetail> PurchaseDetail { get; set; }
        public DbSet<SupplierProduct> SupplierProduct { get; set; }
        public DbSet<TransferNoteHeader> TransferNoteHeader { get; set; }
        public DbSet<TransferNoteDetail> TransferNoteDetail { get; set; }
        public DbSet<PriceLevel> PriceLevel { get; set; }
        public DbSet<StockAdjustmentType> StockAdjustmentType { get; set; }
        public DbSet<StockAdjustmentHeader> StockAdjustmentHeader { get; set; }
        public DbSet<StockAdjustmentDetail> StockAdjustmentDetail { get; set; }
        public DbSet<JobHeader> JobHeader { get; set; }
        public DbSet<JobItem> JobItem { get; set; }
        public DbSet<KitchenPrinterTypes> KitchenPrinterTypes { get; set; }

        public DbSet<InvPriceLevel> InvPriceLevels { get; set; }

        #endregion

        #region reports
        public DbSet<Domain.Reports.ReportCategory> ReportCategory { get; set; }
        public DbSet<Domain.Reports.ReportInfo> ReportInfo { get; set; }
        public DbSet<TmpProductStockDetail> TmpProductStockDetail { get; set; }

        #endregion

        #region Configurations
        public DbSet<AutoGenerateInfo> AutoGenerateInfo { get; set; }
        public DbSet<DocumentNumber> DocumentNumber { get; set; }
        public DbSet<SysConfiguration> SysConfiguration { get; set; }
        public DbSet<PrinterType> PrinterType { get; set; }
        public new DbSet<Configuration> Configuration { get; set; }
        #endregion

        #region Accounts
        public DbSet<Tax> Taxes { get; set; }
        public DbSet<Currency> Currency { get; set; }
        public DbSet<CurrencyHistory> CurrencyHistory { get; set; }
        public DbSet<PaymentMethod> PaymentMethod { get; set; }
        public DbSet<PaymentTerm> PaymentTerm { get; set; }
        public DbSet<POProductTax> POProductTax { get; set; }
        public DbSet<PaidInType> PaiedInType { get; set; }
        public DbSet<PaidOutType> PaidOutType { get; set; }
        public DbSet<PayType> PayType { get; set; }

        //public static implicit operator Exception(ApplicationDbContext v)
        //{
        //    throw new NotImplementedException();
        //}
        #endregion

        #region POS
        public DbSet<SuspendHed> SuspendHed { get; set; }
        public DbSet<SuspendDet> SuspendDet { get; set; }
        public DbSet<SuspendDetBackup> SuspendDetBackup { get; set; }
        public DbSet<SuspendHedBackup> SuspendHedBackup { get; set; }
        public DbSet<SuspendPaymentDet> SuspendPaymentDet { get; set; }
        public DbSet<PaymentDet> PaymentDet { get; set; }
        public DbSet<TransactionDet> TransactionDet { get; set; }
        public DbSet<TransactionLog> TransactionLog { get; set; }
        public DbSet<InvSales> InvSales { get; set; }
        public DbSet<CashierGroup> CashierGroup { get; set; }
        public DbSet<CashierFunction> CashierFunction { get; set; }
        public DbSet<CashierPermission> CashierPermission { get; set; }
        public DbSet<POSUserGroup> POSUserGroup { get; set; }
        public DbSet<TempItemTax> TempItemTax { get; set; }
        public DbSet<InvAdvanceNoteHed> InvAdvanceNoteHed { get; set; }
        public DbSet<InvAdvancePaymentDet> InvAdvancePaymentDet { get; set; }
        public DbSet<InvAdvanceNoteDet> InvAdvanceNoteDet { get; set; }
        public DbSet<InvPosTerminalDetails> InvPosTerminalDetails { get; set; }
        #endregion

        #region Promotion Module by Hasanka
        public DbSet<InvPromotionType> InvPromotionType { get; set; }
        public DbSet<InvPromotionMaster> InvPromotionMaster { get; set; }
        public DbSet<InvPromotionDetailsBuyXProduct> InvPromotionDetailsBuyXProduct { get; set; }
        public DbSet<InvPromotionDetailsProductDis> InvPromotionDetailsProductDis { get; set; }

        // Migrated from Chamodi's
        public DbSet<InvPromoBusinessType> InvPromoBusinessType { get; set; }
        public DbSet<InvPromoCustomerCategory> InvPromoCustomerCategory { get; set; }
        public DbSet<CateringMood> CateringMood { get; set; }
        public DbSet<InvPromoLowestPriceWaveOff> InvPromoLowestPriceWaveOff { get; set; }
        public DbSet<InvPromoBillValueBasedGetYProduct> InvPromoBillValueBasedGetYProduct { get; set; }
        public DbSet<InvGiftVoucherPromotions> InvGiftVoucherPromotions { get; set; }
        public DbSet<Bank> Bank { get; set; }
        public DbSet<BankBin> BankBin { get; set; }
        public DbSet<CardType> CardType { get; set; }
        public DbSet<InvBillValueDiscount> InvBillValueDiscount { get; set; }
        public DbSet<InvBundleItemPrice> InvBundleItemPrice { get; set; } 
        public DbSet<InvComboPackBundleItemPrice> InvComboPackBundleItemPrice { get; set; } 
        #endregion

        #region Logs

        public DbSet<LOGAddons> LOGAddons { get; set; }
        public DbSet<LOGCustomer> LOGCustomer { get; set; }
        public DbSet<LOGInvPromotionMaster> LOGInvPromotionMaster { get; set; }
        public DbSet<LOGProduct> LOGProduct { get; set; }
        public DbSet<LOGProductServingUnit> LOGProductServingUnit { get; set; }
        public DbSet<LOGProductStockMaster> LOGProductStockMaster { get; set; }
        public DbSet<LOGProductTax> LOGProductTax { get; set; }
        public DbSet<LOGReceipe> LOGReceipe { get; set; }
        public DbSet<LOGSupplier> LOGSupplier { get; set; }
        public DbSet<LOGSupplierProduct> LOGSupplierProduct { get; set; }
        public DbSet<LOGUnitConversion> LOGUnitConversion { get; set; }


        #endregion Logs

        #region Loyalty Module
        public DbSet<LoyaltyCustomer> LoyaltyCustomer { get; set; }
        public DbSet<ReferenceType> ReferenceType { get; set; }
        public DbSet<CardMaster> CardMaster { get; set; }
        public DbSet<LoyaltyCardSchems> LoyaltyCardSchems { get; set; }
        public DbSet<LoyaltyCardGenerationHeader> LoyaltyCardGenerationHeader { get; set; }
        public DbSet<LoyaltyCardGenerationDetail> LoyaltyCardGenerationDetail { get; set; }
        public DbSet<CardGenerationLocationSetting> CardGenerationLocationSetting { get; set; }
        public DbSet<LoyaltyCardIssueHeader> LoyaltyCardIssueHeader { get; set; }
        public DbSet<LoyaltyCardIssueDetail> LoyaltyCardIssueDetail { get; set; }
        public DbSet<InvLoyaltyTransaction> InvLoyaltyTransaction { get; set; }
        public DbSet<PointsExpiration> PointsExpiration { get; set; }
        public DbSet<PointsExpirationSchedule> PointsExpirationSchedule { get; set; }
        public DbSet<PointsExpirationType> PointsExpirationType { get; set; }

        #endregion

        #region Journal
        public DbSet<ImportJournalDetails> ImportJournalDetails { get; set; }
        public DbSet<ImportJournalDetailsLog> ImportJournalDetailsLog { get; set; }

        #endregion Journal

        #region Delivery Module - Instructed By Anura. Developed by Hasanka
        //public DbSet<DeliveryModuleCustomer> DeliveryModuleCustomer { get; set; }
        //public DbSet<DeliveryModuleProduct> DeliveryModuleProduct { get; set; }
        //public DbSet<DeliveryModuleProductMap> DeliveryModuleProductMap { get; set; }

        #endregion

        #region gvgroup
        public DbSet<InvGiftVoucherGroup> InvGiftVoucherGroups { get; set; }
        #endregion gvgroup




        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            
            #region PO Model Creations
            // PO Header
            modelBuilder.Entity<PurchaseOrderHeader>().Property(x => x.GrossAmount).HasPrecision(18, 3);
            modelBuilder.Entity<PurchaseOrderHeader>().Property(x => x.OtherCharges).HasPrecision(18, 3);
            modelBuilder.Entity<PurchaseOrderHeader>().Property(x => x.DiscountAmount).HasPrecision(18, 3);
            modelBuilder.Entity<PurchaseOrderHeader>().Property(x => x.DiscountPercentage).HasPrecision(18, 3);
            modelBuilder.Entity<PurchaseOrderHeader>().Property(x => x.Addition).HasPrecision(18, 3);
            modelBuilder.Entity<PurchaseOrderHeader>().Property(x => x.Deduction).HasPrecision(18, 3);
            modelBuilder.Entity<PurchaseOrderHeader>().Property(x => x.NetAmount).HasPrecision(18, 3);
            modelBuilder.Entity<PurchaseOrderHeader>().Property(x => x.LineDiscountTotal).HasPrecision(18, 3);
            modelBuilder.Entity<PurchaseOrderHeader>().Property(x => x.TotalTaxAmount).HasPrecision(18, 3);
            modelBuilder.Entity<PurchaseOrderHeader>().Property(x => x.TotSellingPrice).HasPrecision(18, 3);
            modelBuilder.Entity<PurchaseOrderHeader>().Property(x => x.TotCostPrice).HasPrecision(18, 3);
            modelBuilder.Entity<PurchaseOrderHeader>().Property(x => x.TotDiscounts).HasPrecision(18, 3);

            // PO Detail
            modelBuilder.Entity<PurchaseOrderDetail>().Property(x => x.OrderQty).HasPrecision(18, 3);
            modelBuilder.Entity<PurchaseOrderDetail>().Property(x => x.FreeQty).HasPrecision(18, 3);
            modelBuilder.Entity<PurchaseOrderDetail>().Property(x => x.BalanceFreeQty).HasPrecision(18, 3);
            modelBuilder.Entity<PurchaseOrderDetail>().Property(x => x.BalanceQty).HasPrecision(18, 3);
            modelBuilder.Entity<PurchaseOrderDetail>().Property(x => x.CurrentQty).HasPrecision(18, 3);
            modelBuilder.Entity<PurchaseOrderDetail>().Property(x => x.CostPrice).HasPrecision(18, 3);
            modelBuilder.Entity<PurchaseOrderDetail>().Property(x => x.SellingPrice).HasPrecision(18, 3);
            modelBuilder.Entity<PurchaseOrderDetail>().Property(x => x.GrossAmount).HasPrecision(18, 3);
            modelBuilder.Entity<PurchaseOrderDetail>().Property(x => x.DiscountPercentage).HasPrecision(18, 3);
            modelBuilder.Entity<PurchaseOrderDetail>().Property(x => x.DiscountAmount).HasPrecision(18, 3);
            modelBuilder.Entity<PurchaseOrderDetail>().Property(x => x.SubTotalDiscount).HasPrecision(18, 3);
            modelBuilder.Entity<PurchaseOrderDetail>().Property(x => x.NetAmount).HasPrecision(18, 3);
            modelBuilder.Entity<PurchaseOrderDetail>().Property(x => x.ItemTaxTotal).HasPrecision(18, 3);
            modelBuilder.Entity<PurchaseOrderDetail>().Property(x => x.CostValue).HasPrecision(18, 3);
            modelBuilder.Entity<PurchaseOrderDetail>().Property(x => x.GRNQuantity).HasPrecision(18, 3);

            #endregion

            #region GRN Model Creations
            // Purchase Header
            modelBuilder.Entity<PurchaseHeader>().Property(x => x.GrossAmount).HasPrecision(18, 3);
            modelBuilder.Entity<PurchaseHeader>().Property(x => x.DiscountAmount).HasPrecision(18, 3);
            modelBuilder.Entity<PurchaseHeader>().Property(x => x.OtherChargers).HasPrecision(18, 3);
            modelBuilder.Entity<PurchaseHeader>().Property(x => x.NetAmount).HasPrecision(18, 3);
            modelBuilder.Entity<PurchaseHeader>().Property(x => x.LineDiscountTotal).HasPrecision(18, 3);
            modelBuilder.Entity<PurchaseHeader>().Property(x => x.TotalTax).HasPrecision(18, 3);
            modelBuilder.Entity<PurchaseHeader>().Property(x => x.TotCostPrice).HasPrecision(18, 3);
            modelBuilder.Entity<PurchaseHeader>().Property(x => x.TotDiscounts).HasPrecision(18, 3);
            modelBuilder.Entity<PurchaseHeader>().Property(x => x.TotSellingPrice).HasPrecision(18, 3);
            modelBuilder.Entity<PurchaseHeader>().Property(x => x.TotDiscounts).HasPrecision(18, 3);


            //Purchase Header

            // Purchase Detail
            //amounts
            modelBuilder.Entity<PurchaseDetail>().Property(x => x.GrossAmount).HasPrecision(18, 3);
            modelBuilder.Entity<PurchaseDetail>().Property(x => x.CostPrice).HasPrecision(18, 3);
            modelBuilder.Entity<PurchaseDetail>().Property(x => x.SellingPrice).HasPrecision(18, 3);
            modelBuilder.Entity<PurchaseDetail>().Property(x => x.NetAmount).HasPrecision(18, 3);
            modelBuilder.Entity<PurchaseDetail>().Property(x => x.DiscountPercentage).HasPrecision(18, 3);
            modelBuilder.Entity<PurchaseDetail>().Property(x => x.DiscountAmount).HasPrecision(18, 3);
            modelBuilder.Entity<PurchaseDetail>().Property(x => x.CostValue).HasPrecision(18, 3);
            //quantities
            modelBuilder.Entity<PurchaseDetail>().Property(x => x.OrderQty).HasPrecision(18, 3);
            modelBuilder.Entity<PurchaseDetail>().Property(x => x.FreeQty).HasPrecision(18, 3);
            modelBuilder.Entity<PurchaseDetail>().Property(x => x.CurrentQty).HasPrecision(18, 3);
            modelBuilder.Entity<PurchaseDetail>().Property(x => x.BalanceQty).HasPrecision(18, 3);
            modelBuilder.Entity<PurchaseDetail>().Property(x => x.GRNQuantity).HasPrecision(18, 3);
            modelBuilder.Entity<PurchaseDetail>().Property(x => x.TOGQty).HasPrecision(18, 3);
            // Purchase Detail

            //Addons
            modelBuilder.Entity<Addons>().Property(x => x.AddonQuantity).HasPrecision(18, 3);
            modelBuilder.Entity<Addons>().Property(x => x.AddonSellingPrice).HasPrecision(18, 3);
            modelBuilder.Entity<Receipe>().Property(x => x.Quantity).HasPrecision(18, 4);
            //Addons

            #endregion GRN Model Creations


            #region POS Model Creations
            modelBuilder.Entity<InvAdvancePaymentDet>().Property(x => x.Amount).HasPrecision(18, 4);
            modelBuilder.Entity<InvAdvancePaymentDet>().Property(x => x.Balance).HasPrecision(18, 4);
            #endregion POS Model Creations

            //------------------------- Navigation Properties manage card types  ===================

            modelBuilder.Entity<CardMaster>().HasKey(p => p.CardMasterId);
            modelBuilder.Entity<CardMaster>().Property(c => c.CardMasterId)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<LoyaltyCardSchems>().HasKey(b => b.LoyaltyCardSchemsID);
            modelBuilder.Entity<LoyaltyCardSchems>().Property(b => b.LoyaltyCardSchemsID)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<LoyaltyCardSchems>().HasRequired(p => p.CardMaster)
                .WithMany(b => b.LoyaltyCardSchems).HasForeignKey(b => b.CardMasterId);


            modelBuilder.Entity<ProductStockMaster>().Property(x => x.CostPrice).HasPrecision(18, 3);
            modelBuilder.Entity<ProductStockMaster>().Property(x => x.AvgCost).HasPrecision(18, 3);
            modelBuilder.Entity<LOGProductStockMaster>().Property(x => x.CostPrice).HasPrecision(18, 3);
            modelBuilder.Entity<LOGProductStockMaster>().Property(x => x.AvgCost).HasPrecision(18, 3);
            //modelBuilder.Entity<ProductLocationViewModel>().Property(x => x.CostPrice).HasPrecision(18, 4);
            //modelBuilder.Entity<ProductLocationViewModel>().Property(x => x.AverageCost).HasPrecision(18, 4);
            modelBuilder.Entity<Product>().Property(x => x.CostPrice).HasPrecision(18, 3);





            //------------------------- End Navigation Properties manage ===================

            //------------------------- indexing -------------------------------------------------



        }

        public System.Data.Entity.DbSet<RIT.HMS.Domain.Transactions.InvGiftVoucherMaster> InvGiftVoucherMasters { get; set; }
        public System.Data.Entity.DbSet<RIT.HMS.Domain.Transactions.InvGiftVoucherPurchaseOrderHeader> InvGiftVoucherPurchaseOrderHeaders { get; set; }
        public System.Data.Entity.DbSet<RIT.HMS.Domain.Transactions.InvGiftVoucherPurchaseOrderDetail> InvGiftVoucherPurchaseOrderDetails { get; set; }
        public System.Data.Entity.DbSet<RIT.HMS.Domain.Transactions.invGiftVoucherPurchaseHeaders> InvGiftVoucherPurchaseHeaders { get; set; }
        public System.Data.Entity.DbSet<RIT.HMS.Domain.Transactions.InvGiftVoucherPurchaseDetails> InvGiftVoucherPurchaseDetails { get; set; }
        public System.Data.Entity.DbSet<RIT.HMS.Domain.Transactions.InvGiftVoucherDocumentNumber> InvGiftVoucherDocumentNumbers { get; set; }
        public System.Data.Entity.DbSet<RIT.HMS.Domain.Transactions.InvGiftVoucherTransferNoteHeader> InvGiftVoucherTransferNoteHeader { get; set; }
        public System.Data.Entity.DbSet<RIT.HMS.Domain.Transactions.InvGiftVoucherTransferNoteDetail> InvGiftVoucherTransferNoteDetail { get; set; }
        public System.Data.Entity.DbSet<RIT.HMS.Domain.Transactions.InvStructureChanges> InvStructureChanges { get; set; }
        //public System.Data.Entity.DbSet<RIT.HMS.Domain.Supplier> Suppliers { get; set; }
        //public System.Data.Entity.DbSet<RIT.HMS.Domain.Transactions.PaymentTermA> PaymentTerms { get; set; }
        
    }
}
