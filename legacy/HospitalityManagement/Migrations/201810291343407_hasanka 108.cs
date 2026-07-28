namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka108 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.RequestNoteAcceptanceDetails", "AvgCost");
        }
        
        public override void Down()
        {
            AddColumn("dbo.RequestNoteAcceptanceDetails", "AvgCost", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
    }
}
