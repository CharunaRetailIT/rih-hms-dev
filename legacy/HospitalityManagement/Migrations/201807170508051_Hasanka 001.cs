namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Hasanka001 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SysGroupOfCompanies", "CompanyLogoType", c => c.String());
            AddColumn("dbo.SysGroupOfCompanies", "CompanyLogoName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.SysGroupOfCompanies", "CompanyLogoName");
            DropColumn("dbo.SysGroupOfCompanies", "CompanyLogoType");
        }
    }
}
