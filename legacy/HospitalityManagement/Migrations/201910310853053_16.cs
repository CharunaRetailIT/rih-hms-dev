namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _16 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SuspendDets", "DeploCardNo", c => c.String(maxLength: 50, unicode: false));
            AddColumn("dbo.TransactionDets", "DeploCardNo", c => c.String(maxLength: 50, unicode: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TransactionDets", "DeploCardNo");
            DropColumn("dbo.SuspendDets", "DeploCardNo");
        }
    }
}
