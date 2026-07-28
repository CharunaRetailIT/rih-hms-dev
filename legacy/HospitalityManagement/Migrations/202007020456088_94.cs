namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _94 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.InvPromotionMasters", "CreateDate", c => c.DateTime());
            AddColumn("dbo.InvPromotionMasters", "CreateUser", c => c.String());
            AddColumn("dbo.InvPromotionMasters", "ModifiedUser", c => c.String());
            AddColumn("dbo.LOGInvPromotionMasters", "CreateDate", c => c.DateTime());
            AddColumn("dbo.LOGInvPromotionMasters", "CreateUser", c => c.String());
            AddColumn("dbo.LOGInvPromotionMasters", "ModifiedUser", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.LOGInvPromotionMasters", "ModifiedUser");
            DropColumn("dbo.LOGInvPromotionMasters", "CreateUser");
            DropColumn("dbo.LOGInvPromotionMasters", "CreateDate");
            DropColumn("dbo.InvPromotionMasters", "ModifiedUser");
            DropColumn("dbo.InvPromotionMasters", "CreateUser");
            DropColumn("dbo.InvPromotionMasters", "CreateDate");
        }
    }
}
