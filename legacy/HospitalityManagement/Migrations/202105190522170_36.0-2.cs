namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _3602 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ImportJournalDetails",
                c => new
                    {
                        REFINDEX = c.Decimal(nullable: false, precision: 18, scale: 0),
                        EXBATCH = c.String(maxLength: 15, unicode: false),
                        TRANTYPE = c.String(maxLength: 2, fixedLength: true),
                        DOCNO = c.String(maxLength: 15, unicode: false),
                        DOCNO1 = c.String(maxLength: 15, unicode: false),
                        DATE = c.DateTime(nullable: false),
                        numeric = c.DateTime(nullable: false),
                        SEQNO = c.Decimal(nullable: false, precision: 5, scale: 0, storeType: "numeric"),
                        ACODE = c.String(maxLength: 15, unicode: false),
                        CCODE = c.String(maxLength: 3, unicode: false),
                        DRCR = c.String(maxLength: 1, unicode: false),
                        DESCRIPTION = c.String(maxLength: 250, unicode: false),
                        AMOUNT = c.Decimal(nullable: false, precision: 18, scale: 2, storeType: "numeric"),
                        CQNO = c.String(maxLength: 8000, unicode: false),
                        CQDATE = c.DateTime(),
                        BANK = c.String(maxLength: 4, unicode: false),
                        BANKBRANCH = c.String(maxLength: 4, unicode: false),
                        PROCESS = c.Boolean(nullable: false),
                        GLPOST = c.Boolean(nullable: false),
                        GLPOSTUSER = c.String(maxLength: 10),
                        GLPOSTDATETIME = c.DateTime(nullable: false),
                        GLPOSTCPNAME = c.String(maxLength: 50),
                        CUSTOMER = c.Boolean(nullable: false),
                        CUSTOMERCODE = c.String(maxLength: 250),
                        SUPPLIER = c.Boolean(nullable: false),
                        ISTAX = c.Boolean(nullable: false),
                        ADDITION = c.Boolean(nullable: false),
                        DEDUCTION = c.Boolean(nullable: false),
                        ISPAIDIN = c.Boolean(nullable: false),
                        ISPAIDOUT = c.Boolean(nullable: false),
                        ISCREDITED = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.REFINDEX);
            
            CreateTable(
                "dbo.ImportJournalDetailsLogs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DocumentNumber = c.String(maxLength: 50, unicode: false),
                        FromDate = c.DateTime(nullable: false),
                        ToDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.ImportJournalDetailsLogs");
            DropTable("dbo.ImportJournalDetails");
        }
    }
}
