namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka39 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PurchaseOrderHeaders", "VAT", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.PurchaseOrderHeaders", "NBT", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.PurchaseOrderHeaders", "TotalTaxAmount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            DropColumn("dbo.PurchaseOrderDetails", "TaxAmount1");
            DropColumn("dbo.PurchaseOrderDetails", "TaxAmount2");
            DropColumn("dbo.PurchaseOrderDetails", "TaxAmount3");
            DropColumn("dbo.PurchaseOrderDetails", "TaxAmount4");
            DropColumn("dbo.PurchaseOrderDetails", "TaxAmount5");
            DropColumn("dbo.PurchaseOrderHeaders", "TaxAmount1");
            DropColumn("dbo.PurchaseOrderHeaders", "TaxAmount2");
            DropColumn("dbo.PurchaseOrderHeaders", "TaxAmount3");
            DropColumn("dbo.PurchaseOrderHeaders", "TaxAmount4");
            DropColumn("dbo.PurchaseOrderHeaders", "TaxAmount5");
            DropColumn("dbo.PurchaseOrderHeaders", "TaxAmount");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PurchaseOrderHeaders", "TaxAmount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.PurchaseOrderHeaders", "TaxAmount5", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.PurchaseOrderHeaders", "TaxAmount4", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.PurchaseOrderHeaders", "TaxAmount3", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.PurchaseOrderHeaders", "TaxAmount2", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.PurchaseOrderHeaders", "TaxAmount1", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.PurchaseOrderDetails", "TaxAmount5", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.PurchaseOrderDetails", "TaxAmount4", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.PurchaseOrderDetails", "TaxAmount3", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.PurchaseOrderDetails", "TaxAmount2", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.PurchaseOrderDetails", "TaxAmount1", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            DropColumn("dbo.PurchaseOrderHeaders", "TotalTaxAmount");
            DropColumn("dbo.PurchaseOrderHeaders", "NBT");
            DropColumn("dbo.PurchaseOrderHeaders", "VAT");
        }
    }
}
