namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _71 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PurchaseHeaders", "RejectReason", c => c.String(maxLength: 200));
            AddColumn("dbo.PurchaseHeaders", "CancelReason", c => c.String(maxLength: 200));
            AddColumn("dbo.PurchaseOrderHeaders", "RejectReason", c => c.String(maxLength: 200));
            AddColumn("dbo.PurchaseOrderHeaders", "CancelReason", c => c.String(maxLength: 200));
            AddColumn("dbo.RequestNoteHeaders", "RejectReason", c => c.String(maxLength: 200));
            AddColumn("dbo.RequestNoteHeaders", "CancelReason", c => c.String(maxLength: 200));
            AddColumn("dbo.TransferNoteHeaders", "RejectReason", c => c.String(maxLength: 200));
            AddColumn("dbo.TransferNoteHeaders", "CancelReason", c => c.String(maxLength: 200));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TransferNoteHeaders", "CancelReason");
            DropColumn("dbo.TransferNoteHeaders", "RejectReason");
            DropColumn("dbo.RequestNoteHeaders", "CancelReason");
            DropColumn("dbo.RequestNoteHeaders", "RejectReason");
            DropColumn("dbo.PurchaseOrderHeaders", "CancelReason");
            DropColumn("dbo.PurchaseOrderHeaders", "RejectReason");
            DropColumn("dbo.PurchaseHeaders", "CancelReason");
            DropColumn("dbo.PurchaseHeaders", "RejectReason");
        }
    }
}
