namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka43 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.PurchaseOrderHeaders", "IsUpLoad");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PurchaseOrderHeaders", "IsUpLoad", c => c.Boolean(nullable: false));
        }
    }
}
