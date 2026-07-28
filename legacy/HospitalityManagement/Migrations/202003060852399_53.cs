namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _53 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PurchaseOrderDetails", "IsGRN", c => c.Boolean(nullable: false));
            AlterColumn("dbo.PurchaseHeaders", "Remark", c => c.String(nullable: false, maxLength: 150));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.PurchaseHeaders", "Remark", c => c.String(maxLength: 150));
            DropColumn("dbo.PurchaseOrderDetails", "IsGRN");
        }
    }
}
