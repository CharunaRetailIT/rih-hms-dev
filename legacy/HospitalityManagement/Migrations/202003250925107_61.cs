namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _61 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TransferNoteHeaders", "StockTransferType", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TransferNoteHeaders", "StockTransferType");
        }
    }
}
