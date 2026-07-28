namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _3604 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PaymentDets", "IsGLTransfe", c => c.Int(nullable: false));
            AddColumn("dbo.TransactionDets", "IsGLTransfe", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TransactionDets", "IsGLTransfe");
            DropColumn("dbo.PaymentDets", "IsGLTransfe");
        }
    }
}
