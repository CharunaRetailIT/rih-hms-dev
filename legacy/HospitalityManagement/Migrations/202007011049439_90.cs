namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _90 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.LOGAddons", "Action", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.LOGAddons", "Action");
        }
    }
}
