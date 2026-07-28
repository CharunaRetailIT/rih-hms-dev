namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka095 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProductionNoteDetails", "ProductName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ProductionNoteDetails", "ProductName");
        }
    }
}
