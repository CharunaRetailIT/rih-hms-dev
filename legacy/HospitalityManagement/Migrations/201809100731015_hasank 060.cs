namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasank060 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PurchaseOrderHeaders", "TotSellingPrice", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.PurchaseOrderHeaders", "TotCostPrice", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.PurchaseOrderHeaders", "TotDiscounts", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PurchaseOrderHeaders", "TotDiscounts");
            DropColumn("dbo.PurchaseOrderHeaders", "TotCostPrice");
            DropColumn("dbo.PurchaseOrderHeaders", "TotSellingPrice");
        }
    }
}
