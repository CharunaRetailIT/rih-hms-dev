namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v335 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.JobItems", "DepartmentId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.JobItems", "DepartmentId");
        }
    }
}
