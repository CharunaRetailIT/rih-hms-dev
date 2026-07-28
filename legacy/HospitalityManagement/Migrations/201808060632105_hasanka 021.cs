namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka021 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UnitConversions", "SubUnitSymbol", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.UnitConversions", "SubUnitSymbol");
        }
    }
}
