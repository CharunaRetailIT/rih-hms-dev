namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _86 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.SuspendDets", "ExpiaryDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.SuspendDets", "ExpiaryDate", c => c.DateTime(nullable: false));
        }
    }
}
