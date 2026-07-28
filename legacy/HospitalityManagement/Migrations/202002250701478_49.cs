namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _49 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.DocStatusChangeLogs", "StatusAppliedBy", c => c.String(maxLength: 20));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.DocStatusChangeLogs", "StatusAppliedBy", c => c.Int(nullable: false));
        }
    }
}
