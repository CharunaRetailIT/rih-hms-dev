namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka143 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TableMasters", "TableState", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.TableMasters", "TableState");
        }
    }
}
