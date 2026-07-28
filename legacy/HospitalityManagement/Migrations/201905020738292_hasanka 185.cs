namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka185 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Taxes", "EffectivePercentage");
            DropColumn("dbo.Taxes", "EffectiveDate");
            DropColumn("dbo.Taxes", "Tax1");
            DropColumn("dbo.Taxes", "Tax2");
            DropColumn("dbo.Taxes", "Tax3");
            DropColumn("dbo.Taxes", "Tax4");
            DropColumn("dbo.Taxes", "Tax5");
            DropColumn("dbo.Taxes", "PrintOrder");
            DropColumn("dbo.Taxes", "LedgerID");
            DropColumn("dbo.Taxes", "PaidLedgerID");
            DropColumn("dbo.Taxes", "Remark");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Taxes", "Remark", c => c.String(maxLength: 150));
            AddColumn("dbo.Taxes", "PaidLedgerID", c => c.Long(nullable: false));
            AddColumn("dbo.Taxes", "LedgerID", c => c.Long(nullable: false));
            AddColumn("dbo.Taxes", "PrintOrder", c => c.Int(nullable: false));
            AddColumn("dbo.Taxes", "Tax5", c => c.Boolean(nullable: false));
            AddColumn("dbo.Taxes", "Tax4", c => c.Boolean(nullable: false));
            AddColumn("dbo.Taxes", "Tax3", c => c.Boolean(nullable: false));
            AddColumn("dbo.Taxes", "Tax2", c => c.Boolean(nullable: false));
            AddColumn("dbo.Taxes", "Tax1", c => c.Boolean(nullable: false));
            AddColumn("dbo.Taxes", "EffectiveDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Taxes", "EffectivePercentage", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
    }
}
