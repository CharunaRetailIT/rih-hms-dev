namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v278 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.PointsExpirationSchedules", "EndDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.PointsExpirationSchedules", "EndDate", c => c.DateTime(nullable: false));
        }
    }
}
