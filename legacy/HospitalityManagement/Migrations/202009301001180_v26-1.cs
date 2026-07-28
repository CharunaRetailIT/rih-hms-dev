namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v261 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RequestNoteAcceptanceDetails", "RequestedBy", c => c.String());
            AddColumn("dbo.RequestNoteDetails", "RequestedBy", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.RequestNoteDetails", "RequestedBy");
            DropColumn("dbo.RequestNoteAcceptanceDetails", "RequestedBy");
        }
    }
}
