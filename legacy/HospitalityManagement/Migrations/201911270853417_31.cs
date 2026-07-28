namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _31 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CustomerDiscounts", "ServingUnitId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.CustomerDiscounts", "ServingUnitId");
        }
    }
}
