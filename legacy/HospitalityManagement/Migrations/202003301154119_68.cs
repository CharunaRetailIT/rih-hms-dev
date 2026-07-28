namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _68 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.PurchaseDetails", "PRNQuantity");
            DropColumn("dbo.PurchaseHeaders", "IsPRN");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PurchaseHeaders", "IsPRN", c => c.Boolean(nullable: false));
            AddColumn("dbo.PurchaseDetails", "PRNQuantity", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
    }
}
