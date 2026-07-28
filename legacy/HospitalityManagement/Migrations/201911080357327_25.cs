namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _25 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PurchaseDetails", "DiscountType", c => c.String(maxLength: 3, unicode: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PurchaseDetails", "DiscountType");
        }
    }
}
