namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasank037 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.POProductTaxes",
                c => new
                    {
                        POProductTaxId = c.Long(nullable: false, identity: true),
                        POId = c.Long(nullable: false),
                        PRoductId = c.Long(nullable: false),
                        TaxId = c.Long(nullable: false),
                        TaxPrecentage = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.POProductTaxId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.POProductTaxes");
        }
    }
}
