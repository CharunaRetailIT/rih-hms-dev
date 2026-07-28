namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _008 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Customers", "MembershipCardNo", c => c.String(maxLength: 50, unicode: false));
            AlterColumn("dbo.Customers", "Other", c => c.String(maxLength: 50, unicode: false));
            AlterColumn("dbo.Customers", "Remarks", c => c.String(maxLength: 200, unicode: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Customers", "Remarks", c => c.String());
            AlterColumn("dbo.Customers", "Other", c => c.String());
            AlterColumn("dbo.Customers", "MembershipCardNo", c => c.String());
        }
    }
}
