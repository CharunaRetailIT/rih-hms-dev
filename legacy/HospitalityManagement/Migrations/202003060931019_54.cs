namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _54 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.PurchaseOrderDetails", "IsGRN");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PurchaseOrderDetails", "IsGRN", c => c.Boolean(nullable: false));
        }
    }
}
