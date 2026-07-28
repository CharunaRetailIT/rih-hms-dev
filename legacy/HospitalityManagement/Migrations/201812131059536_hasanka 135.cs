namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka135 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PurchaseHeaders", "GRNType", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.PurchaseHeaders", "GRNType");
        }
    }
}
