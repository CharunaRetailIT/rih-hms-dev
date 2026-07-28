namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka144 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.TableMasters", "TablePositionX");
            DropColumn("dbo.TableMasters", "TablePositionY");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TableMasters", "TablePositionY", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.TableMasters", "TablePositionX", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
    }
}
