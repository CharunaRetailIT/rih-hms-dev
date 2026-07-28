namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _73 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.InvPromotionMasters", "CustomerGroupId", c => c.Int(nullable: false));
            DropColumn("dbo.InvPromotionMasters", "PromotionCount");
            DropColumn("dbo.PurchaseHeaders", "RejectReason");
            DropColumn("dbo.PurchaseHeaders", "CancelReason");
            DropColumn("dbo.PurchaseOrderHeaders", "RejectReason");
            DropColumn("dbo.PurchaseOrderHeaders", "CancelReason");
            DropColumn("dbo.RequestNoteHeaders", "RejectReason");
            DropColumn("dbo.RequestNoteHeaders", "CancelReason");
            DropColumn("dbo.TransferNoteHeaders", "RejectReason");
            DropColumn("dbo.TransferNoteHeaders", "CancelReason");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TransferNoteHeaders", "CancelReason", c => c.String(maxLength: 200));
            AddColumn("dbo.TransferNoteHeaders", "RejectReason", c => c.String(maxLength: 200));
            AddColumn("dbo.RequestNoteHeaders", "CancelReason", c => c.String(maxLength: 200));
            AddColumn("dbo.RequestNoteHeaders", "RejectReason", c => c.String(maxLength: 200));
            AddColumn("dbo.PurchaseOrderHeaders", "CancelReason", c => c.String(maxLength: 200));
            AddColumn("dbo.PurchaseOrderHeaders", "RejectReason", c => c.String(maxLength: 200));
            AddColumn("dbo.PurchaseHeaders", "CancelReason", c => c.String(maxLength: 200));
            AddColumn("dbo.PurchaseHeaders", "RejectReason", c => c.String(maxLength: 200));
            AddColumn("dbo.InvPromotionMasters", "PromotionCount", c => c.Int(nullable: false));
            AlterColumn("dbo.InvPromotionMasters", "CustomerGroupId", c => c.Int());
        }
    }
}
