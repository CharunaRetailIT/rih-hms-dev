namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka013 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DeliveryPersons",
                c => new
                    {
                        DeliveryPersonId = c.Long(nullable: false, identity: true),
                        EmployeeId = c.String(nullable: false, maxLength: 15),
                        Title = c.String(nullable: false),
                        FullName = c.String(nullable: false, maxLength: 100),
                        Address = c.String(nullable: false, maxLength: 200),
                        DOB = c.DateTime(nullable: false),
                        Gender = c.String(nullable: false),
                        Designation = c.String(maxLength: 100),
                        Picture = c.Binary(),
                        PictureName = c.String(),
                        PictureType = c.String(),
                        NIC = c.String(nullable: false, maxLength: 12),
                        DrivingLicence = c.String(),
                        Telephone = c.String(),
                        Mobile = c.String(nullable: false),
                        Email = c.String(),
                        InCaseOfEmergency = c.String(nullable: false, maxLength: 200),
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
                .PrimaryKey(t => t.DeliveryPersonId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.DeliveryPersons");
        }
    }
}
