namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _005 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Customers", "CustomerCategoryId", c => c.Int(nullable: false));
            DropColumn("dbo.Customers", "CustomerType");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Customers", "CustomerType", c => c.String());
            DropColumn("dbo.Customers", "CustomerCategoryId");
        }
    }
}
