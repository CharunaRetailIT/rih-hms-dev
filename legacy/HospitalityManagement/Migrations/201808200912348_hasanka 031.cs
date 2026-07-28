namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka031 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PaymentMethods",
                c => new
                    {
                        PaymentMethodId = c.Long(nullable: false, identity: true),
                        PaymentMethodCode = c.String(nullable: false),
                        PaymentMethodName = c.String(nullable: false),
                        CommissionRate = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PaymentType = c.Decimal(nullable: false, precision: 18, scale: 2),
                        IsPaymentType = c.Boolean(nullable: false),
                        IsReceiptType = c.Boolean(nullable: false),
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
                .PrimaryKey(t => t.PaymentMethodId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.PaymentMethods");
        }
    }
}
