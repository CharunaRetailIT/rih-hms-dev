namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v263 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RequestNoteDetails", "ServingUnitId", c => c.Int(nullable: false));
            AddColumn("dbo.RequestNoteDetails", "ServingUnit", c => c.String());
            AddColumn("dbo.RequestNoteHeaders", "RequestType", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.RequestNoteHeaders", "RequestType");
            DropColumn("dbo.RequestNoteDetails", "ServingUnit");
            DropColumn("dbo.RequestNoteDetails", "ServingUnitId");
        }
    }
}
