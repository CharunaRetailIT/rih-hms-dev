namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka030 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.PurchaseOrderDetails", "CostCentreId");
            DropColumn("dbo.PurchaseOrderDetails", "DocumentId");
            DropColumn("dbo.PurchaseOrderDetails", "DocumentNo");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PurchaseOrderDetails", "DocumentNo", c => c.String(maxLength: 20));
            AddColumn("dbo.PurchaseOrderDetails", "DocumentId", c => c.Int(nullable: false));
            AddColumn("dbo.PurchaseOrderDetails", "CostCentreId", c => c.Int(nullable: false));
        }
    }
}
