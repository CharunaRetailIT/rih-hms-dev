namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class promotionmodule09 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.InvPromoBusinessTypes", "ModifiedUserUser", c => c.String(maxLength: 50));
            AddColumn("dbo.InvPromoBusinessTypes", "ModifiedDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.InvPromoBusinessTypes", "ModifiedDate");
            DropColumn("dbo.InvPromoBusinessTypes", "ModifiedUserUser");
        }
    }
}
