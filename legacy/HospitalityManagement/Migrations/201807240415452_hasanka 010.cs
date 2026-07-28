namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka010 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SysUserGroupPermissions", "SysUserGroupId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SysUserGroupPermissions", "SysUserGroupId");
        }
    }
}
