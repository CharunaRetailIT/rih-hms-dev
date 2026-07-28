namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka088 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Taxes", "IsPurchasingTax", c => c.Boolean(nullable: false));
            AddColumn("dbo.Taxes", "IsSellingTax", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Taxes", "IsSellingTax");
            DropColumn("dbo.Taxes", "IsPurchasingTax");
        }
    }
}
