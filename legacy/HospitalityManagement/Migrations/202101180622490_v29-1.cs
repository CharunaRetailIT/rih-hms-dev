namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v291 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.StockAdjustmentHeaders", "DocumentStatus", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.StockAdjustmentHeaders", "DocumentStatus");
        }
    }
}
