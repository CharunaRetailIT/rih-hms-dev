namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka079 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProductionNoteHeaders", "ProductQty", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ProductionNoteHeaders", "ProductQty");
        }
    }
}
