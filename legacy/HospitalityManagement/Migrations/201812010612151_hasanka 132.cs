namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka132 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PaidOutTypes",
                c => new
                    {
                        PaidOutTypeId = c.Int(nullable: false, identity: true),
                        Code = c.String(),
                        Description = c.String(),
                        IsSalesSummery = c.Boolean(nullable: false),
                        IsDelete = c.Boolean(nullable: false),
                        DayFrom = c.Int(nullable: false),
                        DayTo = c.Int(nullable: false),
                        GroupOfCompanyId = c.Int(nullable: false),
                        CreatedUser = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedUser = c.String(),
                        ModifiedDate = c.DateTime(nullable: false),
                        DataTransfer = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.PaidOutTypeId);
            
            CreateTable(
                "dbo.PaidInTypes",
                c => new
                    {
                        PaidInTypeId = c.Int(nullable: false, identity: true),
                        Code = c.String(),
                        Description = c.String(),
                        IsSalesSummery = c.Boolean(nullable: false),
                        IsDelete = c.Boolean(nullable: false),
                        GroupOfCompanyId = c.Int(nullable: false),
                        CreatedUser = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedUser = c.String(),
                        ModifiedDate = c.DateTime(nullable: false),
                        DataTransfer = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.PaidInTypeId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.PaidInTypes");
            DropTable("dbo.PaidOutTypes");
        }
    }
}
