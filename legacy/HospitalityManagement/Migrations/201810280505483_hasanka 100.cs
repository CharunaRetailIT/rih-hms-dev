namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka100 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Customers", "CustomerCode");
            DropColumn("dbo.Customers", "CustomerTitle");
            DropColumn("dbo.Customers", "CustomerName");
            DropColumn("dbo.Customers", "CustomerType");
            DropColumn("dbo.Customers", "Address1");
            DropColumn("dbo.Customers", "Address2");
            DropColumn("dbo.Customers", "Address3");
            DropColumn("dbo.Customers", "DOB");
            DropColumn("dbo.Customers", "NIC");
            DropColumn("dbo.Customers", "Passport");
            DropColumn("dbo.Customers", "Telephone");
            DropColumn("dbo.Customers", "Mobile");
            DropColumn("dbo.Customers", "Fax");
            DropColumn("dbo.Customers", "Email");
            DropColumn("dbo.Customers", "VehicleNo");
            DropColumn("dbo.Customers", "Profession");
            DropColumn("dbo.Customers", "WeddingAnniversary");
            DropColumn("dbo.Customers", "IsActiveForLoyalty");
            DropColumn("dbo.Customers", "CustomerPicture");
            DropColumn("dbo.Customers", "CustomerPictureName");
            DropColumn("dbo.Customers", "CustomerPictureType");
            DropColumn("dbo.Customers", "IsActive");
            DropColumn("dbo.Customers", "IsDelete");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Customers", "IsDelete", c => c.Boolean(nullable: false));
            AddColumn("dbo.Customers", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.Customers", "CustomerPictureType", c => c.String());
            AddColumn("dbo.Customers", "CustomerPictureName", c => c.String());
            AddColumn("dbo.Customers", "CustomerPicture", c => c.Binary());
            AddColumn("dbo.Customers", "IsActiveForLoyalty", c => c.Boolean(nullable: false));
            AddColumn("dbo.Customers", "WeddingAnniversary", c => c.DateTime(nullable: false));
            AddColumn("dbo.Customers", "Profession", c => c.String());
            AddColumn("dbo.Customers", "VehicleNo", c => c.String());
            AddColumn("dbo.Customers", "Email", c => c.String());
            AddColumn("dbo.Customers", "Fax", c => c.String());
            AddColumn("dbo.Customers", "Mobile", c => c.String());
            AddColumn("dbo.Customers", "Telephone", c => c.String());
            AddColumn("dbo.Customers", "Passport", c => c.String());
            AddColumn("dbo.Customers", "NIC", c => c.String(nullable: false, maxLength: 12));
            AddColumn("dbo.Customers", "DOB", c => c.DateTime(nullable: false));
            AddColumn("dbo.Customers", "Address3", c => c.String());
            AddColumn("dbo.Customers", "Address2", c => c.String(nullable: false, maxLength: 100));
            AddColumn("dbo.Customers", "Address1", c => c.String(nullable: false, maxLength: 100));
            AddColumn("dbo.Customers", "CustomerType", c => c.String());
            AddColumn("dbo.Customers", "CustomerName", c => c.String(nullable: false, maxLength: 100));
            AddColumn("dbo.Customers", "CustomerTitle", c => c.String(nullable: false));
            AddColumn("dbo.Customers", "CustomerCode", c => c.String(nullable: false));
        }
    }
}
