namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka025 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Receipes", "MaterialId", c => c.Long(nullable: false));
            DropColumn("dbo.Products", "ReceipeId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Products", "ReceipeId", c => c.Int(nullable: false));
            DropColumn("dbo.Receipes", "MaterialId");
        }
    }
}
