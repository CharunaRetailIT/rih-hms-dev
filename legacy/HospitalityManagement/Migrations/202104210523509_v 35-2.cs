namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v352 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.InvPosTerminalDetails",
                c => new
                    {
                        InvPosTerminalDetailsID = c.Long(nullable: false, identity: true),
                        LocationID = c.Int(nullable: false),
                        TerminalId = c.Int(nullable: false),
                        IP = c.String(),
                        DBNAME = c.String(),
                        UserId = c.String(),
                        PWD = c.String(),
                        JrnlPath = c.String(),
                        CompanyID = c.Int(nullable: false),
                        CreatedUser = c.String(maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedUser = c.String(maxLength: 50),
                        ModifiedDate = c.DateTime(nullable: false),
                        DataTransfer = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.InvPosTerminalDetailsID);
            
            AddColumn("dbo.SuspendDets", "OrigUnitNo", c => c.Int(nullable: false));
            AddColumn("dbo.TransactionDets", "OrigUnitNo", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TransactionDets", "OrigUnitNo");
            DropColumn("dbo.SuspendDets", "OrigUnitNo");
            DropTable("dbo.InvPosTerminalDetails");
        }
    }
}
