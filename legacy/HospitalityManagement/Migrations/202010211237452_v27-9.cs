namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v279 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MonthEnds", "CompanyId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.MonthEnds", "CompanyId");
        }
    }
}
