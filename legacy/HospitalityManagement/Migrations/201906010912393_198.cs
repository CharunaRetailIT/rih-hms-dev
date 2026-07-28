namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _198 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PriceLevels", "ModifiedDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.PriceLevels", "DataTransfer", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PriceLevels", "DataTransfer");
            DropColumn("dbo.PriceLevels", "ModifiedDate");
        }
    }
}
