namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka023 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Receipes",
                c => new
                    {
                        ReceipeId = c.Long(nullable: false, identity: true),
                        ProductId = c.Long(nullable: false),
                        Quantity = c.Decimal(nullable: false, precision: 18, scale: 2),
                        GroupOfCompanyID = c.Int(nullable: false),
                        CompanyID = c.Int(nullable: false),
                        LocationId = c.Int(nullable: false),
                        CreatedUser = c.String(maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedUser = c.String(maxLength: 50),
                        ModifiedDate = c.DateTime(nullable: false),
                        DataTransfer = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ReceipeId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Receipes");
        }
    }
}
