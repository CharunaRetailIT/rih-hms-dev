namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka063 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PurchaseOrderHeaders", "POType", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.PurchaseOrderHeaders", "POType");
        }
    }
}
