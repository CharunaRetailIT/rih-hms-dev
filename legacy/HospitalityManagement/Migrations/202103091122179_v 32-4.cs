namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v324 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProductionNoteHeaders", "ActualQty", c => c.Decimal(nullable: false, precision: 18, scale: 2,defaultValue:0));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ProductionNoteHeaders", "ActualQty");
        }
    }
}
