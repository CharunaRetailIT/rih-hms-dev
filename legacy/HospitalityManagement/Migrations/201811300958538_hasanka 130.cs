namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka130 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Customers", "WeddingAnniversary", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Customers", "WeddingAnniversary", c => c.DateTime(nullable: false));
        }
    }
}
