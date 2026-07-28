namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _33 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.InvPromotionDetailsProductDis", "ServingUnitId", c => c.Int(nullable: false));
            DropColumn("dbo.InvPromotionDetailsProductDis", "ServingUunitId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.InvPromotionDetailsProductDis", "ServingUunitId", c => c.Int(nullable: false));
            DropColumn("dbo.InvPromotionDetailsProductDis", "ServingUnitId");
        }
    }
}
