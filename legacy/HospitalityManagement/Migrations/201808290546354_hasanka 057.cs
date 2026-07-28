namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka057 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.PurchaseOrderDetails", "DocumentStatus");
            DropColumn("dbo.PurchaseOrderDetails", "ScanDocument");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PurchaseOrderDetails", "ScanDocument", c => c.Binary());
            AddColumn("dbo.PurchaseOrderDetails", "DocumentStatus", c => c.Int(nullable: false));
        }
    }
}
