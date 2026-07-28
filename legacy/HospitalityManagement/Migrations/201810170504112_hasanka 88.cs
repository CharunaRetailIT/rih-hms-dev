namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka88 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.StockAdjustmentTypes",
                c => new
                    {
                        AdjustmentTypeId = c.Int(nullable: false, identity: true),
                        BaseType = c.String(),
                        Type = c.String(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.AdjustmentTypeId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.StockAdjustmentTypes");
        }
    }
}
