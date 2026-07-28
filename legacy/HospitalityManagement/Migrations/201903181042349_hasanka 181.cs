namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka181 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AddonCategoryMasters",
                c => new
                    {
                        AddonCategoryMasterId = c.Long(nullable: false, identity: true),
                        AddonCatCode = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        AddonCatName = c.String(),
                        MaxAddons = c.Int(nullable: false),
                        IsDelete = c.Boolean(nullable: false),
                        GroupOfCompanyID = c.Int(nullable: false),
                        CompanyID = c.Int(nullable: false),
                        LocationId = c.Int(nullable: false),
                        CreatedUser = c.String(maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedUser = c.String(maxLength: 50),
                        ModifiedDate = c.DateTime(nullable: false),
                        DataTransfer = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.AddonCategoryMasterId);
            
            AddColumn("dbo.Products", "AddonCategoryMasterId", c => c.Long());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Products", "AddonCategoryMasterId");
            DropTable("dbo.AddonCategoryMasters");
        }
    }
}
