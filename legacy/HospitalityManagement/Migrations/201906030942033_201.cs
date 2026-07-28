namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _201 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SuspendDets", "ModifiedDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.SuspendHeds", "ModifiedDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SuspendHeds", "ModifiedDate");
            DropColumn("dbo.SuspendDets", "ModifiedDate");
        }
    }
}
