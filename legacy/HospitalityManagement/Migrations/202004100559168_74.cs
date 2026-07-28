namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _74 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.InvPromotionMasters", "PromotionCount", c => c.Int(nullable: false));
            AddColumn("dbo.PurchaseHeaders", "RejectReason", c => c.String(maxLength: 200));
            AddColumn("dbo.PurchaseHeaders", "CancelReason", c => c.String(maxLength: 200));
            AddColumn("dbo.PurchaseOrderHeaders", "RejectReason", c => c.String(maxLength: 200));
            AddColumn("dbo.PurchaseOrderHeaders", "CancelReason", c => c.String(maxLength: 200));
            AddColumn("dbo.RequestNoteHeaders", "RejectReason", c => c.String(maxLength: 200));
            AddColumn("dbo.RequestNoteHeaders", "CancelReason", c => c.String(maxLength: 200));
            AddColumn("dbo.TransferNoteHeaders", "RejectReason", c => c.String(maxLength: 200));
            AddColumn("dbo.TransferNoteHeaders", "CancelReason", c => c.String(maxLength: 200));
            AlterColumn("dbo.InvPromotionMasters", "CustomerGroupId", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.InvPromotionMasters", "CustomerGroupId", c => c.Int(nullable: false));
            DropColumn("dbo.TransferNoteHeaders", "CancelReason");
            DropColumn("dbo.TransferNoteHeaders", "RejectReason");
            DropColumn("dbo.RequestNoteHeaders", "CancelReason");
            DropColumn("dbo.RequestNoteHeaders", "RejectReason");
            DropColumn("dbo.PurchaseOrderHeaders", "CancelReason");
            DropColumn("dbo.PurchaseOrderHeaders", "RejectReason");
            DropColumn("dbo.PurchaseHeaders", "CancelReason");
            DropColumn("dbo.PurchaseHeaders", "RejectReason");
            DropColumn("dbo.InvPromotionMasters", "PromotionCount");
        }
    }
}
