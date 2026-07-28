namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _63 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TransferNoteHeaders", "EventId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TransferNoteHeaders", "EventId");
        }
    }
}
