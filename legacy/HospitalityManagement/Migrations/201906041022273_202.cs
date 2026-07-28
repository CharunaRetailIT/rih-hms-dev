namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _202 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TempItemTaxes", "DataTransfer", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TempItemTaxes", "DataTransfer");
        }
    }
}
