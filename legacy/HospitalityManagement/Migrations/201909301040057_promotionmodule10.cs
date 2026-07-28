namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class promotionmodule10 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.InvPromoBusinessTypes", "ModifiedUser", c => c.String(maxLength: 50));
            DropColumn("dbo.InvPromoBusinessTypes", "ModifiedUserUser");
        }
        
        public override void Down()
        {
            AddColumn("dbo.InvPromoBusinessTypes", "ModifiedUserUser", c => c.String(maxLength: 50));
            DropColumn("dbo.InvPromoBusinessTypes", "ModifiedUser");
        }
    }
}
