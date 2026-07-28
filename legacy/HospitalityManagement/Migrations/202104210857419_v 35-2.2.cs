namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v3522 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TransactionDets", "CancelRemark", c => c.String(maxLength: 100, unicode: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TransactionDets", "CancelRemark");
        }
    }
}
