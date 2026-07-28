namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _91 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.LOGAddons", "SourceId", c => c.Int(nullable: false));
            AddColumn("dbo.LOGCustomers", "SourceId", c => c.Int(nullable: false));
            AddColumn("dbo.LOGInvPromotionMasters", "SourceId", c => c.Int(nullable: false));
            AddColumn("dbo.LOGProducts", "SourceId", c => c.Int(nullable: false));
            AddColumn("dbo.LOGProductServingUnits", "SourceId", c => c.Int(nullable: false));
            AddColumn("dbo.LOGProductStockMasters", "SourceId", c => c.Int(nullable: false));
            AddColumn("dbo.LOGProductTaxes", "SourceId", c => c.Int(nullable: false));
            AddColumn("dbo.LOGReceipes", "SourceId", c => c.Int(nullable: false));
            AddColumn("dbo.LOGSuppliers", "SourceId", c => c.Int(nullable: false));
            AddColumn("dbo.LOGSupplierProducts", "SourceId", c => c.Int(nullable: false));
            AddColumn("dbo.LOGUnitConversions", "SourceId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.LOGUnitConversions", "SourceId");
            DropColumn("dbo.LOGSupplierProducts", "SourceId");
            DropColumn("dbo.LOGSuppliers", "SourceId");
            DropColumn("dbo.LOGReceipes", "SourceId");
            DropColumn("dbo.LOGProductTaxes", "SourceId");
            DropColumn("dbo.LOGProductStockMasters", "SourceId");
            DropColumn("dbo.LOGProductServingUnits", "SourceId");
            DropColumn("dbo.LOGProducts", "SourceId");
            DropColumn("dbo.LOGInvPromotionMasters", "SourceId");
            DropColumn("dbo.LOGCustomers", "SourceId");
            DropColumn("dbo.LOGAddons", "SourceId");
        }
    }
}
