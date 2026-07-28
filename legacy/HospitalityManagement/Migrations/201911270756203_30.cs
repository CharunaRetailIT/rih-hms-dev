namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _30 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CustomerDiscounts",
                c => new
                    {
                        CustomerDiscountId = c.Int(nullable: false, identity: true),
                        CustomerId = c.Int(nullable: false),
                        CustomerCode = c.String(maxLength: 20),
                        ProductId = c.Int(nullable: false),
                        ProductCode = c.String(maxLength: 20),
                        DiscountAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        DiscountPercentage = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CustomerSellPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CreditDiscountAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CreditDiscountPercentage = c.Decimal(nullable: false, precision: 18, scale: 2),
                        DateFrom = c.DateTime(nullable: false),
                        DateTo = c.DateTime(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        GroupOfCompanyID = c.Int(nullable: false),
                        CompanyID = c.Int(nullable: false),
                        LocationId = c.Int(nullable: false),
                        CreatedUser = c.String(maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedUser = c.String(maxLength: 50),
                        ModifiedDate = c.DateTime(nullable: false),
                        DataTransfer = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.CustomerDiscountId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.CustomerDiscounts");
        }
    }
}
