namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _007 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Customers", "EPFNo", c => c.String(maxLength: 50, unicode: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Customers", "EPFNo", c => c.String());
        }
    }
}
