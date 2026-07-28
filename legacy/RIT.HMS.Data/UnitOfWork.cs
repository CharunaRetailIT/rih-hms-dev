using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RIT.HMS.Domain;
using System.Data.Entity;
using RIT.HMS.Domain.Common;
using RIT.HMS.Domain.Transactions;
using RIT.HMS.Domain.Reports;
using RIT.HMS.Domain.ViewModels.Reports;
using RIT.HMS.Domain.Promotions;
using RIT.HMS.Domain.Configurations;
using RIT.HMS.Domain.ViewModels;
using RIT.HMS.Domain.Logs;
using RIT.HMS.Domain.Loyalty;
using RIT.HMS.Domain.ConnectionManager;
using System.Web;
using RIT.HMS.Domain.Journal;

namespace RIT.HMS.Data
{
    public class UnitOfWork : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private bool _disposed = false;
        private DbContextTransaction _objTran;

        public UnitOfWork()
        {

          //  _context = new ApplicationDbContext(ConnectionManager.CurrentConnectionName);
        }



        public UnitOfWork(string connectionname)
        {
           
            Connection cn = new Connection();
            cn.ConnectionName = connectionname;        
            _context = new ApplicationDbContext(cn.ConnectionName);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
           
        }

        public void CreateTransaction()
        {    
            _objTran = _context.Database.BeginTransaction();
        }

        public void Commit()
        {
            _objTran.Commit();
        }

        public void Rollback()
        {
            _objTran.Rollback();
            _objTran.Dispose();
        }


        public int Save()
        {
            return _context.SaveChanges();
        }


        #region Connection Manager

        private GenericRepository<CompanyUser> _companyUserRepository;
        public GenericRepository<CompanyUser> CompanyUserRepository
        {
            get
            {
                if (this._companyUserRepository == null)
                {
                    this._companyUserRepository = new GenericRepository<CompanyUser>(_context);
                }
                return _companyUserRepository;
            }
        }

        #endregion


        #region Master repositories 

        private GenericRepository<SysCompany> _companyRepository;
        public GenericRepository<SysCompany> CompanyRepository
        {
            get
            {
                if (this._companyRepository == null)
                {
                    this._companyRepository = new GenericRepository<SysCompany>(_context);
                }
                return _companyRepository;
            }
        }

        private GenericRepository<SysLocation> _locationRepository;
        public GenericRepository<SysLocation> LocationRepository
        {
            get
            {
                if (this._locationRepository == null)
                {
                    this._locationRepository = new GenericRepository<SysLocation>(_context);
                }
                return _locationRepository;
            }
        }

        private GenericRepository<SysLocationType> _locationTypeRepository;
        public GenericRepository<SysLocationType> LocationTypeRepository
        {
            get
            {
                if (this._locationTypeRepository == null)
                {
                    this._locationTypeRepository = new GenericRepository<SysLocationType>(_context);
                }
                return _locationTypeRepository;
            }
        }


        private GenericRepository<SysLocationMapper> _locationMapperRepository;
        public GenericRepository<SysLocationMapper> LocationMapperRepository
        {
            get
            {
                if (this._locationMapperRepository == null)
                {
                    this._locationMapperRepository = new GenericRepository<SysLocationMapper>(_context);
                }
                return _locationMapperRepository;
            }
        }


        private GenericRepository<ProductKitchenMapper> _productKitchenMapperRepository;
        public GenericRepository<ProductKitchenMapper> ProductKitchenMapperRepository
        {
            get
            {
                if (this._productKitchenMapperRepository == null)
                {
                    this._productKitchenMapperRepository = new GenericRepository<ProductKitchenMapper>(_context);
                }
                return _productKitchenMapperRepository;
            }
        }

        private GenericRepository<ProductStockMaster> _productStockMasterRepository;
        public GenericRepository<ProductStockMaster> ProductStockMasterRepository
        {
            get
            {
                if (this._productStockMasterRepository == null)
                {
                    this._productStockMasterRepository = new GenericRepository<ProductStockMaster>(_context);
                }
                return _productStockMasterRepository;
            }
        }

        private GenericRepository<KitchenPrinterTypes> _kitchenPrinterTypesRepository;
        public GenericRepository<KitchenPrinterTypes> KitchenPrinterTypesRepository
        {
            get
            {
                if (this._kitchenPrinterTypesRepository == null)
                {
                    this._kitchenPrinterTypesRepository = new GenericRepository<KitchenPrinterTypes>(_context);
                }
                return _kitchenPrinterTypesRepository;
            }
        }

        private GenericRepository<Receipe> _receipeRepository;
        public GenericRepository<Receipe> ReceipeRepository
        {
            get
            {
                if (this._receipeRepository == null)
                {
                    this._receipeRepository = new GenericRepository<Receipe>(_context);
                }
                return _receipeRepository;
            }
        }

        private GenericRepository<Addons> _addonsRepository;
        public GenericRepository<Addons> AddonsRepository
        {
            get
            {
                if (this._addonsRepository == null)
                {
                    this._addonsRepository = new GenericRepository<Addons>(_context);
                }
                return _addonsRepository;
            }
        }

        private GenericRepository<ProductServingUnit> _productServingUnitRepository;
        public GenericRepository<ProductServingUnit> ProductServingUnitRepository
        {
            get
            {
                if (this._productServingUnitRepository == null)
                {
                    this._productServingUnitRepository = new GenericRepository<ProductServingUnit>(_context);
                }
                return _productServingUnitRepository;
            }
        }

        private GenericRepository<ServingUnit> _ServingUnit;
        public GenericRepository<ServingUnit> ServingUnit
        {
            get
            {
                if (this._ServingUnit == null)
                {
                    this._ServingUnit = new GenericRepository<ServingUnit>(_context);
                }
                return _ServingUnit;
            }
        }

        private GenericRepository<CurrencyHistory> _currencyHistoryRepository;
        public GenericRepository<CurrencyHistory> CurrencyHistoryRepository
        {
            get
            {
                if (this._currencyHistoryRepository == null)
                {
                    this._currencyHistoryRepository = new GenericRepository<CurrencyHistory>(_context);
                }
                return _currencyHistoryRepository;
            }
        }

        private GenericRepository<Product> _productRepository;
        public GenericRepository<Product> ProductRepository
        {
            get
            {
                if (this._productRepository == null)
                {
                    this._productRepository = new GenericRepository<Product>(_context);
                }
                return _productRepository;
            }
        }

        private GenericRepository<Currency> _currencyRepository;
        public GenericRepository<Currency> CurrencyRepository
        {
            get
            {
                if (this._currencyRepository == null)
                {
                    this._currencyRepository = new GenericRepository<Currency>(_context);
                }
                return _currencyRepository;
            }
        }

        private GenericRepository<ProductTax> _productTaxRepository;
        public GenericRepository<ProductTax> ProductTaxRepository
        {
            get
            {
                if (this._productTaxRepository == null)
                {
                    this._productTaxRepository = new GenericRepository<ProductTax>(_context);
                }
                return _productTaxRepository;
            }
        }

        private GenericRepository<Customer> _customerRepository;
        public GenericRepository<Customer> CustomerRepository
        {
            get
            {
                if (this._customerRepository == null)
                {
                    this._customerRepository = new GenericRepository<Customer>(_context);
                }
                return _customerRepository;
            }
        }

        private GenericRepository<DeliveryPerson> _deliveryPersonRepository;
        public GenericRepository<DeliveryPerson> DeliveryPersonRepository
        {
            get
            {
                if (this._deliveryPersonRepository == null)
                {
                    this._deliveryPersonRepository = new GenericRepository<DeliveryPerson>(_context);
                }
                return _deliveryPersonRepository;
            }
        }

        private GenericRepository<SupplierProduct> _supplierProductRepository;
        public GenericRepository<SupplierProduct> SupplierProductRepository
        {
            get
            {
                if (this._supplierProductRepository == null)
                {
                    this._supplierProductRepository = new GenericRepository<SupplierProduct>(_context);
                }
                return _supplierProductRepository;
            }
        }

        private GenericRepository<RstDepartment> _departmentRepository;
        public GenericRepository<RstDepartment> DepartmentRepository
        {
            get
            {
                if (this._departmentRepository == null)
                {
                    this._departmentRepository = new GenericRepository<RstDepartment>(_context);
                }
                return _departmentRepository;
            }
        }

