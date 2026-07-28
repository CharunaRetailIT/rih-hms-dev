namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _004 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CustomerCategories", "DiscountPrc", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Customers", "RefNo01", c => c.String());
            AddColumn("dbo.Customers", "RefNo02", c => c.String());
            AddColumn("dbo.Customers", "RefNo03", c => c.String());
            AddColumn("dbo.Customers", "Remarks", c => c.String());
            AddColumn("dbo.Customers", "Status", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Customers", "Status");
            DropColumn("dbo.Customers", "Remarks");
            DropColumn("dbo.Customers", "RefNo03");
            DropColumn("dbo.Customers", "RefNo02");
            DropColumn("dbo.Customers", "RefNo01");
            DropColumn("dbo.CustomerCategories", "DiscountPrc");
        }
    }
}
