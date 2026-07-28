namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v292 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CateringModeTaxes",
                c => new
                    {
                        CateringModeTaxId = c.Long(nullable: false, identity: true),
                        CateringModeId = c.Long(nullable: false),
                        TaxId = c.Long(nullable: false),
                        TaxPracentage = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TaxSequence = c.Int(nullable: false),
                        GroupOfCompanyID = c.Int(nullable: false),
                        CompanyID = c.Int(nullable: false),
                        LocationId = c.Int(nullable: false),
                        CreatedUser = c.String(maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedUser = c.String(maxLength: 50),
                        ModifiedDate = c.DateTime(nullable: false),
                        DataTransfer = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.CateringModeTaxId);
            
            CreateTable(
                "dbo.LocationTaxes",
                c => new
                    {
                        LocationTaxId = c.Long(nullable: false, identity: true),
                        TaxLocationId = c.Long(nullable: false),
                        TaxId = c.Long(nullable: false),
                        TaxPracentage = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TaxSequence = c.Int(nullable: false),
                        GroupOfCompanyID = c.Int(nullable: false),
                        CompanyID = c.Int(nullable: false),
                        LocationId = c.Int(nullable: false),
                        CreatedUser = c.String(maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedUser = c.String(maxLength: 50),
                        ModifiedDate = c.DateTime(nullable: false),
                        DataTransfer = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.LocationTaxId);
            
            CreateTable(
                "dbo.PayTypeTaxes",
                c => new
                    {
                        PayTypeTaxId = c.Long(nullable: false, identity: true),
                        PayTypeId = c.Long(nullable: false),
                        TaxId = c.Long(nullable: false),
                        TaxPracentage = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TaxSequence = c.Int(nullable: false),
                        GroupOfCompanyID = c.Int(nullable: false),
                        CompanyID = c.Int(nullable: false),
                        LocationId = c.Int(nullable: false),
                        CreatedUser = c.String(maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedUser = c.String(maxLength: 50),
                        ModifiedDate = c.DateTime(nullable: false),
                        DataTransfer = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.PayTypeTaxId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.PayTypeTaxes");
            DropTable("dbo.LocationTaxes");
            DropTable("dbo.CateringModeTaxes");
        }
    }
}
