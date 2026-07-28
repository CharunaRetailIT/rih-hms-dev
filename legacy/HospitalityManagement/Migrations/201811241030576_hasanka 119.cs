namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka119 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Products", "MaxPrice", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Products", "MinPrice", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Products", "DiscountPrecentage", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Products", "MaximumDiscount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Products", "FixedDiscountPercentage", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Products", "FixedDiscountAmount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Products", "MaximumDiscountPercentage", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            DropColumn("dbo.ProductStockMasters", "MinimumPrice");
            DropColumn("dbo.ProductStockMasters", "MaxPrice");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ProductStockMasters", "MaxPrice", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.ProductStockMasters", "MinimumPrice", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            DropColumn("dbo.Products", "MaximumDiscountPercentage");
            DropColumn("dbo.Products", "FixedDiscountAmount");
            DropColumn("dbo.Products", "FixedDiscountPercentage");
            DropColumn("dbo.Products", "MaximumDiscount");
            DropColumn("dbo.Products", "DiscountPrecentage");
            DropColumn("dbo.Products", "MinPrice");
            DropColumn("dbo.Products", "MaxPrice");
        }
    }
}
