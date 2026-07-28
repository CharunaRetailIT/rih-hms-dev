namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _41 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.InvPromotionMasters", "MinimumValue", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.InvPromotionMasters", "MinimumValue");
        }
    }
}
