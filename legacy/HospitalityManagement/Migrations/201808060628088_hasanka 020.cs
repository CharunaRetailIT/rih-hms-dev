namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka020 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UnitConversions",
                c => new
                    {
                        UnitConversionId = c.Long(nullable: false, identity: true),
                        UnitOfMeasureId = c.Long(nullable: false),
                        SubUnit = c.String(nullable: false),
                        SubUnitValue = c.Decimal(nullable: false, precision: 18, scale: 2),
                        GroupOfCompanyID = c.Int(nullable: false),
                        CompanyID = c.Int(nullable: false),
                        LocationId = c.Int(nullable: false),
                        CreatedUser = c.String(maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedUser = c.String(maxLength: 50),
                        ModifiedDate = c.DateTime(nullable: false),
                        DataTransfer = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.UnitConversionId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.UnitConversions");
        }
    }
}
