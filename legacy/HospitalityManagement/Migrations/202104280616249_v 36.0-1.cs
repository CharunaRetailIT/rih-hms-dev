namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v3601 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.InvPromotionMasters", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.LOGInvPromotionMasters", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.LOGReceipes", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.Receipes", "IsActive", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Receipes", "IsActive");
            DropColumn("dbo.LOGReceipes", "IsActive");
            DropColumn("dbo.LOGInvPromotionMasters", "IsActive");
            DropColumn("dbo.InvPromotionMasters", "IsActive");
        }
    }
}
