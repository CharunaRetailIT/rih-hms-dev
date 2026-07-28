namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka141 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TableMasters", "TableNumber", c => c.Int(nullable: false));
            AddColumn("dbo.TableMasters", "NumberOfSeats", c => c.Int(nullable: false));
            AddColumn("dbo.TableMasters", "TablePositionX", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.TableMasters", "TablePositionY", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.TableMasters", "TableState", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            DropColumn("dbo.TableMasters", "TicketID");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TableMasters", "TicketID", c => c.Int(nullable: false));
            DropColumn("dbo.TableMasters", "TableState");
            DropColumn("dbo.TableMasters", "TablePositionY");
            DropColumn("dbo.TableMasters", "TablePositionX");
            DropColumn("dbo.TableMasters", "NumberOfSeats");
            DropColumn("dbo.TableMasters", "TableNumber");
        }
    }
}
