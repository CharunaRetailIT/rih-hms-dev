namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka092 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Products", "IsAddon", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Products", "IsAddon");
        }
    }
}
