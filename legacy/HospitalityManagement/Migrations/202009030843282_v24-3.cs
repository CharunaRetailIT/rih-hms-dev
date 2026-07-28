namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v243 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.InvAdvanceNoteHeds", "Status", c => c.Boolean(nullable: false,defaultValue:true));
        }
        
        public override void Down()
        {
            DropColumn("dbo.InvAdvanceNoteHeds", "Status");
        }
    }
}
