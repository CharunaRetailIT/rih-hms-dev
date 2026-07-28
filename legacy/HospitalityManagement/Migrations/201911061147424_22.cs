namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _22 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.PurchaseDetails", "CostPrice", c => c.Decimal(nullable: false, precision: 18, scale: 3));
            AlterColumn("dbo.PurchaseDetails", "SellingPrice", c => c.Decimal(nullable: false, precision: 18, scale: 3));
            AlterColumn("dbo.PurchaseDetails", "GrossAmount", c => c.Decimal(nullable: false, precision: 18, scale: 3));
            AlterColumn("dbo.PurchaseDetails", "DiscountPercentage", c => c.Decimal(nullable: false, precision: 18, scale: 3));
            AlterColumn("dbo.PurchaseDetails", "DiscountAmount", c => c.Decimal(nullable: false, precision: 18, scale: 3));
            AlterColumn("dbo.PurchaseDetails", "NetAmount", c => c.Decimal(nullable: false, precision: 18, scale: 3));
            AlterColumn("dbo.PurchaseHeaders", "OtherChargers", c => c.Decimal(nullable: false, precision: 18, scale: 3));
            AlterColumn("dbo.PurchaseHeaders", "NetAmount", c => c.Decimal(nullable: false, precision: 18, scale: 3));
            AlterColumn("dbo.PurchaseHeaders", "LineDiscountTotal", c => c.Decimal(nullable: false, precision: 18, scale: 3));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.PurchaseHeaders", "LineDiscountTotal", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.PurchaseHeaders", "NetAmount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.PurchaseHeaders", "OtherChargers", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.PurchaseDetails", "NetAmount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.PurchaseDetails", "DiscountAmount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.PurchaseDetails", "DiscountPercentage", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.PurchaseDetails", "GrossAmount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.PurchaseDetails", "SellingPrice", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.PurchaseDetails", "CostPrice", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
    }
}
