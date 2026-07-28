namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class promotionmodule05 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.InvPromotionDetailsBuyXProducts", "GroupId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.InvPromotionDetailsBuyXProducts", "GroupId");
        }
    }
}