        private GenericRepository<PrinterType> _printerTypeRepository;
        public GenericRepository<PrinterType> PrinterTypeRepository
        {
            get
            {
                if (this._printerTypeRepository == null)
                {
                    this._printerTypeRepository = new GenericRepository<PrinterType>(_context);
                }
                return _printerTypeRepository;
            }
        }
        #region TourAgent
        private GenericRepository<TourAgent> _tourAgentRepository;
        public GenericRepository<TourAgent> TourAgentRepository
        {
            get
            {
                if (this._tourAgentRepository == null)
                {
                    this._tourAgentRepository = new GenericRepository<TourAgent>(_context);
                }
                return _tourAgentRepository;
            }
        }
      
        private GenericRepository<TourAgentCompany> touragentcompanyRepository;
        public GenericRepository<TourAgentCompany> TourAgentCompanyRepository
        {
            get
            {
                if (this.touragentcompanyRepository == null)
                {
                    this.touragentcompanyRepository = new GenericRepository<TourAgentCompany>(_context);
                }
                return touragentcompanyRepository;
            }
        }
        #endregion

        private GenericRepository<EmployeeGroup> _employeeGroupRepository;
        public GenericRepository<EmployeeGroup> EmployeeGroupRepository
        {
            get
            {
                if (this._employeeGroupRepository == null)
                {
                    this._employeeGroupRepository = new GenericRepository<EmployeeGroup>(_context);
                }
                return _employeeGroupRepository;
            }
        }
        private GenericRepository<UnitOfMeasure> _unitOfMeasureRepository;
        public GenericRepository<UnitOfMeasure> UnitOfMeasureRepository
        {
            get
            {
                if (this._unitOfMeasureRepository == null)
                {
                    this._unitOfMeasureRepository = new GenericRepository<UnitOfMeasure>(_context);
                }
                return _unitOfMeasureRepository;
            }
        }

       

        private GenericRepository<Employee> _employeeRepository;
        public GenericRepository<Employee> EmployeeRepository
        {
            get
            {
                if (this._employeeRepository == null)
                {
                    this._employeeRepository = new GenericRepository<Employee>(_context);
                }
                return _employeeRepository;
            }
        }

        private GenericRepository<StewardsMaster> _stewardsMasterRepository;
        public GenericRepository<StewardsMaster> StewardsMasterRepository
        {
            get
            {
                if (this._stewardsMasterRepository == null)
                {
                    this._stewardsMasterRepository = new GenericRepository<StewardsMaster>(_context);
                }
                return _stewardsMasterRepository;
            }
        }

        private GenericRepository<SysGroupOfCompany> _groupOfCompanyRepository;
        public GenericRepository<SysGroupOfCompany> GroupOfCompanyRepository
        {
            get
            {
                if (this._groupOfCompanyRepository == null)
                {
                    this._groupOfCompanyRepository = new GenericRepository<SysGroupOfCompany>(_context);
                }
                return _groupOfCompanyRepository;
            }
        }

        private GenericRepository<InterDepartment> _interDepartmentRepository;
        public GenericRepository<InterDepartment> InterDepartmentRepository
        {
            get
            {
                if (this._interDepartmentRepository == null)
                {
                    this._interDepartmentRepository = new GenericRepository<InterDepartment>(_context);
                }
                return _interDepartmentRepository;
            }
        }


        private GenericRepository<InvPriceLevel> _invpricelevel;
        public GenericRepository<InvPriceLevel> InvPriceLevels
        {
            get
            {
                if (this._invpricelevel == null)
                {
                    this._invpricelevel = new GenericRepository<InvPriceLevel>(_context);
                }
                return _invpricelevel;
            }
        }

        private GenericRepository<InvPriceLevelList> _invpricelevellist;
        public GenericRepository<InvPriceLevelList> Invpricelevellists
        {
            get
            {
                if (this._invpricelevellist == null)
                {
                    this._invpricelevellist = new GenericRepository<InvPriceLevelList>(_context);
                }
                return _invpricelevellist;
            }
        }
        private GenericRepository<RstMealType> _mealTypeRepository;
        public GenericRepository<RstMealType> MealTypeRepository
        {
            get
            {
                if (this._mealTypeRepository == null)
                {
                    this._mealTypeRepository = new GenericRepository<RstMealType>(_context);
                }
                return _mealTypeRepository;
            }
        }
   

        private GenericRepository<RstSubCategory> _subCategoryRepository;
        public GenericRepository<RstSubCategory> SubCategoryRepository
        {
            get
            {
                if (this._subCategoryRepository == null)
                {
                    this._subCategoryRepository = new GenericRepository<RstSubCategory>(_context);
                }
                return _subCategoryRepository;
            }
        }

     

        private GenericRepository<RstCategory> _categoryRepository;
        public GenericRepository<RstCategory> CategoryRepository
        {
            get
            {
                if (this._categoryRepository == null)
                {
                    this._categoryRepository = new GenericRepository<RstCategory>(_context);
                }
                return _categoryRepository;
            }
        }

        private GenericRepository<Tax> _taxRepository;
        public GenericRepository<Tax> TaxRepository
        {
            get
            {
                if (this._taxRepository == null)
                {
                    this._taxRepository = new GenericRepository<Tax>(_context);
                }
                return _taxRepository;
            }
        }

        private GenericRepository<LocationTax> _locationTaxRepository;
        public GenericRepository<LocationTax> LocationTaxRepository
        {
            get
            {
                if (this._locationTaxRepository == null)
                {
                    this._locationTaxRepository = new GenericRepository<LocationTax>(_context);
                }
                return _locationTaxRepository;
            }
        }

        private GenericRepository<PayTypeTax> _payTypeTaxRepository;
        public GenericRepository<PayTypeTax> PayTypeTaxRepository
        {
            get
            {
                if (this._payTypeTaxRepository == null)
                {
                    this._payTypeTaxRepository = new GenericRepository<PayTypeTax>(_context);
                }
                return _payTypeTaxRepository;
            }
        }


        private GenericRepository<CateringModeTax> _cateringModeTaxRepository;
        public GenericRepository<CateringModeTax> CateringModeTaxRepository
        {
            get
            {
                if (this._cateringModeTaxRepository == null)
                {
                    this._cateringModeTaxRepository = new GenericRepository<CateringModeTax>(_context);
                }
                return _cateringModeTaxRepository;
            }
        }

        private GenericRepository<SupplierGroup> _supplerGroupRepository;
        public GenericRepository<SupplierGroup> SuplierGroupRepository
        {
            get
            {
                if (this._supplerGroupRepository == null)
                {
                    this._supplerGroupRepository = new GenericRepository<SupplierGroup>(_context);
                }
                return _supplerGroupRepository;
            }
        }


        private GenericRepository<Supplier> _supplerRepository; 
        public GenericRepository<Supplier> SuplierRepository
        {
            get
            {
                if (this._supplerRepository == null)
                {
                    this._supplerRepository = new GenericRepository<Supplier>(_context);
                }
                return _supplerRepository;
            }
        }

        private GenericRepository<SupplierType> _supplerTypeRepository;
        public GenericRepository<SupplierType> SuplierTypeRepository
        {
            get
            {
                if (this._supplerTypeRepository == null)
                {
                    this._supplerTypeRepository = new GenericRepository<SupplierType>(_context);
                }
                return _supplerTypeRepository;
            }
        }

        private GenericRepository<AddonCategoryMaster> _addonCategoryMaster;
        public GenericRepository<AddonCategoryMaster> AddonCategoryMasterRepository
        {
            get
            {
                if (this._addonCategoryMaster == null)
                {
                    this._addonCategoryMaster = new GenericRepository<AddonCategoryMaster>(_context);
                }
                return _addonCategoryMaster;
            }
        }

        private GenericRepository<PaymentMethod> _paymentMethodRepository;
        public GenericRepository<PaymentMethod> PaymentMethodRepository
        {
            get
            {
                if (this._paymentMethodRepository == null)
                {
                    this._paymentMethodRepository = new GenericRepository<PaymentMethod>(_context);
                }
                return _paymentMethodRepository;
            }
        }

        private GenericRepository<ServingUnit> _servingunitRepository;
        public GenericRepository<ServingUnit> ServingUnitRepository
        {
            get
            {
                if (this._servingunitRepository == null)
                {
                    this._servingunitRepository = new GenericRepository<ServingUnit>(_context);
                }
                return _servingunitRepository;
            }
        }

