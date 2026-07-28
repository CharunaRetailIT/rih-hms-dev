namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _17 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TransactionDets", "ServingUnit", c => c.String(maxLength: 50, unicode: false));
            AddColumn("dbo.TransactionDets", "TableNumber", c => c.Int(nullable: false));
            AddColumn("dbo.TransactionDets", "NoOfCustomers", c => c.Int(nullable: false));
            AddColumn("dbo.TransactionDets", "NoOfAdults", c => c.Int(nullable: false));
            AddColumn("dbo.TransactionDets", "NoOfChild", c => c.Int(nullable: false));
            AddColumn("dbo.TransactionDets", "IsAddonItem", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TransactionDets", "IsAddonItem");
            DropColumn("dbo.TransactionDets", "NoOfChild");
            DropColumn("dbo.TransactionDets", "NoOfAdults");
            DropColumn("dbo.TransactionDets", "NoOfCustomers");
            DropColumn("dbo.TransactionDets", "TableNumber");
            DropColumn("dbo.TransactionDets", "ServingUnit");
        }
    }
}
