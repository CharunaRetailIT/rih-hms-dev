namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v257 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.LoyaltyCardGenerationDetails",
                c => new
                    {
                        LoyaltyCardGenerationDetailId = c.Long(nullable: false, identity: true),
                        CardGenerationDetailID = c.Long(nullable: false),
                        LoyaltyCardGenerationHeaderID = c.Long(nullable: false),
                        CardPrefix = c.String(maxLength: 10),
                        CardLength = c.Int(nullable: false),
                        CardStartingNo = c.Int(nullable: false),
                        EncodeLength = c.Int(nullable: false),
                        EncodeStartingNo = c.Int(nullable: false),
                        EncodePrefix = c.String(maxLength: 3),
                        GeneratedDate = c.DateTime(nullable: false),
                        CardNo = c.String(maxLength: 50),
                        EncodeNo = c.String(maxLength: 50),
                        IsIssued = c.Boolean(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsDelete = c.Boolean(nullable: false),
                        RefCardNo1 = c.String(maxLength: 50),
                        RefCardNo2 = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.LoyaltyCardGenerationDetailId)
                .ForeignKey("dbo.LoyaltyCardGenerationHeaders", t => t.LoyaltyCardGenerationHeaderID, cascadeDelete: true)
                .Index(t => t.LoyaltyCardGenerationHeaderID);
            
            CreateTable(
                "dbo.LoyaltyCardGenerationHeaders",
                c => new
                    {
                        LoyaltyCardGenerationHeaderId = c.Long(nullable: false, identity: true),
                        CardGenerationHeaderID = c.Long(nullable: false),
                        CardPrefix = c.String(maxLength: 10),
                        CardLength = c.Int(nullable: false),
                        CardStartingNo = c.Int(nullable: false),
                        EncodeLength = c.Int(nullable: false),
                        EncodeStartingNo = c.Int(nullable: false),
                        EncodePrefix = c.String(maxLength: 3),
                        GeneratedDate = c.DateTime(nullable: false),
                        CardMasterId = c.Long(nullable: false),
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
                .PrimaryKey(t => t.LoyaltyCardGenerationHeaderId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.LoyaltyCardGenerationDetails", "LoyaltyCardGenerationHeaderID", "dbo.LoyaltyCardGenerationHeaders");
            DropIndex("dbo.LoyaltyCardGenerationDetails", new[] { "LoyaltyCardGenerationHeaderID" });
            DropTable("dbo.LoyaltyCardGenerationHeaders");
            DropTable("dbo.LoyaltyCardGenerationDetails");
        }
    }
}
