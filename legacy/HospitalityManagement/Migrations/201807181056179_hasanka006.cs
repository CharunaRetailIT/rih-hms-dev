namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka006 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AutoGenerateInfoes",
                c => new
                    {
                        AutoGenerateInfoID = c.Long(nullable: false, identity: true),
                        ModuleType = c.Int(nullable: false),
                        DocumentID = c.Int(nullable: false),
                        FormId = c.Int(nullable: false),
                        FormName = c.String(nullable: false, maxLength: 50),
                        FormText = c.String(nullable: false, maxLength: 100),
                        Prefix = c.String(maxLength: 3),
                        Prefix2 = c.String(maxLength: 3),
                        CodeLength = c.Int(nullable: false),
                        Suffix = c.Int(nullable: false),
                        AutoGenerete = c.Boolean(nullable: false),
                        AutoClear = c.Boolean(nullable: false),
                        IsDepend = c.Boolean(nullable: false),
                        IsDependCode = c.Boolean(nullable: false),
                        IsSupplierProduct = c.Boolean(nullable: false),
                        IsOverWriteQty = c.Boolean(nullable: false),
                        IsLocationCode = c.Boolean(nullable: false),
                        ReportPrefix = c.String(maxLength: 3),
                        ReportType = c.Int(nullable: false),
                        PoIsMandatory = c.Boolean(nullable: false),
                        IsDispatchRecall = c.Boolean(nullable: false),
                        IsBackDated = c.Boolean(nullable: false),
                        IsCard = c.Boolean(nullable: false),
                        CardId = c.Int(nullable: false),
                        IsEntry = c.Boolean(nullable: false),
                        IsSlabReport = c.Boolean(nullable: false),
                        IsConsignment = c.Boolean(nullable: false),
                        IsRoundOff = c.Boolean(nullable: false),
                        IsAutoComplete = c.Boolean(nullable: false),
                        IsUpdateProductImage = c.Boolean(nullable: false),
                        IsAllowedInHO = c.Boolean(nullable: false),
                        IsAllowedInOutlet = c.Boolean(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        Layout = c.String(),
                        LayoutNew = c.String(),
                        ReferenceDocumentID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.AutoGenerateInfoID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.AutoGenerateInfoes");
        }
    }
}
