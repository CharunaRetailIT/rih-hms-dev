namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _193 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SysLocations", "IsShowRoom", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SysLocations", "IsShowRoom");
        }
    }
}
