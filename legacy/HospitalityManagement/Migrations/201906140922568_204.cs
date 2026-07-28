namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _204 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PayTypes",
                c => new
                    {
                        PayTypeId = c.Long(nullable: false, identity: true),
                        PaymentID = c.Int(nullable: false),
                        Descrip = c.String(),
                        IsSwipe = c.Boolean(nullable: false),
                        Type = c.Int(nullable: false),
                        Rate = c.Decimal(nullable: false, precision: 18, scale: 2),
                        IsRefundable = c.Boolean(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsBillCopy = c.Boolean(nullable: false),
                        PrintDescrip = c.String(),
                        PreFix = c.String(),
                        MaxLength = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.PayTypeId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.PayTypes");
        }
    }
}
