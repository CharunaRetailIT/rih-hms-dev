namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka153 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Customers", "CreditLimit", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Customers", "Outstanding", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Customers", "Outstanding");
            DropColumn("dbo.Customers", "CreditLimit");
        }
    }
}
