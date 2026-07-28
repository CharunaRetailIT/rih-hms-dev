namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _99 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.InvAdvanceNoteDets",
                c => new
                    {
                        InvAdvanceNoteDetID = c.Int(nullable: false, identity: true),
                        Idx = c.Long(),
                        ProductID = c.Long(nullable: false),
                        ProductCode = c.String(maxLength: 25),
                        RefCode = c.String(maxLength: 25),
                        BarCodeFull = c.Long(nullable: false),
                        Descrip = c.String(maxLength: 50),
                        BatchNo = c.String(maxLength: 50),
                        SerialNo = c.String(maxLength: 50),
                        ExpiryDate = c.DateTime(),
                        Cost = c.Decimal(nullable: false, precision: 18, scale: 2),
                        AvgCost = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Qty = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        UnitOfMeasureID = c.Long(nullable: false),
                        UnitOfMeasureName = c.String(maxLength: 10),
                        ConvertFactor = c.Decimal(nullable: false, precision: 18, scale: 2),
                        IDI1 = c.Int(nullable: false),
                        IDis1 = c.Decimal(nullable: false, precision: 18, scale: 2),
                        IDiscount1 = c.Decimal(nullable: false, precision: 18, scale: 2),
                        IDI1CashierID = c.Long(nullable: false),
                        IDI2 = c.Int(nullable: false),
                        IDis2 = c.Decimal(nullable: false, precision: 18, scale: 2),
                        IDiscount2 = c.Decimal(nullable: false, precision: 18, scale: 2),
                        IDI2CashierID = c.Long(nullable: false),
                        IDI3 = c.Int(nullable: false),
                        IDis3 = c.Decimal(nullable: false, precision: 18, scale: 2),
                        IDiscount3 = c.Decimal(nullable: false, precision: 18, scale: 2),
                        IDI3CashierID = c.Long(nullable: false),
                        IDI4 = c.Decimal(nullable: false, precision: 18, scale: 2),
                        IDis4 = c.Decimal(nullable: false, precision: 18, scale: 2),
                        IDiscount4 = c.Decimal(nullable: false, precision: 18, scale: 2),
                        IDI4CashierID = c.Long(nullable: false),
                        IDI5 = c.Int(nullable: false),
                        IDis5 = c.Decimal(nullable: false, precision: 18, scale: 2),
                        IDiscount5 = c.Decimal(nullable: false, precision: 18, scale: 2),
                        IDI5CashierID = c.Long(nullable: false),
                        Rate = c.Decimal(nullable: false, precision: 18, scale: 2),
                        IsSDis = c.Boolean(nullable: false),
                        SDNo = c.Int(nullable: false),
                        SDID = c.Int(nullable: false),
                        SDIs = c.Decimal(nullable: false, precision: 18, scale: 2),
                        SDiscount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        DDisCashierID = c.Long(nullable: false),
                        Nett = c.Decimal(nullable: false, precision: 18, scale: 2),
                        LocationID = c.Int(nullable: false),
                        DocumentID = c.Int(nullable: false),
                        BillTypeID = c.Int(nullable: false),
                        SaleTypeID = c.Int(nullable: false),
                        Receipt = c.String(maxLength: 10),
                        SalesmanID = c.Long(nullable: false),
                        Salesman = c.String(maxLength: 15),
                        CustomerID = c.Long(nullable: false),
                        Customer = c.String(maxLength: 15),
                        CashierID = c.Int(nullable: false),
                        Cashier = c.String(maxLength: 15),
                        StartTime = c.DateTime(nullable: false),
                        EndTime = c.DateTime(nullable: false),
                        RecDate = c.DateTime(nullable: false),
                        BaseUnitID = c.Long(nullable: false),
                        UnitNo = c.Int(nullable: false),
                        RowNo = c.Int(nullable: false),
                        IsRecall = c.Boolean(nullable: false),
                        RecallNO = c.String(maxLength: 10),
                        RecallAdv = c.Boolean(nullable: false),
                        TaxAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        IsTax = c.Boolean(nullable: false),
                        TaxPercentage = c.Decimal(nullable: false, precision: 18, scale: 2),
                        IsStock = c.Boolean(nullable: false),
                        CreditNoteNo = c.String(maxLength: 150),
                        CreditNoteBy = c.Long(nullable: false),
                        CustomerType = c.Int(nullable: false),
                        TransStatus = c.Int(nullable: false),
                        IsPromotionApplied = c.Boolean(nullable: false),
                        PromotionID = c.Int(nullable: false),
                        IsPromotion = c.Boolean(nullable: false),
                        ItemSerial = c.String(maxLength: 50),
                        warranty = c.String(maxLength: 50),
                        RecallFromInvoiceNo = c.String(maxLength: 50, unicode: false),
                        WorkComplete = c.Boolean(),
                        WorkCompUser = c.String(maxLength: 30),
                        WorkCompDateTime = c.DateTime(),
                        CustCollected = c.Boolean(),
                        CustColDateTime = c.DateTime(),
                        IsNewPrice = c.Boolean(nullable: false),
                        IsApproved = c.Boolean(nullable: false),
                        ApprovedBy = c.Long(nullable: false),
                        ApprovedFor = c.String(maxLength: 10, fixedLength: true),
                        ReferenceProductId = c.Int(nullable: false),
                        ReferenceProductRow = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.InvAdvanceNoteDetID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.InvAdvanceNoteDets");
        }
    }
}
