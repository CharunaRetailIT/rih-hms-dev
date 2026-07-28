namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v334 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.InvBundleItemPrices", "BundleName", c => c.String(maxLength: 50, unicode: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.InvBundleItemPrices", "BundleName");
        }
    }
}
