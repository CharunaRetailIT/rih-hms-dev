namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanak090 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.StockAdjustmentDetails", "ProductName", c => c.String());
            AddColumn("dbo.StockAdjustmentDetails", "BaseType", c => c.String());
            AddColumn("dbo.StockAdjustmentDetails", "Reason", c => c.String());
            DropColumn("dbo.StockAdjustmentDetails", "AdjustmentTypeId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.StockAdjustmentDetails", "AdjustmentTypeId", c => c.Int(nullable: false));
            DropColumn("dbo.StockAdjustmentDetails", "Reason");
            DropColumn("dbo.StockAdjustmentDetails", "BaseType");
            DropColumn("dbo.StockAdjustmentDetails", "ProductName");
        }
    }
}
