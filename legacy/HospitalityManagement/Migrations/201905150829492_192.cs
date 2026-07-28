namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _192 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TempItemTaxes",
                c => new
                    {
                        Idx = c.Long(nullable: false, identity: true),
                        LocationId = c.Int(nullable: false),
                        UnitNo = c.String(maxLength: 20, unicode: false),
                        Receipt = c.String(maxLength: 10, fixedLength: true, unicode: false),
                        TDate = c.DateTime(nullable: false, storeType: "date"),
                        RowNo = c.Int(nullable: false),
                        ProductId = c.Long(nullable: false),
                        Nett = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TaxId = c.Long(nullable: false),
                        TaxCode = c.String(maxLength: 50, fixedLength: true, unicode: false),
                        TaxName = c.String(maxLength: 50, fixedLength: true, unicode: false),
                        TaxRate = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CalcAmt = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TaxAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ZNo = c.Long(nullable: false),
                        Online = c.Short(nullable: false),
                    })
                .PrimaryKey(t => t.Idx);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.TempItemTaxes");
        }
    }
}
