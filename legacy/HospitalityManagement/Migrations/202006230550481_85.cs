namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _85 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RequestNoteAcceptanceDetails", "IsTOG", c => c.Boolean(nullable: false));
            AddColumn("dbo.RequestNoteAccptanceHeaders", "IsTOG", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.RequestNoteAccptanceHeaders", "IsTOG");
            DropColumn("dbo.RequestNoteAcceptanceDetails", "IsTOG");
        }
    }
}
