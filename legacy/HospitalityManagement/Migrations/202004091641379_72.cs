namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _72 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.InvPromotionMasters", "PromotionCount", c => c.Int(nullable: false));
            AlterColumn("dbo.InvPromotionMasters", "CustomerGroupId", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.InvPromotionMasters", "CustomerGroupId", c => c.Int(nullable: false));
            DropColumn("dbo.InvPromotionMasters", "PromotionCount");
        }
    }
}
