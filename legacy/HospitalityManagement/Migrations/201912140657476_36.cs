namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _36 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProductStockMasters", "PrinterType_Id", c => c.Int(nullable: false));
            AddColumn("dbo.SupplierProducts", "IsPreferredSupplier", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SupplierProducts", "IsPreferredSupplier");
            DropColumn("dbo.ProductStockMasters", "PrinterType_Id");
        }
    }
}
