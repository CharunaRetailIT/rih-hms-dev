namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class promotionmodule07 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.InvPromoLowestPriceWaveOffs", "ModifiedUser", c => c.String(maxLength: 50));
            AddColumn("dbo.InvPromoLowestPriceWaveOffs", "ModifiedDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.InvPromoLowestPriceWaveOffs", "ModifiedDate");
            DropColumn("dbo.InvPromoLowestPriceWaveOffs", "ModifiedUser");
        }
    }
}
