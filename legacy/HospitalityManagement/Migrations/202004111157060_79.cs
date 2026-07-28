namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _79 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CardTypes",
                c => new
                    {
                        CardTypeId = c.Int(nullable: false, identity: true),
                        CardTypeName = c.String(maxLength: 50, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.CardTypeId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.CardTypes");
        }
    }
}
