namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v271 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.InvAdvanceNoteHeds", "CompanyId", c => c.Int(nullable: false));
            AddColumn("dbo.RequestNoteAccptanceHeaders", "CompanyId", c => c.Int(nullable: false));
            AddColumn("dbo.RequestNoteHeaders", "CompanyId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.RequestNoteHeaders", "CompanyId");
            DropColumn("dbo.RequestNoteAccptanceHeaders", "CompanyId");
            DropColumn("dbo.InvAdvanceNoteHeds", "CompanyId");
        }
    }
}
