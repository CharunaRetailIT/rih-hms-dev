namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka179 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.KOTBOTDescriptions", "Description", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.KOTBOTDescriptions", "Description", c => c.String());
        }
    }
}
