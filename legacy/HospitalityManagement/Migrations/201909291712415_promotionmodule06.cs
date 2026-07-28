namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class promotionmodule06 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CateringMoods",
                c => new
                    {
                        CateringMoodID = c.Long(nullable: false, identity: true),
                        CateringMoodName = c.String(nullable: false, maxLength: 20),
                        OrderSequence = c.String(nullable: false, maxLength: 50),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.CateringMoodID);
            
            CreateTable(
                "dbo.InvPromoBillValueBasedGetYProducts",
                c => new
                    {
                        InvPromoBillValueBasedGetYProductId = c.Long(nullable: false, identity: true),
                        InvPromotionMasterId = c.Long(nullable: false),
                        ProductId = c.Long(nullable: false),
                        ValueFrom = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ValueTo = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ServingUnitId = c.Int(nullable: false),
                        BuyUnitOfMeasureId = c.Long(nullable: false),
                        Rate = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Qty = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Points = c.Long(nullable: false),
                        DiscountPercentage = c.Decimal(nullable: false, precision: 18, scale: 2),
                        DiscountAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
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
                .PrimaryKey(t => t.InvPromoBillValueBasedGetYProductId)
                .ForeignKey("dbo.InvPromotionMasters", t => t.InvPromotionMasterId, cascadeDelete: true)
                .Index(t => t.InvPromotionMasterId);
            
            CreateTable(
                "dbo.InvPromoBusinessTypes",
                c => new
                    {
                        InvPromoBusinessTypeID = c.Long(nullable: false, identity: true),
                        InvPromotionMasterID = c.Long(nullable: false),
                        CateringMoodID = c.Long(nullable: false),
                        CateringMoodName = c.String(nullable: false, maxLength: 20),
                        Remark = c.String(maxLength: 150),
                        Status = c.Boolean(nullable: false),
                        CreatedUser = c.String(maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.InvPromoBusinessTypeID);
            
            CreateTable(
                "dbo.InvPromoCustomerCategories",
                c => new
                    {
                        InvPromoCustomerCategoryID = c.Long(nullable: false, identity: true),
                        InvPromotionMasterID = c.Long(nullable: false),
                        CustomerCategoryID = c.Int(nullable: false),
                        Remark = c.String(maxLength: 150),
                        Status = c.Boolean(nullable: false),
                        CreatedUser = c.String(maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.InvPromoCustomerCategoryID);
            
            CreateTable(
                "dbo.InvPromoLowestPriceWaveOffs",
                c => new
                    {
                        InvPromoLowestPriceWaveOffID = c.Long(nullable: false, identity: true),
                        InvPromotionMasterID = c.Long(nullable: false),
                        LowestPriceWaveOffCode = c.String(nullable: false, maxLength: 15),
                        LowestPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        IsFullWaveOff = c.Boolean(nullable: false),
                        Qty = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Remark = c.String(maxLength: 150),
                        Status = c.Boolean(nullable: false),
                        CreatedUser = c.String(maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.InvPromoLowestPriceWaveOffID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.InvPromoBillValueBasedGetYProducts", "InvPromotionMasterId", "dbo.InvPromotionMasters");
            DropIndex("dbo.InvPromoBillValueBasedGetYProducts", new[] { "InvPromotionMasterId" });
            DropTable("dbo.InvPromoLowestPriceWaveOffs");
            DropTable("dbo.InvPromoCustomerCategories");
            DropTable("dbo.InvPromoBusinessTypes");
            DropTable("dbo.InvPromoBillValueBasedGetYProducts");
            DropTable("dbo.CateringMoods");
        }
    }
}
