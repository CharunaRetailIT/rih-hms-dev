namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka126 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TransferNoteHeaders", "TOGType", c => c.String());
            AddColumn("dbo.TransferNoteHeaders", "DocumentId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TransferNoteHeaders", "DocumentId");
            DropColumn("dbo.TransferNoteHeaders", "TOGType");
        }
    }
}
