namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v262 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PurchaseOrderHeaders", "RequestNoteId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PurchaseOrderHeaders", "RequestNoteId");
        }
    }
}
