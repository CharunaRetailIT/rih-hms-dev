namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class h002 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.SysGroupOfCompanies", "CompanyLogo1");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SysGroupOfCompanies", "CompanyLogo1", c => c.String());
        }
    }
}
