namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka114 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Suppliers", "SupplierGroupID", c => c.String(maxLength: 30));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Suppliers", "SupplierGroupID", c => c.String(nullable: false, maxLength: 30));
        }
    }
}
