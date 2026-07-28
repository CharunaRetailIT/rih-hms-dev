namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka072 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.TransferNoteDetails", "TransferNoteHeaderID", "dbo.TransferNoteHeaders");
            DropIndex("dbo.TransferNoteDetails", new[] { "TransferNoteHeaderID" });
            AddColumn("dbo.TransferNoteHeaders", "FromLocationId", c => c.Int(nullable: false));
            AddColumn("dbo.TransferNoteHeaders", "TotSellingPrice", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.TransferNoteHeaders", "TotCostPrice", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.TransferNoteDetails", "SerialNo", c => c.String());
            DropColumn("dbo.TransferNoteDetails", "CostCentre_ID");
            DropColumn("dbo.TransferNoteDetails", "Document_ID");
            DropColumn("dbo.TransferNoteDetails", "Document_No");
            DropColumn("dbo.TransferNoteDetails", "Document_Date");
            DropColumn("dbo.TransferNoteHeaders", "JobClassID");
            DropColumn("dbo.TransferNoteHeaders", "DocumentID");
            DropColumn("dbo.TransferNoteHeaders", "TransferTypeID");
            DropColumn("dbo.TransferNoteHeaders", "PosReferenceNo");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TransferNoteHeaders", "PosReferenceNo", c => c.String(maxLength: 10));
            AddColumn("dbo.TransferNoteHeaders", "TransferTypeID", c => c.Int(nullable: false));
            AddColumn("dbo.TransferNoteHeaders", "DocumentID", c => c.Int(nullable: false));
            AddColumn("dbo.TransferNoteHeaders", "JobClassID", c => c.Long(nullable: false));
            AddColumn("dbo.TransferNoteDetails", "Document_Date", c => c.DateTime(nullable: false));
            AddColumn("dbo.TransferNoteDetails", "Document_No", c => c.String(maxLength: 20));
            AddColumn("dbo.TransferNoteDetails", "Document_ID", c => c.Int(nullable: false));
            AddColumn("dbo.TransferNoteDetails", "CostCentre_ID", c => c.Int(nullable: false));
            AlterColumn("dbo.TransferNoteDetails", "SerialNo", c => c.Int(nullable: false));
            DropColumn("dbo.TransferNoteHeaders", "TotCostPrice");
            DropColumn("dbo.TransferNoteHeaders", "TotSellingPrice");
            DropColumn("dbo.TransferNoteHeaders", "FromLocationId");
            CreateIndex("dbo.TransferNoteDetails", "TransferNoteHeaderID");
            AddForeignKey("dbo.TransferNoteDetails", "TransferNoteHeaderID", "dbo.TransferNoteHeaders", "TransferNoteHeaderID", cascadeDelete: true);
        }
    }
}
