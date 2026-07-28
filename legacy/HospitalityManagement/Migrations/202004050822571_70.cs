namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _70 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.InvPromotionMasters", "CustomerGroupId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.InvPromotionMasters", "CustomerGroupId");
        }
    }
}
