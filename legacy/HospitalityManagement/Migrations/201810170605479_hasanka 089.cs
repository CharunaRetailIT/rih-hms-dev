namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka089 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.StockAdjustmentDetails",
                c => new
                    {
                        StockAdjustmentDetailId = c.Long(nullable: false, identity: true),
                        StockAdjustmentHeaderId = c.Long(nullable: false),
                        ProductId = c.Long(nullable: false),
                        CurrentStock = c.Decimal(nullable: false, precision: 18, scale: 2),
                        AdjustStock = c.Decimal(nullable: false, precision: 18, scale: 2),
                        AdjustmentTypeId = c.Int(nullable: false),
                        NewStock = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CostPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        SellingPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        AvgCost = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.StockAdjustmentDetailId);
            
            CreateTable(
                "dbo.StockAdjustmentHeaders",
                c => new
                    {
                        StockAdjustmentHeaderId = c.Long(nullable: false, identity: true),
                        DocumentNo = c.String(),
                        StockLocationId = c.Long(nullable: false),
                        Remark = c.String(),
                        GroupOfCompanyID = c.Int(nullable: false),
                        CompanyID = c.Int(nullable: false),
                        LocationId = c.Int(nullable: false),
                        CreatedUser = c.String(maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedUser = c.String(maxLength: 50),
                        ModifiedDate = c.DateTime(nullable: false),
                        DataTransfer = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.StockAdjustmentHeaderId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.StockAdjustmentHeaders");
            DropTable("dbo.StockAdjustmentDetails");
        }
    }
}
