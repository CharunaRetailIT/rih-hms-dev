namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _24 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CateringMoods", "ModifiedDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.CateringMoods", "ModifiedDate");
        }
    }
}
