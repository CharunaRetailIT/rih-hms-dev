namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka102 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.InterDepartments",
                c => new
                    {
                        InterDepartmentId = c.Long(nullable: false, identity: true),
                        InterDepartmentCode = c.String(),
                        InterDepartmentName = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        GroupOfCompanyID = c.Int(nullable: false),
                        CompanyID = c.Int(nullable: false),
                        LocationId = c.Int(nullable: false),
                        CreatedUser = c.String(maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedUser = c.String(maxLength: 50),
                        ModifiedDate = c.DateTime(nullable: false),
                        DataTransfer = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.InterDepartmentId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.InterDepartments");
        }
    }
}
