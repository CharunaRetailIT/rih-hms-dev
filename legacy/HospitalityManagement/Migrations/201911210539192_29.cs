namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _29 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TransactionDets", "OrderStatus", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.TransactionDets", "OrderStatus");
        }
    }
}
