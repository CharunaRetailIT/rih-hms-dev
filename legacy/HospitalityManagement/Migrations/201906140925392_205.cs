namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _205 : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.PayTypes");
            AlterColumn("dbo.PayTypes", "PaymentID", c => c.Int(nullable: false, identity: true));
            AddPrimaryKey("dbo.PayTypes", "PaymentID");
            DropColumn("dbo.PayTypes", "PayTypeId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PayTypes", "PayTypeId", c => c.Long(nullable: false, identity: true));
            DropPrimaryKey("dbo.PayTypes");
            AlterColumn("dbo.PayTypes", "PaymentID", c => c.Int(nullable: false));
            AddPrimaryKey("dbo.PayTypes", "PayTypeId");
        }
    }
}
