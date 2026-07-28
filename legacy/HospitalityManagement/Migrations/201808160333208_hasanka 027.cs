namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka027 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ProductTaxes",
                c => new
                    {
                        ProductTaxId = c.Long(nullable: false, identity: true),
                        ProductId = c.Long(nullable: false),
                        TaxId = c.Long(nullable: false),
                        GroupOfCompanyID = c.Int(nullable: false),
                        CompanyID = c.Int(nullable: false),
                        LocationId = c.Int(nullable: false),
                        CreatedUser = c.String(maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedUser = c.String(maxLength: 50),
                        ModifiedDate = c.DateTime(nullable: false),
                        DataTransfer = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ProductTaxId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.ProductTaxes");
        }
    }
}
