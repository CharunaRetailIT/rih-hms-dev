namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka176 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.KOTBOTDescriptions", "Type", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.KOTBOTDescriptions", "Type");
        }
    }
}
