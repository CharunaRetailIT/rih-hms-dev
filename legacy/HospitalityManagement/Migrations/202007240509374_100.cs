namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _100 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PaymentDets", "AdvancePayTypeID", c => c.Int(nullable: false));
            AddColumn("dbo.PaymentDets", "AdvancePayRefNo", c => c.String(maxLength: 30, unicode: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PaymentDets", "AdvancePayRefNo");
            DropColumn("dbo.PaymentDets", "AdvancePayTypeID");
        }
    }
}
