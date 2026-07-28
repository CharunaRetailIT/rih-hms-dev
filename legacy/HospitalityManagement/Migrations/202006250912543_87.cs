namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _87 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.SuspendDets", "ReferenceProductId", c => c.Int());
            AlterColumn("dbo.SuspendDets", "ReferenceProductRow", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.SuspendDets", "ReferenceProductRow", c => c.Int(nullable: false));
            AlterColumn("dbo.SuspendDets", "ReferenceProductId", c => c.Int(nullable: false));
        }
    }
}
