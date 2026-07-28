namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka123 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PurchaseHeaders", "POID", c => c.Long(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PurchaseHeaders", "POID");
        }
    }
}
