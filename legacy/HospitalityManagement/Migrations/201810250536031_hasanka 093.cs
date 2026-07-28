namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka093 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProductionNoteDetails", "ProductId", c => c.Long(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ProductionNoteDetails", "ProductId");
        }
    }
}
