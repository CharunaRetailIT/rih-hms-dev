namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _036 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PaymentTerms", "GroupOfCompanyID", c => c.Int(nullable: false));
            AddColumn("dbo.PaymentTerms", "CompanyID", c => c.Int(nullable: false));
            AddColumn("dbo.PaymentTerms", "LocationId", c => c.Int(nullable: false));
            AddColumn("dbo.PaymentTerms", "CreatedUser", c => c.String(maxLength: 50));
            AddColumn("dbo.PaymentTerms", "CreatedDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.PaymentTerms", "ModifiedUser", c => c.String(maxLength: 50));
            AddColumn("dbo.PaymentTerms", "ModifiedDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.PaymentTerms", "DataTransfer", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PaymentTerms", "DataTransfer");
            DropColumn("dbo.PaymentTerms", "ModifiedDate");
            DropColumn("dbo.PaymentTerms", "ModifiedUser");
            DropColumn("dbo.PaymentTerms", "CreatedDate");
            DropColumn("dbo.PaymentTerms", "CreatedUser");
            DropColumn("dbo.PaymentTerms", "LocationId");
            DropColumn("dbo.PaymentTerms", "CompanyID");
            DropColumn("dbo.PaymentTerms", "GroupOfCompanyID");
        }
    }
}
