namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka074 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Products", "OrderType", c => c.String(maxLength: 200));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Products", "OrderType");
        }
    }
}
