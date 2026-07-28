namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka173 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Customers", "BillingAddress1");
            DropColumn("dbo.Customers", "BillingAddress2");
            DropColumn("dbo.Customers", "BillingAddress3");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Customers", "BillingAddress3", c => c.String());
            AddColumn("dbo.Customers", "BillingAddress2", c => c.String(nullable: false, maxLength: 100));
            AddColumn("dbo.Customers", "BillingAddress1", c => c.String(nullable: false, maxLength: 100));
        }
    }
}
