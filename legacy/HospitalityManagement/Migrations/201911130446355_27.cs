namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _27 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Employees", "DepartmentID", c => c.Int(nullable: false));
            AlterColumn("dbo.Employees", "EmployeeGroupID", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Employees", "EmployeeGroupID", c => c.String(maxLength: 30));
            AlterColumn("dbo.Employees", "DepartmentID", c => c.String(maxLength: 30));
        }
    }
}
