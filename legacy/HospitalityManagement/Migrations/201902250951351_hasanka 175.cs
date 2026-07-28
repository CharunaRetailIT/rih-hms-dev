namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka175 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Addons", "AddonSellingPrice", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Addons", "AddonQuantity", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Addons", "AddonQuantity");
            DropColumn("dbo.Addons", "AddonSellingPrice");
        }
    }
}
