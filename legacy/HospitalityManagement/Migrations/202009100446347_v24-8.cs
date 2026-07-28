namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v248 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Customers", "Religion", c => c.Int());
            AlterColumn("dbo.Customers", "Race", c => c.Int());
            AlterColumn("dbo.Customers", "SpouseDateOfBirth", c => c.DateTime());
            AlterColumn("dbo.Customers", "CustomerSince", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Customers", "CustomerSince", c => c.DateTime(nullable: false));
            AlterColumn("dbo.Customers", "SpouseDateOfBirth", c => c.DateTime(nullable: false));
            AlterColumn("dbo.Customers", "Race", c => c.Int(nullable: false));
            AlterColumn("dbo.Customers", "Religion", c => c.Int(nullable: false));
        }
    }
}
