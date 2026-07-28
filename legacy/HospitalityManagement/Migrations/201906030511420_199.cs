namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _199 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProductInstructions", "ModifiedDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ProductInstructions", "ModifiedDate");
        }
    }
}
