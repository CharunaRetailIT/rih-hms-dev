namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka069 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PurchaseHeaders", "IsTempGRN", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PurchaseHeaders", "IsTempGRN");
        }
    }
}
