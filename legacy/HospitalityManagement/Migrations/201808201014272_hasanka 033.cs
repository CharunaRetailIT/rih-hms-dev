namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka033 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SysLocations", "CostCenter", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.SysLocations", "CostCenter");
        }
    }
}
