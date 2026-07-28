namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka168 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PurchaseOrderDetails", "CostValue", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.PurchaseOrderDetails", "CostValue");
        }
    }
}
