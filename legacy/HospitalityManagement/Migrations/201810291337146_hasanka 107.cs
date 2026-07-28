namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka107 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RequestNoteAcceptanceDetails", "MaterialId", c => c.Long(nullable: false));
            AddColumn("dbo.RequestNoteAcceptanceDetails", "MaterialQty", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.RequestNoteAcceptanceDetails", "IssueQty", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            DropColumn("dbo.RequestNoteAcceptanceDetails", "RequestQty");
        }
        
        public override void Down()
        {
            AddColumn("dbo.RequestNoteAcceptanceDetails", "RequestQty", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            DropColumn("dbo.RequestNoteAcceptanceDetails", "IssueQty");
            DropColumn("dbo.RequestNoteAcceptanceDetails", "MaterialQty");
            DropColumn("dbo.RequestNoteAcceptanceDetails", "MaterialId");
        }
    }
}
