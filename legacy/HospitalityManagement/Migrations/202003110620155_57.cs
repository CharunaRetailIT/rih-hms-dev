namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _57 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PurchaseDetails", "IsPRN", c => c.Boolean(nullable: false));
            AlterColumn("dbo.Suppliers", "SupplierTitle", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Suppliers", "SupplierTitle", c => c.String(nullable: false));
            DropColumn("dbo.PurchaseDetails", "IsPRN");
        }
    }
}
