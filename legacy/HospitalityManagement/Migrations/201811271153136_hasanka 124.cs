namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka124 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PurchaseHeaders", "GRNId", c => c.Long(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PurchaseHeaders", "GRNId");
        }
    }
}
