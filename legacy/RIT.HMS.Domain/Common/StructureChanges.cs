using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace RIT.HMS.Domain.Common
{
    public class StructureChanges
    {
        string tableName = "";
        string query = "";
        string spName = "";
        string ViewName = "";
        string ColumnName = "";
        public bool status = true;
        string Stringsqlconnection = "";
        public StructureChanges()
        {
            //string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            //Stringsqlconnection = cn;
        }

        private string CheckTable(string table, string qry)
        {
            var sqlQuery = string.Format(@"IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE  TABLE_NAME = '{0}')) 
                                                BEGIN
                                                 {1}                               
                                                END", tableName, query);
            return sqlQuery;
        }
        private string CheckSP(string sp, string qry)
        {
            var SPQuery = string.Format(@"IF EXISTS(SELECT 1 FROM sys.procedures  WHERE Name = '{0}')
                                        BEGIN
                                            DROP PROCEDURE {1}
                                        END
                                          BEGIN
                                            DECLARE @sql NVARCHAR(Max)
        	                                SET @sql = ' {2} '                                            
                                            EXEC (@sql)
                                            END", sp, sp, qry);
            return SPQuery;
        }
        private string ReplaceQuotationmark(string strQ)
        {
            return strQ.Replace("'", "''");
        }

        public void mainQueries(string connectionString)
        {
            Stringsqlconnection = connectionString;
            try
            {
                #region Table
                #region Currencies
                tableName = "Currencies";
                query = @"CREATE TABLE [dbo].[Currencies](
	                        [CurrencyID] [int] IDENTITY(1,1) NOT NULL,
	                        [CurrencyCode] [nvarchar](5) NOT NULL,
	                        [CurrencyDescription] [nvarchar](50) NOT NULL,
	                        [CurrencyFormat] [nvarchar](15) NOT NULL,
	                        [CurrencySymbol] [nvarchar](5) NOT NULL,
	                        [BuyingRate] [decimal](18, 2) NOT NULL,
	                        [SellingRate] [decimal](18, 2) NOT NULL,
	                        [AsofDate] [datetime] NOT NULL,
	                        [IsActive] [bit] NOT NULL,
	                        [IsDelete] [bit] NOT NULL,
	                        [GroupOfCompanyID] [int] NOT NULL,
	                        [CompanyID] [int] NOT NULL,
	                        [LocationId] [int] NOT NULL,
	                        [CreatedUser] [nvarchar](50) NULL,
	                        [CreatedDate] [datetime] NOT NULL,
	                        [ModifiedUser] [nvarchar](50) NULL,
	                        [ModifiedDate] [datetime] NOT NULL,
	                        [DataTransfer] [int] NOT NULL,
                         CONSTRAINT [PK_dbo.Currencies] PRIMARY KEY CLUSTERED 
                        (
	                        [CurrencyID] ASC
                        )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                        ) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);
                #endregion Currencies

                #region AutoGenerateInfo
                tableName = "AutoGenerateInfo";
                query = @"CREATE TABLE [dbo].[AutoGenerateInfo](
	                        [AutoGenerateInfoID] [bigint] IDENTITY(1,1) NOT NULL,
	                        [ModuleType] [int] NOT NULL,
	                        [DocumentID] [int] NOT NULL,
	                        [FormId] [int] NOT NULL,
	                        [FormName] [nvarchar](100) NULL,
	                        [FormText] [nvarchar](100) NOT NULL,
	                        [Prefix] [nvarchar](5) NULL,
	                        [Prefix2] [nvarchar](3) NULL,
	                        [CodeLength] [int] NOT NULL,
	                        [Suffix] [int] NOT NULL,
	                        [AutoGenerete] [bit] NOT NULL,
	                        [AutoClear] [bit] NOT NULL,
	                        [IsDepend] [bit] NOT NULL,
	                        [IsDependCode] [bit] NOT NULL,
	                        [IsSupplierProduct] [bit] NOT NULL,
	                        [IsOverWriteQty] [bit] NOT NULL,
	                        [IsLocationCode] [bit] NOT NULL,
	                        [ReportPrefix] [nvarchar](3) NULL,
	                        [ReportType] [int] NOT NULL,
	                        [PoIsMandatory] [bit] NOT NULL,
	                        [IsDispatchRecall] [bit] NOT NULL,
	                        [IsBackDated] [bit] NOT NULL,
	                        [IsCard] [bit] NOT NULL,
	                        [CardId] [int] NOT NULL,
	                        [IsEntry] [bit] NOT NULL,
	                        [IsSlabReport] [bit] NOT NULL,
	                        [IsConsignment] [bit] NOT NULL,
	                        [IsRoundOff] [bit] NOT NULL,
	                        [IsAutoComplete] [bit] NOT NULL,
	                        [IsUpdateProductImage] [bit] NOT NULL,
	                        [IsAllowedInHO] [bit] NOT NULL,
	                        [IsAllowedInOutlet] [bit] NOT NULL,
	                        [IsActive] [bit] NOT NULL,
	                        [Layout] [nvarchar](max) NULL,
	                        [ReferenceDocumentID] [int] NOT NULL,
	                        [LayoutNew] [nvarchar](max) NULL,
	                        [MenuName] [nvarchar](200) NULL,
                         CONSTRAINT [PK_dbo.AutoGenerateInfo] PRIMARY KEY CLUSTERED 
                        (
	                        [AutoGenerateInfoID] ASC
                        )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                        ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion AutoGenerateInfo

                #region AddonCategoryMasters
                tableName = "AddonCategoryMasters";
                query = @"CREATE TABLE [dbo].[AddonCategoryMasters](
	                    [AddonCategoryMasterId] [bigint] IDENTITY(1,1) NOT NULL,
	                    [AddonCatCode] [nvarchar](max) NULL,
	                    [IsActive] [bit] NOT NULL,
	                    [AddonCatName] [nvarchar](max) NULL,
	                    [MaxAddons] [int] NOT NULL,
	                    [IsDelete] [bit] NOT NULL,
	                    [GroupOfCompanyID] [int] NOT NULL,
	                    [CompanyID] [int] NOT NULL,
	                    [LocationId] [int] NOT NULL,
	                    [CreatedUser] [nvarchar](50) NULL,
	                    [CreatedDate] [datetime] NOT NULL,
	                    [ModifiedUser] [nvarchar](50) NULL,
	                    [ModifiedDate] [datetime] NOT NULL,
	                    [DataTransfer] [int] NOT NULL,
                     CONSTRAINT [PK_dbo.AddonCategoryMasters] PRIMARY KEY CLUSTERED 
                    (
	                    [AddonCategoryMasterId] ASC
                    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);
                #endregion AddonCategoryMasters

                #region Addons
                tableName = "Addons";
                query = @"CREATE TABLE [dbo].[Addons](
	                        [AddonsId] [int] IDENTITY(1,1) NOT NULL,
	                        [ProductId] [bigint] NOT NULL,
	                        [ProductAddonId] [bigint] NOT NULL,
	                        [DepartmentId] [bigint] NOT NULL,
	                        [IsActive] [bit] NOT NULL,
	                        [GroupOfCompanyID] [int] NOT NULL,
	                        [CompanyID] [int] NOT NULL,
	                        [LocationId] [int] NOT NULL,
	                        [CreatedUser] [nvarchar](50) NULL,
	                        [CreatedDate] [datetime] NOT NULL,
	                        [ModifiedUser] [nvarchar](50) NULL,
	                        [ModifiedDate] [datetime] NOT NULL,
	                        [DataTransfer] [int] NOT NULL,
	                        [AddonSellingPrice] [decimal](18, 3) NOT NULL,
	                        [AddonQuantity] [decimal](18, 3) NOT NULL,
	                        [IsShowOnBill] [bit] NOT NULL,
                         CONSTRAINT [PK_dbo.Addons] PRIMARY KEY CLUSTERED 
                        (
	                        [AddonsId] ASC
                        )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                        ) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);
                #endregion Addons

                #region BankBins
                tableName = "BankBins";
                query = @"CREATE TABLE [dbo].[BankBins](
	                        [BankBinId] [int] IDENTITY(1,1) NOT NULL,
	                        [CardPfx] [nchar](100) NULL,
	                        [CardName] [nchar](250) NULL,
	                        [CardType] [nchar](250) NULL,
	                        [CardID] [int] NOT NULL,
	                        [BankID] [int] NOT NULL,
	                        [BankName] [nvarchar](max) NULL,
	                        [Rate] [decimal](18, 2) NOT NULL,
	                        [DiscountPercentage] [decimal](18, 2) NOT NULL,
	                        [DateFrom] [datetime] NOT NULL,
	                        [DateTo] [datetime] NOT NULL,
	                        [StartTime] [time](7) NOT NULL,
	                        [EndTime] [time](7) NOT NULL,
	                        [ValueFrom] [decimal](18, 2) NOT NULL,
	                        [ValueTo] [decimal](18, 2) NOT NULL,
	                        [DiscountAmount] [decimal](18, 2) NOT NULL,
	                        [LocationId] [int] NOT NULL,
	                        [IsValidForGVSales] [bit] NOT NULL,
	                        [IsCombined] [bit] NOT NULL,
	                        [PromotionID] [int] NOT NULL,
	                        [CompanyId] [int] NOT NULL,
                         CONSTRAINT [PK_dbo.BankBins] PRIMARY KEY CLUSTERED 
                        (
	                        [BankBinId] ASC
                        )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                        ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);
                #endregion BankBins

                #region Banks
                tableName = "Banks";
                query = @"CREATE TABLE [dbo].[Banks](
	[BankId] [int] IDENTITY(1,1) NOT NULL,
	[BankName] [varchar](100) NULL,
	[ISActive] [bit] NOT NULL,
 CONSTRAINT [PK_dbo.Banks] PRIMARY KEY CLUSTERED 
(
	[BankId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);
                #endregion Banks

                #region CardGenerationLocationSettings
                tableName = "CardGenerationLocationSettings";
                query = @"CREATE TABLE [dbo].[CardGenerationLocationSettings](
	[CardGenerationLocationSettingId] [bigint] IDENTITY(1,1) NOT NULL,
	[CardNoLength] [int] NOT NULL,
	[CardStartingNo] [bigint] NOT NULL,
	[EncodeStartingNo] [bigint] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 CONSTRAINT [PK_dbo.CardGenerationLocationSettings] PRIMARY KEY CLUSTERED 
(
	[CardGenerationLocationSettingId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);
                #endregion CardGenerationLocationSettings

                #region CardMasters
                tableName = "CardMasters";
                query = @"CREATE TABLE [dbo].[CardMasters](
	[CardMasterId] [bigint] IDENTITY(1,1) NOT NULL,
	[CardType] [int] NOT NULL,
	[CardCode] [nvarchar](15) NOT NULL,
	[CardName] [nvarchar](50) NOT NULL,
	[Discount] [decimal](18, 2) NOT NULL,
	[PointValue] [decimal](18, 2) NOT NULL,
	[MinimumPoints] [decimal](18, 2) NOT NULL,
	[ReDeemPointValue] [decimal](18, 2) NOT NULL,
	[Remark] [nvarchar](150) NULL,
	[IsDelete] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 CONSTRAINT [PK_dbo.CardMasters] PRIMARY KEY CLUSTERED 
(
	[CardMasterId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion CardMasters

                #region CardTypes
                tableName = "CardTypes";
                query = @"CREATE TABLE [dbo].[CardTypes](
	[CardTypeId] [int] IDENTITY(1,1) NOT NULL,
	[CardTypeName] [varchar](50) NULL,
	[IsActive] [bit] NOT NULL,
 CONSTRAINT [PK_dbo.CardTypes] PRIMARY KEY CLUSTERED 
(
	[CardTypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion CardTypes

                #region CashierFunctions
                tableName = "CashierFunctions";
                query = @"CREATE TABLE [dbo].[CashierFunctions](
	[CashierFunctionId] [bigint] IDENTITY(1,1) NOT NULL,
	[FunctionName] [nvarchar](max) NOT NULL,
	[FunctionDescription] [nvarchar](max) NOT NULL,
	[Order] [bigint] NOT NULL,
	[TypeID] [int] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[IsValue] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CreatedUser] [nvarchar](max) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](max) NOT NULL,
	[ModifiedDate] [datetime] NOT NULL,
 CONSTRAINT [PK_dbo.CashierFunctions] PRIMARY KEY CLUSTERED 
(
	[CashierFunctionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion CashierFunctions

                #region CashierGroups
                tableName = "CashierGroups";
                query = @"CREATE TABLE [dbo].[CashierGroups](
	[CashierGroupId] [int] IDENTITY(1,1) NOT NULL,
	[EmployeeGroupId] [int] NOT NULL,
	[FunctionName] [nvarchar](max) NOT NULL,
	[FunctionDescription] [nvarchar](max) NOT NULL,
	[Order] [int] NOT NULL,
	[Value] [decimal](18, 2) NOT NULL,
	[Type] [nvarchar](max) NULL,
	[TypeId] [int] NOT NULL,
	[IsAccess] [bit] NOT NULL,
	[IsValue] [bit] NOT NULL,
	[GroupOfCompanyId] [int] NOT NULL,
	[CreatedUser] [nvarchar](max) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](max) NOT NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 CONSTRAINT [PK_dbo.CashierGroups] PRIMARY KEY CLUSTERED 
(
	[CashierGroupId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion CashierGroups

                #region CashierPermissions
                tableName = "CashierPermissions";
                query = @"CREATE TABLE [dbo].[CashierPermissions](
	[CashierPermissionId] [bigint] IDENTITY(1,1) NOT NULL,
	[LocationId] [bigint] NOT NULL,
	[CashierId] [bigint] NOT NULL,
	[EmployeeId] [bigint] NOT NULL,
	[Password] [nvarchar](max) NOT NULL,
	[JournalName] [nvarchar](max) NOT NULL,
	[EnCode] [nvarchar](max) NOT NULL,
	[FunctionName] [nvarchar](max) NOT NULL,
	[FunctionDescription] [nvarchar](max) NOT NULL,
	[Order] [bigint] NOT NULL,
	[Value] [bigint] NOT NULL,
	[MaxValue] [decimal](18, 2) NOT NULL,
	[Type] [nvarchar](max) NOT NULL,
	[TypeID] [nvarchar](max) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[IsAccess] [bit] NOT NULL,
	[Remarks] [nvarchar](max) NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[IsValue] [bit] NOT NULL,
	[GroupOfCompanyId] [int] NOT NULL,
	[CreatedUser] [nvarchar](max) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](max) NOT NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 CONSTRAINT [PK_dbo.CashierPermissions] PRIMARY KEY CLUSTERED 
(
	[CashierPermissionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion CashierPermissions

                #region Categoty$
                tableName = "Categoty$";
                query = @"CREATE TABLE [dbo].[Categoty$](
	[Department ] [nvarchar](255) NULL,
	[Category Code] [nvarchar](255) NULL,
	[Category Name] [nvarchar](255) NULL,
	[Is Active] [nvarchar](255) NULL
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion Categoty$

                #region CateringModeTaxes
                tableName = "CateringModeTaxes";
                query = @"CREATE TABLE [dbo].[CateringModeTaxes](
	[CateringModeTaxId] [bigint] IDENTITY(1,1) NOT NULL,
	[CateringModeId] [bigint] NOT NULL,
	[TaxId] [bigint] NOT NULL,
	[TaxPracentage] [decimal](18, 2) NOT NULL,
	[TaxSequence] [int] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 CONSTRAINT [PK_dbo.CateringModeTaxes] PRIMARY KEY CLUSTERED 
(
	[CateringModeTaxId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion CateringModeTaxes

                #region ChairMasters
                tableName = "ChairMasters";
                query = @"CREATE TABLE [dbo].[ChairMasters](
	[ChairMasterID] [int] IDENTITY(1,1) NOT NULL,
	[ChairCode] [nvarchar](10) NOT NULL,
	[TableID] [int] NOT NULL,
	[ChairName] [nvarchar](max) NULL,
	[TicketID] [int] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 CONSTRAINT [PK_dbo.ChairMasters] PRIMARY KEY CLUSTERED 
(
	[ChairMasterID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion ChairMasters

                #region CompanyUsers
                tableName = "CompanyUsers";
                query = @"CREATE TABLE [dbo].[CompanyUsers](
	[CompanyUserId] [int] IDENTITY(1,1) NOT NULL,
	[CompanyUserName] [varchar](50) NULL,
	[CompanyUserPassword] [varchar](50) NULL,
	[CompanyDbName] [varchar](50) NULL,
	[CompanyId] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreateUser] [varchar](50) NULL,
	[CreateDate] [datetime] NOT NULL,
	[ModifiedUser] [varchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
 CONSTRAINT [PK_dbo.CompanyUsers] PRIMARY KEY CLUSTERED 
(
	[CompanyUserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion CompanyUsers

                #region Configurations
                tableName = "Configurations";
                query = @"CREATE TABLE [dbo].[Configurations](
	[ConfigurationId] [int] IDENTITY(1,1) NOT NULL,
	[ConfigurationKey] [nvarchar](10) NULL,
	[ConfigurationDescription] [nvarchar](50) NULL,
	[EffectLocationId] [int] NOT NULL,
	[ConfigurationOn] [bit] NOT NULL,
	[ConfigurationActive] [bit] NOT NULL,
	[ConfigurationDelete] [bit] NOT NULL,
	[CreateDate] [datetime] NULL,
	[CreateUserId] [int] NOT NULL,
	[CompanyId] [int] NOT NULL,
 CONSTRAINT [PK_dbo.Configurations] PRIMARY KEY CLUSTERED 
(
	[ConfigurationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion Configurations

                #region Currencies
                tableName = "Currencies";
                query = @"CREATE TABLE [dbo].[Currencies](
	[CurrencyID] [int] IDENTITY(1,1) NOT NULL,
	[CurrencyCode] [nvarchar](5) NOT NULL,
	[CurrencyDescription] [nvarchar](50) NOT NULL,
	[CurrencyFormat] [nvarchar](15) NOT NULL,
	[CurrencySymbol] [nvarchar](5) NOT NULL,
	[BuyingRate] [decimal](18, 2) NOT NULL,
	[SellingRate] [decimal](18, 2) NOT NULL,
	[AsofDate] [datetime] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 CONSTRAINT [PK_dbo.Currencies] PRIMARY KEY CLUSTERED 
(
	[CurrencyID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion Currencies

                #region CurrencyHistories
                tableName = "CurrencyHistories";
                query = @"CREATE TABLE [dbo].[CurrencyHistories](
	[CurrencyHistoryID] [int] IDENTITY(1,1) NOT NULL,
	[CurrencyID] [int] NOT NULL,
	[BuyingRate] [decimal](18, 2) NOT NULL,
	[SellingRate] [decimal](18, 2) NOT NULL,
	[AsofDate] [datetime] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 CONSTRAINT [PK_dbo.CurrencyHistories] PRIMARY KEY CLUSTERED 
(
	[CurrencyHistoryID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion CurrencyHistories

                #region CustomerCategories
                tableName = "CustomerCategories";
                query = @"CREATE TABLE [dbo].[CustomerCategories](
	[CustomerCategoryID] [int] IDENTITY(1,1) NOT NULL,
	[CustomerCategoryCode] [nvarchar](max) NOT NULL,
	[CustomerCategoryName] [nvarchar](max) NOT NULL,
	[Remark] [nvarchar](max) NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[IsVat] [bit] NOT NULL,
	[DiscountPrc] [decimal](18, 2) NOT NULL,
 CONSTRAINT [PK_dbo.CustomerCategories] PRIMARY KEY CLUSTERED 
(
	[CustomerCategoryID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion CustomerCategories

                #region CustomerDiscounts
                tableName = "CustomerDiscounts";
                query = @"CREATE TABLE [dbo].[CustomerDiscounts](
	[CustomerDiscountId] [int] IDENTITY(1,1) NOT NULL,
	[CustomerId] [int] NOT NULL,
	[CustomerCode] [nvarchar](20) NULL,
	[ProductId] [int] NOT NULL,
	[ProductCode] [nvarchar](20) NULL,
	[DiscountAmount] [decimal](18, 2) NOT NULL,
	[DiscountPercentage] [decimal](18, 2) NOT NULL,
	[CustomerSellPrice] [decimal](18, 2) NOT NULL,
	[CreditDiscountAmount] [decimal](18, 2) NOT NULL,
	[CreditDiscountPercentage] [decimal](18, 2) NOT NULL,
	[DateFrom] [datetime] NOT NULL,
	[DateTo] [datetime] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[ServingUnitId] [int] NOT NULL,
 CONSTRAINT [PK_dbo.CustomerDiscounts] PRIMARY KEY CLUSTERED 
(
	[CustomerDiscountId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion CustomerDiscounts

                #region Customers
                tableName = "Customers";
                query = @"CREATE TABLE [dbo].[Customers](
	[CustomerID] [int] IDENTITY(1,1) NOT NULL,
	[CustomerCode] [nvarchar](max) NOT NULL,
	[CustomerTitle] [nvarchar](max) NOT NULL,
	[CustomerName] [nvarchar](100) NOT NULL,
	[CustomerType] [nvarchar](max) NULL,
	[CustomerCategoryId] [int] NOT NULL,
	[BillingAddress1] [nvarchar](100) NOT NULL,
	[BillingAddress2] [nvarchar](100) NOT NULL,
	[BillingAddress3] [nvarchar](max) NULL,
	[DOB] [datetime] NULL,
	[NIC] [nvarchar](12) NOT NULL,
	[Passport] [nvarchar](max) NULL,
	[Telephone] [nvarchar](max) NULL,
	[Mobile] [nvarchar](max) NOT NULL,
	[Fax] [nvarchar](max) NULL,
	[Email] [nvarchar](max) NULL,
	[VehicleNo] [nvarchar](max) NULL,
	[Profession] [nvarchar](max) NULL,
	[WeddingAnniversary] [datetime] NULL,
	[IsActiveForLoyalty] [bit] NOT NULL,
	[CustomerPicture] [varbinary](max) NULL,
	[CustomerPictureName] [nvarchar](max) NULL,
	[CustomerPictureType] [nvarchar](max) NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[CreditLimit] [decimal](18, 2) NOT NULL,
	[Outstanding] [decimal](18, 2) NOT NULL,
	[EPFNo] [varchar](50) NULL,
	[MembershipCardNo] [varchar](50) NULL,
	[Other] [varchar](50) NULL,
	[Remarks] [varchar](200) NULL,
	[CustomerStatus] [varchar](20) NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[Gender] [int] NOT NULL,
	[ReferenceNo1] [nvarchar](50) NULL,
	[ReferenceNo2] [nvarchar](50) NULL,
	[Age] [int] NOT NULL,
	[Religion] [int] NULL,
	[Race] [int] NULL,
	[LandMark] [nvarchar](50) NULL,
	[District] [nvarchar](50) NULL,
	[Organization] [nvarchar](50) NULL,
	[WorkAddres1] [nvarchar](50) NULL,
	[WorkAddres2] [nvarchar](50) NULL,
	[WorkAddres3] [nvarchar](50) NULL,
	[WorkEmail] [nvarchar](50) NULL,
	[WorkTelephone] [nvarchar](50) NULL,
	[WorkMobile] [nvarchar](50) NULL,
	[WorkFax] [nvarchar](50) NULL,
	[SpouseName] [nvarchar](50) NULL,
	[CivilStatus] [int] NOT NULL,
	[SpouseDateOfBirth] [datetime] NULL,
	[DeliverTo] [int] NOT NULL,
	[DeliverToAddress] [nvarchar](50) NULL,
	[Country] [nvarchar](50) NULL,
	[CustomerSince] [datetime] NULL,
	[SpecialDayType] [int] NOT NULL,
	[SendUpdatesViaEmail] [bit] NOT NULL,
	[SendUpdatesViaSms] [bit] NOT NULL,
	[IsRegByPOS] [bit] NOT NULL,
 CONSTRAINT [PK_dbo.Customers] PRIMARY KEY CLUSTERED 
(
	[CustomerID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion Customers

                #region Tour Agents
                tableName = "TourAgents";
                query = @"CREATE TABLE [dbo].[TourAgents](
	            [TourAgentID] [int] IDENTITY(1,1) NOT NULL,
	            [AgentCode] [nvarchar](15) NOT NULL,
	            [TourAgentTitle] [nvarchar](max) NOT NULL,
	            [TourAgentName] [nvarchar](100) NOT NULL,
	            [BillingAddress1] [nvarchar](100) NOT NULL,
	            [BillingAddress2] [nvarchar](100) NOT NULL,
	            [BillingAddress3] [nvarchar](max) NULL,
	            [NIC] [nvarchar](12) NOT NULL,
	            [Mobile] [nvarchar](max) NOT NULL,
	            [Email] [nvarchar](max) NULL,
	            [Remarks] [varchar](200) NULL,
	            [TourAgentCompanyID] [int] NOT NULL,
	            [TourAmount] [decimal](18, 2) NOT NULL,
	            [TourPercentage] [decimal](18, 2) NOT NULL,
	            [IsTourAgent] [bit] NOT NULL,
	            [IsActive] [bit] NOT NULL,
	            [TCompanyID] [int] NOT NULL,
	            [GroupOfCompanyID] [int] NOT NULL,
	            [CompanyID] [int] NOT NULL,
	            [LocationId] [int] NOT NULL,
	            [CreatedUser] [nvarchar](50) NULL,
	            [CreatedDate] [datetime] NOT NULL,
	            [ModifiedUser] [nvarchar](50) NULL,
	            [ModifiedDate] [datetime] NOT NULL,
	            [DataTransfer] [int] NOT NULL,
	            [TourAgentCompanyCode] [nvarchar](15) NULL,
             CONSTRAINT [PK_dbo.TourAgent] PRIMARY KEY CLUSTERED 
            (
	            [TourAgentID] ASC
            )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
            ) ON [PRIMARY] 
            SET ANSI_PADDING OFF
            ALTER TABLE [dbo].[TourAgents] ADD  CONSTRAINT [DF__TourAgent__Agent__49CEE3AF]  DEFAULT ('') FOR [AgentCode]
            ALTER TABLE [dbo].[TourAgents] ADD  CONSTRAINT [DF__TourAgent__TourA__4AC307E8]  DEFAULT ('') FOR [TourAgentCompanyID]
            ALTER TABLE [dbo].[TourAgents] ADD  CONSTRAINT [DF__TourAgent__TourA__4BB72C21]  DEFAULT ((0)) FOR [TourAmount]
            ALTER TABLE [dbo].[TourAgents] ADD  CONSTRAINT [DF__TourAgent__TourP__4CAB505A]  DEFAULT ((0)) FOR [TourPercentage]
            ALTER TABLE [dbo].[TourAgents] ADD  CONSTRAINT [DF__TourAgent__IsTou__4D9F7493]  DEFAULT ((0)) FOR [IsTourAgent]
            ALTER TABLE [dbo].[TourAgents] ADD  CONSTRAINT [DF_TourAgents_IsActive]  DEFAULT ((0)) FOR [IsActive]
            ALTER TABLE [dbo].[TourAgents] ADD  CONSTRAINT [DF_TourAgents_CompanyID]  DEFAULT ((0)) FOR [TCompanyID]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion Tour Agents

                #region Tour Agents Company
                tableName = "TourAgentCompanies";
                query = @"CREATE TABLE [dbo].[TourAgentCompanies](
	            [TourAgentCompanyID] [int] IDENTITY(1,1) NOT NULL,
	            [TourAgentCompanyCode] [nvarchar](15) NOT NULL,
	            [TourAgentCompanyName] [nvarchar](50) NOT NULL,
	            [Address1] [nvarchar](50) NOT NULL,
	            [Address2] [nvarchar](50) NULL,
	            [Telephone] [nvarchar](max) NULL,
	            [Mobile] [nvarchar](max) NULL,
	            [FaxNo] [nvarchar](max) NULL,
	            [Email] [nvarchar](max) NULL,
	            [WebAddress] [nvarchar](max) NULL,
	            [ContactPerson] [nvarchar](max) NULL,
	            [IsDelete] [bit] NOT NULL,
	            [CommissionAmount] [decimal](18, 2) NOT NULL,
	            [GroupOfCompanyID] [int] NOT NULL,
	            [CompanyID] [int] NOT NULL,
	            [CreatedUser] [nvarchar](50) NULL,
	            [CreatedDate] [datetime] NOT NULL,
	            [ModifiedUser] [nvarchar](50) NULL,
	            [ModifiedDate] [datetime] NOT NULL,
	            [DataTransfer] [int] NOT NULL,
	            [LocationId] [int] NOT NULL,
	            [IsActive] [bit] NOT NULL,
             CONSTRAINT [PK_dbo.TourAgentCompany] PRIMARY KEY CLUSTERED 
            (
	            [TourAgentCompanyID] ASC
            )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
            ) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion Tour Agents Company


                #region CustomoerPreviousVisits
                tableName = "CustomoerPreviousVisits";
                query = @"CREATE TABLE [dbo].[CustomoerPreviousVisits](
	[CustomoerPreviousVisitsID] [int] IDENTITY(1,1) NOT NULL,
	[CustomerID] [int] NOT NULL,
	[CustomoerPreviousVisitsCode] [nvarchar](max) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[FromDate] [datetime] NOT NULL,
	[ToDate] [datetime] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 CONSTRAINT [PK_dbo.CustomoerPreviousVisits] PRIMARY KEY CLUSTERED 
(
	[CustomoerPreviousVisitsID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion CustomoerPreviousVisits

                #region CustomProductionNoteDetails
                tableName = "CustomProductionNoteDetails";
                query = @"CREATE TABLE [dbo].[CustomProductionNoteDetails](
	[CustomProductionNoteDetailId] [bigint] IDENTITY(1,1) NOT NULL,
	[CustomProductionNoteHeaderId] [bigint] NOT NULL,
	[ProductId] [bigint] NOT NULL,
	[ProductName] [nvarchar](max) NULL,
	[ProductQty] [decimal](18, 2) NOT NULL,
	[ProductCostPrice] [decimal](18, 2) NOT NULL,
	[MaterialId] [bigint] NOT NULL,
	[MaterialName] [nvarchar](max) NULL,
	[MaterialQty] [decimal](18, 2) NOT NULL,
	[MaterialCostPrice] [decimal](18, 2) NOT NULL,
	[MaterialAvgCost] [decimal](18, 2) NOT NULL,
 CONSTRAINT [PK_dbo.CustomProductionNoteDetails] PRIMARY KEY CLUSTERED 
(
	[CustomProductionNoteDetailId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion CustomProductionNoteDetails

                #region CustomProductionNoteHeaders
                tableName = "CustomProductionNoteHeaders";
                query = @"CREATE TABLE [dbo].[CustomProductionNoteHeaders](
	[CustomProductionNoteHeaderId] [bigint] IDENTITY(1,1) NOT NULL,
	[DocumentNo] [nvarchar](15) NOT NULL,
	[ProcessLocId] [bigint] NOT NULL,
	[PickupLocId] [bigint] NOT NULL,
	[Remark] [nvarchar](200) NULL,
	[IsFinished] [bit] NOT NULL,
	[DocumentId] [int] NOT NULL,
	[ReceiptLocID] [int] NOT NULL,
	[R_Zno] [int] NOT NULL,
	[ReceiptNo] [nvarchar](40) NULL,
	[UnitNo] [int] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[ReciptId] [int] NOT NULL,
 CONSTRAINT [PK_dbo.CustomProductionNoteHeaders] PRIMARY KEY CLUSTERED 
(
	[CustomProductionNoteHeaderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion CustomProductionNoteHeaders

                #region DeliveryPersons
                tableName = "DeliveryPersons";
                query = @"CREATE TABLE [dbo].[DeliveryPersons](
	[DeliveryPersonId] [bigint] IDENTITY(1,1) NOT NULL,
	[EmployeeId] [nvarchar](15) NOT NULL,
	[Title] [nvarchar](max) NOT NULL,
	[FullName] [nvarchar](100) NOT NULL,
	[Address] [nvarchar](200) NOT NULL,
	[DOB] [datetime] NOT NULL,
	[Designation] [nvarchar](100) NULL,
	[Picture] [varbinary](max) NULL,
	[PictureName] [nvarchar](max) NULL,
	[PictureType] [nvarchar](max) NULL,
	[NIC] [nvarchar](12) NOT NULL,
	[DrivingLicence] [nvarchar](max) NULL,
	[Telephone] [nvarchar](max) NULL,
	[Mobile] [nvarchar](max) NOT NULL,
	[Email] [nvarchar](max) NULL,
	[InCaseOfEmergency] [nvarchar](200) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 CONSTRAINT [PK_dbo.DeliveryPersons] PRIMARY KEY CLUSTERED 
(
	[DeliveryPersonId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion DeliveryPersons

                #region Department$
                tableName = "Department$";
                query = @"CREATE TABLE [dbo].[Department$](
	[Department Code ] [nvarchar](255) NULL,
	[Department Name ] [nvarchar](255) NULL,
	[Is Active] [nvarchar](255) NULL
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion Department$

                #region DocStatus
                tableName = "DocStatus";
                query = @"CREATE TABLE [dbo].[DocStatus](
	[DocStatusId] [int] IDENTITY(1,1) NOT NULL,
	[DocType] [nvarchar](max) NULL,
	[StatusId] [int] NOT NULL,
	[Description] [nvarchar](max) NULL,
	[Order] [int] NOT NULL,
 CONSTRAINT [PK_dbo.DocStatus] PRIMARY KEY CLUSTERED 
(
	[DocStatusId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion DocStatus

                #region DocStatusChangeLogs
                tableName = "DocStatusChangeLogs";
                query = @"CREATE TABLE [dbo].[DocStatusChangeLogs](
	[DocStatusChangeLogId] [int] IDENTITY(1,1) NOT NULL,
	[Module] [nvarchar](max) NULL,
	[Status] [int] NOT NULL,
	[StatusAppliedBy] [nvarchar](20) NULL,
	[StatusAppliedOn] [datetime] NOT NULL,
 CONSTRAINT [PK_dbo.DocStatusChangeLogs] PRIMARY KEY CLUSTERED 
(
	[DocStatusChangeLogId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion DocStatusChangeLogs

                #region DocumentNumbers
                tableName = "DocumentNumbers";
                query = @"CREATE TABLE [dbo].[DocumentNumbers](
	[DocumentNumberId] [bigint] IDENTITY(1,1) NOT NULL,
	[DocumentId] [int] NOT NULL,
	[DocumentName] [nvarchar](max) NOT NULL,
	[DocumentNo] [bigint] NOT NULL,
	[TempDocumentNo] [bigint] NOT NULL,
	[TemplateDocumentNo] [nvarchar](max) NOT NULL,
	[DocumentYear] [int] NOT NULL,
	[PrefixCode] [nvarchar](max) NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 CONSTRAINT [PK_dbo.DocumentNumbers] PRIMARY KEY CLUSTERED 
(
	[DocumentNumberId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion DocumentNumbers

                #region EmployeeGroups
                tableName = "EmployeeGroups";
                query = @"CREATE TABLE [dbo].[EmployeeGroups](
	[EmployeeGroupID] [int] IDENTITY(1,1) NOT NULL,
	[EmployeeGroupName] [nvarchar](50) NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[EmployeeGroupCode] [nvarchar](15) NOT NULL,
	[IsSteward] [bit] NOT NULL,
 CONSTRAINT [PK_dbo.EmployeeGroups] PRIMARY KEY CLUSTERED 
(
	[EmployeeGroupID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion EmployeeGroups

                #region Employees
                tableName = "Employees";
                query = @"CREATE TABLE [dbo].[Employees](
	[EmployeeID] [bigint] IDENTITY(1,1) NOT NULL,
	[EmployeeCode] [nvarchar](15) NOT NULL,
	[EmployeeTitle] [nvarchar](max) NOT NULL,
	[EmployeeName] [nvarchar](100) NOT NULL,
	[Designation] [nvarchar](100) NULL,
	[Gender] [nvarchar](max) NOT NULL,
	[DOB] [datetime] NOT NULL,
	[NIC] [nvarchar](12) NOT NULL,
	[Passport] [nvarchar](max) NULL,
	[Address1] [nvarchar](100) NOT NULL,
	[Address2] [nvarchar](100) NOT NULL,
	[Address3] [nvarchar](100) NULL,
	[Email] [nvarchar](max) NULL,
	[Telephone] [nvarchar](max) NULL,
	[Mobile] [nvarchar](max) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[DepartmentID] [int] NOT NULL,
	[EmployeePicture] [varbinary](max) NULL,
	[EmployeePictureName] [nvarchar](max) NULL,
	[EmployeePictureType] [nvarchar](max) NULL,
	[EmployeeGroupID] [int] NOT NULL,
	[EpfNo] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_dbo.Employees] PRIMARY KEY CLUSTERED 
(
	[EmployeeID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion Employees

                #region EventProducts
                tableName = "EventProducts";
                query = @"CREATE TABLE [dbo].[EventProducts](
	[EventProductId] [int] IDENTITY(1,1) NOT NULL,
	[EventId] [int] NOT NULL,
	[EventName] [nvarchar](max) NULL,
	[ProductId] [int] NOT NULL,
	[ProductName] [nvarchar](100) NULL,
	[IsActive] [bit] NOT NULL,
	[OrdSeq] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
 CONSTRAINT [PK_dbo.EventProducts] PRIMARY KEY CLUSTERED 
(
	[EventProductId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion EventProducts

                #region Events
                tableName = "Events";
                query = @"CREATE TABLE [dbo].[Events](
	[EventId] [int] IDENTITY(1,1) NOT NULL,
	[EventCode] [nvarchar](max) NOT NULL,
	[EventName] [nvarchar](max) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[FromTime] [time](7) NOT NULL,
	[ToTime] [time](7) NOT NULL,
	[IsPOS] [bit] NOT NULL,
 CONSTRAINT [PK_dbo.Events] PRIMARY KEY CLUSTERED 
(
	[EventId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion Events

                #region ImportJournalDetails
                tableName = "ImportJournalDetails";
                query = @"CREATE TABLE [dbo].[ImportJournalDetails](
	                [REFINDEX] [decimal](18, 0) IDENTITY(1,1) NOT NULL,
	                [EXBATCH] [varchar](15) NOT NULL,
	                [TRANTYPE] [nchar](2) NOT NULL,
	                [DOCNO] [varchar](15) NOT NULL,
	                [DOCNO1] [varchar](15) NULL,
	                [DATE] [datetime] NOT NULL,
	                [DUEDATE] [datetime] NOT NULL,
	                [SEQNO] [bigint] NOT NULL,
	                [ACODE] [varchar](15) NOT NULL,
	                [CCODE] [varchar](3) NOT NULL,
	                [DRCR] [varchar](1) NOT NULL,
	                [DESCRIPTION] [varchar](250) NOT NULL,
	                [AMOUNT] [numeric](18, 2) NOT NULL,
	                [CQNO] [varchar](13) NULL,
	                [CQDATE] [datetime] NULL,
	                [BANK] [varchar](4) NULL,
	                [BANKBRANCH] [varchar](4) NULL,
	                [PROCESS] [bit] NOT NULL,
	                [GLPOST] [bit] NOT NULL,
	                [GLPOSTUSER] [nvarchar](10) NULL,
	                [GLPOSTDATETIME] [datetime] NULL,
	                [GLPOSTCPNAME] [nvarchar](50) NULL,
	                [CUSTOMER] [bit] NOT NULL,
	                [CUSTOMERCODE] [varchar](250) NULL,
	                [SUPPLIER] [bit] NOT NULL,
	                [ISTAX] [bit] NOT NULL,
	                [ADDITION] [bit] NOT NULL,
	                [DEDUCTION] [bit] NOT NULL,
	                [ISPAIDIN] [bit] NOT NULL,
	                [ISPAIDOUT] [bit] NOT NULL,
	                [ISCREDITED] [bit] NOT NULL,
	                [SALESMANID] [bigint] NULL,
                 CONSTRAINT [PK_ImportJournalDetails] PRIMARY KEY CLUSTERED 
                (
	                [REFINDEX] ASC
                )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
                ) ON [PRIMARY]

                SET ANSI_PADDING OFF
                ALTER TABLE [dbo].[ImportJournalDetails] ADD  CONSTRAINT [DF_ImportJournalDetails_EXBATCH]  DEFAULT ('') FOR [EXBATCH]
                ALTER TABLE [dbo].[ImportJournalDetails] ADD  CONSTRAINT [DF_ImportJournalDetails_TRANTYPE]  DEFAULT ('') FOR [TRANTYPE]
                ALTER TABLE [dbo].[ImportJournalDetails] ADD  CONSTRAINT [DF_ImportJournalDetails_DOCNO]  DEFAULT ('') FOR [DOCNO]
                ALTER TABLE [dbo].[ImportJournalDetails] ADD  CONSTRAINT [DF_ImportJournalDetails_DOCNO1]  DEFAULT ('') FOR [DOCNO1]
                ALTER TABLE [dbo].[ImportJournalDetails] ADD  CONSTRAINT [DF_ImportJournalDetails_DATE]  DEFAULT (getdate()) FOR [DATE]
                ALTER TABLE [dbo].[ImportJournalDetails] ADD  CONSTRAINT [DF_ImportJournalDetails_DUEDATE]  DEFAULT (getdate()) FOR [DUEDATE]
                ALTER TABLE [dbo].[ImportJournalDetails] ADD  CONSTRAINT [DF_ImportJournalDetails_SEQNO]  DEFAULT ((0)) FOR [SEQNO]
                ALTER TABLE [dbo].[ImportJournalDetails] ADD  CONSTRAINT [DF_ImportJournalDetails_ACODE]  DEFAULT ('') FOR [ACODE]
                ALTER TABLE [dbo].[ImportJournalDetails] ADD  CONSTRAINT [DF_ImportJournalDetails_CCODE]  DEFAULT ('') FOR [CCODE]
                ALTER TABLE [dbo].[ImportJournalDetails] ADD  CONSTRAINT [DF_ImportJournalDetails_DRCR]  DEFAULT ('') FOR [DRCR]
                ALTER TABLE [dbo].[ImportJournalDetails] ADD  CONSTRAINT [DF_ImportJournalDetails_DESCRIPTION]  DEFAULT ('') FOR [DESCRIPTION]
                ALTER TABLE [dbo].[ImportJournalDetails] ADD  CONSTRAINT [DF_ImportJournalDetails_AMOUNT]  DEFAULT ((0)) FOR [AMOUNT]
                ALTER TABLE [dbo].[ImportJournalDetails] ADD  CONSTRAINT [DF_ImportJournalDetails_CQNO]  DEFAULT ('') FOR [CQNO]
                ALTER TABLE [dbo].[ImportJournalDetails] ADD  CONSTRAINT [DF_ImportJournalDetails_CQDATE]  DEFAULT (getdate()) FOR [CQDATE]
                ALTER TABLE [dbo].[ImportJournalDetails] ADD  CONSTRAINT [DF_ImportJournalDetails_BANK]  DEFAULT ('') FOR [BANK]
                ALTER TABLE [dbo].[ImportJournalDetails] ADD  CONSTRAINT [DF_ImportJournalDetails_BANKBRANCH]  DEFAULT ('') FOR [BANKBRANCH]
                ALTER TABLE [dbo].[ImportJournalDetails] ADD  CONSTRAINT [DF_ImportJournalDetails_PROCESS]  DEFAULT ((0)) FOR [PROCESS]
                ALTER TABLE [dbo].[ImportJournalDetails] ADD  CONSTRAINT [DF_ImportJournalDetails_GLPOST]  DEFAULT ((0)) FOR [GLPOST]
                ALTER TABLE [dbo].[ImportJournalDetails] ADD  CONSTRAINT [DF_ImportJournalDetails_GLPOSTUSER]  DEFAULT ((0)) FOR [GLPOSTUSER]
                ALTER TABLE [dbo].[ImportJournalDetails] ADD  CONSTRAINT [DF_ImportJournalDetails_GLPOSTDATETIME]  DEFAULT (getdate()) FOR [GLPOSTDATETIME]
                ALTER TABLE [dbo].[ImportJournalDetails] ADD  CONSTRAINT [DF_ImportJournalDetails_GLPOSTCPNAME]  DEFAULT ((0)) FOR [GLPOSTCPNAME]
                ALTER TABLE [dbo].[ImportJournalDetails] ADD  CONSTRAINT [DF_ImportJournalDetails_CUSTOMER]  DEFAULT ((0)) FOR [CUSTOMER]
                ALTER TABLE [dbo].[ImportJournalDetails] ADD  CONSTRAINT [DF_ImportJournalDetails_CUSTOMERCODE]  DEFAULT ('') FOR [CUSTOMERCODE]
                ALTER TABLE [dbo].[ImportJournalDetails] ADD  CONSTRAINT [DF_ImportJournalDetails_SUPPLIER]  DEFAULT ((0)) FOR [SUPPLIER]
                ALTER TABLE [dbo].[ImportJournalDetails] ADD  CONSTRAINT [DF_ImportJournalDetails_ISTAX]  DEFAULT ((0)) FOR [ISTAX]
                ALTER TABLE [dbo].[ImportJournalDetails] ADD  CONSTRAINT [DF_ImportJournalDetails_ADDITION]  DEFAULT ((0)) FOR [ADDITION]
                ALTER TABLE [dbo].[ImportJournalDetails] ADD  CONSTRAINT [DF_ImportJournalDetails_DEDUCTION]  DEFAULT ((0)) FOR [DEDUCTION]
                ALTER TABLE [dbo].[ImportJournalDetails] ADD  CONSTRAINT [DF_ImportJournalDetails_ISPAIDIN]  DEFAULT ((0)) FOR [ISPAIDIN]
                ALTER TABLE [dbo].[ImportJournalDetails] ADD  CONSTRAINT [DF_ImportJournalDetails_ISPAIDOUT]  DEFAULT ((0)) FOR [ISPAIDOUT]
                ALTER TABLE [dbo].[ImportJournalDetails] ADD  CONSTRAINT [DF__ImportJou__ISCRE__36DC0ACC]  DEFAULT ((0)) FOR [ISCREDITED]
                ALTER TABLE [dbo].[ImportJournalDetails] ADD  CONSTRAINT [DF__ImportJou__SALES__37D02F05]  DEFAULT ((0)) FOR [SALESMANID]";


                ExecuteMainQuery(Stringsqlconnection);

                #endregion ImportJournalDetails

                #region ImportJournalDetailsLogs
                tableName = "ImportJournalDetailsLogs";
                query = @"CREATE TABLE [dbo].[ImportJournalDetailsLogs](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DocumentNumber] [varchar](50) NULL,
	[FromDate] [datetime] NOT NULL,
	[ToDate] [datetime] NOT NULL,
 CONSTRAINT [PK_dbo.ImportJournalDetailsLogs] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion ImportJournalDetailsLogs

                #region InterDepartments
                tableName = "InterDepartments";
                query = @"CREATE TABLE [dbo].[InterDepartments](
	[InterDepartmentId] [bigint] IDENTITY(1,1) NOT NULL,
	[InterDepartmentCode] [nvarchar](50) NOT NULL,
	[InterDepartmentName] [nvarchar](100) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[InterDeptLocId] [bigint] NOT NULL,
	[Remark] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.InterDepartments] PRIMARY KEY CLUSTERED 
(
	[InterDepartmentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion InterDepartments

                #region InvAdvanceNoteDets
                tableName = "InvAdvanceNoteDets";
                query = @"CREATE TABLE [dbo].[InvAdvanceNoteDets](
	[InvAdvanceNoteDetID] [int] IDENTITY(1,1) NOT NULL,
	[Idx] [bigint] NULL,
	[ProductID] [bigint] NOT NULL,
	[ProductCode] [nvarchar](25) NULL,
	[RefCode] [nvarchar](25) NULL,
	[BarCodeFull] [bigint] NOT NULL,
	[Descrip] [nvarchar](50) NULL,
	[BatchNo] [nvarchar](50) NULL,
	[SerialNo] [nvarchar](50) NULL,
	[ExpiryDate] [datetime] NULL,
	[Cost] [decimal](18, 2) NOT NULL,
	[AvgCost] [decimal](18, 2) NOT NULL,
	[Price] [decimal](18, 2) NOT NULL,
	[Qty] [decimal](18, 2) NOT NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[UnitOfMeasureID] [bigint] NOT NULL,
	[UnitOfMeasureName] [nvarchar](10) NULL,
	[ConvertFactor] [decimal](18, 2) NOT NULL,
	[IDI1] [int] NOT NULL,
	[IDis1] [decimal](18, 2) NOT NULL,
	[IDiscount1] [decimal](18, 2) NOT NULL,
	[IDI1CashierID] [bigint] NOT NULL,
	[IDI2] [int] NOT NULL,
	[IDis2] [decimal](18, 2) NOT NULL,
	[IDiscount2] [decimal](18, 2) NOT NULL,
	[IDI2CashierID] [bigint] NOT NULL,
	[IDI3] [int] NOT NULL,
	[IDis3] [decimal](18, 2) NOT NULL,
	[IDiscount3] [decimal](18, 2) NOT NULL,
	[IDI3CashierID] [bigint] NOT NULL,
	[IDI4] [decimal](18, 2) NOT NULL,
	[IDis4] [decimal](18, 2) NOT NULL,
	[IDiscount4] [decimal](18, 2) NOT NULL,
	[IDI4CashierID] [bigint] NOT NULL,
	[IDI5] [int] NOT NULL,
	[IDis5] [decimal](18, 2) NOT NULL,
	[IDiscount5] [decimal](18, 2) NOT NULL,
	[IDI5CashierID] [bigint] NOT NULL,
	[Rate] [decimal](18, 2) NOT NULL,
	[IsSDis] [bit] NOT NULL,
	[SDNo] [int] NOT NULL,
	[SDID] [int] NOT NULL,
	[SDIs] [decimal](18, 2) NOT NULL,
	[SDiscount] [decimal](18, 2) NOT NULL,
	[DDisCashierID] [bigint] NOT NULL,
	[Nett] [decimal](18, 2) NOT NULL,
	[LocationID] [int] NOT NULL,
	[DocumentID] [int] NOT NULL,
	[BillTypeID] [int] NOT NULL,
	[SaleTypeID] [int] NOT NULL,
	[Receipt] [nvarchar](10) NULL,
	[SalesmanID] [bigint] NOT NULL,
	[Salesman] [nvarchar](15) NULL,
	[CustomerID] [bigint] NOT NULL,
	[Customer] [nvarchar](15) NULL,
	[CashierID] [bigint] NOT NULL,
	[Cashier] [nvarchar](15) NULL,
	[StartTime] [datetime] NOT NULL,
	[EndTime] [datetime] NOT NULL,
	[RecDate] [datetime] NOT NULL,
	[BaseUnitID] [bigint] NOT NULL,
	[UnitNo] [int] NOT NULL,
	[RowNo] [int] NOT NULL,
	[IsRecall] [bit] NOT NULL,
	[RecallNO] [nvarchar](10) NULL,
	[RecallAdv] [bit] NOT NULL,
	[TaxAmount] [decimal](18, 2) NOT NULL,
	[IsTax] [bit] NOT NULL,
	[TaxPercentage] [decimal](18, 2) NOT NULL,
	[IsStock] [bit] NOT NULL,
	[CreditNoteNo] [nvarchar](150) NULL,
	[CreditNoteBy] [bigint] NOT NULL,
	[CustomerType] [int] NOT NULL,
	[TransStatus] [int] NOT NULL,
	[IsPromotionApplied] [bit] NOT NULL,
	[PromotionID] [int] NOT NULL,
	[IsPromotion] [bit] NOT NULL,
	[ItemSerial] [nvarchar](50) NULL,
	[warranty] [nvarchar](50) NULL,
	[RecallFromInvoiceNo] [varchar](50) NULL,
	[WorkComplete] [bit] NULL,
	[WorkCompUser] [nvarchar](30) NULL,
	[WorkCompDateTime] [datetime] NULL,
	[CustCollected] [bit] NULL,
	[CustColDateTime] [datetime] NULL,
	[IsNewPrice] [bit] NOT NULL,
	[IsApproved] [bit] NOT NULL,
	[ApprovedBy] [bigint] NOT NULL,
	[ApprovedFor] [nchar](10) NULL,
	[ReferenceProductId] [int] NOT NULL,
	[ReferenceProductRow] [int] NOT NULL,
	[PrinterType] [int] NULL,
	[IsAddonItem] [bit] NULL,
	[TableNumber] [int] NULL,
	[IsTaxEnable] [bit] NULL,
	[TaxCode] [varchar](50) NULL,
	[SplitItemReceiptNo] [varchar](50) NULL,
	[IsPritRpt] [bit] NULL,
	[ProductRemark] [varchar](200) NULL,
	[OrderStatus] [int] NULL,
	[ServingUnit] [varchar](50) NULL,
	[NoOfCustomers] [int] NULL,
	[IsShowOnBill] [bit] NULL,
	[DeploCardNo] [varchar](50) NULL,
	[ServingUnitId] [int] NULL,
	[IsProduction] [bit] NOT NULL,
 CONSTRAINT [PK_dbo.InvAdvanceNoteDets] PRIMARY KEY CLUSTERED 
(
	[InvAdvanceNoteDetID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion InvAdvanceNoteDets

                #region InvAdvanceNoteHeds
                tableName = "InvAdvanceNoteHeds";
                query = @"CREATE TABLE [dbo].[InvAdvanceNoteHeds](
	[InvAdvanceNoteHedID] [bigint] IDENTITY(1,1) NOT NULL,
	[AdNoteNo] [nvarchar](15) NULL,
	[Receipt] [nvarchar](15) NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[Balance] [decimal](18, 2) NOT NULL,
	[LocationID] [int] NOT NULL,
	[Date] [datetime] NOT NULL,
	[UnitNo] [int] NOT NULL,
	[CashierID] [int] NOT NULL,
	[Time] [datetime] NOT NULL,
	[Zno] [bigint] NOT NULL,
	[RecallFromInvoice] [int] NOT NULL,
	[DeliveryDate] [datetime] NOT NULL,
	[Remark] [nvarchar](max) NULL,
	[IsProduction] [bit] NOT NULL,
	[ProcessLoc] [int] NOT NULL,
	[PickupLoc] [int] NOT NULL,
	[Status] [bit] NOT NULL,
	[CompanyId] [int] NOT NULL,
 CONSTRAINT [PK_dbo.InvAdvanceNoteHeds] PRIMARY KEY CLUSTERED 
(
	[InvAdvanceNoteHedID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion InvAdvanceNoteHeds

                #region InvAdvancePaymentDets
                tableName = "InvAdvancePaymentDets";
                query = @"CREATE TABLE [dbo].[InvAdvancePaymentDets](
	[InvAdvancePaymentDetId] [bigint] IDENTITY(1,1) NOT NULL,
	[Idx] [bigint] NOT NULL,
	[RowNo] [bigint] NOT NULL,
	[PayTypeID] [int] NOT NULL,
	[Amount] [decimal](18, 4) NOT NULL,
	[Balance] [decimal](18, 4) NOT NULL,
	[SDate] [datetime] NOT NULL,
	[Receipt] [char](10) NOT NULL,
	[LocationID] [int] NOT NULL,
	[CashierID] [bigint] NOT NULL,
	[UnitNo] [int] NOT NULL,
	[BillTypeID] [int] NOT NULL,
	[RefNo] [varchar](30) NOT NULL,
	[BankId] [bigint] NOT NULL,
	[ChequeDate] [date] NULL,
	[IsRecallAdv] [bit] NOT NULL,
	[RecallNo] [varchar](10) NOT NULL,
	[Descrip] [varchar](20) NOT NULL,
	[EnCodeName] [varchar](50) NOT NULL,
	[SuspendNo] [nchar](50) NOT NULL,
	[SuspendBy] [bit] NOT NULL,
	[IsDeleteOnRecall] [bit] NOT NULL,
	[AdvanceNumber] [varchar](20) NOT NULL,
 CONSTRAINT [PK_dbo.InvAdvancePaymentDets1] PRIMARY KEY CLUSTERED 
(
	[InvAdvancePaymentDetId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion InvAdvancePaymentDets

                #region InvBillValueDiscounts
                tableName = "InvBillValueDiscounts";
                query = @"CREATE TABLE [dbo].[InvBillValueDiscounts](
	[InvBillValueDiscountId] [int] IDENTITY(1,1) NOT NULL,
	[PromotionMasterId] [int] NOT NULL,
	[TotalBillValueDiscount] [bit] NOT NULL,
	[BillValueRangeDiscount] [bit] NOT NULL,
	[BillValueRangeFrom] [decimal](18, 2) NOT NULL,
	[BillValueRangeTo] [decimal](18, 2) NOT NULL,
	[DiscountType] [varchar](3) NULL,
	[DiscountAmount] [decimal](18, 2) NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 CONSTRAINT [PK_dbo.InvBillValueDiscounts] PRIMARY KEY CLUSTERED 
(
	[InvBillValueDiscountId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion InvBillValueDiscounts

                #region InvBundleItemPrices
                tableName = "InvBundleItemPrices";
                query = @"CREATE TABLE [dbo].[InvBundleItemPrices](
	[InvBundleItemPriceId] [int] IDENTITY(1,1) NOT NULL,
	[PromotionMasterId] [int] NOT NULL,
	[InvId] [int] NOT NULL,
	[SinglePriceForAllItems] [bit] NOT NULL,
	[DifferentPricesForItems] [bit] NOT NULL,
	[ProductId] [int] NOT NULL,
	[ServingUnitId] [int] NOT NULL,
	[Quantity] [decimal](18, 2) NOT NULL,
	[SellingPrice] [decimal](18, 2) NOT NULL,
	[GroupId] [int] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[BundleName] [varchar](50) NULL,
 CONSTRAINT [PK_dbo.InvBundleItemPrices] PRIMARY KEY CLUSTERED 
(
	[InvBundleItemPriceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion InvBundleItemPrices

                #region InvGiftVoucherBookCodes
                tableName = "InvGiftVoucherBookCodes";
                query = @"CREATE TABLE [dbo].[InvGiftVoucherBookCodes](
	[InvGiftVoucherBookCodeID] [bigint] IDENTITY(1,1) NOT NULL,
	[InvGiftVoucherGroupID] [int] NOT NULL,
	[BookCode] [nvarchar](20) NOT NULL,
	[BookName] [nvarchar](50) NOT NULL,
	[BookPrefix] [nvarchar](4) NULL,
	[GiftVoucherValue] [decimal](18, 2) NOT NULL,
	[GiftVoucherPercentage] [decimal](18, 2) NOT NULL,
	[ValidityPeriod] [int] NOT NULL,
	[VoucherType] [int] NOT NULL,
	[StartingNo] [int] NOT NULL,
	[CurrentSerialNo] [int] NOT NULL,
	[SerialLength] [int] NOT NULL,
	[PageCount] [int] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[BasedOn] [int] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[CompanyID] [int] NULL,
	[LocationId] [int] NULL,
 CONSTRAINT [PK_dbo.InvGiftVoucherBookCode] PRIMARY KEY CLUSTERED 
(
	[InvGiftVoucherBookCodeID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion InvGiftVoucherBookCodes

                #region InvGiftVoucherGroups
                tableName = "InvGiftVoucherGroups";
                query = @"CREATE TABLE [dbo].[InvGiftVoucherGroups](
	[InvGiftVoucherGroupID] [int] IDENTITY(1,1) NOT NULL,
	[GiftVoucherGroupCode] [nvarchar](20) NOT NULL,
	[GiftVoucherGroupName] [nvarchar](50) NOT NULL,
	[Remark] [nvarchar](150) NULL,
	[IsDelete] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[CompanyID] [int] NULL,
	[LocationId] [int] NULL,
 CONSTRAINT [PK_dbo.InvGiftVoucherGroup] PRIMARY KEY CLUSTERED 
(
	[InvGiftVoucherGroupID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion InvGiftVoucherGroups

                #region InvGiftVoucherMasters
                tableName = "InvGiftVoucherMasters";
                query = @"CREATE TABLE [dbo].[InvGiftVoucherMasters](
	[InvGiftVoucherMasterID] [bigint] IDENTITY(1,1) NOT NULL,
	[InvGiftVoucherBookCodeID] [bigint] NOT NULL,
	[InvGiftVoucherGroupID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationID] [int] NOT NULL,
	[VoucherNo] [nvarchar](15) NOT NULL,
	[VoucherNoSerial] [int] NOT NULL,
	[VoucherPrefix] [nvarchar](4) NULL,
	[SerialLength] [int] NOT NULL,
	[GiftVoucherValue] [decimal](18, 2) NOT NULL,
	[GiftVoucherPercentage] [decimal](18, 2) NOT NULL,
	[StartingNo] [int] NOT NULL,
	[VoucherCount] [int] NOT NULL,
	[PageCount] [int] NOT NULL,
	[VoucherSerial] [nvarchar](max) NULL,
	[VoucherSerialNo] [int] NOT NULL,
	[VoucherType] [int] NOT NULL,
	[VoucherStatus] [int] NOT NULL,
	[ToLocationID] [int] NOT NULL,
	[SoldLocationID] [int] NOT NULL,
	[SoldCashierID] [bigint] NOT NULL,
	[SoldReceiptNo] [nvarchar](max) NULL,
	[SoldUnitID] [int] NOT NULL,
	[SoldZNo] [bigint] NOT NULL,
	[SoldDate] [datetime] NOT NULL,
	[RedeemedLocationID] [int] NOT NULL,
	[RedeemedCashierID] [bigint] NOT NULL,
	[RedeemedReceiptNo] [nvarchar](max) NULL,
	[RedeemedUnitID] [int] NOT NULL,
	[RedeemedZNo] [bigint] NOT NULL,
	[RedeemedDate] [datetime] NOT NULL,
	[IsBarcodePrinted] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[Expirydate] [datetime] NULL,
	[IsTemporaryBlocked] [bit] NOT NULL,
	[BlockedLocationID] [int] NOT NULL,
	[BlockedCashierID] [bit] NOT NULL,
	[BlockedUnitID] [int] NOT NULL,
	[BlockedDate] [datetime] NOT NULL,
	[GiftVoucherGroupCode] [nvarchar](50) NULL,
	[BookCode] [nvarchar](20) NULL,
	[IsCancel] [bit] NULL
 CONSTRAINT [PK_dbo.InvGiftVoucherMasters] PRIMARY KEY CLUSTERED 
(
	[InvGiftVoucherMasterID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion InvGiftVoucherMasters

                #region InvGiftVoucherPromotions
                tableName = "InvGiftVoucherPromotions";
                query = @"CREATE TABLE [dbo].[InvGiftVoucherPromotions](
	[InvGiftVoucherPromotionsId] [int] IDENTITY(1,1) NOT NULL,
	[PromotionMasterId] [int] NOT NULL,
	[GiftVoucherAmount] [decimal](18, 2) NOT NULL,
	[NoOfOccurrences] [int] NOT NULL,
	[Remarks] [nvarchar](max) NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[BillValue] [decimal](18, 2) NOT NULL,
 CONSTRAINT [PK_dbo.InvGiftVoucherPromotions] PRIMARY KEY CLUSTERED 
(
	[InvGiftVoucherPromotionsId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion InvGiftVoucherPromotions

                #region InvLoyaltyTransactions
                tableName = "InvLoyaltyTransactions";
                query = @"CREATE TABLE [dbo].[InvLoyaltyTransactions](
	[InvLoyaltyTransactionID] [bigint] IDENTITY(1,1) NOT NULL,
	[CustomerID] [bigint] NOT NULL,
	[CustomerType] [smallint] NOT NULL,
	[Receipt] [nvarchar](15) NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[Points] [decimal](18, 2) NOT NULL,
	[TransID] [smallint] NOT NULL,
	[LocationID] [smallint] NOT NULL,
	[DocumentDate] [datetime] NOT NULL,
	[UnitNo] [smallint] NOT NULL,
	[CashierID] [bigint] NOT NULL,
	[DocumentTime] [datetime] NOT NULL,
	[DiscPer] [decimal](18, 2) NOT NULL,
	[DiscAmt] [decimal](18, 2) NOT NULL,
	[PointsRate] [decimal](18, 2) NOT NULL,
	[Zno] [bigint] NOT NULL,
	[CardNo] [nvarchar](15) NULL,
	[CardType] [int] NOT NULL,
	[LoyaltyType] [int] NOT NULL,
	[IsGuidClaimed] [bit] NOT NULL,
	[IsSync] [bit] NOT NULL,
	[CustomerCode] [nvarchar](15) NULL,
	[NIC] [nvarchar](50) NULL,
	[RefNo] [nvarchar](50) NULL,
 CONSTRAINT [PK_dbo.InvLoyaltyTransactions] PRIMARY KEY CLUSTERED 
(
	[InvLoyaltyTransactionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion InvLoyaltyTransactions

                #region InvPosTerminalDetails
                tableName = "InvPosTerminalDetails";
                query = @"CREATE TABLE [dbo].[InvPosTerminalDetails](
	[InvPosTerminalDetailsID] [bigint] IDENTITY(1,1) NOT NULL,
	[LocationID] [int] NOT NULL,
	[TerminalId] [int] NOT NULL,
	[IP] [nvarchar](max) NULL,
	[DBNAME] [nvarchar](max) NULL,
	[UserId] [nvarchar](max) NULL,
	[PWD] [nvarchar](max) NULL,
	[JrnlPath] [nvarchar](max) NULL,
	[CompanyID] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 CONSTRAINT [PK_dbo.InvPosTerminalDetails] PRIMARY KEY CLUSTERED 
(
	[InvPosTerminalDetailsID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion InvPosTerminalDetails

                #region InvProductMasters
                tableName = "InvProductMasters";
                query = @"CREATE TABLE [dbo].[InvProductMasters](
	[InvProductMasterID] [int] IDENTITY(1,1) NOT NULL,
	[ProductCode] [nvarchar](20) NOT NULL,
	[BarCode] [nvarchar](max) NULL,
	[ReferenceCode] [nvarchar](max) NULL,
	[ProductName] [nvarchar](100) NOT NULL,
	[InvoicePrintName] [nvarchar](50) NOT NULL,
	[SinhalaDescription] [nvarchar](max) NULL,
	[Department] [int] NOT NULL,
	[Category] [int] NOT NULL,
	[SubCategory] [int] NOT NULL,
	[SubCategory2] [int] NOT NULL,
	[KitchecnBarCategory] [int] NOT NULL,
	[SuplierID] [int] NOT NULL,
	[Image] [varbinary](max) NULL,
	[CostPrice] [decimal](18, 2) NOT NULL,
	[OrderPrice] [decimal](18, 2) NOT NULL,
	[AverageCost] [decimal](18, 2) NOT NULL,
	[SellingPrice] [decimal](18, 2) NOT NULL,
	[WholesalePrice] [decimal](18, 2) NOT NULL,
	[MinimumPrice] [decimal](18, 2) NOT NULL,
	[FixedDiscount] [decimal](18, 2) NOT NULL,
	[MaximumDiscount] [decimal](18, 2) NOT NULL,
	[MaximumPrice] [decimal](18, 2) NOT NULL,
	[FixDiscountPercentage] [decimal](18, 2) NOT NULL,
	[MaximumDiscountPercentage] [decimal](18, 2) NOT NULL,
	[ReorderLevel] [decimal](18, 2) NOT NULL,
	[ReorderQty] [decimal](18, 2) NOT NULL,
	[ReorderPeriod] [decimal](18, 2) NOT NULL,
	[Remarks] [nvarchar](max) NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 CONSTRAINT [PK_dbo.InvProductMasters] PRIMARY KEY CLUSTERED 
(
	[InvProductMasterID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion InvProductMasters

                #region InvPromoBillValueBasedGetYProducts
                tableName = "InvPromoBillValueBasedGetYProducts";
                query = @"CREATE TABLE [dbo].[InvPromoBillValueBasedGetYProducts](
	[InvPromoBillValueBasedGetYProductId] [bigint] IDENTITY(1,1) NOT NULL,
	[InvPromotionMasterId] [bigint] NOT NULL,
	[ProductId] [bigint] NOT NULL,
	[ValueFrom] [decimal](18, 2) NOT NULL,
	[ValueTo] [decimal](18, 2) NOT NULL,
	[ServingUnitId] [int] NOT NULL,
	[BuyUnitOfMeasureId] [bigint] NOT NULL,
	[Rate] [decimal](18, 2) NOT NULL,
	[Qty] [decimal](18, 2) NOT NULL,
	[Points] [bigint] NOT NULL,
	[DiscountPercentage] [decimal](18, 2) NOT NULL,
	[DiscountAmount] [decimal](18, 2) NOT NULL,
	[ProductType] [int] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 CONSTRAINT [PK_dbo.InvPromoBillValueBasedGetYProducts] PRIMARY KEY CLUSTERED 
(
	[InvPromoBillValueBasedGetYProductId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion InvPromoBillValueBasedGetYProducts

                #region InvPromoBusinessTypes
                tableName = "InvPromoBusinessTypes";
                query = @"CREATE TABLE [dbo].[InvPromoBusinessTypes](
	[InvPromoBusinessTypeID] [bigint] IDENTITY(1,1) NOT NULL,
	[InvPromotionMasterID] [bigint] NOT NULL,
	[CateringMoodID] [bigint] NOT NULL,
	[CateringMoodName] [nvarchar](20) NOT NULL,
	[Remark] [nvarchar](150) NULL,
	[Status] [bit] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
 CONSTRAINT [PK_dbo.InvPromoBusinessTypes] PRIMARY KEY CLUSTERED 
(
	[InvPromoBusinessTypeID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion InvPromoBusinessTypes

                #region InvPromoCustomerCategories
                tableName = "InvPromoCustomerCategories";
                query = @"CREATE TABLE [dbo].[InvPromoCustomerCategories](
	[InvPromoCustomerCategoryID] [bigint] IDENTITY(1,1) NOT NULL,
	[InvPromotionMasterID] [bigint] NOT NULL,
	[CustomerCategoryID] [int] NOT NULL,
	[Remark] [nvarchar](150) NULL,
	[Status] [bit] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
 CONSTRAINT [PK_dbo.InvPromoCustomerCategories] PRIMARY KEY CLUSTERED 
(
	[InvPromoCustomerCategoryID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion InvPromoCustomerCategories

                #region InvPromoLowestPriceWaveOffs
                tableName = "InvPromoLowestPriceWaveOffs";
                query = @"CREATE TABLE [dbo].[InvPromoLowestPriceWaveOffs](
	[InvPromoLowestPriceWaveOffID] [bigint] IDENTITY(1,1) NOT NULL,
	[InvPromotionMasterID] [bigint] NOT NULL,
	[LowestPriceWaveOffCode] [nvarchar](15) NOT NULL,
	[LowestPrice] [decimal](18, 2) NOT NULL,
	[IsFullWaveOff] [bit] NOT NULL,
	[Qty] [decimal](18, 2) NOT NULL,
	[Remark] [nvarchar](150) NULL,
	[Status] [bit] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[CompanyId] [int] NOT NULL,
 CONSTRAINT [PK_dbo.InvPromoLowestPriceWaveOffs] PRIMARY KEY CLUSTERED 
(
	[InvPromoLowestPriceWaveOffID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion InvPromoLowestPriceWaveOffs

                #region InvPromotionDetailsBuyXProducts
                tableName = "InvPromotionDetailsBuyXProducts";
                query = @"CREATE TABLE [dbo].[InvPromotionDetailsBuyXProducts](
	[InvPromotionDetailsBuyXProductId] [bigint] IDENTITY(1,1) NOT NULL,
	[InvPromotionMasterId] [bigint] NOT NULL,
	[ProductId] [bigint] NOT NULL,
	[ServingUnitId] [int] NOT NULL,
	[BuyUnitOfMeasureId] [bigint] NOT NULL,
	[Rate] [decimal](18, 2) NOT NULL,
	[Qty] [decimal](18, 2) NOT NULL,
	[ProductType] [int] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[Points] [bigint] NOT NULL,
	[DiscountPercentage] [decimal](18, 2) NOT NULL,
	[DiscountAmount] [decimal](18, 2) NOT NULL,
	[GroupId] [int] NOT NULL,
 CONSTRAINT [PK_dbo.InvPromotionDetailsBuyXProducts] PRIMARY KEY CLUSTERED 
(
	[InvPromotionDetailsBuyXProductId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion InvPromotionDetailsBuyXProducts

                #region InvPromotionDetailsProductDis
                tableName = "InvPromotionDetailsProductDis";
                query = @"CREATE TABLE [dbo].[InvPromotionDetailsProductDis](
	[InvPromotionDetailsProductDisId] [bigint] IDENTITY(1,1) NOT NULL,
	[InvPromotionMasterID] [bigint] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationID] [int] NOT NULL,
	[ProductID] [int] NOT NULL,
	[UnitOfMeasureID] [bigint] NOT NULL,
	[Rate] [decimal](18, 2) NOT NULL,
	[FromQty] [decimal](18, 2) NOT NULL,
	[ToQty] [decimal](18, 2) NOT NULL,
	[Points] [bigint] NOT NULL,
	[DiscountPercentage] [decimal](18, 2) NOT NULL,
	[DiscountAmount] [decimal](18, 2) NOT NULL,
	[ServingUnitId] [int] NOT NULL,
	[DepartmentId] [bigint] NOT NULL,
	[CategoryId] [bigint] NOT NULL,
	[SubCategoryId] [bigint] NOT NULL,
 CONSTRAINT [PK_dbo.InvPromotionDetailsProductDis] PRIMARY KEY CLUSTERED 
(
	[InvPromotionDetailsProductDisId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion InvPromotionDetailsProductDis

                #region InvPromotionMasters
                tableName = "InvPromotionMasters";
                query = @"CREATE TABLE [dbo].[InvPromotionMasters](
	[InvPromotionMasterID] [bigint] IDENTITY(1,1) NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationID] [int] NOT NULL,
	[CostCentreID] [int] NOT NULL,
	[PromotionCode] [nvarchar](15) NOT NULL,
	[PromotionName] [nvarchar](50) NOT NULL,
	[IsAutoApply] [bit] NOT NULL,
	[PromotionTypeID] [int] NOT NULL,
	[StartDate] [datetime] NOT NULL,
	[EndDate] [datetime] NOT NULL,
	[IsMonday] [bit] NOT NULL,
	[IsTuesday] [bit] NOT NULL,
	[IsWednesday] [bit] NOT NULL,
	[IsThuresday] [bit] NOT NULL,
	[IsFriday] [bit] NOT NULL,
	[IsSaturday] [bit] NOT NULL,
	[IsSunday] [bit] NOT NULL,
	[IsMondayTime] [bit] NOT NULL,
	[IsTuesdayTime] [bit] NOT NULL,
	[IsWednesdayTime] [bit] NOT NULL,
	[IsThuresdayTime] [bit] NOT NULL,
	[IsFridayTime] [bit] NOT NULL,
	[IsSaturdayTime] [bit] NOT NULL,
	[IsSundayTime] [bit] NOT NULL,
	[MondayStartTime] [datetime] NULL,
	[MondayEndTime] [datetime] NULL,
	[TuesdayStartTime] [datetime] NULL,
	[TuesdayEndTime] [datetime] NULL,
	[WednesdayStartTime] [datetime] NULL,
	[WednesdayEndTime] [datetime] NULL,
	[ThuresdayStartTime] [datetime] NULL,
	[ThuresdayEndTime] [datetime] NULL,
	[FridayStartTime] [datetime] NULL,
	[FridayEndTime] [datetime] NULL,
	[SaturdayStartTime] [datetime] NULL,
	[SaturdayEndTime] [datetime] NULL,
	[SundayStartTime] [datetime] NULL,
	[SundayEndTime] [datetime] NULL,
	[PaymentMethodID] [int] NOT NULL,
	[IsProvider] [bit] NOT NULL,
	[IsAllLocations] [bit] NOT NULL,
	[IsAllType] [bit] NOT NULL,
	[IsValueRange] [bit] NOT NULL,
	[MaximumValue] [decimal](18, 2) NOT NULL,
	[DiscountValue] [decimal](18, 2) NOT NULL,
	[DiscountPercentage] [decimal](18, 2) NOT NULL,
	[Points] [decimal](18, 2) NOT NULL,
	[Remark] [nvarchar](150) NULL,
	[DisplayMessage] [nvarchar](150) NULL,
	[CashierMessage] [nvarchar](150) NULL,
	[IsDelete] [bit] NOT NULL,
	[IsRaffle] [bit] NOT NULL,
	[IsIncreseQty] [bit] NOT NULL,
	[PromotionTypeNew] [int] NOT NULL,
	[SupplierID] [int] NOT NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[MinimumValue] [decimal](18, 2) NOT NULL,
	[CustomerGroupId] [int] NULL,
	[PromotionCount] [int] NOT NULL,
	[CreateDate] [datetime] NULL,
	[CreateUser] [nvarchar](max) NULL,
	[ModifiedUser] [nvarchar](max) NULL,
	[IsActive] [bit] NOT NULL,
 CONSTRAINT [PK_dbo.InvPromotionMasters] PRIMARY KEY CLUSTERED 
(
	[InvPromotionMasterID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion InvPromotionMasters

                #region InvPromotionTypes
                tableName = "InvPromotionTypes";
                query = @"CREATE TABLE [dbo].[InvPromotionTypes](
	[InvPromotionTypeID] [bigint] IDENTITY(1,1) NOT NULL,
	[PromotionTypeCode] [nvarchar](15) NOT NULL,
	[PromotionTypeName] [nvarchar](100) NOT NULL,
	[Remark] [nvarchar](150) NULL,
	[IsDelete] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 CONSTRAINT [PK_dbo.InvPromotionTypes] PRIMARY KEY CLUSTERED 
(
	[InvPromotionTypeID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion InvPromotionTypes

                #region InvSales
                tableName = "InvSales";
                query = @"CREATE TABLE [dbo].[InvSales](
	[InvSalesId] [bigint] IDENTITY(1,1) NOT NULL,
	[SalesId] [bigint] NOT NULL,
	[CompanyId] [int] NOT NULL,
	[CompanyCode] [nvarchar](max) NULL,
	[CompanyName] [nvarchar](max) NULL,
	[LocationId] [int] NOT NULL,
	[LocationCode] [nvarchar](max) NULL,
	[LocationName] [nvarchar](max) NULL,
	[CostCentreId] [int] NOT NULL,
	[DocumentId] [int] NOT NULL,
	[DocumentNo] [nvarchar](max) NULL,
	[ReferenceNo] [nvarchar](max) NULL,
	[DocumentDate] [datetime] NOT NULL,
	[TransactionTime] [datetime] NOT NULL,
	[CustomerType] [int] NOT NULL,
	[CustomerId] [bigint] NOT NULL,
	[CustomerCode] [nvarchar](max) NULL,
	[CustomerName] [nvarchar](max) NULL,
	[SupplierID] [bigint] NOT NULL,
	[SupplierCode] [nvarchar](max) NULL,
	[SupplierName] [nvarchar](max) NULL,
	[SalesPersonId] [bigint] NOT NULL,
	[SalesPersonCode] [nvarchar](max) NULL,
	[SalesPersonName] [nvarchar](max) NULL,
	[GrossAmount] [decimal](18, 2) NOT NULL,
	[DiscountPercentage] [decimal](18, 2) NOT NULL,
	[DiscountAmount] [decimal](18, 2) NOT NULL,
	[NetAmount] [decimal](18, 2) NOT NULL,
	[SubTotalDiscountPercentage] [decimal](18, 2) NOT NULL,
	[SubTotalDiscountAmount] [decimal](18, 2) NOT NULL,
	[CurrencyId] [int] NOT NULL,
	[CurrencyRate] [int] NOT NULL,
	[DepartmentId] [int] NOT NULL,
	[DepartmentCode] [nvarchar](max) NULL,
	[DepartmentName] [nvarchar](max) NULL,
	[CategoryId] [bigint] NOT NULL,
	[CategoryCode] [nvarchar](max) NULL,
	[CategoryName] [nvarchar](max) NULL,
	[SubCategoryId] [bigint] NOT NULL,
	[SubCategoryCode] [nvarchar](max) NULL,
	[SubCategoryName] [nvarchar](max) NULL,
	[ProductId] [bigint] NOT NULL,
	[ProductCode] [nvarchar](max) NULL,
	[ProductName] [nvarchar](max) NULL,
	[BarCode] [nvarchar](max) NULL,
	[BatchNo] [nvarchar](max) NULL,
	[ExpiryDate] [datetime] NOT NULL,
	[Qty] [decimal](18, 2) NOT NULL,
	[UnitOfMeasureId] [bigint] NOT NULL,
	[UnitOfMeasureName] [nvarchar](max) NULL,
	[PackSize] [decimal](18, 2) NOT NULL,
	[SellingPrice] [decimal](18, 2) NOT NULL,
	[WholeSalePrice] [decimal](18, 2) NOT NULL,
	[CostPrice] [decimal](18, 2) NOT NULL,
	[AverageCost] [decimal](18, 2) NOT NULL,
	[DocumentStatus] [int] NOT NULL,
	[IsFreeIssue] [bit] NOT NULL,
	[TerminalNo] [nvarchar](max) NULL,
	[IsDispatch] [bit] NOT NULL,
	[IsUpLoad] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[UnitNo] [int] NOT NULL,
	[IsBackOffice] [bit] NOT NULL,
	[ZNo] [bigint] NOT NULL,
	[GroupOfCompanyId] [int] NOT NULL,
	[CreatedUser] [nvarchar](max) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](max) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[SerialNo] [int] NOT NULL,
	[CorporatePrice] [decimal](18, 2) NOT NULL,
 CONSTRAINT [PK_dbo.InvSales] PRIMARY KEY CLUSTERED 
(
	[InvSalesId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion InvSales

                #region InvSuppliers
                tableName = "InvSuppliers";
                query = @"CREATE TABLE [dbo].[InvSuppliers](
	[InvSupplierID] [int] IDENTITY(1,1) NOT NULL,
	[SupplierCode] [nvarchar](max) NOT NULL,
	[SupplierName] [nvarchar](100) NOT NULL,
	[SupplierType] [nvarchar](max) NULL,
	[Address1] [nvarchar](max) NULL,
	[Address2] [nvarchar](max) NULL,
	[Address3] [nvarchar](max) NULL,
	[Telephone] [nvarchar](max) NULL,
	[Mobile] [nvarchar](max) NULL,
	[Fax] [nvarchar](max) NULL,
	[Email] [nvarchar](max) NULL,
	[ContactPerson] [nvarchar](max) NULL,
	[ConsignmentType] [int] NOT NULL,
	[CreditLimit] [decimal](18, 2) NOT NULL,
	[CreditPeriod] [decimal](18, 2) NOT NULL,
	[OpeningBalance] [decimal](18, 2) NOT NULL,
	[CurrentMonthPurchase] [nvarchar](max) NULL,
	[CurrentMonthReturns] [nvarchar](max) NULL,
	[CurrentMonthPayments] [nvarchar](max) NULL,
	[TotalOutstandings] [decimal](18, 2) NOT NULL,
	[SupplierGroup] [int] NOT NULL,
	[SupplierOrderCycle] [nvarchar](max) NULL,
	[SupplierVATRegNo] [nvarchar](max) NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 CONSTRAINT [PK_dbo.InvSuppliers] PRIMARY KEY CLUSTERED 
(
	[InvSupplierID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion InvSuppliers

                #region JobHeaders
                tableName = "JobHeaders";
                query = @"CREATE TABLE [dbo].[JobHeaders](
	[JobHeaderId] [int] IDENTITY(1,1) NOT NULL,
	[JobNumber] [nvarchar](max) NULL,
	[JobDate] [datetime] NOT NULL,
	[StartTime] [datetime] NOT NULL,
	[EndTime] [datetime] NULL,
	[Status] [int] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 CONSTRAINT [PK_dbo.JobHeaders] PRIMARY KEY CLUSTERED 
(
	[JobHeaderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion JobHeaders

                #region JobItems
                tableName = "JobItems";
                query = @"CREATE TABLE [dbo].[JobItems](
	[JobItemId] [int] IDENTITY(1,1) NOT NULL,
	[JobHeaderId] [int] NOT NULL,
	[ProductId] [int] NOT NULL,
	[SystemQty] [decimal](18, 2) NOT NULL,
	[PhysicalQty] [decimal](18, 2) NOT NULL,
	[Status] [int] NOT NULL,
	[DepartmentId] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
 CONSTRAINT [PK_dbo.JobItems] PRIMARY KEY CLUSTERED 
(
	[JobItemId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion JobItems

                #region KitchenMasters
                tableName = "KitchenMasters";
                query = @"CREATE TABLE [dbo].[KitchenMasters](
	[KitchenID] [bigint] IDENTITY(1,1) NOT NULL,
	[KitchenCode] [varchar](10) NOT NULL,
	[KitchenDesc] [varchar](20) NOT NULL,
	[KitchenPrinterName] [varchar](100) NOT NULL,
	[KitchenPrinterType] [int] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 CONSTRAINT [PK_dbo.KitchenMasters] PRIMARY KEY CLUSTERED 
(
	[KitchenID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion KitchenMasters

                #region KitchenPrinterTypes
                tableName = "KitchenPrinterTypes";
                query = @"CREATE TABLE [dbo].[KitchenPrinterTypes](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[ProductID] [int] NOT NULL,
	[PrinterName] [nvarchar](50) NULL,
	[LocationID] [int] NOT NULL,
	[PrinterID] [int] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
 CONSTRAINT [PK_KitchenPrinterTypes] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion KitchenPrinterTypes

                #region KOTBOTDescriptions
                tableName = "KOTBOTDescriptions";
                query = @"CREATE TABLE [dbo].[KOTBOTDescriptions](
	[KOTBOTDescriptionId] [bigint] IDENTITY(1,1) NOT NULL,
	[Description] [nvarchar](max) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[Type] [nvarchar](max) NOT NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[CompanyId] [int] NOT NULL,
 CONSTRAINT [PK_dbo.KOTBOTDescriptions] PRIMARY KEY CLUSTERED 
(
	[KOTBOTDescriptionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion KOTBOTDescriptions

                #region LoanPaymentDetails
                tableName = "LoanPaymentDetails";
                query = @"CREATE TABLE [dbo].[LoanPaymentDetails](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerId] [int] NOT NULL,
	[CustomerCode] [varchar](50) NOT NULL,
	[CustomerName] [varchar](150) NOT NULL,
	[LoanCode] [varchar](50) NOT NULL,
	[LoanName] [varchar](50) NOT NULL,
	[PayType] [int] NOT NULL,
	[PayDescrip] [varchar](50) NOT NULL,
	[PaidAmount] [decimal](18, 2) NOT NULL,
	[BalanceAmount] [decimal](18, 2) NOT NULL,
	[RecDate] [datetime] NULL,
	[LocationId] [int] NOT NULL,
	[BankId] [int] NOT NULL,
	[ChequeNumber] [varchar](50) NOT NULL,
	[ChequeDate] [datetime] NULL,
	[Receipt] [varchar](20) NOT NULL,
	[UnitNo] [int] NOT NULL,
	[ZNumber] [int] NOT NULL,
	[CashierID] [int] NOT NULL,
	[Online] [int] NOT NULL,
 CONSTRAINT [PK_LoanPaymentDetails] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion LoanPaymentDetails

                #region LocationTaxes
                tableName = "LocationTaxes";
                query = @"CREATE TABLE [dbo].[LocationTaxes](
	[LocationTaxId] [bigint] IDENTITY(1,1) NOT NULL,
	[TaxLocationId] [bigint] NOT NULL,
	[TaxId] [bigint] NOT NULL,
	[TaxPracentage] [decimal](18, 2) NOT NULL,
	[TaxSequence] [int] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 CONSTRAINT [PK_dbo.LocationTaxes] PRIMARY KEY CLUSTERED 
(
	[LocationTaxId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion LocationTaxes

                #region LOGAddons
                tableName = "LOGAddons";
                query = @"CREATE TABLE [dbo].[LOGAddons](
	[AddonsId] [int] IDENTITY(1,1) NOT NULL,
	[SourceId] [int] NOT NULL,
	[ProductId] [bigint] NOT NULL,
	[ProductAddonId] [bigint] NOT NULL,
	[DepartmentId] [bigint] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[AddonSellingPrice] [decimal](18, 2) NOT NULL,
	[AddonQuantity] [decimal](18, 2) NOT NULL,
	[IsShowOnBill] [bit] NOT NULL,
	[Action] [nvarchar](max) NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 CONSTRAINT [PK_dbo.LOGAddons] PRIMARY KEY CLUSTERED 
(
	[AddonsId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion LOGAddons

                #region LOGCustomers
                tableName = "LOGCustomers";
                query = @"CREATE TABLE [dbo].[LOGCustomers](
	[CustomerID] [int] IDENTITY(1,1) NOT NULL,
	[CustomerCode] [nvarchar](max) NOT NULL,
	[CustomerTitle] [nvarchar](max) NOT NULL,
	[CustomerName] [nvarchar](100) NOT NULL,
	[CustomerType] [nvarchar](max) NULL,
	[CustomerCategoryId] [int] NOT NULL,
	[BillingAddress1] [nvarchar](100) NOT NULL,
	[BillingAddress2] [nvarchar](100) NOT NULL,
	[BillingAddress3] [nvarchar](max) NULL,
	[DOB] [datetime] NULL,
	[NIC] [nvarchar](12) NOT NULL,
	[Passport] [nvarchar](max) NULL,
	[Telephone] [nvarchar](max) NULL,
	[Mobile] [nvarchar](max) NOT NULL,
	[Fax] [nvarchar](max) NULL,
	[Email] [nvarchar](max) NULL,
	[VehicleNo] [nvarchar](max) NULL,
	[Profession] [nvarchar](max) NULL,
	[WeddingAnniversary] [datetime] NULL,
	[IsActiveForLoyalty] [bit] NOT NULL,
	[CustomerPicture] [varbinary](max) NULL,
	[CustomerPictureName] [nvarchar](max) NULL,
	[CustomerPictureType] [nvarchar](max) NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[CreditLimit] [decimal](18, 2) NOT NULL,
	[Outstanding] [decimal](18, 2) NOT NULL,
	[EPFNo] [varchar](50) NULL,
	[MembershipCardNo] [varchar](50) NULL,
	[Other] [varchar](50) NULL,
	[Remarks] [varchar](200) NULL,
	[CustomerStatus] [varchar](20) NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[SourceId] [int] NOT NULL,
	[Gender] [int] NOT NULL,
	[ReferenceNo1] [nvarchar](50) NULL,
	[ReferenceNo2] [nvarchar](50) NULL,
	[Age] [int] NOT NULL,
	[Religion] [int] NULL,
	[Race] [int] NULL,
	[LandMark] [nvarchar](50) NULL,
	[District] [nvarchar](50) NULL,
	[Organization] [nvarchar](50) NULL,
	[WorkAddres1] [nvarchar](50) NULL,
	[WorkAddres2] [nvarchar](50) NULL,
	[WorkAddres3] [nvarchar](50) NULL,
	[WorkEmail] [nvarchar](50) NULL,
	[WorkTelephone] [nvarchar](50) NULL,
	[WorkMobile] [nvarchar](50) NULL,
	[WorkFax] [nvarchar](50) NULL,
	[SpouseName] [nvarchar](50) NULL,
	[CivilStatus] [int] NOT NULL,
	[SpouseDateOfBirth] [datetime] NULL,
	[DeliverTo] [int] NOT NULL,
	[DeliverToAddress] [nvarchar](50) NULL,
	[Country] [nvarchar](50) NULL,
	[CustomerSince] [datetime] NULL,
	[SpecialDayType] [int] NOT NULL,
	[SendUpdatesViaEmail] [bit] NOT NULL,
	[SendUpdatesViaSms] [bit] NOT NULL,
	[IsRegByPOS] [bit] NOT NULL,
 CONSTRAINT [PK_dbo.LOGCustomers] PRIMARY KEY CLUSTERED 
(
	[CustomerID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion LOGCustomers

                #region LOGInvPromotionMasters
                tableName = "LOGInvPromotionMasters";
                query = @"CREATE TABLE [dbo].[LOGInvPromotionMasters](
	[InvPromotionMasterID] [bigint] IDENTITY(1,1) NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationID] [int] NOT NULL,
	[CostCentreID] [int] NOT NULL,
	[PromotionCode] [nvarchar](15) NOT NULL,
	[PromotionName] [nvarchar](50) NOT NULL,
	[IsAutoApply] [bit] NOT NULL,
	[PromotionTypeID] [int] NOT NULL,
	[StartDate] [datetime] NOT NULL,
	[EndDate] [datetime] NOT NULL,
	[IsMonday] [bit] NOT NULL,
	[IsTuesday] [bit] NOT NULL,
	[IsWednesday] [bit] NOT NULL,
	[IsThuresday] [bit] NOT NULL,
	[IsFriday] [bit] NOT NULL,
	[IsSaturday] [bit] NOT NULL,
	[IsSunday] [bit] NOT NULL,
	[IsMondayTime] [bit] NOT NULL,
	[IsTuesdayTime] [bit] NOT NULL,
	[IsWednesdayTime] [bit] NOT NULL,
	[IsThuresdayTime] [bit] NOT NULL,
	[IsFridayTime] [bit] NOT NULL,
	[IsSaturdayTime] [bit] NOT NULL,
	[IsSundayTime] [bit] NOT NULL,
	[MondayStartTime] [datetime] NULL,
	[MondayEndTime] [datetime] NULL,
	[TuesdayStartTime] [datetime] NULL,
	[TuesdayEndTime] [datetime] NULL,
	[WednesdayStartTime] [datetime] NULL,
	[WednesdayEndTime] [datetime] NULL,
	[ThuresdayStartTime] [datetime] NULL,
	[ThuresdayEndTime] [datetime] NULL,
	[FridayStartTime] [datetime] NULL,
	[FridayEndTime] [datetime] NULL,
	[SaturdayStartTime] [datetime] NULL,
	[SaturdayEndTime] [datetime] NULL,
	[SundayStartTime] [datetime] NULL,
	[SundayEndTime] [datetime] NULL,
	[PaymentMethodID] [int] NOT NULL,
	[IsProvider] [bit] NOT NULL,
	[IsAllLocations] [bit] NOT NULL,
	[IsAllType] [bit] NOT NULL,
	[IsValueRange] [bit] NOT NULL,
	[MinimumValue] [decimal](18, 2) NOT NULL,
	[MaximumValue] [decimal](18, 2) NOT NULL,
	[DiscountValue] [decimal](18, 2) NOT NULL,
	[DiscountPercentage] [decimal](18, 2) NOT NULL,
	[Points] [decimal](18, 2) NOT NULL,
	[Remark] [nvarchar](150) NULL,
	[DisplayMessage] [nvarchar](150) NULL,
	[CashierMessage] [nvarchar](150) NULL,
	[IsDelete] [bit] NOT NULL,
	[IsRaffle] [bit] NOT NULL,
	[IsIncreseQty] [bit] NOT NULL,
	[PromotionTypeNew] [int] NOT NULL,
	[SupplierID] [int] NOT NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[PromotionCount] [int] NOT NULL,
	[CustomerGroupId] [int] NULL,
	[SourceId] [int] NOT NULL,
	[CreateDate] [datetime] NULL,
	[CreateUser] [nvarchar](max) NULL,
	[ModifiedUser] [nvarchar](max) NULL,
	[IsActive] [bit] NOT NULL,
 CONSTRAINT [PK_dbo.LOGInvPromotionMasters] PRIMARY KEY CLUSTERED 
(
	[InvPromotionMasterID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion LOGInvPromotionMasters

                #region LOGProducts
                tableName = "LOGProducts";
                query = @"CREATE TABLE [dbo].[LOGProducts](
	[ProductId] [int] IDENTITY(1,1) NOT NULL,
	[ProductCode] [nvarchar](10) NOT NULL,
	[ProductName] [nvarchar](100) NOT NULL,
	[NameOnInvoice] [nvarchar](50) NULL,
	[IsPackItem] [bit] NOT NULL,
	[PackSize] [decimal](18, 2) NOT NULL,
	[PackPrice] [decimal](18, 2) NOT NULL,
	[IsPromotion] [bit] NOT NULL,
	[IsFreeIssue] [bit] NOT NULL,
	[IsExpiry] [bit] NOT NULL,
	[IsTaxInclude] [bit] NOT NULL,
	[IsTax] [bit] NOT NULL,
	[WeightPerUnit] [int] NOT NULL,
	[IsUnderCost] [bit] NOT NULL,
	[IsBundle] [bit] NOT NULL,
	[MaxPrice] [decimal](18, 2) NOT NULL,
	[MinPrice] [decimal](18, 2) NOT NULL,
	[DiscountPrecentage] [decimal](18, 2) NOT NULL,
	[MaximumDiscount] [decimal](18, 2) NOT NULL,
	[FixedDiscountPercentage] [decimal](18, 2) NOT NULL,
	[FixedDiscountAmount] [decimal](18, 2) NOT NULL,
	[MaximumDiscountPercentage] [decimal](18, 2) NOT NULL,
	[ProductNameInSinhala] [nvarchar](100) NULL,
	[IsRowMaterial] [bit] NOT NULL,
	[IsAddon] [bit] NOT NULL,
	[IsCountable] [bit] NOT NULL,
	[IsDiscount] [bit] NOT NULL,
	[IsCostOnReceipe] [bit] NOT NULL,
	[IsScaleItem] [bit] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[ProductImage] [varbinary](max) NULL,
	[ProductImageName] [nvarchar](max) NULL,
	[ProductImageType] [nvarchar](max) NULL,
	[DepartmentId] [int] NOT NULL,
	[CategoryId] [int] NOT NULL,
	[SubCategoryId] [int] NOT NULL,
	[CostPrice] [decimal](18, 2) NOT NULL,
	[SellingPrice] [decimal](18, 2) NOT NULL,
	[ReOrderLevel] [decimal](18, 2) NOT NULL,
	[ReOrderQuantity] [decimal](18, 2) NOT NULL,
	[LocationWiseStock] [decimal](18, 2) NOT NULL,
	[WastagePrc] [decimal](18, 2) NOT NULL,
	[PurchasingUnit] [int] NOT NULL,
	[Printer] [nvarchar](10) NULL,
	[Barcode] [nvarchar](200) NULL,
	[IsItemLock] [bit] NOT NULL,
	[RefCode01] [nvarchar](200) NULL,
	[RefCode02] [nvarchar](200) NULL,
	[PrinterTypeId] [int] NOT NULL,
	[AddonCategoryMasterId] [bigint] NULL,
	[IsOpenItem] [bit] NOT NULL,
	[AutoProduction] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[SourceId] [int] NOT NULL,
	[IsNoEffectCostforMenu] [bit] NOT NULL,
	[KitchenCode] [varchar](10) NOT NULL,
	[FastMovingGoods] [bit] NOT NULL,
 CONSTRAINT [PK_dbo.LOGProducts] PRIMARY KEY CLUSTERED 
(
	[ProductId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion LOGProducts

                #region LOGProductServingUnits
                tableName = "LOGProductServingUnits";
                query = @"CREATE TABLE [dbo].[LOGProductServingUnits](
	[ProductServingUnitId] [bigint] IDENTITY(1,1) NOT NULL,
	[ProductId] [bigint] NOT NULL,
	[ServingUnit] [nvarchar](max) NOT NULL,
	[CostPrice] [decimal](18, 2) NOT NULL,
	[SellingPrice] [decimal](18, 2) NOT NULL,
	[DeductStockOnRecipe] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[SourceId] [int] NOT NULL,
 CONSTRAINT [PK_dbo.LOGProductServingUnits] PRIMARY KEY CLUSTERED 
(
	[ProductServingUnitId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion LOGProductServingUnits

                #region LOGProductStockMasters
                tableName = "LOGProductStockMasters";
                query = @"CREATE TABLE [dbo].[LOGProductStockMasters](
	[ProductStockMasterId] [bigint] IDENTITY(1,1) NOT NULL,
	[CostCentreId] [int] NOT NULL,
	[ProductId] [bigint] NOT NULL,
	[StockCode] [nvarchar](25) NOT NULL,
	[Stock] [decimal](18, 2) NOT NULL,
	[CostPrice] [decimal](18, 2) NOT NULL,
	[SellingPrice] [decimal](18, 2) NOT NULL,
	[ForignCustomerPrice] [decimal](18, 2) NOT NULL,
	[ReOrderLevel] [decimal](18, 2) NOT NULL,
	[ReOrderQuantity] [decimal](18, 2) NOT NULL,
	[ReOrderPeriod] [decimal](18, 2) NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[ProductCode] [nvarchar](20) NULL,
	[ProductName] [nvarchar](100) NULL,
	[Barcode] [nvarchar](30) NULL,
	[RefNo1] [nvarchar](30) NULL,
	[RefNo2] [nvarchar](30) NULL,
	[ExtendedId] [int] NOT NULL,
	[ExtendedName] [nvarchar](30) NULL,
	[PLUCode] [nvarchar](5) NULL,
	[WeightPerunit] [decimal](18, 2) NOT NULL,
	[UomId] [int] NOT NULL,
	[Unit] [nvarchar](10) NULL,
	[AvgCost] [decimal](18, 2) NOT NULL,
	[FixedGP] [decimal](18, 2) NOT NULL,
	[GP] [decimal](18, 2) NOT NULL,
	[OpenBal] [decimal](18, 2) NOT NULL,
	[InitSIH] [decimal](18, 2) NOT NULL,
	[InitCost] [decimal](18, 2) NOT NULL,
	[AdjQty] [decimal](18, 2) NOT NULL,
	[IsDamage] [bit] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[IsBundle] [bit] NOT NULL,
	[IsInitialize] [bit] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[Ispacksize] [bit] NOT NULL,
	[Iscommission] [bit] NOT NULL,
	[Isdecimal] [bit] NOT NULL,
	[DiscountPrc] [decimal](18, 2) NOT NULL,
	[DocumentNo] [nvarchar](20) NULL,
	[LastUpdatedDate] [datetime] NULL,
	[MaximumDiscount] [decimal](18, 2) NOT NULL,
	[FixedDiscountPercentage] [decimal](18, 2) NOT NULL,
	[FixedDiscountAmount] [decimal](18, 2) NOT NULL,
	[MaximumDiscountPercentage] [decimal](18, 2) NOT NULL,
	[PrinterType_Id] [int] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[SourceId] [int] NOT NULL,
 CONSTRAINT [PK_dbo.LOGProductStockMasters] PRIMARY KEY CLUSTERED 
(
	[ProductStockMasterId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion LOGProductStockMasters

                #region LOGProductTaxes
                tableName = "LOGProductTaxes";
                query = @"CREATE TABLE [dbo].[LOGProductTaxes](
	[ProductTaxId] [bigint] IDENTITY(1,1) NOT NULL,
	[ProductId] [bigint] NOT NULL,
	[TaxId] [bigint] NOT NULL,
	[TaxPracentage] [decimal](18, 2) NOT NULL,
	[TaxSequence] [int] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[SourceId] [int] NOT NULL,
 CONSTRAINT [PK_dbo.LOGProductTaxes] PRIMARY KEY CLUSTERED 
(
	[ProductTaxId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion LOGProductTaxes

                #region LOGReceipes
                tableName = "LOGReceipes";
                query = @"CREATE TABLE [dbo].[LOGReceipes](
	[ReceipeId] [bigint] IDENTITY(1,1) NOT NULL,
	[ProductId] [bigint] NOT NULL,
	[MaterialId] [bigint] NOT NULL,
	[Quantity] [decimal](18, 2) NOT NULL,
	[ProductServingUnitId] [bigint] NOT NULL,
	[CostPrice] [decimal](18, 2) NOT NULL,
	[SellingPrice] [decimal](18, 2) NOT NULL,
	[ProductQty] [decimal](18, 2) NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[SourceId] [int] NOT NULL,
	[IsActive] [bit] NOT NULL,
 CONSTRAINT [PK_dbo.LOGReceipes] PRIMARY KEY CLUSTERED 
(
	[ReceipeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion LOGReceipes

                #region LOGSupplierProducts
                tableName = "LOGSupplierProducts";
                query = @"CREATE TABLE [dbo].[LOGSupplierProducts](
	[SupplierProductId] [bigint] IDENTITY(1,1) NOT NULL,
	[SupplierId] [int] NOT NULL,
	[ProductId] [int] NOT NULL,
	[IsPreferredSupplier] [bit] NOT NULL,
	[LastCostPrice] [decimal](18, 2) NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[SourceId] [int] NOT NULL,
 CONSTRAINT [PK_dbo.LOGSupplierProducts] PRIMARY KEY CLUSTERED 
(
	[SupplierProductId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion LOGSupplierProducts

                #region LOGSuppliers
                tableName = "LOGSuppliers";
                query = @"CREATE TABLE [dbo].[LOGSuppliers](
	[SupplierID] [bigint] IDENTITY(1,1) NOT NULL,
	[SupplierCode] [nvarchar](15) NOT NULL,
	[SupplierTitle] [nvarchar](max) NULL,
	[SupplierName] [nvarchar](100) NOT NULL,
	[Gender] [nvarchar](max) NOT NULL,
	[SupplierTypeID] [int] NOT NULL,
	[ContactPersonName] [nvarchar](100) NULL,
	[BillingAddress1] [nvarchar](250) NOT NULL,
	[BillingAddress2] [nvarchar](100) NULL,
	[BillingAddress3] [nvarchar](100) NULL,
	[BillingTelephone] [nvarchar](50) NOT NULL,
	[BillingMobile] [nvarchar](50) NULL,
	[BillingFax] [nvarchar](50) NULL,
	[Email] [nvarchar](100) NULL,
	[RepresentativeName] [nvarchar](100) NULL,
	[RepresentativeNICNo] [nvarchar](50) NULL,
	[PayeeName] [nvarchar](100) NULL,
	[DeliveryAddress1] [nvarchar](50) NULL,
	[DeliveryAddress2] [nvarchar](50) NULL,
	[DeliveryAddress3] [nvarchar](50) NULL,
	[DeliveryTelephone] [nvarchar](50) NULL,
	[DeliveryMobile] [nvarchar](50) NULL,
	[DeliveryFax] [nvarchar](50) NULL,
	[SupplierPicture] [varbinary](max) NULL,
	[SupplierPictureName] [nvarchar](max) NULL,
	[SupplierPictureType] [nvarchar](max) NULL,
	[ReferenceNo] [nvarchar](20) NULL,
	[ReferenceSerial] [nvarchar](20) NULL,
	[PostalCode] [nvarchar](20) NULL,
	[TaxID1] [int] NOT NULL,
	[TaxNo1] [nvarchar](25) NULL,
	[TaxID2] [int] NOT NULL,
	[TaxNo2] [nvarchar](25) NULL,
	[TaxID3] [int] NOT NULL,
	[TaxNo3] [nvarchar](25) NULL,
	[TaxID4] [int] NOT NULL,
	[TaxNo4] [nvarchar](25) NULL,
	[TaxID5] [int] NOT NULL,
	[TaxNo5] [nvarchar](25) NULL,
	[TaxRegistrationNo] [nvarchar](50) NULL,
	[TaxRegistrationName] [nvarchar](100) NULL,
	[PaymentMethod] [int] NOT NULL,
	[CreditLimit] [decimal](18, 2) NOT NULL,
	[ChequeLimit] [decimal](18, 2) NOT NULL,
	[ChequePeriod] [int] NOT NULL,
	[PaymentTermID] [int] NOT NULL,
	[CreditPeriod] [int] NOT NULL,
	[ProductBusinessType] [nvarchar](200) NULL,
	[SuppliedProducts] [nvarchar](200) NULL,
	[OrderCircle] [int] NOT NULL,
	[SupplierGroupID] [int] NOT NULL,
	[LedgerID] [bigint] NOT NULL,
	[OtherLedgerID] [bigint] NOT NULL,
	[TaxIdNo] [nvarchar](50) NULL,
	[DepositeAmount] [decimal](18, 2) NOT NULL,
	[EmailBoday] [nvarchar](100) NULL,
	[EmailSubject] [nvarchar](100) NULL,
	[Remark] [nvarchar](100) NULL,
	[IsUpload] [bit] NOT NULL,
	[IsSuspended] [bit] NOT NULL,
	[IsPOMail] [bit] NOT NULL,
	[IsBlocked] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[SourceId] [int] NOT NULL,
 CONSTRAINT [PK_dbo.LOGSuppliers] PRIMARY KEY CLUSTERED 
(
	[SupplierID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion LOGSuppliers

                #region LOGUnitConversions
                tableName = "LOGUnitConversions";
                query = @"CREATE TABLE [dbo].[LOGUnitConversions](
	[UnitConversionId] [bigint] IDENTITY(1,1) NOT NULL,
	[UnitOfMeasureId] [bigint] NOT NULL,
	[SubUnit] [nvarchar](max) NOT NULL,
	[BaseUnitValue] [decimal](18, 2) NOT NULL,
	[SubUnitValue] [decimal](18, 2) NOT NULL,
	[SubUnitSymbol] [nvarchar](max) NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[SourceId] [int] NOT NULL,
	[Action] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.LOGUnitConversions] PRIMARY KEY CLUSTERED 
(
	[UnitConversionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion LOGUnitConversions

                #region LoyaltyCardGenerationDetails
                tableName = "LoyaltyCardGenerationDetails";
                query = @"CREATE TABLE [dbo].[LoyaltyCardGenerationDetails](
	[LoyaltyCardGenerationDetailId] [bigint] IDENTITY(1,1) NOT NULL,
	[CardGenerationDetailID] [bigint] NOT NULL,
	[LoyaltyCardGenerationHeaderID] [bigint] NOT NULL,
	[CardPrefix] [nvarchar](10) NULL,
	[CardLength] [int] NOT NULL,
	[CardStartingNo] [int] NOT NULL,
	[EncodeLength] [int] NOT NULL,
	[EncodeStartingNo] [int] NOT NULL,
	[EncodePrefix] [nvarchar](3) NULL,
	[GeneratedDate] [datetime] NOT NULL,
	[CardNo] [nvarchar](50) NULL,
	[EncodeNo] [nvarchar](50) NULL,
	[IsIssued] [bit] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[RefCardNo1] [nvarchar](50) NULL,
	[RefCardNo2] [nvarchar](50) NULL,
 CONSTRAINT [PK_dbo.LoyaltyCardGenerationDetails] PRIMARY KEY CLUSTERED 
(
	[LoyaltyCardGenerationDetailId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion LoyaltyCardGenerationDetails

                #region LoyaltyCardGenerationHeaders
                tableName = "LoyaltyCardGenerationHeaders";
                query = @"CREATE TABLE [dbo].[LoyaltyCardGenerationHeaders](
	[LoyaltyCardGenerationHeaderId] [bigint] IDENTITY(1,1) NOT NULL,
	[CardGenerationHeaderID] [bigint] NOT NULL,
	[CardPrefix] [nvarchar](10) NULL,
	[CardLength] [int] NOT NULL,
	[CardStartingNo] [int] NOT NULL,
	[EncodeLength] [int] NOT NULL,
	[EncodeStartingNo] [int] NOT NULL,
	[EncodePrefix] [nvarchar](3) NULL,
	[GeneratedDate] [datetime] NOT NULL,
	[CardMasterId] [bigint] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 CONSTRAINT [PK_dbo.LoyaltyCardGenerationHeaders] PRIMARY KEY CLUSTERED 
(
	[LoyaltyCardGenerationHeaderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion LoyaltyCardGenerationHeaders

                #region LoyaltyCardIssueDetails
                tableName = "LoyaltyCardIssueDetails";
                query = @"CREATE TABLE [dbo].[LoyaltyCardIssueDetails](
	[LoyaltyCardIssueDetailId] [bigint] IDENTITY(1,1) NOT NULL,
	[CardIssueDetailID] [bigint] NOT NULL,
	[ToLocationID] [int] NOT NULL,
	[IssueDate] [datetime] NOT NULL,
	[CardNo] [nvarchar](max) NULL,
	[EncodeNo] [nvarchar](max) NULL,
	[IsIssued] [bit] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[FefCardNo1] [nvarchar](50) NULL,
	[FefCardNo2] [nvarchar](50) NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[LoyaltyCardIssueHeaderId] [int] NOT NULL,
 CONSTRAINT [PK_dbo.LoyaltyCardIssueDetails] PRIMARY KEY CLUSTERED 
(
	[LoyaltyCardIssueDetailId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion LoyaltyCardIssueDetails

                #region LoyaltyCardIssueHeaders
                tableName = "LoyaltyCardIssueHeaders";
                query = @"CREATE TABLE [dbo].[LoyaltyCardIssueHeaders](
	[LoyaltyCardIssueHeaderId] [int] IDENTITY(1,1) NOT NULL,
	[CardIssueHeaderID] [bigint] NOT NULL,
	[IssueDate] [datetime] NOT NULL,
	[ToLocationID] [int] NOT NULL,
	[DocumentNo] [nvarchar](50) NULL,
	[Remark] [nvarchar](50) NULL,
	[ReferenceNo] [nvarchar](max) NULL,
	[EmployeeID] [int] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 CONSTRAINT [PK_dbo.LoyaltyCardIssueHeaders] PRIMARY KEY CLUSTERED 
(
	[LoyaltyCardIssueHeaderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion LoyaltyCardIssueHeaders

                #region LoyaltyCardSchems
                tableName = "LoyaltyCardSchems";
                query = @"CREATE TABLE [dbo].[LoyaltyCardSchems](
	[LoyaltyCardSchemsID] [bigint] IDENTITY(1,1) NOT NULL,
	[CardMasterId] [bigint] NOT NULL,
	[BillFromValue] [decimal](18, 2) NOT NULL,
	[BillToValue] [decimal](18, 2) NOT NULL,
	[Increment] [decimal](18, 2) NOT NULL,
	[PointValue] [decimal](18, 2) NOT NULL,
	[PointPer] [decimal](18, 2) NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 CONSTRAINT [PK_dbo.LoyaltyCardSchems] PRIMARY KEY CLUSTERED 
(
	[LoyaltyCardSchemsID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion LoyaltyCardSchems

                #region LoyaltyCustomers
                tableName = "LoyaltyCustomers";
                query = @"CREATE TABLE [dbo].[LoyaltyCustomers](
	[LoyaltyCustomerId] [int] IDENTITY(1,1) NOT NULL,
	[CardNo] [nvarchar](50) NULL,
	[CustomerId] [bigint] NOT NULL,
	[NameOnCard] [nvarchar](50) NULL,
	[CardMasterId] [bigint] NOT NULL,
	[CardIssued] [bit] NOT NULL,
	[IssuedOn] [datetime] NOT NULL,
	[ExpiryDate] [datetime] NOT NULL,
	[RenewedOn] [datetime] NOT NULL,
	[LedgerId] [bigint] NOT NULL,
	[LedgerId2] [bigint] NOT NULL,
	[CreditLimit] [decimal](18, 2) NOT NULL,
	[CreditPeriod] [int] NOT NULL,
	[CPoints] [decimal](18, 2) NOT NULL,
	[EPoints] [decimal](18, 2) NOT NULL,
	[RPoints] [decimal](18, 2) NOT NULL,
	[IsReDimm] [bit] NOT NULL,
	[AcitiveDate] [datetime] NOT NULL,
	[LocationID] [int] NOT NULL,
	[CashierID] [int] NOT NULL,
	[LoyaltyType] [int] NOT NULL,
	[Remark] [nvarchar](200) NULL,
	[SystemGeneratedCode] [nvarchar](15) NULL,
	[ExpiryPoints] [decimal](18, 2) NOT NULL,
	[IsSold] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[ExpiryPoints1] [decimal](18, 2) NOT NULL,
	[Discount] [decimal](18, 2) NOT NULL,
	[SalesPersonCode] [nvarchar](10) NULL,
	[LastUpdatedLocId] [int] NOT NULL,
	[Status] [int] NOT NULL,
	[IsCardIssued] [int] NOT NULL,
	[CompanyId] [int] NOT NULL,
 CONSTRAINT [PK_dbo.LoyaltyCustomers] PRIMARY KEY CLUSTERED 
(
	[LoyaltyCustomerId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion LoyaltyCustomers

                #region MonthEnds
                tableName = "MonthEnds";
                query = @"CREATE TABLE [dbo].[MonthEnds](
	[MonthEndId] [bigint] IDENTITY(1,1) NOT NULL,
	[LocationId] [int] NOT NULL,
	[LocYear] [int] NOT NULL,
	[LocMonth] [int] NOT NULL,
	[LocMonthDesc] [nvarchar](50) NULL,
	[LocStatus] [bit] NOT NULL,
	[LocIsClose] [bit] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NULL,
	[DataTransfer] [int] NOT NULL,
	[CompanyId] [int] NOT NULL,
 CONSTRAINT [PK_dbo.MonthEnds] PRIMARY KEY CLUSTERED 
(
	[MonthEndId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion MonthEnds

                #region PaidInTypes
                tableName = "PaidInTypes";
                query = @"CREATE TABLE [dbo].[PaidInTypes](
	[PaidInTypeId] [int] IDENTITY(1,1) NOT NULL,
	[Code] [nvarchar](max) NULL,
	[Description] [nvarchar](max) NULL,
	[IsSalesSummery] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[GroupOfCompanyId] [int] NOT NULL,
	[CreatedUser] [nvarchar](max) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](max) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 CONSTRAINT [PK_dbo.PaidInTypes] PRIMARY KEY CLUSTERED 
(
	[PaidInTypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion PaidInTypes

                #region PaidOutTypes
                tableName = "PaidOutTypes";
                query = @"CREATE TABLE [dbo].[PaidOutTypes](
	[PaidOutTypeId] [int] NOT NULL,
	[Code] [nvarchar](max) NULL,
	[Description] [nvarchar](max) NULL,
	[IsSalesSummery] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[DayFrom] [int] IDENTITY(1,1) NOT NULL,
	[DayTo] [int] NOT NULL,
	[GroupOfCompanyId] [int] NOT NULL,
	[CreatedUser] [nvarchar](max) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](max) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 CONSTRAINT [PK_dbo.PaidOutTypes] PRIMARY KEY CLUSTERED 
(
	[PaidOutTypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion PaidOutTypes

                #region PaymentDets
                tableName = "PaymentDets";
                query = @"CREATE TABLE [dbo].[PaymentDets](
	[PaymentDetID] [int] IDENTITY(1,1) NOT NULL,
	[RowNo] [int] NOT NULL,
	[PayTypeID] [int] NOT NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[Balance] [decimal](18, 2) NOT NULL,
	[SDate] [datetime] NOT NULL,
	[Receipt] [nvarchar](max) NULL,
	[LocationID] [int] NOT NULL,
	[CashierID] [int] NOT NULL,
	[UnitNo] [int] NOT NULL,
	[BillTypeID] [int] NOT NULL,
	[SaleTypeID] [int] NOT NULL,
	[RefNo] [nvarchar](max) NULL,
	[BankId] [int] NOT NULL,
	[ChequeDate] [datetime] NULL,
	[IsRecallAdv] [int] NOT NULL,
	[RecallNo] [nvarchar](max) NULL,
	[Descrip] [nvarchar](max) NULL,
	[EnCodeName] [nvarchar](max) NULL,
	[UpdatedBy] [int] NOT NULL,
	[Status] [int] NOT NULL,
	[ZNo] [int] NOT NULL,
	[CustomerId] [int] NOT NULL,
	[CustomerType] [int] NOT NULL,
	[CustomerCode] [nvarchar](max) NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[Datatransfer] [int] NOT NULL,
	[ZDate] [datetime] NOT NULL,
	[TerminalID] [int] NOT NULL,
	[LoyaltyType] [int] NOT NULL,
	[IsUploadToGL] [int] NOT NULL,
	[LocationIDBilling] [int] NOT NULL,
	[TableID] [int] NOT NULL,
	[TicketID] [int] NOT NULL,
	[OrderNo] [int] NOT NULL,
	[ShiftNo] [int] NOT NULL,
	[IsDayEnd] [bit] NOT NULL,
	[UpdateUnitNo] [int] NOT NULL,
	[Online] [int] NOT NULL,
	[SerialNo] [nvarchar](max) NULL,
	[CurrencyCode] [nvarchar](max) NULL,
	[CurrencyRate] [decimal](18, 2) NOT NULL,
	[AcountNumber] [nvarchar](max) NULL,
	[CopperratePrice] [decimal](18, 2) NOT NULL,
	[IsCopperratePriceEnable] [bit] NOT NULL,
	[AmountCopperratePrice] [decimal](18, 2) NOT NULL,
	[BalanceCopperratePrice] [decimal](18, 2) NOT NULL,
	[AdvancePayTypeID] [int] NOT NULL,
	[AdvancePayRefNo] [varchar](30) NULL,
	[IsGLTransfer] [int] NOT NULL,
 CONSTRAINT [PK_dbo.PaymentDets] PRIMARY KEY CLUSTERED 
(
	[PaymentDetID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion PaymentDets

                #region PaymentMethods
                tableName = "PaymentMethods";
                query = @"CREATE TABLE [dbo].[PaymentMethods](
	[PaymentMethodId] [bigint] IDENTITY(1,1) NOT NULL,
	[PaymentMethodCode] [nvarchar](max) NOT NULL,
	[PaymentMethodName] [nvarchar](max) NOT NULL,
	[CommissionRate] [decimal](18, 2) NOT NULL,
	[PaymentType] [decimal](18, 2) NOT NULL,
	[IsPaymentType] [bit] NOT NULL,
	[IsReceiptType] [bit] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 CONSTRAINT [PK_dbo.PaymentMethods] PRIMARY KEY CLUSTERED 
(
	[PaymentMethodId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion PaymentMethods

                #region PaymentTerms
                tableName = "PaymentTerms";
                query = @"CREATE TABLE [dbo].[PaymentTerms](
	[PaymenttermId] [bigint] IDENTITY(1,1) NOT NULL,
	[PaymentTermCode] [nvarchar](max) NOT NULL,
	[PaymentTermName] [nvarchar](max) NOT NULL,
	[CreditPeriod] [int] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 CONSTRAINT [PK_dbo.PaymentTerms] PRIMARY KEY CLUSTERED 
(
	[PaymenttermId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion PaymentTerms

                #region PayTypes
                tableName = "PayTypes";
                query = @"CREATE TABLE [dbo].[PayTypes](
	[PaymentID] [int] NOT NULL,
	[Descrip] [nvarchar](max) NULL,
	[IsSwipe] [bit] NOT NULL,
	[Type] [int] NOT NULL,
	[Rate] [decimal](18, 2) NOT NULL,
	[IsRefundable] [bit] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[IsBillCopy] [bit] NOT NULL,
	[PrintDescrip] [nvarchar](max) NULL,
	[PreFix] [nvarchar](max) NULL,
	[MaxLength] [int] NOT NULL,
 CONSTRAINT [PK_dbo.PayTypes] PRIMARY KEY CLUSTERED 
(
	[PaymentID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion PayTypes

                #region PayTypeTaxes
                tableName = "PayTypeTaxes";
                query = @"CREATE TABLE [dbo].[PayTypeTaxes](
	[PayTypeTaxId] [bigint] IDENTITY(1,1) NOT NULL,
	[PayTypeId] [bigint] NOT NULL,
	[TaxId] [bigint] NOT NULL,
	[TaxPracentage] [decimal](18, 2) NOT NULL,
	[TaxSequence] [int] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 CONSTRAINT [PK_dbo.PayTypeTaxes] PRIMARY KEY CLUSTERED 
(
	[PayTypeTaxId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion PayTypeTaxes

                #region PointsExpirations
                tableName = "PointsExpirations";
                query = @"CREATE TABLE [dbo].[PointsExpirations](
	[PointsExpirationId] [int] IDENTITY(1,1) NOT NULL,
	[Year] [int] NOT NULL,
	[CardType] [int] NOT NULL,
	[FirstReminderMessage] [nvarchar](max) NOT NULL,
	[FirstReminderDate] [datetime] NOT NULL,
	[SecontReminderMessage] [nvarchar](max) NULL,
	[SecondReminderDate] [datetime] NOT NULL,
	[PointsExpiryDate] [datetime] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 CONSTRAINT [PK_dbo.PointsExpirations] PRIMARY KEY CLUSTERED 
(
	[PointsExpirationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion PointsExpirations

                #region PointsExpirationSchedules
                tableName = "PointsExpirationSchedules";
                query = @"CREATE TABLE [dbo].[PointsExpirationSchedules](
	[Idx] [int] IDENTITY(1,1) NOT NULL,
	[Type] [int] NOT NULL,
	[SQL] [nvarchar](max) NULL,
	[user] [nvarchar](15) NULL,
	[Date] [datetime] NOT NULL,
	[ScheduleDate] [datetime] NOT NULL,
	[Status] [int] NOT NULL,
	[EndDate] [datetime] NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[PointsExpirationId] [int] NOT NULL,
 CONSTRAINT [PK_dbo.PointsExpirationSchedules] PRIMARY KEY CLUSTERED 
(
	[Idx] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion PointsExpirationSchedules

                #region PointsExpirationTypes
                tableName = "PointsExpirationTypes";
                query = @"CREATE TABLE [dbo].[PointsExpirationTypes](
	[PointsExpirationTypeId] [int] IDENTITY(1,1) NOT NULL,
	[Desc] [nvarchar](max) NULL,
	[IsActive] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 CONSTRAINT [PK_dbo.PointsExpirationTypes] PRIMARY KEY CLUSTERED 
(
	[PointsExpirationTypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion PointsExpirationTypes

                #region POProductTaxes
                tableName = "POProductTaxes";
                query = @"CREATE TABLE [dbo].[POProductTaxes](
	[POProductTaxId] [bigint] IDENTITY(1,1) NOT NULL,
	[PRoductId] [bigint] NOT NULL,
	[TaxId] [bigint] NOT NULL,
	[TaxPrecentage] [decimal](18, 2) NOT NULL,
	[PurchaseOrderHeaderId] [bigint] NOT NULL,
 CONSTRAINT [PK_dbo.POProductTaxes] PRIMARY KEY CLUSTERED 
(
	[POProductTaxId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion POProductTaxes

                #region POSUserGroups
                tableName = "POSUserGroups";
                query = @"CREATE TABLE [dbo].[POSUserGroups](
	[POSUserGroupId] [int] IDENTITY(1,1) NOT NULL,
	[POSUserGroupName] [nvarchar](max) NOT NULL,
	[POSUserGroupDesc] [nvarchar](max) NOT NULL,
	[CreatedUser] [nvarchar](max) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](max) NOT NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[CompanyId] [int] NOT NULL,
 CONSTRAINT [PK_dbo.POSUserGroups] PRIMARY KEY CLUSTERED 
(
	[POSUserGroupId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion POSUserGroups

                #region PriceLevels
                tableName = "PriceLevels";
                query = @"CREATE TABLE [dbo].[PriceLevels](
	[PriceLevelId] [bigint] IDENTITY(1,1) NOT NULL,
	[ProductId] [bigint] NOT NULL,
	[CostPrice] [decimal](18, 2) NOT NULL,
	[SellingPrice] [decimal](18, 2) NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[Qty] [decimal](18, 2) NOT NULL,
	[LocationId] [int] NOT NULL,
	[DocumentId] [int] NOT NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 CONSTRAINT [PK_dbo.PriceLevels] PRIMARY KEY CLUSTERED 
(
	[PriceLevelId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion PriceLevels

                #region PrinterTypes
                tableName = "PrinterTypes";
                query = @"CREATE TABLE [dbo].[PrinterTypes](
	[PrinterTypeId] [int] IDENTITY(1,1) NOT NULL,
	[PrinterTypeName] [nvarchar](max) NULL,
	[IsDelete] [bit] NOT NULL,
 CONSTRAINT [PK_dbo.PrinterTypes] PRIMARY KEY CLUSTERED 
(
	[PrinterTypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion PrinterTypes

                #region PRODUCT$
                tableName = "PRODUCT$";
                query = @"CREATE TABLE [dbo].[PRODUCT$](
	[ProductCode] [nvarchar](255) NULL,
	[ProductName] [nvarchar](255) NULL,
	[NameOnInvoice] [nvarchar](255) NULL,
	[Is Row Material] [nvarchar](255) NULL,
	[Is Countable] [nvarchar](255) NULL,
	[Is Scale Item] [nvarchar](255) NULL,
	[Is Active] [nvarchar](255) NULL,
	[Is Delete] [nvarchar](255) NULL,
	[DepartmentId] [nvarchar](255) NULL,
	[CategoryId] [nvarchar](255) NULL,
	[Cat Descri] [nvarchar](255) NULL,
	[SubCategoryId] [nvarchar](255) NULL,
	[CostPrice] [nvarchar](255) NULL,
	[SellingPrice] [float] NULL,
	[ReOrderLevel] [nvarchar](255) NULL,
	[ReOrderQuantity] [nvarchar](255) NULL,
	[LocationWiseStock] [nvarchar](255) NULL,
	[Barcode] [nvarchar](255) NULL,
	[IsItemLock] [nvarchar](255) NULL,
	[GroupOfCompanyID] [nvarchar](255) NULL,
	[CompanyID] [nvarchar](255) NULL,
	[LocationId] [nvarchar](255) NULL,
	[RefCode01] [nvarchar](255) NULL,
	[RefCode02] [nvarchar](255) NULL,
	[WastagePrc] [nvarchar](255) NULL,
	[PurchasingUnit] [nvarchar](255) NULL,
	[IsDiscount] [nvarchar](255) NULL,
	[IsCostOnReceipe] [nvarchar](255) NULL,
	[IsAddon] [nvarchar](255) NULL,
	[IsPackItem] [nvarchar](255) NULL,
	[PackSize] [nvarchar](255) NULL,
	[PackPrice] [nvarchar](255) NULL,
	[IsPromotion] [nvarchar](255) NULL,
	[IsFreeIssue] [nvarchar](255) NULL,
	[IsExpiry] [nvarchar](255) NULL,
	[IsTax] [nvarchar](255) NULL,
	[WeightPerUnit] [nvarchar](255) NULL,
	[IsUnderCost] [nvarchar](255) NULL,
	[IsBundle] [nvarchar](255) NULL,
	[MaxPrice] [nvarchar](255) NULL,
	[MinPrice] [nvarchar](255) NULL,
	[DiscountPrecentage] [nvarchar](255) NULL,
	[MaximumDiscount] [nvarchar](255) NULL,
	[FixedDiscountPercentage] [nvarchar](255) NULL,
	[FixedDiscountAmount] [nvarchar](255) NULL,
	[MaximumDiscountPercentage] [nvarchar](255) NULL,
	[AddonCategoryMasterId] [nvarchar](255) NULL,
	[IsTaxInclude] [nvarchar](255) NULL,
	[IsOpenItem] [nvarchar](255) NULL,
	[Serving Unit] [nvarchar](255) NULL,
	[Deduct Stock On Recipe] [nvarchar](255) NULL
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion PRODUCT$

                #region Product_Master$
                tableName = "Product_Master$";
                query = @"CREATE TABLE [dbo].[Product_Master$](
	[ProductId] [nvarchar](255) NULL,
	[ProductCode] [nvarchar](255) NULL,
	[ProductName] [nvarchar](255) NULL,
	[NameOnInvoice] [nvarchar](255) NULL,
	[IsRowMaterial] [float] NULL,
	[IsCountable] [float] NULL,
	[IsScaleItem] [float] NULL,
	[IsActive] [float] NULL,
	[IsDelete] [float] NULL,
	[DepartmentId] [float] NULL,
	[CategoryId] [float] NULL,
	[CatDescri] [nvarchar](255) NULL,
	[SubCategoryId] [float] NULL,
	[CostPrice] [nvarchar](255) NULL,
	[SellingPrice] [decimal](18, 0) NULL,
	[ReOrderLevel] [nvarchar](255) NULL,
	[ReOrderQuantity] [nvarchar](255) NULL,
	[LocationWiseStock] [nvarchar](255) NULL,
	[Barcode] [nvarchar](255) NULL,
	[IsItemLock] [nvarchar](255) NULL,
	[GroupOfCompanyID] [nvarchar](255) NULL,
	[CompanyID] [nvarchar](255) NULL,
	[LocationId] [nvarchar](255) NULL,
	[RefCode01] [nvarchar](255) NULL,
	[RefCode02] [nvarchar](255) NULL,
	[WastagePrc] [nvarchar](255) NULL,
	[PurchasingUnit] [float] NULL,
	[IsDiscount] [nvarchar](255) NULL,
	[IsCostOnReceipe] [nvarchar](255) NULL,
	[IsAddon] [nvarchar](255) NULL,
	[IsPackItem] [float] NULL,
	[PackSize] [nvarchar](255) NULL,
	[PackPrice] [nvarchar](255) NULL,
	[IsPromotion] [nvarchar](255) NULL,
	[IsFreeIssue] [nvarchar](255) NULL,
	[IsExpiry] [nvarchar](255) NULL,
	[IsTax] [float] NULL,
	[WeightPerUnit] [nvarchar](255) NULL,
	[IsUnderCost] [nvarchar](255) NULL,
	[IsBundle] [nvarchar](255) NULL,
	[MaxPrice] [nvarchar](255) NULL,
	[MinPrice] [nvarchar](255) NULL,
	[DiscountPrecentage] [nvarchar](255) NULL,
	[MaximumDiscount] [nvarchar](255) NULL,
	[FixedDiscountPercentage] [nvarchar](255) NULL,
	[FixedDiscountAmount] [nvarchar](255) NULL,
	[MaximumDiscountPercentage] [nvarchar](255) NULL,
	[AddonCategoryMasterId] [nvarchar](255) NULL,
	[IsTaxInclude] [nvarchar](255) NULL,
	[IsOpenItem] [nvarchar](255) NULL,
	[Serving Unit] [nvarchar](255) NULL,
	[DeductStockOnRecipe] [float] NULL
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion Product_Master$

                #region ProductGroupDetails
                tableName = "ProductGroupDetails";
                query = @"CREATE TABLE [dbo].[ProductGroupDetails](
	[ProductGroupDetailID] [int] IDENTITY(1,1) NOT NULL,
	[ProductGroupCode] [nvarchar](max) NULL,
	[ProductGroupHeaderID] [int] NOT NULL,
	[ProductID] [int] NOT NULL,
	[ProductCode] [nvarchar](max) NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 CONSTRAINT [PK_dbo.ProductGroupDetails] PRIMARY KEY CLUSTERED 
(
	[ProductGroupDetailID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion ProductGroupDetails

                #region ProductGroupHeaders
                tableName = "ProductGroupHeaders";
                query = @"CREATE TABLE [dbo].[ProductGroupHeaders](
	[ProductGroupHeaderID] [int] IDENTITY(1,1) NOT NULL,
	[ProductGroupCode] [nvarchar](max) NOT NULL,
	[ProductGroupName] [nvarchar](max) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 CONSTRAINT [PK_dbo.ProductGroupHeaders] PRIMARY KEY CLUSTERED 
(
	[ProductGroupHeaderID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion ProductGroupHeaders

                #region ProductInstructions
                tableName = "ProductInstructions";
                query = @"CREATE TABLE [dbo].[ProductInstructions](
	[ProductInstructionId] [bigint] IDENTITY(1,1) NOT NULL,
	[InstructionList] [nvarchar](max) NULL,
	[ProductId] [bigint] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[CompanyId] [int] NOT NULL,
 CONSTRAINT [PK_dbo.ProductInstructions] PRIMARY KEY CLUSTERED 
(
	[ProductInstructionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion ProductInstructions

                #region ProductionNoteDetails
                tableName = "ProductionNoteDetails";
                query = @"CREATE TABLE [dbo].[ProductionNoteDetails](
	[ProductionNoteDetailId] [bigint] IDENTITY(1,1) NOT NULL,
	[ProductionNoteHeaderId] [bigint] NOT NULL,
	[MaterialId] [bigint] NOT NULL,
	[MaterialName] [nvarchar](max) NULL,
	[MaterialQty] [decimal](18, 2) NOT NULL,
	[SellingPrice] [decimal](18, 2) NOT NULL,
	[CostPrice] [decimal](18, 2) NOT NULL,
	[AvgCost] [decimal](18, 2) NOT NULL,
	[ProductId] [bigint] NOT NULL,
	[ProductQty] [decimal](18, 2) NOT NULL,
	[ProductName] [nvarchar](max) NULL,
	[ProductCostPrice] [decimal](18, 2) NOT NULL,
	[ProductSellingPrice] [decimal](18, 2) NOT NULL,
	[ServingUnitId] [bigint] NOT NULL,
	[ActualQty] [decimal](18, 2) NOT NULL,
 CONSTRAINT [PK_dbo.ProductionNoteDetails] PRIMARY KEY CLUSTERED 
(
	[ProductionNoteDetailId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion ProductionNoteDetails

                #region ProductionNoteHeaders
                tableName = "ProductionNoteHeaders";
                query = @"CREATE TABLE [dbo].[ProductionNoteHeaders](
	[ProductionNoteHeaderId] [bigint] IDENTITY(1,1) NOT NULL,
	[DocumentNo] [nvarchar](15) NOT NULL,
	[ProductionLocId] [bigint] NOT NULL,
	[Remark] [nvarchar](200) NULL,
	[ProductId] [bigint] NOT NULL,
	[ProductCostPrice] [decimal](18, 2) NOT NULL,
	[ProductSellingPrice] [decimal](18, 2) NOT NULL,
	[ProductDiscounts] [decimal](18, 2) NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[ProductQty] [decimal](18, 2) NOT NULL,
	[IsTempPN] [bit] NOT NULL,
	[IsFinished] [bit] NOT NULL,
	[DocumentId] [int] NOT NULL,
	[ReceiptLocID] [int] NOT NULL,
	[R_Zno] [int] NOT NULL,
	[ReceiptNo] [nvarchar](40) NOT NULL,
	[UnitNo] [int] NOT NULL,
 CONSTRAINT [PK_dbo.ProductionNoteHeaders] PRIMARY KEY CLUSTERED 
(
	[ProductionNoteHeaderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion ProductionNoteHeaders

                #region ProductKitchenMappers
                tableName = "ProductKitchenMappers";
                query = @"CREATE TABLE [dbo].[ProductKitchenMappers](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ProductId] [int] NOT NULL,
	[SubLocationId] [int] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NULL,
	[DataTransfer] [int] NOT NULL,
	[IsActive] [bit] NOT NULL,
 CONSTRAINT [PK_ProductKitchenMapper] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion ProductKitchenMappers

                #region Products
                tableName = "Products";
                query = @"CREATE TABLE [dbo].[Products](
	[ProductId] [int] IDENTITY(1,1) NOT NULL,
	[ProductCode] [nvarchar](20) NOT NULL,
	[ProductName] [nvarchar](100) NOT NULL,
	[ProductNameInSinhala] [nvarchar](100) NULL,
	[IsRowMaterial] [bit] NULL,
	[IsCountable] [bit] NOT NULL,
	[IsScaleItem] [bit] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[ProductImage] [varbinary](max) NULL,
	[ProductImageName] [nvarchar](max) NULL,
	[ProductImageType] [nvarchar](max) NULL,
	[DepartmentId] [int] NOT NULL,
	[CategoryId] [int] NOT NULL,
	[SubCategoryId] [int] NOT NULL,
	[CostPrice] [decimal](18, 2) NOT NULL,
	[SellingPrice] [decimal](18, 2) NOT NULL,
	[ReOrderLevel] [decimal](18, 2) NOT NULL,
	[ReOrderQuantity] [decimal](18, 2) NOT NULL,
	[LocationWiseStock] [decimal](18, 2) NOT NULL,
	[Printer] [nvarchar](10) NULL,
	[Barcode] [nvarchar](200) NULL,
	[IsItemLock] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[RefCode01] [nvarchar](200) NULL,
	[RefCode02] [nvarchar](200) NULL,
	[WastagePrc] [decimal](18, 2) NOT NULL,
	[PurchasingUnit] [int] NOT NULL,
	[IsDiscount] [bit] NOT NULL,
	[IsCostOnReceipe] [bit] NOT NULL,
	[IsAddon] [bit] NOT NULL,
	[NameOnInvoice] [nvarchar](50) NULL,
	[IsPackItem] [bit] NOT NULL,
	[PackSize] [decimal](18, 2) NOT NULL,
	[PackPrice] [decimal](18, 2) NOT NULL,
	[IsPromotion] [bit] NOT NULL,
	[IsFreeIssue] [bit] NOT NULL,
	[IsExpiry] [bit] NOT NULL,
	[IsTax] [bit] NOT NULL,
	[WeightPerUnit] [int] NOT NULL,
	[IsUnderCost] [bit] NOT NULL,
	[IsBundle] [bit] NOT NULL,
	[MaxPrice] [decimal](18, 2) NOT NULL,
	[MinPrice] [decimal](18, 2) NOT NULL,
	[DiscountPrecentage] [decimal](18, 2) NOT NULL,
	[MaximumDiscount] [decimal](18, 2) NOT NULL,
	[FixedDiscountPercentage] [decimal](18, 2) NOT NULL,
	[FixedDiscountAmount] [decimal](18, 2) NOT NULL,
	[MaximumDiscountPercentage] [decimal](18, 2) NOT NULL,
	[PrinterTypeId] [int] NOT NULL,
	[AddonCategoryMasterId] [bigint] NULL,
	[IsTaxInclude] [bit] NOT NULL,
	[IsOpenItem] [bit] NOT NULL,
	[AutoProduction] [bit] NOT NULL,
	[IsNoEffectCostforMenu] [bit] NOT NULL,
	[KitchenCode] [varchar](10) NOT NULL,
	[FastMovingGoods] [bit] NOT NULL,
 CONSTRAINT [PK_dbo.Products] PRIMARY KEY CLUSTERED 
(
	[ProductId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion Products

                #region Products_Temp
                tableName = "Products_Temp";
                query = @"CREATE TABLE [dbo].[Products_Temp](
	[ProductId] [int] IDENTITY(1,1) NOT NULL,
	[ProductCode] [nvarchar](20) NOT NULL,
	[ProductName] [nvarchar](100) NOT NULL,
	[ProductNameInSinhala] [nvarchar](100) NULL,
	[IsRowMaterial] [bit] NULL,
	[IsCountable] [bit] NOT NULL,
	[IsScaleItem] [bit] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[ProductImage] [varbinary](max) NULL,
	[ProductImageName] [nvarchar](max) NULL,
	[ProductImageType] [nvarchar](max) NULL,
	[DepartmentId] [int] NOT NULL,
	[CategoryId] [int] NOT NULL,
	[SubCategoryId] [int] NOT NULL,
	[CostPrice] [decimal](18, 2) NOT NULL,
	[SellingPrice] [decimal](18, 2) NOT NULL,
	[ReOrderLevel] [decimal](18, 2) NOT NULL,
	[ReOrderQuantity] [decimal](18, 2) NOT NULL,
	[LocationWiseStock] [decimal](18, 2) NOT NULL,
	[Printer] [nvarchar](10) NULL,
	[Barcode] [nvarchar](200) NULL,
	[IsItemLock] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[RefCode01] [nvarchar](200) NULL,
	[RefCode02] [nvarchar](200) NULL,
	[WastagePrc] [decimal](18, 2) NOT NULL,
	[PurchasingUnit] [int] NOT NULL,
	[IsDiscount] [bit] NOT NULL,
	[IsCostOnReceipe] [bit] NOT NULL,
	[IsAddon] [bit] NOT NULL,
	[NameOnInvoice] [nvarchar](50) NULL,
	[IsPackItem] [bit] NOT NULL,
	[PackSize] [decimal](18, 2) NOT NULL,
	[PackPrice] [decimal](18, 2) NOT NULL,
	[IsPromotion] [bit] NOT NULL,
	[IsFreeIssue] [bit] NOT NULL,
	[IsExpiry] [bit] NOT NULL,
	[IsTax] [bit] NOT NULL,
	[WeightPerUnit] [int] NOT NULL,
	[IsUnderCost] [bit] NOT NULL,
	[IsBundle] [bit] NOT NULL,
	[MaxPrice] [decimal](18, 2) NOT NULL,
	[MinPrice] [decimal](18, 2) NOT NULL,
	[DiscountPrecentage] [decimal](18, 2) NOT NULL,
	[MaximumDiscount] [decimal](18, 2) NOT NULL,
	[FixedDiscountPercentage] [decimal](18, 2) NOT NULL,
	[FixedDiscountAmount] [decimal](18, 2) NOT NULL,
	[MaximumDiscountPercentage] [decimal](18, 2) NOT NULL,
	[PrinterTypeId] [int] NOT NULL,
	[AddonCategoryMasterId] [bigint] NULL,
	[IsTaxInclude] [bit] NOT NULL,
	[IsOpenItem] [bit] NOT NULL,
 CONSTRAINT [PK_dbo.Products_Temp] PRIMARY KEY CLUSTERED 
(
	[ProductId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion Products_Temp

                #region ProductServingUnits
                tableName = "ProductServingUnits";
                query = @"CREATE TABLE [dbo].[ProductServingUnits](
	[ProductServingUnitId] [bigint] IDENTITY(1,1) NOT NULL,
	[ProductId] [bigint] NOT NULL,
	[ServingUnit] [nvarchar](max) NOT NULL,
	[CostPrice] [decimal](18, 2) NOT NULL,
	[SellingPrice] [decimal](18, 2) NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[DeductStockOnRecipe] [bit] NOT NULL,
 CONSTRAINT [PK_dbo.ProductServingUnits] PRIMARY KEY CLUSTERED 
(
	[ProductServingUnitId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion ProductServingUnits

                #region ProductStockMasters
                tableName = "ProductStockMasters";
                query = @"CREATE TABLE [dbo].[ProductStockMasters](
	[ProductStockMasterId] [bigint] IDENTITY(1,1) NOT NULL,
	[CostCentreId] [int] NOT NULL,
	[ProductId] [bigint] NOT NULL,
	[StockCode] [nvarchar](25) NOT NULL,
	[Stock] [decimal](18, 2) NOT NULL,
	[CostPrice] [decimal](18, 2) NOT NULL,
	[SellingPrice] [decimal](18, 2) NOT NULL,
	[ReOrderLevel] [decimal](18, 2) NOT NULL,
	[ReOrderQuantity] [decimal](18, 2) NOT NULL,
	[ReOrderPeriod] [decimal](18, 2) NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[ProductCode] [nvarchar](20) NULL,
	[ProductName] [nvarchar](100) NULL,
	[Barcode] [nvarchar](30) NULL,
	[RefNo1] [nvarchar](30) NULL,
	[RefNo2] [nvarchar](30) NULL,
	[ExtendedId] [int] NOT NULL,
	[ExtendedName] [nvarchar](30) NULL,
	[PLUCode] [nvarchar](5) NULL,
	[WeightPerunit] [decimal](18, 2) NOT NULL,
	[UomId] [int] NOT NULL,
	[Unit] [nvarchar](10) NULL,
	[AvgCost] [decimal](18, 2) NOT NULL,
	[FixedGP] [decimal](18, 2) NOT NULL,
	[GP] [decimal](18, 2) NOT NULL,
	[OpenBal] [decimal](18, 2) NOT NULL,
	[InitSIH] [decimal](18, 2) NOT NULL,
	[InitCost] [decimal](18, 2) NOT NULL,
	[AdjQty] [decimal](18, 2) NOT NULL,
	[IsDamage] [bit] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[IsBundle] [bit] NOT NULL,
	[IsInitialize] [bit] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[Ispacksize] [bit] NOT NULL,
	[Iscommission] [bit] NOT NULL,
	[Isdecimal] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DiscountPrc] [decimal](18, 2) NOT NULL,
	[DocumentNo] [nvarchar](20) NULL,
	[LastUpdatedDate] [datetime] NULL,
	[ForignCustomerPrice] [decimal](18, 2) NOT NULL,
	[MaximumDiscount] [decimal](18, 2) NOT NULL,
	[FixedDiscountPercentage] [decimal](18, 2) NOT NULL,
	[FixedDiscountAmount] [decimal](18, 2) NOT NULL,
	[MaximumDiscountPercentage] [decimal](18, 2) NOT NULL,
	[PrinterType_Id] [int] NOT NULL,
 CONSTRAINT [PK_dbo.ProductStockMasters] PRIMARY KEY CLUSTERED 
(
	[ProductStockMasterId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion ProductStockMasters

                #region ProductTaxes
                tableName = "ProductTaxes";
                query = @"CREATE TABLE [dbo].[ProductTaxes](
	[ProductTaxId] [bigint] IDENTITY(1,1) NOT NULL,
	[ProductId] [bigint] NOT NULL,
	[TaxId] [bigint] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[TaxPracentage] [decimal](18, 2) NOT NULL,
	[TaxSequence] [int] NOT NULL,
 CONSTRAINT [PK_dbo.ProductTaxes] PRIMARY KEY CLUSTERED 
(
	[ProductTaxId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion ProductTaxes

                #region PurchaseDetails
                tableName = "PurchaseDetails";
                query = @"CREATE TABLE [dbo].[PurchaseDetails](
	[PurchaseDetailID] [bigint] IDENTITY(1,1) NOT NULL,
	[PurchaseHeaderID] [bigint] NOT NULL,
	[CostCentreID] [int] NOT NULL,
	[DocumentID] [int] NOT NULL,
	[DocumentNo] [nvarchar](20) NULL,
	[LineNo] [bigint] NOT NULL,
	[ProductID] [bigint] NOT NULL,
	[IsBatch] [bit] NOT NULL,
	[BatchNo] [nvarchar](50) NULL,
	[StockCode] [nvarchar](25) NULL,
	[UnitOfMeasureID] [bigint] NOT NULL,
	[BaseUnitID] [bigint] NOT NULL,
	[IsExpiry] [bit] NOT NULL,
	[ExpiryDate] [datetime] NULL,
	[OrderQty] [decimal](18, 3) NOT NULL,
	[FreeQty] [decimal](18, 3) NOT NULL,
	[CurrentQty] [decimal](18, 3) NOT NULL,
	[ConvertFactor] [decimal](18, 2) NOT NULL,
	[BalanceQty] [decimal](18, 3) NOT NULL,
	[CostPrice] [decimal](18, 3) NOT NULL,
	[SellingPrice] [decimal](18, 3) NOT NULL,
	[AvgCost] [decimal](18, 2) NOT NULL,
	[GrossAmount] [decimal](18, 3) NOT NULL,
	[DiscountPercentage] [decimal](18, 3) NOT NULL,
	[DiscountAmount] [decimal](18, 3) NOT NULL,
	[SubTotalDiscount] [decimal](18, 2) NOT NULL,
	[TotalTax] [decimal](18, 2) NOT NULL,
	[NetAmount] [decimal](18, 3) NOT NULL,
	[DocumentStatus] [int] NOT NULL,
	[DocumentDate] [datetime] NOT NULL,
	[ProductRemark] [nvarchar](200) NULL,
	[Packsize] [decimal](18, 2) NOT NULL,
	[profitMargin] [decimal](18, 2) NOT NULL,
	[SerialNo] [nvarchar](max) NULL,
	[IsUsed] [bit] NOT NULL,
	[Discount] [decimal](18, 2) NOT NULL,
	[GRNQuantity] [decimal](18, 3) NOT NULL,
	[CostValue] [decimal](18, 3) NOT NULL,
	[TOGQty] [decimal](18, 3) NOT NULL,
	[DiscountType] [varchar](3) NULL,
	[IsPRN] [bit] NOT NULL,
	[PRNQuantity] [decimal](18, 2) NOT NULL,
 CONSTRAINT [PK_dbo.PurchaseDetails] PRIMARY KEY CLUSTERED 
(
	[PurchaseDetailID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion PurchaseDetails

                #region PurchaseHeaders
                tableName = "PurchaseHeaders";
                query = @"CREATE TABLE [dbo].[PurchaseHeaders](
	[PurchaseHeaderId] [bigint] IDENTITY(1,1) NOT NULL,
	[CostCentreID] [int] NOT NULL,
	[DocumentID] [int] NOT NULL,
	[DocumentNo] [nvarchar](20) NOT NULL,
	[DocumentDate] [datetime] NOT NULL,
	[SupplierID] [bigint] NOT NULL,
	[GrossAmount] [decimal](18, 3) NOT NULL,
	[DiscountAmount] [decimal](18, 3) NOT NULL,
	[OtherChargers] [decimal](18, 3) NOT NULL,
	[DiscountPercentage] [decimal](18, 2) NOT NULL,
	[TotalTax] [decimal](18, 3) NOT NULL,
	[NetAmount] [decimal](18, 3) NOT NULL,
	[BatchNo] [nvarchar](50) NULL,
	[Remark] [nvarchar](max) NULL,
	[LineDiscountTotal] [decimal](18, 3) NOT NULL,
	[PaymentTermID] [int] NOT NULL,
	[PaymentPeriod] [int] NOT NULL,
	[CurrencyID] [int] NOT NULL,
	[CurrencyRate] [decimal](18, 2) NOT NULL,
	[ReferenceDocumentDocumentID] [int] NOT NULL,
	[ReferenceDocumentID] [bigint] NOT NULL,
	[ReferenceNo] [nvarchar](20) NULL,
	[SupplierInvoiceNo] [nvarchar](20) NULL,
	[DocumentStatus] [int] NOT NULL,
	[IsUpLoad] [bit] NOT NULL,
	[ReturnTypeID] [int] NOT NULL,
	[OtherDeduction] [decimal](18, 2) NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[PaymentMethodId] [int] NOT NULL,
	[TotSellingPrice] [decimal](18, 3) NOT NULL,
	[TotCostPrice] [decimal](18, 3) NOT NULL,
	[TotDiscounts] [decimal](18, 3) NOT NULL,
	[GRNLocationId] [bigint] NOT NULL,
	[IsTempGRN] [bit] NOT NULL,
	[OtherDocNo] [nvarchar](20) NULL,
	[IsGRN] [bit] NOT NULL,
	[IsTempPRN] [bit] NOT NULL,
	[POID] [bigint] NOT NULL,
	[GRNId] [bigint] NOT NULL,
	[PRNType] [nvarchar](max) NULL,
	[GRNType] [nvarchar](max) NULL,
	[GRNDate] [datetime] NOT NULL,
	[Deductions] [decimal](18, 2) NOT NULL,
	[EventId] [int] NULL,
	[NewDocNumber] [nvarchar](20) NULL,
	[IsPRN] [bit] NOT NULL,
	[RejectReason] [nvarchar](200) NULL,
	[CancelReason] [nvarchar](200) NULL,
 CONSTRAINT [PK_dbo.PurchaseHeaders] PRIMARY KEY CLUSTERED 
(
	[PurchaseHeaderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion PurchaseHeaders

                #region PurchaseOrderDetails
                tableName = "PurchaseOrderDetails";
                query = @"CREATE TABLE [dbo].[PurchaseOrderDetails](
	[PurchaseOrderDetailId] [bigint] IDENTITY(1,1) NOT NULL,
	[PurchaseOrderHeaderId] [bigint] NOT NULL,
	[ProductId] [bigint] NOT NULL,
	[IsBatch] [bit] NOT NULL,
	[StockCode] [nvarchar](25) NULL,
	[OrderQty] [decimal](18, 3) NOT NULL,
	[FreeQty] [decimal](18, 3) NOT NULL,
	[CurrentQty] [decimal](18, 3) NOT NULL,
	[BalanceQty] [decimal](18, 3) NOT NULL,
	[BalanceFreeQty] [decimal](18, 3) NOT NULL,
	[CostPrice] [decimal](18, 3) NOT NULL,
	[SellingPrice] [decimal](18, 3) NOT NULL,
	[PackSize] [nvarchar](max) NULL,
	[UnitOfMeasureId] [bigint] NOT NULL,
	[BaseUnitId] [bigint] NOT NULL,
	[ConvertFactor] [decimal](18, 2) NOT NULL,
	[GrossAmount] [decimal](18, 3) NOT NULL,
	[DiscountPercentage] [decimal](18, 3) NOT NULL,
	[DiscountAmount] [decimal](18, 3) NOT NULL,
	[SubTotalDiscount] [decimal](18, 3) NOT NULL,
	[NetAmount] [decimal](18, 3) NOT NULL,
	[LineNo] [bigint] NOT NULL,
	[BatchNo] [nvarchar](max) NULL,
	[ProductRemark] [nvarchar](200) NULL,
	[ItemTaxTotal] [decimal](18, 3) NOT NULL,
	[CostValue] [decimal](18, 3) NOT NULL,
	[GRNQuantity] [decimal](18, 3) NOT NULL,
	[IsGRN] [bit] NOT NULL,
 CONSTRAINT [PK_dbo.PurchaseOrderDetails] PRIMARY KEY CLUSTERED 
(
	[PurchaseOrderDetailId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion PurchaseOrderDetails

                #region PurchaseOrderHeaders
                tableName = "PurchaseOrderHeaders";
                query = @"CREATE TABLE [dbo].[PurchaseOrderHeaders](
	[PurchaseOrderHeaderId] [bigint] IDENTITY(1,1) NOT NULL,
	[JobClassId] [bigint] NOT NULL,
	[DocumentNo] [nvarchar](20) NOT NULL,
	[DocumentDate] [datetime] NOT NULL,
	[SupplierId] [bigint] NOT NULL,
	[ExpectedDate] [datetime] NOT NULL,
	[ExpiryDate] [datetime] NOT NULL,
	[PaymentExpectedDate] [datetime] NOT NULL,
	[ValidityPeriod] [int] NOT NULL,
	[GrossAmount] [decimal](18, 3) NOT NULL,
	[OtherCharges] [decimal](18, 3) NOT NULL,
	[DiscountAmount] [decimal](18, 3) NOT NULL,
	[DiscountPercentage] [decimal](18, 3) NOT NULL,
	[Addition] [decimal](18, 3) NOT NULL,
	[Deduction] [decimal](18, 3) NOT NULL,
	[NetAmount] [decimal](18, 3) NOT NULL,
	[RequestedBy] [nvarchar](50) NULL,
	[DeliveryLocationId] [int] NOT NULL,
	[LineDiscountTotal] [decimal](18, 3) NOT NULL,
	[PaymentTermId] [int] NOT NULL,
	[PaymentPeriod] [int] NOT NULL,
	[CurrencyId] [int] NOT NULL,
	[CurrencyRate] [decimal](18, 2) NOT NULL,
	[DeliveryDetail] [nvarchar](500) NULL,
	[ReferenceNo] [nvarchar](20) NULL,
	[Remark] [nvarchar](150) NULL,
	[DocumentStatus] [int] NOT NULL,
	[LastAuthorizedBy] [nvarchar](50) NULL,
	[LastAuthorizedDate] [datetime] NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[TotalTaxAmount] [decimal](18, 3) NOT NULL,
	[TotSellingPrice] [decimal](18, 3) NOT NULL,
	[TotCostPrice] [decimal](18, 3) NOT NULL,
	[TotDiscounts] [decimal](18, 3) NOT NULL,
	[POType] [nvarchar](max) NULL,
	[POLocationId] [bigint] NOT NULL,
	[PaymentMethodId] [int] NOT NULL,
	[TempDocNumber] [nvarchar](max) NULL,
	[IsTempPO] [bit] NOT NULL,
	[IsGRN] [bit] NOT NULL,
	[DocumentId] [int] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[DeliveryAddress] [nvarchar](max) NULL,
	[PODate] [datetime] NOT NULL,
	[EventId] [int] NULL,
	[NewDocNumber] [nvarchar](20) NULL,
	[RejectReason] [nvarchar](200) NULL,
	[CancelReason] [nvarchar](200) NULL,
	[RequestNoteId] [int] NOT NULL,
 CONSTRAINT [PK_dbo.PurchaseOrderHeaders] PRIMARY KEY CLUSTERED 
(
	[PurchaseOrderHeaderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion PurchaseOrderHeaders

                #region Receipes
                tableName = "Receipes";
                query = @"CREATE TABLE [dbo].[Receipes](
	[ReceipeId] [bigint] IDENTITY(1,1) NOT NULL,
	[ProductId] [bigint] NOT NULL,
	[Quantity] [decimal](18, 4) NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[MaterialId] [bigint] NOT NULL,
	[ProductServingUnitId] [bigint] NOT NULL,
	[CostPrice] [decimal](18, 2) NOT NULL,
	[SellingPrice] [decimal](18, 2) NOT NULL,
	[ProductQty] [decimal](18, 2) NOT NULL,
	[IsActive] [bit] NOT NULL
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion Receipes

                #region Receipes12
                tableName = "Receipes12";
                query = @"CREATE TABLE [dbo].[Receipes12](
	[ReceipeId] [bigint] IDENTITY(1,1) NOT NULL,
	[ProductId] [bigint] NOT NULL,
	[Quantity] [decimal](18, 4) NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[MaterialId] [bigint] NOT NULL,
	[ProductServingUnitId] [bigint] NOT NULL,
	[CostPrice] [decimal](18, 2) NOT NULL,
	[SellingPrice] [decimal](18, 2) NOT NULL,
	[ProductQty] [decimal](18, 2) NOT NULL,
 CONSTRAINT [PK_dbo.Receipes] PRIMARY KEY CLUSTERED 
(
	[ReceipeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion Receipes12

                #region Receipes3
                tableName = "Receipes3";
                query = @"CREATE TABLE [dbo].[Receipes3](
	[ReceipeId] [bigint] IDENTITY(1,1) NOT NULL,
	[ProductId] [bigint] NOT NULL,
	[Quantity] [decimal](18, 4) NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[MaterialId] [bigint] NOT NULL,
	[ProductServingUnitId] [bigint] NOT NULL,
	[CostPrice] [decimal](18, 2) NOT NULL,
	[SellingPrice] [decimal](18, 2) NOT NULL,
	[ProductQty] [decimal](18, 2) NOT NULL,
 CONSTRAINT [PK_dbo.Receipes3] PRIMARY KEY CLUSTERED 
(
	[ReceipeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion Receipes3

                #region RECIPES_2$
                tableName = "RECIPES_2$";
                query = @"CREATE TABLE [dbo].[RECIPES_2$](
	[ReceipeId] [nvarchar](255) NULL,
	[ProductId] [nvarchar](255) NULL,
	[ProductServingUnitId] [nvarchar](255) NULL,
	[ProductQty] [float] NULL,
	[GroupOfCompanyID] [nvarchar](255) NULL,
	[CompanyID] [nvarchar](255) NULL,
	[LocationId] [nvarchar](255) NULL,
	[CreatedUser] [nvarchar](255) NULL,
	[CreatedDate] [datetime] NULL,
	[ModifiedUser] [nvarchar](255) NULL,
	[ModifiedDate] [datetime] NULL,
	[DataTransfer] [nvarchar](255) NULL,
	[MaterialId] [nvarchar](255) NULL,
	[Quantity] [float] NULL,
	[CostPrice] [nvarchar](255) NULL,
	[SellingPrice] [float] NULL
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion RECIPES_2$

                #region ReferenceTypes
                tableName = "ReferenceTypes";
                query = @"CREATE TABLE [dbo].[ReferenceTypes](
	[ReferenceTypeId] [int] IDENTITY(1,1) NOT NULL,
	[LookupType] [nvarchar](25) NULL,
	[LookupKey] [int] NOT NULL,
	[LookupValue] [nvarchar](100) NULL,
	[Remark] [nvarchar](100) NULL,
	[IsDelete] [int] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 CONSTRAINT [PK_dbo.ReferenceTypes] PRIMARY KEY CLUSTERED 
(
	[ReferenceTypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion ReferenceTypes

                #region ReportCategories
                tableName = "ReportCategories";
                query = @"CREATE TABLE [dbo].[ReportCategories](
	[ReportCategoryId] [int] IDENTITY(1,1) NOT NULL,
	[ReportCategoryCode] [nvarchar](max) NOT NULL,
	[ReportCategoryName] [nvarchar](100) NOT NULL,
	[OrderId] [int] NOT NULL,
	[Permission] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.ReportCategories] PRIMARY KEY CLUSTERED 
(
	[ReportCategoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion ReportCategories

                #region ReportInfoes
                tableName = "ReportInfoes";
                query = @"CREATE TABLE [dbo].[ReportInfoes](
	[ReportInfoId] [bigint] IDENTITY(1,1) NOT NULL,
	[ReportCategoryId] [bigint] NOT NULL,
	[ReportName] [nvarchar](100) NOT NULL,
	[ReportPath] [nvarchar](200) NULL,
	[ReportFileName] [nvarchar](150) NULL,
	[ReportURL] [nvarchar](200) NULL,
	[OrderId] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
 CONSTRAINT [PK_dbo.ReportInfoes] PRIMARY KEY CLUSTERED 
(
	[ReportInfoId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion ReportInfoes

                #region ReportInfoes27052022
                tableName = "ReportInfoes27052022";
                query = @"CREATE TABLE [dbo].[ReportInfoes27052022](
	[ReportInfoId] [bigint] IDENTITY(1,1) NOT NULL,
	[ReportCategoryId] [bigint] NOT NULL,
	[ReportName] [nvarchar](100) NOT NULL,
	[ReportPath] [nvarchar](200) NULL,
	[ReportFileName] [nvarchar](150) NULL,
	[ReportURL] [nvarchar](200) NULL,
	[OrderId] [int] NOT NULL,
	[CompanyID] [int] NOT NULL
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion ReportInfoes27052022

                #region RequestNoteAcceptanceDetails
                tableName = "RequestNoteAcceptanceDetails";
                query = @"CREATE TABLE [dbo].[RequestNoteAcceptanceDetails](
	[RequestNoteAcceptanceDetailId] [bigint] IDENTITY(1,1) NOT NULL,
	[RequestNoteAccptanceHeaderId] [bigint] NOT NULL,
	[LineNo] [bigint] NOT NULL,
	[ProductId] [bigint] NOT NULL,
	[CostPrice] [decimal](18, 2) NOT NULL,
	[SellingPrice] [decimal](18, 2) NOT NULL,
	[UnitOfMeasureId] [bigint] NOT NULL,
	[MaterialId] [bigint] NOT NULL,
	[MaterialQty] [decimal](18, 2) NOT NULL,
	[IssueQty] [decimal](18, 2) NOT NULL,
	[IsTOG] [bit] NOT NULL,
	[RequestedBy] [nvarchar](max) NULL,
	[ServingUnitId] [int] NOT NULL,
	[ServingUnit] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.RequestNoteAcceptanceDetails] PRIMARY KEY CLUSTERED 
(
	[RequestNoteAcceptanceDetailId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion RequestNoteAcceptanceDetails

                #region RequestNoteAccptanceHeaders
                tableName = "RequestNoteAccptanceHeaders";
                query = @"CREATE TABLE [dbo].[RequestNoteAccptanceHeaders](
	[RequestNoteAccptanceHeaderId] [bigint] IDENTITY(1,1) NOT NULL,
	[FromLocationId] [int] NOT NULL,
	[FromDepartmentId] [int] NOT NULL,
	[ToLocationId] [int] NOT NULL,
	[ToDepartmentId] [int] NOT NULL,
	[DocumentNo] [nvarchar](20) NULL,
	[DocumentDate] [datetime] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[TotSellingPrice] [decimal](18, 2) NOT NULL,
	[TotCostPrice] [decimal](18, 2) NOT NULL,
	[IsTempRequest] [bit] NOT NULL,
	[Remark] [nvarchar](150) NULL,
	[NewDocNumber] [nvarchar](20) NULL,
	[IsTOG] [bit] NOT NULL,
	[RequestType] [nvarchar](max) NULL,
	[CompanyId] [int] NOT NULL,
	[IsProductionComplete] [bit] NOT NULL,
	[IsTOGComplete] [bit] NOT NULL,
	[IsPOComplete] [bit] NOT NULL,
 CONSTRAINT [PK_dbo.RequestNoteAccptanceHeaders] PRIMARY KEY CLUSTERED 
(
	[RequestNoteAccptanceHeaderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion RequestNoteAccptanceHeaders

                #region RequestNoteDetails
                tableName = "RequestNoteDetails";
                query = @"CREATE TABLE [dbo].[RequestNoteDetails](
	[RequestNoteDetailId] [bigint] IDENTITY(1,1) NOT NULL,
	[RequestnoteHeaderId] [bigint] NOT NULL,
	[LineNo] [bigint] NOT NULL,
	[ProductId] [bigint] NOT NULL,
	[AvgCost] [decimal](18, 2) NOT NULL,
	[CostPrice] [decimal](18, 2) NOT NULL,
	[SellingPrice] [decimal](18, 2) NOT NULL,
	[RequestQty] [decimal](18, 2) NOT NULL,
	[UnitOfMeasureId] [bigint] NOT NULL,
	[RequestedBy] [nvarchar](max) NULL,
	[ServingUnitId] [int] NOT NULL,
	[ServingUnit] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.RequestNoteDetails] PRIMARY KEY CLUSTERED 
(
	[RequestNoteDetailId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion RequestNoteDetails

                #region RequestNoteHeaders
                tableName = "RequestNoteHeaders";
                query = @"CREATE TABLE [dbo].[RequestNoteHeaders](
	[RequestnoteHeaderId] [bigint] IDENTITY(1,1) NOT NULL,
	[FromLocationId] [int] NOT NULL,
	[FromDepartmentId] [int] NOT NULL,
	[ToLocationId] [int] NOT NULL,
	[ToDepartmentId] [int] NOT NULL,
	[DocumentNo] [nvarchar](20) NULL,
	[DocumentDate] [datetime] NOT NULL,
	[ReferenceNo] [nvarchar](20) NULL,
	[Remark] [nvarchar](150) NULL,
	[IsActive] [bit] NOT NULL,
	[TotSellingPrice] [decimal](18, 2) NOT NULL,
	[TotCostPrice] [decimal](18, 2) NOT NULL,
	[IsTempRequest] [bit] NOT NULL,
	[DocumentId] [int] NOT NULL,
	[IsApproved] [bit] NOT NULL,
	[DocumentStatus] [int] NOT NULL,
	[NewDocNumber] [nvarchar](20) NULL,
	[RejectReason] [nvarchar](200) NULL,
	[CancelReason] [nvarchar](200) NULL,
	[RequestType] [nvarchar](max) NULL,
	[CompanyId] [int] NOT NULL,
 CONSTRAINT [PK_dbo.RequestNoteHeaders] PRIMARY KEY CLUSTERED 
(
	[RequestnoteHeaderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion RequestNoteHeaders

                #region RstCategories
                tableName = "RstCategories";
                query = @"CREATE TABLE [dbo].[RstCategories](
	[RstCategoryID] [int] IDENTITY(1,1) NOT NULL,
	[RstDepartmentID] [int] NOT NULL,
	[RstCategoryCode] [nvarchar](50) NOT NULL,
	[RstCategoryName] [nvarchar](100) NOT NULL,
	[Remark] [nvarchar](max) NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[CatImage] [varbinary](max) NULL,
	[CatImageName] [nvarchar](max) NULL,
	[CatImageType] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.RstCategories] PRIMARY KEY CLUSTERED 
(
	[RstCategoryID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion RstCategories

                #region RstDepartments
                tableName = "RstDepartments";
                query = @"CREATE TABLE [dbo].[RstDepartments](
	[RstDepartmentID] [int] IDENTITY(1,1) NOT NULL,
	[DepartmentCode] [nvarchar](50) NOT NULL,
	[DepartmentName] [nvarchar](100) NOT NULL,
	[Remark] [nvarchar](max) NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[DeptImage] [varbinary](max) NULL,
	[DeptImageName] [nvarchar](max) NULL,
	[DeptImageType] [nvarchar](max) NULL,
	[DashBoardColor] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.RstDepartments] PRIMARY KEY CLUSTERED 
(
	[RstDepartmentID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion RstDepartments

                #region RstKotCategories
                tableName = "RstKotCategories";
                query = @"CREATE TABLE [dbo].[RstKotCategories](
	[RstKotCategoryID] [int] IDENTITY(1,1) NOT NULL,
	[RstKotCategoryCode] [nvarchar](max) NOT NULL,
	[RstKotCategoryName] [nvarchar](100) NOT NULL,
	[IPAddress] [nvarchar](max) NULL,
	[PrinterName] [nvarchar](max) NULL,
	[COMName] [nvarchar](max) NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 CONSTRAINT [PK_dbo.RstKotCategories] PRIMARY KEY CLUSTERED 
(
	[RstKotCategoryID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion RstKotCategories

                #region RstMealTypes
                tableName = "RstMealTypes";
                query = @"CREATE TABLE [dbo].[RstMealTypes](
	[RstMealTypeId] [int] IDENTITY(1,1) NOT NULL,
	[RstMealTypeCode] [nvarchar](10) NOT NULL,
	[Description] [nvarchar](max) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 CONSTRAINT [PK_dbo.RstMealTypes] PRIMARY KEY CLUSTERED 
(
	[RstMealTypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion RstMealTypes

                #region RstPromotions
                tableName = "RstPromotions";
                query = @"CREATE TABLE [dbo].[RstPromotions](
	[RstPromotionsID] [int] IDENTITY(1,1) NOT NULL,
	[PromotionCode] [nvarchar](max) NOT NULL,
	[PromotionTypeID] [int] NOT NULL,
	[Description] [nvarchar](100) NOT NULL,
	[FromDate] [datetime] NOT NULL,
	[ToDate] [datetime] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 CONSTRAINT [PK_dbo.RstPromotions] PRIMARY KEY CLUSTERED 
(
	[RstPromotionsID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion RstPromotions

                #region RstPromotionTypes
                tableName = "RstPromotionTypes";
                query = @"CREATE TABLE [dbo].[RstPromotionTypes](
	[RstPromotionTypesID] [int] IDENTITY(1,1) NOT NULL,
	[PromotionTypeCode] [nvarchar](max) NOT NULL,
	[Description] [nvarchar](100) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 CONSTRAINT [PK_dbo.RstPromotionTypes] PRIMARY KEY CLUSTERED 
(
	[RstPromotionTypesID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion RstPromotionTypes

                #region RstRoomMasters
                tableName = "RstRoomMasters";
                query = @"CREATE TABLE [dbo].[RstRoomMasters](
	[RstRoomMasterID] [int] IDENTITY(1,1) NOT NULL,
	[RoomMasterCode] [nvarchar](max) NOT NULL,
	[RoomName] [nvarchar](100) NOT NULL,
	[RoomType] [int] NOT NULL,
	[Floor] [int] NOT NULL,
	[InterComNo] [nvarchar](max) NULL,
	[RFIDNo] [nvarchar](max) NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[RoomNo] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_dbo.RstRoomMasters] PRIMARY KEY CLUSTERED 
(
	[RstRoomMasterID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion RstRoomMasters

                #region RstRoomTypeRates
                tableName = "RstRoomTypeRates";
                query = @"CREATE TABLE [dbo].[RstRoomTypeRates](
	[RstRoomTypeRateID] [int] IDENTITY(1,1) NOT NULL,
	[RoomTypeRateCode] [nvarchar](max) NOT NULL,
	[RoomTypeRateName] [nvarchar](100) NOT NULL,
	[Rate] [decimal](18, 2) NOT NULL,
	[FromDate] [datetime] NOT NULL,
	[ToDate] [datetime] NOT NULL,
	[ExtraAdultRate] [decimal](18, 2) NOT NULL,
	[ExtraChildRate] [decimal](18, 2) NOT NULL,
	[ForeignRate] [decimal](18, 2) NOT NULL,
	[Package] [nvarchar](max) NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 CONSTRAINT [PK_dbo.RstRoomTypeRates] PRIMARY KEY CLUSTERED 
(
	[RstRoomTypeRateID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion RstRoomTypeRates

                #region RstRoomTypes
                tableName = "RstRoomTypes";
                query = @"CREATE TABLE [dbo].[RstRoomTypes](
	[RstRoomTypeID] [int] IDENTITY(1,1) NOT NULL,
	[RoomTypeCode] [nvarchar](max) NOT NULL,
	[RoomTypeName] [nvarchar](100) NOT NULL,
	[BedType] [nvarchar](max) NULL,
	[MaxAdult] [int] NOT NULL,
	[MaxChild] [int] NOT NULL,
	[MaxInfant] [int] NOT NULL,
	[IsAC] [bit] NOT NULL,
	[IsSmoking] [bit] NOT NULL,
	[IsMiniBar] [bit] NOT NULL,
	[IsNormalView] [bit] NOT NULL,
	[IsOceanView] [bit] NOT NULL,
	[IsLandside] [bit] NOT NULL,
	[IsBalcony] [bit] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 CONSTRAINT [PK_dbo.RstRoomTypes] PRIMARY KEY CLUSTERED 
(
	[RstRoomTypeID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion RstRoomTypes

                #region RstSubCategories
                tableName = "RstSubCategories";
                query = @"CREATE TABLE [dbo].[RstSubCategories](
	[RstSubCategoryID] [int] IDENTITY(1,1) NOT NULL,
	[RstCategoryID] [int] NOT NULL,
	[RstSubCategoryCode] [nvarchar](max) NOT NULL,
	[RstSubCategoryName] [nvarchar](max) NOT NULL,
	[Remark] [nvarchar](max) NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[SubCatImage] [varbinary](max) NULL,
	[SubCatImageName] [nvarchar](max) NULL,
	[SubCatImageType] [nvarchar](max) NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 CONSTRAINT [PK_dbo.RstSubCategories] PRIMARY KEY CLUSTERED 
(
	[RstSubCategoryID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion RstSubCategories

                #region ServingUnits
                tableName = "ServingUnits";
                query = @"CREATE TABLE [dbo].[ServingUnits](
	[ServingUnitId] [bigint] IDENTITY(1,1) NOT NULL,
	[ServingUnitName] [nvarchar](max) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 CONSTRAINT [PK_dbo.ServingUnits] PRIMARY KEY CLUSTERED 
(
	[ServingUnitId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion ServingUnits

                #region StewardsMasters
                tableName = "StewardsMasters";
                query = @"CREATE TABLE [dbo].[StewardsMasters](
	[StewardsMasterID] [int] IDENTITY(1,1) NOT NULL,
	[StewardCode] [nvarchar](max) NOT NULL,
	[StewardTitle] [nvarchar](max) NOT NULL,
	[StewardName] [nvarchar](max) NOT NULL,
	[Address1] [nvarchar](max) NULL,
	[Address2] [nvarchar](max) NULL,
	[Address3] [nvarchar](max) NULL,
	[DOB] [datetime] NOT NULL,
	[NIC] [nvarchar](max) NULL,
	[Passport] [nvarchar](max) NULL,
	[Telephone] [nvarchar](max) NULL,
	[Mobile] [nvarchar](max) NULL,
	[Fax] [nvarchar](max) NULL,
	[Email] [nvarchar](max) NULL,
	[Target] [nvarchar](max) NULL,
	[Commission] [decimal](18, 2) NOT NULL,
	[IsDeliveryPerson] [bit] NOT NULL,
	[IsKarokeGirl] [bit] NOT NULL,
	[Picture] [varbinary](max) NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 CONSTRAINT [PK_dbo.StewardsMasters] PRIMARY KEY CLUSTERED 
(
	[StewardsMasterID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion StewardsMasters

                #region StockAdjustmentDetails
                tableName = "StockAdjustmentDetails";
                query = @"CREATE TABLE [dbo].[StockAdjustmentDetails](
	[StockAdjustmentDetailId] [bigint] IDENTITY(1,1) NOT NULL,
	[StockAdjustmentHeaderId] [bigint] NOT NULL,
	[ProductId] [bigint] NOT NULL,
	[CurrentStock] [decimal](18, 2) NOT NULL,
	[AdjustStock] [decimal](18, 2) NOT NULL,
	[NewStock] [decimal](18, 2) NOT NULL,
	[CostPrice] [decimal](18, 2) NOT NULL,
	[SellingPrice] [decimal](18, 2) NOT NULL,
	[AvgCost] [decimal](18, 2) NOT NULL,
	[ProductName] [nvarchar](max) NULL,
	[BaseType] [nvarchar](max) NULL,
	[Reason] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.StockAdjustmentDetails] PRIMARY KEY CLUSTERED 
(
	[StockAdjustmentDetailId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion StockAdjustmentDetails

                #region StockAdjustmentHeaders
                tableName = "StockAdjustmentHeaders";
                query = @"CREATE TABLE [dbo].[StockAdjustmentHeaders](
	[StockAdjustmentHeaderId] [bigint] IDENTITY(1,1) NOT NULL,
	[DocumentNo] [nvarchar](max) NULL,
	[StockLocationId] [bigint] NOT NULL,
	[Remark] [nvarchar](max) NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[DocumentId] [int] NOT NULL,
	[NewDocNumber] [nvarchar](20) NULL,
	[DocumentStatus] [int] NOT NULL,
 CONSTRAINT [PK_dbo.StockAdjustmentHeaders] PRIMARY KEY CLUSTERED 
(
	[StockAdjustmentHeaderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion StockAdjustmentHeaders

                #region StockAdjustmentTypes
                tableName = "StockAdjustmentTypes";
                query = @"CREATE TABLE [dbo].[StockAdjustmentTypes](
	[AdjustmentTypeId] [int] IDENTITY(1,1) NOT NULL,
	[BaseType] [nvarchar](max) NULL,
	[Type] [nvarchar](max) NULL,
	[IsActive] [bit] NOT NULL,
 CONSTRAINT [PK_dbo.StockAdjustmentTypes] PRIMARY KEY CLUSTERED 
(
	[AdjustmentTypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion StockAdjustmentTypes

                #region Sub_Category$
                tableName = "Sub_Category$";
                query = @"CREATE TABLE [dbo].[Sub_Category$](
	[Category ] [nvarchar](255) NULL,
	[Sub Category Code ] [nvarchar](255) NULL,
	[Sub Category Name ] [nvarchar](255) NULL,
	[Is Active] [nvarchar](255) NULL
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion Sub_Category$

                #region Sub_Category_1$
                tableName = "Sub_Category_1$";
                query = @"CREATE TABLE [dbo].[Sub_Category_1$](
	[Category ] [nvarchar](255) NULL,
	[Sub Category Code ] [nvarchar](255) NULL,
	[Sub Category Name ] [nvarchar](255) NULL,
	[Is Active] [nvarchar](255) NULL
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion Sub_Category_1$

                #region SupplierGroups
                tableName = "SupplierGroups";
                query = @"CREATE TABLE [dbo].[SupplierGroups](
	[SupplierGroupID] [int] IDENTITY(1,1) NOT NULL,
	[SupplierGroupCode] [nvarchar](20) NOT NULL,
	[SupplierGroupName] [nvarchar](50) NOT NULL,
	[Remark] [nvarchar](150) NULL,
	[IsDelete] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 CONSTRAINT [PK_dbo.SupplierGroups] PRIMARY KEY CLUSTERED 
(
	[SupplierGroupID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion SupplierGroups

                #region SupplierProducts
                tableName = "SupplierProducts";
                query = @"CREATE TABLE [dbo].[SupplierProducts](
	[SupplierProductId] [bigint] IDENTITY(1,1) NOT NULL,
	[SupplierId] [int] NOT NULL,
	[ProductId] [int] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[IsPreferredSupplier] [bit] NOT NULL,
	[LastCostPrice] [decimal](18, 2) NOT NULL,
 CONSTRAINT [PK_dbo.SupplierProducts] PRIMARY KEY CLUSTERED 
(
	[SupplierProductId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion SupplierProducts

                #region Suppliers
                tableName = "Suppliers";
                query = @"CREATE TABLE [dbo].[Suppliers](
	[SupplierID] [bigint] IDENTITY(1,1) NOT NULL,
	[SupplierCode] [nvarchar](15) NOT NULL,
	[SupplierTitle] [nvarchar](max) NULL,
	[SupplierName] [nvarchar](100) NOT NULL,
	[ContactPersonName] [nvarchar](100) NULL,
	[Gender] [nvarchar](max) NOT NULL,
	[BillingAddress1] [nvarchar](250) NOT NULL,
	[BillingAddress2] [nvarchar](100) NULL,
	[BillingAddress3] [nvarchar](100) NULL,
	[BillingTelephone] [nvarchar](50) NOT NULL,
	[BillingMobile] [nvarchar](50) NULL,
	[BillingFax] [nvarchar](50) NULL,
	[Email] [nvarchar](100) NULL,
	[RepresentativeName] [nvarchar](100) NULL,
	[RepresentativeNICNo] [nvarchar](50) NULL,
	[PayeeName] [nvarchar](100) NULL,
	[DeliveryAddress1] [nvarchar](50) NULL,
	[DeliveryAddress2] [nvarchar](50) NULL,
	[DeliveryAddress3] [nvarchar](50) NULL,
	[DeliveryTelephone] [nvarchar](50) NULL,
	[DeliveryMobile] [nvarchar](50) NULL,
	[DeliveryFax] [nvarchar](50) NULL,
	[ReferenceNo] [nvarchar](20) NULL,
	[ReferenceSerial] [nvarchar](20) NULL,
	[PostalCode] [nvarchar](20) NULL,
	[TaxNo1] [nvarchar](25) NULL,
	[TaxNo2] [nvarchar](25) NULL,
	[TaxNo3] [nvarchar](25) NULL,
	[TaxNo4] [nvarchar](25) NULL,
	[TaxNo5] [nvarchar](25) NULL,
	[TaxRegistrationNo] [nvarchar](50) NULL,
	[TaxRegistrationName] [nvarchar](100) NULL,
	[PaymentMethod] [int] NOT NULL,
	[CreditLimit] [decimal](18, 2) NOT NULL,
	[ChequeLimit] [decimal](18, 2) NOT NULL,
	[ChequePeriod] [int] NOT NULL,
	[PaymentTermID] [int] NOT NULL,
	[CreditPeriod] [int] NOT NULL,
	[Remark] [nvarchar](100) NULL,
	[ProductBusinessType] [nvarchar](200) NULL,
	[SuppliedProducts] [nvarchar](200) NULL,
	[OrderCircle] [int] NOT NULL,
	[SupplierGroupID] [int] NOT NULL,
	[LedgerID] [bigint] NOT NULL,
	[OtherLedgerID] [bigint] NOT NULL,
	[TaxIdNo] [nvarchar](50) NULL,
	[IsUpload] [bit] NOT NULL,
	[IsBlocked] [bit] NOT NULL,
	[IsSuspended] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[IsPOMail] [bit] NOT NULL,
	[EmailBoday] [nvarchar](100) NULL,
	[EmailSubject] [nvarchar](100) NULL,
	[DepositeAmount] [decimal](18, 2) NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[SupplierPicture] [varbinary](max) NULL,
	[SupplierPictureName] [nvarchar](max) NULL,
	[SupplierPictureType] [nvarchar](max) NULL,
	[SupplierTypeID] [int] NOT NULL,
	[TaxID1] [int] NOT NULL,
	[TaxID2] [int] NOT NULL,
	[TaxID3] [int] NOT NULL,
	[TaxID4] [int] NOT NULL,
	[TaxID5] [int] NOT NULL,
 CONSTRAINT [PK_dbo.Suppliers] PRIMARY KEY CLUSTERED 
(
	[SupplierID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion Suppliers

                #region SupplierTypes
                tableName = "SupplierTypes";
                query = @"CREATE TABLE [dbo].[SupplierTypes](
	[SupplierTypeID] [int] IDENTITY(1,1) NOT NULL,
	[SupplierTypeCode] [nvarchar](20) NOT NULL,
	[SupplierTypeName] [nvarchar](50) NOT NULL,
	[Remark] [nvarchar](150) NULL,
	[IsDelete] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 CONSTRAINT [PK_dbo.SupplierTypes] PRIMARY KEY CLUSTERED 
(
	[SupplierTypeID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion SupplierTypes

                #region SuspendDetBackups
                tableName = "SuspendDetBackups";
                query = @"CREATE TABLE [dbo].[SuspendDetBackups](
	[Idx] [int] IDENTITY(1,1) NOT NULL,
	[ProductID] [int] NOT NULL,
	[ProductCode] [nvarchar](max) NULL,
	[RefCode] [nvarchar](max) NULL,
	[BarCodeFull] [int] NOT NULL,
	[Descrip] [nvarchar](max) NULL,
	[BatchNo] [nvarchar](max) NULL,
	[SerialNo] [nvarchar](max) NULL,
	[ExpiaryDate] [datetime] NOT NULL,
	[Cost] [decimal](18, 2) NOT NULL,
	[AvgCost] [decimal](18, 2) NOT NULL,
	[Price] [decimal](18, 2) NOT NULL,
	[Qty] [decimal](18, 2) NOT NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[UnitOfMeasureID] [int] NOT NULL,
	[UnitOfMeasureName] [nvarchar](max) NULL,
	[ConvertFactor] [decimal](18, 2) NOT NULL,
	[IDI1] [int] NOT NULL,
	[IDis1] [decimal](18, 2) NOT NULL,
	[IDiscount1] [decimal](18, 2) NOT NULL,
	[IDI1CashierID] [int] NOT NULL,
	[IDI2] [int] NOT NULL,
	[IDis2] [decimal](18, 2) NOT NULL,
	[IDiscount2] [decimal](18, 2) NOT NULL,
	[IDI2CashierID] [int] NOT NULL,
	[IDI3] [int] NOT NULL,
	[IDis3] [decimal](18, 2) NOT NULL,
	[IDiscount3] [decimal](18, 2) NOT NULL,
	[IDI3CashierID] [int] NOT NULL,
	[IDI4] [int] NOT NULL,
	[IDis4] [decimal](18, 2) NOT NULL,
	[IDiscount4] [decimal](18, 2) NOT NULL,
	[IDI4CashierID] [int] NOT NULL,
	[IDI5] [int] NOT NULL,
	[IDis5] [decimal](18, 2) NOT NULL,
	[IDiscount5] [decimal](18, 2) NOT NULL,
	[IDI5CashierID] [int] NOT NULL,
	[Rate] [decimal](18, 2) NOT NULL,
	[IsSDis] [bit] NOT NULL,
	[SDNo] [int] NOT NULL,
	[SDID] [int] NOT NULL,
	[SDIs] [decimal](18, 2) NOT NULL,
	[SDiscount] [decimal](18, 2) NOT NULL,
	[DDisCashierID] [int] NOT NULL,
	[Nett] [decimal](18, 2) NOT NULL,
	[LocationID] [int] NOT NULL,
	[DocumentID] [int] NOT NULL,
	[BillTypeID] [int] NOT NULL,
	[SaleTypeID] [int] NOT NULL,
	[Receipt] [nvarchar](max) NULL,
	[SalesmanID] [int] NOT NULL,
	[Salesman] [nvarchar](max) NULL,
	[CustomerID] [int] NOT NULL,
	[Customer] [nvarchar](max) NULL,
	[CashierID] [int] NOT NULL,
	[Cashier] [nvarchar](max) NULL,
	[StartTime] [datetime] NOT NULL,
	[EndTime] [datetime] NOT NULL,
	[RecDate] [datetime] NOT NULL,
	[BaseUnitID] [int] NOT NULL,
	[UnitNo] [int] NOT NULL,
	[RowNo] [int] NOT NULL,
	[IsRecall] [bit] NOT NULL,
	[RecallNo] [nvarchar](max) NULL,
	[RecallAdv] [bit] NOT NULL,
	[TaxAmount] [decimal](18, 2) NOT NULL,
	[IsTax] [bit] NOT NULL,
	[TaxPercentage] [decimal](18, 2) NOT NULL,
	[IsStock] [bit] NOT NULL,
	[SuspendNo] [nvarchar](max) NULL,
	[SuspendBy] [int] NOT NULL,
	[CustomerType] [int] NOT NULL,
	[TransStatus] [int] NOT NULL,
 CONSTRAINT [PK_dbo.SuspendDetBackups] PRIMARY KEY CLUSTERED 
(
	[Idx] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion SuspendDetBackups

                #region SuspendDets
                tableName = "SuspendDets";
                query = @"CREATE TABLE [dbo].[SuspendDets](
	[Idx] [int] IDENTITY(1,1) NOT NULL,
	[ProductID] [int] NOT NULL,
	[ProductCode] [nvarchar](25) NULL,
	[RefCode] [nvarchar](25) NULL,
	[BarCodeFull] [bigint] NOT NULL,
	[Descrip] [nvarchar](50) NULL,
	[BatchNo] [nvarchar](50) NULL,
	[SerialNo] [nvarchar](50) NULL,
	[ExpiaryDate] [datetime] NULL,
	[Cost] [decimal](18, 2) NOT NULL,
	[AvgCost] [decimal](18, 2) NOT NULL,
	[Price] [decimal](18, 2) NULL,
	[Qty] [decimal](18, 2) NOT NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[UnitOfMeasureID] [int] NOT NULL,
	[UnitOfMeasureName] [nvarchar](10) NULL,
	[ConvertFactor] [decimal](18, 2) NOT NULL,
	[IDI1] [int] NOT NULL,
	[IDis1] [decimal](18, 2) NOT NULL,
	[IDiscount1] [decimal](18, 2) NOT NULL,
	[IDI1CashierID] [int] NOT NULL,
	[IDI2] [int] NOT NULL,
	[IDis2] [decimal](18, 2) NOT NULL,
	[IDiscount2] [decimal](18, 2) NOT NULL,
	[IDI2CashierID] [int] NOT NULL,
	[IDI3] [int] NOT NULL,
	[IDis3] [decimal](18, 2) NOT NULL,
	[IDiscount3] [decimal](18, 2) NOT NULL,
	[IDI3CashierID] [int] NOT NULL,
	[IDI4] [int] NOT NULL,
	[IDis4] [decimal](18, 2) NOT NULL,
	[IDiscount4] [decimal](18, 2) NOT NULL,
	[IDI4CashierID] [int] NOT NULL,
	[IDI5] [int] NOT NULL,
	[IDis5] [decimal](18, 2) NOT NULL,
	[IDiscount5] [decimal](18, 2) NOT NULL,
	[IDI5CashierID] [int] NOT NULL,
	[Rate] [decimal](18, 2) NOT NULL,
	[IsSDis] [bit] NOT NULL,
	[SDNo] [int] NOT NULL,
	[SDID] [int] NOT NULL,
	[SDIs] [decimal](18, 2) NOT NULL,
	[SDiscount] [decimal](18, 2) NOT NULL,
	[DDisCashierID] [int] NOT NULL,
	[Nett] [decimal](18, 2) NOT NULL,
	[LocationID] [int] NOT NULL,
	[DocumentID] [int] NOT NULL,
	[BillTypeID] [int] NOT NULL,
	[SaleTypeID] [int] NOT NULL,
	[Receipt] [nvarchar](10) NULL,
	[SalesmanID] [int] NOT NULL,
	[Salesman] [nvarchar](15) NULL,
	[CustomerID] [int] NOT NULL,
	[Customer] [nvarchar](15) NULL,
	[CashierID] [int] NOT NULL,
	[Cashier] [nvarchar](15) NULL,
	[StartTime] [datetime] NOT NULL,
	[EndTime] [datetime] NOT NULL,
	[RecDate] [datetime] NOT NULL,
	[BaseUnitID] [int] NOT NULL,
	[UnitNo] [int] NOT NULL,
	[RowNo] [int] NOT NULL,
	[IsRecall] [bit] NOT NULL,
	[RecallNo] [varchar](50) NULL,
	[RecallAdv] [bit] NOT NULL,
	[TaxAmount] [decimal](18, 4) NOT NULL,
	[IsTax] [bit] NOT NULL,
	[TaxPercentage] [decimal](18, 4) NOT NULL,
	[IsStock] [bit] NOT NULL,
	[SuspendNo] [nvarchar](50) NULL,
	[SuspendBy] [int] NOT NULL,
	[CustomerType] [int] NOT NULL,
	[TransStatus] [int] NOT NULL,
	[IsPromotionApplied] [bit] NOT NULL,
	[PromotionID] [int] NOT NULL,
	[IsPromotion] [bit] NOT NULL,
	[InvPriceLevelID] [int] NOT NULL,
	[ItemSerial] [nvarchar](50) NULL,
	[warranty] [nvarchar](50) NULL,
	[TableNumber] [int] NOT NULL,
	[PrinterType] [int] NOT NULL,
	[IsPritRpt] [bit] NOT NULL,
	[ReferenceProductId] [int] NULL,
	[ReferenceProductRow] [int] NULL,
	[IsAddonItem] [bit] NOT NULL,
	[IsTaxEnable] [bit] NOT NULL,
	[TaxCode] [nvarchar](50) NULL,
	[SplitItemReceiptNo] [nvarchar](50) NULL,
	[ProductRemark] [varchar](200) NULL,
	[ServingUnit] [varchar](50) NULL,
	[OrderStatus] [int] NULL,
	[DeploCardNo] [varchar](50) NOT NULL,
	[IsShowOnBill] [bit] NOT NULL,
	[NoOfCustomers] [int] NOT NULL,
	[KitchenCode] [varchar](10) NOT NULL,
	[ServingUnitId] [int] NOT NULL,
	[OrigUnitNo] [int] NOT NULL,
 CONSTRAINT [PK_dbo.SuspendDets] PRIMARY KEY CLUSTERED 
(
	[Idx] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion SuspendDets

                #region SuspendHedBackups
                tableName = "SuspendHedBackups";
                query = @"CREATE TABLE [dbo].[SuspendHedBackups](
	[Idx] [int] IDENTITY(1,1) NOT NULL,
	[SuspendNo] [nvarchar](max) NULL,
	[Receipt] [nvarchar](max) NULL,
	[LocationID] [int] NOT NULL,
	[UnitNo] [int] NOT NULL,
	[STime] [datetime] NOT NULL,
	[SDate] [datetime] NOT NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[CashierID] [int] NOT NULL,
	[IsRecall] [bit] NOT NULL,
	[RecallReceipt] [nvarchar](max) NULL,
	[RecallCashierID] [int] NOT NULL,
	[RecallCashier] [nvarchar](max) NULL,
	[RecallUnitNo] [int] NOT NULL,
	[TransStatus] [int] NOT NULL,
 CONSTRAINT [PK_dbo.SuspendHedBackups] PRIMARY KEY CLUSTERED 
(
	[Idx] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion SuspendHedBackups

                #region SuspendHeds
                tableName = "SuspendHeds";
                query = @"CREATE TABLE [dbo].[SuspendHeds](
	[Idx] [int] IDENTITY(1,1) NOT NULL,
	[SuspendNo] [nvarchar](50) NULL,
	[Receipt] [nvarchar](50) NULL,
	[LocationID] [int] NOT NULL,
	[UnitNo] [int] NOT NULL,
	[STime] [datetime] NOT NULL,
	[SDate] [datetime] NOT NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[CashierID] [int] NOT NULL,
	[IsRecall] [bit] NOT NULL,
	[RecallReceipt] [nvarchar](50) NULL,
	[RecallCashierID] [int] NULL,
	[RecallCashier] [nvarchar](20) NULL,
	[RecallUnitNo] [int] NULL,
	[RecallTime] [datetime] NOT NULL,
	[TransStatus] [int] NOT NULL,
	[TokenNumber] [nvarchar](50) NULL,
	[NextBillDate] [int] NOT NULL,
	[CustomerId] [int] NOT NULL,
	[TableNumber] [int] NOT NULL,
	[OrderStatus] [int] NULL,
	[OrigSuspendNo] [varchar](50) NULL,
 CONSTRAINT [PK_dbo.SuspendHeds] PRIMARY KEY CLUSTERED 
(
	[Idx] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion SuspendHeds

                #region SuspendPaymentDets
                tableName = "SuspendPaymentDets";
                query = @"CREATE TABLE [dbo].[SuspendPaymentDets](
	[Idx] [int] IDENTITY(1,1) NOT NULL,
	[RowNo] [int] NOT NULL,
	[PayTypeID] [int] NOT NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[Balance] [decimal](18, 2) NOT NULL,
	[SDate] [datetime] NOT NULL,
	[Receipt] [nvarchar](max) NULL,
	[LocationID] [int] NOT NULL,
	[CashierID] [int] NOT NULL,
	[UnitNo] [int] NOT NULL,
	[BillTypeID] [int] NOT NULL,
	[RefNo] [nvarchar](max) NULL,
	[BankId] [int] NOT NULL,
	[ChequeDate] [datetime] NOT NULL,
	[IsRecallAdv] [bit] NOT NULL,
	[RecallNo] [nvarchar](max) NULL,
	[Descrip] [nvarchar](max) NULL,
	[EnCodeName] [nvarchar](max) NULL,
	[SuspendNo] [nvarchar](max) NULL,
	[SuspendBy] [int] NOT NULL,
	[IsDeleteOnRecall] [bit] NOT NULL,
 CONSTRAINT [PK_dbo.SuspendPaymentDets] PRIMARY KEY CLUSTERED 
(
	[Idx] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion SuspendPaymentDets

                #region SysCompanies
                tableName = "SysCompanies";
                query = @"CREATE TABLE [dbo].[SysCompanies](
	[SysCompanyID] [int] IDENTITY(1,1) NOT NULL,
	[CompanyCode] [nvarchar](max) NOT NULL,
	[CompanyName] [nvarchar](max) NOT NULL,
	[SysGroupOfCompanyId] [int] NOT NULL,
	[OtherBusinessName1] [nvarchar](max) NULL,
	[OtherBusinessName2] [nvarchar](max) NULL,
	[OtherBusinessName3] [nvarchar](max) NULL,
	[Address1] [nvarchar](max) NULL,
	[Address2] [nvarchar](max) NULL,
	[Address3] [nvarchar](max) NULL,
	[Telephone] [nvarchar](max) NULL,
	[Mobile] [nvarchar](max) NULL,
	[Fax] [nvarchar](max) NULL,
	[ContactPerson] [nvarchar](max) NULL,
	[Website] [nvarchar](max) NULL,
	[TaxID1] [nvarchar](max) NULL,
	[TaxID2] [nvarchar](max) NULL,
	[TaxID3] [nvarchar](max) NULL,
	[TaxRegistrationNo1] [nvarchar](max) NULL,
	[TaxRegistrationNo2] [nvarchar](max) NULL,
	[TaxRegistrationNo3] [nvarchar](max) NULL,
	[IsVat] [bit] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
 CONSTRAINT [PK_dbo.SysCompanies] PRIMARY KEY CLUSTERED 
(
	[SysCompanyID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion SysCompanies

                #region SysConfigurations
                tableName = "SysConfigurations";
                query = @"CREATE TABLE [dbo].[SysConfigurations](
	[ConfigId] [bigint] IDENTITY(1,1) NOT NULL,
	[SysName] [nvarchar](max) NULL,
	[VAT] [decimal](18, 2) NOT NULL,
	[NBT] [decimal](18, 2) NOT NULL,
	[MaxLoginAttemts] [int] NOT NULL,
	[BatchWiseGRN] [bit] NOT NULL,
	[Version] [nvarchar](max) NULL,
	[IsTaxInclusiveToCost] [bit] NOT NULL,
	[CreateGroupOfCompanies] [bit] NOT NULL,
	[CreateCompanies] [bit] NOT NULL,
 CONSTRAINT [PK_dbo.SysConfigurations] PRIMARY KEY CLUSTERED 
(
	[ConfigId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion SysConfigurations

                #region SysGroupOfCompanies
                tableName = "SysGroupOfCompanies";
                query = @"CREATE TABLE [dbo].[SysGroupOfCompanies](
	[SysGroupOfCompanyId] [int] IDENTITY(1,1) NOT NULL,
	[GroupOfCompanyCode] [nvarchar](max) NOT NULL,
	[GroupOfCompanyName] [nvarchar](max) NOT NULL,
	[CompanyGmail] [nvarchar](max) NULL,
	[CompanyVatNumber] [nvarchar](max) NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[CompanyLogo] [varbinary](max) NULL,
	[CompanyLogoType] [nvarchar](max) NULL,
	[CompanyLogoName] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.SysGroupOfCompanies] PRIMARY KEY CLUSTERED 
(
	[SysGroupOfCompanyId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion SysGroupOfCompanies

                #region SysLocationMappers
                tableName = "SysLocationMappers";
                query = @"CREATE TABLE [dbo].[SysLocationMappers](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[MainLocationId] [int] NOT NULL,
	[SubLocationId] [int] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NULL,
	[DataTransfer] [int] NOT NULL,
	[IsActive] [bit] NOT NULL,
 CONSTRAINT [PK_SysLocationMapper] PRIMARY KEY CLUSTERED 
(
	[MainLocationId] ASC,
	[SubLocationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion SysLocationMappers

                #region SysLocations
                tableName = "SysLocations";
                query = @"CREATE TABLE [dbo].[SysLocations](
	[SysLocationID] [int] IDENTITY(1,1) NOT NULL,
	[LocationCode] [nvarchar](max) NOT NULL,
	[LocationName] [nvarchar](max) NOT NULL,
	[Address1] [nvarchar](max) NULL,
	[Address2] [nvarchar](max) NULL,
	[Address3] [nvarchar](max) NULL,
	[Telephone] [nvarchar](max) NULL,
	[Fax] [nvarchar](max) NULL,
	[Email] [nvarchar](max) NULL,
	[ContactPersonName] [nvarchar](max) NULL,
	[OtherBusinessName] [nvarchar](max) NULL,
	[LocationPrefixCode] [nvarchar](max) NULL,
	[IsVAT] [bit] NOT NULL,
	[IsStockLocation] [bit] NOT NULL,
	[IsHeadOffice] [bit] NOT NULL,
	[LocationIP] [nvarchar](max) NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NULL,
	[DataTransfer] [int] NOT NULL,
	[CostCenter] [nvarchar](max) NULL,
	[IsShowRoom] [bit] NOT NULL,
	[LocationTypeId] [int] NOT NULL,
 CONSTRAINT [PK_dbo.SysLocations] PRIMARY KEY CLUSTERED 
(
	[SysLocationID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion SysLocations

                #region SysLocationTypes
                tableName = "SysLocationTypes";
                query = @"CREATE TABLE [dbo].[SysLocationTypes](
	[Id] [int] NOT NULL,
	[Code] [varchar](50) NOT NULL,
	[Description] [varchar](500) NOT NULL,
	[IsActive] [bit] NOT NULL,
 CONSTRAINT [PK_SysLocationTypes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion SysLocationTypes

                #region SysUserFunctions
                tableName = "SysUserFunctions";
                query = @"CREATE TABLE [dbo].[SysUserFunctions](
	[SysUserFunctionID] [int] IDENTITY(1,1) NOT NULL,
	[FunctionName] [nvarchar](30) NOT NULL,
	[FunctionDescription] [nvarchar](100) NOT NULL,
	[Order] [int] NOT NULL,
	[TypeID] [int] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[IsValue] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[FormId] [int] NOT NULL,
 CONSTRAINT [PK_dbo.SysUserFunctions] PRIMARY KEY CLUSTERED 
(
	[SysUserFunctionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion SysUserFunctions

                #region SysUserGroupPermissions
                tableName = "SysUserGroupPermissions";
                query = @"CREATE TABLE [dbo].[SysUserGroupPermissions](
	[SysUserGroupPermissionID] [int] IDENTITY(1,1) NOT NULL,
	[FunctionName] [nvarchar](100) NOT NULL,
	[FunctionDescription] [nvarchar](250) NOT NULL,
	[Order] [int] NOT NULL,
	[Value] [decimal](18, 2) NOT NULL,
	[MaxValue] [decimal](18, 2) NOT NULL,
	[Type] [nvarchar](max) NULL,
	[TypeID] [int] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[IsAccess] [bit] NOT NULL,
	[Remarks] [nvarchar](500) NULL,
	[IsDelete] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[SysUserGroupId] [int] NOT NULL,
	[FormId] [int] NOT NULL,
 CONSTRAINT [PK_dbo.SysUserGroupPermissions] PRIMARY KEY CLUSTERED 
(
	[SysUserGroupPermissionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion SysUserGroupPermissions

                #region SysUserGroups
                tableName = "SysUserGroups";
                query = @"CREATE TABLE [dbo].[SysUserGroups](
	[SysUserGroupID] [int] IDENTITY(1,1) NOT NULL,
	[UserGroupName] [nvarchar](50) NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[UserGroupCode] [nvarchar](15) NOT NULL,
 CONSTRAINT [PK_dbo.SysUserGroups] PRIMARY KEY CLUSTERED 
(
	[SysUserGroupID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion SysUserGroups

                #region SysUserMasters
                tableName = "SysUserMasters";
                query = @"CREATE TABLE [dbo].[SysUserMasters](
	[SysUserMasterID] [int] IDENTITY(1,1) NOT NULL,
	[UserName] [nvarchar](50) NOT NULL,
	[UserDescription] [nvarchar](100) NOT NULL,
	[Password] [nvarchar](100) NOT NULL,
	[ConfirmPassword] [nvarchar](max) NULL,
	[UserGroupID] [bigint] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[IsUserCantChangePassword] [bit] NOT NULL,
	[IsUserMustChangePassword] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[EmployeeCode] [nvarchar](15) NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[Email] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_dbo.SysUserMasters] PRIMARY KEY CLUSTERED 
(
	[SysUserMasterID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion SysUserMasters

                #region SysUserPermissions
                tableName = "SysUserPermissions";
                query = @"CREATE TABLE [dbo].[SysUserPermissions](
	[SysUserPermissionID] [int] IDENTITY(1,1) NOT NULL,
	[EmployeeID] [int] NOT NULL,
	[EmployeeCode] [nvarchar](max) NOT NULL,
	[EnCode] [nvarchar](50) NULL,
	[FunctionName] [nvarchar](100) NOT NULL,
	[FunctionDescription] [nvarchar](250) NOT NULL,
	[Order] [int] NOT NULL,
	[Value] [decimal](18, 2) NOT NULL,
	[MaxValue] [decimal](18, 2) NOT NULL,
	[Type] [nvarchar](max) NULL,
	[TypeID] [int] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[IsAccess] [bit] NOT NULL,
	[Remarks] [nvarchar](500) NULL,
	[IsDelete] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[GroupId] [bigint] NOT NULL,
	[FormId] [int] NOT NULL,
 CONSTRAINT [PK_dbo.SysUserPermissions] PRIMARY KEY CLUSTERED 
(
	[SysUserPermissionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion SysUserPermissions

                #region SysYears
                tableName = "SysYears";
                query = @"CREATE TABLE [dbo].[SysYears](
	[SysYearsId] [int] IDENTITY(1,1) NOT NULL,
	[SysYear] [int] NOT NULL,
 CONSTRAINT [PK_dbo.SysYears] PRIMARY KEY CLUSTERED 
(
	[SysYearsId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion SysYears

                #region TableMasters
                tableName = "TableMasters";
                query = @"CREATE TABLE [dbo].[TableMasters](
	[TableMasterID] [int] IDENTITY(1,1) NOT NULL,
	[TableCode] [nvarchar](10) NOT NULL,
	[TableName] [nvarchar](max) NULL,
	[IsDelete] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[NumberOfSeats] [int] NOT NULL,
	[TableState] [nvarchar](max) NULL,
	[TablePositionX] [int] NOT NULL,
	[TablePositionY] [int] NOT NULL,
	[InterDeptId] [int] NOT NULL,
 CONSTRAINT [PK_dbo.TableMasters] PRIMARY KEY CLUSTERED 
(
	[TableMasterID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion TableMasters

                #region Taxes
                tableName = "Taxes";
                query = @"CREATE TABLE [dbo].[Taxes](
	[TaxID] [int] IDENTITY(1,1) NOT NULL,
	[TaxCode] [nvarchar](10) NOT NULL,
	[TaxName] [nvarchar](50) NOT NULL,
	[TaxPercentage] [decimal](18, 2) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[IsTaxOnTax] [bit] NOT NULL,
	[IsPurchasingTax] [bit] NOT NULL,
	[IsSellingTax] [bit] NOT NULL,
	[IsServiceCharge] [bit] NOT NULL,
	[isExcludeTax] [bit] NOT NULL,
 CONSTRAINT [PK_dbo.Taxes] PRIMARY KEY CLUSTERED 
(
	[TaxID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
IF Not EXISTS (SELECT * FROM Taxes WHERE [TaxName] = 'Dine in A/C Charges' )
INSERT [dbo].[Taxes] ( [TaxCode], [TaxName], [TaxPercentage], [IsActive], [IsDelete], [GroupOfCompanyID], [CompanyID], [LocationId], 
[CreatedUser], [CreatedDate], [ModifiedUser], [ModifiedDate], [DataTransfer], [IsTaxOnTax], [IsPurchasingTax], [IsSellingTax], [IsServiceCharge],
[isExcludeTax]) VALUES ( N'101', N'Dine in A/C Charges', CAST(20.00 AS Decimal(18, 2)), 1, 0, 1, 1, 3, N'ADMIN', CAST(N'2023-04-27T15:20:18.590' AS DateTime),
N'Aruna ', CAST(N'2023-04-27T15:38:02.427' AS DateTime), 0, 0, 0, 0, 1, 0)";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion Taxes

                #region TempItemTaxes
                tableName = "TempItemTaxes";
                query = @"CREATE TABLE [dbo].[TempItemTaxes](
	[Idx] [bigint] IDENTITY(1,1) NOT NULL,
	[LocationId] [int] NOT NULL,
	[UnitNo] [varchar](20) NULL,
	[Receipt] [char](10) NULL,
	[TDate] [date] NOT NULL,
	[RowNo] [int] NOT NULL,
	[ProductId] [bigint] NOT NULL,
	[Nett] [decimal](18, 2) NOT NULL,
	[TaxId] [bigint] NOT NULL,
	[TaxCode] [char](50) NULL,
	[TaxName] [char](50) NULL,
	[TaxRate] [decimal](18, 2) NOT NULL,
	[CalcAmt] [decimal](18, 2) NOT NULL,
	[TaxAmount] [decimal](18, 2) NOT NULL,
	[ZNo] [bigint] NOT NULL,
	[Online] [smallint] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 CONSTRAINT [PK_dbo.TempItemTaxes] PRIMARY KEY CLUSTERED 
(
	[Idx] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion TempItemTaxes

                #region tmpItem
                tableName = "tmpItem";
                query = @"CREATE TABLE [dbo].[tmpItem](
	[ProductId] [nvarchar](255) NULL,
	[ProductCode] [nvarchar](255) NULL,
	[ProductName] [nvarchar](255) NULL,
	[NameOnInvoice] [nvarchar](255) NULL,
	[IsRowMaterial] [float] NULL,
	[IsCountable] [float] NULL,
	[IsScaleItem] [float] NULL,
	[IsActive] [float] NULL,
	[IsDelete] [float] NULL,
	[DepartmentId] [nvarchar](255) NULL,
	[CategoryId] [nvarchar](255) NULL,
	[CatDescri] [nvarchar](255) NULL,
	[SubCategoryId] [nvarchar](255) NULL,
	[CostPrice] [nvarchar](255) NULL,
	[SellingPrice] [float] NULL,
	[ReOrderLevel] [nvarchar](255) NULL,
	[ReOrderQuantity] [nvarchar](255) NULL,
	[LocationWiseStock] [nvarchar](255) NULL,
	[Barcode] [nvarchar](255) NULL,
	[IsItemLock] [nvarchar](255) NULL,
	[GroupOfCompanyID] [nvarchar](255) NULL,
	[CompanyID] [nvarchar](255) NULL,
	[LocationId] [nvarchar](255) NULL,
	[RefCode01] [nvarchar](255) NULL,
	[RefCode02] [nvarchar](255) NULL,
	[WastagePrc] [nvarchar](255) NULL,
	[PurchasingUnit] [nvarchar](255) NULL,
	[IsDiscount] [nvarchar](255) NULL,
	[IsCostOnReceipe] [nvarchar](255) NULL,
	[IsAddon] [nvarchar](255) NULL,
	[IsPackItem] [float] NULL,
	[PackSize] [nvarchar](255) NULL,
	[PackPrice] [nvarchar](255) NULL,
	[IsPromotion] [nvarchar](255) NULL,
	[IsFreeIssue] [nvarchar](255) NULL,
	[IsExpiry] [nvarchar](255) NULL,
	[IsTax] [float] NULL,
	[WeightPerUnit] [nvarchar](255) NULL,
	[IsUnderCost] [nvarchar](255) NULL,
	[IsBundle] [nvarchar](255) NULL,
	[MaxPrice] [nvarchar](255) NULL,
	[MinPrice] [nvarchar](255) NULL,
	[DiscountPrecentage] [nvarchar](255) NULL,
	[MaximumDiscount] [nvarchar](255) NULL,
	[FixedDiscountPercentage] [nvarchar](255) NULL,
	[FixedDiscountAmount] [nvarchar](255) NULL,
	[MaximumDiscountPercentage] [nvarchar](255) NULL,
	[AddonCategoryMasterId] [nvarchar](255) NULL,
	[IsTaxInclude] [nvarchar](255) NULL,
	[IsOpenItem] [nvarchar](255) NULL,
	[Serving Unit] [nvarchar](255) NULL,
	[DeductStockOnRecipe] [float] NULL
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion tmpItem

                #region TmpMatCons
                tableName = "TmpMatCons";
                query = @"CREATE TABLE [dbo].[TmpMatCons](
	[ProductCode] [varchar](20) NOT NULL,
	[SaleQty] [decimal](18, 2) NULL,
	[MaterialCode] [varchar](20) NULL,
	[MaterialQty] [decimal](18, 2) NOT NULL,
	[MaterialValue] [decimal](18, 2) NULL
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion TmpMatCons

                #region tmpMonthEnds
                tableName = "tmpMonthEnds";
                query = @"CREATE TABLE [dbo].[tmpMonthEnds](
	[tmpMonthEndId] [bigint] IDENTITY(1,1) NOT NULL,
	[SysLocationID] [int] NOT NULL,
	[DocumentType] [nvarchar](max) NULL,
	[Message] [nvarchar](max) NULL,
	[DocumentCount] [int] NOT NULL,
 CONSTRAINT [PK_dbo.tmpMonthEnds] PRIMARY KEY CLUSTERED 
(
	[tmpMonthEndId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion tmpMonthEnds

                #region TmpProductStockDetails
                tableName = "TmpProductStockDetails";
                query = @"CREATE TABLE [dbo].[TmpProductStockDetails](
	[TmpProductStockDetailsID] [int] IDENTITY(1,1) NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationID] [int] NOT NULL,
	[ToLocationName] [nvarchar](max) NULL,
	[GivenDate] [datetime] NOT NULL,
	[ProductID] [int] NOT NULL,
	[ProductCode] [nvarchar](max) NULL,
	[ProductName] [nvarchar](max) NULL,
	[TransactionType] [int] NOT NULL,
	[TransactionNo] [nvarchar](max) NULL,
	[BatchNo] [nvarchar](max) NULL,
	[TransactionDate] [datetime] NOT NULL,
	[CostPrice] [decimal](18, 2) NOT NULL,
	[SellingPrice] [decimal](18, 2) NOT NULL,
	[AverageCost] [decimal](18, 2) NOT NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[DepartmentID] [int] NOT NULL,
	[CategoryID] [int] NOT NULL,
	[SubCategoryID] [int] NOT NULL,
	[SubCategory2ID] [int] NOT NULL,
	[SupplierID] [int] NOT NULL,
	[CustomerID] [int] NOT NULL,
	[StockQty] [decimal](18, 2) NOT NULL,
	[Qty1] [decimal](18, 2) NOT NULL,
	[Qty2] [decimal](18, 2) NOT NULL,
	[Qty3] [decimal](18, 2) NOT NULL,
	[Qty4] [decimal](18, 2) NOT NULL,
	[Qty5] [decimal](18, 2) NOT NULL,
	[Qty6] [decimal](18, 2) NOT NULL,
	[Qty7] [decimal](18, 2) NOT NULL,
	[Qty8] [decimal](18, 2) NOT NULL,
	[Qty9] [decimal](18, 2) NOT NULL,
	[Qty10] [decimal](18, 2) NOT NULL,
	[UserID] [int] NOT NULL,
	[UniqueID] [int] NOT NULL,
	[GrossProfit] [decimal](18, 2) NOT NULL,
	[IsDelete] [int] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CreatedUser] [nvarchar](max) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](max) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[ZNo] [int] NOT NULL,
	[UnitNo] [int] NOT NULL,
	[SuppName] [nvarchar](max) NULL,
	[SerialNo] [int] NOT NULL,
 CONSTRAINT [PK_dbo.TmpProductStockDetails] PRIMARY KEY CLUSTERED 
(
	[TmpProductStockDetailsID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion TmpProductStockDetails

                #region TmpProductTrans
                tableName = "TmpProductTrans";
                query = @"CREATE TABLE [dbo].[TmpProductTrans](
	[StockCode] [nvarchar](25) NULL,
	[OpBal] [decimal](18, 2) NULL,
	[GRNVal] [decimal](18, 2) NULL,
	[PRNVal] [decimal](18, 2) NULL,
	[TOGInVal] [decimal](18, 2) NULL,
	[TOGOutVal] [decimal](18, 2) NULL,
	[AdjInVal] [decimal](18, 2) NULL,
	[AdjOutVal] [decimal](18, 2) NULL,
	[MatCons] [decimal](18, 2) NULL,
	[CloseBal] [decimal](18, 2) NULL,
	[SaleVal] [decimal](18, 2) NULL
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion TmpProductTrans

                #region tmpRecipes
                tableName = "tmpRecipes";
                query = @"CREATE TABLE [dbo].[tmpRecipes](
	[ReceipeId] [nvarchar](255) NULL,
	[ProductId] [nvarchar](255) NULL,
	[ProductServingUnitId] [nvarchar](255) NULL,
	[ProductQty] [float] NULL,
	[GroupOfCompanyID] [float] NULL,
	[CompanyID] [float] NULL,
	[LocationId] [float] NULL,
	[CreatedUser] [nvarchar](255) NULL,
	[CreatedDate] [nvarchar](255) NULL,
	[ModifiedUser] [nvarchar](255) NULL,
	[ModifiedDate] [nvarchar](255) NULL,
	[DataTransfer] [float] NULL,
	[MaterialId] [nvarchar](255) NULL,
	[Quantity] [float] NULL,
	[CostPrice] [nvarchar](255) NULL,
	[SellingPrice] [float] NULL
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion tmpRecipes

                #region TmpStockTrans
                tableName = "TmpStockTrans";
                query = @"CREATE TABLE [dbo].[TmpStockTrans](
	[LocationID] [bigint] NOT NULL,
	[StockCode] [nvarchar](25) NULL,
	[BatchNo] [nvarchar](25) NULL,
	[Qty] [decimal](18, 2) NOT NULL,
	[TransactionType] [nvarchar](50) NULL,
	[TransactionNo] [nvarchar](20) NULL,
	[TransactionDate] [datetime] NULL,
	[CostPrice] [decimal](18, 2) NULL,
	[SellingPrice] [decimal](18, 2) NULL,
	[ZNo] [int] NULL,
	[UnitNo] [int] NULL,
	[ToLocationName] [nvarchar](50) NULL
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion TmpStockTrans

                #region tmpUom
                tableName = "tmpUom";
                query = @"CREATE TABLE [dbo].[tmpUom](
	[Unit Of Measure Code] [nvarchar](255) NULL,
	[Unit Of Measure name] [nvarchar](255) NULL
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion tmpUom

                #region TransactionDets
                tableName = "TransactionDets";
                query = @"CREATE TABLE [dbo].[TransactionDets](
	[TransactionDetID] [int] IDENTITY(1,1) NOT NULL,
	[ProductID] [int] NOT NULL,
	[ProductCode] [nvarchar](max) NULL,
	[RefCode] [nvarchar](max) NULL,
	[BarCodeFull] [int] NOT NULL,
	[Descrip] [nvarchar](max) NULL,
	[BatchNo] [nvarchar](max) NULL,
	[SerialNo] [nvarchar](max) NULL,
	[ExpiryDate] [datetime] NULL,
	[Cost] [decimal](18, 2) NOT NULL,
	[AvgCost] [decimal](18, 2) NOT NULL,
	[Price] [decimal](18, 2) NOT NULL,
	[Qty] [decimal](18, 2) NOT NULL,
	[BalanceQty] [decimal](18, 2) NOT NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[UnitOfMeasureID] [int] NOT NULL,
	[UnitOfMeasureName] [nvarchar](max) NULL,
	[ConvertFactor] [decimal](18, 2) NOT NULL,
	[IDI1] [int] NOT NULL,
	[IDis1] [decimal](18, 2) NOT NULL,
	[IDiscount1] [decimal](18, 2) NOT NULL,
	[IDI1CashierID] [int] NOT NULL,
	[IDI2] [int] NOT NULL,
	[IDis2] [decimal](18, 2) NOT NULL,
	[IDiscount2] [decimal](18, 2) NOT NULL,
	[IDI2CashierID] [int] NOT NULL,
	[IDI3] [int] NOT NULL,
	[IDis3] [decimal](18, 2) NOT NULL,
	[IDiscount3] [decimal](18, 2) NOT NULL,
	[IDI3CashierID] [int] NOT NULL,
	[IDI4] [decimal](18, 2) NOT NULL,
	[IDis4] [decimal](18, 2) NOT NULL,
	[IDiscount4] [decimal](18, 2) NOT NULL,
	[IDI4CashierID] [int] NOT NULL,
	[IDI5] [int] NOT NULL,
	[IDis5] [decimal](18, 2) NOT NULL,
	[IDiscount5] [decimal](18, 2) NOT NULL,
	[IDI5CashierID] [int] NOT NULL,
	[Rate] [decimal](18, 2) NOT NULL,
	[IsSDis] [bit] NOT NULL,
	[SDNo] [int] NOT NULL,
	[SDID] [int] NOT NULL,
	[SDIs] [decimal](18, 2) NOT NULL,
	[SDiscount] [decimal](18, 2) NOT NULL,
	[DDisCashierID] [int] NOT NULL,
	[Nett] [decimal](18, 2) NOT NULL,
	[LocationID] [int] NOT NULL,
	[DocumentID] [int] NOT NULL,
	[BillTypeID] [int] NOT NULL,
	[SaleTypeID] [int] NOT NULL,
	[Receipt] [nvarchar](max) NULL,
	[SalesmanID] [int] NOT NULL,
	[Salesman] [nvarchar](max) NULL,
	[CustomerID] [int] NOT NULL,
	[Customer] [nvarchar](max) NULL,
	[CashierID] [int] NOT NULL,
	[Cashier] [nvarchar](max) NULL,
	[StartTime] [datetime] NOT NULL,
	[EndTime] [datetime] NOT NULL,
	[RecDate] [datetime] NOT NULL,
	[BaseUnitID] [int] NOT NULL,
	[UnitNo] [int] NOT NULL,
	[RowNo] [int] NOT NULL,
	[IsRecall] [bit] NOT NULL,
	[RecallNO] [nvarchar](max) NULL,
	[RecallAdv] [bit] NOT NULL,
	[TaxAmount] [decimal](18, 2) NOT NULL,
	[IsTax] [bit] NOT NULL,
	[TaxPercentage] [decimal](18, 2) NOT NULL,
	[IsStock] [bit] NOT NULL,
	[UpdateBy] [int] NOT NULL,
	[Status] [int] NOT NULL,
	[ZNo] [int] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[CustomerType] [int] NOT NULL,
	[TransStatus] [int] NOT NULL,
	[ZDate] [datetime] NOT NULL,
	[IsPromotionApplied] [int] NOT NULL,
	[PromotionID] [int] NOT NULL,
	[IsPromotion] [int] NOT NULL,
	[LocationIDBilling] [int] NOT NULL,
	[TableID] [int] NOT NULL,
	[OrderTerminalID] [int] NOT NULL,
	[TicketID] [int] NOT NULL,
	[OrderNo] [int] NOT NULL,
	[IsPrinted] [int] NOT NULL,
	[ItemComment] [nvarchar](max) NULL,
	[Packs] [int] NOT NULL,
	[IsCancelKOT] [bit] NOT NULL,
	[StewardID] [int] NOT NULL,
	[StewardName] [nvarchar](max) NULL,
	[ServiceCharge] [decimal](18, 2) NOT NULL,
	[ServiceChargeAmount] [decimal](18, 2) NOT NULL,
	[ShiftNo] [int] NOT NULL,
	[IsDayEnd] [bit] NOT NULL,
	[UpdateUnitNo] [int] NOT NULL,
	[InvPriceLevelID] [int] NOT NULL,
	[Online] [int] NOT NULL,
	[Deliverdate] [datetime] NOT NULL,
	[PackSize] [decimal](18, 2) NOT NULL,
	[TourAgentCode] [nvarchar](max) NULL,
	[TourAgentId] [int] NOT NULL,
	[TourAmount] [decimal](18, 2) NOT NULL,
	[TourPrecent] [decimal](18, 2) NOT NULL,
	[TourCommition] [decimal](18, 2) NOT NULL,
	[TourCommitionPaidAmount] [decimal](18, 2) NOT NULL,
	[TourAgentCompanyCode] [nvarchar](max) NULL,
	[TourAgentCompanyId] [int] NOT NULL,
	[TourCompanyAmount] [decimal](18, 2) NOT NULL,
	[TourCompanyPrecent] [decimal](18, 2) NOT NULL,
	[TourCompanyCommition] [decimal](18, 2) NOT NULL,
	[TourCompanyCommitionPaidAmount] [decimal](18, 2) NOT NULL,
	[DelvryBalQty] [decimal](18, 2) NOT NULL,
	[warranty] [decimal](18, 2) NOT NULL,
	[ItemSerial] [nvarchar](max) NULL,
	[CreditPeriod] [int] NOT NULL,
	[CopperratePrice] [decimal](18, 2) NOT NULL,
	[SellingCopperratePrice] [decimal](18, 2) NOT NULL,
	[AmountCopperratePrice] [decimal](18, 2) NOT NULL,
	[NettCopperratePrice] [decimal](18, 2) NOT NULL,
	[IsCopperratePriceEnable] [bit] NOT NULL,
	[RateCopperratePrice] [decimal](18, 2) NOT NULL,
	[IsBundleItem] [bit] NOT NULL,
	[NextBillDate] [int] NOT NULL,
	[PackPrice] [decimal](18, 2) NOT NULL,
	[IsPackSale] [bit] NOT NULL,
	[ExchageQty] [decimal](18, 2) NOT NULL,
	[DeploCardNo] [varchar](50) NULL,
	[ServingUnit] [varchar](50) NULL,
	[TableNumber] [int] NULL,
	[NoOfCustomers] [int] NULL,
	[NoOfAdults] [int] NULL,
	[NoOfChild] [int] NULL,
	[IsAddonItem] [bit] NOT NULL,
	[OrderStatus] [int] NULL,
	[StockLocationID] [int] NOT NULL,
	[ServingUnitId] [int] NOT NULL,
	[KitchenCode] [varchar](10) NOT NULL,
	[IsGLTransfer] [int] NOT NULL,
	[OrigUnitNo] [int] NOT NULL,
	[CancelRemark] [varchar](100) NULL,
 CONSTRAINT [PK_dbo.TransactionDets] PRIMARY KEY CLUSTERED 
(
	[TransactionDetID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion TransactionDets

                #region TransactionLogs
                tableName = "TransactionLogs";
                query = @"CREATE TABLE [dbo].[TransactionLogs](
	[TransactionLogID] [int] IDENTITY(1,1) NOT NULL,
	[TransactionDocumentNo] [nvarchar](max) NULL,
	[TransactionDocumentId] [int] NOT NULL,
	[FormName] [nvarchar](max) NULL,
	[TransactionDate] [datetime] NOT NULL,
	[AuditDate] [datetime] NOT NULL,
	[LoggedLocation] [nvarchar](max) NULL,
	[ReferenceNo] [nvarchar](max) NULL,
	[ComputerName] [nvarchar](max) NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CreatedUser] [nvarchar](max) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](max) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 CONSTRAINT [PK_dbo.TransactionLogs] PRIMARY KEY CLUSTERED 
(
	[TransactionLogID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion TransactionLogs

                #region TransferNoteDetails
                tableName = "TransferNoteDetails";
                query = @"CREATE TABLE [dbo].[TransferNoteDetails](
	[TransferNoteDetailID] [bigint] IDENTITY(1,1) NOT NULL,
	[TransferNoteHeaderID] [bigint] NOT NULL,
	[LineNo] [bigint] NOT NULL,
	[ProductID] [bigint] NOT NULL,
	[IsBatch] [bit] NOT NULL,
	[BatchNo] [nvarchar](50) NULL,
	[StockCode] [nvarchar](25) NULL,
	[BatchExpiryDate] [datetime] NOT NULL,
	[AvgCost] [decimal](18, 2) NOT NULL,
	[CostPrice] [decimal](18, 2) NOT NULL,
	[SellingPrice] [decimal](18, 2) NOT NULL,
	[PackID] [int] NOT NULL,
	[OrderQty] [decimal](18, 2) NOT NULL,
	[UnitOfMeasureID] [bigint] NOT NULL,
	[SerialNo] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.TransferNoteDetails] PRIMARY KEY CLUSTERED 
(
	[TransferNoteDetailID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion TransferNoteDetails

                #region TransferNoteHeaders
                tableName = "TransferNoteHeaders";
                query = @"CREATE TABLE [dbo].[TransferNoteHeaders](
	[TransferNoteHeaderID] [bigint] IDENTITY(1,1) NOT NULL,
	[CostCentreID] [int] NOT NULL,
	[DocumentNo] [nvarchar](20) NULL,
	[DocumentDate] [datetime] NOT NULL,
	[ReferenceDocumentID] [bigint] NOT NULL,
	[ReferenceNo] [nvarchar](20) NULL,
	[ToLocationID] [int] NOT NULL,
	[GrossAmount] [decimal](18, 2) NOT NULL,
	[NetAmount] [decimal](18, 2) NOT NULL,
	[Remark] [nvarchar](150) NULL,
	[DocumentStatus] [int] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[FromLocationId] [int] NOT NULL,
	[TotSellingPrice] [decimal](18, 2) NOT NULL,
	[TotCostPrice] [decimal](18, 2) NOT NULL,
	[TotQty] [decimal](18, 2) NOT NULL,
	[IsTempTOG] [bit] NOT NULL,
	[TOGType] [nvarchar](max) NULL,
	[DocumentId] [int] NOT NULL,
	[TOGDate] [datetime] NOT NULL,
	[StockTransferType] [int] NOT NULL,
	[EventId] [int] NULL,
	[NewDocNumber] [nvarchar](20) NULL,
	[RejectReason] [nvarchar](200) NULL,
	[CancelReason] [nvarchar](200) NULL,
 CONSTRAINT [PK_dbo.TransferNoteHeaders] PRIMARY KEY CLUSTERED 
(
	[TransferNoteHeaderID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion TransferNoteHeaders

                #region Unit_Of_Measure$
                tableName = "Unit_Of_Measure$";
                query = @"CREATE TABLE [dbo].[Unit_Of_Measure$](
	[Unit Of Measure Code] [nvarchar](255) NULL,
	[Unit Of Measure name] [nvarchar](255) NULL
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion Unit_Of_Measure$

                #region UnitConversions
                tableName = "UnitConversions";
                query = @"CREATE TABLE [dbo].[UnitConversions](
	[UnitConversionId] [bigint] IDENTITY(1,1) NOT NULL,
	[UnitOfMeasureId] [bigint] NOT NULL,
	[SubUnit] [nvarchar](max) NOT NULL,
	[SubUnitValue] [decimal](18, 2) NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[SubUnitSymbol] [nvarchar](max) NOT NULL,
	[BaseUnitValue] [decimal](18, 2) NOT NULL,
 CONSTRAINT [PK_dbo.UnitConversions] PRIMARY KEY CLUSTERED 
(
	[UnitConversionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion UnitConversions

                #region UnitOfMeasures
                tableName = "UnitOfMeasures";
                query = @"CREATE TABLE [dbo].[UnitOfMeasures](
	[UnitOfMeasureId] [bigint] IDENTITY(1,1) NOT NULL,
	[UnitOfMeasureCode] [nvarchar](15) NOT NULL,
	[UnitOfMeasureName] [nvarchar](50) NOT NULL,
	[Remark] [nvarchar](150) NULL,
	[IsDelete] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 CONSTRAINT [PK_dbo.UnitOfMeasures] PRIMARY KEY CLUSTERED 
(
	[UnitOfMeasureId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion UnitOfMeasures

                #region Vehicles
                tableName = "Vehicles";
                query = @"CREATE TABLE [dbo].[Vehicles](
	[VehicleID] [bigint] IDENTITY(1,1) NOT NULL,
	[RegistrationNo] [nvarchar](50) NOT NULL,
	[VehicleName] [nvarchar](50) NOT NULL,
	[EngineNo] [nvarchar](50) NULL,
	[ChassesNo] [nvarchar](50) NULL,
	[VehicleType] [nvarchar](50) NULL,
	[FuelType] [nvarchar](25) NOT NULL,
	[Make] [nvarchar](50) NULL,
	[Model] [nvarchar](50) NULL,
	[EngineCapacity] [nvarchar](max) NULL,
	[SeatingCapacity] [nvarchar](max) NULL,
	[Weight] [int] NOT NULL,
	[Remark] [nvarchar](150) NULL,
	[IsDelete] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 CONSTRAINT [PK_dbo.Vehicles] PRIMARY KEY CLUSTERED 
(
	[VehicleID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion Vehicles

                #region InvGiftVoucherDocumentNumbers
                tableName = "InvGiftVoucherDocumentNumbers";
                query = @"CREATE TABLE [dbo].[InvGiftVoucherDocumentNumbers](
	[DocumentNumberId] [bigint] IDENTITY(1,1) NOT NULL,
	[DocumentId] [int] NULL,
	[DocumentName] [varchar](50) NULL,
	[DocumentNo] [varchar](50) NULL,
	[GroupOfCompanyID] [int] NULL,
	[CompanyID] [int] NULL,
	[LocationId] [int] NULL,
 CONSTRAINT [PK_InvGiftVoucherDocumentNumbers] PRIMARY KEY CLUSTERED 
(
	[DocumentNumberId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion InvGiftVoucherDocumentNumbers

                #region InvGiftVoucherPurchaseOrderDetails
                tableName = "InvGiftVoucherPurchaseOrderDetails";
                query = @"CREATE TABLE [dbo].[InvGiftVoucherPurchaseOrderDetails](
	[InvGiftVoucherPurchaseOrderDetailID] [bigint] IDENTITY(1,1) NOT NULL,
	[GiftVoucherPurchaseOrderDetailID] [bigint] NOT NULL,
	[InvGiftVoucherPurchaseOrderHeaderID] [bigint] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationID] [int] NOT NULL,
	[DocumentID] [int] NOT NULL,
	[DocumentDate] [datetime] NOT NULL,
	[LineNo] [bigint] NOT NULL,
	[InvGiftVoucherMasterID] [bigint] NOT NULL,
	[NumberOfCount] [decimal](18, 2) NOT NULL,
	[VoucherAmount] [decimal](18, 2) NOT NULL,
	[VoucherType] [int] NOT NULL,
	[IsPurchase] [bit] NOT NULL,
	[DocumentStatus] [int] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL
)";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion InvGiftVoucherPurchaseOrderDetails

                #region InvGiftVoucherPurchaseOrderHeaders
                tableName = "InvGiftVoucherPurchaseOrderHeaders";
                query = @"CREATE TABLE [dbo].[InvGiftVoucherPurchaseOrderHeaders](
	[InvGiftVoucherPurchaseOrderHeaderID] [bigint] IDENTITY(1,1) NOT NULL,
	[GiftVoucherPurchaseOrderHeaderID] [bigint] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationID] [int] NOT NULL,
	[DocumentID] [int] NOT NULL,
	[DocumentNo] [nvarchar](20) NULL,
	[DocumentDate] [datetime] NOT NULL,
	[SupplierID] [bigint] NOT NULL,
	[ExpectedDate] [datetime] NOT NULL,
	[ExpiryDate] [datetime] NOT NULL,
	[PaymentTermID] [int] NOT NULL,
	[PaymentPeriod] [int] NOT NULL,
	[GiftVoucherAmount] [decimal](18, 2) NOT NULL,
	[GiftVoucherPercentage] [decimal](18, 2) NOT NULL,
	[GrossAmount] [decimal](18, 2) NOT NULL,
	[DiscountAmount] [decimal](18, 2) NOT NULL,
	[DiscountPercentage] [decimal](18, 2) NOT NULL,
	[OtherCharges] [decimal](18, 2) NOT NULL,
	[TaxAmount1] [decimal](18, 2) NOT NULL,
	[TaxAmount2] [decimal](18, 2) NOT NULL,
	[TaxAmount3] [decimal](18, 2) NOT NULL,
	[TaxAmount4] [decimal](18, 2) NOT NULL,
	[TaxAmount5] [decimal](18, 2) NOT NULL,
	[TaxAmount] [decimal](18, 2) NOT NULL,
	[NetAmount] [decimal](18, 2) NOT NULL,
	[CreditLimit] [decimal](18, 2) NOT NULL,
	[CreditPeriod] [int] NOT NULL,
	[ChequeLimit] [decimal](18, 2) NOT NULL,
	[ChequePeriod] [int] NOT NULL,
	[GiftVoucherQty] [int] NOT NULL,
	[Remark] [nvarchar](150) NULL,
	[ReferenceNo] [nvarchar](20) NULL,
	[VoucherType] [int] NOT NULL,
	[DocumentStatus] [int] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 )";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion InvGiftVoucherPurchaseOrderHeaders


                #region InvComboPackBundleItemPrices
                tableName = "InvComboPackBundleItemPrices";
                query = @"CREATE TABLE [dbo].[InvComboPackBundleItemPrices](
	[InvBundleItemPriceId] [int] IDENTITY(1,1) NOT NULL,
	[PromotionMasterId] [int] NOT NULL,
	[BundleName] [varchar](50) NULL,
	[InvId] [int] NOT NULL,
	[IsAllowDiscountPresentage] [bit] NOT NULL,
	[IsAllowDiscountAmount] [bit] NOT NULL,
	[ProductId] [int] NOT NULL,
	[ServingUnitId] [int] NOT NULL,
	[Quantity] [decimal](18, 2) NOT NULL,
	[DiscountValue] [decimal](18, 2) NOT NULL,
	[GroupId] [int] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	
 CONSTRAINT [PK_dbo.InvComboPackBundleItemPrices] PRIMARY KEY CLUSTERED 
(
	[InvBundleItemPriceId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion InvComboPackBundleItemPrices				  

                #region InvPriceLevelLists
                tableName = "InvPriceLevelLists";
                query = @"CREATE TABLE [dbo].[InvPriceLevelLists](
	[InvPriceLevelListID] [bigint] IDENTITY(1,1) NOT NULL,
	[PriceLevelID] [int] NOT NULL,
	[ProductID] [int] NOT NULL,
	[ServingUnitID] [int] NOT NULL,
	[CostPrice] [decimal](18, 2) NOT NULL,
	[SellingPrice] [decimal](18, 2) NOT NULL,
	[Qty] [decimal](18, 2) NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationID] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
	[IsDelete]  [bit] NOT NULL DEFAULT ((0)),
 CONSTRAINT [PK_dbo.InvPriceLevelList] PRIMARY KEY CLUSTERED 
(
	[InvPriceLevelListID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion InvPriceLevelLists

                #region InvPriceLevels
                tableName = "InvPriceLevels";
                query = @"CREATE TABLE [dbo].[InvPriceLevels](
	[InvPriceLevelID] [bigint] IDENTITY(1,1) NOT NULL,
	[PriceLevelCode] [nvarchar](15) NOT NULL,
	[PriceLevelName] [nvarchar](100) NOT NULL,
	[CostPrice] [decimal](18, 2) NOT NULL,
	[SellingPrice] [decimal](18, 2) NOT NULL,
	[Qty] [decimal](18, 2) NOT NULL,
	[ServingUnitID] [int] NOT NULL,
	[ServingUnit] [nvarchar](15) NOT NULL,
	[Remark] [nvarchar](150) NULL,
	[IsDelete] [bit] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationID] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 CONSTRAINT [PK_dbo.InvPriceLevel] PRIMARY KEY CLUSTERED 
(
	[InvPriceLevelID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion InvPriceLevels

                #region CateringMood
                tableName = "CateringMood";
                query = @"CREATE TABLE [dbo].[CateringMood](
	[CateringMoodID] [bigint] IDENTITY(1,1) NOT NULL,
	[CateringMoodName] [nvarchar](20) NOT NULL,
	[OrderSequence] [nvarchar](50) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[IsServiceCharge] [bit] NOT NULL DEFAULT ((0)),
	[ModifiedDate] [datetime] NOT NULL DEFAULT ('1900-01-01T00:00:00.000'),
	[CompanyId] [int] NOT NULL DEFAULT ((0)),
 CONSTRAINT [PK_dbo.CateringMood] PRIMARY KEY CLUSTERED 
(
	[CateringMoodID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
IF NOT EXISTS(SELECT * FROM CateringMood WHERE CateringMoodName = 'Dine in A/C') 
 INSERT [dbo].[CateringMood] ([CateringMoodName], [OrderSequence], [IsActive], [IsServiceCharge], [ModifiedDate],[CompanyId]) 
  VALUES (N'Dine in A/C', N'5', 1, 1, CAST(N'2023-04-27T15:35:49.040' AS DateTime),1);";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion CateringMoods

                #region InvGiftVoucherPurchaseHeaders
                tableName = "InvGiftVoucherPurchaseHeaders";
                query = @"CREATE TABLE [dbo].[InvGiftVoucherPurchaseHeaders](
	[InvGiftVoucherPurchaseHeaderID] [bigint] IDENTITY(1,1) NOT NULL,
	[GiftVoucherPurchaseHeaderID] [bigint] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationID] [int] NOT NULL,
	[CostCentreID] [int] NOT NULL,
	[DocumentID] [int] NOT NULL,
	[DocumentNo] [nvarchar](20) NULL,
	[DocumentDate] [datetime] NOT NULL,
	[SupplierID] [bigint] NOT NULL,
	[PartyInvoiceDate] [datetime] NOT NULL,
	[DispatchDate] [datetime] NOT NULL,
	[GiftVoucherAmount] [decimal](18, 2) NOT NULL,
	[GiftVoucherPercentage] [decimal](18, 2) NOT NULL,
	[GrossAmount] [decimal](18, 2) NOT NULL,
	[DiscountAmount] [decimal](18, 2) NOT NULL,
	[DiscountPercentage] [decimal](18, 2) NOT NULL,
	[OtherCharges] [decimal](18, 2) NOT NULL,
	[TaxAmount1] [decimal](18, 2) NOT NULL,
	[TaxAmount2] [decimal](18, 2) NOT NULL,
	[TaxAmount3] [decimal](18, 2) NOT NULL,
	[TaxAmount4] [decimal](18, 2) NOT NULL,
	[TaxAmount5] [decimal](18, 2) NOT NULL,
	[TaxAmount] [decimal](18, 2) NOT NULL,
	[NetAmount] [decimal](18, 2) NOT NULL,
	[CreditLimit] [decimal](18, 2) NOT NULL,
	[CreditPeriod] [int] NOT NULL,
	[ChequeLimit] [decimal](18, 2) NOT NULL,
	[ChequePeriod] [int] NOT NULL,
	[GiftVoucherQty] [int] NOT NULL,
	[Remark] [nvarchar](150) NULL,
	[ReferenceNo] [nvarchar](20) NULL,
	[PartyInvoiceNo] [nvarchar](20) NULL,
	[DispatchNo] [nvarchar](20) NULL,
	[PaymentTermID] [int] NOT NULL,
	[PaymentPeriod] [int] NOT NULL,
	[DeliveryPerson] [nvarchar](150) NULL,
	[DeliveryPersonNICNo] [nvarchar](150) NULL,
	[VehicleNo] [nvarchar](150) NULL,
	[ReferenceDocumentDocumentID] [int] NOT NULL,
	[ReferenceDocumentID] [bigint] NOT NULL,
	[VoucherType] [int] NOT NULL,
	[DocumentStatus] [int] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 )";

                ExecuteMainQuery(Stringsqlconnection);

				#endregion InvGiftVoucherPurchaseHeaders

				#region InvGiftVoucherPurchaseDetails
				tableName = "InvGiftVoucherPurchaseDetails";
				query = @"CREATE TABLE [dbo].[InvGiftVoucherPurchaseDetails](
	[InvGiftVoucherPurchaseDetailID] [bigint] IDENTITY(1,1) NOT NULL,
	[GiftVoucherPurchaseDetailID] [bigint] NOT NULL,
	[InvGiftVoucherPurchaseHeaderID] [bigint] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationID] [int] NOT NULL,
	[DocumentID] [int] NOT NULL,
	[DocumentDate] [datetime] NOT NULL,
	[LineNo] [bigint] NOT NULL,
	[InvGiftVoucherMasterID] [bigint] NOT NULL,
	[NumberOfCount] [decimal](18, 2) NOT NULL,
	[VoucherAmount] [decimal](18, 2) NOT NULL,
	[VoucherType] [int] NOT NULL,
	[IsTransfer] [bit] NOT NULL,
	[DocumentStatus] [int] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 )";

				ExecuteMainQuery(Stringsqlconnection);

				#endregion InvGiftVoucherPurchaseDetails

				#region InvGiftVoucherTransferNoteHeaders
				tableName = "InvGiftVoucherTransferNoteHeaders";
				query = @"CREATE TABLE [dbo].[InvGiftVoucherTransferNoteHeaders](
	[InvGiftVoucherTransferNoteHeaderID] [bigint] IDENTITY(1,1) NOT NULL,
	[GiftVoucherTransferNoteHeaderID] [bigint] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationID] [int] NOT NULL,
	[CostCentreID] [int] NOT NULL,
	[DocumentID] [int] NOT NULL,
	[DocumentNo] [nvarchar](20) NULL,
	[DocumentDate] [datetime] NOT NULL,
	[TransferTypeID] [int] NOT NULL,
	[GiftVoucherAmount] [decimal](18, 2) NOT NULL,
	[GiftVoucherPercentage] [decimal](18, 2) NOT NULL,
	[GiftVoucherQty] [int] NOT NULL,
	[Remark] [nvarchar](150) NULL,
	[ReferenceNo] [nvarchar](20) NULL,
	[ReferenceDocumentDocumentID] [int] NOT NULL,
	[ReferenceDocumentID] [bigint] NOT NULL,
	[ReferenceDocumentNo] [nvarchar](20) NULL,
	[ToLocationID] [int] NOT NULL,
	[VoucherType] [int] NOT NULL,
	[DocumentStatus] [int] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 )";

				ExecuteMainQuery(Stringsqlconnection);

				#endregion InvGiftVoucherTransferNoteHeaders

				#region InvGiftVoucherTransferNoteDetails
				tableName = "InvGiftVoucherTransferNoteDetails";
				query = @"CREATE TABLE [dbo].[InvGiftVoucherTransferNoteDetails](
	[InvGiftVoucherTransferNoteDetailID] [bigint] IDENTITY(1,1) NOT NULL,
	[GiftVoucherTransferNoteDetailID] [bigint] NOT NULL,
	[InvGiftVoucherTransferNoteHeaderID] [bigint] NOT NULL,
	[CompanyID] [int] NOT NULL,
	[LocationID] [int] NOT NULL,
	[DocumentID] [int] NOT NULL,
	[DocumentDate] [datetime] NOT NULL,
	[LineNo] [bigint] NOT NULL,
	[InvGiftVoucherMasterID] [bigint] NOT NULL,
	[NumberOfCount] [decimal](18, 2) NOT NULL,
	[VoucherAmount] [decimal](18, 2) NOT NULL,
	[ToLocationID] [int] NOT NULL,
	[VoucherType] [int] NOT NULL,
	[DocumentStatus] [int] NOT NULL,
	[GroupOfCompanyID] [int] NOT NULL,
	[CreatedUser] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedUser] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[DataTransfer] [int] NOT NULL,
 )";

				ExecuteMainQuery(Stringsqlconnection);

                #endregion InvGiftVoucherTransferNoteDetails

                #region InvRequestNotePOTransactions
                tableName = "InvRequestNotePOTransactions";
                query = @"CREATE TABLE [dbo].[InvRequestNotePOTransactions](
	                        [POReqNoteNo] [bigint] IDENTITY(1,1) NOT NULL,
	                        [RequestNoteHeaderID] [bigint] NOT NULL,
	                        [PurchaseOrderDetailID] [bigint] NOT NULL,
	                        [PurchaseOrderDocumentNo] [varchar](50) NULL,
	                        [RequestNoteDocumentNo] [varchar](50) NULL,
	                        [LocationID] [bigint] NULL,
	                        [ProductID] [bigint] NULL,
	                        [QTY] [decimal](18, 0) NULL,
                            [ReqNoteCreatedDate] datetime,
                            [ReqNoteAcceptedDate] datetime,
                            [POCreateDate] datetime,
							[IssueQtY]  [decimal](18, 0) NULL,
							[BalanceQtY] [decimal](18, 0) NULL
                         CONSTRAINT [PK_RequestNotePOTransaction] PRIMARY KEY CLUSTERED 
                        (
	                        [POReqNoteNo] ASC
                        )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                        ) ON [PRIMARY]";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion InvGiftVoucherTransferNoteDetails

                #endregion Table

                #region The columns have added and altered 

                query = @"IF COL_LENGTH('RequestNoteDetails','IsPoTransfer') IS NULL
                            begin
                            alter table RequestNoteDetails Add IsPoTransfer bit default (0) NOT NULL
                            end;";

                ExecuteMainQuery(Stringsqlconnection);

                query = @"IF COL_LENGTH('RequestNoteDetails','SupplierId') IS NULL
                            begin
                            alter table RequestNoteDetails Add SupplierId int default (0) NOT NULL
                            end;";

                ExecuteMainQuery(Stringsqlconnection);

                query = @"IF COL_LENGTH('RequestNoteHeaders','IsPoTransfer') IS NULL
                            begin
                            alter table RequestNoteHeaders Add IsPoTransfer bit default (0) NOT NULL
                            end;";

                ExecuteMainQuery(Stringsqlconnection);

                            query = @"IF COL_LENGTH('TransferNoteHeaders','GRNNo') IS NULL
                            begin
                            alter table TransferNoteHeaders Add GRNNo nvarchar(100) DEFAULT ('') NOT NULL
                            end;";

                ExecuteMainQuery(Stringsqlconnection);


                query = @"IF COL_LENGTH('Customers','SenderPreference') IS NULL
                            begin
                            alter table Customers Add SenderPreference int default (0) NOT NULL
                            end;";

                ExecuteMainQuery(Stringsqlconnection);

                query = @"IF COL_LENGTH('SysLocations','LocationTypeId') IS NULL
                            begin
                            alter table SysLocations Add LocationTypeId int default (0) NOT NULL
                            end;";

                ExecuteMainQuery(Stringsqlconnection);

                query = @"IF COL_LENGTH('Receipes','IsActive') IS NULL
                            begin
                            alter table Receipes Add IsActive bit default (0) NOT NULL
                            end;";

                ExecuteMainQuery(Stringsqlconnection);

                query = @"IF COL_LENGTH('InvPromotionMasters','IsActive') IS NULL
                            begin
                            alter table InvPromotionMasters Add IsActive bit default (0) NOT NULL
                            end;";

                ExecuteMainQuery(Stringsqlconnection);

                query = @"IF COL_LENGTH('Products','ProductDesp') IS NULL
                            begin
                            ALTER TABLE Products ADD ProductDesp nvarchar(100) DEFAULT ('') NOT NULL
                            end;";

                ExecuteMainQuery(Stringsqlconnection);

                query = @"IF COL_LENGTH('PurchaseHeaders','IsTOGTransfer') IS NULL
                            begin
                            ALTER TABLE PurchaseHeaders ADD IsTOGTransfer bit default (0) NOT NULL
                            end;";

                ExecuteMainQuery(Stringsqlconnection);

                query = @"IF COL_LENGTH('PurchaseHeaders','IsTOGTransfer') IS NULL
                            begin
                            ALTER TABLE PurchaseHeaders ADD IsTOGTransfer bit default (0) NOT NULL
                            end;";

                ExecuteMainQuery(Stringsqlconnection);

                query = @"IF COL_LENGTH('SupplierProducts','CostPrice') IS NULL
                            begin
                            ALTER TABLE SupplierProducts ADD CostPrice decimal(18, 2) DEFAULT (0) NOT NULL;
                            end;";

                ExecuteMainQuery(Stringsqlconnection);

                query = @"IF COL_LENGTH('SupplierProducts','SellingPrice') IS NULL
                            begin
                            ALTER TABLE SupplierProducts ADD SellingPrice decimal(18, 2) DEFAULT (0) NOT NULL;
                            end;";

                ExecuteMainQuery(Stringsqlconnection);

                query = @"IF COL_LENGTH('LOGSupplierProducts','CostPrice') IS NULL
                            begin
                            ALTER TABLE LOGSupplierProducts ADD CostPrice decimal(18, 2) DEFAULT (0) NOT NULL;
                            end;";

                ExecuteMainQuery(Stringsqlconnection);

                query = @"IF COL_LENGTH('LOGSupplierProducts','SellingPrice') IS NULL
                            begin
                            ALTER TABLE LOGSupplierProducts ADD SellingPrice decimal(18, 2) DEFAULT (0) NOT NULL;
                            end;";

                ExecuteMainQuery(Stringsqlconnection);

                query = @"IF COL_LENGTH('InvProductMasters','ProductDesp') IS NULL
                            begin
                            ALTER TABLE InvProductMasters ADD ProductDesp nvarchar(100) DEFAULT ('') NOT NULL
                            end;";

                ExecuteMainQuery(Stringsqlconnection);

                query = @"IF COL_LENGTH('LOGProducts','ProductDesp') IS NULL
                            begin
                            ALTER TABLE LOGProducts ADD ProductDesp nvarchar(100) DEFAULT ('') NOT NULL
                            end;";

                ExecuteMainQuery(Stringsqlconnection);

                query = @"IF COL_LENGTH('SysGroupOfCompanies','AccountServerName') IS NULL
                            begin
                            ALTER TABLE SysGroupOfCompanies ADD AccountServerName nvarchar(MAX) NULL
                            end;";

                ExecuteMainQuery(Stringsqlconnection);

                query = @"IF COL_LENGTH('SysGroupOfCompanies','AccountServerUserName') IS NULL
                            begin
                            ALTER TABLE SysGroupOfCompanies ADD AccountServerUserName nvarchar(MAX) NULL
                            end;";

                ExecuteMainQuery(Stringsqlconnection);

                query = @"IF COL_LENGTH('SysGroupOfCompanies','AccountServerUserPassword') IS NULL
                            begin
                            ALTER TABLE SysGroupOfCompanies ADD AccountServerUserPassword nvarchar(MAX) NULL
                            end;";

                ExecuteMainQuery(Stringsqlconnection);

                query = @"IF COL_LENGTH('SysGroupOfCompanies','AccountServerDBName') IS NULL
                            begin
                            ALTER TABLE SysGroupOfCompanies ADD AccountServerDBName nvarchar(MAX) NULL
                            end;";

                ExecuteMainQuery(Stringsqlconnection);


                query = @"IF COL_LENGTH('Products','Target_Qty') IS NULL
                            begin
                            ALTER TABLE Products ADD Target_Qty decimal(18, 2) DEFAULT (0) NOT NULL
                            end;";
                ExecuteMainQuery(Stringsqlconnection);

                query = @"IF COL_LENGTH('Products','TypeIdTargetPeriod') IS NULL
                            begin
                            ALTER TABLE Products ADD TypeIdTargetPeriod nvarchar(MAX) NULL
                            end;";
                ExecuteMainQuery(Stringsqlconnection);

                query = @"IF COL_LENGTH('Products','TypeIdTargetType') IS NULL
                            begin
                            ALTER TABLE Products ADD TypeIdTargetType nvarchar(MAX) NULL
                            end;";
                ExecuteMainQuery(Stringsqlconnection);

                query = @"IF COL_LENGTH('SupplierProducts','IsDelete') IS NULL
                            begin
                            ALTER TABLE SupplierProducts ADD IsDelete bit default (0) NOT NULL
                            end;";
                ExecuteMainQuery(Stringsqlconnection);

                query = @" IF  not exists( SELECT  character_maximum_length
								FROM information_schema.columns
								WHERE table_name = 'Configurations' and column_name ='ConfigurationKey' and  character_maximum_length >=250  ) 
										begin
									ALTER TABLE Configurations ALTER COLUMN ConfigurationKey nvarchar(250) NULL

							End ;";
                ExecuteMainQuery(Stringsqlconnection);


				query = @" IF  not exists( SELECT  character_maximum_length
								FROM information_schema.columns
								WHERE table_name = 'Configurations' and column_name ='ConfigurationDescription' and  character_maximum_length >=250  ) 
										begin
									ALTER TABLE Configurations ALTER COLUMN ConfigurationDescription nvarchar(250) NULL

							End ;";
				ExecuteMainQuery(Stringsqlconnection);


				query = @"IF COL_LENGTH('RequestNoteAcceptanceDetails','PurchaseOrderHeaderId') IS NULL
                            begin
                            ALTER TABLE RequestNoteAcceptanceDetails ADD PurchaseOrderHeaderId bigint DEFAULT (0) ;
                            end;";

				ExecuteMainQuery(Stringsqlconnection);

				query = @"IF COL_LENGTH('RequestNoteAcceptanceDetails','RequestnoteHeaderId') IS NULL
                            begin
                            ALTER TABLE RequestNoteAcceptanceDetails ADD RequestnoteHeaderId bigint DEFAULT (0) ;
                            end;";

				ExecuteMainQuery(Stringsqlconnection);


				query = @"IF COL_LENGTH('InvRequestNotePOTransactions','PurchaseOrderHeaderID') IS NULL
                            begin
                            ALTER TABLE InvRequestNotePOTransactions ADD PurchaseOrderHeaderID bigint DEFAULT (0) ;
                            end;";

				ExecuteMainQuery(Stringsqlconnection);

				

					query = @"IF COL_LENGTH('InvRequestNotePOTransactions','FromLocationID') IS NULL
                            begin
                            ALTER TABLE InvRequestNotePOTransactions ADD FromLocationID bigint DEFAULT (0) ;
                            end;";

				ExecuteMainQuery(Stringsqlconnection);


				query = @"IF COL_LENGTH('TransferNoteDetails','RequestNoteHeaderID') IS NULL
                            begin
                            ALTER TABLE TransferNoteDetails ADD RequestNoteHeaderID  bigint not null DEFAULT (0) ;
                            end;";

				ExecuteMainQuery(Stringsqlconnection);

				query = @"IF COL_LENGTH('TransferNoteDetails','ReqNoteCreatedDate') IS NULL
                            begin
                            ALTER TABLE TransferNoteDetails ADD ReqNoteCreatedDate  datetime not null DEFAULT '1999-09-29 00:00:00.000' ;
                            end;";

				ExecuteMainQuery(Stringsqlconnection);

				query = @"IF COL_LENGTH('TransferNoteDetails','RequestNoteDocumentNo') IS NULL
                            begin
                            ALTER TABLE TransferNoteDetails ADD RequestNoteDocumentNo  varchar(200) not null DEFAULT '' ;
                            end;";

				ExecuteMainQuery(Stringsqlconnection);



				///

				query = @"IF COL_LENGTH('Products','TypeIdTargetType') IS NULL
                            begin
                           ALter Table Products ADD TypeIdTargetType nvarchar(max) NULL Default '' ;
                            end;";

				ExecuteMainQuery(Stringsqlconnection);


				query = @"IF COL_LENGTH('Products','Target_Qty') IS NULL
                            begin
                            ALter Table Products ADD  Target_Qty decimal(18,0)  Default 0 ;
                            end;";

				ExecuteMainQuery(Stringsqlconnection);



				query = @"IF COL_LENGTH('Products','TypeIdTargetPeriod') IS NULL
                            begin
                            ALter Table Products ADD  TypeIdTargetPeriod varchar(150) NULL Default   '';
                            end;";

				ExecuteMainQuery(Stringsqlconnection);



				query = @"IF COL_LENGTH('RequestNoteDetails','Remark') IS NULL
                            begin
                            ALter Table RequestNoteDetails ADD  Remark varchar(300) NULL Default   '';
                            end;";

				ExecuteMainQuery(Stringsqlconnection);





				query = @"IF COL_LENGTH('Customers','FirstName') IS NULL
                            begin
                            ALter Table Customers ADD  FirstName varchar(150) NULL Default   '';
                            end;";

				ExecuteMainQuery(Stringsqlconnection);



				query = @"IF COL_LENGTH('Customers','LastName') IS NULL
                            begin
                            ALter Table Customers ADD  LastName varchar(150) NULL Default   '';
                            end;";

				ExecuteMainQuery(Stringsqlconnection);


				query = @"IF COL_LENGTH('RequestNoteHeaders','ExpectedDeleveryDate') IS NULL
                            begin
                            ALTER TABLE RequestNoteHeaders ADD ExpectedDeleveryDate  datetime not null DEFAULT '1999-09-29 00:00:00.000' ;
                            end;";

				ExecuteMainQuery(Stringsqlconnection);

				query = @"IF COL_LENGTH('RequestNoteHeaders','CanceledDate') IS NULL
                            begin
                            ALTER TABLE RequestNoteHeaders ADD CanceledDate  datetime not null DEFAULT '1999-09-29 00:00:00.000' ;
                            end;";

				ExecuteMainQuery(Stringsqlconnection);


				query = @"IF COL_LENGTH('RequestNoteHeaders','CanceleddBy') IS NULL
                            begin
                            ALter Table RequestNoteHeaders ADD  CanceleddBy varchar(150) NULL Default   '';
                            end;";

				ExecuteMainQuery(Stringsqlconnection);

				// Alter column -- Heshari 23/04/2025
				query = @"IF EXISTS(
							SELECT*
							FROM INFORMATION_SCHEMA.COLUMNS
							WHERE TABLE_NAME = 'ProductStockMasters'
							  AND COLUMN_NAME = 'CostPrice'
							  AND (NUMERIC_PRECISION != 18 OR NUMERIC_SCALE != 3)
						)
						BEGIN
							ALTER TABLE ProductStockMasters
							ALTER COLUMN CostPrice DECIMAL(18,3);
						END;";
				ExecuteMainQuery(Stringsqlconnection);

				query = @"IF EXISTS(
							SELECT*
							FROM INFORMATION_SCHEMA.COLUMNS
							WHERE TABLE_NAME = 'ProductStockMasters'
							  AND COLUMN_NAME = 'AvgCost'
							  AND (NUMERIC_PRECISION != 18 OR NUMERIC_SCALE != 3)
						)
						BEGIN
							ALTER TABLE ProductStockMasters
							ALTER COLUMN AvgCost DECIMAL(18,3);
						END;";
				ExecuteMainQuery(Stringsqlconnection);

				query = @"IF EXISTS(
							SELECT*
							FROM INFORMATION_SCHEMA.COLUMNS
							WHERE TABLE_NAME = 'LOGProductStockMasters'
							  AND COLUMN_NAME = 'CostPrice'
							  AND (NUMERIC_PRECISION != 18 OR NUMERIC_SCALE != 3)
						)
						BEGIN
							ALTER TABLE LOGProductStockMasters
							ALTER COLUMN CostPrice DECIMAL(18,3);
						END;";
				ExecuteMainQuery(Stringsqlconnection);

				query = @"IF EXISTS(
							SELECT*
							FROM INFORMATION_SCHEMA.COLUMNS
							WHERE TABLE_NAME = 'LOGProductStockMasters'
							  AND COLUMN_NAME = 'AvgCost'
							  AND (NUMERIC_PRECISION != 18 OR NUMERIC_SCALE != 3)
						)
						BEGIN
							ALTER TABLE LOGProductStockMasters
							ALTER COLUMN AvgCost DECIMAL(18,3);
						END;";
				ExecuteMainQuery(Stringsqlconnection);
				query = @"IF EXISTS(
							SELECT*
							FROM INFORMATION_SCHEMA.COLUMNS
							WHERE TABLE_NAME = 'Products'
							  AND COLUMN_NAME = 'CostPrice'
							  AND (NUMERIC_PRECISION != 18 OR NUMERIC_SCALE != 3)
						)
						BEGIN
							ALTER TABLE Products
							ALTER COLUMN CostPrice DECIMAL(18,3);
						END;";
				ExecuteMainQuery(Stringsqlconnection);

				//  ExecuteMainQuery(Stringsqlconnection);

				#endregion Alter Columns

				#region The one time insert statement has added by GAYAN 
				#region FOR Configurations
				query = @"IF Not EXISTS(SELECT * FROM Configurations WHERE ConfigurationKey = 'BDSA')
                    Insert into Configurations(ConfigurationKey, ConfigurationDescription, EffectLocationId, ConfigurationOn, ConfigurationActive, ConfigurationDelete, CreateDate, 
                    CreateUserId, CompanyId)
                    VALUES ('BDSA','',1,1,1,0,GETDATE(),1,1)";

                    ExecuteMainQuery(Stringsqlconnection);

                #endregion

                #region FOR Configurations
                query = @"-- Check the column sizes of the Configurations table
SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Configurations';

-- Check the lengths of the data being inserted
SELECT LEN('WithoutRowMaterialInPOBasedRequestNote') AS LengthOfKey,
       LEN('') AS LengthOfDescription;

-- Original script with debugging info
IF NOT EXISTS (
    SELECT * 
    FROM Configurations 
    WHERE ConfigurationKey = 'WithoutRowMaterialInPOBasedRequestNote'
)
BEGIN
    -- Insert statement
    INSERT INTO Configurations (
        ConfigurationKey, 
        ConfigurationDescription, 
        EffectLocationId, 
        ConfigurationOn, 
        ConfigurationActive, 
        ConfigurationDelete, 
        CreateDate, 
        CreateUserId, 
        CompanyId
    )
    VALUES (
        'WithoutRowMaterialInPOBasedRequestNote', -- Ensure this fits in ConfigurationKey
        '',                                      -- Ensure this fits in ConfigurationDescription
        1, 
        0, 
        1, 
        0, 
        GETDATE(), 
        1, 
        1
    );
END
";

                ExecuteMainQuery(Stringsqlconnection);

				#endregion

				#region FOR Create PO based on Request NoteI Item Merging for all request notes 
				query = @"-- Check the column sizes of the Configurations table
SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Configurations';

-- Check the lengths of the data being inserted
SELECT LEN('ReqestNoteBasedPOItemMergeEnable') AS LengthOfKey,
       LEN('') AS LengthOfDescription;

-- Original script with debugging info
IF NOT EXISTS (
    SELECT * 
    FROM Configurations 
    WHERE ConfigurationKey = 'ReqestNoteBasedPOItemMergeEnable'
)
BEGIN
    -- Insert statement
    INSERT INTO Configurations (
        ConfigurationKey, 
        ConfigurationDescription, 
        EffectLocationId, 
        ConfigurationOn, 
        ConfigurationActive, 
        ConfigurationDelete, 
        CreateDate, 
        CreateUserId, 
        CompanyId
    )
    VALUES (
        'ReqestNoteBasedPOItemMergeEnable', -- Ensure this fits in ConfigurationKey
        '',                                      -- Ensure this fits in ConfigurationDescription
        1, 
        0, 
        1, 
        0, 
        GETDATE(), 
        1, 
        1
    );
END
";

				ExecuteMainQuery(Stringsqlconnection);

				#endregion

				#region FOR Configurations Raffels DisableGRNReject
				query = @"IF Not EXISTS(SELECT * FROM Configurations WHERE ConfigurationKey = 'DisableGRNReject')
                    Insert into Configurations(ConfigurationKey, ConfigurationDescription, EffectLocationId, ConfigurationOn, ConfigurationActive, ConfigurationDelete, CreateDate, 
                    CreateUserId, CompanyId)
                    VALUES ('DisableGRNReject','',1,0,1,0,GETDATE(),1,1)";

				ExecuteMainQuery(Stringsqlconnection);

				#endregion

				#region FOR Configurations Raffels DisableTOGReject
				query = @"IF Not EXISTS(SELECT * FROM Configurations WHERE ConfigurationKey = 'DisableTOGReject')
                    Insert into Configurations(ConfigurationKey, ConfigurationDescription, EffectLocationId, ConfigurationOn, ConfigurationActive, ConfigurationDelete, CreateDate, 
                    CreateUserId, CompanyId)
                    VALUES ('DisableTOGReject','',1,0,1,0,GETDATE(),1,1)";

				ExecuteMainQuery(Stringsqlconnection);

				#endregion

				#region FOR Configurations Raffels IsEventMandatory
				query = @"IF Not EXISTS(SELECT * FROM Configurations WHERE ConfigurationKey = 'IsEventMandatory')
                    Insert into Configurations(ConfigurationKey, ConfigurationDescription, EffectLocationId, ConfigurationOn, ConfigurationActive, ConfigurationDelete, CreateDate, 
                    CreateUserId, CompanyId)
                    VALUES ('IsEventMandatory','',1,0,1,0,GETDATE(),1,1)";

				ExecuteMainQuery(Stringsqlconnection);

				#endregion

				#region FOR Configurations Raffels DisablePOReject
				query = @"IF Not EXISTS(SELECT * FROM Configurations WHERE ConfigurationKey = 'DisablePOReject')
                    Insert into Configurations(ConfigurationKey, ConfigurationDescription, EffectLocationId, ConfigurationOn, ConfigurationActive, ConfigurationDelete, CreateDate, 
                    CreateUserId, CompanyId)
                    VALUES ('DisablePOReject','',1,0,1,0,GETDATE(),1,1)";

				ExecuteMainQuery(Stringsqlconnection);

				#endregion

				#region FOR Configurations Raffels RequestNoteAllowOnlyPO
				query = @"IF Not EXISTS(SELECT * FROM Configurations WHERE ConfigurationKey = 'RequestNoteAllowOnlyPO')
                    Insert into Configurations(ConfigurationKey, ConfigurationDescription, EffectLocationId, ConfigurationOn, ConfigurationActive, ConfigurationDelete, CreateDate, 
                    CreateUserId, CompanyId)
                    VALUES ('RequestNoteAllowOnlyPO','',1,0,1,0,GETDATE(),1,1)";

				ExecuteMainQuery(Stringsqlconnection);

				#endregion


				#region FOR Configurations Raffels RequestNoteBasedTOG
				query = @"IF Not EXISTS(SELECT * FROM Configurations WHERE ConfigurationKey = 'RequestNoteBasedTOG')
                    Insert into Configurations(ConfigurationKey, ConfigurationDescription, EffectLocationId, ConfigurationOn, ConfigurationActive, ConfigurationDelete, CreateDate, 
                    CreateUserId, CompanyId)
                    VALUES ('RequestNoteBasedTOG','',1,0,1,0,GETDATE(),1,1)";

				ExecuteMainQuery(Stringsqlconnection);

				#endregion


				#region FOR Configurations Raffels TOGGridAdditionalColumn
				query = @"IF Not EXISTS(SELECT * FROM Configurations WHERE ConfigurationKey = 'TOGGridAdditionalColumn')
                    Insert into Configurations(ConfigurationKey, ConfigurationDescription, EffectLocationId, ConfigurationOn, ConfigurationActive, ConfigurationDelete, CreateDate, 
                    CreateUserId, CompanyId)
                    VALUES ('TOGGridAdditionalColumn','',1,0,1,0,GETDATE(),1,1)";

				ExecuteMainQuery(Stringsqlconnection);

				#endregion

				#region FOR Configurations Raffels DisableServingUnit
				query = @"IF Not EXISTS(SELECT * FROM Configurations WHERE ConfigurationKey = 'DisableServingUnit')
                    Insert into Configurations(ConfigurationKey, ConfigurationDescription, EffectLocationId, ConfigurationOn, ConfigurationActive, ConfigurationDelete, CreateDate, 
                    CreateUserId, CompanyId)
                    VALUES ('DisableServingUnit','',1,0,1,0,GETDATE(),1,1)";

				ExecuteMainQuery(Stringsqlconnection);

				#endregion


				#region FOR Configurations Raffels DisableRequestedBy
				query = @"IF Not EXISTS(SELECT * FROM Configurations WHERE ConfigurationKey = 'DisableRequestedBy')
                    Insert into Configurations(ConfigurationKey, ConfigurationDescription, EffectLocationId, ConfigurationOn, ConfigurationActive, ConfigurationDelete, CreateDate, 
                    CreateUserId, CompanyId)
                    VALUES ('DisableRequestedBy','',1,0,1,0,GETDATE(),1,1)";

				ExecuteMainQuery(Stringsqlconnection);

				#endregion

				#region FOR Configurations Raffels DisablePriceRequestNote
				query = @"IF Not EXISTS(SELECT * FROM Configurations WHERE ConfigurationKey = 'DisablePriceRequestNote')
                    Insert into Configurations(ConfigurationKey, ConfigurationDescription, EffectLocationId, ConfigurationOn, ConfigurationActive, ConfigurationDelete, CreateDate, 
                    CreateUserId, CompanyId)
                    VALUES ('DisablePriceRequestNote','',1,0,1,0,GETDATE(),1,1)";

				ExecuteMainQuery(Stringsqlconnection);
				#endregion
				#region FOR Configurations Raffels EnableRequestNoteLineRemark
				query = @"IF Not EXISTS(SELECT * FROM Configurations WHERE ConfigurationKey = 'EnableRequestNoteLineRemark')
                    Insert into Configurations(ConfigurationKey, ConfigurationDescription, EffectLocationId, ConfigurationOn, ConfigurationActive, ConfigurationDelete, CreateDate, 
                    CreateUserId, CompanyId)
                    VALUES ('EnableRequestNoteLineRemark','',1,0,1,0,GETDATE(),1,1)";

				ExecuteMainQuery(Stringsqlconnection);


				#endregion


				#region FOR Configurations Raffels EnableRequestNoteCancelOnly
				query = @"IF Not EXISTS(SELECT * FROM Configurations WHERE ConfigurationKey = 'EnableRequestNoteCancelOnly')
                    Insert into Configurations(ConfigurationKey, ConfigurationDescription, EffectLocationId, ConfigurationOn, ConfigurationActive, ConfigurationDelete, CreateDate, 
                    CreateUserId, CompanyId)
                    VALUES ('EnableRequestNoteCancelOnly','',1,0,1,0,GETDATE(),1,1)";

				ExecuteMainQuery(Stringsqlconnection);

				#endregion


				#region FOR Configurations Raffels EnableGRNQtyIncreasedValidation
				query = @"IF Not EXISTS(SELECT * FROM Configurations WHERE ConfigurationKey = 'EnableGRNQtyIncreasedValidation')
                    Insert into Configurations(ConfigurationKey, ConfigurationDescription, EffectLocationId, ConfigurationOn, ConfigurationActive, ConfigurationDelete, CreateDate, 
                    CreateUserId, CompanyId)
                    VALUES ('EnableGRNQtyIncreasedValidation','',1,0,1,0,GETDATE(),1,1)";

				ExecuteMainQuery(Stringsqlconnection);
				#endregion

				#region FOR Configurations Raffels DisableEmpoyeeMandoryField
				query = @"IF Not EXISTS(SELECT * FROM Configurations WHERE ConfigurationKey = 'DisableEmpoyeeMandoryField')
                    Insert into Configurations(ConfigurationKey, ConfigurationDescription, EffectLocationId, ConfigurationOn, ConfigurationActive, ConfigurationDelete, CreateDate, 
                    CreateUserId, CompanyId)
                    VALUES ('DisableEmpoyeeMandoryField','',1,0,1,0,GETDATE(),1,1)";

				ExecuteMainQuery(Stringsqlconnection);


				#endregion


				#region FOR Configurations Raffels EnableMonthEndProcess
				query = @"IF Not EXISTS(SELECT * FROM Configurations WHERE ConfigurationKey = 'EnableMonthEndProcess')
                    Insert into Configurations(ConfigurationKey, ConfigurationDescription, EffectLocationId, ConfigurationOn, ConfigurationActive, ConfigurationDelete, CreateDate, 
                    CreateUserId, CompanyId)
                    VALUES ('EnableMonthEndProcess','',1,0,1,0,GETDATE(),1,1)";

				ExecuteMainQuery(Stringsqlconnection);


				#endregion


				#region FOR Configurations Raffels EnableGRNCancelOnly
				query = @"IF Not EXISTS(SELECT * FROM Configurations WHERE ConfigurationKey = 'EnableGRNCancelOnly')
                    Insert into Configurations(ConfigurationKey, ConfigurationDescription, EffectLocationId, ConfigurationOn, ConfigurationActive, ConfigurationDelete, CreateDate, 
                    CreateUserId, CompanyId)
                    VALUES ('EnableGRNCancelOnly','',1,0,1,0,GETDATE(),1,1)";

				ExecuteMainQuery(Stringsqlconnection);

				#endregion

				#region FOR Configurations Raffels UpdateServingUnitCostPricesForAllLocations
				query = @"IF Not EXISTS(SELECT * FROM Configurations WHERE ConfigurationKey = 'UpdateServingUnitCostPricesForAllLocations')
                    Insert into Configurations(ConfigurationKey, ConfigurationDescription, EffectLocationId, ConfigurationOn, ConfigurationActive, ConfigurationDelete, CreateDate, 
                    CreateUserId, CompanyId)
                    VALUES ('UpdateServingUnitCostPricesForAllLocations','',1,0,1,0,GETDATE(),1,1)";

				ExecuteMainQuery(Stringsqlconnection);

				#endregion







				#region FOR Configurations
				query = @"-- Check the column sizes of the Configurations table
SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Configurations';

-- Check the lengths of the data being inserted
SELECT LEN('DisableFromDepartmentAndToDepartment') AS LengthOfKey,
       LEN('') AS LengthOfDescription;

-- Original script with debugging info
IF NOT EXISTS (
    SELECT * 
    FROM Configurations 
    WHERE ConfigurationKey = 'DisableFromDepartmentAndToDepartment'
)
BEGIN
    -- Insert statement
    INSERT INTO Configurations (
        ConfigurationKey, 
        ConfigurationDescription, 
        EffectLocationId, 
        ConfigurationOn, 
        ConfigurationActive, 
        ConfigurationDelete, 
        CreateDate, 
        CreateUserId, 
        CompanyId
    )
    VALUES (
        'DisableFromDepartmentAndToDepartment', -- Ensure this fits in ConfigurationKey
        '',                                      -- Ensure this fits in ConfigurationDescription
        1, 
        0, 
        1, 
        0, 
        GETDATE(), 
        1, 
        1
    );
END
";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion

                #region FOR DocumentNumbers
                query = @"IF Not EXISTS(SELECT * FROM DocumentNumbers WHERE DocumentId = 12)
                        INSERT INTO DocumentNumbers(DocumentId, DocumentName, DocumentNo, TempDocumentNo, TemplateDocumentNo, DocumentYear, PrefixCode, GroupOfCompanyID, CompanyID, LocationId, 
                        CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer)
                        VALUES (12,'CusProd',0,0,0,50,0,1,1,1,'',GETDATE(),'',GETDATE(),0)  ";

                    ExecuteMainQuery(Stringsqlconnection);

                query = @"IF Not EXISTS(SELECT * FROM DocumentNumbers WHERE DocumentId = 16)
                        INSERT INTO DocumentNumbers(DocumentId, DocumentName, DocumentNo, TempDocumentNo, TemplateDocumentNo, DocumentYear, PrefixCode, GroupOfCompanyID, CompanyID, LocationId, 
                        CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer)
                        VALUES (16,'Stock Count',0,0,0,50,0,1,1,1,'',GETDATE(),'',GETDATE(),0)  ";

                ExecuteMainQuery(Stringsqlconnection);

				query = @"IF Not EXISTS(SELECT * FROM DocumentNumbers WHERE DocumentId = 15)
                        INSERT INTO [dbo].[DocumentNumbers]([DocumentID],[DocumentName],[CompanyID],[LocationID],[DocumentNo],[TempDocumentNo],[TemplateDocumentNo],
									[DocumentYear],[PrefixCode],[GroupOfCompanyID],[CreatedUser],[CreatedDate],[ModifiedUser],[ModifiedDate],[DataTransfer])
						VALUES ('15','StockInitialization',1,1,0,0,0,2025,'SI',1,'Admin',GETDATE(),'Admin',GETDATE(),0)  ";

				ExecuteMainQuery(Stringsqlconnection);
				#endregion

				#region FOR SysUserFunctions
				query = @"IF Not EXISTS(SELECT * FROM SysUserFunctions WHERE FormId = 16)
                            INSERT INTO SysUserFunctions(FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, 
                            ModifiedDate, DataTransfer, FormId)
                            VALUES ('StockCount','StockCount',1,1,0,1,1,1,1,1,GETDATE(),1,GETDATE(),1,16) ";

                    ExecuteMainQuery(Stringsqlconnection);

				query = @"IF Not EXISTS(SELECT * FROM SysUserFunctions WHERE FunctionName = 'SupTypeCreate')
                            INSERT INTO SysUserFunctions(FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, 
                            ModifiedDate, DataTransfer, FormId)
                            VALUES ('SupTypeCreate','Suppiler Type Create',1,1,0,1,1,1,1,1,GETDATE(),1,GETDATE(),1,0) ";

				ExecuteMainQuery(Stringsqlconnection);

				query = @"IF Not EXISTS(SELECT * FROM SysUserFunctions WHERE FunctionName = 'SupTypeEdit')
                            INSERT INTO SysUserFunctions(FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, 
                            ModifiedDate, DataTransfer, FormId)
                            VALUES ('SupTypeEdit','Supplier Type Edit',1,1,0,1,1,1,1,1,GETDATE(),1,GETDATE(),1,0) ";

				ExecuteMainQuery(Stringsqlconnection);

				query = @"IF Not EXISTS(SELECT * FROM SysUserFunctions WHERE FunctionName = 'SupTypeView')
                            INSERT INTO SysUserFunctions(FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, 
                            ModifiedDate, DataTransfer, FormId)
                            VALUES ('SupTypeView','View Supplier Type',1,1,0,1,1,1,1,1,GETDATE(),1,GETDATE(),1,0) ";

				ExecuteMainQuery(Stringsqlconnection);


				#endregion

				#region FOR SysUserGroupPermissions
				query = @"IF Not EXISTS(SELECT * FROM SysUserGroupPermissions WHERE FormId = 16)
                            INSERT INTO SysUserGroupPermissions(FunctionName, FunctionDescription, [Order], Value, MaxValue, Type, TypeID, IsActive, IsAccess, Remarks, IsDelete, GroupOfCompanyID, CompanyID, 
                            LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, SysUserGroupId, FormId)
                            VALUES ('StockCount','StockCount',1,1,1,'',1,1,1,'',0,1,1,1,'ADMIN',GETDATE(),'ADMIN',GETDATE(),0,1,16)   ";

                    ExecuteMainQuery(Stringsqlconnection);

                #endregion

                #region FOR AutoGenerateInfoes
                query = @"IF Not EXISTS(SELECT * FROM AutoGenerateInfoes WHERE FormId = 16)
                      INSERT INTO AutoGenerateInfoes(ModuleType, DocumentID, FormId, FormName, FormText, Prefix, Prefix2, CodeLength, Suffix, AutoGenerete, AutoClear, IsDepend, IsDependCode, IsSupplierProduct, 
                      IsOverWriteQty, IsLocationCode, ReportPrefix, ReportType, PoIsMandatory, IsDispatchRecall, IsBackDated, IsCard, CardId, IsEntry, IsSlabReport, IsConsignment, IsRoundOff, IsAutoComplete, 
                      IsUpdateProductImage, IsAllowedInHO, IsAllowedInOutlet, IsActive, Layout, LayoutNew, ReferenceDocumentID, Separator, CompanyId)
                      VALUES (2,16,16,'Stock Count','Stock Count','J','',5,3,1,1,0,0,0,0,1,'',1,0,0,0,0,1,0,0,0,0,0,0,0,0,1,'','',0,'/',1)";

                ExecuteMainQuery(Stringsqlconnection);

                #endregion


                #endregion One time insert statement
                //Run SPs
                StrutureChangesSP runsp = new StrutureChangesSP();
                runsp.RunSp(Stringsqlconnection);

                StructureChangesAlter runAlter = new StructureChangesAlter();
                runAlter.RunAlter(Stringsqlconnection);

                StructureChangesView runview = new StructureChangesView();
                runview.RunView(Stringsqlconnection);

                StructureChangesTriggers runtrigger = new StructureChangesTriggers();
                runtrigger.RunTrigger(Stringsqlconnection);

                StructureChangesFunctions runfunctions = new StructureChangesFunctions();
                runfunctions.RunFunction(Stringsqlconnection);

                StructureChangesTableTypes runUDType = new StructureChangesTableTypes();
                runUDType.RunUDtype(Stringsqlconnection);

                

                StructureChangesInsert runInsert = new StructureChangesInsert();
                runInsert.RunInsert(Stringsqlconnection);
            }
            catch (Exception ex)
            {

            }
        }

        public void ExecuteMainQuery(string con)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(con))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {

            }

        }

    }
}
