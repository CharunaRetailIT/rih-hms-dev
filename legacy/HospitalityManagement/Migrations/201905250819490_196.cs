namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _196 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.TmpProductStockDetails", "TransactionType", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.TmpProductStockDetails", "TransactionType", c => c.String());
        }
    }
}