        private GenericRepository<PaymentTerm> _paymentTermRepository;
        public GenericRepository<PaymentTerm> PaymentTermRepository
        {
            get
            {
                if (this._paymentTermRepository == null)
                {
                    this._paymentTermRepository = new GenericRepository<PaymentTerm>(_context);
                }
                return _paymentTermRepository;
            }
        }


        private GenericRepository<CustomerCategory> _customerCategoryRepository;
        public GenericRepository<CustomerCategory> CustomerCategoryRepository
        {
            get
            {
                if (this._customerCategoryRepository == null)
                {
                    this._customerCategoryRepository = new GenericRepository<CustomerCategory>(_context);
                }
                return _customerCategoryRepository;
            }
        }

        private GenericRepository<KOTBOTDescription> _kOTBOTDescriptionRepository;
        public GenericRepository<KOTBOTDescription> KOTBOTDescriptionRepository
        {
            get
            {
                if (this._kOTBOTDescriptionRepository == null)
                {
                    this._kOTBOTDescriptionRepository = new GenericRepository<KOTBOTDescription>(_context);
                }
                return _kOTBOTDescriptionRepository;
            }
        }

        private GenericRepository<ProductInstruction> _productInstructionRepository;
        public GenericRepository<ProductInstruction> ProductInstructionRepository
        {
            get
            {
                if (this._productInstructionRepository == null)
                {
                    this._productInstructionRepository = new GenericRepository<ProductInstruction>(_context);
                }
                return _productInstructionRepository;
            }
        }

        // chamodi's  start ---------------------------------------------------------------------------------------------------
        private GenericRepository<CateringMood> _cateringMoodRepository;
        public GenericRepository<CateringMood> CateringMoodRepository
        {
            get
            {
                if (this._cateringMoodRepository == null)
                {
                    this._cateringMoodRepository = new GenericRepository<CateringMood>(_context);
                }
                return _cateringMoodRepository;
            }
        }

        private GenericRepository<ChairMaster> _chairRepository;
        public GenericRepository<ChairMaster> ChairRepository
        {
            get
            {
                if (this._chairRepository == null)
                {
                    this._chairRepository = new GenericRepository<ChairMaster>(_context);
                }
                return _chairRepository;
            }
        }


        private GenericRepository<RstRoomMaster> _roomMasterRepository;
        public GenericRepository<RstRoomMaster> RoomMasterRepository
        {
            get
            {
                if (this._roomMasterRepository == null)
                {
                    this._roomMasterRepository = new GenericRepository<RstRoomMaster>(_context);
                }
                return _roomMasterRepository;
            }
        }

        private GenericRepository<RstRoomTypeRate> _roomTypeRateRepository;
        public GenericRepository<RstRoomTypeRate> RoomTypeRateRepository
        {
            get
            {
                if (this._roomTypeRateRepository == null)
                {
                    this._roomTypeRateRepository = new GenericRepository<RstRoomTypeRate>(_context);
                }
                return _roomTypeRateRepository;
            }
        }

        private GenericRepository<RstRoomType> _roomTypeRepository;
        public GenericRepository<RstRoomType> RoomTypeRepository
        {
            get
            {
                if (this._roomTypeRepository == null)
                {
                    this._roomTypeRepository = new GenericRepository<RstRoomType>(_context);
                }
                return _roomTypeRepository;
            }
        }

        private GenericRepository<TableMaster> _tableMasterRepository;
        public GenericRepository<TableMaster> TableMasterRepository
        {
            get
            {
                if (this._tableMasterRepository == null)
                {
                    this._tableMasterRepository = new GenericRepository<TableMaster>(_context);
                }
                return _tableMasterRepository;
            }
        }

        private GenericRepository<UnitConversion> _unitConversionRepository;
        public GenericRepository<UnitConversion> UnitConversionRepository
        {
            get
            {
                if (this._unitConversionRepository == null)
                {
                    this._unitConversionRepository = new GenericRepository<UnitConversion>(_context);
                }
                return _unitConversionRepository;
            }
        }
        private GenericRepository<Vehicle> _vehicleRepository;
        public GenericRepository<Vehicle> VehicleRepository
        {
            get
            {
                if (this._vehicleRepository == null)
                {
                    this._vehicleRepository = new GenericRepository<Vehicle>(_context);
                }
                return _vehicleRepository;
            }
        }

        // chamodi's end

        private GenericRepository<CustomerDiscount> _customerDiscountRepository;
        public GenericRepository<CustomerDiscount> CustomerDiscountRepository
        {
            get
            {
                if (this._customerDiscountRepository == null)
                {
                    this._customerDiscountRepository = new GenericRepository<CustomerDiscount>(_context);
                }
                return _customerDiscountRepository;
            }
        }

        private GenericRepository<Event> _eventRepository;
        public GenericRepository<Event> EventRepository
        {
            get
            {
                if (this._eventRepository == null)
                {
                    this._eventRepository = new GenericRepository<Event>(_context);
                }
                return _eventRepository;
            }
        }

        private GenericRepository<EventProduct> _eventProductRepository;
        public GenericRepository<EventProduct> EventProductRepository
        {
            get
            {
                if (this._eventProductRepository == null)
                {
                    this._eventProductRepository = new GenericRepository<EventProduct>(_context);
                }
                return _eventProductRepository;
            }
        }

        private GenericRepository<SysYears> _sysYearsRepository;
        public GenericRepository<SysYears> SysYearsRepository
        {
            get
            {
                if (this._sysYearsRepository == null)
                {
                    this._sysYearsRepository = new GenericRepository<SysYears>(_context);
                }
                return _sysYearsRepository;
            }
        }
        private GenericRepository<KitchenMaster> _kitchenMasterRepository;
        public GenericRepository<KitchenMaster> KitchenMasterRepository
        {
            get
            {
                if (this._kitchenMasterRepository == null)
                {
                    this._kitchenMasterRepository = new GenericRepository<KitchenMaster>(_context);
                }
                return _kitchenMasterRepository;
            }
        }




        #endregion Masaters

        #region Transactions Repositories

        private GenericRepository<DocStatus> _docStatusRepository;
        public GenericRepository<DocStatus> DocStatus
        {
            get
            {
                if (this._docStatusRepository == null)
                {
                    this._docStatusRepository = new GenericRepository<DocStatus>(_context);
                }
                return _docStatusRepository;
            }
        }

        private GenericRepository<PurchaseOrderHeader> _purchaseOrderHeaderRepository;
        public GenericRepository<PurchaseOrderHeader> PurchaseOrderHeaderRepository
        {
            get
            {
                if (this._purchaseOrderHeaderRepository == null)
                {
                    this._purchaseOrderHeaderRepository = new GenericRepository<PurchaseOrderHeader>(_context);
                }
                return _purchaseOrderHeaderRepository;
            }
        }

        private GenericRepository<PurchaseOrderDetail> _purchaseOrderDegtailRepository;
        public GenericRepository<PurchaseOrderDetail> PurchaseOrderDetailRepository
        {
            get
            {
                if (this._purchaseOrderDegtailRepository == null)
                {
                    this._purchaseOrderDegtailRepository = new GenericRepository<PurchaseOrderDetail>(_context);
                }
                return _purchaseOrderDegtailRepository;
            }
        }

        private GenericRepository<InvRequestNotePOTransaction> _invRequestNotePOTransaction;
        public GenericRepository<InvRequestNotePOTransaction> InvRequestNotePOTransaction
        {
            get
            {
                if (this._invRequestNotePOTransaction == null)
                {
                    this._invRequestNotePOTransaction = new GenericRepository<InvRequestNotePOTransaction>(_context);
                }
                return _invRequestNotePOTransaction;
            }
        }


        private GenericRepository<PurchaseHeader> _purchaseHeaderRepository;
        public GenericRepository<PurchaseHeader> PurchaseHeaderRepository
        {
            get
            {
                if (this._purchaseHeaderRepository == null)
                {
                    this._purchaseHeaderRepository = new GenericRepository<PurchaseHeader>(_context);
                }
                return _purchaseHeaderRepository;
            }
        }


        private GenericRepository<PurchaseDetail> _purchaseDetailRepository;
        public GenericRepository<PurchaseDetail> PurchaseDetailRepository
        {
            get
            {
                if (this._purchaseDetailRepository == null)
                {
                    this._purchaseDetailRepository = new GenericRepository<PurchaseDetail>(_context);
                }
                return _purchaseDetailRepository;
            }
        }

