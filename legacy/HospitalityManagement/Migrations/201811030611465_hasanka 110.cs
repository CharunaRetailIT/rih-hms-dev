namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka110 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SysUserGroupPermissions", "SysUserMaster_SysUserMasterID", c => c.Int());
            AlterColumn("dbo.SysUserMasters", "EmployeeCode", c => c.String(maxLength: 15));
            CreateIndex("dbo.SysUserGroupPermissions", "SysUserMaster_SysUserMasterID");
            AddForeignKey("dbo.SysUserGroupPermissions", "SysUserMaster_SysUserMasterID", "dbo.SysUserMasters", "SysUserMasterID");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SysUserGroupPermissions", "SysUserMaster_SysUserMasterID", "dbo.SysUserMasters");
            DropIndex("dbo.SysUserGroupPermissions", new[] { "SysUserMaster_SysUserMasterID" });
            AlterColumn("dbo.SysUserMasters", "EmployeeCode", c => c.String(nullable: false, maxLength: 15));
            DropColumn("dbo.SysUserGroupPermissions", "SysUserMaster_SysUserMasterID");
        }
    }
}
