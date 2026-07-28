namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka056 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Taxes", "IsTaxOnTax", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Taxes", "IsTaxOnTax");
        }
    }
}
