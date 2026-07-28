namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka026 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Products", "RefCode01", c => c.String(maxLength: 200));
            AddColumn("dbo.Products", "RefCode02", c => c.String(maxLength: 200));
            DropColumn("dbo.Products", "ServingUnitId");
            DropColumn("dbo.Products", "Reference");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Products", "Reference", c => c.String(nullable: false, maxLength: 200));
            AddColumn("dbo.Products", "ServingUnitId", c => c.Int(nullable: false));
            DropColumn("dbo.Products", "RefCode02");
            DropColumn("dbo.Products", "RefCode01");
        }
    }
}
