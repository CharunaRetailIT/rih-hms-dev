namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _34 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Customers", "Mobile", c => c.String(nullable: false));
            AlterColumn("dbo.Suppliers", "SupplierGroupID", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Suppliers", "SupplierGroupID", c => c.String(maxLength: 30));
            AlterColumn("dbo.Customers", "Mobile", c => c.String());
        }
    }
}
