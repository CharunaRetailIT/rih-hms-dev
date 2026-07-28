namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v241 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.InvAdvanceNoteDets", "IsProduction", c => c.Boolean(nullable: false));
            AddColumn("dbo.InvAdvanceNoteHeds", "IsProduction", c => c.Boolean(nullable: false));
            AddColumn("dbo.InvAdvanceNoteHeds", "ProcessLoc", c => c.Int(nullable: false));
            AddColumn("dbo.InvAdvanceNoteHeds", "PickupLoc", c => c.Int(nullable: false));
            AddColumn("dbo.InvAdvanceNoteHeds", "Status", c => c.Boolean(nullable: false));
            AlterColumn("dbo.Events", "EventCode", c => c.String(nullable: false));
            AlterColumn("dbo.Events", "EventName", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Events", "EventName", c => c.String());
            AlterColumn("dbo.Events", "EventCode", c => c.String());
            DropColumn("dbo.InvAdvanceNoteHeds", "Status");
            DropColumn("dbo.InvAdvanceNoteHeds", "PickupLoc");
            DropColumn("dbo.InvAdvanceNoteHeds", "ProcessLoc");
            DropColumn("dbo.InvAdvanceNoteHeds", "IsProduction");
            DropColumn("dbo.InvAdvanceNoteDets", "IsProduction");
        }
    }
}
