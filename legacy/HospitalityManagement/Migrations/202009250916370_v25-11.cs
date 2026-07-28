namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v2511 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Customers", "Religion", c => c.String(maxLength: 20));
            AlterColumn("dbo.Customers", "Race", c => c.String(maxLength: 20));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Customers", "Race", c => c.Int());
            AlterColumn("dbo.Customers", "Religion", c => c.Int());
        }
    }
}
