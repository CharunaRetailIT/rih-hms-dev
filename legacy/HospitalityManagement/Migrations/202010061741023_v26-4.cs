namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v264 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RequestNoteAcceptanceDetails", "ServingUnitId", c => c.Int(nullable: false));
            AddColumn("dbo.RequestNoteAcceptanceDetails", "ServingUnit", c => c.String());
            AddColumn("dbo.RequestNoteAccptanceHeaders", "RequestType", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.RequestNoteAccptanceHeaders", "RequestType");
            DropColumn("dbo.RequestNoteAcceptanceDetails", "ServingUnit");
            DropColumn("dbo.RequestNoteAcceptanceDetails", "ServingUnitId");
        }
    }
}
