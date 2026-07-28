namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka011 : DbMigration
    {
        public override void Up()
        {
            DropTable("dbo.Employees");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Employees",
                c => new
                    {
                        EmployeeID = c.Long(nullable: false, identity: true),
                        EmployeeCode = c.String(nullable: false, maxLength: 15),
                        EmployeeTitle = c.Int(nullable: false),
                        EmployeeName = c.String(nullable: false, maxLength: 100),
                        Designation = c.String(maxLength: 100),
                        Gender = c.Int(nullable: false),
                        DOB = c.DateTime(nullable: false),
                        NIC = c.String(nullable: false, maxLength: 12),
                        Passport = c.String(),
                        Address1 = c.String(nullable: false, maxLength: 100),
                        Address2 = c.String(nullable: false, maxLength: 100),
                        Address3 = c.String(maxLength: 100),
                        Email = c.String(),
                        Telephone = c.String(),
                        Mobile = c.String(nullable: false),
                        Image = c.Binary(),
                        Department = c.String(maxLength: 30),
                        Remark = c.String(maxLength: 100),
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
                .PrimaryKey(t => t.EmployeeID);
            
        }
    }
}
