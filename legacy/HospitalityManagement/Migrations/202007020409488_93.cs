namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _93 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.LOGAddons",
                c => new
                    {
                        AddonsId = c.Int(nullable: false, identity: true),
                        SourceId = c.Int(nullable: false),
                        ProductId = c.Long(nullable: false),
                        ProductAddonId = c.Long(nullable: false),
                        DepartmentId = c.Long(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        AddonSellingPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        AddonQuantity = c.Decimal(nullable: false, precision: 18, scale: 2),
                        IsShowOnBill = c.Boolean(nullable: false),
                        Action = c.String(),
                        GroupOfCompanyID = c.Int(nullable: false),
                        CompanyID = c.Int(nullable: false),
                        LocationId = c.Int(nullable: false),
                        CreatedUser = c.String(maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedUser = c.String(maxLength: 50),
                        ModifiedDate = c.DateTime(nullable: false),
                        DataTransfer = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.AddonsId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.LOGAddons");
        }
    }
}
