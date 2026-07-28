namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _95 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.LOGUnitConversions", "Action", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.LOGUnitConversions", "Action");
        }
    }
}