        private GenericRepository<PriceLevel> _priceLevelRepository;
        public GenericRepository<PriceLevel> PriceLevelRepository
        {
            get
            {
                if (this._priceLevelRepository == null)
                {
                    this._priceLevelRepository = new GenericRepository<PriceLevel>(_context);
                }
                return _priceLevelRepository;
            }
        }

        private GenericRepository<RequestNoteAccptanceHeader> _requestNoteAccptanceHeaderRepository;
        public GenericRepository<RequestNoteAccptanceHeader> RequestNoteAccptanceHeaderRepository
        {
            get
            {
                if (this._requestNoteAccptanceHeaderRepository == null)
                {
                    this._requestNoteAccptanceHeaderRepository = new GenericRepository<RequestNoteAccptanceHeader>(_context);
                }
                return _requestNoteAccptanceHeaderRepository;
            }
        }

        private GenericRepository<RequestNoteAcceptanceDetail> _requestNoteAcceptanceDetailRepository;
        public GenericRepository<RequestNoteAcceptanceDetail> RequestNoteAccptanceDetailRepository
        {
            get
            {
                if (this._requestNoteAcceptanceDetailRepository == null)
                {
                    this._requestNoteAcceptanceDetailRepository = new GenericRepository<RequestNoteAcceptanceDetail>(_context);
                }
                return _requestNoteAcceptanceDetailRepository;
            }
        }

        private GenericRepository<ProductionNoteHeader> _productionNoteHeaderRepository;
        public GenericRepository<ProductionNoteHeader> ProductionNoteHeaderRepository
        {
            get
            {
                if (this._productionNoteHeaderRepository == null)
                {
                    this._productionNoteHeaderRepository = new GenericRepository<ProductionNoteHeader>(_context);
                }
                return _productionNoteHeaderRepository;
            }
        }

        private GenericRepository<ProductionNoteDetail> _productionNoteDetailRepository;
        public GenericRepository<ProductionNoteDetail> ProductionNoteDetailRepository
        {
            get
            {
                if (this._productionNoteDetailRepository == null)
                {
                    this._productionNoteDetailRepository = new GenericRepository<ProductionNoteDetail>(_context);
                }
                return _productionNoteDetailRepository;
            }
        }

        private GenericRepository<CustomProductionNoteHeader> _customProductionNoteHeaderRepository;
        public GenericRepository<CustomProductionNoteHeader> CustomProductionNoteHeaderRepository
        {
            get
            {
                if (this._customProductionNoteHeaderRepository == null)
                {
                    this._customProductionNoteHeaderRepository = new GenericRepository<CustomProductionNoteHeader>(_context);
                }
                return _customProductionNoteHeaderRepository;
            }
        }

        private GenericRepository<CustomProductionNoteDetail> _customProductionNoteDetailRepository;
        public GenericRepository<CustomProductionNoteDetail> customProductionNoteDetailRepository
        {
            get
            {
                if (this._customProductionNoteDetailRepository == null)
                {
                    this._customProductionNoteDetailRepository = new GenericRepository<CustomProductionNoteDetail>(_context);
                }
                return _customProductionNoteDetailRepository;
            }
        }
        private GenericRepository<InvAdvanceNoteHed> _invAdvanceNoteHedRepository;
        public GenericRepository<InvAdvanceNoteHed> InvAdvanceNoteHedRepository
        {
            get
            {
                if (this._invAdvanceNoteHedRepository == null)
                {
                    this._invAdvanceNoteHedRepository = new GenericRepository<InvAdvanceNoteHed>(_context);
                }
                return _invAdvanceNoteHedRepository;
            }
        }

        private GenericRepository<InvAdvanceNoteDet> _invAdvanceNoteDetRepository;
        public GenericRepository<InvAdvanceNoteDet> InvAdvanceNoteDetRepository
        {
            get
            {
                if (this._invAdvanceNoteDetRepository == null)
                {
                    this._invAdvanceNoteDetRepository = new GenericRepository<InvAdvanceNoteDet>(_context);
                }
                return _invAdvanceNoteDetRepository;
            }
        }

        // Chamodi's start ----------------------------------------------------------------------------------------

        private GenericRepository<StockAdjustmentType> _stockAdjustmentTypeRepository;
        public GenericRepository<StockAdjustmentType> StockAdjustmentTypeRepository
        {
            get
            {
                if (this._stockAdjustmentTypeRepository == null)
                {
                    this._stockAdjustmentTypeRepository = new GenericRepository<StockAdjustmentType>(_context);
                }
                return _stockAdjustmentTypeRepository;
            }
        }

        private GenericRepository<StockAdjustmentHeader> _stockAdjustmentHeaderRepository;
        public GenericRepository<StockAdjustmentHeader> StockAdjustmentHeaderRepository
        {
            get
            {
                if (this._stockAdjustmentHeaderRepository == null)
                {
                    this._stockAdjustmentHeaderRepository = new GenericRepository<StockAdjustmentHeader>(_context);
                }
                return _stockAdjustmentHeaderRepository;
            }
        }

        private GenericRepository<StockAdjustmentDetail> _stockAdjustmentDetailRepository;
        public GenericRepository<StockAdjustmentDetail> StockAdjustmentDetailRepository
        {
            get
            {
                if (this._stockAdjustmentDetailRepository == null)
                {
                    this._stockAdjustmentDetailRepository = new GenericRepository<StockAdjustmentDetail>(_context);
                }
                return _stockAdjustmentDetailRepository;
            }
        }

        private GenericRepository<RequestNoteHeader> _requestNoteHeaderRepository;
        public GenericRepository<RequestNoteHeader> RequestNoteHeaderRepository
        {
            get
            {
                if (this._requestNoteHeaderRepository == null)
                {
                    this._requestNoteHeaderRepository = new GenericRepository<RequestNoteHeader>(_context);
                }
                return _requestNoteHeaderRepository;
            }
        }

        private GenericRepository<RequestNoteDetail> _requestNoteDetailRepository;
        public GenericRepository<RequestNoteDetail> RequestNoteDetailRepository
        {
            get
            {
                if (this._requestNoteDetailRepository == null)
                {
                    this._requestNoteDetailRepository = new GenericRepository<RequestNoteDetail>(_context);
                }
                return _requestNoteDetailRepository;
            }
        }

        private GenericRepository<TransferNoteHeader> _transferNoteHeaderRepository;
        public GenericRepository<TransferNoteHeader> TransferNoteHeaderRepository
        {
            get
            {
                if (this._transferNoteHeaderRepository == null)
                {
                    this._transferNoteHeaderRepository = new GenericRepository<TransferNoteHeader>(_context);
                }
                return _transferNoteHeaderRepository;
            }
        }

        private GenericRepository<TransferNoteDetail> _transferNoteDetailRepository;
        public GenericRepository<TransferNoteDetail> TransferNoteDetailRepository
        {
            get
            {
                if (this._transferNoteDetailRepository == null)
                {
                    this._transferNoteDetailRepository = new GenericRepository<TransferNoteDetail>(_context);
                }
                return _transferNoteDetailRepository;
            }
        }

        private GenericRepository<MonthEnd> _monthEndRepository;
        public GenericRepository<MonthEnd> MonthEndRepository
        {
            get
            {
                if (this._monthEndRepository == null)
                {
                    this._monthEndRepository = new GenericRepository<MonthEnd>(_context);
                }
                return _monthEndRepository;
            }
        }

        private GenericRepository<JobHeader> _jobHeaderRepository;
        public GenericRepository<JobHeader> JobHeaderRepository
        {
            get
            {
                if (this._jobHeaderRepository == null)
                {
                    this._jobHeaderRepository = new GenericRepository<JobHeader>(_context);
                }
                return _jobHeaderRepository;
            }
        }

        private GenericRepository<JobItem> _jobItemRepository;
        public GenericRepository<JobItem> JobItemRepository
        {
            get
            {
                if (this._jobItemRepository == null)
                {
                    this._jobItemRepository = new GenericRepository<JobItem>(_context);
                }
                return _jobItemRepository;
            }
        }

        // end ----------------------------------------------------------------------------------------

        #endregion  End Transaction repositories 

        #region Loyalty

