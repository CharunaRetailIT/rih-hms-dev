namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka128 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProductionNoteHeaders", "DocumentId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ProductionNoteHeaders", "DocumentId");
        }
    }
}
