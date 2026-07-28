namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _66 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PurchaseHeaders", "IsPRN", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PurchaseHeaders", "IsPRN");
        }
    }
}
