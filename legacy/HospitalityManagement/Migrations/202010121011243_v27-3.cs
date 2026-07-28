namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v273 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.KOTBOTDescriptions", "CompanyId", c => c.Int(nullable: false));
            AddColumn("dbo.POSUserGroups", "CompanyId", c => c.Int(nullable: false));
            AddColumn("dbo.ProductInstructions", "CompanyId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ProductInstructions", "CompanyId");
            DropColumn("dbo.POSUserGroups", "CompanyId");
            DropColumn("dbo.KOTBOTDescriptions", "CompanyId");
        }
    }
}
