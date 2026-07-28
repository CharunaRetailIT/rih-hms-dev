using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HospitalityManagement.Models;
//using System.Activities.Statements;
using HospitalityManagement.Models.ViewModels;

namespace HospitalityManagement.Service
{
    public class ProductService
    {
        ApplicationDbContext context = new ApplicationDbContext();
        ReceipeService _receipeService = new ReceipeService();
        public IEnumerable<Product> GetProducts()
        {
            try
            {
                IEnumerable<Product> products = context.Product.OrderBy(c => c.ProductCode);
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
        public IEnumerable<Product> GetProductionItems()
        {
            try
            {
                IEnumerable<Product> products = context.Product.Where(p=>p.IsRowMaterial==true).OrderBy(c => c.ProductCode);
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
        public IEnumerable<ProductStockMasterViewModel> GetProductionItems(long locid)
        {
            try
            {

                var productionitems = (
                          from p in context.Product
                          join ps in context.ProductStockMaster on p.ProductId equals ps.ProductId
                          where ps.LocationId == locid
                          && p.IsActive == true && p.IsDelete == false && p.IsRowMaterial==false
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
        public List<Receipe> GetReceipesByProductId(long id)
        {
            try
            {
                List<Receipe> receipes = context.Receipe.Where(r=>r.ProductId==id).OrderBy(c => c.ReceipeId).ToList();
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
                List<ProductServingUnit> servingunits = context.ProductServingUnit.Where(r => r.ProductId == id).OrderBy(c => c.ProductServingUnitId).ToList();
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
                List<ProductTax> productTaxs = context.ProductTax.Where(r => r.ProductId == id).OrderBy(c => c.ProductTaxId).ToList();
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
                List<SupplierProduct> productsuppliers = context.SupplierProduct.Where(r => r.ProductId == id).OrderBy(c => c.ProductId).ToList();
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
        public List<ProductStockMaster> GetProductStockMasterByProductId(long id)
        {
            try
            {

                List<ProductStockMaster> productstockmaster = context.ProductStockMaster.Where(r => r.ProductId == id).
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
        public ProductStockMaster GetProductStockMasterByProductIdLocId(long id,long locid)
        {
            try
            {

                ProductStockMaster productstockmaster = context.ProductStockMaster.Where(r => r.ProductId == id && r.LocationId == locid).FirstOrDefault();
                                                               
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
        public List<ProductStockMaster> GetStockReport(long locid,long productid)
        {
            try
            {
                List<ProductStockMaster> productstockmaster=new List<ProductStockMaster>();

                if (locid != 0 && productid!=0) 
                {
                    productstockmaster = context.ProductStockMaster.Where(r => r.ProductId == productid && r.LocationId == locid).
                                         OrderBy(c => c.ProductName).OrderBy(d=>d.LocationId).ToList();
                }else if (locid!=0 && productid==0)
                {
                    productstockmaster = context.ProductStockMaster.Where(r => r.LocationId == locid).
                                         OrderBy(c => c.ProductName).OrderBy(d => d.LocationId).ToList();
                }
                else if (locid == 0 && productid != 0) 
                {
                    productstockmaster = context.ProductStockMaster.Where(r => r.ProductId == productid).
                                         OrderBy(c => c.ProductName).OrderBy(d => d.LocationId).ToList();
                }else if (locid==0 && productid==0)
                {
                    productstockmaster = context.ProductStockMaster.
                                         OrderBy(c => c.ProductName).OrderBy(d => d.LocationId).ToList();
                }
              
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
        public IEnumerable<Product> GetProductByDepartmentId(long id)
        {
            try
            {
                IEnumerable<Product> products = context.Product.Where(g => g.IsDelete == false && g.DepartmentId == id).OrderBy(g => g.ProductCode);
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
                IEnumerable<Product> products = context.Product.Where(g => g.IsDelete == false
                                                && g.DepartmentId == id && g.IsRowMaterial==false).OrderBy(g => g.ProductCode);
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
                IEnumerable<Product> products = context.Product.Where(g => g.IsDelete == false && g.CategoryId == id   && g.IsRowMaterial == false).OrderBy(g => g.ProductCode);
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
                IEnumerable<Product> products = context.Product.Where(g => g.IsDelete == false && g.SubCategoryId == id && g.IsRowMaterial == false).OrderBy(g => g.ProductCode);
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
        public IEnumerable<Product> GetMenuByDeptCatId(long deptid,long catid)
        {
            try
            {
                IEnumerable<Product> products = context.Product.Where(g => g.IsDelete == false && g.DepartmentId==deptid && 
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
                IEnumerable<Product> products = context.Product.Where(g => g.IsDelete == false && g.DepartmentId == deptid && 
                                                                        g.CategoryId == catid && g.SubCategoryId==scatid && g.IsRowMaterial == false).OrderBy(g => g.ProductCode);
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
                IEnumerable<Product> products = context.Product.Where(g => g.IsDelete == false && g.ProductId==id && g.IsRowMaterial == false).OrderBy(g => g.ProductCode);
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

        public IEnumerable<Product> GetActiveProducts()
        {
            try
            {
                //IEnumerable<Product> products = context.Product.Where(g => g.IsDelete == false && 
                //                                                      g.IsActive == true).OrderBy(g => g.ProductCode);
                //if (products != null)
                //{
                //    return products;
                //}
                //else
                //    return null;

                var sysproducts = context.Product.Select(p => new { p.ProductId, p.ProductCode, p.ProductName, p.IsActive, p.IsDelete, p.IsRowMaterial }).Where(g => g.IsDelete == false &&
                                                                            g.IsActive == true && g.IsDelete == false).OrderBy(g => g.ProductCode);

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
        public IEnumerable<Product> GetFinishGoods()
        {
            try
            {
                //IEnumerable<Product> products = context.Product.Where(g => g.IsDelete == false &&
                //                                                      g.IsActive == true && g.IsRowMaterial==false).OrderBy(g => g.ProductCode);

               var sysproducts = context.Product.Select(p=>new {p.ProductId,p.ProductCode,p.ProductName,p.IsActive,p.IsDelete,p.IsRowMaterial }).Where(g => g.IsDelete == false &&
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
        public IEnumerable<ProductServingUnit> GetServingUnits(long productid)
        {
            try
            {
                IEnumerable<ProductServingUnit> servinguints = context.ProductServingUnit.Where(p => p.ProductId == productid);
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
                //IEnumerable<Product> rowmaterials = context.Product.Where(p => p.IsActive==true && p.IsDelete==false && p.IsRowMaterial==true);

                 var sysrowmaterials = context.Product.Select(p=>new
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
        public IEnumerable<Product> GetAddons()
        {
            try
            {
                //IEnumerable<Product> products = context.Product.Where(g => g.IsDelete == false &&
                //                                                      g.IsActive == true && g.IsRowMaterial==false).OrderBy(g => g.ProductCode);

                var sysproducts = context.Product.Select(p => new { p.ProductId, p.ProductCode, p.ProductName, p.IsActive, p.IsDelete, p.IsAddon,p.PurchasingUnit }).Where(g => g.IsDelete == false &&
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
        public List<Product> GetNotRawProducts()
        {
            try
            {
                List<Product> products = context.Product.Where(g => g.IsDelete == false &&
                                                                      g.IsActive == true && g.IsRowMaterial == false).
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
        public IEnumerable<Product> GetProductAddons()
        {
            try
            {
                IEnumerable<Product> products = context.Product.Where(g => g.IsDelete == false &&
                                                                      g.IsActive == true && g.IsRowMaterial == true).OrderBy(g => g.ProductCode);
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
                IEnumerable<PrinterType> printertypes = context.PrinterType.Where(p => p.IsDelete == false);
                                                           
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
        public Product GetProductById(long id)
        {
            try
            {
                var product = context.Product.FirstOrDefault(g => g.ProductId == id);
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
              // var product = context.Product.Select(p =>new { p.ProductName,p.ProductId }).FirstOrDefault(g => g.ProductId == id);

                var prd = (
                          from p in context.Product
                          join a in context.Addons on p.ProductId equals a.ProductId
                          where p.ProductId == id &&
                          p.IsActive == true && p.IsDelete == false 
                          orderby p.ProductName
                          select new
                          {
                              ProductId = p.ProductId,
                              ProductName = p.ProductName,
                           
                          }
                      ).ToList();


                Product products = new Product();
                products.ProductName = prd.First().ProductName;
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
                          from p in context.Product
                          join a in context.Addons on p.ProductId equals a.ProductAddonId
                          where a.ProductAddonId==id 
                          orderby p.ProductName
                          select new
                          {
                              ProductId = p.ProductId,
                              ProductName = p.ProductName,

                          }
                      ).ToList();


              
                products.ProductName = prd.First().ProductName;
                return products ?? null;
            }
            catch (Exception ex)
            {
                return products;
             // throw;
            }
        }
        public Boolean CheckProductCodeExists(string productcode)
        {
            try
            {
                return context.Product.Any(g => g.ProductCode == productcode);
            }
            catch (Exception)
            {

                throw;
            }
        }
        public bool SaveProduct(Product product)
        {

            using (var dbtransaction = context.Database.BeginTransaction())
            {
                try
                {

                       context.Product.Add(product);
                    if (context.SaveChanges() == 1)
                    {
                        //if (product.Receipes.Count > 0)
                        //{


                        //    foreach (var receipe in product.Receipes)
                        //    {
                        //        receipe.ProductId = product.ProductId;
                        //        receipe.GroupOfCompanyID = 1;
                        //        receipe.CompanyID = 1;
                        //        receipe.LocationId = 1;
                        //        receipe.DataTransfer = 0;


                        //        context.Receipe.Add(receipe);

                        //    }

                        //    if (context.SaveChanges() != product.Receipes.Count)
                        //    {
                        //        dbtransaction.Rollback();
                        //        return false;
                        //    }
                        //}

                        // If taxes exists
                        if (product.ProductTax.Count > 0)
                        {


                            foreach (var tax in product.ProductTax)
                            {
                                tax.ProductId = product.ProductId;
                                tax.GroupOfCompanyID = 1;
                                tax.CompanyID = product.CompanyID;
                                tax.LocationId = product.LocationId;
                                tax.DataTransfer = 0;
                                tax.TaxSequence = product.ProductTax.IndexOf(tax)+1;
                                tax.TaxPracentage = 100;

                                context.ProductTax.Add(tax);

                            }

                            if (context.SaveChanges() != product.ProductTax.Count)
                            {
                                dbtransaction.Rollback();
                                return false;
                            }

                        }
                        // If Serving units exists

                        if (product.ProductServingUnit.Count > 0)
                        {


                            foreach (var servingunit in product.ProductServingUnit)
                            {
                                servingunit.ProductId = product.ProductId;
                                servingunit.GroupOfCompanyID = 1;
                                servingunit.CompanyID = 1;
                                servingunit.LocationId = 1;
                                servingunit.DataTransfer = 0;


                                context.ProductServingUnit.Add(servingunit);

                            }

                            if (context.SaveChanges() != product.ProductServingUnit.Count)
                            {
                                dbtransaction.Rollback();
                                return false;
                            }

                        }

                        // If Supplier exists

                        if (product.SupplierProduct.Count > 0)
                        {
                            foreach (var supplierproduct in product.SupplierProduct)
                            {
                                supplierproduct.ProductId = product.ProductId;
                                supplierproduct.GroupOfCompanyID = 1;
                                supplierproduct.CompanyID = 1;
                                supplierproduct.LocationId = 1;
                                supplierproduct.DataTransfer = 0;
                                supplierproduct.ModifiedDate = product.ModifiedDate;
                                supplierproduct.CreatedDate = product.CreatedDate;
                                supplierproduct.CreatedUser = product.CreatedUser;
                                supplierproduct.ModifiedUser = product.ModifiedUser;

                                context.SupplierProduct.Add(supplierproduct);

                            }

                            if (context.SaveChanges() != product.SupplierProduct.Count)
                            {
                                dbtransaction.Rollback();
                                return false;
                            }
                        }

                        // If Stock Location Master Exists

                        if (product.ProductLocationViewModel.Count > 0)
                        {
                            //List<ProductStockMaster> prdstocklist = new List<ProductStockMaster>();
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
                                prdstock.AvgCost = 0;
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

                                //if (prdstock.CostPrice != 0 && prdstock.SellingPrice != 0)
                                //{
                                    context.ProductStockMaster.Add(prdstock);
                                    context.SaveChanges();
                               // }
                              

                            }

                            //  int sss = context.SaveChanges();

                            //if (context.SaveChanges() != product.ProductLocationViewModel.Count)
                            //{
                            //    dbtransaction.Rollback();
                            //    return false;
                            //}
                        }

                        dbtransaction.Commit();
                        return true;
                    }
                    else
                    {
                        dbtransaction.Rollback();
                        return false;
                    }
                }

                catch (Exception)
                {
                    dbtransaction.Rollback();
                    throw;
                }
            }
        }
        public int DeleteReceipesByProductId(long id)
        {
            try
            {
                context.Receipe.RemoveRange(context.Receipe.Where(x => x.ProductId == id));
                var res = context.SaveChanges();
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
                context.ProductServingUnit.RemoveRange(context.ProductServingUnit.Where(x => x.ProductId == id));
                var res = context.SaveChanges();
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
                context.ProductTax.RemoveRange(context.ProductTax.Where(x => x.ProductId == id));
                var res = context.SaveChanges();
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
                context.SupplierProduct.RemoveRange(context.SupplierProduct.Where(x => x.ProductId == id));
                var res = context.SaveChanges();
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
                context.ProductStockMaster.RemoveRange(context.ProductStockMaster.Where(x => x.ProductId == id));
                var res = context.SaveChanges();
                return res;

            }
            catch (Exception)
            {

                throw;
            }
        }
        public int UpdateProductHeader(Product product)
        {
            int res = context.SaveChanges();
            return res;
        }
        public bool UpdateProduct(Product prd)
        {
            using (var dbtransaction = context.Database.BeginTransaction())
            {

                try
                {


                    if (context.SaveChanges() == 1)
                    {
                        //update receipes

                       // if (prd.Receipes.Count > 0)
                       //{
                       //         DeleteReceipesByProductId(prd.ProductId);
                            
                       //         foreach (var receipe in prd.Receipes)
                       //         {
                       //             receipe.ProductId = prd.ProductId;
                       //             receipe.GroupOfCompanyID = 1;
                       //             receipe.CompanyID = 1;
                       //             receipe.LocationId = 1;
                       //             receipe.DataTransfer = 0;


                       //             context.Receipe.Add(receipe);

                       //         }

                       //         if (context.SaveChanges() != prd.Receipes.Count)
                       //         {
                       //             dbtransaction.Rollback();
                       //             return false;
                       //         }
                      

                       // }


                        // If Serving units exists

                        if (prd.ProductServingUnit.Count > 0)
                        {

                            if (_receipeService.GetReceipeByProductId(prd.ProductId).Count() == 0)
                            {

                                DeleteServingUnitsByProductId(prd.ProductId);
                                foreach (var servingunit in prd.ProductServingUnit)
                                {
                                    servingunit.ProductId = prd.ProductId;
                                    servingunit.GroupOfCompanyID = prd.GroupOfCompanyID;
                                    servingunit.CompanyID = prd.CompanyID;
                                    servingunit.LocationId = prd.LocationId;
                                    servingunit.DataTransfer = 0;


                                    context.ProductServingUnit.Add(servingunit);

                                }

                                if (context.SaveChanges() != prd.ProductServingUnit.Count)
                                {
                                    dbtransaction.Rollback();
                                    return false;
                                }

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
                                
                                    context.ProductTax.Add(tax);

                                }

                                if (context.SaveChanges() != prd.ProductTax.Count)
                                {
                                    dbtransaction.Rollback();
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
                                    supplier.CompanyID = 1;
                                    supplier.LocationId = 1;
                                    supplier.DataTransfer = 0;


                                    context.SupplierProduct.Add(supplier);

                                }

                                if (context.SaveChanges() != prd.SupplierProduct.Count)
                                {
                                    dbtransaction.Rollback();
                                    return false;
                                }


                            }

                            // if locations exists
                            if (prd.ProductLocationViewModel.Count > 0)
                            {


                            
                           //   DeleteLocationByProductId(prd.ProductId);
                             
                             foreach (var loc in prd.ProductLocationViewModel)
                             {
                                

                                if (context.ProductStockMaster.Any(se => se.ProductId == loc.ProductId && se.LocationId == loc.LocationId))
                                {

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
                                    //   ps.LastUpdatedDate = dbproductstockmaster.LastUpdatedDate;

                                    //if (dbproductstockmaster.CostPrice != 0)
                                    //{
                                        // context.ProductStockMaster.Add(ps);
                                        int fff = context.SaveChanges();
                                    //}
                                }
                                else
                                {
                                    var ps = new ProductStockMaster();
                                    ps.ProductId = prd.ProductId;
                                    ps.LocationId = loc.LocationId;
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

                                    //if (loc.CostPrice != 0)
                                    //{
                                         context.ProductStockMaster.Add(ps);
                                        int fff = context.SaveChanges();
                                    //}


                                }
                            }
                         
                                


                            }

                             dbtransaction.Commit();
                             return true;
                    }
                    else
                    {
                        dbtransaction.Rollback();
                        return false;

                    }

                 }

                catch (Exception ex)
                {
                    dbtransaction.Rollback();
                    throw;
                }

            }

        }
        public ProductStockMaster psm(long productid,long locationid)
        {
            try
            {
                long loc = locationid;
                long prd = productid;
                ProductStockMaster productStockMaster = context.ProductStockMaster.Where(r => r.ProductId == productid &&
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
        public List<long> GetProductStockMasterByLocId(long frmloc,long toloc)
        {
            try
            {

                
                List<ProductStockMaster> pslist = new List<ProductStockMaster>();

                List<long> from = new List<long>();
                List<long> to = new List<long>();
                List<long> match = new List<long>();

                foreach (var p in context.ProductStockMaster.Where(p => p.LocationId == frmloc).ToList())
                {
                    from.Add(p.ProductId);
                }
                foreach (var p in context.ProductStockMaster.Where(p => p.LocationId == toloc).ToList())
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


                //  pslist = context.ProductStockMaster.Where(p => p.LocationId == frmloc).ToList();

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


              return  context.ProductStockMaster.Where(p => p.LocationId == locid).ToList();
              

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
                var products = (from p in context.Product
                           join pm in context.ProductStockMaster on p.ProductId equals pm.ProductId
                           join um in context.UnitOfMeasure on p.PurchasingUnit equals um.UnitOfMeasureId
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

               return  null;
            }
        }
        public string  GetUOMById(long uomid)
        {
            if (uomid != 0)
            {
                var uom = context.UnitOfMeasure.Where(u => u.UnitOfMeasureId == uomid).FirstOrDefault().UnitOfMeasureName;
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
        public List<ProductStockMaster> GetProductsByLocIdProductId(long locid,long prdid)
        {
            try
            {


                return context.ProductStockMaster.Where(p => p.LocationId == locid && p.ProductId==prdid).ToList();


            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public ProductStockMasterViewModel GetReceipeDetails(long locid,long productid,decimal qty)
        {
            try
            {
                var product = context.ProductStockMaster.Where(p=>p.LocationId==locid && p.ProductId==productid).FirstOrDefault();
                ProductStockMasterViewModel vm = new ProductStockMasterViewModel();
                if (product != null)
                {                                   
                    vm.CostPrice = product.CostPrice * qty;
                    vm.SellingPrice = product.SellingPrice * qty;
                    vm.ProductName = product.ProductName;
                    vm.ProductId = product.ProductId;
                }
                return vm;


            }
            catch (Exception)
            {

                throw;
            }
        }
        public int RemoveAddons(long id,long aid)
        {
            try
            {
                context.Addons.RemoveRange(context.Addons.Where(x => x.ProductId == id && x.ProductAddonId==aid));
                var res = context.SaveChanges();
                return res;

            }
            catch (Exception)
            {

                throw;
            }
        }
        public int RemoveAddonsbyId(long id)
        {
            try
            {
                context.Addons.RemoveRange(context.Addons.Where(x => x.AddonsId==id));
                var res = context.SaveChanges();
                return res;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public Product FindByCode(string code)
        {
            var product = context.Product.Where(p => p.ProductCode == code).FirstOrDefault();
            if (product != null)
            {
                return product;
            }
            else
            {
                return null;
            }
        }


    }
}