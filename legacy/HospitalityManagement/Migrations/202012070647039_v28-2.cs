namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v282 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TransactionDets", "ServingUnitId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TransactionDets", "ServingUnitId");
        }
    }
}
