namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka167 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.KOTBOTDescriptions",
                c => new
                    {
                        KOTBOTDescriptionId = c.Long(nullable: false, identity: true),
                        Description = c.String(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.KOTBOTDescriptionId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.KOTBOTDescriptions");
        }
    }
}
