namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _210 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TransferNoteDetails", "TOGQty", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TransferNoteDetails", "TOGQty");
        }
    }
}
