namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka142 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.TableMasters", "TableNumber");
            DropColumn("dbo.TableMasters", "TableState");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TableMasters", "TableState", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.TableMasters", "TableNumber", c => c.Int(nullable: false));
        }
    }
}
