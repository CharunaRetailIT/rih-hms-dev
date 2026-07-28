namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _009 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Customers", "CustomerType", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Customers", "CustomerType");
        }
    }
}
