namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _3606 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ImportJournalDetails", "SEQNO", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ImportJournalDetails", "SEQNO", c => c.Decimal(nullable: false, precision: 18, scale: 2, storeType: "numeric"));
        }
    }
}
