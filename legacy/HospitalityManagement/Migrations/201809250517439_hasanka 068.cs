namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka068 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PriceLevels", "DocumentId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PriceLevels", "DocumentId");
        }
    }
}