        private GenericRepository<ReferenceType> _referenceTypeRepository;
        public GenericRepository<ReferenceType> ReferenceTypeRepository
        {
            get
            {
                if (this._referenceTypeRepository == null)
                {
                    this._referenceTypeRepository = new GenericRepository<ReferenceType>(_context);
                }
                return _referenceTypeRepository;
            }
        }
        private GenericRepository<CardMaster> _cardMasterRepository;
        public GenericRepository<CardMaster> CardMasterRepository
        {
            get
            {
                if (this._cardMasterRepository == null)
                {
                    this._cardMasterRepository = new GenericRepository<CardMaster>(_context);
                }
                return _cardMasterRepository;
            }
        }

        private GenericRepository<LoyaltyCardSchems> _loyaltyCardSchemsRepository;
        public GenericRepository<LoyaltyCardSchems> LoyaltyCardSchemsRepository
        {
            get
            {
                if (this._loyaltyCardSchemsRepository == null)
                {
                    this._loyaltyCardSchemsRepository = new GenericRepository<LoyaltyCardSchems>(_context);
                }
                return _loyaltyCardSchemsRepository;
            }
        }

        private GenericRepository<CardGenerationLocationSetting> _cardGenerationLocationSettingReporsitory;
        public GenericRepository<CardGenerationLocationSetting> cardGenerationLocationSettingReporsitory
        {
            get
            {
                if (this._cardGenerationLocationSettingReporsitory == null)
                {
                    this._cardGenerationLocationSettingReporsitory = new GenericRepository<CardGenerationLocationSetting>(_context);
                }
                return _cardGenerationLocationSettingReporsitory;
            }
        }

        private GenericRepository<LoyaltyCardGenerationHeader> _loyaltyCardGenerationHeaderReporsitory;
        public GenericRepository<LoyaltyCardGenerationHeader> loyaltyCardGenerationHeaderReporsitory
        {
            get
            {
                if (this._loyaltyCardGenerationHeaderReporsitory == null)
                {
                    this._loyaltyCardGenerationHeaderReporsitory = new GenericRepository<LoyaltyCardGenerationHeader>(_context);
                }
                return _loyaltyCardGenerationHeaderReporsitory;
            }
        }

        private GenericRepository<LoyaltyCardGenerationDetail> _loyaltyCardGenerationDetailReporsitory;
        public GenericRepository<LoyaltyCardGenerationDetail> loyaltyCardGenerationDetailReporsitory
        {
            get
            {
                if (this._loyaltyCardGenerationDetailReporsitory == null)
                {
                    this._loyaltyCardGenerationDetailReporsitory = new GenericRepository<LoyaltyCardGenerationDetail>(_context);
                }
                return _loyaltyCardGenerationDetailReporsitory;
            }
        }

        private GenericRepository<LoyaltyCardIssueHeader> _loyaltyCardIssueHeaderReporsitory;
        public GenericRepository<LoyaltyCardIssueHeader> LoyaltyCardIssueHeaderReporsitory
        {
            get
            {
                if (this._loyaltyCardIssueHeaderReporsitory == null)
                {
                    this._loyaltyCardIssueHeaderReporsitory = new GenericRepository<LoyaltyCardIssueHeader>(_context);
                }
                return _loyaltyCardIssueHeaderReporsitory;
            }
        }

        private GenericRepository<LoyaltyCardIssueDetail> _loyaltyCardIssueDetailReporsitory;
        public GenericRepository<LoyaltyCardIssueDetail> LoyaltyCardIssueDetailReporsitory
        {
            get
            {
                if (this._loyaltyCardIssueDetailReporsitory == null)
                {
                    this._loyaltyCardIssueDetailReporsitory = new GenericRepository<LoyaltyCardIssueDetail>(_context);
                }
                return _loyaltyCardIssueDetailReporsitory;
            }
        }

        private GenericRepository<LoyaltyCustomer> _loyaltyCustomerReporsitory;
        public GenericRepository<LoyaltyCustomer> LoyaltyCustomerReporsitory
        {
            get
            {
                if (this._loyaltyCustomerReporsitory == null)
                {
                    this._loyaltyCustomerReporsitory = new GenericRepository<LoyaltyCustomer>(_context);
                }
                return _loyaltyCustomerReporsitory;
            }
        }

        private GenericRepository<PointsExpiration> _pointsExpirationRepository;
        public GenericRepository<PointsExpiration> PointsExpirationReporsitory
        {
            get
            {
                if (this._pointsExpirationRepository == null)
                {
                    this._pointsExpirationRepository = new GenericRepository<PointsExpiration>(_context);
                }
                return _pointsExpirationRepository;
            }
        }

        private GenericRepository<PointsExpirationSchedule> _pointsExpirationScheduleRepository;
        public GenericRepository<PointsExpirationSchedule> PointsExpirationScheduleReporsitory
        {
            get
            {
                if (this._pointsExpirationScheduleRepository == null)
                {
                    this._pointsExpirationScheduleRepository = new GenericRepository<PointsExpirationSchedule>(_context);
                }
                return _pointsExpirationScheduleRepository;
            }
        }
        private GenericRepository<PointsExpirationType> _pointsExpirationTypeRepository;
        public GenericRepository<PointsExpirationType> PointsExpirationTypeReporsitory
        {
            get
            {
                if (this._pointsExpirationTypeRepository == null)
                {
                    this._pointsExpirationTypeRepository = new GenericRepository<PointsExpirationType>(_context);
                }
                return _pointsExpirationTypeRepository;
            }
        }
        private GenericRepository<InvLoyaltyTransaction> _invLoyaltyTransactionRepository;
        public GenericRepository<InvLoyaltyTransaction> InvLoyaltyTransactionReporsitory
        {
            get
            {
                if (this._invLoyaltyTransactionRepository == null)
                {
                    this._invLoyaltyTransactionRepository = new GenericRepository<InvLoyaltyTransaction>(_context);
                }
                return _invLoyaltyTransactionRepository;
            }
        }

        
        #endregion

        #region Promotions

        private GenericRepository<InvPromotionMaster> _promotionRepository;
        public GenericRepository<InvPromotionMaster> PromotionRepository
        {
            get
            {
                if (this._promotionRepository == null)
                {
                    this._promotionRepository = new GenericRepository<InvPromotionMaster>(_context);
                }
                return _promotionRepository;
            }
        }

        private GenericRepository<InvPromotionType> _promotionTypeRepository;
        public GenericRepository<InvPromotionType> PromotionTypeRepository
        {
            get
            {
                if (this._promotionTypeRepository == null)
                {
                    this._promotionTypeRepository = new GenericRepository<InvPromotionType>(_context);
                }
                return _promotionTypeRepository;
            }
        }

        private GenericRepository<InvPromoBusinessType> _promoBusinessTypeRepository;
        public GenericRepository<InvPromoBusinessType> PromoBusinessTypeRepository
        {
            get
            {
                if (this._promoBusinessTypeRepository == null)
                {
                    this._promoBusinessTypeRepository = new GenericRepository<InvPromoBusinessType>(_context);
                }
                return _promoBusinessTypeRepository;
            }
        }

        private GenericRepository<InvPromoLowestPriceWaveOff> _promoLowestPriceWaveOffRepository;
        public GenericRepository<InvPromoLowestPriceWaveOff> PromoLowestPriceWaveOffRepository
        {
            get
            {
                if (this._promoLowestPriceWaveOffRepository == null)
                {
                    this._promoLowestPriceWaveOffRepository = new GenericRepository<InvPromoLowestPriceWaveOff>(_context);
                }
                return _promoLowestPriceWaveOffRepository;
            }
        }

       

        private GenericRepository<InvPromoCustomerCategory> _promoCustomerCategoryRepository;
        public GenericRepository<InvPromoCustomerCategory> PromoCustomerCategoryRepository
        {
            get
            {
                if (this._promoCustomerCategoryRepository == null)
                {
                    this._promoCustomerCategoryRepository = new GenericRepository<InvPromoCustomerCategory>(_context);
                }
                return _promoCustomerCategoryRepository;
            }
        }


        private GenericRepository<InvPromotionDetailsBuyXProduct> _promotionDetailsBuyXProductRepository;
        public GenericRepository<InvPromotionDetailsBuyXProduct> PromotionDetailsBuyXProductRepository
        {
            get
            {
                if (this._promotionDetailsBuyXProductRepository == null)
                {
                    this._promotionDetailsBuyXProductRepository = new GenericRepository<InvPromotionDetailsBuyXProduct>(_context);
                }
                return _promotionDetailsBuyXProductRepository;
            }
        }

