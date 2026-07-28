namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _55 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PurchaseOrderDetails", "IsGRN", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PurchaseOrderDetails", "IsGRN");
        }
    }
}
