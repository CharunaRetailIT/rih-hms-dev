namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _82 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BankBins",
                c => new
                    {
                        BankBinId = c.Int(nullable: false, identity: true),
                        CardPfx = c.String(maxLength: 100, fixedLength: true),
                        CardName = c.String(maxLength: 250, fixedLength: true),
                        CardType = c.String(maxLength: 250, fixedLength: true),
                        CardID = c.Int(nullable: false),
                        BankID = c.Int(nullable: false),
                        BankName = c.String(),
                        Rate = c.Decimal(nullable: false, precision: 18, scale: 2),
                        DiscountPercentage = c.Decimal(nullable: false, precision: 18, scale: 2),
                        DateFrom = c.DateTime(nullable: false),
                        DateTo = c.DateTime(nullable: false),
                        StartTime = c.Time(nullable: false, precision: 7),
                        EndTime = c.Time(nullable: false, precision: 7),
                        ValueFrom = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ValueTo = c.Decimal(nullable: false, precision: 18, scale: 2),
                        DiscountAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        LocationId = c.Int(nullable: false),
                        IsValidForGVSales = c.Boolean(nullable: false),
                        IsCombined = c.Boolean(nullable: false),
                        PromotionID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.BankBinId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.BankBins");
        }
    }
}
