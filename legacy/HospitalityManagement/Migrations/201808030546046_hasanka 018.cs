namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka018 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Currencies", "CurrencyFormat", c => c.String(nullable: false, maxLength: 15));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Currencies", "CurrencyFormat", c => c.String(maxLength: 15));
        }
    }
}
