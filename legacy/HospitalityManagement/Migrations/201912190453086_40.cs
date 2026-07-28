namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _40 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RequestNoteHeaders", "IsApproved", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.RequestNoteHeaders", "IsApproved");
        }
    }
}
