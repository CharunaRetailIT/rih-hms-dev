namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka164 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PurchaseDetails", "CostValue", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PurchaseDetails", "CostValue");
        }
    }
}
