namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka032 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DocumentNumbers",
                c => new
                    {
                        DocumentNumberId = c.Long(nullable: false, identity: true),
                        DocumentId = c.Int(nullable: false),
                        DocumentName = c.Int(nullable: false),
                        DocumentNo = c.Long(nullable: false),
                        TempDocumentNo = c.Long(nullable: false),
                        TemplateDocumentNo = c.Long(nullable: false),
                        DocumentYear = c.Long(nullable: false),
                        PrefixCode = c.String(nullable: false),
                        GroupOfCompanyID = c.Int(nullable: false),
                        CompanyID = c.Int(nullable: false),
                        LocationId = c.Int(nullable: false),
                        CreatedUser = c.String(maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedUser = c.String(maxLength: 50),
                        ModifiedDate = c.DateTime(nullable: false),
                        DataTransfer = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.DocumentNumberId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.DocumentNumbers");
        }
    }
}
