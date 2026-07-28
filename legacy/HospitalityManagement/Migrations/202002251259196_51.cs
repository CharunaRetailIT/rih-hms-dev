namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _51 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Products", "AutoProduction", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Products", "AutoProduction");
        }
    }
}
