namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka117 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Suppliers", "TaxID1", c => c.Int(nullable: false, defaultValue: 0));
            AddColumn("dbo.Suppliers", "TaxID2", c => c.Int(nullable: false, defaultValue: 0));
            AddColumn("dbo.Suppliers", "TaxID3", c => c.Int(nullable: false, defaultValue: 0));
            AddColumn("dbo.Suppliers", "TaxID4", c => c.Int(nullable: false, defaultValue: 0));
            AddColumn("dbo.Suppliers", "TaxID5", c => c.Int(nullable: false, defaultValue: 0));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Suppliers", "TaxID5");
            DropColumn("dbo.Suppliers", "TaxID4");
            DropColumn("dbo.Suppliers", "TaxID3");
            DropColumn("dbo.Suppliers", "TaxID2");
            DropColumn("dbo.Suppliers", "TaxID1");
        }
    }
}
