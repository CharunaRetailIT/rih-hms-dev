namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka159 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CashierGroups", "EmployeekroupId", c => c.Int(nullable: false));
            DropColumn("dbo.CashierGroups", "EmployeeGroupId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.CashierGroups", "EmployeeGroupId", c => c.Int(nullable: false));
            DropColumn("dbo.CashierGroups", "EmployeekroupId");
        }
    }
}
