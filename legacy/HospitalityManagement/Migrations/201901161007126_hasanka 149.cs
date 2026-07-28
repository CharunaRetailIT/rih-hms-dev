namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka149 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Receipes", "CostPrice", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Receipes", "SellingPrice", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Receipes", "SellingPrice");
            DropColumn("dbo.Receipes", "CostPrice");
        }
    }
}
