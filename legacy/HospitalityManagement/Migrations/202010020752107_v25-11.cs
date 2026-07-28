namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v2511 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.InvLoyaltyTransactions",
                c => new
                    {
                        InvLoyaltyTransactionID = c.Long(nullable: false, identity: true),
                        CustomerID = c.Long(nullable: false),
                        CustomerType = c.Short(nullable: false),
                        Receipt = c.String(maxLength: 15),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Points = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TransID = c.Short(nullable: false),
                        LocationID = c.Short(nullable: false),
                        DocumentDate = c.DateTime(nullable: false),
                        UnitNo = c.Short(nullable: false),
                        CashierID = c.Long(nullable: false),
                        DocumentTime = c.DateTime(nullable: false),
                        DiscPer = c.Decimal(nullable: false, precision: 18, scale: 2),
                        DiscAmt = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PointsRate = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Zno = c.Long(nullable: false),
                        CardNo = c.String(maxLength: 15),
                        CardType = c.Int(nullable: false),
                        LoyaltyType = c.Int(nullable: false),
                        IsGuidClaimed = c.Boolean(nullable: false),
                        IsSync = c.Boolean(nullable: false),
                        CustomerCode = c.String(maxLength: 15),
                        NIC = c.String(maxLength: 50),
                        RefNo = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.InvLoyaltyTransactionID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.InvLoyaltyTransactions");
        }
    }
}
