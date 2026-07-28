namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka133 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Products", "PrinterTypeId", c => c.Int(nullable: false));
            DropColumn("dbo.Products", "OrderType");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Products", "OrderType", c => c.String(maxLength: 200));
            DropColumn("dbo.Products", "PrinterTypeId");
        }
    }
}
