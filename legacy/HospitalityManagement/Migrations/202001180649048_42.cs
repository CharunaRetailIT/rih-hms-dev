namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _42 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Configurations",
                c => new
                    {
                        ConfigurationId = c.Int(nullable: false, identity: true),
                        ConfigurationKey = c.String(maxLength: 10),
                        ConfigurationDescription = c.String(maxLength: 50),
                        EffectLocationId = c.Int(nullable: false),
                        ConfigurationOn = c.Boolean(nullable: false),
                        ConfigurationActive = c.Boolean(nullable: false),
                        ConfigurationDelete = c.Boolean(nullable: false),
                        CreateDate = c.DateTime(),
                        CreateUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ConfigurationId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Configurations");
        }
    }
}
