namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v3512 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SuspendDets", "ProductRemark", c => c.String(maxLength: 200, unicode: false));
            AddColumn("dbo.SuspendDets", "IsShowOnBill", c => c.Boolean(nullable: false));
            AddColumn("dbo.SuspendDets", "ServingUnit", c => c.String(maxLength: 50, unicode: false));
            AddColumn("dbo.SuspendDets", "OrderStatus", c => c.Int());
            AddColumn("dbo.SuspendDets", "NoOfCustomers", c => c.Int(nullable: false));
            AddColumn("dbo.SuspendDets", "ServingUnitId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SuspendDets", "ServingUnitId");
            DropColumn("dbo.SuspendDets", "NoOfCustomers");
            DropColumn("dbo.SuspendDets", "OrderStatus");
            DropColumn("dbo.SuspendDets", "ServingUnit");
            DropColumn("dbo.SuspendDets", "IsShowOnBill");
            DropColumn("dbo.SuspendDets", "ProductRemark");
        }
    }
}
