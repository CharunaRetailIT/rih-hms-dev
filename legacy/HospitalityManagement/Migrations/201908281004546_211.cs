namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _211 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PurchaseDetails", "TOGQty", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            DropColumn("dbo.TransferNoteDetails", "TOGQty");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TransferNoteDetails", "TOGQty", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            DropColumn("dbo.PurchaseDetails", "TOGQty");
        }
    }
}
