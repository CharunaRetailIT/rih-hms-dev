namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _50 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ServingUnits",
                c => new
                    {
                        ServingUnitId = c.Long(nullable: false, identity: true),
                        ServingUnitName = c.String(nullable: false),
                        IsActive = c.Boolean(nullable: false),
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
                .PrimaryKey(t => t.ServingUnitId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.ServingUnits");
        }
    }
}
