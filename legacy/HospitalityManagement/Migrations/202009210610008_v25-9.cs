namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v259 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.LoyaltyCardIssueDetails",
                c => new
                    {
                        LoyaltyCardIssueDetailId = c.Long(nullable: false, identity: true),
                        LoyaltyCardIssueHeaderId = c.Long(nullable: false),
                        CardIssueDetailID = c.Long(nullable: false),
                        ToLocationID = c.Int(nullable: false),
                        IssueDate = c.DateTime(nullable: false),
                        CardNo = c.String(),
                        EncodeNo = c.String(),
                        IsIssued = c.Boolean(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsDelete = c.Boolean(nullable: false),
                        FefCardNo1 = c.String(maxLength: 50),
                        FefCardNo2 = c.String(maxLength: 50),
                        GroupOfCompanyID = c.Int(nullable: false),
                        CompanyID = c.Int(nullable: false),
                        LocationId = c.Int(nullable: false),
                        CreatedUser = c.String(maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedUser = c.String(maxLength: 50),
                        ModifiedDate = c.DateTime(nullable: false),
                        DataTransfer = c.Int(nullable: false),
                        LoyaltyCardIssueHeader_LoyaltyCardIssueHeaderId = c.Int(),
                    })
                .PrimaryKey(t => t.LoyaltyCardIssueDetailId)
                .ForeignKey("dbo.LoyaltyCardIssueHeaders", t => t.LoyaltyCardIssueHeader_LoyaltyCardIssueHeaderId)
                .Index(t => t.LoyaltyCardIssueHeader_LoyaltyCardIssueHeaderId);
            
            CreateTable(
                "dbo.LoyaltyCardIssueHeaders",
                c => new
                    {
                        LoyaltyCardIssueHeaderId = c.Int(nullable: false, identity: true),
                        CardIssueHeaderID = c.Long(nullable: false),
                        IssueDate = c.DateTime(nullable: false),
                        ToLocationID = c.Int(nullable: false),
                        DocumentNo = c.String(maxLength: 50),
                        Remark = c.String(maxLength: 50),
                        ReferenceNo = c.String(),
                        EmployeeID = c.Int(nullable: false),
                        GroupOfCompanyID = c.Int(nullable: false),
                        CompanyID = c.Int(nullable: false),
                        LocationId = c.Int(nullable: false),
                        CreatedUser = c.String(maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedUser = c.String(maxLength: 50),
                        ModifiedDate = c.DateTime(nullable: false),
                        DataTransfer = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.LoyaltyCardIssueHeaderId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.LoyaltyCardIssueDetails", "LoyaltyCardIssueHeader_LoyaltyCardIssueHeaderId", "dbo.LoyaltyCardIssueHeaders");
            DropIndex("dbo.LoyaltyCardIssueDetails", new[] { "LoyaltyCardIssueHeader_LoyaltyCardIssueHeaderId" });
            DropTable("dbo.LoyaltyCardIssueHeaders");
            DropTable("dbo.LoyaltyCardIssueDetails");
        }
    }
}
