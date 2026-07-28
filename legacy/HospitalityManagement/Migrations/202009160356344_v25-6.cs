namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v256 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CardMasters",
                c => new
                    {
                        CardMasterId = c.Long(nullable: false, identity: true),
                        CardType = c.Int(nullable: false),
                        CardCode = c.String(nullable: false, maxLength: 15),
                        CardName = c.String(nullable: false, maxLength: 50),
                        Discount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PointValue = c.Decimal(nullable: false, precision: 18, scale: 2),
                        MinimumPoints = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ReDeemPointValue = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Remark = c.String(maxLength: 150),
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
                .PrimaryKey(t => t.CardMasterId);
            
            CreateTable(
                "dbo.LoyaltyCardSchems",
                c => new
                    {
                        LoyaltyCardSchemsID = c.Long(nullable: false, identity: true),
                        CardMasterId = c.Long(nullable: false),
                        BillFromValue = c.Decimal(nullable: false, precision: 18, scale: 2),
                        BillToValue = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Increment = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PointValue = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PointPer = c.Decimal(nullable: false, precision: 18, scale: 2),
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
                .PrimaryKey(t => t.LoyaltyCardSchemsID)
                .ForeignKey("dbo.CardMasters", t => t.CardMasterId, cascadeDelete: true)
                .Index(t => t.CardMasterId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.LoyaltyCardSchems", "CardMasterId", "dbo.CardMasters");
            DropIndex("dbo.LoyaltyCardSchems", new[] { "CardMasterId" });
            DropTable("dbo.LoyaltyCardSchems");
            DropTable("dbo.CardMasters");
        }
    }
}
