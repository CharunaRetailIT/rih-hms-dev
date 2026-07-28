namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _78 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BankBins", "LocationId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.BankBins", "LocationId");
        }
    }
}
