namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v325 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProductionNoteDetails", "ActualQty", c => c.Decimal(nullable: false, precision: 18, scale: 2,defaultValue:0));
            DropColumn("dbo.ProductionNoteHeaders", "ActualQty");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ProductionNoteHeaders", "ActualQty", c => c.Decimal(nullable: false, precision: 18, scale: 2,defaultValue:0));
            DropColumn("dbo.ProductionNoteDetails", "ActualQty");
        }
    }
}
