namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka131 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PrinterTypes",
                c => new
                    {
                        PrinterTypeId = c.Int(nullable: false, identity: true),
                        PrinterTypeName = c.String(),
                        IsDelete = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.PrinterTypeId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.PrinterTypes");
        }
    }
}
