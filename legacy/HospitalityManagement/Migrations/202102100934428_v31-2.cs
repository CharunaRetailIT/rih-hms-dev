namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v312 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ReportInfoes", "CompanyID", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ReportInfoes", "CompanyID");
        }
    }
}
