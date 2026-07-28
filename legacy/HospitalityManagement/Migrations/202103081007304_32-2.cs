namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _322 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.InvBillValueDiscounts",
                c => new
                    {
                        InvBillValueDiscountId = c.Int(nullable: false, identity: true),
                        PromotionMasterId = c.Int(nullable: false),
                        TotalBillValueDiscount = c.Boolean(nullable: false),
                        BillValueRangeDiscount = c.Boolean(nullable: false),
                        BillValueRangeFrom = c.Decimal(nullable: false, precision: 18, scale: 2),
                        BillValueRangeTo = c.Decimal(nullable: false, precision: 18, scale: 2),
                        DiscountType = c.String(maxLength: 3, unicode: false),
                        DiscountAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        GroupOfCompanyID = c.Int(nullable: false),
                        CompanyID = c.Int(nullable: false),
                        LocationId = c.Int(nullable: false),
                        CreatedUser = c.String(maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedUser = c.String(maxLength: 50),
                        ModifiedDate = c.DateTime(nullable: false),
                        DataTransfer = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.InvBillValueDiscountId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.InvBillValueDiscounts");
        }
    }
}
