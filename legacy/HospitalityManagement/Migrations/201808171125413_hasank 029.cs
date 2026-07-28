namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasank029 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PurchaseOrderDetails",
                c => new
                    {
                        PurchaseOrderDetailId = c.Long(nullable: false, identity: true),
                        PurchaseOrderHeaderId = c.Long(nullable: false),
                        CostCentreId = c.Int(nullable: false),
                        ProductId = c.Long(nullable: false),
                        IsBatch = c.Boolean(nullable: false),
                        StockCode = c.String(maxLength: 25),
                        DocumentId = c.Int(nullable: false),
                        DocumentNo = c.String(maxLength: 20),
                        OrderQty = c.Decimal(nullable: false, precision: 18, scale: 2),
                        FreeQty = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CurrentQty = c.Decimal(nullable: false, precision: 18, scale: 2),
                        BalanceQty = c.Decimal(nullable: false, precision: 18, scale: 2),
                        BalanceFreeQty = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CostPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        SellingPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PackSize = c.String(),
                        PackId = c.Int(nullable: false),
                        UnitOfMeasureId = c.Long(nullable: false),
                        BaseUnitId = c.Long(nullable: false),
                        ConvertFactor = c.Decimal(nullable: false, precision: 18, scale: 2),
                        GrossAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        DiscountPercentage = c.Decimal(nullable: false, precision: 18, scale: 2),
                        DiscountAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        SubTotalDiscount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TaxAmount1 = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TaxAmount2 = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TaxAmount3 = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TaxAmount4 = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TaxAmount5 = c.Decimal(nullable: false, precision: 18, scale: 2),
                        NetAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CurrencyId = c.Int(nullable: false),
                        CurrencyRate = c.Decimal(nullable: false, precision: 18, scale: 2),
                        LineNo = c.Long(nullable: false),
                        DocumentStatus = c.Int(nullable: false),
                        ScanDocument = c.Binary(),
                        BatchNo = c.String(),
                        ProductRemark = c.String(maxLength: 200),
                    })
                .PrimaryKey(t => t.PurchaseOrderDetailId)
                .ForeignKey("dbo.PurchaseOrderHeaders", t => t.PurchaseOrderHeaderId, cascadeDelete: true)
                .Index(t => t.PurchaseOrderHeaderId);
            
            CreateTable(
                "dbo.PurchaseOrderHeaders",
                c => new
                    {
                        PurchaseOrderHeaderId = c.Long(nullable: false, identity: true),
                        CostCentreId = c.Int(nullable: false),
                        JobClassId = c.Long(nullable: false),
                        DocumentId = c.Int(nullable: false),
                        DocumentNo = c.String(maxLength: 20),
                        DocumentDate = c.DateTime(nullable: false),
                        SupplierId = c.Long(nullable: false),
                        ExpectedDate = c.DateTime(nullable: false),
                        ExpiryDate = c.DateTime(nullable: false),
                        PaymentExpectedDate = c.DateTime(nullable: false),
                        ValidityPeriod = c.Int(nullable: false),
                        IsConsignmentBasis = c.Boolean(nullable: false),
                        GrossAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        OtherCharges = c.Decimal(nullable: false, precision: 18, scale: 2),
                        DiscountAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        DiscountPercentage = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TaxAmount1 = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TaxAmount2 = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TaxAmount3 = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TaxAmount4 = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TaxAmount5 = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TaxAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Addition = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Deduction = c.Decimal(nullable: false, precision: 18, scale: 2),
                        NetAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        RequestedBy = c.String(maxLength: 50),
                        DeliveryLocationId = c.Int(nullable: false),
                        LineDiscountTotal = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PaymentTermId = c.Int(nullable: false),
                        PaymentPeriod = c.Int(nullable: false),
                        CurrencyId = c.Int(nullable: false),
                        CurrencyRate = c.Decimal(nullable: false, precision: 18, scale: 2),
                        DeliveryDetail = c.String(maxLength: 500),
                        ReferenceDocumentId = c.Int(nullable: false),
                        ReferenceNo = c.String(maxLength: 20),
                        Remark = c.String(maxLength: 150),
                        DocumentStatus = c.Int(nullable: false),
                        IsUpLoad = c.Boolean(nullable: false),
                        LastAuthorizedBy = c.String(maxLength: 50),
                        IsAuthorized = c.Boolean(nullable: false),
                        LastAuthorizedDate = c.DateTime(),
                        GroupOfCompanyID = c.Int(nullable: false),
                        CompanyID = c.Int(nullable: false),
                        LocationId = c.Int(nullable: false),
                        CreatedUser = c.String(maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedUser = c.String(maxLength: 50),
                        ModifiedDate = c.DateTime(nullable: false),
                        DataTransfer = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.PurchaseOrderHeaderId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PurchaseOrderDetails", "PurchaseOrderHeaderId", "dbo.PurchaseOrderHeaders");
            DropIndex("dbo.PurchaseOrderDetails", new[] { "PurchaseOrderHeaderId" });
            DropTable("dbo.PurchaseOrderHeaders");
            DropTable("dbo.PurchaseOrderDetails");
        }
    }
}
