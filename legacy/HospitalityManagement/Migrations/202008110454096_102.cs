namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _102 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SysUserFunctions", "FormId", c => c.Int(nullable: false));
            AddColumn("dbo.SysUserGroupPermissions", "FormId", c => c.Int(nullable: false));
            AddColumn("dbo.SysUserPermissions", "FormId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SysUserPermissions", "FormId");
            DropColumn("dbo.SysUserGroupPermissions", "FormId");
            DropColumn("dbo.SysUserFunctions", "FormId");
        }
    }
}
