namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasankaqw12333 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Products", "WastagePrc", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Products", "PurchasingUnit", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Products", "PurchasingUnit");
            DropColumn("dbo.Products", "WastagePrc");
        }
    }
}
