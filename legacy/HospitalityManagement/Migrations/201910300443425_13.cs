namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _13 : DbMigration
    {
        public override void Up()
        {
            DropTable("dbo.Customers");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Customers",
                c => new
                    {
                        CustomerID = c.Int(nullable: false, identity: true),
                        CustomerCode = c.String(nullable: false),
                        CustomerTitle = c.String(nullable: false),
                        CustomerName = c.String(nullable: false, maxLength: 100),
                        CustomerType = c.String(),
                        CustomerCategoryId = c.Int(nullable: false),
                        BillingAddress1 = c.String(nullable: false, maxLength: 100),
                        BillingAddress2 = c.String(nullable: false, maxLength: 100),
                        BillingAddress3 = c.String(),
                        DOB = c.DateTime(),
                        NIC = c.String(nullable: false, maxLength: 12),
                        Passport = c.String(),
                        Telephone = c.String(),
                        Mobile = c.String(),
                        Fax = c.String(),
                        Email = c.String(),
                        VehicleNo = c.String(),
                        Profession = c.String(),
                        WeddingAnniversary = c.DateTime(),
                        IsActiveForLoyalty = c.Boolean(nullable: false),
                        CustomerPicture = c.Binary(),
                        CustomerPictureName = c.String(),
                        CustomerPictureType = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        IsDelete = c.Boolean(nullable: false),
                        CreditLimit = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Outstanding = c.Decimal(nullable: false, precision: 18, scale: 2),
                        EPFNo = c.String(maxLength: 50, unicode: false),
                        MembershipCardNo = c.String(maxLength: 50, unicode: false),
                        Other = c.String(maxLength: 50, unicode: false),
                        Remarks = c.String(maxLength: 200, unicode: false),
                        CustomerStatus = c.String(maxLength: 20, unicode: false),
                        GroupOfCompanyID = c.Int(nullable: false),
                        CompanyID = c.Int(nullable: false),
                        LocationId = c.Int(nullable: false),
                        CreatedUser = c.String(maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedUser = c.String(maxLength: 50),
                        ModifiedDate = c.DateTime(nullable: false),
                        DataTransfer = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.CustomerID);
            
        }
    }
}
