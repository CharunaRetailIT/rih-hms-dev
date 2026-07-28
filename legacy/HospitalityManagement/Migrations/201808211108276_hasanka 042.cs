namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka042 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.PurchaseOrderDetails", "PackId");
            DropColumn("dbo.PurchaseOrderHeaders", "CostCentreId");
            DropColumn("dbo.PurchaseOrderHeaders", "IsConsignmentBasis");
            DropColumn("dbo.PurchaseOrderHeaders", "VAT");
            DropColumn("dbo.PurchaseOrderHeaders", "NBT");
            DropColumn("dbo.PurchaseOrderHeaders", "ReferenceDocumentId");
            DropColumn("dbo.PurchaseOrderHeaders", "IsAuthorized");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PurchaseOrderHeaders", "IsAuthorized", c => c.Boolean(nullable: false));
            AddColumn("dbo.PurchaseOrderHeaders", "ReferenceDocumentId", c => c.Int(nullable: false));
            AddColumn("dbo.PurchaseOrderHeaders", "NBT", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.PurchaseOrderHeaders", "VAT", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.PurchaseOrderHeaders", "IsConsignmentBasis", c => c.Boolean(nullable: false));
            AddColumn("dbo.PurchaseOrderHeaders", "CostCentreId", c => c.Int(nullable: false));
            AddColumn("dbo.PurchaseOrderDetails", "PackId", c => c.Int(nullable: false));
        }
    }
}
