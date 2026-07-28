namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _203 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.PaymentDets", "ChequeDate", c => c.DateTime());
            AlterColumn("dbo.TransactionDets", "ExpiryDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.TransactionDets", "ExpiryDate", c => c.DateTime(nullable: false));
            AlterColumn("dbo.PaymentDets", "ChequeDate", c => c.DateTime(nullable: false));
        }
    }
}
