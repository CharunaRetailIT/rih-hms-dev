namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _48 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DocStatusChangeLogs",
                c => new
                    {
                        DocStatusChangeLogId = c.Int(nullable: false, identity: true),
                        Module = c.String(),
                        Status = c.Int(nullable: false),
                        StatusAppliedBy = c.Int(nullable: false),
                        StatusAppliedOn = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.DocStatusChangeLogId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.DocStatusChangeLogs");
        }
    }
}
