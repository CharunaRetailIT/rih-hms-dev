namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka172 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TableMasters", "InterDeptId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TableMasters", "InterDeptId");
        }
    }
}
