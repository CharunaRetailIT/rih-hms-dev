namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v333 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Receipes", "Quantity", c => c.Decimal(nullable: false, precision: 18, scale: 4));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Receipes", "Quantity", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
    }
}
