namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v265 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RequestNoteAccptanceHeaders", "IsProductionComplete", c => c.Boolean(nullable: false));
            AddColumn("dbo.RequestNoteAccptanceHeaders", "IsTOGComplete", c => c.Boolean(nullable: false));
            AddColumn("dbo.RequestNoteAccptanceHeaders", "IsPOComplete", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.RequestNoteAccptanceHeaders", "IsPOComplete");
            DropColumn("dbo.RequestNoteAccptanceHeaders", "IsTOGComplete");
            DropColumn("dbo.RequestNoteAccptanceHeaders", "IsProductionComplete");
        }
    }
}
