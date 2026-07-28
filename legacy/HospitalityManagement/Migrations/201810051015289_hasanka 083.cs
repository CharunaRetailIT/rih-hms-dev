namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka083 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Products", "PurchasingUnit");
            DropColumn("dbo.TransferNoteHeaders", "TotQty");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TransferNoteHeaders", "TotQty", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Products", "PurchasingUnit", c => c.String(nullable: false));
        }
    }
}
