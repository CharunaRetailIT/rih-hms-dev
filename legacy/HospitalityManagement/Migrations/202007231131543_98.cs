namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _98 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.InvAdvancePaymentDets", "Receipt", c => c.String(nullable: false, maxLength: 10, fixedLength: true, unicode: false));
            AlterColumn("dbo.InvAdvancePaymentDets", "RefNo", c => c.String(nullable: false, maxLength: 30, unicode: false));
            AlterColumn("dbo.InvAdvancePaymentDets", "RecallNo", c => c.String(nullable: false, maxLength: 10, unicode: false));
            AlterColumn("dbo.InvAdvancePaymentDets", "Descrip", c => c.String(nullable: false, maxLength: 20, unicode: false));
            AlterColumn("dbo.InvAdvancePaymentDets", "EnCodeName", c => c.String(nullable: false, maxLength: 50, unicode: false));
            AlterColumn("dbo.InvAdvancePaymentDets", "SuspendNo", c => c.String(nullable: false, maxLength: 50, fixedLength: true));
            AlterColumn("dbo.InvAdvancePaymentDets", "AdvanceNumber", c => c.String(nullable: false, maxLength: 20, unicode: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.InvAdvancePaymentDets", "AdvanceNumber", c => c.String(maxLength: 20, unicode: false));
            AlterColumn("dbo.InvAdvancePaymentDets", "SuspendNo", c => c.String(maxLength: 50, fixedLength: true));
            AlterColumn("dbo.InvAdvancePaymentDets", "EnCodeName", c => c.String(maxLength: 50, unicode: false));
            AlterColumn("dbo.InvAdvancePaymentDets", "Descrip", c => c.String(maxLength: 20, unicode: false));
            AlterColumn("dbo.InvAdvancePaymentDets", "RecallNo", c => c.String(maxLength: 10, unicode: false));
            AlterColumn("dbo.InvAdvancePaymentDets", "RefNo", c => c.String(maxLength: 30, unicode: false));
            AlterColumn("dbo.InvAdvancePaymentDets", "Receipt", c => c.String(maxLength: 10, fixedLength: true, unicode: false));
        }
    }
}
