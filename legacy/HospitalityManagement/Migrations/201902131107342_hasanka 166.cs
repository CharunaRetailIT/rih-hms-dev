namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka166 : DbMigration
    {
        public override void Up()
        {
            DropTable("dbo.KOTBOTDescriptions");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.KOTBOTDescriptions",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Description = c.String(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
    }
}
