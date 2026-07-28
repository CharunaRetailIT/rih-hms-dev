namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka080 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProductionNoteHeaders", "IsTempPN", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ProductionNoteHeaders", "IsTempPN");
        }
    }
}
