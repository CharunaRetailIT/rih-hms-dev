namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _32 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.InvPromotionDetailsProductDis",
                c => new
                    {
                        InvPromotionDetailsProductDisId = c.Long(nullable: false, identity: true),
                        InvPromotionMasterID = c.Long(nullable: false),
                        CompanyID = c.Int(nullable: false),
                        LocationID = c.Int(nullable: false),
                        ProductID = c.Int(nullable: false),
                        ServingUunitId = c.Int(nullable: false),
                        UnitOfMeasureID = c.Long(nullable: false),
                        Rate = c.Decimal(nullable: false, precision: 18, scale: 2),
                        FromQty = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ToQty = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Points = c.Long(nullable: false),
                        DiscountPercentage = c.Decimal(nullable: false, precision: 18, scale: 2),
                        DiscountAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.InvPromotionDetailsProductDisId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.InvPromotionDetailsProductDis");
        }
    }
}
