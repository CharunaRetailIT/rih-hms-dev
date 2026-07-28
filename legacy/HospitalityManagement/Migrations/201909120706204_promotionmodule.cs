namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class promotionmodule : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.InvPromotionMasters",
                c => new
                    {
                        InvPromotionMasterID = c.Long(nullable: false, identity: true),
                        CompanyID = c.Int(nullable: false),
                        LocationID = c.Int(nullable: false),
                        CostCentreID = c.Int(nullable: false),
                        PromotionCode = c.String(nullable: false, maxLength: 15),
                        PromotionName = c.String(nullable: false, maxLength: 50),
                        IsAutoApply = c.Boolean(nullable: false),
                        PromotionTypeID = c.Int(nullable: false),
                        StartDate = c.DateTime(),
                        EndDate = c.DateTime(),
                        IsMonday = c.Boolean(nullable: false),
                        IsTuesday = c.Boolean(nullable: false),
                        IsWednesday = c.Boolean(nullable: false),
                        IsThuresday = c.Boolean(nullable: false),
                        IsFriday = c.Boolean(nullable: false),
                        IsSaturday = c.Boolean(nullable: false),
                        IsSunday = c.Boolean(nullable: false),
                        IsMondayTime = c.Boolean(nullable: false),
                        IsTuesdayTime = c.Boolean(nullable: false),
                        IsWednesdayTime = c.Boolean(nullable: false),
                        IsThuresdayTime = c.Boolean(nullable: false),
                        IsFridayTime = c.Boolean(nullable: false),
                        IsSaturdayTime = c.Boolean(nullable: false),
                        IsSundayTime = c.Boolean(nullable: false),
                        MondayStartTime = c.DateTime(),
                        MondayEndTime = c.DateTime(),
                        TuesdayStartTime = c.DateTime(),
                        TuesdayEndTime = c.DateTime(),
                        WednesdayStartTime = c.DateTime(),
                        WednesdayEndTime = c.DateTime(),
                        ThuresdayStartTime = c.DateTime(),
                        ThuresdayEndTime = c.DateTime(),
                        FridayStartTime = c.DateTime(),
                        FridayEndTime = c.DateTime(),
                        SaturdayStartTime = c.DateTime(),
                        SaturdayEndTime = c.DateTime(),
                        SundayStartTime = c.DateTime(),
                        SundayEndTime = c.DateTime(),
                        PaymentMethodID = c.Int(nullable: false),
                        IsProvider = c.Boolean(nullable: false),
                        IsAllLocations = c.Boolean(nullable: false),
                        IsAllType = c.Boolean(nullable: false),
                        IsValueRange = c.Boolean(nullable: false),
                        MinimumValue = c.Decimal(nullable: false, precision: 18, scale: 2),
                        MaximumValue = c.Decimal(nullable: false, precision: 18, scale: 2),
                        DiscountValue = c.Decimal(nullable: false, precision: 18, scale: 2),
                        DiscountPercentage = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Points = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Remark = c.String(maxLength: 150),
                        DisplayMessage = c.String(maxLength: 150),
                        CashierMessage = c.String(maxLength: 150),
                        IsDelete = c.Boolean(nullable: false),
                        IsRaffle = c.Boolean(nullable: false),
                        IsIncreseQty = c.Boolean(nullable: false),
                        PromotionTypeNew = c.Int(nullable: false),
                        SupplierID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.InvPromotionMasterID);
            
            CreateTable(
                "dbo.InvPromotionTypes",
                c => new
                    {
                        InvPromotionTypeID = c.Long(nullable: false, identity: true),
                        PromotionTypeCode = c.String(nullable: false, maxLength: 15),
                        PromotionTypeName = c.String(nullable: false, maxLength: 100),
                        Remark = c.String(maxLength: 150),
                        IsDelete = c.Boolean(nullable: false),
                        GroupOfCompanyID = c.Int(nullable: false),
                        CompanyID = c.Int(nullable: false),
                        LocationId = c.Int(nullable: false),
                        CreatedUser = c.String(maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedUser = c.String(maxLength: 50),
                        ModifiedDate = c.DateTime(nullable: false),
                        DataTransfer = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.InvPromotionTypeID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.InvPromotionTypes");
            DropTable("dbo.InvPromotionMasters");
        }
    }
}
