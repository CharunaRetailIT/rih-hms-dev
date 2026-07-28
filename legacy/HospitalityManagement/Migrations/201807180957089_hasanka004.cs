namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka004 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.SysLocations", "LocationIP", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.SysLocations", "LocationIP", c => c.Boolean(nullable: false));
        }
    }
}
