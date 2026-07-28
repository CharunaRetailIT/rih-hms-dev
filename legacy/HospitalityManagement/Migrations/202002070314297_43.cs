namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _43 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProductionNoteHeaders", "ReceiptLocID", c => c.Int(nullable: false));
            AddColumn("dbo.ProductionNoteHeaders", "R_Zno", c => c.Int(nullable: false));
            AddColumn("dbo.ProductionNoteHeaders", "ReceiptNo", c => c.String(maxLength: 40, nullable: false, defaultValue: ""));
            AddColumn("dbo.ProductionNoteHeaders", "UnitNo", c => c.Int(nullable: false));
            AddColumn("dbo.TransactionDets", "StockLocationID", c => c.Int(nullable: false));
        }

       

        public override void Down()
        {
            DropColumn("dbo.TransactionDets", "StockLocationID");
            DropColumn("dbo.ProductionNoteHeaders", "UnitNo");
            DropColumn("dbo.ProductionNoteHeaders", "ReceiptNo");
            DropColumn("dbo.ProductionNoteHeaders", "R_Zno");
            DropColumn("dbo.ProductionNoteHeaders", "ReceiptLocID");
        }
    }
}
