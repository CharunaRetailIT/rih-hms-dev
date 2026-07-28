namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka014 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.DeliveryPersons", "Gender");
        }
        
        public override void Down()
        {
            AddColumn("dbo.DeliveryPersons", "Gender", c => c.String(nullable: false));
        }
    }
}
