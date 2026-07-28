namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka005 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.SysLocations", "CreatedDate", c => c.DateTime());
            AlterColumn("dbo.SysLocations", "ModifiedDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.SysLocations", "ModifiedDate", c => c.DateTime(nullable: false));
            AlterColumn("dbo.SysLocations", "CreatedDate", c => c.DateTime(nullable: false));
        }
    }
}
