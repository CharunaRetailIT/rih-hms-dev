namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka086 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Products", "IsDiscount", c => c.Boolean(nullable: false));
            AddColumn("dbo.Products", "IsCostOnReceipe", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Products", "IsCostOnReceipe");
            DropColumn("dbo.Products", "IsDiscount");
        }
    }
}
