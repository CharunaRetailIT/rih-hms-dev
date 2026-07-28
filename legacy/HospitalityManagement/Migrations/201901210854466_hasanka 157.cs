namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka157 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.CashierGroups", "Type", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.CashierGroups", "Type", c => c.String(nullable: false));
        }
    }
}
