namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka111 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.SysUserGroupPermissions", "SysUserMaster_SysUserMasterID", "dbo.SysUserMasters");
            DropIndex("dbo.SysUserGroupPermissions", new[] { "SysUserMaster_SysUserMasterID" });
            DropColumn("dbo.SysUserGroupPermissions", "SysUserMaster_SysUserMasterID");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SysUserGroupPermissions", "SysUserMaster_SysUserMasterID", c => c.Int());
            CreateIndex("dbo.SysUserGroupPermissions", "SysUserMaster_SysUserMasterID");
            AddForeignKey("dbo.SysUserGroupPermissions", "SysUserMaster_SysUserMasterID", "dbo.SysUserMasters", "SysUserMasterID");
        }
    }
}
