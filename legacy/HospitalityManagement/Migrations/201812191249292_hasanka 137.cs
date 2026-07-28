namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka137 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Receipes", "ProductServingUnitId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Receipes", "ProductServingUnitId", c => c.String());
        }
    }
}
