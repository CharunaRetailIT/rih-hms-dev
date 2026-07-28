namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka087 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SysConfigurations", "IsTaxInclusiveToCost", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SysConfigurations", "IsTaxInclusiveToCost");
        }
    }
}
