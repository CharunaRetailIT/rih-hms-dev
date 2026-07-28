namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _186 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProductTaxes", "TaxPracentage", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.ProductTaxes", "TaxSequence", c => c.Int(nullable: false));
            DropColumn("dbo.Products", "IsTaxOnTax");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Products", "IsTaxOnTax", c => c.Boolean(nullable: false));
            DropColumn("dbo.ProductTaxes", "TaxSequence");
            DropColumn("dbo.ProductTaxes", "TaxPracentage");
        }
    }
}
