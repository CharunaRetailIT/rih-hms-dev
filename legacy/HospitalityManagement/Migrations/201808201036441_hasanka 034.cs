namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka034 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.PurchaseOrderHeaders", "DocumentNo", c => c.String(nullable: false, maxLength: 20));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.PurchaseOrderHeaders", "DocumentNo", c => c.String(maxLength: 20));
        }
    }
}
