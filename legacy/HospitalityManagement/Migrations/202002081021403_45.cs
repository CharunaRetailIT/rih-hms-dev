namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _45 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.PurchaseDetails", "CostValue", c => c.Decimal(nullable: false, precision: 18, scale: 3));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.PurchaseDetails", "CostValue", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
    }
}
