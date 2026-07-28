namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v281 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SuspendHeds", "OrigSuspendNo", c => c.String(maxLength: 50, unicode: false,nullable:true));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SuspendHeds", "OrigSuspendNo");
        }
    }
}
