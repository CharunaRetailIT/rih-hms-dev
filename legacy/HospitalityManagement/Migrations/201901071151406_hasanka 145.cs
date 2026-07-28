namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka145 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TableMasters", "TablePositionX", c => c.Int(nullable: false));
            AddColumn("dbo.TableMasters", "TablePositionY", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TableMasters", "TablePositionY");
            DropColumn("dbo.TableMasters", "TablePositionX");
        }
    }
}
