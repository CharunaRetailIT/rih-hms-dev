namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka066 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PurchaseOrderHeaders", "PaymentMethodId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PurchaseOrderHeaders", "PaymentMethodId");
        }
    }
}
