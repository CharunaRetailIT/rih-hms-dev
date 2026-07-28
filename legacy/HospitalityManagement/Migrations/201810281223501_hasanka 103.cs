namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka103 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.RequestNoteDetails",
                c => new
                    {
                        RequestNoteDetailId = c.Long(nullable: false, identity: true),
                        RequestnoteHeaderId = c.Long(nullable: false),
                        LineNo = c.Long(nullable: false),
                        ProductId = c.Long(nullable: false),
                        AvgCost = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CostPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        SellingPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        RequestQty = c.Decimal(nullable: false, precision: 18, scale: 2),
                        UnitOfMeasureId = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.RequestNoteDetailId);
            
            CreateTable(
                "dbo.RequestNoteHeaders",
                c => new
                    {
                        RequestnoteHeaderId = c.Long(nullable: false, identity: true),
                        FromLocationId = c.Int(nullable: false),
                        FromDepartmentId = c.Int(nullable: false),
                        ToLocationId = c.Int(nullable: false),
                        ToDepartmentId = c.Int(nullable: false),
                        DocumentNo = c.String(maxLength: 20),
                        DocumentDate = c.DateTime(nullable: false),
                        ReferenceNo = c.String(maxLength: 20),
                        Remark = c.String(maxLength: 150),
                        IsActive = c.Boolean(nullable: false),
                        TotSellingPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TotCostPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        IsTempRequest = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.RequestnoteHeaderId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.RequestNoteHeaders");
            DropTable("dbo.RequestNoteDetails");
        }
    }
}
