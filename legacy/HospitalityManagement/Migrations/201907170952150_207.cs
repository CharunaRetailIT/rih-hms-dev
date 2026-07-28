namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _207 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PurchaseHeaders", "GRNDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PurchaseHeaders", "GRNDate");
        }
    }
}
