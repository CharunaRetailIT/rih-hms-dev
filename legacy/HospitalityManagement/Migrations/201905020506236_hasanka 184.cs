namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka184 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Products", "IsTaxInclude", c => c.Boolean(nullable: false));
            AddColumn("dbo.Products", "IsTaxOnTax", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Products", "IsTaxOnTax");
            DropColumn("dbo.Products", "IsTaxInclude");
        }
    }
}
