namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka009 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Customers", "CustomerPictureName", c => c.String());
            AddColumn("dbo.Customers", "CustomerPictureType", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Customers", "CustomerPictureType");
            DropColumn("dbo.Customers", "CustomerPictureName");
        }
    }
}
