namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _39 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.InvPromotionMasters", "MinimumValue");
        }
        
        public override void Down()
        {
            AddColumn("dbo.InvPromotionMasters", "MinimumValue", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
    }
}
