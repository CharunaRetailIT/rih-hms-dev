namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v337 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.JobItems", "LocationId", c => c.Int(nullable: false));
            DropColumn("dbo.JobHeaders", "DepartmentId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.JobHeaders", "DepartmentId", c => c.Int(nullable: false));
            DropColumn("dbo.JobItems", "LocationId");
        }
    }
}
