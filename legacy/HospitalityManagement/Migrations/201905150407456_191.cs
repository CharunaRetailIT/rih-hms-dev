namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _191 : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.TransferNoteDetails", "TransferNoteHeaderId");
            AddForeignKey("dbo.TransferNoteDetails", "TransferNoteHeaderId", "dbo.TransferNoteHeaders", "TransferNoteHeaderId", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TransferNoteDetails", "TransferNoteHeaderId", "dbo.TransferNoteHeaders");
            DropIndex("dbo.TransferNoteDetails", new[] { "TransferNoteHeaderId" });
        }
    }
}
