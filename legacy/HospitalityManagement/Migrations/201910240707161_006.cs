namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _006 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Customers", "EPFNo", c => c.String());
            AddColumn("dbo.Customers", "MembershipCardNo", c => c.String());
            AddColumn("dbo.Customers", "Other", c => c.String());
            DropColumn("dbo.Customers", "RefNo01");
            DropColumn("dbo.Customers", "RefNo02");
            DropColumn("dbo.Customers", "RefNo03");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Customers", "RefNo03", c => c.String());
            AddColumn("dbo.Customers", "RefNo02", c => c.String());
            AddColumn("dbo.Customers", "RefNo01", c => c.String());
            DropColumn("dbo.Customers", "Other");
            DropColumn("dbo.Customers", "MembershipCardNo");
            DropColumn("dbo.Customers", "EPFNo");
        }
    }
}
