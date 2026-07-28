namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v251 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CardMasters",
                c => new
                    {
                        CardMasterId = c.Long(nullable: false, identity: true),
                        CardType = c.Int(nullable: false),
                        CardCode = c.String(maxLength: 15),
                        CardName = c.String(maxLength: 50),
                        Discount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PointValue = c.Decimal(nullable: false, precision: 18, scale: 2),
                        MinimumPoints = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ReDeemPointValue = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Remark = c.String(maxLength: 150),
                        IsDelete = c.Decimal(nullable: false, precision: 18, scale: 2),
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
                        CardMasterID = c.Long(nullable: false),
                        BillFromValue = c.Decimal(nullable: false, precision: 18, scale: 2),
                        BillToValue = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Increment = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PointValue = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PointPer = c.Decimal(nullable: false, precision: 18, scale: 2),
                        IsDelete = c.Decimal(nullable: false, precision: 18, scale: 2),
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
                .ForeignKey("dbo.CardMasters", t => t.CardMasterID, cascadeDelete: true)
                .Index(t => t.CardMasterID);
            
            CreateTable(
                "dbo.ReferenceTypes",
                c => new
                    {
                        ReferenceTypeId = c.Int(nullable: false, identity: true),
                        LookupType = c.String(maxLength: 25),
                        LookupKey = c.Int(nullable: false),
                        LookupValue = c.String(maxLength: 100),
                        Remark = c.String(maxLength: 100),
                        IsDelete = c.Int(nullable: false),
                        GroupOfCompanyID = c.Int(nullable: false),
                        CompanyID = c.Int(nullable: false),
                        LocationId = c.Int(nullable: false),
                        CreatedUser = c.String(maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedUser = c.String(maxLength: 50),
                        ModifiedDate = c.DateTime(nullable: false),
                        DataTransfer = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ReferenceTypeId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.LoyaltyCardSchems", "CardMasterID", "dbo.CardMasters");
            DropIndex("dbo.LoyaltyCardSchems", new[] { "CardMasterID" });
            DropTable("dbo.ReferenceTypes");
            DropTable("dbo.LoyaltyCardSchems");
            DropTable("dbo.CardMasters");
        }
    }
}
