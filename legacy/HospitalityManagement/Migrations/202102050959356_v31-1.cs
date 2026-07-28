namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v311 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CompanyUsers",
                c => new
                    {
                        CompanyUserId = c.Int(nullable: false, identity: true),
                        CompanyUserName = c.String(maxLength: 50, unicode: false),
                        CompanyUserPassword = c.String(maxLength: 50, unicode: false),
                        CompanyDbName = c.String(maxLength: 50, unicode: false),
                        CompanyId = c.Int(nullable: false),
                        LocationId = c.Int(nullable: false),
                        CreateUser = c.String(maxLength: 50, unicode: false),
                        CreateDate = c.DateTime(nullable: false),
                        ModifiedUser = c.String(maxLength: 50, unicode: false),
                        ModifiedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.CompanyUserId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.CompanyUsers");
        }
    }
}
