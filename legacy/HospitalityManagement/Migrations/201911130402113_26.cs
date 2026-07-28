namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _26 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EmployeeGroups", "IsSteward", c => c.Boolean(nullable: false));
            AddColumn("dbo.Employees", "EmployeeGroupID", c => c.String(maxLength: 30));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Employees", "EmployeeGroupID");
            DropColumn("dbo.EmployeeGroups", "IsSteward");
        }
    }
}
