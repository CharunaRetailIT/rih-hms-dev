namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka015 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Currencies",
                c => new
                    {
                        CurrencyId = c.Int(nullable: false, identity: true),
                        CurrencyCode = c.String(maxLength: 5),
                        CurrencyDescription = c.String(maxLength: 50),
                        CurrencyFormat = c.String(maxLength: 15),
                        CurrencySymbol = c.String(maxLength: 5),
                        BuyingRate = c.Decimal(nullable: false, precision: 18, scale: 2),
                        SellingRate = c.Decimal(nullable: false, precision: 18, scale: 2),
                        AsofDate = c.DateTime(nullable: false),
                        IsActive = c.Boolean(nullable: false),
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
                .PrimaryKey(t => t.CurrencyId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Currencies");
        }
    }
}
