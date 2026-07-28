namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v272 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BankBins", "CompanyId", c => c.Int(nullable: false));
            AddColumn("dbo.InvPromoLowestPriceWaveOffs", "CompanyId", c => c.Int(nullable: false));
            AddColumn("dbo.LoyaltyCustomers", "CompanyId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.LoyaltyCustomers", "CompanyId");
            DropColumn("dbo.InvPromoLowestPriceWaveOffs", "CompanyId");
            DropColumn("dbo.BankBins", "CompanyId");
        }
    }
}
