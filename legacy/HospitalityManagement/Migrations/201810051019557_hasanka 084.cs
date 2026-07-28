namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka084 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Products", "PurchasingUnit", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Products", "PurchasingUnit");
        }
    }
}
