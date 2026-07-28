namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka055 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ProductStockMasters",
                c => new
                    {
                        ProductStockMasterId = c.Long(nullable: false, identity: true),
                        CostCentreId = c.Int(nullable: false),
                        ProductId = c.Long(nullable: false),
                        StockCode = c.String(nullable: false, maxLength: 25),
                        Stock = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CostPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        SellingPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        MinimumPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ReOrderLevel = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ReOrderQuantity = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ReOrderPeriod = c.Decimal(nullable: false, precision: 18, scale: 2),
                        IsDelete = c.Boolean(nullable: false),
                        ProductCode = c.String(maxLength: 20),
                        ProductName = c.String(maxLength: 100),
                        Barcode = c.String(maxLength: 30),
                        RefNo1 = c.String(maxLength: 30),
                        RefNo2 = c.String(maxLength: 30),
                        ExtendedId = c.Int(nullable: false),
                        ExtendedName = c.String(maxLength: 30),
                        PLUCode = c.String(maxLength: 5),
                        PLUName = c.String(maxLength: 30),
                        WeightPerunit = c.Decimal(nullable: false, precision: 18, scale: 2),
                        UomId = c.Int(nullable: false),
                        Unit = c.String(maxLength: 10),
                        SupplierID = c.Int(nullable: false),
                        SupplierCode = c.String(maxLength: 20),
                        MaxPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        AvgCost = c.Decimal(nullable: false, precision: 18, scale: 2),
                        WholeSalePrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        FixedGP = c.Decimal(nullable: false, precision: 18, scale: 2),
                        GP = c.Decimal(nullable: false, precision: 18, scale: 2),
                        OpenBal = c.Decimal(nullable: false, precision: 18, scale: 2),
                        InitSIH = c.Decimal(nullable: false, precision: 18, scale: 2),
                        InitCost = c.Decimal(nullable: false, precision: 18, scale: 2),
                        AdjQty = c.Decimal(nullable: false, precision: 18, scale: 2),
                        IsWarranty = c.Boolean(nullable: false),
                        IsDamage = c.Boolean(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsBundle = c.Boolean(nullable: false),
                        IsInitialize = c.Boolean(nullable: false),
                        DataTransfer = c.Int(nullable: false),
                        Ispacksize = c.Boolean(nullable: false),
                        Iscommission = c.Boolean(nullable: false),
                        Isdecimal = c.Boolean(nullable: false),
                        GroupOfCompanyID = c.Int(nullable: false),
                        CompanyID = c.Int(nullable: false),
                        LocationId = c.Int(nullable: false),
                        CreatedUser = c.String(maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedUser = c.String(maxLength: 50),
                        ModifiedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ProductStockMasterId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.ProductStockMasters");
        }
    }
}
