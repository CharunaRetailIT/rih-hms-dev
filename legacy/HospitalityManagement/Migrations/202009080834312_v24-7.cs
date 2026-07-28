namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v247 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.LoyaltyCustomers", "CardNo", c => c.String(maxLength: 50));
            AlterColumn("dbo.LoyaltyCustomers", "ModifiedUser", c => c.String(maxLength: 50));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.LoyaltyCustomers", "ModifiedUser", c => c.String(maxLength: 4000));
            AlterColumn("dbo.LoyaltyCustomers", "CardNo", c => c.String(maxLength: 4000));
        }
    }
}
