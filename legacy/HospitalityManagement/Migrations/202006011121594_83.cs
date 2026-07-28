namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _83 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Addons", "AddonSellingPrice", c => c.Decimal(nullable: false, precision: 18, scale: 3));
            AlterColumn("dbo.Addons", "AddonQuantity", c => c.Decimal(nullable: false, precision: 18, scale: 3));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Addons", "AddonQuantity", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.Addons", "AddonSellingPrice", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
    }
}
