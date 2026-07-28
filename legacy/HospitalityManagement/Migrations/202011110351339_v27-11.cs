namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v2711 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AutoGenerateInfoes", "CompanyId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AutoGenerateInfoes", "CompanyId");
        }
    }
}
