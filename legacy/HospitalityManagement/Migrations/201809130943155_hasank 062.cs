namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasank062 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SupplierProducts",
                c => new
                    {
                        SupplierProductId = c.Long(nullable: false, identity: true),
                        SupplierId = c.Int(nullable: false),
                        ProductId = c.Int(nullable: false),
                        GroupOfCompanyID = c.Int(nullable: false),
                        CompanyID = c.Int(nullable: false),
                        LocationId = c.Int(nullable: false),
                        CreatedUser = c.String(maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedUser = c.String(maxLength: 50),
                        ModifiedDate = c.DateTime(nullable: false),
                        DataTransfer = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.SupplierProductId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.SupplierProducts");
        }
    }
}
