namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka152 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RstCategories", "CatImage", c => c.Binary());
            AddColumn("dbo.RstCategories", "CatImageName", c => c.String());
            AddColumn("dbo.RstCategories", "CatImageType", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.RstCategories", "CatImageType");
            DropColumn("dbo.RstCategories", "CatImageName");
            DropColumn("dbo.RstCategories", "CatImage");
        }
    }
}
