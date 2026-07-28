namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _23 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.PurchaseHeaders", "TotalTax", c => c.Decimal(nullable: false, precision: 18, scale: 3));
            AlterColumn("dbo.PurchaseHeaders", "TotSellingPrice", c => c.Decimal(nullable: false, precision: 18, scale: 3));
            AlterColumn("dbo.PurchaseHeaders", "TotCostPrice", c => c.Decimal(nullable: false, precision: 18, scale: 3));
            AlterColumn("dbo.PurchaseHeaders", "TotDiscounts", c => c.Decimal(nullable: false, precision: 18, scale: 3));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.PurchaseHeaders", "TotDiscounts", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.PurchaseHeaders", "TotCostPrice", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.PurchaseHeaders", "TotSellingPrice", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.PurchaseHeaders", "TotalTax", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
    }
}
