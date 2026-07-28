namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka040 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.DocumentNumbers", "DocumentName", c => c.String(nullable: false));
            AlterColumn("dbo.DocumentNumbers", "TemplateDocumentNo", c => c.String(nullable: false));
            AlterColumn("dbo.DocumentNumbers", "DocumentYear", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.DocumentNumbers", "DocumentYear", c => c.Long(nullable: false));
            AlterColumn("dbo.DocumentNumbers", "TemplateDocumentNo", c => c.Long(nullable: false));
            AlterColumn("dbo.DocumentNumbers", "DocumentName", c => c.Int(nullable: false));
        }
    }
}
