namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _46 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PurchaseOrderHeaders", "DeliveryAddress", c => c.String());
            AddColumn("dbo.PurchaseOrderHeaders", "PODate", c => c.DateTime(nullable: false));
            AlterColumn("dbo.PurchaseOrderDetails", "OrderQty", c => c.Decimal(nullable: false, precision: 18, scale: 3));
            AlterColumn("dbo.PurchaseOrderDetails", "FreeQty", c => c.Decimal(nullable: false, precision: 18, scale: 3));
            AlterColumn("dbo.PurchaseOrderDetails", "CurrentQty", c => c.Decimal(nullable: false, precision: 18, scale: 3));
            AlterColumn("dbo.PurchaseOrderDetails", "BalanceQty", c => c.Decimal(nullable: false, precision: 18, scale: 3));
            AlterColumn("dbo.PurchaseOrderDetails", "BalanceFreeQty", c => c.Decimal(nullable: false, precision: 18, scale: 3));
            AlterColumn("dbo.PurchaseOrderDetails", "CostPrice", c => c.Decimal(nullable: false, precision: 18, scale: 3));
            AlterColumn("dbo.PurchaseOrderDetails", "SellingPrice", c => c.Decimal(nullable: false, precision: 18, scale: 3));
            AlterColumn("dbo.PurchaseOrderDetails", "GrossAmount", c => c.Decimal(nullable: false, precision: 18, scale: 3));
            AlterColumn("dbo.PurchaseOrderDetails", "DiscountPercentage", c => c.Decimal(nullable: false, precision: 18, scale: 3));
            AlterColumn("dbo.PurchaseOrderDetails", "DiscountAmount", c => c.Decimal(nullable: false, precision: 18, scale: 3));
            AlterColumn("dbo.PurchaseOrderDetails", "SubTotalDiscount", c => c.Decimal(nullable: false, precision: 18, scale: 3));
            AlterColumn("dbo.PurchaseOrderDetails", "NetAmount", c => c.Decimal(nullable: false, precision: 18, scale: 3));
            AlterColumn("dbo.PurchaseOrderDetails", "ItemTaxTotal", c => c.Decimal(nullable: false, precision: 18, scale: 3));
            AlterColumn("dbo.PurchaseOrderDetails", "CostValue", c => c.Decimal(nullable: false, precision: 18, scale: 3));
            AlterColumn("dbo.PurchaseOrderDetails", "GRNQuantity", c => c.Decimal(nullable: false, precision: 18, scale: 3));
            AlterColumn("dbo.PurchaseOrderHeaders", "GrossAmount", c => c.Decimal(nullable: false, precision: 18, scale: 3));
            AlterColumn("dbo.PurchaseOrderHeaders", "OtherCharges", c => c.Decimal(nullable: false, precision: 18, scale: 3));
            AlterColumn("dbo.PurchaseOrderHeaders", "DiscountAmount", c => c.Decimal(nullable: false, precision: 18, scale: 3));
            AlterColumn("dbo.PurchaseOrderHeaders", "DiscountPercentage", c => c.Decimal(nullable: false, precision: 18, scale: 3));
            AlterColumn("dbo.PurchaseOrderHeaders", "TotalTaxAmount", c => c.Decimal(nullable: false, precision: 18, scale: 3));
            AlterColumn("dbo.PurchaseOrderHeaders", "Addition", c => c.Decimal(nullable: false, precision: 18, scale: 3));
            AlterColumn("dbo.PurchaseOrderHeaders", "Deduction", c => c.Decimal(nullable: false, precision: 18, scale: 3));
            AlterColumn("dbo.PurchaseOrderHeaders", "NetAmount", c => c.Decimal(nullable: false, precision: 18, scale: 3));
            AlterColumn("dbo.PurchaseOrderHeaders", "LineDiscountTotal", c => c.Decimal(nullable: false, precision: 18, scale: 3));
            AlterColumn("dbo.PurchaseOrderHeaders", "TotSellingPrice", c => c.Decimal(nullable: false, precision: 18, scale: 3));
            AlterColumn("dbo.PurchaseOrderHeaders", "TotCostPrice", c => c.Decimal(nullable: false, precision: 18, scale: 3));
            AlterColumn("dbo.PurchaseOrderHeaders", "TotDiscounts", c => c.Decimal(nullable: false, precision: 18, scale: 3));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.PurchaseOrderHeaders", "TotDiscounts", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.PurchaseOrderHeaders", "TotCostPrice", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.PurchaseOrderHeaders", "TotSellingPrice", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.PurchaseOrderHeaders", "LineDiscountTotal", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.PurchaseOrderHeaders", "NetAmount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.PurchaseOrderHeaders", "Deduction", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.PurchaseOrderHeaders", "Addition", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.PurchaseOrderHeaders", "TotalTaxAmount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.PurchaseOrderHeaders", "DiscountPercentage", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.PurchaseOrderHeaders", "DiscountAmount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.PurchaseOrderHeaders", "OtherCharges", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.PurchaseOrderHeaders", "GrossAmount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.PurchaseOrderDetails", "GRNQuantity", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.PurchaseOrderDetails", "CostValue", c => c.String());
            AlterColumn("dbo.PurchaseOrderDetails", "ItemTaxTotal", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.PurchaseOrderDetails", "NetAmount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.PurchaseOrderDetails", "SubTotalDiscount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.PurchaseOrderDetails", "DiscountAmount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.PurchaseOrderDetails", "DiscountPercentage", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.PurchaseOrderDetails", "GrossAmount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.PurchaseOrderDetails", "SellingPrice", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.PurchaseOrderDetails", "CostPrice", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.PurchaseOrderDetails", "BalanceFreeQty", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.PurchaseOrderDetails", "BalanceQty", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.PurchaseOrderDetails", "CurrentQty", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.PurchaseOrderDetails", "FreeQty", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.PurchaseOrderDetails", "OrderQty", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            DropColumn("dbo.PurchaseOrderHeaders", "PODate");
            DropColumn("dbo.PurchaseOrderHeaders", "DeliveryAddress");
        }
    }
}
