namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _19 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.TransactionDets", "TableNumber", c => c.Int());
            AlterColumn("dbo.TransactionDets", "NoOfCustomers", c => c.Int());
            AlterColumn("dbo.TransactionDets", "NoOfAdults", c => c.Int());
            AlterColumn("dbo.TransactionDets", "NoOfChild", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.TransactionDets", "NoOfChild", c => c.Int(nullable: false));
            AlterColumn("dbo.TransactionDets", "NoOfAdults", c => c.Int(nullable: false));
            AlterColumn("dbo.TransactionDets", "NoOfCustomers", c => c.Int(nullable: false));
            AlterColumn("dbo.TransactionDets", "TableNumber", c => c.Int(nullable: false));
        }
    }
}
