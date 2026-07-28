namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _58 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RequestNoteHeaders", "DocumentStatus", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.RequestNoteHeaders", "DocumentStatus");
        }
    }
}
