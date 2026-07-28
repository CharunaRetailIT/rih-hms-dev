namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _38 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PurchaseOrderHeaders", "IsActive", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PurchaseOrderHeaders", "IsActive");
        }
    }
}
