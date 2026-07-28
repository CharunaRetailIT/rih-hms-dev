namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka154 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RstSubCategories", "SubCatImage", c => c.Binary());
            AddColumn("dbo.RstSubCategories", "SubCatImageName", c => c.String());
            AddColumn("dbo.RstSubCategories", "SubCatImageType", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.RstSubCategories", "SubCatImageType");
            DropColumn("dbo.RstSubCategories", "SubCatImageName");
            DropColumn("dbo.RstSubCategories", "SubCatImage");
        }
    }
}
