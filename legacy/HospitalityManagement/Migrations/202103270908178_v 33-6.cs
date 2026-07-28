namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v336 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.JobHeaders", "EndTime", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.JobHeaders", "EndTime", c => c.DateTime(nullable: false));
        }
    }
}