        private GenericRepository<InvPromoBillValueBasedGetYProduct> _promoBillValueBasedGetYProductRepository;
        public GenericRepository<InvPromoBillValueBasedGetYProduct> PromoBillValueBasedGetYProductRepository
        {
            get
            {
                if (this._promoBillValueBasedGetYProductRepository == null)
                {
                    this._promoBillValueBasedGetYProductRepository = new GenericRepository<InvPromoBillValueBasedGetYProduct>(_context);
                }
                return _promoBillValueBasedGetYProductRepository;
            }
        }

        private GenericRepository<InvPromotionDetailsProductDis> _invPromotionDetailsProductDisRepository;
        public GenericRepository<InvPromotionDetailsProductDis> InvPromotionDetailsProductDisRepository
        {
            get
            {
                if (this._invPromotionDetailsProductDisRepository == null)
                {
                    this._invPromotionDetailsProductDisRepository = new GenericRepository<InvPromotionDetailsProductDis>(_context);
                }
                return _invPromotionDetailsProductDisRepository;
            }
        }

        private GenericRepository<InvPromoBillValueBasedGetYProduct> _InvPromoBillValueBasedGetYProduct;
        public GenericRepository<InvPromoBillValueBasedGetYProduct> InvPromoBillValueBasedGetYProduct
        {
            get
            {
                if (this._InvPromoBillValueBasedGetYProduct == null)
                {
                    this._InvPromoBillValueBasedGetYProduct = new GenericRepository<InvPromoBillValueBasedGetYProduct>(_context);
                }
                return _InvPromoBillValueBasedGetYProduct;
            }
        }

        private GenericRepository<Bank> _bankRepository;
        public GenericRepository<Bank> BankRepository
        {
            get
            {
                if (this._bankRepository == null)
                {
                    this._bankRepository = new GenericRepository<Bank>(_context);
                }
                return _bankRepository;
            }
        }

        private GenericRepository<BankBin> _bankBinRepository;
        public GenericRepository<BankBin> BankBinRepository
        {
            get
            {
                if (this._bankBinRepository == null)
                {
                    this._bankBinRepository = new GenericRepository<BankBin>(_context);
                }
                return _bankBinRepository;
            }
        }

        private GenericRepository<CardType> _cardTypeRepository;
        public GenericRepository<CardType> CardTypeRepository
        {
            get
            {
                if (this._cardTypeRepository == null)
                {
                    this._cardTypeRepository = new GenericRepository<CardType>(_context);
                }
                return _cardTypeRepository;
            }
        }

        private GenericRepository<InvBillValueDiscount> _invBillValueDiscountRepository;
        public GenericRepository<InvBillValueDiscount> InvBillValueDiscountRepository
        {
            get
            {
                if (this._invBillValueDiscountRepository == null)
                {
                    this._invBillValueDiscountRepository = new GenericRepository<InvBillValueDiscount>(_context);
                }
                return _invBillValueDiscountRepository;
            }
        }


        private GenericRepository<InvBundleItemPrice> _invBundleItemPriceRepository;
        public GenericRepository<InvBundleItemPrice> InvBundleItemPriceRepository
        {
            get
            {
                if (this._invBundleItemPriceRepository == null)
                {
                    this._invBundleItemPriceRepository = new GenericRepository<InvBundleItemPrice>(_context);
                }
                return _invBundleItemPriceRepository;
            }
        }

        private GenericRepository<InvComboPackBundleItemPrice> _invBundleItemPriceRepositorys;
        public GenericRepository<InvComboPackBundleItemPrice> InvBundleItemPriceRepositorys
        {
            get
            {
                if (this._invBundleItemPriceRepositorys == null)
                {
                    this._invBundleItemPriceRepositorys = new GenericRepository<InvComboPackBundleItemPrice>(_context);
                }
                return _invBundleItemPriceRepositorys;
            }
        }

        #endregion promotions

        #region Reports

        private GenericRepository<ReportInfo> _reportInfoRepository;
        public GenericRepository<ReportInfo> ReportInfoRepository
        {
            get
            {
                if (this._reportInfoRepository == null)
                {
                    this._reportInfoRepository = new GenericRepository<ReportInfo>(_context);
                }
                return _reportInfoRepository;
            }
        }

        private GenericRepository<ReportCategory> _reportCategoryRepository;
        public GenericRepository<ReportCategory> ReportCategoryRepository
        {
            get
            {
                if (this._reportCategoryRepository == null)
                {
                    this._reportCategoryRepository = new GenericRepository<ReportCategory>(_context);
                }
                return _reportCategoryRepository;
            }
        }


        private GenericRepository<DailySalesViewMdel.SalesData> _salesDataReportRepository;
        public GenericRepository<DailySalesViewMdel.SalesData> SalesDataReportRepository
        {
            get
            {
                if (this._salesDataReportRepository == null)
                {
                    this._salesDataReportRepository = new GenericRepository<DailySalesViewMdel.SalesData>(_context);
                }
                return _salesDataReportRepository;
            }
        }
        private GenericRepository<AccountDataTransfer.ImportJournalDetailsHMS> _ImportJurnalDetReportRepository;
        public GenericRepository<AccountDataTransfer.ImportJournalDetailsHMS> ImportJurnalDetReportRepository
        {
            get
            {
                if (this._ImportJurnalDetReportRepository == null)
                {
                    this._ImportJurnalDetReportRepository = new GenericRepository<AccountDataTransfer.ImportJournalDetailsHMS>(_context);
                }
                return _ImportJurnalDetReportRepository;
            }
        }
        private GenericRepository<SalesRegisterViewModel> _salesRegisterViewModelRepository;
        public GenericRepository<SalesRegisterViewModel> SalesRegisterViewModelRepository
        {
            get
            {
                if (this._salesRegisterViewModelRepository == null)
                {
                    this._salesRegisterViewModelRepository = new GenericRepository<SalesRegisterViewModel>(_context);
                }
                return _salesRegisterViewModelRepository;
            }
        }

        private GenericRepository<DailySalesViewMdel.ValidMonthEndData> _validMonthEndDataReportRepository;

        public GenericRepository<DailySalesViewMdel.ValidMonthEndData> ValidMonthEndDataReportRepository

        {

            get

            {

                if (this._validMonthEndDataReportRepository == null)

                {

                    this._validMonthEndDataReportRepository = new GenericRepository<DailySalesViewMdel.ValidMonthEndData>(_context);

                }

                return _validMonthEndDataReportRepository;

            }

        }

        #endregion Reports

        #region Common Access
        private GenericRepository<AutoGenerateInfo> _autoGeneratedInfoRepository;
        public GenericRepository<AutoGenerateInfo> AutoGeneratedInfoRepository
        {
            get
            {
                if (this._autoGeneratedInfoRepository == null)
                {
                    this._autoGeneratedInfoRepository = new GenericRepository<AutoGenerateInfo>(_context);
                }
                return _autoGeneratedInfoRepository;
            }
        }

        private GenericRepository<DocumentNumber> _documentNumberRepository;
        public GenericRepository<DocumentNumber> DocumentNumberRepository
        {
            get
            {
                if (this._documentNumberRepository == null)
                {
                    this._documentNumberRepository = new GenericRepository<DocumentNumber>(_context);
                }
                return _documentNumberRepository;
            }
        }

        private GenericRepository<SysConfiguration> _sysConfigurationRepository;
        public GenericRepository<SysConfiguration> SysConfigurationRepository
        {
            get
            {
                if (this._sysConfigurationRepository == null)
                {
                    this._sysConfigurationRepository = new GenericRepository<SysConfiguration>(_context);
                }
                return _sysConfigurationRepository;
            }
        }

        #endregion

        #region User Permission

        private GenericRepository<SysUserGroup> _userGroupRepository;
        public GenericRepository<SysUserGroup> UserGroupRepository
        {
            get
            {
                if (this._userGroupRepository == null)
                {
                    this._userGroupRepository = new GenericRepository<SysUserGroup>(_context);
                }
                return _userGroupRepository;
            }
        }

        private GenericRepository<SysUserFunction> _userFunctionRepository;
        public GenericRepository<SysUserFunction> UserFunctionRepository
        {
            get
            {
                if (this._userFunctionRepository == null)
                {
                    this._userFunctionRepository = new GenericRepository<SysUserFunction>(_context);
                }
                return _userFunctionRepository;
            }
        }

        private GenericRepository<CashierFunction> _cashierFunction;
        public GenericRepository<CashierFunction> CashierFunctionRepository
        {
            get
            {
                if (this._cashierFunction == null)
                {
                    this._cashierFunction = new GenericRepository<CashierFunction>(_context);
                }
                return _cashierFunction;
            }
        }

