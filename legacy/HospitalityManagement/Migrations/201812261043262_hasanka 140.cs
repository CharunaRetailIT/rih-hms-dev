namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka140 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProductionNoteDetails", "ServingUnitId", c => c.Long(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ProductionNoteDetails", "ServingUnitId");
        }
    }
}
