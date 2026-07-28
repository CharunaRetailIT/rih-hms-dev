namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v2713 : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.Products", "ProductId", unique: true, clustered: true);
            CreateIndex("dbo.ProductStockMasters", "ProductId", unique: true, clustered: true);
        }
        
        public override void Down()
        {
            DropIndex("dbo.ProductStockMasters", new[] { "ProductId" });
            DropIndex("dbo.Products", new[] { "ProductId" });
        }
    }
}
