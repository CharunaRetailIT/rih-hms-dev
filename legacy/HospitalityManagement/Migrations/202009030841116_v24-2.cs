namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v242 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.InvAdvanceNoteHeds", "Status");
        }
        
        public override void Down()
        {
            AddColumn("dbo.InvAdvanceNoteHeds", "Status", c => c.Boolean(nullable: false));
        }
    }
}
