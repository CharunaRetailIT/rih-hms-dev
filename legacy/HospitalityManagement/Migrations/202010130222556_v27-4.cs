namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v274 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CateringMoods", "CompanyId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.CateringMoods", "CompanyId");
        }
    }
}
