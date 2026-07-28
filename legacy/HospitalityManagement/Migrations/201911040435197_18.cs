namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _18 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProductServingUnits", "DeductStockOnRecipe", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ProductServingUnits", "DeductStockOnRecipe");
        }
    }
}
