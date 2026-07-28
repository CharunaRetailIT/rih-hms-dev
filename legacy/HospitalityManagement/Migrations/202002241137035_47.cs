namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _47 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DocStatus",
                c => new
                    {
                        DocStatusId = c.Int(nullable: false, identity: true),
                        DocType = c.String(),
                        StatusId = c.Int(nullable: false),
                        Description = c.String(),
                        Order = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.DocStatusId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.DocStatus");
        }
    }
}
