namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _35 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.InvPromotionDetailsProductDis", "DepartmentId", c => c.Long(nullable: false));
            AddColumn("dbo.InvPromotionDetailsProductDis", "CategoryId", c => c.Long(nullable: false));
            AddColumn("dbo.InvPromotionDetailsProductDis", "SubCategoryId", c => c.Long(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.InvPromotionDetailsProductDis", "SubCategoryId");
            DropColumn("dbo.InvPromotionDetailsProductDis", "CategoryId");
            DropColumn("dbo.InvPromotionDetailsProductDis", "DepartmentId");
        }
    }
}
