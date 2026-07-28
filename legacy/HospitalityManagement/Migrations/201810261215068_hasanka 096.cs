namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka096 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProductionNoteDetails", "ProductCostPrice", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.ProductionNoteDetails", "ProductSellingPrice", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ProductionNoteDetails", "ProductSellingPrice");
            DropColumn("dbo.ProductionNoteDetails", "ProductCostPrice");
        }
    }
}
