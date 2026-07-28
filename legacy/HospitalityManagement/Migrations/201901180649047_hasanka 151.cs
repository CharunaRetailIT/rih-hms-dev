namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka151 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RstDepartments", "DeptImage", c => c.Binary());
            AddColumn("dbo.RstDepartments", "DeptImageName", c => c.String());
            AddColumn("dbo.RstDepartments", "DeptImageType", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.RstDepartments", "DeptImageType");
            DropColumn("dbo.RstDepartments", "DeptImageName");
            DropColumn("dbo.RstDepartments", "DeptImage");
        }
    }
}
