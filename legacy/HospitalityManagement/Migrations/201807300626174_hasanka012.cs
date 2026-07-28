namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka012 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.SysUserMasters", "EmployeeCode", c => c.String(nullable: false, maxLength: 15));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.SysUserMasters", "EmployeeCode", c => c.String(maxLength: 15));
        }
    }
}
