namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka112 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SysUserPermissions", "GroupId", c => c.Long(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SysUserPermissions", "GroupId");
        }
    }
}
