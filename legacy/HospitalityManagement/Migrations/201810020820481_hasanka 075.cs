namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka075 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Products", "PurchasingUnit");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Products", "PurchasingUnit", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
    }
}
