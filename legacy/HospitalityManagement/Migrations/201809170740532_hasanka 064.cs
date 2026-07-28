namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka064 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.ProductStockMasters", "PLUName");
            DropColumn("dbo.ProductStockMasters", "SupplierID");
            DropColumn("dbo.ProductStockMasters", "SupplierCode");
            DropColumn("dbo.ProductStockMasters", "WholeSalePrice");
            DropColumn("dbo.ProductStockMasters", "IsWarranty");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ProductStockMasters", "IsWarranty", c => c.Boolean(nullable: false));
            AddColumn("dbo.ProductStockMasters", "WholeSalePrice", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.ProductStockMasters", "SupplierCode", c => c.String(maxLength: 20));
            AddColumn("dbo.ProductStockMasters", "SupplierID", c => c.Int(nullable: false));
            AddColumn("dbo.ProductStockMasters", "PLUName", c => c.String(maxLength: 30));
        }
    }
}
