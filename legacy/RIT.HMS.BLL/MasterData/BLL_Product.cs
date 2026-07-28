using RIT.HMS.BLL.Common;
using RIT.HMS.Data;
using RIT.HMS.Domain;
using RIT.HMS.Domain.Logs;
using RIT.HMS.Domain.ViewModels;
using RIT.HMS.Domain.Transactions;
using RIT.HMS.Domain.ViewModels.DataUpload;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Data.Entity.Validation;
using System.Data.Entity.Infrastructure;

namespace RIT.HMS.BLL.MasterData
{
    public class BLL_Product
    {
        private readonly UnitOfWork _unitofwork;
        private readonly BLL_ProductStock _productstockmaster;
        private readonly BLL_Location _blllocation;
        public BLL_Product()
        {
            _unitofwork = new UnitOfWork();
            _productstockmaster = new BLL_ProductStock();
            _blllocation = new BLL_Location();
        }
        public BLL_Product(string connectionname)
        {
            _unitofwork = new UnitOfWork(connectionname);
            _productstockmaster = new BLL_ProductStock(connectionname);
            _blllocation = new BLL_Location(connectionname);
        }

        public IEnumerable<Product> GetProducts(Int32 compid)
        {
            try
            {
                IEnumerable<Product> products = _unitofwork.ProductRepository.Get(p => p.IsDelete == false && p.CompanyID == compid).OrderBy(c => c.ProductCode);
                if (products != null)
                {
                    return products;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<Product> GetAutoProductionProducts(Int32 compid)
        {
            try
            {
                IEnumerable<Product> products = _unitofwork.ProductRepository.Get(p => p.IsDelete == false && p.CompanyID == compid && p.AutoProduction == true).OrderBy(c => c.ProductCode);
                if (products != null)
                {
                    return products;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }


        public IEnumerable<Product> GetProductionItems(Int32 compid)
        {
            try
            {
                IEnumerable<Product> products = _unitofwork.ProductRepository.Get(p => p.IsRowMaterial == true && p.CompanyID == compid).OrderBy(c => c.ProductCode);
                if (products != null)
                {
                    return products;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<ProductStockMasterViewModel> GetProductionItems(long locid, int companyid)
        {
            try
            {
                var productionitems = (
                          from p in _unitofwork.ProductRepository.Get(p => p.CompanyID == companyid && p.IsActive == true && p.IsDelete == false && p.IsRowMaterial == false)
                          join ps in _unitofwork.ProductStockMasterRepository.Get(ps => ps.CompanyID == companyid && ps.LocationId == locid) on p.ProductId equals ps.ProductId
                          // where ps.LocationId == locid
                          // && p.IsActive == true && p.IsDelete == false && p.IsRowMaterial == false
                          //  && p.CompanyID==companyid && ps.CompanyID==companyid
                          orderby p.ProductName
                          select new
                          {
                              ProductId = p.ProductId,
                              ProductName = p.ProductName,
                              Cost = ps.CostPrice,
                              Selling = ps.SellingPrice,
                              Discounts = ps.DiscountPrc,
                              Stock = ps.Stock
                          }
                      ).ToList();

                if (productionitems != null)
                {
                    List<ProductStockMasterViewModel> vvm = new List<ProductStockMasterViewModel>();
                    foreach (var prd in productionitems)
                    {
                        ProductStockMasterViewModel vm = new ProductStockMasterViewModel();
                        vm.ProductId = prd.ProductId;
                        vm.ProductName = prd.ProductName;
                        vvm.Add(vm);

                    }
                    return vvm;
                }
                else
                    return null;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IEnumerable<ProductStockMasterViewModel> GetAllProductionItems(long locid)
        {
            try
            {
                var productionitems = (
                          from p in _unitofwork.ProductRepository.Get()
                          join ps in _unitofwork.ProductStockMasterRepository.Get() on p.ProductId equals ps.ProductId
                          where ps.LocationId == locid
                          && p.IsActive == true && p.IsDelete == false
                          orderby p.ProductName
                          select new
                          {
                              ProductId = p.ProductId,
                              ProductCode = p.ProductCode,
                              ProductName = p.ProductName,
                              Cost = ps.CostPrice,
                              Selling = ps.SellingPrice,
                              Discounts = ps.DiscountPrc,
                              Stock = ps.Stock
                          }
                      ).ToList();

                if (productionitems != null)
                {
                    List<ProductStockMasterViewModel> vvm = new List<ProductStockMasterViewModel>();
                    foreach (var prd in productionitems)
                    {
                        ProductStockMasterViewModel vm = new ProductStockMasterViewModel();
                        vm.ProductId = prd.ProductId;
                        vm.ProductCode = prd.ProductCode;
                        vm.ProductName = prd.ProductName;
                        vvm.Add(vm);

                    }
                    return vvm;
                }
                else
                    return null;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public List<Receipe> GetReceipesByProductId(long id)
        {
            try
            {
                List<Receipe> receipes = _unitofwork.ReceipeRepository.Get(r => r.ProductId == id).OrderBy(c => c.ReceipeId).ToList();
                if (receipes != null)
                {
                    return receipes;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public List<ProductServingUnit> GetservingUnitsByProductId(long id)
        {
            try
            {
                List<ProductServingUnit> servingunits = _unitofwork.ProductServingUnitRepository.Get(r => r.ProductId == id).OrderBy(c => c.ProductServingUnitId).ToList();
                if (servingunits != null)
                {
                    return servingunits;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public List<ProductTax> GetProductTaxByProductId(long id)
        {
            try
            {
                List<ProductTax> productTaxs = _unitofwork.ProductTaxRepository.Get(r => r.ProductId == id).OrderBy(c => c.ProductTaxId).ToList();
                if (productTaxs != null)
                {
                    return productTaxs;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public List<SupplierProduct> GetProductSuppliersByProductId(long id)
        {
            try
            {
                List<SupplierProduct> productsuppliers = _unitofwork.SupplierProductRepository.Get(r => r.ProductId == id).OrderBy(c => c.ProductId).ToList();
                if (productsuppliers != null)
                {
                    return productsuppliers;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public List<SupplierProduct>  ProductSuppliersByProductId(long id,long locationid)
        {
            try
            {
                List<SupplierProduct> productsuppliers = _unitofwork.SupplierProductRepository
    .Get(r => r.ProductId == id && r.LocationId == locationid)
    .OrderBy(c => c.ProductId)
    //.Take(1)
    .ToList();
                if (productsuppliers != null)
                {
                    return productsuppliers;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public List<KitchenPrinterTypes> GetProductKitchenPrinterTypesByProductId(long id)
        {
            try
            {
                List<KitchenPrinterTypes> kitchenPrinterTypes = _unitofwork.KitchenPrinterTypesRepository.Get(r => r.ProductID == id).OrderBy(c => c.ProductID).ToList();
                if (kitchenPrinterTypes != null)
                {
                    return kitchenPrinterTypes;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public List<InvPriceLevelList> GetPriceLevelListProductId(long id)
        {
            try
            {
                List<InvPriceLevelList> PriceLevelListTypes = _unitofwork.Invpricelevellists.Get(r => r.ProductID == id).OrderBy(c => c.ProductID).ToList();
                if (PriceLevelListTypes != null)
                {
                    return PriceLevelListTypes;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }


        public List<ProductStockMaster> GetProductStockMasterByProductId(long id)
        {
            try
            {
                List<ProductStockMaster> productstockmaster = _unitofwork.ProductStockMasterRepository.Get(r => r.ProductId == id).
                                                                OrderBy(c => c.LocationId).ToList();
                if (productstockmaster != null)
                {
                    return productstockmaster;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public ProductStockMaster GetProductStockMasterByProductIdLocId(long id, long locid)
        {
            try
            {
                ProductStockMaster productstockmaster = _unitofwork.ProductStockMasterRepository.Get(r => r.ProductId == id && r.LocationId == locid).FirstOrDefault();

                if (productstockmaster != null)
                {
                    return productstockmaster;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public List<ProductStockMaster> GetStockReport(long locid, long productid, int copmanyid, string stockcodeFrom, string stockcodeTo)
        {
            try
            {
                List<ProductStockMaster> productstockmaster = new List<ProductStockMaster>();

                //if (locid != 0 && productid != 0)
                //{
                //    productstockmaster = _unitofwork.ProductStockMasterRepository.Get(r => r.ProductId == productid && r.LocationId == locid && r.CompanyID== copmanyid).
                //                         OrderBy(c => c.ProductName).OrderBy(d => d.LocationId).ToList();
                //}
                //else if (locid != 0 && productid == 0)
                //{
                //DateTime startDate = new DateTime(2022, 2, 5);
                //DateTime endDate = new DateTime(2022, 5, 30);
                //productstockmaster = _unitofwork.ProductStockMasterRepository.Get(r => r.LocationId == locid && r.CompanyID == copmanyid).
                //                     OrderBy(c => c.ProductName).OrderBy(d => d.LocationId).ToList();
                //productstockmaster = _unitofwork.ProductStockMasterRepository.Get(r => r.LocationId == locid && r.CompanyID == copmanyid).
                //                    OrderBy(c => c.ProductName).OrderBy(d => d.LocationId).ToList();  //added by Aruna

                var result = _unitofwork.ProductStockMasterRepository.SQLQuery<ProductStockMaster>("[dbo].[SP_LoadAllproducts] @CompanyID,@LocationId,@productCodefrom,@productCodeto,@productID",
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = Convert.ToInt32(copmanyid) },
                    new SqlParameter("@LocationId", SqlDbType.BigInt) { Value = Convert.ToInt32(locid) },
                    new SqlParameter("@productCodefrom", SqlDbType.NVarChar)
                    {
                        Value = Convert.ToString(stockcodeFrom ?? string.Empty)
                    },
                    new SqlParameter("@productCodeto", SqlDbType.NVarChar)
                    {
                        Value = Convert.ToString(stockcodeTo ?? string.Empty)
                    },

                    new SqlParameter("@productID", SqlDbType.BigInt) { Value = Convert.ToInt32(productid) }
                    ).ToList();
                return result;
                //}
                //else if (locid == 0 && productid != 0)
                //{
                //    //productstockmaster = _unitofwork.ProductStockMasterRepository.Get(r => r.ProductId == productid && r.CompanyID == copmanyid).
                //    //                     OrderBy(c => c.ProductName).OrderBy(d => d.LocationId).ToList();

                //    productstockmaster = _unitofwork.ProductStockMasterRepository.Get(r => r.ProductId == productid && r.CompanyID == copmanyid ).
                //                            OrderBy(c => c.ProductName).OrderBy(d => d.LocationId).ToList(); //added by Aruna
                //}
                //else if (locid == 0 && productid == 0)
                //{
                //    //productstockmaster = _unitofwork.ProductStockMasterRepository.Get(r=>r.CompanyID== copmanyid).OrderBy(c => c.ProductName).OrderBy(d => d.LocationId).ToList();

                //    productstockmaster = _unitofwork.ProductStockMasterRepository.Get(r => r.CompanyID == copmanyid ).OrderBy(c => c.ProductName).OrderBy(d => d.LocationId).ToList();//added by Aruna
                //}

                //if (productstockmaster != null)
                //{
                //    return productstockmaster;
                //}
                //else
                //    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public List<ProductStockMaster> GetStockReportForPopUpSearch(long locid, long productid, int copmanyid, string stockcodeFrom, string stockcodeTo)
        {
            try
            {
                List<ProductStockMaster> productstockmaster = new List<ProductStockMaster>();
                var result = _unitofwork.ProductStockMasterRepository.SQLQuery<ProductStockMaster>("[dbo].[SP_LoadAllproductsForPopUp] @CompanyID,@LocationId,@productCodefrom,@productCodeto,@productID",
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = Convert.ToInt32(copmanyid) },
                    new SqlParameter("@LocationId", SqlDbType.BigInt) { Value = Convert.ToInt32(locid) },
                    new SqlParameter("@productCodefrom", SqlDbType.NVarChar) { Value = Convert.ToString(stockcodeFrom) },
                    new SqlParameter("@productCodeto", SqlDbType.NVarChar) { Value = Convert.ToString(stockcodeTo) },
                    new SqlParameter("@productID", SqlDbType.BigInt) { Value = Convert.ToInt32(productid) }
                    ).ToList();
                return result;

            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public IEnumerable<Product> GetProductByDepartmentId(long id, int companyid)
        {
            try
            {
                IEnumerable<Product> products = _unitofwork.ProductRepository.Get(g => g.IsDelete == false && g.DepartmentId == id && g.CompanyID == companyid).OrderBy(g => g.ProductCode);
                if (products != null)
                {
                    return products;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<Product> GetMenuByDepartmentId(long id)
        {
            try
            {
                IEnumerable<Product> products = _unitofwork.ProductRepository.Get(g => g.IsDelete == false
                                                && g.DepartmentId == id && g.IsRowMaterial == false).OrderBy(g => g.ProductCode);
                if (products != null)
                {
                    return products;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<Product> GetMenuByCategoryId(long id)
        {
            try
            {
                IEnumerable<Product> products = _unitofwork.ProductRepository.Get(g => g.IsDelete == false && g.CategoryId == id && g.IsRowMaterial == false).OrderBy(g => g.ProductCode);
                if (products != null)
                {
                    return products;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<Product> GetMenuBySubCategoryId(long id)
        {
            try
            {
                IEnumerable<Product> products = _unitofwork.ProductRepository.Get(g => g.IsDelete == false && g.SubCategoryId == id && g.IsRowMaterial == false).OrderBy(g => g.ProductCode);
                if (products != null)
                {
                    return products;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<Product> GetMenuByDeptCatId(long deptid, long catid)
        {
            try
            {
                IEnumerable<Product> products = _unitofwork.ProductRepository.Get(g => g.IsDelete == false && g.IsActive == true && g.DepartmentId == deptid &&
                                                                    g.CategoryId == catid && g.IsRowMaterial == false).OrderBy(g => g.ProductCode);
                if (products != null)
                {
                    return products;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<Product> GetMenuByDeptCatSCatId(long deptid, long catid, long scatid)
        {
            try
            {
                IEnumerable<Product> products = _unitofwork.ProductRepository.Get(g => g.IsDelete == false && g.IsActive == true && g.DepartmentId == deptid &&
                                                                        g.CategoryId == catid && g.SubCategoryId == scatid
                                                                        && g.IsRowMaterial == false
                                                                        ).OrderBy(g => g.ProductCode);
                if (products != null)
                {
                    return products;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<Product> GetMenuById(long id)
        {
            try
            {
                IEnumerable<Product> products = _unitofwork.ProductRepository.Get(g => g.IsDelete == false && g.IsActive == true && g.ProductId == id && g.IsRowMaterial == false).OrderBy(g => g.ProductCode);
                if (products != null)
                {
                    return products;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<Product> GetActiveProducts(Int32 compid)
        {
            try
            {
                var sysproducts = _unitofwork.ProductRepository.Get(p => p.CompanyID == compid && p.IsDelete == false && p.IsActive == true).Select(
                                                                        p => new
                                                                        {
                                                                            p.ProductId,
                                                                            p.ProductCode,
                                                                            p.ProductName,
                                                                            p.IsActive,
                                                                            p.IsDelete,
                                                                            p.IsRowMaterial
                                                                        }
                                                                        ).OrderBy(g => g.ProductCode);

                List<Product> products = new List<Product>();
                foreach (var p in sysproducts)
                {
                    Product prd = new Product();
                    prd.ProductId = p.ProductId;
                    prd.ProductCode = p.ProductCode;
                    prd.ProductName = p.ProductName;
                    prd.IsActive = p.IsActive;
                    prd.IsDelete = p.IsDelete;
                    prd.IsRowMaterial = p.IsRowMaterial;
                    products.Add(prd);
                }
                if (products != null)
                {
                    return products;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<ProductStockMaster> GetActiveProductsForStockAdjustment(int locationid)
        {
            try
            {
                var sysproducts = _unitofwork.ProductStockMasterRepository.Get().Select(p => new { p.ProductId, p.ProductCode, p.ProductName, p.IsActive, p.IsDelete, p.LocationId }).Where(g => g.IsDelete == false &&
                                                                              g.IsActive == true && g.LocationId == locationid).OrderBy(g => g.ProductCode);

                List<ProductStockMaster> products = new List<ProductStockMaster>();
                foreach (var p in sysproducts)
                {
                    ProductStockMaster prd = new ProductStockMaster();
                    prd.ProductId = p.ProductId;
                    prd.ProductCode = p.ProductCode;
                    prd.ProductName = p.ProductName;
                    prd.IsActive = p.IsActive;
                    prd.IsDelete = p.IsDelete;
                    //  prd.IsRowMaterial = p.IsRowMaterial;

                    products.Add(prd);
                }

                if (products != null)
                {
                    return products;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<Product> GetFinishGoods(Int32 compid)
        {
            try
            {
                var sysproducts = _unitofwork.ProductRepository.Get(g => g.CompanyID == compid).Select(p => new { p.ProductId, p.ProductCode, p.ProductName, p.IsActive, p.IsDelete, p.IsRowMaterial, p.IsOpenItem }).Where(g => g.IsDelete == false &&
                                                                                   g.IsActive == true && g.IsRowMaterial == false).OrderBy(g => g.ProductCode);

                List<Product> products = new List<Product>();
                foreach (var p in sysproducts)
                {
                    Product prd = new Product();
                    prd.ProductId = p.ProductId;
                    prd.ProductCode = p.ProductCode;
                    prd.ProductName = p.ProductName;
                    prd.IsActive = p.IsActive;
                    prd.IsDelete = p.IsDelete;
                    prd.IsRowMaterial = p.IsRowMaterial;

                    products.Add(prd);
                }

                if (products != null)
                {
                    return products;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<Product> GetOpenItems(int companyid)
        {
            try
            {
                var sysproducts = _unitofwork.ProductRepository.Get().Select(p => new { p.ProductId, p.ProductCode, p.ProductName, p.IsActive, p.IsDelete, p.IsRowMaterial, p.IsOpenItem, p.CompanyID }).Where(g => g.IsDelete == false &&
                                                                                g.IsActive == true && g.IsRowMaterial == false && g.IsOpenItem == true && g.CompanyID == companyid).OrderBy(g => g.ProductCode);

                List<Product> products = new List<Product>();
                foreach (var p in sysproducts)
                {
                    Product prd = new Product();
                    prd.ProductId = p.ProductId;
                    prd.ProductCode = p.ProductCode;
                    prd.ProductName = p.ProductName;
                    prd.IsActive = p.IsActive;
                    prd.IsDelete = p.IsDelete;
                    prd.IsRowMaterial = p.IsRowMaterial;

                    products.Add(prd);
                }

                if (products != null)
                {
                    return products;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<ProductServingUnit> GetServingUnits(long productid)
        {
            try
            {
                IEnumerable<ProductServingUnit> servinguints = _unitofwork.ProductServingUnitRepository.Get(p => p.ProductId == productid);
                if (servinguints != null)
                {
                    return servinguints;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<Product> GetRowMaterials()
        {
            try
            {
                var sysrowmaterials = _unitofwork.ProductRepository.Get().Select(p => new
                {
                    p.ProductId,
                    p.ProductCode,
                    p.ProductName,
                    p.IsActive,
                    p.IsDelete,
                    p.IsRowMaterial,
                    p.PurchasingUnit
                }
                ).Where(
               g => g.IsDelete == false &&
               g.IsActive == true && g.IsRowMaterial == true
               ).OrderBy(g => g.ProductCode).ToList();

                List<Product> rowmaterials = new List<Product>();
                foreach (var p in sysrowmaterials)
                {
                    Product prd = new Product();
                    prd.ProductId = p.ProductId;
                    prd.ProductCode = p.ProductCode;
                    prd.ProductName = p.ProductName;
                    prd.IsActive = p.IsActive;
                    prd.IsDelete = p.IsDelete;
                    prd.IsRowMaterial = p.IsRowMaterial;
                    prd.PurchasingUnit = p.PurchasingUnit;
                    rowmaterials.Add(prd);
                }
                if (rowmaterials != null)
                {
                    return rowmaterials;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<Product> GetAddons(Int32 compid)
        {
            try
            {
                var sysproducts = _unitofwork.ProductRepository.Get(p => p.CompanyID == compid).Select(p => new { p.ProductId, p.ProductCode, p.ProductName, p.IsActive, p.IsDelete, p.IsAddon, p.PurchasingUnit }).Where(g => g.IsDelete == false &&
                                                                                   g.IsActive == true && g.IsAddon == true).OrderBy(g => g.ProductCode);

                List<Product> products = new List<Product>();
                foreach (var p in sysproducts)
                {
                    Product prd = new Product();
                    prd.ProductId = p.ProductId;
                    prd.ProductCode = p.ProductCode;
                    prd.ProductName = p.ProductName;
                    prd.IsActive = p.IsActive;
                    prd.IsDelete = p.IsDelete;
                    prd.IsAddon = p.IsAddon;
                    prd.PurchasingUnit = p.PurchasingUnit;
                    products.Add(prd);
                }

                if (products != null)
                {
                    return products;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public List<Product> GetNotRawProducts(Int32 compid)
        {
            try
            {
                List<Product> products = _unitofwork.ProductRepository.Get(g => g.IsDelete == false &&
                                                                      g.IsActive == true && g.IsRowMaterial == false && g.CompanyID == compid).
                                                                      OrderBy(g => g.ProductCode).ToList();


                if (products != null)
                {
                    return products;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<Product> GetProductAddons(Int32 compid)
        {
            try
            {
                IEnumerable<Product> products = _unitofwork.ProductRepository.Get(g => g.IsDelete == false &&
                                                                      g.IsActive == true && g.IsRowMaterial == true && g.CompanyID == compid).OrderBy(g => g.ProductCode);
                if (products != null)
                {
                    return products;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<PrinterType> GetPrinterTypes()
        {
            try
            {
                IEnumerable<PrinterType> printertypes = _unitofwork.PrinterTypeRepository.Get(p => p.IsDelete == false);

                if (printertypes != null)
                {
                    return printertypes;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<InvPriceLevel> GetPriceLevel()
        {
            try
            {
                IEnumerable<InvPriceLevel> pricelevels = _unitofwork.InvPriceLevels.Get(p => p.IsDelete == false);

                if (pricelevels != null)
                {
                    return pricelevels;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public KitchenMaster GetKitchenPrinterTypesById(long id)
        {
            try
            {
                KitchenMaster kitchenPrinterTypes = _unitofwork.KitchenMasterRepository.GetById(id);
                if (kitchenPrinterTypes != null)
                {
                    return kitchenPrinterTypes;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public KitchenMaster GetPrinterById(long id)
        {
            try
            {
                KitchenMaster kitchenmater = _unitofwork.KitchenMasterRepository.GetById(id);
                if (kitchenmater != null)
                {
                    return kitchenmater;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public SysLocation GetLocations(long id)
        {
            try
            {
                SysLocation Locations = _unitofwork.LocationRepository.GetById(id);
                if (Locations != null)
                {
                    return Locations;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public InvPriceLevel GetPriceLevelName(long id)
        {
            try
            {
                InvPriceLevel pricelevelname = _unitofwork.InvPriceLevels.GetById(id);
                if (pricelevelname != null)
                {
                    return pricelevelname;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public ServingUnit GetservingUnitsByPriceLevelId(long id)
        {
            try
            {
                ServingUnit servingunits = _unitofwork.ServingUnit.GetById(id);
                if (servingunits != null)
                {
                    return servingunits;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public InvPriceLevelList GetPriceLevelPriceList(long id)
        {  
            try
            {
                InvPriceLevelList pricelevelpricelist = _unitofwork.Invpricelevellists.GetById(id);
                if (pricelevelpricelist != null)
                {
                    return pricelevelpricelist;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public KitchenMaster GetSupplierById(int id)
        {
            try
            {
                KitchenMaster kmaster = _unitofwork.KitchenMasterRepository.GetById(id);
                if (kmaster != null)
                {
                    return kmaster;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public SupplierProduct GetProductBySupplierId(long id)
        {
            try
            {
                var ProductBySupplierId = _unitofwork.SupplierProductRepository.Get(p => p.ProductId == id).FirstOrDefault();

                if (ProductBySupplierId != null)
                {
                    return ProductBySupplierId;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public IEnumerable<PrinterType> GetKitchenPrinterTypes()
        {
            try
            {
                IEnumerable<PrinterType> printertypes = _unitofwork.PrinterTypeRepository.Get(p => p.IsDelete == false);

                if (printertypes != null)
                {
                    return printertypes;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public PrinterType GetPrinterByName(string printername)
        {
            try
            {
                var printer = _unitofwork.PrinterTypeRepository.Get(p => p.PrinterTypeName == printername).FirstOrDefault();

                if (printer != null)
                {
                    return printer;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public SysLocation GetPrinterByLocation(string locationname)
        {
            try
            {
                var location= _unitofwork.LocationRepository.Get(p => p.LocationName == locationname).FirstOrDefault();

                if (location != null)
                {
                    return location;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }


        public InvPriceLevel GetPriceLevels(string pricelevelname)
        {
            try
            {
                var pricelevel = _unitofwork.InvPriceLevels.Get(p => p.PriceLevelName == pricelevelname).FirstOrDefault();

                if (pricelevel != null)
                {
                    return pricelevel;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public ServingUnit GetServingUnits(string Unitname)
        {
            try
            {
                var unit = _unitofwork.ServingUnit.Get(p => p.ServingUnitName == Unitname).FirstOrDefault();

                if (unit != null)
                {
                    return unit;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public Product GetProductById(long id)
        {
            try
            {
                var product = _unitofwork.ProductRepository.GetById(id);
                return product ?? null;
            }
            catch (Exception)
            {

                throw;
            }
        }


        public Product GetActiveProductById(long id)
        {
            try
            {
                var product = _unitofwork.ProductRepository.Get(p => p.ProductId == id &&
                          p.IsActive == true && p.IsDelete == false).FirstOrDefault();
                return product ?? null;
            }
            catch (Exception)
            {

                throw;
            }
        }


        public Product GetProductDescById(long id)
        {
            try
            {
                var prd = (
                          from p in _unitofwork.ProductRepository.Get(p => p.ProductId == id &&
                          p.IsActive == true && p.IsDelete == false)
                          join a in _unitofwork.AddonsRepository.Get() on p.ProductId equals a.ProductId
                          //where p.ProductId == id &&
                          //p.IsActive == true && p.IsDelete == false
                          orderby p.ProductName
                          select new
                          {
                              ProductId = p.ProductId,
                              ProductCode = p.ProductCode,
                              ProductName = p.ProductName,

                          }
                      ).ToList();

                Product products = new Product();
                products.ProductName = prd.First().ProductName;
                products.ProductCode = prd.First().ProductCode;
                return products ?? null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public Product GetAddonsDescById(long id)
        {
            Product products = new Product();
            try
            {
                var prd = (
                          from p in _unitofwork.ProductRepository.Get()
                          join a in _unitofwork.AddonsRepository.Get() on p.ProductId equals a.ProductAddonId
                          where a.ProductAddonId == id
                          orderby p.ProductName
                          select new
                          {
                              ProductId = p.ProductId,
                              ProductName = p.ProductName,
                              ProductCode = p.ProductCode,
                          }
                      ).ToList();

                products.ProductName = prd.First().ProductName;
                products.ProductCode = prd.First().ProductCode;
                return products ?? null;
            }
            catch (Exception ex)
            {
                return products;
                // throw;
            }
        }

        public Boolean CheckProductCodeExists(string productcode, Int32 compid)
        {
            try
            {
                // return _unitofwork.ProductRepository.Get().Any(g => g.ProductCode == productcode && g.CompanyID==compid);
                var exists = _unitofwork.ProductRepository.Get(g => g.ProductCode == productcode && g.CompanyID == compid).FirstOrDefault();
                if (exists == null)
                {
                    return false;
                }
                return true;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public bool SaveProduct(Product product)
        {

            _unitofwork.CreateTransaction();
            try
            {
              
                _unitofwork.ProductRepository.Insert(product);
                if (_unitofwork.Save() == 1)
                { 
                        // If taxes exists
                        if (product.ProductTax.Count > 0)
                    {
                        //foreach (var tax in product.ProductTax)
                        //{
                        //    tax.ProductId = product.ProductId;
                        //    tax.GroupOfCompanyID = 1;
                        //    tax.CompanyID = product.CompanyID;
                        //    tax.LocationId = product.LocationId;
                        //    tax.DataTransfer = 0;
                        //    tax.TaxSequence = product.ProductTax.IndexOf(tax) + 1;
                        //    tax.TaxPracentage = 100;
                        //    _unitofwork.ProductTaxRepository.Insert(tax);
                        //}

                        product.ProductTax.ForEach(pt =>
                        {
                            pt.ProductId = product.ProductId;
                            

                            //pt.ImagePath = System.IO.File.Move(product.ProductImageName, Convert.ToString(product.ProductId));
                            pt.GroupOfCompanyID = 1;
                            pt.CompanyID = product.CompanyID;
                            pt.LocationId = product.LocationId;
                            pt.DataTransfer = 0;
                            pt.TaxSequence = product.ProductTax.IndexOf(pt) + 1;
                            pt.TaxPracentage = 100;
                        });

                        _unitofwork.ProductTaxRepository.BulkInsert(product.ProductTax);
                        if (_unitofwork.Save() != product.ProductTax.Count)
                        {
                            _unitofwork.Rollback();
                            return false;
                        }
                    }


                    // If Serving units exists

                    if (product.ProductServingUnit.Count > 0)
                    {
                        //foreach (var servingunit in product.ProductServingUnit)
                        //{
                        //    servingunit.ProductId = product.ProductId;
                        //    servingunit.GroupOfCompanyID = 1;
                        //    servingunit.CompanyID = product.CompanyID;
                        //    servingunit.LocationId = product.LocationId;
                        //    servingunit.DataTransfer = 0;

                        //    _unitofwork.ProductServingUnitRepository.Insert(servingunit);
                        //}

                        //====GAYAN 05-09-2023====================================================================
                        if (product.ProductLocationViewModel.Count > 0)
                        {
                            List<ProductServingUnit> ProdServUnitbulk = new List<ProductServingUnit>();
                            foreach (var prdvm in product.ProductLocationViewModel)
                            {

                                var prdserun = new ProductServingUnit();

                                prdserun.ProductId = Convert.ToInt64(product.ProductId);
                                prdserun.LocationName = product.ProductName;
                                prdserun.ServingUnit = product.ProductServingUnit.FirstOrDefault().ServingUnit;
                                prdserun.GroupOfCompanyID = 1;
                                prdserun.CompanyID = product.CompanyID;
                                prdserun.LocationId = prdvm.LocationId;
                                prdserun.DataTransfer = 0;
                                prdserun.CreatedUser = product.CreatedUser;
                                prdserun.ModifiedUser = product.ModifiedUser;
                           
                                ProdServUnitbulk.Add(prdserun);

                            }

                            _unitofwork.ProductServingUnitRepository.BulkInsert(ProdServUnitbulk);
                            _unitofwork.Save();

                        }


                        //========================================================================

                        //product.ProductServingUnit.ForEach(psu =>
                        //{
                        //    psu.ProductId = product.ProductId;
                        //    psu.GroupOfCompanyID = 1;
                        //    psu.CompanyID = product.CompanyID;
                        //    psu.LocationId = product.LocationId;
                        //    psu.DataTransfer = 0;
                        //});

                        //_unitofwork.ProductServingUnitRepository.BulkInsert(product.ProductServingUnit);
                        //if (_unitofwork.Save() != product.ProductServingUnit.Count)
                        //{
                        //    _unitofwork.Rollback();
                        //    return false;
                        //}
                    }

                    // If Supplier exists

                    if (product.SupplierProduct.Count > 0)
                    {
                        //foreach (var supplierproduct in product.SupplierProduct)
                        //{
                        //    supplierproduct.ProductId = product.ProductId;
                        //    supplierproduct.GroupOfCompanyID = 1;
                        //    supplierproduct.CompanyID = product.CompanyID;
                        //    supplierproduct.LocationId = product.LocationId;
                        //    supplierproduct.DataTransfer = 0;
                        //    supplierproduct.ModifiedDate = product.ModifiedDate;
                        //    supplierproduct.CreatedDate = product.CreatedDate;
                        //    supplierproduct.CreatedUser = product.CreatedUser;
                        //    supplierproduct.ModifiedUser = product.ModifiedUser;                           
                        //    _unitofwork.SupplierProductRepository.Insert(supplierproduct);
                        //}

                        product.SupplierProduct.ForEach(
                            ps =>
                            {
                                ps.ProductId = product.ProductId;
                                ps.GroupOfCompanyID = 1;
                                ps.CompanyID = product.CompanyID;
                                ps.LocationId = product.LocationId;
                                ps.DataTransfer = 0;
                                ps.ModifiedDate = product.ModifiedDate;
                                ps.CreatedDate = product.CreatedDate;
                                ps.CreatedUser = product.CreatedUser;
                                ps.ModifiedUser = product.ModifiedUser;
                            });

                        _unitofwork.SupplierProductRepository.BulkInsert(product.SupplierProduct);
                        if (_unitofwork.Save() != product.SupplierProduct.Count)
                        {
                            _unitofwork.Rollback();
                            return false;
                        }
                    }

                    // If Stock Location Master Exists


                    if (product.ProductLocationViewModel.Count > 0)
                    {
                        List<ProductStockMaster> prdstockbulk = new List<ProductStockMaster>();
                        foreach (var prdvm in product.ProductLocationViewModel)
                        {
                            var prdstock = new ProductStockMaster();
                            prdstock.ProductId = product.ProductId;
                            prdstock.CostCentreId = prdvm.LocationId;
                            prdstock.StockCode = product.ProductCode;
                            prdstock.CostPrice = prdvm.CostPrice;
                            prdstock.SellingPrice = prdvm.SellingPrice;
                            prdstock.MinimumPrice = prdvm.MinPrice;
                            prdstock.MaxPrice = prdvm.MaxPrice;
                            prdstock.ReOrderLevel = prdvm.ReOrdderLevel;
                            prdstock.ReOrderQuantity = prdvm.ReOrderQuantity;
                            prdstock.ReOrderPeriod = prdvm.ReOrderPeriod;
                            prdstock.DiscountPrc = prdvm.DiscountPrc;
                            prdstock.ForignCustomerPrice = prdvm.ForignCustomerPrice;

                            prdstock.ProductCode = product.ProductCode;
                            prdstock.ProductName = product.ProductName;
                            prdstock.Barcode = product.Barcode;
                            prdstock.RefNo1 = product.RefCode01;
                            prdstock.RefNo2 = product.RefCode02;

                            prdstock.ExtendedId = 0;
                            prdstock.ExtendedName = "1";
                            prdstock.PLUCode = "1";
                            prdstock.WeightPerunit = 1;
                            prdstock.UomId = 0;
                            prdstock.Unit = "1";
                            prdstock.AvgCost = prdvm.CostPrice;
                            prdstock.FixedGP = 0;
                            prdstock.OpenBal = 0;
                            prdstock.InitSIH = 0;
                            prdstock.InitCost = 0;
                            prdstock.AdjQty = 0;
                            prdstock.IsDamage = false;
                            prdstock.IsActive = product.IsActive;
                            prdstock.IsBundle = false;
                            prdstock.IsInitialize = false;
                            prdstock.DataTransfer = 0;
                            prdstock.Ispacksize = false;
                            prdstock.Iscommission = false;
                            prdstock.Isdecimal = false;

                            prdstock.GroupOfCompanyID = product.GroupOfCompanyID;
                            prdstock.LocationId = prdvm.LocationId;
                            prdstock.CompanyID = product.CompanyID;
                            prdstock.CreatedDate = product.CreatedDate;
                            prdstock.CreatedUser = product.CreatedUser;
                            prdstock.ModifiedDate = product.ModifiedDate;
                            prdstock.ModifiedUser = product.ModifiedUser;
                            prdstock.PrinterType_Id = prdvm.PrinterType_Id;

                            prdstockbulk.Add(prdstock);
                            ///if (prdstock.CostPrice != 0 && prdstock.SellingPrice != 0)
                            ///{
                            //_unitofwork.ProductStockMasterRepository.Insert(prdstock);
                            //_unitofwork.Save();
                            /// }
                            /// 

                            LOGProductStockMaster lgprdstock = new LOGProductStockMaster();


                            var mappedprdstock = HMSExtensions.MatchAndMap(prdstock, lgprdstock);
                            mappedprdstock.SourceId = Convert.ToInt32(prdstock.ProductStockMasterId);
                             
                            _unitofwork.LOGProductStockMaster.Insert(mappedprdstock); 
                        }



                        _unitofwork.ProductStockMasterRepository.BulkInsert(prdstockbulk);
                        _unitofwork.Save();
                    }


                    if (product.ProductLocationViewModel.Count > 0)
                    {
                        List<KitchenPrinterTypes> prdprinterbulk = new List<KitchenPrinterTypes>();
                        foreach (var item in product.KitchenPrinters_Modl)
                        {
                            var printertype = new KitchenPrinterTypes();
                            printertype.ProductID = product.ProductId;
                            printertype.LocationID = 0;
                            printertype.PrinterID = Convert.ToInt32(item.KitchenID);
                            printertype.PrinterName = item.KitchenPrinterName;
                            printertype.CreatedDate = DateTime.Now;
                            printertype.CreatedUser = product.CreatedUser;
                            printertype.ModifiedDate = DateTime.Now;
                            printertype.ModifiedUser = product.ModifiedUser;
                            prdprinterbulk.Add(printertype);
                        }

                        _unitofwork.KitchenPrinterTypesRepository.BulkInsert(prdprinterbulk);
                        _unitofwork.Save();
                    }

                    //Price Levels
                    if (product.PriceLevelTypes.Count > 0)
                    {
                        List<InvPriceLevelList> pricelevellist = new List<InvPriceLevelList>();
                        foreach (var prdvm in product.PriceLevelTypes)
                        {
                            var pricelist = new InvPriceLevelList();

                            pricelist.PriceLevelID = Convert.ToInt32(prdvm.InvPriceLevelID);
                            pricelist.ProductID = product.ProductId;
                            pricelist.ServingUnitID = prdvm.ServingUnitID;
                            pricelist.CostPrice = prdvm.CostPrice;
                            pricelist.SellingPrice = prdvm.SellingPrice;
                            pricelist.Qty = prdvm.Qty;
                            pricelist.GroupOfCompanyID = product.GroupOfCompanyID;
                            pricelist.CompanyID = product.CompanyID;
                            pricelist.LocationId = prdvm.LocationId;
                            pricelist.CreatedUser = product.CreatedUser;
                            pricelist.CreatedDate = product.CreatedDate;
                            pricelist.ModifiedUser = product.ModifiedUser;
                            pricelist.ModifiedDate = product.ModifiedDate;
                            pricelist.DataTransfer = 0;
                            pricelist.IsDelete = false;
                            pricelevellist.Add(pricelist);
                        }

                        _unitofwork.Invpricelevellists.BulkInsert(pricelevellist);
                        _unitofwork.Save();
                    }
                        //

                        _unitofwork.Commit();
                    return true;
                }
                else
                {
                    _unitofwork.Rollback();
                    return false;
                }
            }

            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                throw;
            }
            //catch (Exception ex)
            //{
            //    _unitofwork.Rollback();
            //    throw;
            //}


        }

        public int DeleteReceipesByProductId(long id)
        {
            try
            {
                _unitofwork.ReceipeRepository.DeleteRange(_unitofwork.ReceipeRepository.Get(x => x.ProductId == id));
                var res = _unitofwork.Save();
                return res;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public int DeleteServingUnitsByProductId(long id)
        {
            try
            {
                _unitofwork.ProductServingUnitRepository.DeleteRange(_unitofwork.ProductServingUnitRepository.Get(x => x.ProductId == id));
                var res = _unitofwork.Save();
                return res;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public int DeleteTaxesByProductId(long id)
        {
            try
            {
                _unitofwork.ProductTaxRepository.DeleteRange(_unitofwork.ProductTaxRepository.Get(x => x.ProductId == id));
                var res = _unitofwork.Save();
                return res;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public int DeleteSupplierByProductId(long id)
        {
            try
            {
                _unitofwork.SupplierProductRepository.DeleteRange(_unitofwork.SupplierProductRepository.Get(x => x.ProductId == id));
                var res = _unitofwork.Save();
                return res;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public int DeletePrinterTypeByProductId(long id)
        {
            try
            {
                _unitofwork.KitchenPrinterTypesRepository.DeleteRange(_unitofwork.KitchenPrinterTypesRepository.Get(x => x.ProductID == id));
                var res = _unitofwork.Save();
                return res;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public int DeletePriceLevelProductId(long id)
        {
            try
            {
                _unitofwork.Invpricelevellists.DeleteRange(_unitofwork.Invpricelevellists.Get(x => x.ProductID == id));
                var res = _unitofwork.Save();
                return res;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public int DeleteLocationByProductId(long id)
        {
            try
            {
                _unitofwork.ProductStockMasterRepository.DeleteRange(_unitofwork.ProductStockMasterRepository.Get(x => x.ProductId == id));
                var res = _unitofwork.Save();
                return res;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public int UpdateProductHeader(Product product)
        {
            _unitofwork.ProductRepository.Update(product);
            int res = _unitofwork.Save();
            return res;
        }

        public int DeleteServingUnitsByProductIdAndServingUnit(string productcode, string name)
        {
            try
            {
                var productid = _unitofwork.ProductRepository.Get(p => p.ProductCode == productcode).FirstOrDefault().ProductId;
                _unitofwork.ProductServingUnitRepository.DeleteRange(_unitofwork.ProductServingUnitRepository.Get(
                x => x.ProductId == productid && x.ServingUnit == name));
                var res = _unitofwork.Save();
                return res;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public bool UpdateProduct(Product prd)
        {
            _unitofwork.CreateTransaction();
            try
            {
                _unitofwork.ProductRepository.Update(prd);
                 
                // logs 
                LOGProduct lgproduct = new LOGProduct();
                var mappedprd = HMSExtensions.MatchAndMap(prd, lgproduct);
                mappedprd.SourceId = prd.ProductId;
                

                if (_unitofwork.Save() > 0)
                {
                    // If Serving units exists

                    if (prd.ProductServingUnit.Count > 0)
                    {

                        // if (_unitofwork.ReceipeRepository.Get(r => r.ProductId == prd.ProductId).Count() == 0)
                        //{

                        //DeleteServingUnitsByProductId(prd.ProductId);
                        //foreach (var servingunit in prd.ProductServingUnit)
                        //{
                        //    servingunit.ProductId = prd.ProductId;
                        //    servingunit.GroupOfCompanyID = prd.GroupOfCompanyID;
                        //    servingunit.CompanyID = prd.CompanyID;
                        //    servingunit.LocationId = prd.LocationId;
                        //    servingunit.DataTransfer = 0;

                        //    var crrsu = _unitofwork.ProductServingUnitRepository.Get(p => p.ProductId == prd.ProductId
                        //                                && p.ServingUnit == servingunit.ServingUnit).Count();
                        //    if (crrsu == 0)
                        //    {
                        //        _unitofwork.ProductServingUnitRepository.Insert(servingunit);
                        //        _unitofwork.Save();
                        //    }

                        //    // logs 
                        //    LOGProductServingUnit lgprdservingunits = new LOGProductServingUnit();
                        //    var mappedprdsunits = HMSExtensions.MatchAndMap(servingunit, lgprdservingunits);
                        //    mappedprdsunits.SourceId = Convert.ToInt32(servingunit.ProductServingUnitId);
                        //    _unitofwork.LOGProductServingUnit.Insert(mappedprdsunits);

                        //}

                        ////if (_unitofwork.Save() != prd.ProductServingUnit.Count)
                        //if (_unitofwork.Save() == 0)
                        //{
                        //    _unitofwork.Rollback();
                        //    return false;
                        //}
                        //}
                        if (prd.ProductLocationViewModel.Count > 0)
                        {
                            List<ProductServingUnit> ProdServUnitbulk = new List<ProductServingUnit>();
                            foreach (var prdvm in prd.ProductLocationViewModel)
                            {
                                foreach (var item in prd.ProductServingUnit)
                                {
                                    var prdserun = new ProductServingUnit();

                                    var exists = _unitofwork.ProductServingUnitRepository.Get(ps => ps.ProductId == prd.ProductId
                                                                                      && ps.ServingUnit == item.ServingUnit
                                                                                      && ps.LocationId == prdvm.LocationId).ToList();
                                    if(exists==null || exists.Count==0)
                                    {
                                        prdserun.ProductId = Convert.ToInt64(prd.ProductId);
                                        prdserun.LocationName = prd.ProductName;
                                        prdserun.ServingUnit = item.ServingUnit;
                                        prdserun.GroupOfCompanyID = 1;
                                        prdserun.CompanyID = prd.CompanyID;
                                        prdserun.LocationId = prdvm.LocationId;
                                        prdserun.DataTransfer = 0;
                                        prdserun.CreatedUser = prd.CreatedUser;
                                        prdserun.ModifiedUser = prd.ModifiedUser;
                                        prdserun.DeductStockOnRecipe = item.DeductStockOnRecipe;
                                        ProdServUnitbulk.Add(prdserun);
                                    }
                                }
                            }

                            _unitofwork.ProductServingUnitRepository.BulkInsert(ProdServUnitbulk);
                            _unitofwork.Save();

                        }


                    }

                    // If taxes exists
                    if (prd.ProductTax.Count > 0)
                    {

                        DeleteTaxesByProductId(prd.ProductId);
                        foreach (var tax in prd.ProductTax)
                        {

                            tax.ProductId = prd.ProductId;
                            tax.GroupOfCompanyID = 1;
                            tax.CompanyID = prd.CompanyID;
                            tax.LocationId = prd.LocationId;
                            tax.DataTransfer = 0;
                            tax.TaxSequence = prd.ProductTax.IndexOf(tax) + 1;
                            tax.TaxPracentage = 100;

                            _unitofwork.ProductTaxRepository.Insert(tax);
                            _unitofwork.Save();
                            // logs 
                            LOGProductTax lgprdtax = new LOGProductTax();
                            var mappedprdtax = HMSExtensions.MatchAndMap(tax, lgprdtax);
                            mappedprdtax.SourceId = Convert.ToInt32(tax.ProductTaxId);
                            _unitofwork.LOGProductTax.Insert(mappedprdtax);

                        }

                        //if (_unitofwork.Save() != prd.ProductTax.Count)
                        if (_unitofwork.Save() == 0)
                        {
                            _unitofwork.Rollback();
                            return false;
                        }
                    }

                    // If suppliers exists
                    if (prd.SupplierProduct.Count > 0)
                    {

                        DeleteSupplierByProductId(prd.ProductId);

                        foreach (var supplier in prd.SupplierProduct)
                        {
                            supplier.ProductId = prd.ProductId;
                            supplier.GroupOfCompanyID = 1;
                            supplier.CompanyID = prd.CompanyID;
                            supplier.LocationId = prd.LocationId;
                            supplier.DataTransfer = 0;
                            _unitofwork.SupplierProductRepository.Insert(supplier);
                            _unitofwork.Save();

                            // logs 
                            LOGSupplierProduct lgsupprd = new LOGSupplierProduct();
                            var mappedprdsuplier = HMSExtensions.MatchAndMap(supplier, lgsupprd);
                            mappedprdsuplier.SourceId = Convert.ToInt32(supplier.SupplierProductId);
                            _unitofwork.LOGSupplierProduct.Insert(mappedprdsuplier);
                        }

                        //if (_unitofwork.Save() != prd.SupplierProduct.Count)
                        if (_unitofwork.Save() == 0)
                        {
                            _unitofwork.Rollback();
                            return false;
                        }
                    }

                    if (prd.KitchenPrinters_Modl1 != null)
                    {
                        if (prd.KitchenPrinters_Modl1.Count > 0)
                        {
                            DeletePrinterTypeByProductId(prd.ProductId);

                            foreach (var KitchenPrinters in prd.KitchenPrinters_Modl1)
                            {

                                var printertype = new KitchenPrinterTypes();
                                printertype.ProductID = prd.ProductId;
                                printertype.LocationID = KitchenPrinters.LocationID;
                                printertype.PrinterID = Convert.ToInt32(KitchenPrinters.ProductID);
                                printertype.PrinterName = KitchenPrinters.PrinterName;
                                printertype.CreatedDate = DateTime.Now;
                                printertype.CreatedUser = prd.CreatedUser;
                                printertype.ModifiedDate = DateTime.Now;
                                printertype.ModifiedUser = prd.ModifiedUser;
                                _unitofwork.KitchenPrinterTypesRepository.Insert(KitchenPrinters);
                                _unitofwork.Save();

                            }
                        }
                    }
                    else
                    {
                        DeletePrinterTypeByProductId(prd.ProductId);
                    }


                    prd.KitchenPrinters_Modl1 = new List<KitchenPrinterTypes>();
                    // if locations exists
                    if (prd.ProductLocationViewModel.Count > 0)
                    {
                        //   DeleteLocationByProductId(prd.ProductId);

                        foreach (var loc in prd.ProductLocationViewModel)
                        {
                            if (_unitofwork.ProductStockMasterRepository.Get(ps => ps.LocationId == loc.LocationId).Any(se => se.ProductId == loc.ProductId))
                            {
                               // var PriceList = _productstockmaster.GetProductStockMasterByProductId(Convert.ToInt64(loc.ProductId));

                                 
                                    var dbproductstockmaster = psm(loc.ProductId, loc.LocationId);

                                    dbproductstockmaster.ProductId = prd.ProductId;
                                    dbproductstockmaster.ProductName = prd.ProductName;
                                    dbproductstockmaster.LocationId = loc.LocationId;
                                    dbproductstockmaster.CostCentreId = loc.LocationId;

                                    dbproductstockmaster.CostPrice = loc.CostPrice;
                                    dbproductstockmaster.SellingPrice = loc.SellingPrice;
                                    dbproductstockmaster.ReOrderLevel = loc.ReOrdderLevel;
                                    dbproductstockmaster.ReOrderQuantity = loc.ReOrderQuantity;
                                    dbproductstockmaster.ReOrderPeriod = loc.ReOrderPeriod;
                                    dbproductstockmaster.MaxPrice = loc.MaxPrice;
                                    dbproductstockmaster.MinimumPrice = loc.MinPrice;
                                    dbproductstockmaster.DiscountPrc = loc.DiscountPrc;
                                    dbproductstockmaster.ForignCustomerPrice = loc.ForignCustomerPrice;
                                    dbproductstockmaster.Stock = dbproductstockmaster.Stock;
                                    dbproductstockmaster.CostCentreId = dbproductstockmaster.CostCentreId;
                                    dbproductstockmaster.DocumentNo = dbproductstockmaster.DocumentNo;

                                    dbproductstockmaster.ProductCode = prd.ProductCode;
                                    dbproductstockmaster.ProductName = prd.ProductName;
                                    dbproductstockmaster.Barcode = prd.Barcode;
                                    dbproductstockmaster.StockCode = prd.ProductCode;
                                    dbproductstockmaster.RefNo1 = prd.RefCode01;
                                    dbproductstockmaster.RefNo2 = prd.RefCode02;

                                    dbproductstockmaster.ExtendedId = 0;
                                    dbproductstockmaster.ExtendedName = "1";
                                    dbproductstockmaster.PLUCode = "1";
                                    dbproductstockmaster.WeightPerunit = 1;
                                    dbproductstockmaster.UomId = 0;
                                    dbproductstockmaster.Unit = "1";
                                    dbproductstockmaster.AvgCost = loc.AverageCost;
                                    dbproductstockmaster.FixedGP = 0;
                                    dbproductstockmaster.OpenBal = 0;
                                    dbproductstockmaster.InitSIH = 0;
                                    dbproductstockmaster.InitCost = 0;
                                    dbproductstockmaster.AdjQty = 0;
                                    //  dbproductstockmaster.AvgCost = 0;
                                    dbproductstockmaster.IsDamage = false;
                                    dbproductstockmaster.IsActive = prd.IsActive;
                                
                                    dbproductstockmaster.IsDelete = prd.IsDelete; // 2025-02-14

                                dbproductstockmaster.IsBundle = false;
                                    dbproductstockmaster.IsInitialize = false;
                                    dbproductstockmaster.DataTransfer = 0;
                                    dbproductstockmaster.Ispacksize = false;
                                    dbproductstockmaster.Iscommission = false;
                                    dbproductstockmaster.Isdecimal = false;

                                    dbproductstockmaster.GroupOfCompanyID = prd.GroupOfCompanyID;
                                    dbproductstockmaster.LocationId = loc.LocationId;
                                    dbproductstockmaster.CompanyID = prd.CompanyID;
                                    dbproductstockmaster.CreatedDate = prd.CreatedDate;
                                    dbproductstockmaster.CreatedUser = prd.CreatedUser;
                                    dbproductstockmaster.ModifiedDate = prd.ModifiedDate;
                                    dbproductstockmaster.ModifiedUser = prd.ModifiedUser;
                                    dbproductstockmaster.PrinterType_Id = loc.PrinterType_Id;
                                    dbproductstockmaster.LastUpdatedDate = DateTime.Now;

                                    //if (dbproductstockmaster.CostPrice != 0)
                                    //{
                                    // context.ProductStockMaster.Add(ps);

                                    // logs 
                                    LOGProductStockMaster lgprdstock = new LOGProductStockMaster();


                                    var mappedprdstock = HMSExtensions.MatchAndMap(dbproductstockmaster, lgprdstock);
                                    mappedprdstock.SourceId = Convert.ToInt32(dbproductstockmaster.ProductStockMasterId);



                                    //if (p.CostPrice != mappedprdstock.CostPrice && p.SellingPrice != mappedprdstock.SellingPrice)
                                 //   {
                                        _unitofwork.LOGProductStockMaster.Insert(mappedprdstock);
                              //      }


                                    int fff = _unitofwork.Save();
                                 
                                //}
                            }
                            else
                            {
                                var ps = new ProductStockMaster();
                                ps.ProductId = prd.ProductId;
                                ps.LocationId = loc.LocationId;
                                ps.CompanyID = prd.CompanyID;
                                ps.CostCentreId = loc.LocationId;

                                ps.CostPrice = loc.CostPrice;
                                ps.SellingPrice = loc.SellingPrice;
                                ps.ReOrderLevel = loc.ReOrdderLevel;
                                ps.ReOrderQuantity = loc.ReOrderQuantity;
                                ps.ReOrderPeriod = loc.ReOrderPeriod;
                                ps.MaxPrice = loc.MaxPrice;
                                ps.MinimumPrice = loc.MinPrice;
                                ps.DiscountPrc = loc.DiscountPrc;
                                ps.ForignCustomerPrice = loc.ForignCustomerPrice;
                                ps.Stock = 0;
                                ps.CostCentreId = loc.LocationId;
                                ps.DocumentNo = "";

                                ps.ProductCode = prd.ProductCode;
                                ps.ProductName = prd.ProductName;
                                ps.Barcode = prd.Barcode;
                                ps.StockCode = prd.ProductCode;
                                ps.RefNo1 = prd.RefCode01;
                                ps.RefNo2 = prd.RefCode02;

                                ps.ExtendedId = 0;
                                ps.ExtendedName = "1";
                                ps.PLUCode = "1";
                                ps.WeightPerunit = 1;
                                ps.UomId = 0;
                                ps.Unit = "1";
                                ps.AvgCost = 0;
                                ps.FixedGP = 0;
                                ps.OpenBal = 0;
                                ps.InitSIH = 0;
                                ps.InitCost = 0;
                                ps.AdjQty = 0;
                                ps.AvgCost = 0;
                                ps.IsDamage = false;
                                ps.IsActive = prd.IsActive;

                                ps.IsActive = prd.IsDelete; // 2025-02-14


                                ps.IsBundle = false;
                                ps.IsInitialize = false;
                                ps.DataTransfer = 0;
                                ps.Ispacksize = false;
                                ps.Iscommission = false;
                                ps.Isdecimal = false;

                                ps.GroupOfCompanyID = prd.GroupOfCompanyID;
                                ps.LocationId = loc.LocationId;
                                ps.CompanyID = prd.CompanyID;
                                ps.CreatedDate = prd.CreatedDate;
                                ps.CreatedUser = prd.CreatedUser;
                                ps.ModifiedDate = prd.ModifiedDate;
                                ps.ModifiedUser = prd.ModifiedUser;
                                ps.PrinterType_Id = loc.PrinterType_Id;
                                //if (loc.CostPrice != 0)
                                //{
                                _unitofwork.ProductStockMasterRepository.Insert(ps);
                                _unitofwork.Save();

                                // logs 
                                LOGProductStockMaster lgprdstock = new LOGProductStockMaster();
                                var mappedprdstock = HMSExtensions.MatchAndMap(ps, lgprdstock);
                                mappedprdstock.SourceId = Convert.ToInt32(ps.ProductStockMasterId);
                                _unitofwork.LOGProductStockMaster.Insert(mappedprdstock);
                                int fff = _unitofwork.Save();
                                //}
                            }
                        }
                    }
                    //Price Levels
                     
                    if (prd.PriceLevelLists != null)
                    {
                        if (prd.PriceLevelLists.Count > 0)
                        {
                            DeletePriceLevelProductId(prd.ProductId);
                            
                            foreach (var prdvm in prd.PriceLevelLists)
                            {
                               
                                    var pricelist = new InvPriceLevelList();

                                    pricelist.PriceLevelID = Convert.ToInt32(prdvm.PriceLevelID);
                                    pricelist.ProductID = prd.ProductId;
                                    pricelist.ServingUnitID = Convert.ToInt32(prdvm.ServingUnitID);
                                    pricelist.CostPrice = prdvm.CostPrice;
                                    pricelist.SellingPrice = prdvm.SellingPrice;
                                    pricelist.Qty = prdvm.Qty;
                                    pricelist.GroupOfCompanyID = prd.GroupOfCompanyID;
                                    pricelist.CompanyID = prd.CompanyID;
                                    pricelist.LocationId = prdvm.LocationId;
                                    pricelist.CreatedUser = prd.CreatedUser;
                                    pricelist.CreatedDate = prd.CreatedDate;
                                    pricelist.ModifiedUser = prd.ModifiedUser;
                                    pricelist.ModifiedDate = prd.ModifiedDate;
                                    pricelist.DataTransfer = 0;
                                    pricelist.IsDelete = false;
                                    _unitofwork.Invpricelevellists.Insert(pricelist);
                                    _unitofwork.Save();
                            }
                        }
                        else
                        {
                            DeletePriceLevelProductId(prd.ProductId);
                        }
                    }
                    else
                    {
                        DeletePriceLevelProductId(prd.ProductId);
                    }
                    
                    //

                    _unitofwork.Commit();
                    return true;

                    
                }
                else
                {
                    _unitofwork.Rollback();
                    return false;

                }
            }
            catch (Exception ex)
            {
                _unitofwork.Rollback();
                throw;
            }
        }

        public ProductStockMaster psm(long productid, long locationid)
        {
            try
            {
                long loc = locationid;
                long prd = productid;
                ProductStockMaster productStockMaster = _unitofwork.ProductStockMasterRepository.Get(r => r.ProductId == productid &&
                                                r.LocationId == locationid).ToList().First();
                if (productStockMaster != null)
                {
                    return productStockMaster;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public List<long> GetProductStockMasterByLocId(long frmloc, long toloc)
        {
            try
            {
                List<ProductStockMaster> pslist = new List<ProductStockMaster>();
                List<long> from = new List<long>();
                List<long> to = new List<long>();
                List<long> match = new List<long>();

                foreach (var p in _unitofwork.ProductStockMasterRepository.Get(p => p.LocationId == frmloc).ToList())
                {
                    from.Add(p.ProductId);
                }
                foreach (var p in _unitofwork.ProductStockMasterRepository.Get(p => p.LocationId == toloc).ToList())
                {
                    to.Add(p.ProductId);
                }
                foreach (long x in from)
                {
                    if (from.Contains(x) && to.Contains(x))
                    {
                        match.Add(x);
                    }

                }
                foreach (long y in to)
                {
                    if (from.Contains(y) && to.Contains(y))
                    {
                        if (!match.Contains(y))
                        {
                            match.Add(y);
                        }
                    }
                }

                return match;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public List<ProductStockMaster> GetProductsByLocId(long locid)
        {
            try
            {
                return _unitofwork.ProductStockMasterRepository.Get(p => p.LocationId == locid).ToList();
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public List<ProductStockMasterViewModel> GetRowMaterialsByLocId(long locid)
        {
            try
            {
                var products = (from p in _unitofwork.ProductRepository.Get()
                                join pm in _unitofwork.ProductStockMasterRepository.Get() on p.ProductId equals pm.ProductId
                                join um in _unitofwork.UnitOfMeasureRepository.Get() on p.PurchasingUnit equals um.UnitOfMeasureId
                                where pm.LocationId == locid && p.IsRowMaterial == true
                                select new
                                {
                                    ProductId = pm.ProductId,
                                    ProductCode = p.ProductCode,
                                    ProductName = p.ProductName,
                                    UOM = um.UnitOfMeasureName
                                });
                List<ProductStockMasterViewModel> vvm = new List<ProductStockMasterViewModel>();
                foreach (var p in products)
                {
                    ProductStockMasterViewModel vm = new ProductStockMasterViewModel();
                    vm.ProductId = p.ProductId;
                    vm.ProductCode = p.ProductCode;
                    vm.ProductName = p.ProductName;
                    vm.UOM = p.UOM;
                    vvm.Add(vm);
                }

                return vvm;
            }
            catch (Exception ex)
            {

                return null;
            }
        }

        public string GetUOMById(long uomid)
        {
            if (uomid != 0)
            {
                var uom = _unitofwork.UnitOfMeasureRepository.Get(u => u.UnitOfMeasureId == uomid).FirstOrDefault().UnitOfMeasureName;
                if (uom == null)
                {
                    return "";
                }
                else
                {
                    return uom;
                }
            }
            else
            {
                return "";
            }
        }

        public string GetUnitConvertionById(long uomid)
        {
            if (uomid != 0)
            {
                var uom = _unitofwork.UnitConversionRepository.Get(u => u.UnitConversionId == uomid).FirstOrDefault().SubUnit;
                if (uom == null)
                {
                    return "";
                }
                else
                {
                    return uom;
                }
            }
            else
            {
                return "";
            }
        }

        public List<ProductStockMaster> GetProductsByLocIdProductId(long locid, long prdid)
        {
            try
            {
                return _unitofwork.ProductStockMasterRepository.Get(p => p.LocationId == locid && p.ProductId == prdid).ToList();

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public decimal GetActualProductStock(long locid, long prdid)
        {
            try
            {
                var stock = _unitofwork.ProductStockMasterRepository.Get(p => p.LocationId == locid &&
                                                                         p.ProductId == prdid).FirstOrDefault().Stock;

                var tempprns = (from p in _unitofwork.PurchaseHeaderRepository.Get(p => p.GRNLocationId == locid && (p.DocumentStatus == 1 || p.DocumentStatus == 2) && p.DocumentID == 6)
                                join u in _unitofwork.PurchaseDetailRepository.Get(u => u.ProductID == prdid) on p.PurchaseHeaderId equals u.PurchaseHeaderID
                                // where p.GRNLocationId == locid && 
                                // (p.DocumentStatus==1 || p.DocumentStatus == 2) && p.DocumentID==6                              
                                // && u.ProductID==prdid                             
                                select new
                                {
                                    u.OrderQty
                                }
                              ).ToList();

                var togs = (from p in _unitofwork.TransferNoteHeaderRepository.Get(p => p.FromLocationId == locid &&
                           (p.DocumentStatus == 1 || p.DocumentStatus == 2))
                            join u in _unitofwork.TransferNoteDetailRepository.Get(u => u.ProductId == prdid) on p.TransferNoteHeaderId equals u.TransferNoteHeaderId
                            // where p.FromLocationId == locid &&
                            // (p.DocumentStatus == 1 || p.DocumentStatus == 2) &&
                            // u.ProductId == prdid
                            select new
                            {
                                u.OrderQty
                            }
                          ).ToList();

                decimal actualstock = 0;

                if (tempprns != null && togs != null)
                {
                    actualstock = stock - tempprns.Sum(p => p.OrderQty) - togs.Sum(t => t.OrderQty);
                    if (actualstock >= 0)
                    {
                        return actualstock;
                    }
                    else
                    {
                        return 0;
                    }

                }
                else if (tempprns == null && togs != null)
                {
                    actualstock = stock - togs.Sum(t => t.OrderQty);
                    if (actualstock >= 0)
                    {
                        return actualstock;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else if (tempprns != null && togs == null)
                {
                    actualstock = stock - togs.Sum(t => t.OrderQty);
                    if (actualstock >= 0)
                    {
                        return actualstock;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    return stock;
                }

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public decimal GetActualProductStockTOG(long locid, long prdid,ref decimal HoldStock)
        {
            try
            {
               
                var stock = _unitofwork.ProductStockMasterRepository.Get(p => p.LocationId == locid &&
                                                                         p.ProductId == prdid).FirstOrDefault().Stock;

                var togs = (from p in _unitofwork.TransferNoteHeaderRepository.Get(p => p.FromLocationId == locid &&
                                (p.DocumentStatus == 1 || p.DocumentStatus == 2))
                            join u in _unitofwork.TransferNoteDetailRepository.Get(u => u.ProductId == prdid) on p.TransferNoteHeaderId equals u.TransferNoteHeaderId
                            //where p.FromLocationId == locid &&
                            //(p.DocumentStatus == 1 || p.DocumentStatus==2) &&
                            //u.ProductId == prdid
                            select new
                            {
                                u.OrderQty
                            }
                                ).ToList();

                var prns = (from p in _unitofwork.PurchaseHeaderRepository.Get(p => p.GRNLocationId == locid &&
                                (p.DocumentStatus == 1 || p.DocumentStatus == 2) && p.DocumentID == 6)
                            join u in _unitofwork.PurchaseDetailRepository.Get(u => u.ProductID == prdid) on p.PurchaseHeaderId equals u.PurchaseHeaderID
                            //where p.GRNLocationId == locid &&
                            //  (p.DocumentStatus == 1 || p.DocumentStatus == 2) && p.DocumentID == 6
                            // && u.ProductID == prdid
                            select new
                            {
                                u.OrderQty
                            }
                            ).ToList();

                decimal actualstock = 0;

                if (togs != null && prns != null)
                {
                    HoldStock= togs.Sum(p => p.OrderQty)+ prns.Sum(p => p.OrderQty);
                    actualstock = stock - togs.Sum(p => p.OrderQty) - prns.Sum(p => p.OrderQty);
                    if (actualstock >= 0)
                    {
                        return actualstock;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else if (togs == null && prns != null)
                {
                    HoldStock = prns.Sum(p => p.OrderQty);
                    actualstock = stock - prns.Sum(p => p.OrderQty);
                    if (actualstock >= 0)
                    {
                        return actualstock;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else if (togs != null && prns == null)
                {
                    HoldStock = togs.Sum(p => p.OrderQty) ;
                    actualstock = stock - togs.Sum(p => p.OrderQty);
                    if (actualstock >= 0)
                    {
                        return actualstock;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    return stock;
                }

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public ProductStockMasterViewModel GetReceipeDetails(long locid, long productid, decimal qty, decimal UnitConvertion, int companyid)
        {
            try
            {
                if (UnitConvertion <= 0) { UnitConvertion = 1; }

                // var product = _unitofwork.ProductStockMasterRepository.Get(p => p.LocationId == locid && p.ProductId == productid && p.CompanyID==companyid).FirstOrDefault();
                ProductStockMasterViewModel vm = new ProductStockMasterViewModel();
                //if (product != null)
                //{
                //    vm.CostPrice = (product.AvgCost / UnitConvertion) * qty;
                //    vm.SellingPrice = (product.SellingPrice / UnitConvertion) * qty;
                //    vm.ProductName = product.ProductName;
                //    vm.ProductId = product.ProductId;
                //    vm.ProductCode = product.ProductCode;
                //}

                var query = (from p in _unitofwork.ProductRepository.Get(p => p.CompanyID == companyid && p.ProductId == productid)
                             join ps in _unitofwork.ProductStockMasterRepository.Get(ps => ps.CompanyID == companyid && ps.LocationId == locid
                                                                                     && ps.ProductId == productid)
                             on p.ProductId equals ps.ProductId into productstock
                             from x in productstock.DefaultIfEmpty()
                             select new
                             {
                                 ProductId = p.ProductId,
                                 ProductCode = p.ProductCode,
                                 ProductName = p.ProductName,
                                 AverageCost = (x == null ? 0 : x.AvgCost),
                                 Costprice = (x == null ? 0 : x.CostPrice),
                                 SellingPrice = (x == null ? 0 : x.SellingPrice),
                             }).FirstOrDefault();

                if (query != null)
                {
                    vm.CostPrice = (query.AverageCost / UnitConvertion) * qty;
                    vm.SellingPrice = (query.SellingPrice / UnitConvertion) * qty;
                    vm.ProductName = query.ProductName;
                    vm.ProductId = query.ProductId;
                    vm.ProductCode = query.ProductCode;
                }
                return vm;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public List<ProductStockMasterViewModel> CostPriceForAllLocations(long productid, decimal qty, decimal UnitConvertion, int companyid)
        {
            try
            {
                var showroomlocations = _unitofwork.LocationRepository.Get(l => l.CompanyID == companyid && l.IsActive == true && l.IsDelete == false && l.IsShowRoom == true).ToList();
                var product = _unitofwork.ProductStockMasterRepository.Get(p => p.ProductId == productid && p.CompanyID == companyid);
                var res = (from
                           l in showroomlocations
                           join p in product on l.SysLocationID equals p.CostCentreId
                           select new
                           {
                               p.AvgCost,
                               p.CostPrice,
                               p.SellingPrice,
                               p.ProductId,
                               p.ProductCode,
                               p.ProductName,
                               l.LocationName,
                           }
                    ).ToList();

                List<ProductStockMasterViewModel> vvm = new List<ProductStockMasterViewModel>();

                foreach (var s in res)
                {
                    ProductStockMasterViewModel v = new ProductStockMasterViewModel();
                    v.CostPrice = (s.AvgCost / UnitConvertion) * qty;
                    v.SellingPrice = (s.SellingPrice / UnitConvertion) * qty;
                    v.ProductName = s.ProductName;
                    v.ProductId = s.ProductId;
                    v.ProductCode = s.ProductCode;
                    v.LocationName = s.LocationName;
                    vvm.Add(v);

                }
                return vvm;
            }
            catch (Exception)
            {

                throw;
            }
        }


        public int RemoveAddons(long id, long aid)
        {
            try
            {
                _unitofwork.AddonsRepository.DeleteRange(_unitofwork.AddonsRepository.Get(x => x.ProductId == id && x.ProductAddonId == aid));
                //var ad = _unitofwork.AddonsRepository.Get(x => x.ProductId == id && x.ProductAddonId == aid).FirstOrDefault();
                //LOGAddons logaddons = new LOGAddons();
                //var mapped = Common.HMSExtensions.MatchAndMap(ad, logaddons);
                //mapped.SourceId = ad.AddonsId;
                //mapped.Action = "Removed";
                //_unitofwork.LOGAddons.Insert(mapped);

                var res = _unitofwork.Save();
                return res;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public int RemoveAddonsbyId(Addons exists)
        {
            try
            {
                _unitofwork.AddonsRepository.Delete(exists.AddonsId);

                LOGAddons logaddons = new LOGAddons();
                var mapped = Common.HMSExtensions.MatchAndMap(exists, logaddons);
                mapped.SourceId = exists.AddonsId;
                mapped.Action = "Removed From Source";
                _unitofwork.LOGAddons.Insert(mapped);
                var res = _unitofwork.Save();
                return res;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public Product FindByCode(string code, Int32 compid)
        {
            var product = _unitofwork.ProductRepository.Get(p => p.ProductCode == code && p.CompanyID == compid).FirstOrDefault();
            if (product != null)
            {
                return product;
            }
            else
            {
                return null;
            }
        }

        public IEnumerable<Product> GetRowMaterialsForRecipe(Int32 compid)
        {
            try
            {
                var sysrowmaterials = (from p in _unitofwork.ProductRepository.Get(p => p.CompanyID == compid)
                                       join u in _unitofwork.UnitConversionRepository.Get() on p.WeightPerUnit equals u.UnitConversionId
                                       where p.IsDelete == false && p.IsActive == true
                                       //&& p.IsRowMaterial == true
                                       select new
                                       {
                                           p.ProductId,
                                           p.ProductCode,
                                           p.ProductName,
                                           p.IsActive,
                                           p.IsDelete,
                                           p.IsRowMaterial,
                                           p.PurchasingUnit,
                                           p.WeightPerUnit,
                                           u.SubUnit,
                                           u.SubUnitValue
                                       }).OrderBy(g => g.ProductCode).ToList();

                List<Product> rowmaterials = new List<Product>();
                foreach (var p in sysrowmaterials)
                {
                    Product prd = new Product();
                    prd.ProductId = p.ProductId;
                    prd.ProductCode = p.ProductCode;
                    prd.ProductName = p.ProductName;
                    prd.IsActive = p.IsActive;
                    prd.IsDelete = p.IsDelete;
                    prd.IsRowMaterial = p.IsRowMaterial;
                    prd.PurchasingUnit = p.PurchasingUnit;
                    prd.WeightPerUnit = p.WeightPerUnit;
                    prd.UOM = p.SubUnit;
                    prd.PackSize = p.SubUnitValue;

                    rowmaterials.Add(prd);
                }
                if (rowmaterials != null)
                {
                    return rowmaterials;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public List<ProductStockMasterViewModel> GetLocationProductsByLocId(long fromlocation, long tolocation)
        {
            var products = (from ps in _unitofwork.ProductStockMasterRepository.Get()
                            join p in _unitofwork.ProductRepository.Get() on ps.ProductId equals p.ProductId
                            join uom in _unitofwork.UnitOfMeasureRepository.Get() on p.PurchasingUnit equals uom.UnitOfMeasureId
                            where   ps.CostPrice>0 && (ps.LocationId == fromlocation || ps.LocationId == tolocation)
                            orderby p.ProductCode
                            select new
                            {
                                p.ProductId,
                                p.ProductCode,
                                p.ProductName,
                                uom.UnitOfMeasureName
                            }
                            ).ToList().Distinct();

            List<ProductStockMasterViewModel> locationproducts = new List<ProductStockMasterViewModel>();
            foreach (var prd in products)
            {
                ProductStockMasterViewModel locationproduct = new ProductStockMasterViewModel();
                locationproduct.ProductId = prd.ProductId;
                locationproduct.ProductCode = prd.ProductCode;
                locationproduct.ProductName = prd.ProductName;
                locationproduct.UOM = prd.UnitOfMeasureName;
                locationproducts.Add(locationproduct);
            }

            return locationproducts == null ? null : locationproducts;

        }

        public List<ProductStockMasterViewModel> GetLocationMenuesByLocId(long fromlocation, long tolocation, int companyid)
        {
            var products = (from ps in _unitofwork.ProductStockMasterRepository.Get(ps => (ps.LocationId == fromlocation) && ps.CompanyID == companyid)
                            join p in _unitofwork.ProductRepository.Get(p => p.CompanyID == companyid) on ps.ProductId equals p.ProductId
                            join uom in _unitofwork.UnitOfMeasureRepository.Get() on p.PurchasingUnit equals uom.UnitOfMeasureId
                              where (ps.CostPrice > 0) && ps.IsActive == true && ps.IsDelete == false && p.IsDelete == false
                              && p.IsActive == true && ps.SellingPrice > 0  // == fromlocation || ps.LocationId == tolocation) 
                            // && p.CompanyID==companyid
                            // && ps.CompanyID==companyid
                            orderby p.ProductCode
                            select new
                            {
                                p.ProductId,
                                p.ProductCode,
                                p.ProductName,
                                uom.UnitOfMeasureName,
                                p.IsRowMaterial
                            }
                            ).ToList().Distinct();

            List<ProductStockMasterViewModel> locationproducts = new List<ProductStockMasterViewModel>();
            foreach (var prd in products)
            {
                ProductStockMasterViewModel locationproduct = new ProductStockMasterViewModel();
                locationproduct.ProductId = prd.ProductId;
                locationproduct.ProductCode = prd.ProductCode;
                locationproduct.ProductName = prd.ProductName;
                locationproduct.UOM = prd.UnitOfMeasureName;
                locationproduct.IsRowMaterial = prd.IsRowMaterial;
                locationproducts.Add(locationproduct);
            }

            return locationproducts == null ? null : locationproducts;

        }

        public List<ProductStockMasterViewModel> GetProductsStockByLocId(long stocklocationid, int companyid)
        {
            var products = (from ps in _unitofwork.ProductStockMasterRepository.Get(ps => (ps.LocationId == stocklocationid) && ps.CompanyID == companyid)
                            join p in _unitofwork.ProductRepository.Get(p => p.CompanyID == companyid) on ps.ProductId equals p.ProductId
                            join uom in _unitofwork.UnitOfMeasureRepository.Get(u => u.CompanyID == companyid) on p.PurchasingUnit equals uom.UnitOfMeasureId
                            // where (ps.LocationId == stocklocationid) && p.CompanyID==companyid 
                            orderby p.ProductCode
                            select new
                            {
                                p.ProductId,
                                p.ProductCode,
                                p.ProductName,
                                uom.UnitOfMeasureName,
                                ps.LocationId
                            }
                            ).ToList().Distinct();

            List<ProductStockMasterViewModel> locationproducts = new List<ProductStockMasterViewModel>();
            foreach (var prd in products)
            {
                ProductStockMasterViewModel locationproduct = new ProductStockMasterViewModel();
                locationproduct.ProductId = prd.ProductId;
                locationproduct.ProductCode = prd.ProductCode;
                locationproduct.ProductName = prd.ProductName;
                locationproduct.UOM = prd.UnitOfMeasureName;
                locationproduct.LocationId = prd.LocationId;
                locationproducts.Add(locationproduct);
            }
            locationproducts.OrderBy(x => x.ProductCode);
            return locationproducts == null ? null : locationproducts;

        }


        public int UpdateRecipeCostPrice(int productid, int companyid, int locationid)
        {
            var materil = _unitofwork.ProductStockMasterRepository.Get(r => r.ProductId == productid &&
                                                                        r.LocationId == locationid &&
                                                                        r.CompanyID == companyid
                                                                        ).FirstOrDefault();
            var wp = _unitofwork.ProductRepository.GetById(materil.ProductId).WeightPerUnit;
            if (wp != 0)
            {
                materil.SubUnitValue = _unitofwork.UnitConversionRepository.GetById(wp).SubUnitValue;
            }
            if (materil.SubUnitValue == 0)
                materil.SubUnitValue = 1;

            var recipelines = _unitofwork.ReceipeRepository.Get(r => r.MaterialId == productid
                                                                     && r.LocationId == locationid && r.CompanyID == companyid).ToList();
            foreach (var r in recipelines)
            {

                r.CostPrice = (materil.AvgCost / materil.SubUnitValue) * r.Quantity;

                _unitofwork.ReceipeRepository.Update(r);

                var recipe = _unitofwork.ReceipeRepository.Get(k => k.ProductId == r.ProductId).ToList();
                var servingunit = _unitofwork.ProductServingUnitRepository.GetById(r.ProductServingUnitId);
                servingunit.CostPrice = recipe.Sum(c => c.CostPrice);
                _unitofwork.ProductServingUnitRepository.Update(servingunit);

                _unitofwork.Save();
            }

            return recipelines.Count();
        }

        public List<KitchenMaster> GetKitchens(int companyid)
        {
            var kitchens = _unitofwork.KitchenMasterRepository.Get(k => k.CompanyID == companyid
                                                                    && k.IsActive == true).ToList();
            return kitchens ?? null;
        }
        public Product GetTargetPeriad(int companyID)
        {
            var product = _unitofwork.ProductRepository.Get(p => p.CompanyID == companyID).FirstOrDefault();
            return product ?? null;
        }

        //public List<KitchenMaster> GetPrinters(int LocationID)
        //{
        //    var kitchens = _unitofwork.KitchenMasterRepository.Get(k => k.LocationId == LocationID
        //                                                            && k.IsActive == true).ToList();
        //    return kitchens ?? null;
        //}

        public List<KitchenMaster> GetPrinters()
        {
            var kitchens = _unitofwork.KitchenMasterRepository.Get(k => k.IsActive == true).ToList();
            return kitchens ?? null;
        }

        public List<InvPriceLevel> GetPriceLevels()
        {
            var PriceLevels = _unitofwork.InvPriceLevels.Get(p => p.IsDelete == false).ToList();
            return PriceLevels ?? null;
        }
        public Product GetProductByCode(string code, int companyid)
        {
            var product = _unitofwork.ProductRepository.Get(p => p.CompanyID == companyid && p.ProductCode == code).FirstOrDefault();
            return product ?? null;
        }
        public Product GetProducCode(int companyid)
        {
            var product = _unitofwork.ProductRepository.Get(p => p.CompanyID == companyid).FirstOrDefault();
            return product ?? null;
        }
        public List<ServingUnitPricesViewModel> DownloadServingUnitPriceData(int companyid)
        {
            var products = (from p in _unitofwork.ProductRepository.GetAsNoTracking(p => p.CompanyID == companyid)
                            join ser in _unitofwork.ProductServingUnitRepository.GetAsNoTracking(ser => ser.CompanyID == companyid) on p.ProductId equals ser.ProductId
                            orderby p.ProductCode
                            select new
                            {
                                ser.LocationId,
                                ser.ProductId,
                                p.ProductName,
                                ser.ServingUnit,
                                ser.CostPrice,
                                ser.SellingPrice,
                                ser.ProductServingUnitId,
                            }).ToList();

            List<ServingUnitPricesViewModel> vmpServingUnitslist = new List<ServingUnitPricesViewModel>();
            foreach (var p in products)
            {
                ServingUnitPricesViewModel vmproduct = new ServingUnitPricesViewModel();
                vmproduct.LocationId = Convert.ToInt32(p.LocationId);
                vmproduct.ProductId = Convert.ToInt32(p.ProductId);
                vmproduct.ProductServingUnitId = p.ProductServingUnitId;
                vmproduct.ServingUnit = p.ServingUnit;
                vmproduct.CostPrice = p.CostPrice;
                vmproduct.SellingPrice = p.SellingPrice;
                vmproduct.ProductName = p.ProductName;
                vmpServingUnitslist.Add(vmproduct);
            }

            return vmpServingUnitslist == null ? null : vmpServingUnitslist;
        }
        public List<DataUploadProductViewModel> DownloadProductPriceData(int companyid)
        {
            var products = (from p in _unitofwork.ProductRepository.GetAsNoTracking(p => p.CompanyID == companyid)
                            join ser in _unitofwork.ProductServingUnitRepository.GetAsNoTracking(ser => ser.CompanyID == companyid) on p.ProductId equals ser.ProductId
                            orderby p.ProductCode
                            select new
                            {
                                ser.LocationId,
                                ser.ProductId,
                                p.ProductName,
                                ser.ServingUnit,
                                ser.CostPrice,
                                ser.SellingPrice,
                            }).ToList();

            List<DataUploadProductViewModel> vmproductlist = new List<DataUploadProductViewModel>();
            foreach (var p in products)
            {
                DataUploadProductViewModel vmproduct = new DataUploadProductViewModel();
                vmproduct.LocationCode = Convert.ToString(p.LocationId);
                vmproduct.ProductId = Convert.ToInt32(p.ProductId);
                vmproduct.ProductName = p.ProductName;
                vmproduct.ServingUnit = p.ServingUnit;
                vmproduct.CostPrice = p.CostPrice;
                vmproduct.SellingPrice = p.SellingPrice;
                vmproductlist.Add(vmproduct);
            }

            return vmproductlist == null ? null : vmproductlist;
        }
        public List<DataUploadProductViewModel> DownloadProductUploadData(int companyid)
        {
            var products = (from p in _unitofwork.ProductRepository.GetAsNoTracking(p => p.CompanyID == companyid)
                            join d in _unitofwork.DepartmentRepository.GetAsNoTracking(d => d.CompanyID == companyid) on p.DepartmentId equals d.RstDepartmentID
                            join c in _unitofwork.CategoryRepository.GetAsNoTracking(c => c.CompanyID == companyid) on p.CategoryId equals c.RstCategoryID
                            join s in _unitofwork.SubCategoryRepository.GetAsNoTracking(s => s.CompanyID == companyid) on p.SubCategoryId equals s.RstSubCategoryID
                            join pu in _unitofwork.UnitOfMeasureRepository.GetAsNoTracking(pu => pu.CompanyID == companyid) on p.PurchasingUnit equals pu.UnitOfMeasureId
                            join su in _unitofwork.UnitConversionRepository.GetAsNoTracking(su => su.CompanyID == companyid) on p.WeightPerUnit equals su.UnitConversionId
                            join sp in _unitofwork.SupplierProductRepository.GetAsNoTracking(sp => sp.CompanyID == companyid) on p.ProductId equals sp.ProductId
                            join sup in _unitofwork.SuplierRepository.GetAsNoTracking(sup => sup.CompanyID == companyid) on sp.SupplierId equals sup.SupplierID
                            join psm in _unitofwork.ProductStockMasterRepository.GetAsNoTracking(psm => psm.CompanyID == companyid) on p.ProductId equals psm.ProductId
                            join l in _unitofwork.LocationRepository.GetAsNoTracking(l => l.CompanyID == companyid) on psm.CostCentreId equals l.SysLocationID
                            join pr in _unitofwork.PrinterTypeRepository.GetAsNoTracking() on p.PrinterTypeId equals pr.PrinterTypeId
                            orderby p.ProductCode
                            select new
                            {
                                p.ProductId,
                                p.ProductCode,
                                p.ProductName,
                                p.NameOnInvoice,
                                p.IsRowMaterial,
                                p.IsScaleItem,
                                d.DepartmentCode,
                                c.RstCategoryCode,
                                s.RstSubCategoryCode,
                                pu.UnitOfMeasureCode,
                                su.SubUnit,
                                pr.PrinterTypeName,
                                p.IsDiscount,
                                p.IsCostOnReceipe,
                                p.IsAddon,
                                p.IsPromotion,
                                p.IsExpiry,
                                p.IsTax,
                                p.IsUnderCost,
                                p.IsTaxInclude,
                                p.IsOpenItem,
                                p.AutoProduction,
                                p.IsNoEffectCostforMenu,
                                sup.SupplierCode,
                                l.LocationCode,
                                psm.Stock,
                                psm.AvgCost,
                                psm.CostPrice,
                                psm.SellingPrice,
                                psm.ReOrderLevel,
                                psm.ReOrderQuantity, 
                                p.IsActive,
                                p.DepartmentId,
                                p.CategoryId,
                                p.SubCategoryId,
                                psm.LocationId,
                            }).ToList();

            List<DataUploadProductViewModel> vmproductlist = new List<DataUploadProductViewModel>();
            foreach (var p in products)
            {
                DataUploadProductViewModel vmproduct = new DataUploadProductViewModel();
                vmproduct.ProductId = p.ProductId;
                vmproduct.ProductCode = p.ProductCode;
                vmproduct.ProductName = p.ProductName;
                vmproduct.NameOnInvoice = p.NameOnInvoice;
                vmproduct.IsRawMaterial = p.IsRowMaterial;
                vmproduct.IsScaleItem = p.IsScaleItem;
                vmproduct.DepartmentCode = p.DepartmentCode;
                vmproduct.RstCategoryCode = p.RstCategoryCode;
                vmproduct.RstSubCategoryCode = p.RstSubCategoryCode;
                vmproduct.UnitOfMeasureCode = p.UnitOfMeasureCode;
                vmproduct.SubUnit = p.SubUnit;
                vmproduct.PrinterType = p.PrinterTypeName;
                vmproduct.IsDiscount = p.IsDiscount;
                vmproduct.IsCostOnReceipe = p.IsCostOnReceipe;
                vmproduct.IsAddon = p.IsAddon;
                vmproduct.IsPromotion = p.IsPromotion;
                vmproduct.IsExpiry = p.IsExpiry;
                vmproduct.IsTax = p.IsTax;
                vmproduct.IsUnderCost = p.IsUnderCost;
                vmproduct.IsTaxInclude = p.IsTaxInclude;
                vmproduct.IsOpenItem = p.IsOpenItem;
                vmproduct.AutoProduction = p.AutoProduction;
                vmproduct.IsNoEffectCostforMenu = p.IsNoEffectCostforMenu;
                vmproduct.SupplierCode = p.SupplierCode;
                vmproduct.LocationCode = p.LocationCode;
                vmproduct.Stock = p.Stock;
                vmproduct.AvgCost = p.AvgCost;
                vmproduct.CostPrice = p.CostPrice;
                vmproduct.SellingPrice = p.SellingPrice;
                vmproduct.ReOrderLevel = p.ReOrderLevel;
                vmproduct.ReOrderQuantity = p.ReOrderQuantity;
                vmproduct.LocationID = p.LocationId;
                vmproduct.DepartmentID = p.DepartmentId;
                vmproduct.CategoryID = p.CategoryId;
                vmproduct.SubCategoryID = p.SubCategoryId;


                vmproductlist.Add(vmproduct);

            }

             return vmproductlist == null ? null : vmproductlist;
        }

        public List<DataUploadProductViewModel> DownloadProductPriceChnageUploadData(int companyid)
        {
            var products = (from p in _unitofwork.ProductRepository.GetAsNoTracking(p => p.CompanyID == companyid)
                             
                            join psm in _unitofwork.ProductStockMasterRepository.GetAsNoTracking(psm => psm.CompanyID == companyid) on p.ProductId equals psm.ProductId
                            join s in _unitofwork.SupplierProductRepository.GetAsNoTracking(s => s.CompanyID == companyid) on psm.ProductId equals s.ProductId
                            join l in _unitofwork.LocationRepository.GetAsNoTracking(l => l.CompanyID == companyid) on psm.CostCentreId equals l.SysLocationID
                            join sup in _unitofwork.SuplierRepository.GetAsNoTracking(sup => sup.CompanyID == companyid) on s.SupplierId equals sup.SupplierID
                           where  s.LocationId == psm.LocationId
                            orderby p.ProductCode
                            select new
                            {
                                p.ProductId,
                                p.ProductCode,
                                p.ProductName, 
                                p.IsRowMaterial, 
                                psm.CostPrice,
                                psm.SellingPrice, 
                                p.IsActive,
                                p.DepartmentId,
                                p.CategoryId,
                                p.SubCategoryId,
                                psm.LocationId,
                                sup.SupplierCode
                            }).ToList();

            List<DataUploadProductViewModel> vmproductlist = new List<DataUploadProductViewModel>();
            foreach (var p in products)
            {
                DataUploadProductViewModel vmproduct = new DataUploadProductViewModel();
                vmproduct.ProductId = p.ProductId;
                vmproduct.ProductCode = p.ProductCode;
                vmproduct.ProductName = p.ProductName;
                vmproduct.IsActive = p.IsActive;
                vmproduct.IsRawMaterial = p.IsRowMaterial; 
                vmproduct.CostPrice = p.CostPrice;
                vmproduct.SellingPrice = p.SellingPrice; 
                vmproduct.LocationID = p.LocationId;
                vmproduct.DepartmentID = p.DepartmentId;
                vmproduct.CategoryID = p.CategoryId;
                vmproduct.SubCategoryID = p.SubCategoryId;
               vmproduct.SupplierCode = p.SupplierCode;

                vmproductlist.Add(vmproduct);

            }

            return vmproductlist == null ? null : vmproductlist;
        }

        public List<DataUploadProductViewModel> DownloadProductStockListUploadData(int companyid, int locationId)
        {
            var products = (from p in _unitofwork.ProductRepository.GetAsNoTracking(p => p.CompanyID == companyid)

                            join psm in _unitofwork.ProductStockMasterRepository.GetAsNoTracking(psm => psm.CompanyID == companyid) on p.ProductId equals psm.ProductId
                            where p.IsActive == true && p.LocationId == locationId && psm.LocationId == locationId && psm.IsActive == true
                            orderby p.ProductCode
                            select new
                            {
                                p.ProductId,
                                p.ProductCode,
                                p.ProductName,
                                //p.IsActive,
                                psm.Stock
                            }).ToList().Distinct();

            List<DataUploadProductViewModel> vmproductlist = new List<DataUploadProductViewModel>();
            foreach (var p in products)
            {
                DataUploadProductViewModel vmproduct = new DataUploadProductViewModel();
                vmproduct.ProductId = p.ProductId;
                vmproduct.ProductCode = p.ProductCode;
                vmproduct.ProductName = p.ProductName;
                //vmproduct.IsActive = p.IsActive;
                vmproduct.Stock = p.Stock;

                vmproductlist.Add(vmproduct);

            }

            return vmproductlist == null ? null : vmproductlist;
        }

        public Tuple<string, int> ProductExcelUpload(ProductUploadViewModel productuploadviewmodel)
        {
            var DepartmentIds = _unitofwork.DepartmentRepository.Get(d => d.CompanyID == productuploadviewmodel.CompanyId
                                                                        && d.IsActive == true).Select(d => d.RstDepartmentID).ToList();
            var CategoryIds = _unitofwork.CategoryRepository.Get(d => d.CompanyID == productuploadviewmodel.CompanyId
                                                                        && d.IsActive == true).Select(d => d.RstCategoryID).ToList();
            var SubCategoryIds = _unitofwork.SubCategoryRepository.Get(d => d.CompanyID == productuploadviewmodel.CompanyId
                                                                        && d.IsActive == true).Select(d => d.RstSubCategoryID).ToList();
            var Locations = _unitofwork.LocationRepository.Get(d => d.CompanyID == productuploadviewmodel.CompanyId
                                                                       && d.IsActive == true).Select(d => d.SysLocationID).ToList();

            var Suppliers = _unitofwork.SupplierProductRepository.Get(d => d.CompanyID == productuploadviewmodel.CompanyId
                                                                        ).Select(d => d.SupplierId).ToList();

            var Taxes = _unitofwork.TaxRepository.Get(d => d.CompanyID == productuploadviewmodel.CompanyId
                                                                     && d.IsActive == true).Select(d => d.TaxId).ToList();

            var ServingUnits = _unitofwork.ServingUnitRepository.Get(d => d.CompanyID == productuploadviewmodel.CompanyId
                                                                     && d.IsActive == true).Select(d => d.ServingUnitName).ToList();

            var PerchasingunitIds = _unitofwork.UnitOfMeasureRepository.Get(d => d.CompanyID == productuploadviewmodel.CompanyId).Select(d => d.UnitOfMeasureId).ToList();
            var WeightPerUnitIds = _unitofwork.UnitConversionRepository.Get(d => d.CompanyID == productuploadviewmodel.CompanyId).Select(d => d.UnitConversionId).ToList();


            _unitofwork.CreateTransaction();



            try
            {
                int which = 0;
                //Products
                var productlist = productuploadviewmodel.ProductList.GroupBy(m => m.ProductCode).Select(x => x.First()).ToList();
                var productstocklist = productuploadviewmodel.ProductStockMasterList.GroupBy(m => new { m.CostCentreId, m.ProductCode }).Select(x => x.First()).ToList();
                var supplierproductlist = productuploadviewmodel.SupplierProductList.GroupBy(s => new { s.ProductCode, s.SupplierCode }).Select(x => x.First()).ToList();

                var recipeuploaduniqlist = productuploadviewmodel.RecipeUploadList.GroupBy(r => new { r.LocationCode, r.ProductCode, r.ServingUnitName }).Select(r => r.First()).ToList();


                List<Product> distinctproductlist, productlisttosave = new List<Product>();
                distinctproductlist = productlist;
                List<ProductStockMaster> distinctstocklist = new List<ProductStockMaster>();
                distinctstocklist = productstocklist;
                List<SupplierProduct> distinctsuplierproductlist = new List<SupplierProduct>();
                distinctsuplierproductlist = supplierproductlist;
                foreach (var p in distinctproductlist)
                {
                    //var dbproduct = _unitofwork.ProductRepository.GetAsNoTracking(d=>d.CompanyID==p.CompanyID && d.ProductCode==p.ProductCode).FirstOrDefault();
                    //if (dbproduct != null)
                    //{
                    //    _unitofwork.Rollback();
                    //    return Tuple.Create("Product Code " + p.ProductCode + " is already exists.", 0);
                    //}

                    if (!DepartmentIds.Contains(p.DepartmentId))
                    {
                        _unitofwork.Rollback();
                        return Tuple.Create("Department Code " + Convert.ToString(p.DepartmnetCode) + " is not exists in system", 0);
                    }

                    if (!CategoryIds.Contains(p.CategoryId))
                    {
                        _unitofwork.Rollback();
                        return Tuple.Create("Category Code " + Convert.ToString(p.CategoryCode) + " is not exists in system", 0);
                    }

                    if (!SubCategoryIds.Contains(p.SubCategoryId))
                    {
                        _unitofwork.Rollback();
                        return Tuple.Create("Sub Category Code " + Convert.ToString(p.SubCategoryCode) + " is not exists in system", 0);
                    }

                    if (!PerchasingunitIds.Contains(p.PurchasingUnit))
                    {
                        _unitofwork.Rollback();
                        return Tuple.Create("Purchasing Unit Code " + Convert.ToString(p.UOMCode) + " is not exists in system", 0);
                    }
                    p.WeightPerUnit = 1;
                    p.ProductDesp = p.NameOnInvoice;
                    productlisttosave.Add(p);
                }
                if (productlisttosave.Count() != 0)
                {
                    foreach (var p1 in productlisttosave)
                    {
                        var dbproduct = _unitofwork.ProductRepository.Get(p => p.ProductCode == p1.ProductCode
                                                                                        && p.CompanyID == p1.CompanyID).FirstOrDefault();
                        if (dbproduct == null)
                        {
                            p1.WeightPerUnit =1;
                            p1.ProductDesp = p1.NameOnInvoice;
                            _unitofwork.ProductRepository.Insert(p1);
                        }
                        else
                        {
                            dbproduct.ProductName = p1.ProductName;
                            dbproduct.NameOnInvoice = p1.NameOnInvoice;
                            dbproduct.IsRowMaterial = p1.IsRowMaterial;
                            dbproduct.IsScaleItem = p1.IsScaleItem;
                            dbproduct.DepartmentId = p1.DepartmentId;
                            dbproduct.CategoryId = p1.CategoryId;
                            dbproduct.SubCategoryId = p1.SubCategoryId;
                            dbproduct.PurchasingUnit = p1.PurchasingUnit;
                            dbproduct.WeightPerUnit = p1.WeightPerUnit;
                            dbproduct.PrinterTypeId = p1.PrinterTypeId;
                            dbproduct.IsDiscount = p1.IsDiscount;
                            dbproduct.IsCostOnReceipe = p1.IsCostOnReceipe;
                            dbproduct.IsAddon = p1.IsAddon;
                            dbproduct.IsPromotion = p1.IsPromotion;
                            dbproduct.IsExpiry = p1.IsExpiry;
                            dbproduct.IsTax = p1.IsTax;
                            dbproduct.IsUnderCost = p1.IsUnderCost;
                            dbproduct.IsTaxInclude = p1.IsTaxInclude;
                            dbproduct.IsOpenItem = p1.IsOpenItem;
                            dbproduct.AutoProduction = p1.AutoProduction;
                            dbproduct.IsNoEffectCostforMenu = p1.IsNoEffectCostforMenu;
                            dbproduct.ModifiedDate = DateTime.Now;
                            dbproduct.ModifiedUser = p1.ModifiedUser;
                            dbproduct.ProductDesp = p1.NameOnInvoice;
                            if (p1.WeightPerUnit == null)
                            {
                                dbproduct.WeightPerUnit = 1;
                            }

                            _unitofwork.ProductRepository.Update(dbproduct);
                        }

                        //  _unitofwork.ProductRepository.BulkInsert(productlisttosave);
                    }
                    which = _unitofwork.Save();
                }
                //end products

                // stock master
                List<ProductStockMaster> stocklisttosave = new List<ProductStockMaster>();
                foreach (var s in distinctstocklist)
                {
                    if (!Locations.Contains(s.LocationId))
                    {
                        _unitofwork.Rollback();
                        return Tuple.Create("Location Code " + Convert.ToString(s.LocationCode) + " is not exists in system", 0);
                    }

                    s.ProductId = _unitofwork.ProductRepository.GetAsNoTracking(p => p.ProductCode == s.ProductCode
                                                                                  && p.CompanyID == s.CompanyID).FirstOrDefault().ProductId;

                    stocklisttosave.Add(s);

                }
                if (stocklisttosave.Count != 0)
                {
                    foreach (var s in stocklisttosave)
                    {
                        var dbstock = _unitofwork.ProductStockMasterRepository.Get(st => st.CompanyID == s.CompanyID && st.ProductId == s.ProductId
                                                                                   && st.LocationId == s.LocationId).FirstOrDefault();
                        if (dbstock != null)
                        {
                            dbstock.Stock = s.Stock;
                            dbstock.ReOrderLevel = s.ReOrderLevel;
                            dbstock.ReOrderPeriod = s.ReOrderPeriod;
                            dbstock.ReOrderQuantity = s.ReOrderQuantity;
                            dbstock.ModifiedDate = DateTime.Now;
                            dbstock.ModifiedUser = s.ModifiedUser;
                            _unitofwork.ProductStockMasterRepository.Update(dbstock);
                        }
                        else
                        {
                            _unitofwork.ProductStockMasterRepository.Insert(s);
                        }

                    }
                    // _unitofwork.ProductStockMasterRepository.BulkInsert(stocklisttosave);
                    which = _unitofwork.Save();
                }
                //end stock master

                // suppler products
                List<SupplierProduct> supplierproducttosave = new List<SupplierProduct>();
                foreach (var s in distinctsuplierproductlist)
                {
                    if (!Suppliers.Contains(s.SupplierId))
                    {
                        _unitofwork.Rollback();
                        return Tuple.Create("Supplier Code " + Convert.ToString(s.SupplierCode) + " is not exists in system", 0);
                    }

                    s.ProductId = _unitofwork.ProductRepository.GetAsNoTracking(p => p.ProductCode == s.ProductCode
                                                                                  && p.CompanyID == s.CompanyID).FirstOrDefault().ProductId;
                    supplierproducttosave.Add(s);

                }
                if (supplierproducttosave.Count != 0)
                {
                    foreach (var sprd in supplierproducttosave)
                    {
                        var dbsupprd = _unitofwork.SupplierProductRepository.Get(sp => sp.CompanyID == sprd.CompanyID
                                                                                    && sp.SupplierId == sprd.SupplierId
                                                                                    && sp.ProductId == sprd.ProductId).FirstOrDefault();

                        if (dbsupprd != null)
                        {
                            dbsupprd.ProductId = sprd.ProductId;
                            dbsupprd.SupplierId = sprd.SupplierId;
                            dbsupprd.ModifiedDate = DateTime.Now;
                            dbsupprd.ModifiedUser = sprd.ModifiedUser;
                            _unitofwork.SupplierProductRepository.Update(dbsupprd);

                        }
                        else
                        {
                            _unitofwork.SupplierProductRepository.Insert(sprd);
                        }

                        //  _unitofwork.SupplierProductRepository.BulkInsert(supplierproducttosave);

                    }


                    which = _unitofwork.Save();
                }

                //end supplier products

                // Product Taxes
                List<ProductTax> producttaxtosave = new List<ProductTax>();
                foreach (var s in productuploadviewmodel.ProductTaxList)
                {
                    if (!Taxes.Contains(Convert.ToInt32(s.TaxId)))
                    {
                        _unitofwork.Rollback();
                        return Tuple.Create("Tax Code " + Convert.ToString(s.TaxCode) + " is not exists in system", 0);
                    }

                    s.ProductId = _unitofwork.ProductRepository.GetAsNoTracking(p => p.ProductCode == s.ProductCode
                                                                                  && p.CompanyID == s.CompanyID).FirstOrDefault().ProductId;
                    var existspt = _unitofwork.ProductTaxRepository.GetAsNoTracking(pt => pt.CompanyID == s.CompanyID && pt.TaxId == s.TaxId
                                                                                   && pt.ProductId == s.ProductId).FirstOrDefault();
                    if (existspt == null)
                    {
                        producttaxtosave.Add(s);
                    }

                }
                if (producttaxtosave.Count != 0)
                {
                    _unitofwork.ProductTaxRepository.BulkInsert(producttaxtosave);
                    which = _unitofwork.Save();
                }

                //end Product Taxes

                // Recipe Prices
                List<ReceipeViewModel> recipepricetosave = new List<ReceipeViewModel>();
                ProductServingUnit dbproductservingunit = null;

                foreach (var s in productuploadviewmodel.RecipeList)
                {
                    if (!ServingUnits.Contains(s.ServingUnitName))
                    {
                        _unitofwork.Rollback();
                        return Tuple.Create("Serving Unit " + Convert.ToString(s.ServingUnitName) + " is not exists in system", 0);
                    }
                    if (!Locations.Contains(s.LocationId))
                    {
                        _unitofwork.Rollback();
                        return Tuple.Create("Location " + Convert.ToString(s.LocationId) + " is not exists in system", 0);
                    }


                    dbproductservingunit = new ProductServingUnit();
                    var productid = _unitofwork.ProductRepository.Get(p => p.ProductCode == s.ProductCode && p.CompanyID == s.CompanyId).FirstOrDefault().ProductId;
                    dbproductservingunit = _unitofwork.ProductServingUnitRepository.Get(r => r.ProductId == productid &&
                                                                                  r.ServingUnit == s.ServingUnitName &&
                                                                                  r.CompanyID == s.CompanyId && r.LocationId == s.LocationId).FirstOrDefault();
                    if (dbproductservingunit == null)
                    {

                        _unitofwork.Rollback();
                        return Tuple.Create("No Recipe for Prduct: " + s.ProductCode + " and Serving Unit: " + s.ServingUnitName + " in Location: " + s.LocationCode, 0);
                    }
                    else
                    {
                        s.ProductId = _unitofwork.ProductRepository.GetAsNoTracking(p => p.ProductCode == s.ProductCode
                                                                                      && p.CompanyID == s.CompanyId).FirstOrDefault().ProductId;

                        which = productuploadviewmodel.RecipeList.Count();
                        dbproductservingunit.SellingPrice = s.SellingPrice;
                        dbproductservingunit.CostPrice = s.CostPrice;
                        dbproductservingunit.ModifiedDate = s.CreateDate;
                        dbproductservingunit.ModifiedUser = s.CreatedUser;
                    }

                    if (dbproductservingunit != null)
                    {
                        _unitofwork.ProductServingUnitRepository.Update(dbproductservingunit);
                        which = _unitofwork.Save();
                    }

                }

                //end End Recipe Prices

                // starts recipe bulk
                foreach (var ur in recipeuploaduniqlist)
                {
                    var recipeproduct = _unitofwork.ProductRepository.GetAsNoTracking(p => p.CompanyID == ur.CompanyId
                                                                                           && p.ProductCode == ur.ProductCode).FirstOrDefault();

                    var dbrecipe = (from r in _unitofwork.ReceipeRepository.Get(r => r.CompanyID == ur.CompanyId && r.ProductId == recipeproduct.ProductId
                                                                  && r.ProductQty == ur.RecipeQuantity
                                                                   && r.LocationId == ur.LocationId)
                                    join psu in _unitofwork.ProductServingUnitRepository.Get(psu => psu.CompanyID == ur.CompanyId
                                                                                             && psu.LocationId == ur.LocationId
                                                                                             && psu.ServingUnit == ur.ServingUnitName)
                                    on new { r.ProductId, r.ProductServingUnitId } equals new { psu.ProductId, psu.ProductServingUnitId }
                                    select new
                                    {
                                        r.ProductId,
                                        r.ProductServingUnitId,
                                        r.ProductQty,
                                        psu.ServingUnit,
                                        psu.CostPrice,
                                        psu.SellingPrice
                                    }

                                   ).ToList();
                    if (dbrecipe.Count != 0)
                    {

                        Int64 productid = dbrecipe.First().ProductId;
                        Int64 productservingunitid = dbrecipe.First().ProductServingUnitId;

                        _unitofwork.ReceipeRepository.DeleteRange(_unitofwork.ReceipeRepository.Get(d => d.CompanyID == ur.CompanyId
                                                        && d.LocationId == ur.LocationId && d.ProductId == productid
                                                        && d.ProductServingUnitId == productservingunitid
                                                        && d.ProductQty == ur.RecipeQuantity));

                        List<Receipe> newrecipelist = new List<Receipe>();
                        foreach (var nr in productuploadviewmodel.RecipeUploadList.Where(r => r.ProductCode == ur.ProductCode
                                    && r.ServingUnitName == dbrecipe.First().ServingUnit && r.LocationId == ur.LocationId &&
                                      r.CompanyId == ur.CompanyId && r.RecipeQuantity == dbrecipe.First().ProductQty))
                        {
                            Receipe newrecipe = new Receipe();
                            newrecipe.ProductId = recipeproduct.ProductId;
                            newrecipe.Quantity = nr.Quantity;
                            newrecipe.GroupOfCompanyID = 1;
                            newrecipe.CompanyID = nr.CompanyId;
                            newrecipe.LocationId = nr.LocationId;
                            newrecipe.CreatedUser = nr.CreatedUser;
                            newrecipe.CreatedDate = DateTime.Now;
                            newrecipe.DataTransfer = 0;
                            var material = _unitofwork.ProductRepository.Get(m => m.ProductCode == nr.MaterialCode
                                                                                && m.CompanyID == nr.CompanyId).FirstOrDefault();
                            newrecipe.MaterialId = material.ProductId;
                            newrecipe.ProductServingUnitId = dbrecipe.First().ProductServingUnitId;
                            newrecipe.CostPrice = GetReceipeDetails(nr.LocationId, material.ProductId, nr.Quantity, 1, nr.CompanyId).CostPrice;
                            newrecipe.ProductQty = nr.RecipeQuantity;
                            newrecipelist.Add(newrecipe);

                        }

                        var dbservingunit = _unitofwork.ProductServingUnitRepository.GetById(dbrecipe.First().ProductServingUnitId);
                        dbservingunit.CostPrice = newrecipelist.Sum(s => s.CostPrice);
                        dbservingunit.SellingPrice = ur.SellingPrice;
                        dbservingunit.ModifiedUser = ur.CreatedUser;
                        dbservingunit.ModifiedDate = DateTime.Now;
                        _unitofwork.ReceipeRepository.BulkInsert(newrecipelist);
                    }
                    else
                    {

                        Int64 productid = recipeproduct.ProductId;
                        //  Int64 productservingunitid = dbrecipe.First().ProductServingUnitId;

                        //_unitofwork.ReceipeRepository.DeleteRange(_unitofwork.ReceipeRepository.Get(d => d.CompanyID == ur.CompanyId
                        //                                && d.LocationId == ur.LocationId && d.ProductId == productid
                        //                                && d.ProductServingUnitId == productservingunitid
                        //                                && d.ProductQty == ur.RecipeQuantity));

                        ProductServingUnit newproductservingunit = new ProductServingUnit();
                        newproductservingunit.ProductId = productid;
                        newproductservingunit.ServingUnit = ur.ServingUnitName;
                        newproductservingunit.CostPrice = 0;
                        newproductservingunit.SellingPrice = ur.SellingPrice;
                        newproductservingunit.ModifiedUser = ur.CreatedUser;
                        newproductservingunit.ModifiedDate = DateTime.Now;
                        newproductservingunit.LocationId = ur.LocationId;
                        newproductservingunit.CompanyID = ur.CompanyId;

                        _unitofwork.ProductServingUnitRepository.Insert(newproductservingunit);
                        _unitofwork.Save();

                        List<Receipe> newrecipelist2 = new List<Receipe>();
                        foreach (var nr in productuploadviewmodel.RecipeUploadList.Where(r => r.ProductCode == ur.ProductCode
                                    && r.ServingUnitName == ur.ServingUnitName && r.LocationId == ur.LocationId &&
                                      r.CompanyId == ur.CompanyId && r.RecipeQuantity == ur.RecipeQuantity))
                        {
                            Receipe newrecipe = new Receipe();
                            newrecipe.ProductId = recipeproduct.ProductId;
                            newrecipe.Quantity = nr.Quantity;
                            newrecipe.GroupOfCompanyID = 1;
                            newrecipe.CompanyID = nr.CompanyId;
                            newrecipe.LocationId = nr.LocationId;
                            newrecipe.CreatedUser = nr.CreatedUser;
                            newrecipe.CreatedDate = DateTime.Now;
                            newrecipe.DataTransfer = 0;
                            var material = _unitofwork.ProductRepository.Get(m => m.ProductCode == nr.MaterialCode
                                                                                && m.CompanyID == nr.CompanyId).FirstOrDefault();
                            newrecipe.MaterialId = material.ProductId;
                            newrecipe.ProductServingUnitId = newproductservingunit.ProductServingUnitId;
                            newrecipe.CostPrice = GetReceipeDetails(nr.LocationId, material.ProductId, nr.Quantity, 1, nr.CompanyId).CostPrice;
                            newrecipe.ProductQty = nr.RecipeQuantity;
                            newrecipelist2.Add(newrecipe);

                        }
                        _unitofwork.ReceipeRepository.BulkInsert(newrecipelist2);
                        var dbservingunit = _unitofwork.ProductServingUnitRepository.GetById(newproductservingunit.ProductServingUnitId);
                        dbservingunit.CostPrice = newrecipelist2.Sum(s => s.CostPrice);
                        dbservingunit.SellingPrice = ur.SellingPrice;
                        dbservingunit.ModifiedUser = ur.CreatedUser;
                        dbservingunit.ModifiedDate = DateTime.Now;


                    }
                    _unitofwork.Save();

                }
                which = productuploadviewmodel.RecipeUploadList.Count();
                _unitofwork.Commit();
                return Tuple.Create(Convert.ToString(which) + " Record(s) updated.", 1);


            }
            catch (DbEntityValidationException ex)
            {
                var errorMessages = ex.EntityValidationErrors
                    .SelectMany(x => x.ValidationErrors)
                    .Select(x => $"Property: {x.PropertyName} Error: {x.ErrorMessage}");

                string fullErrorMessage = string.Join(" | ", errorMessages);

                _unitofwork.Rollback();

                return Tuple.Create(fullErrorMessage, 0);
            }
            catch (DbUpdateException ex)
            {
                _unitofwork.Rollback();

                return Tuple.Create("Database Update Error : " + ex.InnerException?.InnerException?.Message, 0);
            }
            catch (SqlException ex)
            {
                _unitofwork.Rollback();

                return Tuple.Create("SQL Error : " + ex.Message, 0);
            }
            catch (Exception ex)
            {
                _unitofwork.Rollback();

                return Tuple.Create(
                    ex.Message + " " + ex.InnerException?.Message,
                    0
                );
            }

        }

        public Tuple<string, int> StockExcelUpload(List<ProductStockMaster> excelstock, Int32 companyid)
        {
            var LocationIds = _unitofwork.LocationRepository.Get(d => d.CompanyID == companyid
                                                                        && d.IsActive == true).Select(d => d.SysLocationID).ToList();

            var ProductIds = _unitofwork.ProductRepository.Get(d => d.CompanyID == companyid
                                                                        && d.IsActive == true).Select(d => d.ProductId).ToList();

            _unitofwork.CreateTransaction();
            List<ProductStockMaster> stocklist = new List<ProductStockMaster>();

            try
            {
                foreach (var p in excelstock)
                {
                    if (!LocationIds.Contains(p.LocationId))
                    {
                        return Tuple.Create("Location Code " + Convert.ToString(p.LocationId) + " is not exists in system", 0);
                    }

                    if (!ProductIds.Contains(Convert.ToInt32(p.ProductId)))
                    {
                        return Tuple.Create("Product Code " + Convert.ToString(p.ProductCode) + " is not exists in system", 0);
                    }
                    var prd = _unitofwork.ProductRepository.GetById(p.ProductId);
                    p.ProductCode = prd.ProductCode;
                    p.StockCode = prd.ProductCode;
                    p.ProductName = prd.ProductName;
                    p.IsDelete = false;
                    p.Barcode = prd.Barcode;
                    p.RefNo1 = prd.RefCode01;
                    p.RefNo2 = prd.RefCode02;
                    stocklist.Add(p);
                }



                _unitofwork.ProductStockMasterRepository.BulkInsert(stocklist);
                _unitofwork.Save();
                _unitofwork.Commit();
                return Tuple.Create(Convert.ToString(stocklist.Count) + " Record(s) uploaded.", 1);

            }
            catch (Exception e)
            {
                _unitofwork.Rollback();
                return Tuple.Create(e.Message, 0);

            }

        }
        public Tuple<string, int> UpdateProductPriceChanges(DataTable excelStock, int companyId)
        {
            int recordsUpdated = 0;
            int prductrecordUpdate = 0;
            int recordsInserted = 0;
            int stockrecordsInserted = 0;
            int supplierproductinserted = 0;
            int supplierproductupdate = 0;
            try
            {
                BLL_Location BLL_LocationService = new BLL_Location();
                var productsToUpdate = new List<ProductStockMaster>();
                var productMasterUpdate = new List<Product>();
                var productsToInsert = new List<Product>();
                var productsToInsertStock = new List<ProductStockMaster>();
                var supplierproducts = new List<SupplierProduct>();
                var supplierproductsupdate = new List<SupplierProduct>();
                var loc = _blllocation.GetAllActiveLocations();

                foreach (DataRow row in excelStock.Rows)
                {
                    int locationId = Convert.ToInt32(row["LocationID"]);
                    int productId = Convert.ToInt32(row["ProductID"]);
                    decimal costPrice = Convert.ToDecimal(row["CostPrice"]);
                    decimal sellingPrice = Convert.ToDecimal(row["SellingPrice"]);
                    string productCode = row["ProductCode"].ToString();
                    string productName = row["ProductName"].ToString();
                    bool isRowMaterial = Convert.ToBoolean(row["IsRawMaterial"]);
                    bool isActive = Convert.ToBoolean(row["IsActive"]);
                    int departmentId = Convert.ToInt32(row["DepartmentID"]);
                    int categoryId = Convert.ToInt32(row["CategoryID"]);
                    int subCategoryId = Convert.ToInt32(row["SubCategoryID"]);
                    string SupplierCode =  row["SupplierCode"].ToString();
                    // Validate locationId before proceeding
                    if (locationId != 0)
                    {
                        var existingProductStock = _unitofwork.ProductStockMasterRepository
                            .Get(d => d.LocationId == locationId && d.ProductId == productId)
                            .FirstOrDefault();

                        var existingProductMasterupdate = _unitofwork.ProductRepository
                            .Get(p => p.ProductId == productId )
                            .FirstOrDefault();

                        var  suppliers = _unitofwork.SuplierRepository
                    .Get(sup => sup.SupplierCode == SupplierCode)
                    .FirstOrDefault();

                        var existingproductsuppliers = _unitofwork.SupplierProductRepository
                     .Get(sup => sup.ProductId == productId)
                     .FirstOrDefault();

                        if (existingProductMasterupdate != null)
                        {
                            if(existingProductMasterupdate.ProductName != productName ||
                               existingProductMasterupdate.IsRowMaterial != isRowMaterial ||
                               existingProductMasterupdate.IsActive != isActive ||
                               existingProductMasterupdate.DepartmentId != departmentId ||
                               existingProductMasterupdate.CategoryId != categoryId ||
                               existingProductMasterupdate.SubCategoryId != subCategoryId )
                            {
                                existingProductMasterupdate.ProductName = productName;
                                existingProductMasterupdate.IsRowMaterial = isRowMaterial;
                                existingProductMasterupdate.IsActive = isActive;
                                existingProductMasterupdate.DepartmentId = departmentId;
                                existingProductMasterupdate.CategoryId = categoryId;
                                existingProductMasterupdate.SubCategoryId = subCategoryId;
                                existingProductMasterupdate.ModifiedDate = DateTime.Now;


                                productMasterUpdate.Add(existingProductMasterupdate);
                                prductrecordUpdate++;
                            }
                        }

                        if(existingproductsuppliers != null)
                        {
                            if(existingproductsuppliers.SupplierId != suppliers.SupplierID)
                            {
                                existingproductsuppliers.SupplierId = Convert.ToInt32(suppliers.SupplierID);
                                existingproductsuppliers.ModifiedDate = DateTime.Now;
                                supplierproductsupdate.Add(existingproductsuppliers);
                                supplierproductupdate++;
                            }
                        }
                        // If the product exists in ProductStockMaster, update it
                        if (existingProductStock != null)
                        {
                            if (existingProductStock.CostPrice != costPrice ||
                                existingProductStock.SellingPrice != sellingPrice ||
                                existingProductStock.ProductCode != productCode ||
                                existingProductStock.ProductName != productName)
                            {
                                existingProductStock.CostPrice = costPrice;
                                existingProductStock.SellingPrice = sellingPrice;
                                existingProductStock.ProductCode = productCode;
                                existingProductStock.ProductName = productName;
                                existingProductStock.ModifiedDate = DateTime.Now;

                                productsToUpdate.Add(existingProductStock);
                                recordsUpdated++;
                            }
                        }
                        else
                        {
                            // Check if the product exists in the Product table
                            var existingProduct = _unitofwork.ProductRepository
                                .Get(d => d.ProductCode == productCode)
                                .FirstOrDefault();

                            var existingProductMaster = _unitofwork.ProductStockMasterRepository
                                .Get(p => p.ProductCode == productCode)
                                .FirstOrDefault();


                            if (existingProduct == null)
                            {
                                // Create a new product and add it to the insert list
                                var newProduct = new Product
                                {
                                    ProductCode = productCode,
                                    ProductName = productName,
                                    ProductNameInSinhala = string.Empty,
                                    IsRowMaterial = isRowMaterial,
                                    IsCountable = false,
                                    IsScaleItem = false,
                                    IsActive = isActive,
                                    IsDelete = false,
                                    ProductImage = null,
                                    ProductImageName = string.Empty,
                                    ProductImageType = string.Empty,
                                    DepartmentId = departmentId,
                                    CategoryId = categoryId,
                                    SubCategoryId = subCategoryId,
                                    CostPrice = costPrice,
                                    SellingPrice = sellingPrice,
                                    ReOrderLevel = 0,
                                    ReOrderQuantity = 0,
                                    LocationWiseStock = 0,
                                    Printer = string.Empty,
                                    Barcode = string.Empty,
                                    IsItemLock = false,
                                    GroupOfCompanyID = 1,
                                    CompanyID = 1,
                                    LocationId = locationId,
                                    CreatedUser = "ADMIN",
                                    CreatedDate = DateTime.Now,
                                    ModifiedUser = "ADMIN",
                                    ModifiedDate = DateTime.Now,
                                    DataTransfer = 0,
                                    RefCode01 = string.Empty,
                                    RefCode02 = string.Empty,
                                    WastagePrc = 0,
                                    PurchasingUnit = 1,
                                    IsDiscount = false,
                                    IsCostOnReceipe = false,
                                    IsAddon = false,
                                    NameOnInvoice = string.Empty,
                                    IsPackItem = false,
                                    PackSize = 0,
                                    PackPrice = 0,
                                    IsPromotion = false,
                                    IsFreeIssue = false,
                                    IsExpiry = false,
                                    IsTax = false,
                                    WeightPerUnit = 1,
                                    IsUnderCost = false,
                                    IsBundle = false,
                                    MaxPrice = 0,
                                    MinPrice = 0,
                                    DiscountPrecentage = 0,
                                    MaximumDiscount = 0,
                                    FixedDiscountPercentage = 0,
                                    MaximumDiscountPercentage = 0,
                                    PrinterTypeId = 1,
                                    AddonCategoryMasterId = 0,
                                    IsTaxInclude = false,
                                    IsOpenItem = false,
                                    AutoProduction = false,
                                    IsNoEffectCostforMenu = false,
                                    KitchenCode = string.Empty,
                                    ProductDesp = productName,
                                    ImagePath = string.Empty,
                                    TypeIdTargetType = string.Empty,
                                    Target_Qty = 0,
                                    TypeIdTargetPeriod = string.Empty
                                };

                                productsToInsert.Add(newProduct);
                                recordsInserted++;

                                var existingsuppliers = _unitofwork.SuplierRepository
                             .Get(sup => sup.SupplierCode == SupplierCode)
                             .FirstOrDefault();

                                if (existingsuppliers != null)
                                {
                                    var newsupplierProduct = new SupplierProduct
                                    {
                                        SupplierId = Convert.ToInt32(existingsuppliers.SupplierID),
                                        ProductId = 0,

                                        GroupOfCompanyID = 1,

                                        CompanyID = 1,

                                        LocationId = 1,

                                        CreatedUser = "ADMIN",
                                        CreatedDate = DateTime.Now,
                                        ModifiedUser = "ADMIN",
                                        ModifiedDate = DateTime.Now,

                                        DataTransfer = 0,

                                        IsPreferredSupplier = false,

                                        LastCostPrice = 0,

                                        CostPrice = 0,
                                        SellingPrice = 0,

                                        IsDelete = false
                                    };

                                    supplierproducts.Add(newsupplierProduct);
                                    supplierproductinserted++;

                                }
                                else
                                {
                                    
                                    return Tuple.Create(" '"  + SupplierCode + "' ",2);

                                }
                            }
                           else
                            {
                                return Tuple.Create(" '" + productCode + "' ", 3);
                            }



                            if(existingProductMaster == null)
                            {
                                // Create a new product and add it to the insert list
                                var newProductStockMaster = new ProductStockMaster
                                {
                                                        ProductId = 0,
                                                        StockCode = productCode,
                                                        Stock = 0,
                                                        CostPrice = costPrice,
                                                        SellingPrice = sellingPrice,
                                                        ReOrderLevel = 0,
                                                        ReOrderQuantity = 0,
                                                        ReOrderPeriod = 0,
                                                        IsDelete = false,
                                                        ProductCode = productCode,
                                                        ProductName = productName,
                                                        Barcode = string.Empty,
                                                        RefNo1 = string.Empty,
                                                        RefNo2 = string.Empty,
                                                        ExtendedId = 0,
                                                        ExtendedName = "1",
                                                        PLUCode = "1",
                                                        WeightPerunit = 1,
                                                        UomId = 0,
                                                        Unit = "1",
                                                        AvgCost =  0,
                                                        FixedGP = 0,
                                                        GP = 0,
                                                        OpenBal = 0,
                                                        InitSIH = 0,
                                                        InitCost = 0,
                                                        AdjQty = 0,
                                                        IsDamage = false,
                                                        IsActive = false,
                                                        IsBundle = false,
                                                        IsInitialize = false,
                                                        DataTransfer = 0,
                                                        Ispacksize = false,
                                                        Iscommission = false,
                                                        Isdecimal = false,
                                                        GroupOfCompanyID = 1,
                                                        CompanyID = 1,
                                                        LocationId = 0,
                                                        CreatedUser = "ADMIN",
                                                        CreatedDate = DateTime.Now,
                                                        ModifiedUser = "ADMIN",
                                                        ModifiedDate = DateTime.Now,
                                                        DiscountPrc = 0,
                                                        DocumentNo = "",
                                                        LastUpdatedDate  = DateTime.Now,
                                                        ForignCustomerPrice = 0,
                                                        MaximumDiscount = 0,
                                                        FixedDiscountPercentage = 0,
                                                        FixedDiscountAmount = 0,
                                                        MaximumDiscountPercentage = 0,
                                                        PrinterType_Id = 0

                                              
                            };
                                productsToInsertStock.Add(newProductStockMaster);
                                stockrecordsInserted++;
                            }

                        }
                    }
                }

                // Insert new products
                if (productsToInsert.Count > 0)
                {
                    foreach (var product in productsToInsert)
                    {
                        _unitofwork.ProductRepository.Insert(product);
                    }
                    _unitofwork.Save();
                }

                if(supplierproducts.Count > 0)
                {
                    foreach (var supplierproduts in supplierproducts)
                    {
                        foreach (var product in productsToInsert)
                        {
                            var newsupplierproducts = new SupplierProduct
                            {
                                SupplierId = supplierproduts.SupplierId,

                                ProductId = Convert.ToInt32(product.ProductId),

                                GroupOfCompanyID = supplierproduts.GroupOfCompanyID,

                                CompanyID = supplierproduts.CompanyID,

                                LocationId = supplierproduts.LocationId,

                                CreatedUser = supplierproduts.CreatedUser,
                                CreatedDate = supplierproduts.CreatedDate,
                                ModifiedUser = supplierproduts.ModifiedUser,
                                ModifiedDate = supplierproduts.ModifiedDate,

                                DataTransfer = supplierproduts.DataTransfer,

                                IsPreferredSupplier = supplierproduts.IsPreferredSupplier,

                                LastCostPrice = supplierproduts.LastCostPrice,

                                CostPrice = supplierproduts.CostPrice,
                                SellingPrice = supplierproduts.SellingPrice,

                                IsDelete = supplierproduts.IsDelete
                            };

                            _unitofwork.SupplierProductRepository.Insert(newsupplierproducts);
                        }
                    }

                    _unitofwork.Save();
                }
                // Insert new stock master products
                if (productsToInsertStock.Count > 0)
                {
                    foreach (var product in productsToInsertStock)
                    {
                        foreach (var location in loc)
                        {
                            foreach (var productToInsert in productsToInsert)
                            {
                                // Create a copy or new instance of the product for each location and product ID combination
                                var newProductStock = new ProductStockMaster
                                {
                                    ProductId = productToInsert.ProductId,
                                    StockCode = product.ProductCode,
                                    Stock = 0,
                                    CostPrice = product.CostPrice,
                                    SellingPrice = product.SellingPrice,
                                    ReOrderLevel = 0,
                                    ReOrderQuantity = 0,
                                    ReOrderPeriod = 0,
                                    IsDelete = false,
                                    ProductCode = product.ProductCode,
                                    ProductName = product.ProductName,
                                    Barcode = string.Empty,
                                    RefNo1 = string.Empty,
                                    RefNo2 = string.Empty,
                                    ExtendedId = 0,
                                    ExtendedName = "1",
                                    PLUCode = "1",
                                    WeightPerunit = 1,
                                    UomId = 0,
                                    Unit = "1",
                                    AvgCost = 0,
                                    FixedGP = 0,
                                    GP = 0,
                                    OpenBal = 0,
                                    InitSIH = 0,
                                    InitCost = 0,
                                    AdjQty = 0,
                                    IsDamage = false,
                                    IsActive = false,
                                    IsBundle = false,
                                    IsInitialize = false,
                                    DataTransfer = 0,
                                    Ispacksize = false,
                                    Iscommission = false,
                                    Isdecimal = false,
                                    GroupOfCompanyID = 1,
                                    CompanyID = 1,
                                    LocationId = location.SysLocationID,
                                    CreatedUser =  "ADMIN",
                                    CreatedDate = DateTime.Now,
                                    ModifiedUser = "ADMIN",
                                    ModifiedDate = DateTime.Now,
                                    DiscountPrc = 0,
                                    DocumentNo = "",
                                    LastUpdatedDate = DateTime.Now,
                                    ForignCustomerPrice = 0,
                                    MaximumDiscount = 0,
                                    FixedDiscountPercentage = 0,
                                    FixedDiscountAmount = 0,
                                    MaximumDiscountPercentage = 0,
                                    PrinterType_Id = 0,
                                     
                                    // Copy other necessary properties from the original product
                                };

                                _unitofwork.ProductStockMasterRepository.Insert(newProductStock);
                            }
                        }
                    }
                    _unitofwork.Save();
                }

                // Update existing products
                if (productMasterUpdate.Count > 0)
                {
                    foreach (var product in productMasterUpdate)
                    {
                        _unitofwork.ProductRepository.Update(product);
                    }
                    _unitofwork.Save();
                }

                // Update existing supplierproduct
                if (supplierproductsupdate.Count > 0)
                {
                    foreach (var products in supplierproductsupdate)
                    {
                        _unitofwork.SupplierProductRepository.Update(products);
                    }
                    _unitofwork.Save();
                }

                // Update existing products Master
                if (productsToUpdate.Count > 0)
                {
                    foreach (var product in productsToUpdate)
                    {
                        _unitofwork.ProductStockMasterRepository.Update(product);
                    }
                    _unitofwork.Save();
                }

                return Tuple.Create("Success", recordsUpdated + recordsInserted);
            }
            catch (DbEntityValidationException e)
            {
                foreach (var validationErrors in e.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Console.WriteLine("Property: {validationError.PropertyName} Error: {validationError.ErrorMessage}");
                    }
                }
                throw;
              
                return Tuple.Create("Validation Error", 0);
            }
            catch (Exception e)
            {
               // _unitofwork.Rollback();
                return Tuple.Create(e.Message, 0);
            }

        }




        public Tuple<string, int> ProductSavingUnitsExcelUpload(List<ProductServingUnit> excelstock, Int32 companyid)
        {

            _unitofwork.CreateTransaction();
            List<ProductServingUnit> ProductServingUnitlist = new List<ProductServingUnit>();
            int rowcount = 0;
            try
            {
                foreach (var p in excelstock)
                {
                    var ProductIds = _unitofwork.ProductServingUnitRepository.Get(d => d.LocationId == p.LocationId
                                            && d.ServingUnit == p.ServingUnit && d.ProductId == p.ProductId).Select(d => d.ProductServingUnitId).ToList();
                    rowcount = ProductIds.Count();
                    for (int rowIterator = 1; rowIterator <= rowcount; rowIterator++)
                    {
                        _unitofwork.ProductServingUnitRepository.Delete(ProductIds[rowIterator-1]);
                    }
                    ProductServingUnitlist.Add(p);
                }
                _unitofwork.ProductServingUnitRepository.BulkInsert(ProductServingUnitlist);
                _unitofwork.Save();
                _unitofwork.Commit();
                return Tuple.Create(Convert.ToString(ProductServingUnitlist.Count) + " Record(s) uploaded.", 1);
            }
            catch (Exception e)
            {
                _unitofwork.Rollback();
                return Tuple.Create(e.Message, 0);
            }
        }
        public Tuple<string, int> SupplierProductExcelUpload(List<SupplierProduct> excelstock, Int32 companyid)
        {
            var SupplierIds = _unitofwork.SuplierRepository.Get(d => d.CompanyID == companyid && d.IsDelete == false
                                                                && d.IsBlocked == false).Select(d => d.SupplierID).ToList();

            var ProductIds = _unitofwork.ProductRepository.Get(d => d.CompanyID == companyid
                                                                        && d.IsActive == true).Select(d => d.ProductId).ToList();

            _unitofwork.CreateTransaction();
            List<SupplierProduct> supplierproductlist = new List<SupplierProduct>();

            try
            {
                foreach (var p in excelstock)
                {
                    if (!SupplierIds.Contains(p.SupplierId))
                    {
                        return Tuple.Create("Supplier Id " + Convert.ToString(p.SupplierId) + " is not exists in system", 0);
                    }

                    if (!ProductIds.Contains(Convert.ToInt32(p.ProductId)))
                    {
                        return Tuple.Create("Product Id " + Convert.ToString(p.ProductId) + " is not exists in system", 0);
                    }

                    supplierproductlist.Add(p);
                }



                _unitofwork.SupplierProductRepository.BulkInsert(supplierproductlist);
                _unitofwork.Save();
                _unitofwork.Commit();
                return Tuple.Create(Convert.ToString(supplierproductlist.Count) + " Record(s) uploaded.", 1);

            }
            catch (Exception e)
            {
                _unitofwork.Rollback();
                return Tuple.Create(e.Message, 0);

            }

        }
        public Tuple<string, int> ProductTaxesExcelUpload(List<ProductTax> excelstock, Int32 companyid)
        {
            var TaxIds = _unitofwork.TaxRepository.Get(d => d.CompanyID == companyid && d.IsDelete == false
                                                                && d.IsActive == true).Select(d => d.TaxId).ToList();

            var ProductIds = _unitofwork.ProductRepository.Get(d => d.CompanyID == companyid
                                                                        && d.IsActive == true).Select(d => d.ProductId).ToList();

            _unitofwork.CreateTransaction();
            List<ProductTax> producttaxlist = new List<ProductTax>();

            try
            {
                foreach (var p in excelstock)
                {
                    if (!TaxIds.Contains(Convert.ToInt32(p.TaxId)))
                    {
                        return Tuple.Create("Tax Id " + Convert.ToString(p.TaxId) + " is not exists in system", 0);
                    }

                    if (!ProductIds.Contains(Convert.ToInt32(p.ProductId)))
                    {
                        return Tuple.Create("Product Id " + Convert.ToString(p.ProductId) + " is not exists in system", 0);
                    }

                    producttaxlist.Add(p);
                }



                _unitofwork.ProductTaxRepository.BulkInsert(producttaxlist);
                _unitofwork.Save();
                _unitofwork.Commit();
                return Tuple.Create(Convert.ToString(producttaxlist.Count) + " Record(s) uploaded.", 1);

            }
            catch (Exception e)
            {
                _unitofwork.Rollback();
                return Tuple.Create(e.Message, 0);

            }

        }

        public List<DataUploadProductTaxViewModel> DownloadProductTaxData(int companyid)
        {
            var taxes = (from p in _unitofwork.ProductRepository.GetAsNoTracking(p => p.CompanyID == companyid)
                         join pt in _unitofwork.ProductTaxRepository.GetAsNoTracking(pt => pt.CompanyID == companyid) on p.ProductId equals pt.ProductId
                         join t in _unitofwork.TaxRepository.GetAsNoTracking(t => t.CompanyID == companyid) on pt.TaxId equals t.TaxId
                         orderby p.ProductCode, t.TaxCode
                         select new
                         {
                             p.ProductId,
                             p.ProductCode,
                             t.TaxId,
                             t.TaxCode,


                         }).ToList();

            List<DataUploadProductTaxViewModel> vmproducttaxlist = new List<DataUploadProductTaxViewModel>();
            foreach (var r in taxes)
            {
                DataUploadProductTaxViewModel vmproducttax = new DataUploadProductTaxViewModel();
                vmproducttax.ProductId = (Int32)r.ProductId;
                vmproducttax.ProductCode = r.ProductCode;
                vmproducttax.TaxId = r.TaxId;
                vmproducttax.TaxCode = r.TaxCode;

                vmproducttaxlist.Add(vmproducttax);

            }

            return vmproducttaxlist == null ? null : vmproducttaxlist;
        }

        public PrinterType GetPrinterTypebyId(int id)
        {
            try
            {
                PrinterType printertypes = _unitofwork.PrinterTypeRepository.Get(p => p.PrinterTypeId == id).FirstOrDefault();

                if (printertypes != null)
                {
                    return printertypes;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }


        public List<ProductStockMaster> GetLocationStockbyProductID(long productid, int locationid)
        {
            try
            {
                var product = _unitofwork.ProductStockMasterRepository.Get(g => g.ProductId == productid && g.LocationId == locationid).ToList();

                return product;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
