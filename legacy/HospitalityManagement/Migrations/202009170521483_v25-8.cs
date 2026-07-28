namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v258 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CardGenerationLocationSettings",
                c => new
                    {
                        CardGenerationLocationSettingId = c.Long(nullable: false, identity: true),
                        CardNoLength = c.Int(nullable: false),
                        CardStartingNo = c.Long(nullable: false),
                        EncodeStartingNo = c.Long(nullable: false),
                        IsDelete = c.Boolean(nullable: false),
                        GroupOfCompanyID = c.Int(nullable: false),
                        CompanyID = c.Int(nullable: false),
                        LocationId = c.Int(nullable: false),
                        CreatedUser = c.String(maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedUser = c.String(maxLength: 50),
                        ModifiedDate = c.DateTime(nullable: false),
                        DataTransfer = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.CardGenerationLocationSettingId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.CardGenerationLocationSettings");
        }
    }
}