        private GenericRepository<POSUserGroup> _posUserGroupRepository;
        public GenericRepository<POSUserGroup> POSUserGroupRepository
        {
            get
            {
                if (this._posUserGroupRepository == null)
                {
                    this._posUserGroupRepository = new GenericRepository<POSUserGroup>(_context);
                }
                return _posUserGroupRepository;
            }
        }

        private GenericRepository<SysUserGroupPermission> _userGroupPermissionRepository;
        public GenericRepository<SysUserGroupPermission> UserGroupPermissionRepository
        {
            get
            {
                if (this._userGroupPermissionRepository == null)
                {
                    this._userGroupPermissionRepository = new GenericRepository<SysUserGroupPermission>(_context);
                }
                return _userGroupPermissionRepository;
            }
        }

        private GenericRepository<CashierGroup> _cashierGroupRepository;
        public GenericRepository<CashierGroup> CashierGroupRepository
        {
            get
            {
                if (this._cashierGroupRepository == null)
                {
                    this._cashierGroupRepository = new GenericRepository<CashierGroup>(_context);
                }
                return _cashierGroupRepository;
            }
        }

        private GenericRepository<CashierPermission> _cashierPermissionRepository;
        public GenericRepository<CashierPermission> CashierPermissionRepository
        {
            get
            {
                if (this._cashierPermissionRepository == null)
                {
                    this._cashierPermissionRepository = new GenericRepository<CashierPermission>(_context);
                }
                return _cashierPermissionRepository;
            }
        }

        private GenericRepository<SysUserMaster> _sysUserMasterRepository;
        public GenericRepository<SysUserMaster> SysUserMasterRepository
        {
            get
            {
                if (this._sysUserMasterRepository == null)
                {
                    this._sysUserMasterRepository = new GenericRepository<SysUserMaster>(_context);
                }
                return _sysUserMasterRepository;
            }
        }


        private GenericRepository<SysUserPermission> _sysUserPermissionRepository;
        public GenericRepository<SysUserPermission> SysUserPermissionRepository
        {
            get
            {
                if (this._sysUserPermissionRepository == null)
                {
                    this._sysUserPermissionRepository = new GenericRepository<SysUserPermission>(_context);
                }
                return _sysUserPermissionRepository;
            }
        }


        //private GenericRepository<Customization> _customizationRepository;
        //public GenericRepository<Customization> CustomizationRepository
        //{
        //    get
        //    {
        //        if (this._customizationRepository == null)
        //        {
        //            this._customizationRepository = new GenericRepository<Customization>(_context);
        //        }
        //        return _customizationRepository;
        //    }
        //}
        #endregion

        #region configurations
        private GenericRepository<Configuration> _configurationRepository;
        public GenericRepository<Configuration> ConfigurationRepository
        {
            get
            {
                if (this._configurationRepository == null)
                {
                    this._configurationRepository = new GenericRepository<Configuration>(_context);
                }
                return _configurationRepository;
            }
        }
        #endregion

        #region Dashboard
        private GenericRepository<DashboardViewModel.RevenueVsCost> _revenueAndCostRepository;
        public GenericRepository<DashboardViewModel.RevenueVsCost> RevenueAndCostRepository
        {
            get
            {
                if (this._revenueAndCostRepository == null)
                {
                    this._revenueAndCostRepository = new GenericRepository<DashboardViewModel.RevenueVsCost>(_context);
                }
                return _revenueAndCostRepository;
            }
        }
        #endregion

        #region Logs

        private GenericRepository<LOGAddons> _lOGAddons;
        public GenericRepository<LOGAddons> LOGAddons
        {
            get
            {
                if (this._lOGAddons == null)
                {
                    this._lOGAddons = new GenericRepository<LOGAddons>(_context);
                }
                return _lOGAddons;
            }
        }

        private GenericRepository<LOGCustomer> _lOGCustomer;
        public GenericRepository<LOGCustomer> LOGCustomer
        {
            get
            {
                if (this._lOGCustomer == null)
                {
                    this._lOGCustomer = new GenericRepository<LOGCustomer>(_context);
                }
                return _lOGCustomer;
            }
        }

        private GenericRepository<LOGInvPromotionMaster> _lOGInvPromotionMaster;
        public GenericRepository<LOGInvPromotionMaster> LOGInvPromotionMaster
        {
            get
            {
                if (this._lOGInvPromotionMaster == null)
                {
                    this._lOGInvPromotionMaster = new GenericRepository<LOGInvPromotionMaster>(_context);
                }
                return _lOGInvPromotionMaster;
            }
        }

        private GenericRepository<LOGProduct> _lOGProduct;
        public GenericRepository<LOGProduct> LOGProduct
        {
            get
            {
                if (this._lOGProduct == null)
                {
                    this._lOGProduct = new GenericRepository<LOGProduct>(_context);
                }
                return _lOGProduct;
            }
        }

        private GenericRepository<LOGProductServingUnit> _lOGProductServingUnit;
        public GenericRepository<LOGProductServingUnit> LOGProductServingUnit
        {
            get
            {
                if (this._lOGProductServingUnit == null)
                {
                    this._lOGProductServingUnit = new GenericRepository<LOGProductServingUnit>(_context);
                }
                return _lOGProductServingUnit;
            }
        }

        private GenericRepository<LOGProductStockMaster> _lOGProductStockMaster;
        public GenericRepository<LOGProductStockMaster> LOGProductStockMaster
        {
            get
            {
                if (this._lOGProductStockMaster == null)
                {
                    this._lOGProductStockMaster = new GenericRepository<LOGProductStockMaster>(_context);
                }
                return _lOGProductStockMaster;
            }
        }

        private GenericRepository<LOGProductTax> _lLOGProductTax;
        public GenericRepository<LOGProductTax> LOGProductTax
        {
            get
            {
                if (this._lLOGProductTax == null)
                {
                    this._lLOGProductTax = new GenericRepository<LOGProductTax>(_context);
                }
                return _lLOGProductTax;
            }
        }

        private GenericRepository<LOGReceipe> _lOGReceipe;
        public GenericRepository<LOGReceipe> LOGReceipe
        {
            get
            {
                if (this._lOGReceipe == null)
                {
                    this._lOGReceipe = new GenericRepository<LOGReceipe>(_context);
                }
                return _lOGReceipe;
            }
        }

        private GenericRepository<LOGSupplier> _lOGSupplier;
        public GenericRepository<LOGSupplier> LOGSupplier
        {
            get
            {
                if (this._lOGSupplier == null)
                {
                    this._lOGSupplier = new GenericRepository<LOGSupplier>(_context);
                }
                return _lOGSupplier;
            }
        }

        private GenericRepository<LOGSupplierProduct> _lOGSupplierProduct;
        public GenericRepository<LOGSupplierProduct> LOGSupplierProduct
        {
            get
            {
                if (this._lOGSupplierProduct == null)
                {
                    this._lOGSupplierProduct = new GenericRepository<LOGSupplierProduct>(_context);
                }
                return _lOGSupplierProduct;
            }
        }

        private GenericRepository<LOGUnitConversion> _lOGUnitConversion;
        public GenericRepository<LOGUnitConversion> LOGUnitConversion
        {
            get
            {
                if (this._lOGUnitConversion == null)
                {
                    this._lOGUnitConversion = new GenericRepository<LOGUnitConversion>(_context);
                }
                return _lOGUnitConversion;
            }
        }

        #endregion Logs

        #region Sales
        private GenericRepository<TransactionDet> _transactionDetRepository;
        public GenericRepository<TransactionDet> TransactionDetRepository
        {
            get
            {
                if (this._transactionDetRepository == null)
                {
                    this._transactionDetRepository = new GenericRepository<TransactionDet>(_context);
                }
                return _transactionDetRepository;
            }
        }

        private GenericRepository<PaymentDet> _paymentDetDetRepository;
        public GenericRepository<PaymentDet> PaymentDetRepository
        {
            get
            {
                if (this._paymentDetDetRepository == null)
                {
                    this._paymentDetDetRepository = new GenericRepository<PaymentDet>(_context);
                }
                return _paymentDetDetRepository;
            }
        }

