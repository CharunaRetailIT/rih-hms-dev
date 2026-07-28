namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class promotionmodule08 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.InvPromoCustomerCategories", "ModifiedUser", c => c.String(maxLength: 50));
            AddColumn("dbo.InvPromoCustomerCategories", "ModifiedDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.InvPromoCustomerCategories", "ModifiedDate");
            DropColumn("dbo.InvPromoCustomerCategories", "ModifiedUser");
        }
    }
}
