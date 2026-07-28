namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v2410 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.InvAdvanceNoteDets", "CashierID", c => c.Long(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.InvAdvanceNoteDets", "CashierID", c => c.Int(nullable: false));
        }
    }
}
