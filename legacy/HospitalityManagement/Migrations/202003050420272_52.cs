namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _52 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Employees", "EpfNo", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.Suppliers", "BillingAddress1", c => c.String(nullable: false, maxLength: 250));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Suppliers", "BillingAddress1", c => c.String(maxLength: 250));
            DropColumn("dbo.Employees", "EpfNo");
        }
    }
}
