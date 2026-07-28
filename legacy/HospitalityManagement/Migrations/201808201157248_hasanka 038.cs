namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka038 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.POProductTaxes", "PurchaseOrderHeaderId", c => c.Long(nullable: false));
            DropColumn("dbo.POProductTaxes", "POId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.POProductTaxes", "POId", c => c.Long(nullable: false));
            DropColumn("dbo.POProductTaxes", "PurchaseOrderHeaderId");
        }
    }
}
