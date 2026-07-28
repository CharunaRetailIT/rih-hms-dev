namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _96 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.InvAdvanceNoteHeds",
                c => new
                    {
                        InvAdvanceNoteHedID = c.Long(nullable: false, identity: true),
                        AdNoteNo = c.String(maxLength: 15, nullable:true),
                        Receipt = c.String(maxLength: 15, nullable: true),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Balance = c.Decimal(nullable: false, precision: 18, scale: 2),
                        LocationID = c.Int(nullable: false),
                        Date = c.DateTime(nullable: false),
                        UnitNo = c.Int(nullable: false),
                        CashierID = c.Int(nullable: false),
                        Time = c.DateTime(nullable: false),
                        Zno = c.Long(nullable: false),
                        RecallFromInvoice = c.Int(nullable: false),
                        DeliveryDate = c.DateTime(nullable: false),
                        Remark = c.String(),
                    })
                .PrimaryKey(t => t.InvAdvanceNoteHedID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.InvAdvanceNoteHeds");
        }
    }
}
