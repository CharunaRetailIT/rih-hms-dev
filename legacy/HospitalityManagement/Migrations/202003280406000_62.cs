namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _62 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MonthEnds",
                c => new
                    {
                        MonthEndId = c.Long(nullable: false, identity: true),
                        LocationId = c.Int(nullable: false),
                        LocYear = c.Int(nullable: false),
                        LocMonth = c.Int(nullable: false),
                        LocMonthDesc = c.String(maxLength: 50),
                        LocStatus = c.Boolean(nullable: false),
                        LocIsClose = c.Boolean(nullable: false),
                        CreatedUser = c.String(maxLength: 50),
                        CreatedDate = c.DateTime(),
                        ModifiedUser = c.String(maxLength: 50),
                        ModifiedDate = c.DateTime(),
                        DataTransfer = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.MonthEndId);
            
            CreateTable(
                "dbo.SysYears",
                c => new
                    {
                        SysYearsId = c.Int(nullable: false, identity: true),
                        SysYear = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.SysYearsId);
            
            CreateTable(
                "dbo.tmpMonthEnds",
                c => new
                    {
                        tmpMonthEndId = c.Long(nullable: false, identity: true),
                        SysLocationID = c.Int(nullable: false),
                        DocumentType = c.String(),
                        Message = c.String(),
                        DocumentCount = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.tmpMonthEndId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.tmpMonthEnds");
            DropTable("dbo.SysYears");
            DropTable("dbo.MonthEnds");
        }
    }
}
