namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v313 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.SysUserMasters", "UserName", c => c.String(nullable: false, maxLength: 50));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.SysUserMasters", "UserName", c => c.String(nullable: false, maxLength: 15));
        }
    }
}
