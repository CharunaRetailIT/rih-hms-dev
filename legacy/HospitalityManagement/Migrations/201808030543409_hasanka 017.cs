namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka017 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Currencies", "CurrencyCode", c => c.String(nullable: false, maxLength: 5));
            AlterColumn("dbo.Currencies", "CurrencyDescription", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.Currencies", "CurrencySymbol", c => c.String(nullable: false, maxLength: 5));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Currencies", "CurrencySymbol", c => c.String(maxLength: 5));
            AlterColumn("dbo.Currencies", "CurrencyDescription", c => c.String(maxLength: 50));
            AlterColumn("dbo.Currencies", "CurrencyCode", c => c.String(maxLength: 5));
        }
    }
}
