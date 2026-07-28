namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v2712 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SysConfigurations", "CreateGroupOfCompanies", c => c.Boolean(nullable: false));
            AddColumn("dbo.SysConfigurations", "CreateCompanies", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SysConfigurations", "CreateCompanies");
            DropColumn("dbo.SysConfigurations", "CreateGroupOfCompanies");
        }
    }
}
