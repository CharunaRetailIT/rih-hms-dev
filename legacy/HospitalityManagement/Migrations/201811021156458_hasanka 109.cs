namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka109 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.SysUserFunctions", "FunctionName", c => c.String(nullable: false, maxLength: 30));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.SysUserFunctions", "FunctionName", c => c.String(nullable: false, maxLength: 15));
        }
    }
}
