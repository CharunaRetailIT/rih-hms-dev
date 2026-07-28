namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class promotionmodule11 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.InvGiftVoucherPromotions",
                c => new
                    {
                        InvGiftVoucherPromotionsId = c.Int(nullable: false, identity: true),
                        PromotionMasterId = c.Int(nullable: false),
                        GiftVoucherNo = c.String(),
                        GiftVoucherAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        BillValueFrom = c.Decimal(nullable: false, precision: 18, scale: 2),
                        BillValueTo = c.Decimal(nullable: false, precision: 18, scale: 2),
                        NoOfOccurrences = c.Int(nullable: false),
                        Remarks = c.String(),
                        GroupOfCompanyID = c.Int(nullable: false),
                        CompanyID = c.Int(nullable: false),
                        LocationId = c.Int(nullable: false),
                        CreatedUser = c.String(maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedUser = c.String(maxLength: 50),
                        ModifiedDate = c.DateTime(nullable: false),
                        DataTransfer = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.InvGiftVoucherPromotionsId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.InvGiftVoucherPromotions");
        }
    }
}
