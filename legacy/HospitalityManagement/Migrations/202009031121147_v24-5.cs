namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v245 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CustomProductionNoteHeaders", "ReciptId", c => c.Int(nullable: false));
            DropColumn("dbo.CustomProductionNoteDetails", "ProductSellingPrice");
            DropColumn("dbo.CustomProductionNoteDetails", "MaterialSellingPrice");
        }
        
        public override void Down()
        {
            AddColumn("dbo.CustomProductionNoteDetails", "MaterialSellingPrice", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.CustomProductionNoteDetails", "ProductSellingPrice", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            DropColumn("dbo.CustomProductionNoteHeaders", "ReciptId");
        }
    }
}
