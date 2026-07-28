namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v275 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Configurations", "CompanyId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Configurations", "CompanyId");
        }
    }
}
