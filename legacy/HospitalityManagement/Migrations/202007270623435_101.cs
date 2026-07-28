namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _101 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.InvAdvanceNoteDets", "PrinterType", c => c.Int());
            AddColumn("dbo.InvAdvanceNoteDets", "IsAddonItem", c => c.Boolean());
            AddColumn("dbo.InvAdvanceNoteDets", "TableNumber", c => c.Int());
            AddColumn("dbo.InvAdvanceNoteDets", "IsTaxEnable", c => c.Boolean());
            AddColumn("dbo.InvAdvanceNoteDets", "TaxCode", c => c.String(maxLength: 50, unicode: false));
            AddColumn("dbo.InvAdvanceNoteDets", "SplitItemReceiptNo", c => c.String(maxLength: 50, unicode: false));
            AddColumn("dbo.InvAdvanceNoteDets", "IsPritRpt", c => c.Boolean());
            AddColumn("dbo.InvAdvanceNoteDets", "ProductRemark", c => c.String(maxLength: 200, unicode: false));
            AddColumn("dbo.InvAdvanceNoteDets", "OrderStatus", c => c.Int());
            AddColumn("dbo.InvAdvanceNoteDets", "ServingUnit", c => c.String(maxLength: 50, unicode: false));
            AddColumn("dbo.InvAdvanceNoteDets", "NoOfCustomers", c => c.Int());
            AddColumn("dbo.InvAdvanceNoteDets", "IsShowOnBill", c => c.Boolean());
            AddColumn("dbo.InvAdvanceNoteDets", "DeploCardNo", c => c.String(maxLength: 50, unicode: false));
            AddColumn("dbo.InvAdvanceNoteDets", "ServingUnitId", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.InvAdvanceNoteDets", "ServingUnitId");
            DropColumn("dbo.InvAdvanceNoteDets", "DeploCardNo");
            DropColumn("dbo.InvAdvanceNoteDets", "IsShowOnBill");
            DropColumn("dbo.InvAdvanceNoteDets", "NoOfCustomers");
            DropColumn("dbo.InvAdvanceNoteDets", "ServingUnit");
            DropColumn("dbo.InvAdvanceNoteDets", "OrderStatus");
            DropColumn("dbo.InvAdvanceNoteDets", "ProductRemark");
            DropColumn("dbo.InvAdvanceNoteDets", "IsPritRpt");
            DropColumn("dbo.InvAdvanceNoteDets", "SplitItemReceiptNo");
            DropColumn("dbo.InvAdvanceNoteDets", "TaxCode");
            DropColumn("dbo.InvAdvanceNoteDets", "IsTaxEnable");
            DropColumn("dbo.InvAdvanceNoteDets", "TableNumber");
            DropColumn("dbo.InvAdvanceNoteDets", "IsAddonItem");
            DropColumn("dbo.InvAdvanceNoteDets", "PrinterType");
        }
    }
}
