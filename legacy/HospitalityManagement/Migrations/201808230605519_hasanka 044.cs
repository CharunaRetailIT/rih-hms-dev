namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka044 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.PurchaseOrderDetails", "CurrencyId");
            DropColumn("dbo.PurchaseOrderDetails", "CurrencyRate");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PurchaseOrderDetails", "CurrencyRate", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.PurchaseOrderDetails", "CurrencyId", c => c.Int(nullable: false));
        }
    }
}
