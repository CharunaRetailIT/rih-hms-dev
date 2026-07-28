namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka121 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PurchaseHeaders", "POID", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.PurchaseHeaders", "POID");
        }
    }
}
