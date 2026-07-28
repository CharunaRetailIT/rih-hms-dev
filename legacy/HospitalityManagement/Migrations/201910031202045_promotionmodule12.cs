namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class promotionmodule12 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.InvGiftVoucherPromotions", "BillValue", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            DropColumn("dbo.InvGiftVoucherPromotions", "GiftVoucherNo");
            DropColumn("dbo.InvGiftVoucherPromotions", "BillValueFrom");
            DropColumn("dbo.InvGiftVoucherPromotions", "BillValueTo");
        }
        
        public override void Down()
        {
            AddColumn("dbo.InvGiftVoucherPromotions", "BillValueTo", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.InvGiftVoucherPromotions", "BillValueFrom", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.InvGiftVoucherPromotions", "GiftVoucherNo", c => c.String());
            DropColumn("dbo.InvGiftVoucherPromotions", "BillValue");
        }
    }
}
