namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _64 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.TransferNoteHeaders", "EventId", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.TransferNoteHeaders", "EventId", c => c.Int(nullable: false));
        }
    }
}
