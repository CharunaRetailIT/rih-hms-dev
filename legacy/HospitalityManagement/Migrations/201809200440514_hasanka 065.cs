namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka065 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PurchaseOrderHeaders", "POLocationId", c => c.Long(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PurchaseOrderHeaders", "POLocationId");
        }
    }
}
