namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _44 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.PurchaseDetails", "OrderQty", c => c.Decimal(nullable: false, precision: 18, scale: 3));
            AlterColumn("dbo.PurchaseDetails", "FreeQty", c => c.Decimal(nullable: false, precision: 18, scale: 3));
            AlterColumn("dbo.PurchaseDetails", "CurrentQty", c => c.Decimal(nullable: false, precision: 18, scale: 3));
            AlterColumn("dbo.PurchaseDetails", "BalanceQty", c => c.Decimal(nullable: false, precision: 18, scale: 3));
            AlterColumn("dbo.PurchaseDetails", "GRNQuantity", c => c.Decimal(nullable: false, precision: 18, scale: 3));
            AlterColumn("dbo.PurchaseDetails", "TOGQty", c => c.Decimal(nullable: false, precision: 18, scale: 3));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.PurchaseDetails", "TOGQty", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.PurchaseDetails", "GRNQuantity", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.PurchaseDetails", "BalanceQty", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.PurchaseDetails", "CurrentQty", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.PurchaseDetails", "FreeQty", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.PurchaseDetails", "OrderQty", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
    }
}
