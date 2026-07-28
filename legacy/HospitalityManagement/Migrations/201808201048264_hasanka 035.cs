namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka035 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PaymentTerms",
                c => new
                    {
                        PaymenttermId = c.Long(nullable: false, identity: true),
                        PaymentTermCode = c.String(nullable: false),
                        PaymentTermName = c.String(nullable: false),
                        CreditPeriod = c.Int(nullable: false),
                        IsDelete = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.PaymenttermId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.PaymentTerms");
        }
    }
}
