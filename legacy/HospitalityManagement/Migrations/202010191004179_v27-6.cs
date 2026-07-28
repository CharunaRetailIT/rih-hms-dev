namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v276 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PointsExpirations",
                c => new
                    {
                        PointsExpirationId = c.Int(nullable: false, identity: true),
                        Year = c.Int(nullable: false),
                        CardType = c.Int(nullable: false),
                        FirstReminderMessage = c.String(),
                        FirstReminderDate = c.DateTime(nullable: false),
                        SecontReminderMessage = c.String(),
                        SecondReminderDate = c.DateTime(nullable: false),
                        PointsExpiryDate = c.DateTime(nullable: false),
                        GroupOfCompanyID = c.Int(nullable: false),
                        CompanyID = c.Int(nullable: false),
                        LocationId = c.Int(nullable: false),
                        CreatedUser = c.String(maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedUser = c.String(maxLength: 50),
                        ModifiedDate = c.DateTime(nullable: false),
                        DataTransfer = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.PointsExpirationId);
            
            CreateTable(
                "dbo.PointsExpirationSchedules",
                c => new
                    {
                        Idx = c.Int(nullable: false, identity: true),
                        Type = c.Int(nullable: false),
                        SQL = c.String(),
                        user = c.String(maxLength: 15),
                        Date = c.DateTime(nullable: false),
                        ScheduleDate = c.DateTime(nullable: false),
                        Status = c.Int(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        GroupOfCompanyID = c.Int(nullable: false),
                        CompanyID = c.Int(nullable: false),
                        LocationId = c.Int(nullable: false),
                        CreatedUser = c.String(maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedUser = c.String(maxLength: 50),
                        ModifiedDate = c.DateTime(nullable: false),
                        DataTransfer = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Idx);
            
            CreateTable(
                "dbo.PointsExpirationTypes",
                c => new
                    {
                        PointsExpirationTypeId = c.Int(nullable: false, identity: true),
                        Desc = c.String(),
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
                .PrimaryKey(t => t.PointsExpirationTypeId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.PointsExpirationTypes");
            DropTable("dbo.PointsExpirationSchedules");
            DropTable("dbo.PointsExpirations");
        }
    }
}
