namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka024 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Products", "ServingUnitId", c => c.Int(nullable: false));
            DropColumn("dbo.Products", "ServingUnit");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Products", "ServingUnit", c => c.String(nullable: false, maxLength: 100));
            DropColumn("dbo.Products", "ServingUnitId");
        }
    }
}
