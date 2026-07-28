namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka076 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Products", "PurchasingUnit", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Products", "PurchasingUnit");
        }
    }
}
