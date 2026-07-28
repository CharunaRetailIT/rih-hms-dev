namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka099 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Customers", "WeddingAnniversary", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Customers", "WeddingAnniversary");
        }
    }
}
