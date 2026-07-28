namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _97 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.InvAdvancePaymentDets",
                c => new
                    {
                        InvAdvancePaymentDetId = c.Long(nullable: false, identity: true),
                        Idx = c.Long(nullable: false),
                        RowNo = c.Long(nullable: false),
                        PayTypeID = c.Int(nullable: false),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 4),
                        Balance = c.Decimal(nullable: false, precision: 18, scale: 4),
                        SDate = c.DateTime(nullable: false),
                        Receipt = c.String(maxLength: 10, fixedLength: true, unicode: false),
                        LocationID = c.Int(nullable: false),
                        CashierID = c.Long(nullable: false),
                        UnitNo = c.Int(nullable: false),
                        BillTypeID = c.Int(nullable: false),
                        RefNo = c.String(maxLength: 30, unicode: false),
                        BankId = c.Long(nullable: false),
                        ChequeDate = c.DateTime(storeType: "date"),
                        IsRecallAdv = c.Boolean(nullable: false),
                        RecallNo = c.String(maxLength: 10, unicode: false),
                        Descrip = c.String(maxLength: 20, unicode: false),
                        EnCodeName = c.String(maxLength: 50, unicode: false),
                        SuspendNo = c.String(maxLength: 50, fixedLength: true),
                        SuspendBy = c.Boolean(nullable: false),
                        IsDeleteOnRecall = c.Boolean(nullable: false),
                        AdvanceNumber = c.String(maxLength: 20, unicode: false),
                    })
                .PrimaryKey(t => t.InvAdvancePaymentDetId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.InvAdvancePaymentDets");
        }
    }
}
