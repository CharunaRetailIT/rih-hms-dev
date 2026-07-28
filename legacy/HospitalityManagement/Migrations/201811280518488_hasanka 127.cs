namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka127 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RequestNoteHeaders", "DocumentId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.RequestNoteHeaders", "DocumentId");
        }
    }
}
