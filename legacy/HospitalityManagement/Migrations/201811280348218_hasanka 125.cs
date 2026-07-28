namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka125 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.TransferNoteHeaders", "TOGType");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TransferNoteHeaders", "TOGType", c => c.Boolean(nullable: false));
        }
    }
}
