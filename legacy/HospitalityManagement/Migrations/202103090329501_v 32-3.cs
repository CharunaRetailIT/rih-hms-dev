namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v323 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.KitchenMasters", "GroupOfCompanyID", c => c.Int(nullable: false));
            AddColumn("dbo.KitchenMasters", "CompanyID", c => c.Int(nullable: false));
            AddColumn("dbo.KitchenMasters", "LocationId", c => c.Int(nullable: false));
            AddColumn("dbo.KitchenMasters", "CreatedUser", c => c.String(maxLength: 50));
            AddColumn("dbo.KitchenMasters", "CreatedDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.KitchenMasters", "ModifiedUser", c => c.String(maxLength: 50));
            AddColumn("dbo.KitchenMasters", "ModifiedDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.KitchenMasters", "DataTransfer", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.KitchenMasters", "DataTransfer");
            DropColumn("dbo.KitchenMasters", "ModifiedDate");
            DropColumn("dbo.KitchenMasters", "ModifiedUser");
            DropColumn("dbo.KitchenMasters", "CreatedDate");
            DropColumn("dbo.KitchenMasters", "CreatedUser");
            DropColumn("dbo.KitchenMasters", "LocationId");
            DropColumn("dbo.KitchenMasters", "CompanyID");
            DropColumn("dbo.KitchenMasters", "GroupOfCompanyID");
        }
    }
}
