namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v252 : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.LoyaltyCardSchems", new[] { "CardMasterID" });
            AlterColumn("dbo.CardMasters", "CardCode", c => c.String(nullable: false, maxLength: 15));
            AlterColumn("dbo.CardMasters", "CardName", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.CardMasters", "IsDelete", c => c.Boolean(nullable: false));
            CreateIndex("dbo.LoyaltyCardSchems", "CardMasterId");
        }
        
        public override void Down()
        {
            DropIndex("dbo.LoyaltyCardSchems", new[] { "CardMasterId" });
            AlterColumn("dbo.CardMasters", "IsDelete", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.CardMasters", "CardName", c => c.String(maxLength: 50));
            AlterColumn("dbo.CardMasters", "CardCode", c => c.String(maxLength: 15));
            CreateIndex("dbo.LoyaltyCardSchems", "CardMasterID");
        }
    }
}
