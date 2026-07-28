namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _187 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Taxes", "IsServiceCharge", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Taxes", "IsServiceCharge");
        }
    }
}
