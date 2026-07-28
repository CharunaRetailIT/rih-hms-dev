namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka061 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PurchaseDetails",
                c => new
                    {
                        PurchaseDetailID = c.Long(nullable: false, identity: true),
                        PurchaseHeaderID = c.Long(nullable: false),
                        CostCentreID = c.Int(nullable: false),
                        DocumentID = c.Int(nullable: false),
                        DocumentNo = c.String(maxLength: 20),
                        LineNo = c.Long(nullable: false),
                        ProductID = c.Long(nullable: false),
                        IsBatch = c.Boolean(nullable: false),
                        BatchNo = c.String(maxLength: 50),
                        StockCode = c.String(maxLength: 25),
                        StockCodeOriginal = c.String(maxLength: 25),
                        UnitOfMeasureID = c.Long(nullable: false),
                        BaseUnitID = c.Long(nullable: false),
                        IsExpiry = c.Boolean(nullable: false),
                        ExpiryDate = c.DateTime(),
                        OrderQty = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Qty = c.Decimal(nullable: false, precision: 18, scale: 2),
                        FreeQty = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CurrentQty = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ConvertFactor = c.Decimal(nullable: false, precision: 18, scale: 2),
                        BalanceQty = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CostPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        SellingPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        AvgCost = c.Decimal(nullable: false, precision: 18, scale: 2),
                        GrossAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        DiscountPercentage = c.Decimal(nullable: false, precision: 18, scale: 2),
                        DiscountAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        SubTotalDiscount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TotalTax = c.Decimal(nullable: false, precision: 18, scale: 2),
                        NetAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        DocumentStatus = c.Int(nullable: false),
                        DocumentDate = c.DateTime(nullable: false),
                        ProductRemark = c.String(maxLength: 200),
                        Packsize = c.Decimal(nullable: false, precision: 18, scale: 2),
                        profitMargin = c.Decimal(nullable: false, precision: 18, scale: 2),
                        SerialNo = c.String(),
                        IsUsed = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.PurchaseDetailID)
                .ForeignKey("dbo.PurchaseHeaders", t => t.PurchaseHeaderID, cascadeDelete: true)
                .Index(t => t.PurchaseHeaderID);
            
            CreateTable(
                "dbo.PurchaseHeaders",
                c => new
                    {
                        PurchaseHeaderId = c.Long(nullable: false, identity: true),
                        CostCentreID = c.Int(nullable: false),
                        DocumentID = c.Int(nullable: false),
                        DocumentNo = c.String(nullable: false, maxLength: 20),
                        DocumentDate = c.DateTime(nullable: false),
                        SupplierID = c.Long(nullable: false),
                        GrossAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        DiscountAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        OtherChargers = c.Decimal(nullable: false, precision: 18, scale: 2),
                        DiscountPercentage = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TotalTax = c.Decimal(nullable: false, precision: 18, scale: 2),
                        NetAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        BatchNo = c.String(maxLength: 50),
                        Remark = c.String(maxLength: 150),
                        LineDiscountTotal = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PaymentTermID = c.Int(nullable: false),
                        PaymentPeriod = c.Int(nullable: false),
                        CurrencyID = c.Int(nullable: false),
                        CurrencyRate = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ReferenceDocumentDocumentID = c.Int(nullable: false),
                        ReferenceDocumentID = c.Long(nullable: false),
                        ReferenceNo = c.String(maxLength: 20),
                        SupplierInvoiceNo = c.String(maxLength: 20),
                        DocumentStatus = c.Int(nullable: false),
                        IsUpLoad = c.Boolean(nullable: false),
                        ReturnTypeID = c.Int(nullable: false),
                        OtherDeduction = c.Decimal(nullable: false, precision: 18, scale: 2),
                        GroupOfCompanyID = c.Int(nullable: false),
                        CompanyID = c.Int(nullable: false),
                        LocationId = c.Int(nullable: false),
                        CreatedUser = c.String(maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedUser = c.String(maxLength: 50),
                        ModifiedDate = c.DateTime(nullable: false),
                        DataTransfer = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.PurchaseHeaderId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PurchaseDetails", "PurchaseHeaderID", "dbo.PurchaseHeaders");
            DropIndex("dbo.PurchaseDetails", new[] { "PurchaseHeaderID" });
            DropTable("dbo.PurchaseHeaders");
            DropTable("dbo.PurchaseDetails");
        }
    }
}
