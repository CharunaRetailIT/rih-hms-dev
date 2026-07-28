namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka008 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.RstSubCategories", "GroupOfCompanyID");
            DropColumn("dbo.RstSubCategories", "CompanyID");
            DropColumn("dbo.RstSubCategories", "LocationId");
            DropColumn("dbo.RstSubCategories", "CreatedUser");
            DropColumn("dbo.RstSubCategories", "CreatedDate");
            DropColumn("dbo.RstSubCategories", "ModifiedUser");
            DropColumn("dbo.RstSubCategories", "ModifiedDate");
            DropColumn("dbo.RstSubCategories", "DataTransfer");
        }
        
        public override void Down()
        {
            AddColumn("dbo.RstSubCategories", "DataTransfer", c => c.Int(nullable: false));
            AddColumn("dbo.RstSubCategories", "ModifiedDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.RstSubCategories", "ModifiedUser", c => c.String(maxLength: 50));
            AddColumn("dbo.RstSubCategories", "CreatedDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.RstSubCategories", "CreatedUser", c => c.String(maxLength: 50));
            AddColumn("dbo.RstSubCategories", "LocationId", c => c.Int(nullable: false));
            AddColumn("dbo.RstSubCategories", "CompanyID", c => c.Int(nullable: false));
            AddColumn("dbo.RstSubCategories", "GroupOfCompanyID", c => c.Int(nullable: false));
        }
    }
}
