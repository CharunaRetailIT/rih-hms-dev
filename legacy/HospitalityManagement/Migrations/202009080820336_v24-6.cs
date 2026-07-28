namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v246 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.LoyaltyCustomers",
                c => new
                    {
                        LoyaltyCustomerId = c.Int(nullable: false, identity: true),
                        CardNo = c.String(maxLength: 4000),
                        CustomerId = c.Long(nullable: false),
                        NameOnCard = c.String(maxLength: 50),
                        CardMasterId = c.Long(nullable: false),
                        CardIssued = c.Boolean(nullable: false),
                        IssuedOn = c.DateTime(nullable: false),
                        ExpiryDate = c.DateTime(nullable: false),
                        RenewedOn = c.DateTime(nullable: false),
                        LedgerId = c.Long(nullable: false),
                        LedgerId2 = c.Long(nullable: false),
                        CreditLimit = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CreditPeriod = c.Int(nullable: false),
                        CPoints = c.Decimal(nullable: false, precision: 18, scale: 2),
                        EPoints = c.Decimal(nullable: false, precision: 18, scale: 2),
                        RPoints = c.Decimal(nullable: false, precision: 18, scale: 2),
                        IsReDimm = c.Boolean(nullable: false),
                        AcitiveDate = c.DateTime(nullable: false),
                        LocationID = c.Int(nullable: false),
                        CashierID = c.Int(nullable: false),
                        LoyaltyType = c.Int(nullable: false),
                        Remark = c.String(maxLength: 200),
                        SystemGeneratedCode = c.String(maxLength: 15),
                        ExpiryPoints = c.Decimal(nullable: false, precision: 18, scale: 2),
                        IsSold = c.Boolean(nullable: false),
                        GroupOfCompanyID = c.Int(nullable: false),
                        CreatedUser = c.String(maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedUser = c.String(maxLength: 4000),
                        ModifiedDate = c.DateTime(nullable: false),
                        DataTransfer = c.Int(nullable: false),
                        ExpiryPoints1 = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Discount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        SalesPersonCode = c.String(maxLength: 10),
                        LastUpdatedLocId = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                        IsCardIssued = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.LoyaltyCustomerId);
            
            AddColumn("dbo.Customers", "Gender", c => c.Int(nullable: false));
            AddColumn("dbo.Customers", "ReferenceNo1", c => c.String(maxLength: 50));
            AddColumn("dbo.Customers", "ReferenceNo2", c => c.String(maxLength: 50));
            AddColumn("dbo.Customers", "Age", c => c.Int(nullable: false));
            AddColumn("dbo.Customers", "Religion", c => c.Int(nullable: false));
            AddColumn("dbo.Customers", "Race", c => c.Int(nullable: false));
            AddColumn("dbo.Customers", "LandMark", c => c.String(maxLength: 50));
            AddColumn("dbo.Customers", "District", c => c.String(maxLength: 50));
            AddColumn("dbo.Customers", "Organization", c => c.String(maxLength: 50));
            AddColumn("dbo.Customers", "WorkAddres1", c => c.String(maxLength: 50));
            AddColumn("dbo.Customers", "WorkAddres2", c => c.String(maxLength: 50));
            AddColumn("dbo.Customers", "WorkAddres3", c => c.String(maxLength: 50));
            AddColumn("dbo.Customers", "WorkEmail", c => c.String(maxLength: 50));
            AddColumn("dbo.Customers", "WorkTelephone", c => c.String(maxLength: 50));
            AddColumn("dbo.Customers", "WorkMobile", c => c.String(maxLength: 50));
            AddColumn("dbo.Customers", "WorkFax", c => c.String(maxLength: 50));
            AddColumn("dbo.Customers", "SpouseName", c => c.String(maxLength: 50));
            AddColumn("dbo.Customers", "CivilStatus", c => c.Int(nullable: false));
            AddColumn("dbo.Customers", "SpouseDateOfBirth", c => c.DateTime(nullable: false));
            AddColumn("dbo.Customers", "DeliverTo", c => c.Int(nullable: false));
            AddColumn("dbo.Customers", "DeliverToAddress", c => c.String(maxLength: 50));
            AddColumn("dbo.Customers", "Country", c => c.String(maxLength: 50));
            AddColumn("dbo.Customers", "CustomerSince", c => c.DateTime(nullable: false));
            AddColumn("dbo.Customers", "SpecialDayType", c => c.Int(nullable: false));
            AddColumn("dbo.Customers", "SendUpdatesViaEmail", c => c.Boolean(nullable: false));
            AddColumn("dbo.Customers", "SendUpdatesViaSms", c => c.Boolean(nullable: false));
            AddColumn("dbo.Customers", "IsRegByPOS", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Customers", "IsRegByPOS");
            DropColumn("dbo.Customers", "SendUpdatesViaSms");
            DropColumn("dbo.Customers", "SendUpdatesViaEmail");
            DropColumn("dbo.Customers", "SpecialDayType");
            DropColumn("dbo.Customers", "CustomerSince");
            DropColumn("dbo.Customers", "Country");
            DropColumn("dbo.Customers", "DeliverToAddress");
            DropColumn("dbo.Customers", "DeliverTo");
            DropColumn("dbo.Customers", "SpouseDateOfBirth");
            DropColumn("dbo.Customers", "CivilStatus");
            DropColumn("dbo.Customers", "SpouseName");
            DropColumn("dbo.Customers", "WorkFax");
            DropColumn("dbo.Customers", "WorkMobile");
            DropColumn("dbo.Customers", "WorkTelephone");
            DropColumn("dbo.Customers", "WorkEmail");
            DropColumn("dbo.Customers", "WorkAddres3");
            DropColumn("dbo.Customers", "WorkAddres2");
            DropColumn("dbo.Customers", "WorkAddres1");
            DropColumn("dbo.Customers", "Organization");
            DropColumn("dbo.Customers", "District");
            DropColumn("dbo.Customers", "LandMark");
            DropColumn("dbo.Customers", "Race");
            DropColumn("dbo.Customers", "Religion");
            DropColumn("dbo.Customers", "Age");
            DropColumn("dbo.Customers", "ReferenceNo2");
            DropColumn("dbo.Customers", "ReferenceNo1");
            DropColumn("dbo.Customers", "Gender");
            DropTable("dbo.LoyaltyCustomers");
        }
    }
}
