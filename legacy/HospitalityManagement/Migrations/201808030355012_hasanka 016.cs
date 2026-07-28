namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka016 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CurrencyHistories",
                c => new
                    {
                        CurrencyHistoryId = c.Int(nullable: false, identity: true),
                        CurrencyId = c.Int(nullable: false),
                        BuyingRate = c.Decimal(nullable: false, precision: 18, scale: 2),
                        SellingRate = c.Decimal(nullable: false, precision: 18, scale: 2),
                        AsofDate = c.DateTime(nullable: false),
                        GroupOfCompanyID = c.Int(nullable: false),
                        CompanyID = c.Int(nullable: false),
                        LocationId = c.Int(nullable: false),
                        CreatedUser = c.String(maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedUser = c.String(maxLength: 50),
                        ModifiedDate = c.DateTime(nullable: false),
                        DataTransfer = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.CurrencyHistoryId);
            
            CreateTable(
                "dbo.UnitOfMeasures",
                c => new
                    {
                        UnitOfMeasureId = c.Long(nullable: false, identity: true),
                        UnitOfMeasureCode = c.String(nullable: false, maxLength: 15),
                        UnitOfMeasureName = c.String(nullable: false, maxLength: 50),
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
                .PrimaryKey(t => t.UnitOfMeasureId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.UnitOfMeasures");
            DropTable("dbo.CurrencyHistories");
        }
    }
}
