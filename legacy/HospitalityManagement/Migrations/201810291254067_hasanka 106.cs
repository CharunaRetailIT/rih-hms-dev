namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka106 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RequestNoteAccptanceHeaders", "Remark", c => c.String(maxLength: 150));
        }
        
        public override void Down()
        {
            DropColumn("dbo.RequestNoteAccptanceHeaders", "Remark");
        }
    }
}
