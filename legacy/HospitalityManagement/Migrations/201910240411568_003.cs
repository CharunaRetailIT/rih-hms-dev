namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _003 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CustomerCategories", "IsVat", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.CustomerCategories", "IsVat");
        }
    }
}
