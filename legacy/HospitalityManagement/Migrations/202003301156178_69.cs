namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _69 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PurchaseDetails", "PRNQuantity", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.PurchaseHeaders", "IsPRN", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PurchaseHeaders", "IsPRN");
            DropColumn("dbo.PurchaseDetails", "PRNQuantity");
        }
    }
}
