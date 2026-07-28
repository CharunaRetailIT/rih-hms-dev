namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class promotionmodule3 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.InvPromotionDetailsBuyXProducts", "Points", c => c.Long(nullable: false));
            AddColumn("dbo.InvPromotionDetailsBuyXProducts", "DiscountPercentage", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.InvPromotionDetailsBuyXProducts", "DiscountAmount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.InvPromotionDetailsBuyXProducts", "DiscountAmount");
            DropColumn("dbo.InvPromotionDetailsBuyXProducts", "DiscountPercentage");
            DropColumn("dbo.InvPromotionDetailsBuyXProducts", "Points");
        }
    }
}
