namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _3605 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PaymentDets", "IsGLTransfer", c => c.Int(nullable: false));
            AddColumn("dbo.TransactionDets", "IsGLTransfer", c => c.Int(nullable: false));
            DropColumn("dbo.PaymentDets", "IsGLTransfe");
            DropColumn("dbo.TransactionDets", "IsGLTransfe");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TransactionDets", "IsGLTransfe", c => c.Int(nullable: false));
            AddColumn("dbo.PaymentDets", "IsGLTransfe", c => c.Int(nullable: false));
            DropColumn("dbo.TransactionDets", "IsGLTransfer");
            DropColumn("dbo.PaymentDets", "IsGLTransfer");
        }
    }
}
