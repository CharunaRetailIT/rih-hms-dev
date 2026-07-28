namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _80 : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.BankBins");
            AlterColumn("dbo.BankBins", "BankBinId", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.BankBins", "PromotionID", c => c.Int(nullable: false));
            AddPrimaryKey("dbo.BankBins", "BankBinId");
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.BankBins");
            AlterColumn("dbo.BankBins", "PromotionID", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.BankBins", "BankBinId", c => c.Int(nullable: false));
            AddPrimaryKey("dbo.BankBins", "BankBinId");
        }
    }
}
