namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka122 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.PurchaseHeaders", "POID");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PurchaseHeaders", "POID", c => c.String());
        }
    }
}
