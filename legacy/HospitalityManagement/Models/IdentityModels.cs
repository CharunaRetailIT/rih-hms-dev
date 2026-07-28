using System;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using HospitalityManagement.Models.Transactions;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Reflection.Emit;
using System.Data.Entity.Migrations;

using HospitalityManagement.Models.Promotions;
using RIT.HMS.Domain.Logs;
using HospitalityManagement.Models.ViewModels;

namespace HospitalityManagement.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("HMSContext", throwIfV1Schema: false)
        {

        }

       
        public static ApplicationDbContext Create()
        {
           
            return new ApplicationDbContext();
        }

        public DbSet<SysGroupOfCompany> SysGroupOfCompanys { get; set; }
        public DbSet<SysCompany> SysCompanys { get; set; }
        public DbSet<SysLocation> SysLocations { get; set; }

        #region UserFunction And UserPermission 
        public DbSet<SysUserFunction> SysUserFunctions { get; set; }
        public DbSet<SysUserGroup> SysUserGroups { get; set; }
        public DbSet<SysUserGroupPermission> SysUserGroupPermissions { get; set; }
        public DbSet<SysUserMaster> SysUserMasters { get; set; }
        public DbSet<SysUserPermission> SysUserPermissions { get; set; }
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

        public DbSet<ChairMaster> ChairMaster { get; set; }
        public DbSet<TableMaster> TableMasters { get; set; }

        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<DeliveryPerson> DeliveryPerson { get; set; }
        public DbSet<UnitConversion> UnitConversion { get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<Receipe> Receipe { get; set; }
        public DbSet<ProductTax> ProductTax { get; set; }
        public DbSet<ProductServingUnit> ProductServingUnit { get; set; }
        public DbSet<ProductionNoteHeader> ProductionNoteHeader { get; set; }
        public DbSet<ProductionNoteDetail> ProductionNoteDetail { get; set; }
        public DbSet<RstMealType> RstMealType { get; set; }
        public DbSet<InterDepartment> InterDepartment { get; set; }
        public DbSet<RequestNoteHeader> RequestNoteHeader { get; set; }
        public DbSet<RequestNoteDetail> RequestNoteDetail { get; set; }
        public DbSet<Addons> Addons { get; set; }
        public DbSet<RequestNoteAccptanceHeader> RequestNoteAccptanceHeader { get; set; }
        public DbSet<RequestNoteAcceptanceDetail> RequestNoteAcceptanceDetail { get; set; }
        public DbSet<KOTBOTDescription> KOTBOTDescription { get; set; }
        public DbSet<ProductInstruction> ProductInstruction { get; set; }
        public DbSet<AddonCategoryMaster> AddonCategoryMaster { get; set; }

        //ll

        #endregion

        #region Open
        public DbSet<Customer> Customers { get; set; }
        public DbSet<CustomerCategory> CustomerCategorys { get; set; }
        public DbSet<CustomoerPreviousVisits> CustomoerPreviousVisitss { get; set; }
        public DbSet<StewardsMaster> StewardsMastesrs { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<EmployeeGroup> EmployeeGroup { get; set; }
        public DbSet<SupplierType> SupplierType { get; set; }
        #endregion

        #region Inventory
        public DbSet<InvSupplier> InvSupplier { get; set; }
        public DbSet<InvProductMaster> InvProductMaster { get; set; }
        public DbSet<UnitOfMeasure> UnitOfMeasure { get; set; }

        public DbSet<Supplier> Supplier { get; set; }
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

        #endregion

        #region reports
        public DbSet<Reports.ReportCategory> ReportCategory { get; set; }
        public DbSet<Reports.ReportInfo> ReportInfo { get; set; }
        public DbSet<Transactions.TmpProductStockDetail> TmpProductStockDetail { get; set; }
        public DbSet<RIT.HMS.Domain.Transactions.InvGiftVoucherBookCode> InvGiftVoucherBookCode { get; set; }
        #endregion

        #region Configurations
        public DbSet<AutoGenerateInfo> AutoGenerateInfo { get; set; }
        public DbSet<DocumentNumber> DocumentNumber { get; set; }
        public DbSet<SysConfiguration> SysConfiguration { get; set; }
        public DbSet<PrinterType> PrinterType { get; set; }
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

        #endregion


        #region Promotion Module by Hasanka
        public DbSet<InvPromotionType> InvPromotionType { get; set; }
        public DbSet<InvPromotionMaster> InvPromotionMaster { get; set; }
        public DbSet<InvPromotionDetailsBuyXProduct> InvPromotionDetailsBuyXProduct { get; set; }

        // Migrated from Chamodi
        public DbSet<InvPromoBusinessType> InvPromoBusinessType { get; set; }
        public DbSet<InvPromoCustomerCategory> InvPromoCustomerCategory { get; set; }
        public DbSet<CateringMood> CateringMood { get; set; }
        public DbSet<InvPromoLowestPriceWaveOff> InvPromoLowestPriceWaveOff { get; set; }
        public DbSet<InvPromoBillValueBasedGetYProduct> InvPromoBillValueBasedGetYProduct { get; set; }
        public DbSet<InvGiftVoucherPromotions> InvGiftVoucherPromotions { get; set; }



        #endregion


        #region Delivery Module - Instructed By Anura. Developed by Hasanka
        //public DbSet<DeliveryModuleCustomer> DeliveryModuleCustomer { get; set; }
        //public DbSet<DeliveryModuleProduct> DeliveryModuleProduct { get; set; }
        //public DbSet<DeliveryModuleProductMap> DeliveryModuleProductMap { get; set; }

        #endregion


        //public DbSet<Receipe> RReceipe;

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {


            modelBuilder.Entity<IdentityUserRole>()
            .HasKey(r => new { r.UserId, r.RoleId })
            .ToTable("AspNetUserRoles");

            modelBuilder.Entity<IdentityUserLogin>()
                        .HasKey(l => new { l.LoginProvider, l.ProviderKey, l.UserId })
                        .ToTable("AspNetUserLogins");

            modelBuilder.Entity<Receipe>().Property(x => x.Quantity).HasPrecision(18, 4);
            modelBuilder.Entity<ProductStockMaster>().Property(x => x.CostPrice).HasPrecision(18, 3);
            modelBuilder.Entity<ProductStockMaster>().Property(x => x.AvgCost).HasPrecision(18, 3);
            modelBuilder.Entity<LOGProductStockMaster>().Property(x => x.CostPrice).HasPrecision(18, 3);
            modelBuilder.Entity<LOGProductStockMaster>().Property(x => x.AvgCost).HasPrecision(18, 3);
            modelBuilder.Entity<ProductLocationViewModel>().Property(x => x.CostPrice).HasPrecision(18, 3);
            modelBuilder.Entity<ProductLocationViewModel>().Property(x => x.AverageCost).HasPrecision(18, 3);
            modelBuilder.Entity<Product>().Property(x => x.CostPrice).HasPrecision(18, 3);

            //        modelBuilder.Entity<Product>()
            //.Property(p => p.CostPrice)
            //.HasColumnType("decimal(18,4)");
            //        modelBuilder.Entity<ProductStockMaster>()
            //.Property(p => p.CostPrice)
            //.HasColumnType("decimal(18,4)");
            //        modelBuilder.Entity<ProductStockMaster>()
            //.Property(p => p.AvgCost)
            //.HasColumnType("decimal(18,4)");
            //        modelBuilder.Entity<ProductLocationViewModel>()
            //        .Property(p => p.CostPrice)
            //        .HasColumnType("decimal(18,4)");
            //        modelBuilder.Entity<ProductLocationViewModel>()
            //        .Property(p => p.AverageCost)
            //        .HasColumnType("decimal(18,4)");
            //        modelBuilder.Entity<LOGProductStockMaster>()
            //        .Property(p => p.CostPrice)
            //        .HasColumnType("decimal(18,4)");
            //        modelBuilder.Entity<LOGProductStockMaster>()
            //        .Property(p => p.AvgCost)
            //        .HasColumnType("decimal(18,4)");
        }
    }
}