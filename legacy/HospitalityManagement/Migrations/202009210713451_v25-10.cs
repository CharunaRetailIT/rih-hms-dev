namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v2510 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.LoyaltyCardIssueDetails", "LoyaltyCardIssueHeader_LoyaltyCardIssueHeaderId", "dbo.LoyaltyCardIssueHeaders");
            DropIndex("dbo.LoyaltyCardIssueDetails", new[] { "LoyaltyCardIssueHeader_LoyaltyCardIssueHeaderId" });
            DropColumn("dbo.LoyaltyCardIssueDetails", "LoyaltyCardIssueHeaderId");
            RenameColumn(table: "dbo.LoyaltyCardIssueDetails", name: "LoyaltyCardIssueHeader_LoyaltyCardIssueHeaderId", newName: "LoyaltyCardIssueHeaderId");
            AlterColumn("dbo.LoyaltyCardIssueDetails", "LoyaltyCardIssueHeaderId", c => c.Int(nullable: false));
            AlterColumn("dbo.LoyaltyCardIssueDetails", "LoyaltyCardIssueHeaderId", c => c.Int(nullable: false));
            CreateIndex("dbo.LoyaltyCardIssueDetails", "LoyaltyCardIssueHeaderId");
            AddForeignKey("dbo.LoyaltyCardIssueDetails", "LoyaltyCardIssueHeaderId", "dbo.LoyaltyCardIssueHeaders", "LoyaltyCardIssueHeaderId", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.LoyaltyCardIssueDetails", "LoyaltyCardIssueHeaderId", "dbo.LoyaltyCardIssueHeaders");
            DropIndex("dbo.LoyaltyCardIssueDetails", new[] { "LoyaltyCardIssueHeaderId" });
            AlterColumn("dbo.LoyaltyCardIssueDetails", "LoyaltyCardIssueHeaderId", c => c.Int());
            AlterColumn("dbo.LoyaltyCardIssueDetails", "LoyaltyCardIssueHeaderId", c => c.Long(nullable: false));
            RenameColumn(table: "dbo.LoyaltyCardIssueDetails", name: "LoyaltyCardIssueHeaderId", newName: "LoyaltyCardIssueHeader_LoyaltyCardIssueHeaderId");
            AddColumn("dbo.LoyaltyCardIssueDetails", "LoyaltyCardIssueHeaderId", c => c.Long(nullable: false));
            CreateIndex("dbo.LoyaltyCardIssueDetails", "LoyaltyCardIssueHeader_LoyaltyCardIssueHeaderId");
            AddForeignKey("dbo.LoyaltyCardIssueDetails", "LoyaltyCardIssueHeader_LoyaltyCardIssueHeaderId", "dbo.LoyaltyCardIssueHeaders", "LoyaltyCardIssueHeaderId");
        }
    }
}
