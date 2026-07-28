namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _60 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PurchaseHeaders", "EventId", c => c.Int());
            AddColumn("dbo.PurchaseOrderHeaders", "EventId", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.PurchaseOrderHeaders", "EventId");
            DropColumn("dbo.PurchaseHeaders", "EventId");
        }
    }
}
