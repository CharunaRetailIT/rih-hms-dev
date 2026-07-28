namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v244 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CustomProductionNoteDetails",
                c => new
                    {
                        CustomProductionNoteDetailId = c.Long(nullable: false, identity: true),
                        CustomProductionNoteHeaderId = c.Long(nullable: false),
                        ProductId = c.Long(nullable: false),
                        ProductName = c.String(),
                        ProductQty = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ProductCostPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ProductSellingPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        MaterialId = c.Long(nullable: false),
                        MaterialName = c.String(),
                        MaterialQty = c.Decimal(nullable: false, precision: 18, scale: 2),
                        MaterialSellingPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        MaterialCostPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        MaterialAvgCost = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.CustomProductionNoteDetailId)
                .ForeignKey("dbo.CustomProductionNoteHeaders", t => t.CustomProductionNoteHeaderId, cascadeDelete: true)
                .Index(t => t.CustomProductionNoteHeaderId);
            
            CreateTable(
                "dbo.CustomProductionNoteHeaders",
                c => new
                    {
                        CustomProductionNoteHeaderId = c.Long(nullable: false, identity: true),
                        DocumentNo = c.String(nullable: false, maxLength: 15),
                        ProcessLocId = c.Long(nullable: false),
                        PickupLocId = c.Long(nullable: false),
                        Remark = c.String(maxLength: 200),
                        IsFinished = c.Boolean(nullable: false),
                        DocumentId = c.Int(nullable: false),
                        ReceiptLocID = c.Int(nullable: false),
                        R_Zno = c.Int(nullable: false),
                        ReceiptNo = c.String(maxLength: 40),
                        UnitNo = c.Int(nullable: false),
                        GroupOfCompanyID = c.Int(nullable: false),
                        CompanyID = c.Int(nullable: false),
                        LocationId = c.Int(nullable: false),
                        CreatedUser = c.String(maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedUser = c.String(maxLength: 50),
                        ModifiedDate = c.DateTime(nullable: false),
                        DataTransfer = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.CustomProductionNoteHeaderId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CustomProductionNoteDetails", "CustomProductionNoteHeaderId", "dbo.CustomProductionNoteHeaders");
            DropIndex("dbo.CustomProductionNoteDetails", new[] { "CustomProductionNoteHeaderId" });
            DropTable("dbo.CustomProductionNoteHeaders");
            DropTable("dbo.CustomProductionNoteDetails");
        }
    }
}
