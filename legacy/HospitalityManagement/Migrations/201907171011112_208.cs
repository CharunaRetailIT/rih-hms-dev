namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _208 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TransferNoteHeaders", "TOGDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TransferNoteHeaders", "TOGDate");
        }
    }
}
