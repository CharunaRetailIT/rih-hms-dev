namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka081 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProductionNoteHeaders", "IsFinished", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ProductionNoteHeaders", "IsFinished");
        }
    }
}
