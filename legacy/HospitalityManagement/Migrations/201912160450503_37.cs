namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _37 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SupplierProducts", "LastCostPrice", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SupplierProducts", "LastCostPrice");
        }
    }
}
