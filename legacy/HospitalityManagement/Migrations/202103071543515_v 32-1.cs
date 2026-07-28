namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v321 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.KitchenMasters",
                c => new
                    {
                        KitchenID = c.Long(nullable: false, identity: true),
                        KitchenCode = c.String(maxLength: 10, unicode: false,nullable:false,defaultValue:""),
                        KitchenDesc = c.String(maxLength: 20, unicode: false, nullable: false, defaultValue: ""),
                        KitchenPrinterName = c.String(maxLength: 100, unicode: false, nullable: false, defaultValue: ""),
                        KitchenPrinterType = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.KitchenID);
            
            AddColumn("dbo.LOGProducts", "KitchenCode", c => c.String(maxLength: 10, unicode: false, nullable: false, defaultValue: ""));
            AddColumn("dbo.Products", "KitchenCode", c => c.String(maxLength: 10, unicode: false, nullable: false, defaultValue: ""));
            AddColumn("dbo.SuspendDets", "KitchenCode", c => c.String(maxLength: 10, unicode: false,nullable:false, defaultValue: ""));
            AddColumn("dbo.TransactionDets", "KitchenCode", c => c.String(maxLength: 10, unicode: false, nullable: false, defaultValue: ""));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TransactionDets", "KitchenCode");
            DropColumn("dbo.SuspendDets", "KitchenCode");
            DropColumn("dbo.Products", "KitchenCode");
            DropColumn("dbo.LOGProducts", "KitchenCode");
            DropTable("dbo.KitchenMasters");
        }
    }
}
