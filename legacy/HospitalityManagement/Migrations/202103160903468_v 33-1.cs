namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v331 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.InvBundleItemPrices",
                c => new
                    {
                        InvBundleItemPriceId = c.Int(nullable: false, identity: true),
                        PromotionMasterId = c.Int(nullable: false),
                        InvId = c.Int(nullable: false),
                        SinglePriceForAllItems = c.Boolean(nullable: false),
                        DifferentPricesForItems = c.Boolean(nullable: false),
                        ProductId = c.Int(nullable: false),
                        ServingUnitId = c.Int(nullable: false),
                        Quantity = c.Decimal(nullable: false, precision: 18, scale: 2),
                        SellingPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        GroupId = c.Int(nullable: false),
                        GroupOfCompanyID = c.Int(nullable: false),
                        CompanyID = c.Int(nullable: false),
                        LocationId = c.Int(nullable: false),
                        CreatedUser = c.String(maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedUser = c.String(maxLength: 50),
                        ModifiedDate = c.DateTime(nullable: false),
                        DataTransfer = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.InvBundleItemPriceId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.InvBundleItemPrices");
        }
    }
}
