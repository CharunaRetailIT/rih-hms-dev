namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _77 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Banks",
                c => new
                    {
                        BankId = c.Int(nullable: false, identity: true),
                        BankName = c.String(maxLength: 100, unicode: false),
                        ISActive = c.Boolean(nullable: false,defaultValue:true),
                    })
                .PrimaryKey(t => t.BankId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Banks");
        }
    }
}
