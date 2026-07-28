namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka067 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PurchaseOrderHeaders", "TempDocNumber", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.PurchaseOrderHeaders", "TempDocNumber");
        }
    }
}
