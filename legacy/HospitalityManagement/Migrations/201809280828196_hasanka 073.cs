namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka073 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TransferNoteHeaders", "TOGType", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TransferNoteHeaders", "TOGType");
        }
    }
}
