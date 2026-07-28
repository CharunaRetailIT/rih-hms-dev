namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class promotionmodule04 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.InvPromotionMasters", "ModifiedDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.InvPromotionMasters", "ModifiedDate");
        }
    }
}
