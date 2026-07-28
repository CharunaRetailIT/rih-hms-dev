namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka129 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.StockAdjustmentHeaders", "DocumentId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.StockAdjustmentHeaders", "DocumentId");
        }
    }
}
