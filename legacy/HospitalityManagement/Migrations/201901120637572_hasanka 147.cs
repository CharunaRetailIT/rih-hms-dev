namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka147 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Products", "ProductCode", c => c.String(nullable: false, maxLength: 10));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Products", "ProductCode", c => c.String(nullable: false, maxLength: 5));
        }
    }
}
