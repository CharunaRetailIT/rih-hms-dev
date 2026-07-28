namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v2710 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.LOGProducts", "IsNoEffectCostforMenu", c => c.Boolean(nullable: false));
            AddColumn("dbo.Products", "IsNoEffectCostforMenu", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Products", "IsNoEffectCostforMenu");
            DropColumn("dbo.LOGProducts", "IsNoEffectCostforMenu");
        }
    }
}
