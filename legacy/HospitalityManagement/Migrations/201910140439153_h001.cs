namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class h001 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SysGroupOfCompanies", "CompanyLogo1", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.SysGroupOfCompanies", "CompanyLogo1");
        }
    }
}
