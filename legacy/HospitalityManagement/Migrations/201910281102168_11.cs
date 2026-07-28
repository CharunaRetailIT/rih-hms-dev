namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _11 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Customers", "CustomerStatus", c => c.String(maxLength: 20, unicode: false));
            AddColumn("dbo.Taxes", "isExcludeTax", c => c.Boolean(nullable: false));
            DropColumn("dbo.Customers", "Status");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Customers", "Status", c => c.Int(nullable: false));
            DropColumn("dbo.Taxes", "isExcludeTax");
            DropColumn("dbo.Customers", "CustomerStatus");
        }
    }
}
