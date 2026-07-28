namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka171 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.TableMasters", "InterDeptId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TableMasters", "InterDeptId", c => c.Int(nullable: false));
        }
    }
}
