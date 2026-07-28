namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _103 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.EventProducts",
                c => new
                    {
                        EventProductId = c.Int(nullable: false, identity: true),
                        EventId = c.Int(nullable: false),
                        EventName = c.String(),
                        ProductId = c.Int(nullable: false),
                        ProductName = c.String(maxLength: 100),
                        IsActive = c.Boolean(nullable: false),
                        OrdSeq = c.Int(nullable: false),
                        CreatedUser = c.String(maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedUser = c.String(maxLength: 50),
                        ModifiedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.EventProductId);
            
            AddColumn("dbo.Events", "FromTime", c => c.Time(nullable: false, precision: 7));
            AddColumn("dbo.Events", "ToTime", c => c.Time(nullable: false, precision: 7));
            AddColumn("dbo.Events", "IsPOS", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Events", "IsPOS");
            DropColumn("dbo.Events", "ToTime");
            DropColumn("dbo.Events", "FromTime");
            DropTable("dbo.EventProducts");
        }
    }
}
