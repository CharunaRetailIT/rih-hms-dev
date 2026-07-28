namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _88 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RstDepartments", "DashBoardColor", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.RstDepartments", "DashBoardColor");
        }
    }
}
