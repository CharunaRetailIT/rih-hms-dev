namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _332 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.JobHeaders",
                c => new
                    {
                        JobHeaderId = c.Int(nullable: false, identity: true),
                        JobNumber = c.String(),
                        JobDate = c.DateTime(nullable: false),
                        DepartmentId = c.Int(nullable: false),
                        StartTime = c.DateTime(nullable: false),
                        EndTime = c.DateTime(nullable: false),
                        Status = c.Int(nullable: false),
                        GroupOfCompanyID = c.Int(nullable: false),
                        CompanyID = c.Int(nullable: false),
                        LocationId = c.Int(nullable: false),
                        CreatedUser = c.String(maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedUser = c.String(maxLength: 50),
                        ModifiedDate = c.DateTime(nullable: false),
                        DataTransfer = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.JobHeaderId);
            
            CreateTable(
                "dbo.JobItems",
                c => new
                    {
                        JobItemId = c.Int(nullable: false, identity: true),
                        JobHeaderId = c.Int(nullable: false),
                        ProductId = c.Int(nullable: false),
                        SystemQty = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PhysicalQty = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Status = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.JobItemId)
                .ForeignKey("dbo.JobHeaders", t => t.JobHeaderId, cascadeDelete: true)
                .Index(t => t.JobHeaderId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.JobItems", "JobHeaderId", "dbo.JobHeaders");
            DropIndex("dbo.JobItems", new[] { "JobHeaderId" });
            DropTable("dbo.JobItems");
            DropTable("dbo.JobHeaders");
        }
    }
}
