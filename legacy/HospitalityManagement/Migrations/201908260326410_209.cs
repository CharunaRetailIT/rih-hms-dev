namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _209 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PurchaseOrderDetails", "GRNQuantity", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PurchaseOrderDetails", "GRNQuantity");
        }
    }
}
