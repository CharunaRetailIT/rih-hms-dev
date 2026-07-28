namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class promotionmodule02 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.InvPromotionDetailsBuyXProducts",
                c => new
                    {
                        InvPromotionDetailsBuyXProductId = c.Long(nullable: false, identity: true),
                        InvPromotionMasterId = c.Long(nullable: false),
                        ProductId = c.Long(nullable: false),
                        ServingUnitId = c.Int(nullable: false),
                        BuyUnitOfMeasureId = c.Long(nullable: false),
                        Rate = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Qty = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ProductType = c.Int(nullable: false),
                        GroupOfCompanyID = c.Int(nullable: false),
                        CompanyID = c.Int(nullable: false),
                        LocationId = c.Int(nullable: false),
                        CreatedUser = c.String(maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedUser = c.String(maxLength: 50),
                        ModifiedDate = c.DateTime(nullable: false),
                        DataTransfer = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.InvPromotionDetailsBuyXProductId)
                .ForeignKey("dbo.InvPromotionMasters", t => t.InvPromotionMasterId, cascadeDelete: true)
                .Index(t => t.InvPromotionMasterId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.InvPromotionDetailsBuyXProducts", "InvPromotionMasterId", "dbo.InvPromotionMasters");
            DropIndex("dbo.InvPromotionDetailsBuyXProducts", new[] { "InvPromotionMasterId" });
            DropTable("dbo.InvPromotionDetailsBuyXProducts");
        }
    }
}
