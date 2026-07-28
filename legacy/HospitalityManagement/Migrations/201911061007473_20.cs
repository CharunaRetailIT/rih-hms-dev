namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _20 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CateringMoods", "IsServiceCharge", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.CateringMoods", "IsServiceCharge");
        }
    }
}
