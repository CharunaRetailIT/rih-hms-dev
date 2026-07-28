namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasank098 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Customers", "WeddingAnniversary");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Customers", "WeddingAnniversary", c => c.DateTime(nullable: false));
        }
    }
}
