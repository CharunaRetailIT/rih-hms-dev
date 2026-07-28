namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka105 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.RequestNoteAccptanceHeaders", "ReferenceNo");
            DropColumn("dbo.RequestNoteAccptanceHeaders", "Remark");
        }
        
        public override void Down()
        {
            AddColumn("dbo.RequestNoteAccptanceHeaders", "Remark", c => c.String(maxLength: 150));
            AddColumn("dbo.RequestNoteAccptanceHeaders", "ReferenceNo", c => c.String(maxLength: 20));
        }
    }
}
