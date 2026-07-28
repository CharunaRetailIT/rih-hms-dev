namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v277 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PointsExpirationSchedules", "PointsExpirationId", c => c.Int(nullable: false));
            AlterColumn("dbo.PointsExpirations", "FirstReminderMessage", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.PointsExpirations", "FirstReminderMessage", c => c.String());
            DropColumn("dbo.PointsExpirationSchedules", "PointsExpirationId");
        }
    }
}
