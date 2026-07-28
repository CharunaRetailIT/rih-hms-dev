namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka178 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ProductInstructions",
                c => new
                    {
                        ProductInstructionId = c.Long(nullable: false, identity: true),
                        InstructionList = c.String(),
                        ProductId = c.Long(nullable: false),
                        CreateDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ProductInstructionId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.ProductInstructions");
        }
    }
}
