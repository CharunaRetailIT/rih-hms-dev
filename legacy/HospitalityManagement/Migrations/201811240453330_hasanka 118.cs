namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka118 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Products", "NameOnInvoice", c => c.String(maxLength: 50));
            AddColumn("dbo.Products", "IsPackItem", c => c.Boolean(nullable: false));
            AddColumn("dbo.Products", "PackSize", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Products", "PackPrice", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Products", "IsPromotion", c => c.Boolean(nullable: false));
            AddColumn("dbo.Products", "IsFreeIssue", c => c.Boolean(nullable: false));
            AddColumn("dbo.Products", "IsExpiry", c => c.Boolean(nullable: false));
            AddColumn("dbo.Products", "IsTax", c => c.Boolean(nullable: false));
            AddColumn("dbo.Products", "WeightPerUnit", c => c.Int(nullable: false));
            AddColumn("dbo.Products", "IsUnderCost", c => c.Boolean(nullable: false));
            AddColumn("dbo.Products", "IsBundle", c => c.Boolean(nullable: false));
            AddColumn("dbo.ProductStockMasters", "MaximumDiscount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.ProductStockMasters", "FixedDiscountPercentage", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.ProductStockMasters", "FixedDiscountAmount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.ProductStockMasters", "MaximumDiscountPercentage", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ProductStockMasters", "MaximumDiscountPercentage");
            DropColumn("dbo.ProductStockMasters", "FixedDiscountAmount");
            DropColumn("dbo.ProductStockMasters", "FixedDiscountPercentage");
            DropColumn("dbo.ProductStockMasters", "MaximumDiscount");
            DropColumn("dbo.Products", "IsBundle");
            DropColumn("dbo.Products", "IsUnderCost");
            DropColumn("dbo.Products", "WeightPerUnit");
            DropColumn("dbo.Products", "IsTax");
            DropColumn("dbo.Products", "IsExpiry");
            DropColumn("dbo.Products", "IsFreeIssue");
            DropColumn("dbo.Products", "IsPromotion");
            DropColumn("dbo.Products", "PackPrice");
            DropColumn("dbo.Products", "PackSize");
            DropColumn("dbo.Products", "IsPackItem");
            DropColumn("dbo.Products", "NameOnInvoice");
        }
    }
}