        private GenericRepository<PayType> _payTypeRepository;
        public GenericRepository<PayType> PayTypeRepository
        {
            get
            {
                if (this._payTypeRepository == null)
                {
                    this._payTypeRepository = new GenericRepository<PayType>(_context);
                }
                return _payTypeRepository;
            }
        }
        #endregion
        #region journal
        private GenericRepository<ImportJournalDetails> _importJournalDetails;
        public GenericRepository<ImportJournalDetails> ImportJournalDetails
        {
            get
            {
                if (this._importJournalDetails == null)
                {
                    this._importJournalDetails = new GenericRepository<ImportJournalDetails>(_context);
                }
                return _importJournalDetails;
            }
        }
        #endregion joournal
        #region GVGroup
        private GenericRepository<InvGiftVoucherGroup> _giftvoucherRepository;
        public GenericRepository<InvGiftVoucherGroup> GiftVoucherGroupRepository
        {
            get
            {
                if (this._giftvoucherRepository == null)
                {
                    this._giftvoucherRepository = new GenericRepository<InvGiftVoucherGroup>(_context);
                }
                return _giftvoucherRepository;
            }
        }
        #endregion GVGroup
        #region GVBook
        private GenericRepository<InvGiftVoucherBookCode> _giftvoucherbookRepository;
        public GenericRepository<InvGiftVoucherBookCode> GiftVoucherbookRepository
        {
            get
            {
                if (this._giftvoucherbookRepository == null)
                {
                    this._giftvoucherbookRepository = new GenericRepository<InvGiftVoucherBookCode>(_context);
                }
                return _giftvoucherbookRepository;
            }
        }
        #endregion GVBook
        #region GVVoucherMaster
        private GenericRepository<InvGiftVoucherMaster> _giftvoucherMasterRepository;
        public GenericRepository<InvGiftVoucherMaster> GiftVoucherMasterRepository
        {
            get
            {
                if (this._giftvoucherMasterRepository == null)
                {
                    this._giftvoucherMasterRepository = new GenericRepository<InvGiftVoucherMaster>(_context);
                }
                return _giftvoucherMasterRepository;
            }
        }
        #endregion GVVoucherMaster
        #region GVBookCancel
        private GenericRepository<InvGiftVoucherCancel> _giftvoucherCancelRepository;
        public GenericRepository<InvGiftVoucherCancel> GiftVoucherCancelRepository
        {
            get
            {
                if (this._giftvoucherCancelRepository == null)
                {
                    this._giftvoucherCancelRepository = new GenericRepository<InvGiftVoucherCancel>(_context);
                }
                return _giftvoucherCancelRepository;
            }
        }
        #endregion GVBook
        #region BudgetOutlet
        private GenericRepository<BudgetOutlet> _BudgetOutletRepository;
        public GenericRepository<BudgetOutlet> BudgetOutletRepository
        {
            get
            {
                if (this._BudgetOutletRepository == null)
                {
                    this._BudgetOutletRepository = new GenericRepository<BudgetOutlet>(_context);
                }
                return _BudgetOutletRepository;
            }
        }
        #endregion BudgetOutlet
        #region BudgetItemWise
        private GenericRepository<BudgetItemWise> _BudgetItemWiseRepository;
        public GenericRepository<BudgetItemWise> BudgetItemWiseRepository
        {
            get
            {
                if (this._BudgetItemWiseRepository == null)
                {
                    this._BudgetItemWiseRepository = new GenericRepository<BudgetItemWise>(_context);
                }
                return _BudgetItemWiseRepository;
            }
        }
        #endregion BudgetItemWise
        #region GiftVoucherGoodReceiveNote
        private GenericRepository<GiftVoucherGoodReceiveNote> _giftGiftVoucherGoodReceiveNoteRepository;
        public GenericRepository<GiftVoucherGoodReceiveNote> GiftVoucherGoodReceiveNoteRepository
        {
            get
            {
                if (this._giftGiftVoucherGoodReceiveNoteRepository == null)
                {
                    this._giftGiftVoucherGoodReceiveNoteRepository = new GenericRepository<GiftVoucherGoodReceiveNote>(_context);
                }
                return _giftGiftVoucherGoodReceiveNoteRepository;
            }
        }
        #endregion GiftVoucherGoodReceiveNote
        #region GVPO
        private GenericRepository<Supplier> _giftvoucherPORepository;
        public GenericRepository<Supplier> GiftVoucherPORepository
        {
            get
            {
                if (this._giftvoucherPORepository == null)
                {
                    this._giftvoucherPORepository = new GenericRepository<Supplier>(_context);
                }
                return _giftvoucherPORepository;
            }
        }
        #endregion GVPO
        #region InvGiftVoucherPurchaseOrderHeader
        private GenericRepository<InvGiftVoucherPurchaseOrderHeader> _gvpohrepository;
        public GenericRepository<InvGiftVoucherPurchaseOrderHeader> GVPOHRepository
        {
            get
            {
                if (this._gvpohrepository == null)
                {
                    this._gvpohrepository = new GenericRepository<InvGiftVoucherPurchaseOrderHeader>(_context);
                }
                return _gvpohrepository;
            }
        }
        #endregion InvGiftVoucherPurchaseOrderHeader
        #region InvGiftVoucherPurchaseOrderDetails
        private GenericRepository<InvGiftVoucherPurchaseOrderDetail> _gvpoDetailsrepository;
        public GenericRepository<InvGiftVoucherPurchaseOrderDetail> GVPODetailsRepository
        {
            get
            {
                if (this._gvpoDetailsrepository == null)
                {
                    this._gvpoDetailsrepository = new GenericRepository<InvGiftVoucherPurchaseOrderDetail>(_context);
                }
                return _gvpoDetailsrepository;
            }
        }
        #endregion InvGiftVoucherPurchaseOrderDetails
        #region InvGiftVoucherPurchaseOrderDocNo
        private GenericRepository<InvGiftVoucherDocumentNumber> _gvpoDocNorepository;
        public GenericRepository<InvGiftVoucherDocumentNumber> GVPODocNoRepository
        {
            get
            {
                if (this._gvpoDocNorepository == null)
                {
                    this._gvpoDocNorepository = new GenericRepository<InvGiftVoucherDocumentNumber>(_context);
                }
                return _gvpoDocNorepository;
            }
        }
        #endregion InvGiftVoucherPurchaseOrderDocNo
        #region GiftVoucherTransferHeader
        private GenericRepository<InvGiftVoucherTransferNoteHeader> _gvtGiftVoucherTransfer;
        public GenericRepository<InvGiftVoucherTransferNoteHeader> GVTransferRepository
        {
            get
            {
                if (this._gvtGiftVoucherTransfer == null)
                {
                    this._gvtGiftVoucherTransfer = new GenericRepository<InvGiftVoucherTransferNoteHeader>(_context);
                }
                return _gvtGiftVoucherTransfer;
            }
        }
        #endregion GiftVoucherTransferHeader
        #region GiftVoucherTransferDetails
        private GenericRepository<InvGiftVoucherTransferNoteDetail> _gvtGiftVoucherTransferDetails;
        public GenericRepository<InvGiftVoucherTransferNoteDetail> GVTransferDetailsRepository
        {
            get
            {
                if (this._gvtGiftVoucherTransferDetails == null)
                {
                    this._gvtGiftVoucherTransferDetails = new GenericRepository<InvGiftVoucherTransferNoteDetail>(_context);
                }
                return _gvtGiftVoucherTransferDetails;
            }
        }
        #endregion GiftVoucherTransferDetails
        #region InvGiftVoucherPurchaseHeader
        private GenericRepository<invGiftVoucherPurchaseHeaders> _gvphrepository;
        public GenericRepository<invGiftVoucherPurchaseHeaders> GVPHRepository
        {
            get
            {
                if (this._gvphrepository == null)
                {
                    this._gvphrepository = new GenericRepository<invGiftVoucherPurchaseHeaders>(_context);
                }
                return _gvphrepository;
            }
        }
        #endregion InvGiftVoucherPurchaseHeader
        #region InvGiftVoucherPurchaseDetails
        private GenericRepository<InvGiftVoucherPurchaseDetails> _gvpDetailsrepository;       
        public GenericRepository<InvGiftVoucherPurchaseDetails> GVPDetailsRepository
        {
            get
            {
                if (this._gvpDetailsrepository == null)
                {
                    this._gvpDetailsrepository = new GenericRepository<InvGiftVoucherPurchaseDetails>(_context);
                }
                return _gvpDetailsrepository;
            }
        }
         #endregion InvGiftVoucherPurchaseDetails
    }
}

