namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _200 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.KOTBOTDescriptions", "ModifiedDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.KOTBOTDescriptions", "ModifiedDate");
        }
    }
}
