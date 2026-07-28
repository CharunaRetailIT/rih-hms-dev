namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka146 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.InvSales",
                c => new
                    {
                        InvSalesId = c.Long(nullable: false, identity: true),
                        SalesId = c.Long(nullable: false),
                        CompanyId = c.Int(nullable: false),
                        CompanyCode = c.String(),
                        CompanyName = c.String(),
                        LocationId = c.Int(nullable: false),
                        LocationCode = c.String(),
                        LocationName = c.String(),
                        CostCentreId = c.Int(nullable: false),
                        DocumentId = c.Int(nullable: false),
                        DocumentNo = c.String(),
                        ReferenceNo = c.String(),
                        DocumentDate = c.DateTime(nullable: false),
                        TransactionTime = c.DateTime(nullable: false),
                        CustomerType = c.Int(nullable: false),
                        CustomerId = c.Long(nullable: false),
                        CustomerCode = c.String(),
                        CustomerName = c.String(),
                        SupplierID = c.Long(nullable: false),
                        SupplierCode = c.String(),
                        SupplierName = c.String(),
                        SalesPersonId = c.Long(nullable: false),
                        SalesPersonCode = c.String(),
                        SalesPersonName = c.String(),
                        GrossAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        DiscountPercentage = c.Decimal(nullable: false, precision: 18, scale: 2),
                        DiscountAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        NetAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        SubTotalDiscountPercentage = c.Decimal(nullable: false, precision: 18, scale: 2),
                        SubTotalDiscountAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CurrencyId = c.Int(nullable: false),
                        CurrencyRate = c.Int(nullable: false),
                        DepartmentId = c.Int(nullable: false),
                        DepartmentCode = c.String(),
                        DepartmentName = c.String(),
                        CategoryId = c.Long(nullable: false),
                        CategoryCode = c.String(),
                        CategoryName = c.String(),
                        SubCategoryId = c.Long(nullable: false),
                        SubCategoryCode = c.String(),
                        SubCategoryName = c.String(),
                        ProductId = c.Long(nullable: false),
                        ProductCode = c.String(),
                        ProductName = c.String(),
                        BarCode = c.String(),
                        BatchNo = c.String(),
                        ExpiryDate = c.DateTime(nullable: false),
                        Qty = c.Decimal(nullable: false, precision: 18, scale: 2),
                        UnitOfMeasureId = c.Long(nullable: false),
                        UnitOfMeasureName = c.String(),
                        PackSize = c.Decimal(nullable: false, precision: 18, scale: 2),
                        SellingPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        WholeSalePrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CostPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        AverageCost = c.Decimal(nullable: false, precision: 18, scale: 2),
                        DocumentStatus = c.Int(nullable: false),
                        IsFreeIssue = c.Boolean(nullable: false),
                        TerminalNo = c.String(),
                        IsDispatch = c.Boolean(nullable: false),
                        IsUpLoad = c.Boolean(nullable: false),
                        IsDelete = c.Boolean(nullable: false),
                        UnitNo = c.Int(nullable: false),
                        IsBackOffice = c.Boolean(nullable: false),
                        ZNo = c.Long(nullable: false),
                        GroupOfCompanyId = c.Int(nullable: false),
                        CreatedUser = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedUser = c.String(),
                        ModifiedDate = c.DateTime(nullable: false),
                        DataTransfer = c.Int(nullable: false),
                        SerialNo = c.Int(nullable: false),
                        CorporatePrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.InvSalesId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.InvSales");
        }
    }
}
