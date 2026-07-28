namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CustomerCategories",
                c => new
                    {
                        CustomerCategoryID = c.Int(nullable: false, identity: true),
                        CustomerCategoryCode = c.String(nullable: false),
                        CustomerCategoryName = c.String(nullable: false),
                        Remark = c.String(),
                        IsActive = c.Boolean(nullable: false),
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
                .PrimaryKey(t => t.CustomerCategoryID);
            
            CreateTable(
                "dbo.Customers",
                c => new
                    {
                        CustomerID = c.Int(nullable: false, identity: true),
                        CustomerCode = c.String(nullable: false),
                        CustomerTitle = c.String(nullable: false),
                        CustomerName = c.String(nullable: false, maxLength: 100),
                        CustomerType = c.String(),
                        Address1 = c.String(nullable: false, maxLength: 100),
                        Address2 = c.String(nullable: false, maxLength: 100),
                        Address3 = c.String(),
                        DOB = c.DateTime(nullable: false),
                        NIC = c.String(nullable: false, maxLength: 12),
                        Passport = c.String(),
                        Telephone = c.String(),
                        Mobile = c.String(),
                        Fax = c.String(),
                        Email = c.String(),
                        VehicleNo = c.String(),
                        Profession = c.String(),
                        WeddingAnniversary = c.DateTime(nullable: false),
                        IsActiveForLoyalty = c.Boolean(nullable: false),
                        CustomerPicture = c.Binary(),
                        IsActive = c.Boolean(nullable: false),
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
                .PrimaryKey(t => t.CustomerID);
            
            CreateTable(
                "dbo.CustomoerPreviousVisits",
                c => new
                    {
                        CustomoerPreviousVisitsID = c.Int(nullable: false, identity: true),
                        CustomerID = c.Int(nullable: false),
                        CustomoerPreviousVisitsCode = c.String(nullable: false),
                        Description = c.String(),
                        FromDate = c.DateTime(nullable: false),
                        ToDate = c.DateTime(nullable: false),
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
                .PrimaryKey(t => t.CustomoerPreviousVisitsID);
            
            CreateTable(
                "dbo.InvProductMasters",
                c => new
                    {
                        InvProductMasterID = c.Int(nullable: false, identity: true),
                        ProductCode = c.String(nullable: false, maxLength: 20),
                        BarCode = c.String(),
                        ReferenceCode = c.String(),
                        ProductName = c.String(nullable: false, maxLength: 100),
                        InvoicePrintName = c.String(nullable: false, maxLength: 50),
                        SinhalaDescription = c.String(),
                        Department = c.Int(nullable: false),
                        Category = c.Int(nullable: false),
                        SubCategory = c.Int(nullable: false),
                        SubCategory2 = c.Int(nullable: false),
                        KitchecnBarCategory = c.Int(nullable: false),
                        SuplierID = c.Int(nullable: false),
                        Image = c.Binary(),
                        CostPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        OrderPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        AverageCost = c.Decimal(nullable: false, precision: 18, scale: 2),
                        SellingPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        WholesalePrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        MinimumPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        FixedDiscount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        MaximumDiscount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        MaximumPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        FixDiscountPercentage = c.Decimal(nullable: false, precision: 18, scale: 2),
                        MaximumDiscountPercentage = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ReorderLevel = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ReorderQty = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ReorderPeriod = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Remarks = c.String(),
                        IsActive = c.Boolean(nullable: false),
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
                .PrimaryKey(t => t.InvProductMasterID);
            
            CreateTable(
                "dbo.InvSuppliers",
                c => new
                    {
                        InvSupplierID = c.Int(nullable: false, identity: true),
                        SupplierCode = c.String(nullable: false),
                        SupplierName = c.String(nullable: false, maxLength: 100),
                        SupplierType = c.String(),
                        Address1 = c.String(),
                        Address2 = c.String(),
                        Address3 = c.String(),
                        Telephone = c.String(),
                        Mobile = c.String(),
                        Fax = c.String(),
                        Email = c.String(),
                        ContactPerson = c.String(),
                        ConsignmentType = c.Int(nullable: false),
                        CreditLimit = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CreditPeriod = c.Decimal(nullable: false, precision: 18, scale: 2),
                        OpeningBalance = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CurrentMonthPurchase = c.String(),
                        CurrentMonthReturns = c.String(),
                        CurrentMonthPayments = c.String(),
                        TotalOutstandings = c.Decimal(nullable: false, precision: 18, scale: 2),
                        SupplierGroup = c.Int(nullable: false),
                        SupplierOrderCycle = c.String(),
                        SupplierVATRegNo = c.String(),
                        IsActive = c.Boolean(nullable: false),
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
                .PrimaryKey(t => t.InvSupplierID);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.RstDepartments",
                c => new
                    {
                        RstDepartmentID = c.Int(nullable: false, identity: true),
                        DepartmentCode = c.String(nullable: false, maxLength: 50),
                        DepartmentName = c.String(nullable: false, maxLength: 100),
                        Remark = c.String(),
                        IsActive = c.Boolean(nullable: false),
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
                .PrimaryKey(t => t.RstDepartmentID);
            
            CreateTable(
                "dbo.RstCategories",
                c => new
                    {
                        RstCategoryID = c.Int(nullable: false, identity: true),
                        RstDepartmentID = c.Int(nullable: false),
                        RstCategoryCode = c.String(nullable: false, maxLength: 50),
                        RstCategoryName = c.String(nullable: false, maxLength: 100),
                        Remark = c.String(),
                        IsActive = c.Boolean(nullable: false),
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
                .PrimaryKey(t => t.RstCategoryID);
            
            CreateTable(
                "dbo.RstSubCategories",
                c => new
                    {
                        RstSubCategoryID = c.Int(nullable: false, identity: true),
                        RstCategoryID = c.Int(nullable: false),
                        RstSubCategoryCode = c.String(nullable: false),
                        RstSubCategoryName = c.String(nullable: false),
                        Remark = c.String(),
                        IsActive = c.Boolean(nullable: false),
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
                .PrimaryKey(t => t.RstSubCategoryID);
            
            CreateTable(
                "dbo.RstKotCategories",
                c => new
                    {
                        RstKotCategoryID = c.Int(nullable: false, identity: true),
                        RstKotCategoryCode = c.String(nullable: false),
                        RstKotCategoryName = c.String(nullable: false, maxLength: 100),
                        IPAddress = c.String(),
                        PrinterName = c.String(),
                        COMName = c.String(),
                        IsActive = c.Boolean(nullable: false),
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
                .PrimaryKey(t => t.RstKotCategoryID);
            
            CreateTable(
                "dbo.RstPromotions",
                c => new
                    {
                        RstPromotionsID = c.Int(nullable: false, identity: true),
                        PromotionCode = c.String(nullable: false),
                        PromotionTypeID = c.Int(nullable: false),
                        Description = c.String(nullable: false, maxLength: 100),
                        FromDate = c.DateTime(nullable: false),
                        ToDate = c.DateTime(nullable: false),
                        IsActive = c.Boolean(nullable: false),
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
                .PrimaryKey(t => t.RstPromotionsID);
            
            CreateTable(
                "dbo.RstPromotionTypes",
                c => new
                    {
                        RstPromotionTypesID = c.Int(nullable: false, identity: true),
                        PromotionTypeCode = c.String(nullable: false),
                        Description = c.String(nullable: false, maxLength: 100),
                        IsActive = c.Boolean(nullable: false),
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
                .PrimaryKey(t => t.RstPromotionTypesID);
            
            CreateTable(
                "dbo.RstRoomMasters",
                c => new
                    {
                        RstRoomMasterID = c.Int(nullable: false, identity: true),
                        RoomMasterCode = c.String(nullable: false),
                        RoomName = c.String(nullable: false, maxLength: 100),
                        RoomType = c.String(),
                        Floor = c.Int(nullable: false),
                        RoomNo = c.String(),
                        InterComNo = c.String(),
                        RFIDNo = c.String(),
                        IsActive = c.Boolean(nullable: false),
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
                .PrimaryKey(t => t.RstRoomMasterID);
            
            CreateTable(
                "dbo.RstRoomTypes",
                c => new
                    {
                        RstRoomTypeID = c.Int(nullable: false, identity: true),
                        RoomTypeCode = c.String(nullable: false),
                        RoomTypeName = c.String(nullable: false, maxLength: 100),
                        BedType = c.String(),
                        MaxAdult = c.Int(nullable: false),
                        MaxChild = c.Int(nullable: false),
                        MaxInfant = c.Int(nullable: false),
                        IsAC = c.Boolean(nullable: false),
                        IsSmoking = c.Boolean(nullable: false),
                        IsMiniBar = c.Boolean(nullable: false),
                        IsNormalView = c.Boolean(nullable: false),
                        IsOceanView = c.Boolean(nullable: false),
                        IsLandside = c.Boolean(nullable: false),
                        IsBalcony = c.Boolean(nullable: false),
                        IsActive = c.Boolean(nullable: false),
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
                .PrimaryKey(t => t.RstRoomTypeID);
            
            CreateTable(
                "dbo.RstRoomTypeRates",
                c => new
                    {
                        RstRoomTypeRateID = c.Int(nullable: false, identity: true),
                        RoomTypeRateCode = c.String(nullable: false),
                        RoomTypeRateName = c.String(nullable: false, maxLength: 100),
                        Rate = c.Decimal(nullable: false, precision: 18, scale: 2),
                        FromDate = c.DateTime(nullable: false),
                        ToDate = c.DateTime(nullable: false),
                        ExtraAdultRate = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ExtraChildRate = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ForeignRate = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Package = c.String(),
                        IsActive = c.Boolean(nullable: false),
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
                .PrimaryKey(t => t.RstRoomTypeRateID);
            
            CreateTable(
                "dbo.StewardsMasters",
                c => new
                    {
                        StewardsMasterID = c.Int(nullable: false, identity: true),
                        StewardCode = c.String(nullable: false),
                        StewardTitle = c.String(nullable: false),
                        StewardName = c.String(nullable: false),
                        Address1 = c.String(),
                        Address2 = c.String(),
                        Address3 = c.String(),
                        DOB = c.DateTime(nullable: false),
                        NIC = c.String(),
                        Passport = c.String(),
                        Telephone = c.String(),
                        Mobile = c.String(),
                        Fax = c.String(),
                        Email = c.String(),
                        Target = c.String(),
                        Commission = c.Decimal(nullable: false, precision: 18, scale: 2),
                        IsDeliveryPerson = c.Boolean(nullable: false),
                        IsKarokeGirl = c.Boolean(nullable: false),
                        Picture = c.Binary(),
                        IsActive = c.Boolean(nullable: false),
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
                .PrimaryKey(t => t.StewardsMasterID);
            
            CreateTable(
                "dbo.SysCompanies",
                c => new
                    {
                        SysCompanyID = c.Int(nullable: false, identity: true),
                        CompanyCode = c.String(nullable: false),
                        CompanyName = c.String(nullable: false),
                        SysGroupOfCompanyId = c.Int(nullable: false),
                        OtherBusinessName1 = c.String(),
                        OtherBusinessName2 = c.String(),
                        OtherBusinessName3 = c.String(),
                        Address1 = c.String(),
                        Address2 = c.String(),
                        Address3 = c.String(),
                        Telephone = c.String(),
                        Mobile = c.String(),
                        Fax = c.String(),
                        ContactPerson = c.String(),
                        Website = c.String(),
                        TaxID1 = c.String(),
                        TaxID2 = c.String(),
                        TaxID3 = c.String(),
                        TaxRegistrationNo1 = c.String(),
                        TaxRegistrationNo2 = c.String(),
                        TaxRegistrationNo3 = c.String(),
                        IsVat = c.Boolean(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsDelete = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.SysCompanyID);
            
            CreateTable(
                "dbo.SysGroupOfCompanies",
                c => new
                    {
                        SysGroupOfCompanyId = c.Int(nullable: false, identity: true),
                        GroupOfCompanyCode = c.String(nullable: false),
                        GroupOfCompanyName = c.String(nullable: false),
                        CompanyGmail = c.String(),
                        CompanyVatNumber = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        IsDelete = c.Boolean(nullable: false),
                        CompanyLogo = c.Binary(),
                    })
                .PrimaryKey(t => t.SysGroupOfCompanyId);
            
            CreateTable(
                "dbo.SysLocations",
                c => new
                    {
                        SysLocationID = c.Int(nullable: false, identity: true),
                        LocationCode = c.String(nullable: false),
                        LocationName = c.String(nullable: false),
                        Address1 = c.String(),
                        Address2 = c.String(),
                        Address3 = c.String(),
                        Telephone = c.String(),
                        Fax = c.String(),
                        Email = c.String(),
                        ContactPersonName = c.String(),
                        OtherBusinessName = c.String(),
                        LocationPrefixCode = c.String(),
                        IsVAT = c.Boolean(nullable: false),
                        IsStockLocation = c.Boolean(nullable: false),
                        IsHeadOffice = c.Boolean(nullable: false),
                        LocationIP = c.Boolean(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsDelete = c.Boolean(nullable: false),
                        GroupOfCompanyID = c.Int(nullable: false),
                        CompanyID = c.Int(nullable: false),
                        CreatedUser = c.String(maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedUser = c.String(maxLength: 50),
                        ModifiedDate = c.DateTime(nullable: false),
                        DataTransfer = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.SysLocationID);
            
            CreateTable(
                "dbo.SysUserFunctions",
                c => new
                    {
                        SysUserFunctionID = c.Int(nullable: false, identity: true),
                        FunctionName = c.String(nullable: false, maxLength: 15),
                        FunctionDescription = c.String(nullable: false, maxLength: 100),
                        Order = c.Int(nullable: false),
                        TypeID = c.Int(nullable: false),
                        IsDelete = c.Boolean(nullable: false),
                        IsValue = c.Boolean(nullable: false),
                        GroupOfCompanyID = c.Int(nullable: false),
                        CompanyID = c.Int(nullable: false),
                        LocationId = c.Int(nullable: false),
                        CreatedUser = c.String(maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedUser = c.String(maxLength: 50),
                        ModifiedDate = c.DateTime(nullable: false),
                        DataTransfer = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.SysUserFunctionID);
            
            CreateTable(
                "dbo.SysUserGroupPermissions",
                c => new
                    {
                        SysUserGroupPermissionID = c.Int(nullable: false, identity: true),
                        FunctionName = c.String(nullable: false, maxLength: 100),
                        FunctionDescription = c.String(nullable: false, maxLength: 250),
                        Order = c.Int(nullable: false),
                        Value = c.Decimal(nullable: false, precision: 18, scale: 2),
                        MaxValue = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Type = c.String(),
                        TypeID = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsAccess = c.Boolean(nullable: false),
                        Remarks = c.String(maxLength: 500),
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
                .PrimaryKey(t => t.SysUserGroupPermissionID);
            
            CreateTable(
                "dbo.SysUserGroups",
                c => new
                    {
                        SysUserGroupID = c.Int(nullable: false, identity: true),
                        UserGroupName = c.String(nullable: false, maxLength: 50),
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
                .PrimaryKey(t => t.SysUserGroupID);
            
            CreateTable(
                "dbo.SysUserMasters",
                c => new
                    {
                        SysUserMasterID = c.Int(nullable: false, identity: true),
                        UserName = c.String(nullable: false, maxLength: 15),
                        UserDescription = c.String(nullable: false, maxLength: 100),
                        Password = c.String(nullable: false, maxLength: 100),
                        ConfirmPassword = c.String(),
                        UserGroupID = c.Long(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsUserCantChangePassword = c.Boolean(nullable: false),
                        IsUserMustChangePassword = c.Boolean(nullable: false),
                        IsDelete = c.Boolean(nullable: false),
                        EmployeeCode = c.String(maxLength: 15),
                        GroupOfCompanyID = c.Int(nullable: false),
                        CompanyID = c.Int(nullable: false),
                        LocationId = c.Int(nullable: false),
                        CreatedUser = c.String(maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedUser = c.String(maxLength: 50),
                        ModifiedDate = c.DateTime(nullable: false),
                        DataTransfer = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.SysUserMasterID);
            
            CreateTable(
                "dbo.SysUserPermissions",
                c => new
                    {
                        SysUserPermissionID = c.Int(nullable: false, identity: true),
                        EmployeeID = c.Int(nullable: false),
                        EmployeeCode = c.String(nullable: false),
                        EnCode = c.String(maxLength: 50),
                        FunctionName = c.String(nullable: false, maxLength: 100),
                        FunctionDescription = c.String(nullable: false, maxLength: 250),
                        Order = c.Int(nullable: false),
                        Value = c.Decimal(nullable: false, precision: 18, scale: 2),
                        MaxValue = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Type = c.String(),
                        TypeID = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsAccess = c.Boolean(nullable: false),
                        Remarks = c.String(maxLength: 500),
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
                .PrimaryKey(t => t.SysUserPermissionID);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.SysUserPermissions");
            DropTable("dbo.SysUserMasters");
            DropTable("dbo.SysUserGroups");
            DropTable("dbo.SysUserGroupPermissions");
            DropTable("dbo.SysUserFunctions");
            DropTable("dbo.SysLocations");
            DropTable("dbo.SysGroupOfCompanies");
            DropTable("dbo.SysCompanies");
            DropTable("dbo.StewardsMasters");
            DropTable("dbo.RstRoomTypeRates");
            DropTable("dbo.RstRoomTypes");
            DropTable("dbo.RstRoomMasters");
            DropTable("dbo.RstPromotionTypes");
            DropTable("dbo.RstPromotions");
            DropTable("dbo.RstKotCategories");
            DropTable("dbo.RstSubCategories");
            DropTable("dbo.RstCategories");
            DropTable("dbo.RstDepartments");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.InvSuppliers");
            DropTable("dbo.InvProductMasters");
            DropTable("dbo.CustomoerPreviousVisits");
            DropTable("dbo.Customers");
            DropTable("dbo.CustomerCategories");
        }
    }
}
