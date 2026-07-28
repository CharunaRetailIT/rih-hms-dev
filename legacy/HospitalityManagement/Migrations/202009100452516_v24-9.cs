namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v249 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.LOGCustomers", "Gender", c => c.Int(nullable: false));
            AddColumn("dbo.LOGCustomers", "ReferenceNo1", c => c.String(maxLength: 50));
            AddColumn("dbo.LOGCustomers", "ReferenceNo2", c => c.String(maxLength: 50));
            AddColumn("dbo.LOGCustomers", "Age", c => c.Int(nullable: false));
            AddColumn("dbo.LOGCustomers", "Religion", c => c.Int());
            AddColumn("dbo.LOGCustomers", "Race", c => c.Int());
            AddColumn("dbo.LOGCustomers", "LandMark", c => c.String(maxLength: 50));
            AddColumn("dbo.LOGCustomers", "District", c => c.String(maxLength: 50));
            AddColumn("dbo.LOGCustomers", "Organization", c => c.String(maxLength: 50));
            AddColumn("dbo.LOGCustomers", "WorkAddres1", c => c.String(maxLength: 50));
            AddColumn("dbo.LOGCustomers", "WorkAddres2", c => c.String(maxLength: 50));
            AddColumn("dbo.LOGCustomers", "WorkAddres3", c => c.String(maxLength: 50));
            AddColumn("dbo.LOGCustomers", "WorkEmail", c => c.String(maxLength: 50));
            AddColumn("dbo.LOGCustomers", "WorkTelephone", c => c.String(maxLength: 50));
            AddColumn("dbo.LOGCustomers", "WorkMobile", c => c.String(maxLength: 50));
            AddColumn("dbo.LOGCustomers", "WorkFax", c => c.String(maxLength: 50));
            AddColumn("dbo.LOGCustomers", "SpouseName", c => c.String(maxLength: 50));
            AddColumn("dbo.LOGCustomers", "CivilStatus", c => c.Int(nullable: false));
            AddColumn("dbo.LOGCustomers", "SpouseDateOfBirth", c => c.DateTime());
            AddColumn("dbo.LOGCustomers", "DeliverTo", c => c.Int(nullable: false));
            AddColumn("dbo.LOGCustomers", "DeliverToAddress", c => c.String(maxLength: 50));
            AddColumn("dbo.LOGCustomers", "Country", c => c.String(maxLength: 50));
            AddColumn("dbo.LOGCustomers", "CustomerSince", c => c.DateTime());
            AddColumn("dbo.LOGCustomers", "SpecialDayType", c => c.Int(nullable: false));
            AddColumn("dbo.LOGCustomers", "SendUpdatesViaEmail", c => c.Boolean(nullable: false));
            AddColumn("dbo.LOGCustomers", "SendUpdatesViaSms", c => c.Boolean(nullable: false));
            AddColumn("dbo.LOGCustomers", "IsRegByPOS", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.LOGCustomers", "IsRegByPOS");
            DropColumn("dbo.LOGCustomers", "SendUpdatesViaSms");
            DropColumn("dbo.LOGCustomers", "SendUpdatesViaEmail");
            DropColumn("dbo.LOGCustomers", "SpecialDayType");
            DropColumn("dbo.LOGCustomers", "CustomerSince");
            DropColumn("dbo.LOGCustomers", "Country");
            DropColumn("dbo.LOGCustomers", "DeliverToAddress");
            DropColumn("dbo.LOGCustomers", "DeliverTo");
            DropColumn("dbo.LOGCustomers", "SpouseDateOfBirth");
            DropColumn("dbo.LOGCustomers", "CivilStatus");
            DropColumn("dbo.LOGCustomers", "SpouseName");
            DropColumn("dbo.LOGCustomers", "WorkFax");
            DropColumn("dbo.LOGCustomers", "WorkMobile");
            DropColumn("dbo.LOGCustomers", "WorkTelephone");
            DropColumn("dbo.LOGCustomers", "WorkEmail");
            DropColumn("dbo.LOGCustomers", "WorkAddres3");
            DropColumn("dbo.LOGCustomers", "WorkAddres2");
            DropColumn("dbo.LOGCustomers", "WorkAddres1");
            DropColumn("dbo.LOGCustomers", "Organization");
            DropColumn("dbo.LOGCustomers", "District");
            DropColumn("dbo.LOGCustomers", "LandMark");
            DropColumn("dbo.LOGCustomers", "Race");
            DropColumn("dbo.LOGCustomers", "Religion");
            DropColumn("dbo.LOGCustomers", "Age");
            DropColumn("dbo.LOGCustomers", "ReferenceNo2");
            DropColumn("dbo.LOGCustomers", "ReferenceNo1");
            DropColumn("dbo.LOGCustomers", "Gender");
        }
    }
}
