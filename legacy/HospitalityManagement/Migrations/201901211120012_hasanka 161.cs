namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka161 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.POSUserGroups",
                c => new
                    {
                        POSUserGroupId = c.Int(nullable: false, identity: true),
                        POSUserGroupName = c.String(nullable: false),
                        POSUserGroupDesc = c.String(nullable: false),
                        CreatedUser = c.String(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedUser = c.String(nullable: false),
                        ModifiedDate = c.DateTime(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsDelete = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.POSUserGroupId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.POSUserGroups");
        }
    }
}
