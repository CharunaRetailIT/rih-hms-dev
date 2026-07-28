namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _3603 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ImportJournalDetails", "DUEDATE", c => c.DateTime(nullable: false));
            DropColumn("dbo.ImportJournalDetails", "numeric");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ImportJournalDetails", "numeric", c => c.DateTime(nullable: false));
            DropColumn("dbo.ImportJournalDetails", "DUEDATE");
        }
    }
}
