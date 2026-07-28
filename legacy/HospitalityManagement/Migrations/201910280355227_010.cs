namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _010 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Addons", "IsShowOnBill", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Addons", "IsShowOnBill");
        }
    }
}
