namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _65 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PurchaseHeaders", "NewDocNumber", c => c.String(maxLength: 20));
            AddColumn("dbo.PurchaseOrderHeaders", "NewDocNumber", c => c.String(maxLength: 20));
            AddColumn("dbo.RequestNoteAccptanceHeaders", "NewDocNumber", c => c.String(maxLength: 20));
            AddColumn("dbo.RequestNoteHeaders", "NewDocNumber", c => c.String(maxLength: 20));
            AddColumn("dbo.StockAdjustmentHeaders", "NewDocNumber", c => c.String(maxLength: 20));
            AddColumn("dbo.TransferNoteHeaders", "NewDocNumber", c => c.String(maxLength: 20));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TransferNoteHeaders", "NewDocNumber");
            DropColumn("dbo.StockAdjustmentHeaders", "NewDocNumber");
            DropColumn("dbo.RequestNoteHeaders", "NewDocNumber");
            DropColumn("dbo.RequestNoteAccptanceHeaders", "NewDocNumber");
            DropColumn("dbo.PurchaseOrderHeaders", "NewDocNumber");
            DropColumn("dbo.PurchaseHeaders", "NewDocNumber");
        }
    }
}
