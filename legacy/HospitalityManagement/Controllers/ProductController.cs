//using HospitalityManagement.Models.PopUp;
using HospitalityManagement.Models.Transactions;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using RIT.HMS.BLL;
using RIT.HMS.BLL.Common;
using RIT.HMS.BLL.Configurations;
using RIT.HMS.BLL.MasterData;
using RIT.HMS.BLL.TransactionData;
using RIT.HMS.Domain;
using RIT.HMS.Domain.ViewModels;
using RIT.HMS.Domain.ViewModels.DataUpload;
using RIT.HMS.Domain.ViewModels.Reports;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using RIT.HMS.Domain.Transactions;

namespace HospitalityManagement.Controllers
{
    [SessionTimeout]
    public class ProductController : Controller
    {

        private BLL_Product _bllproduct;
        BLL_Location _location;
        private readonly BLL_Tax _blltax;
        private readonly BLL_Supplier _bllSupplier;
        private readonly BLL_Location _blllocation;
        private readonly BLL_SubCategory _bllsubcategory;
        private readonly BLL_AddonCategory _bllAddonCategory;
        private readonly BLL_ServingUnit _bllServingUnits;
        private readonly BLL_Receipe _bllReceipe;
        private readonly AppManager _appmanager;
        private readonly BLL_Configuration _bllconfiguration;
        private readonly BLL_Department _blldepartment;
        private readonly BLL_Category _bllcategory;
        private readonly BLL_UnitOfMeasure _bllunitofmeasure;
        private readonly BLL_UnitConversion _bllunitconversion;
        private readonly BLL_ProductKitchenMapper _bLL_ProductKitchenMapper;
        private readonly BLL_StockAdjustment _bLL_StockAdjustment;
        private readonly BLL_Common _bllcommon;
        private ProductStockViewModel StockViewModelSelection;

        public ProductController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();

            _bllproduct = new BLL_Product(cn);
            _location = new BLL_Location(cn);
            _blltax = new BLL_Tax(cn);
            _bllSupplier = new BLL_Supplier(cn);
            _blllocation = new BLL_Location(cn);
            _bllsubcategory = new BLL_SubCategory(cn);
            _bllAddonCategory = new BLL_AddonCategory(cn);
            _bllServingUnits = new BLL_ServingUnit(cn);
            _bllReceipe = new BLL_Receipe(cn);
            _appmanager = new AppManager(cn);
            _bllconfiguration = new BLL_Configuration(cn);
            _blldepartment = new BLL_Department(cn);
            _bllcategory = new BLL_Category(cn);
            _bllunitofmeasure = new BLL_UnitOfMeasure(cn);
            _bllunitconversion = new BLL_UnitConversion(cn);
            _bLL_ProductKitchenMapper = new BLL_ProductKitchenMapper(cn);
            _bLL_StockAdjustment = new BLL_StockAdjustment(cn);
            StockViewModelSelection = new ProductStockViewModel();

        }

        [Authorize(Roles = "PrdCreatee")]
        public ActionResult AssignKitchens()
        {
            Session["MessageId"] = "0";
            var result = new List<Product>();
            try
            {

                int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
                result = _bllproduct.GetAutoProductionProducts(companyid).ToList();
                result.ToList().ForEach(c =>
                {
                    c.KitchenLocationCount = _bLL_ProductKitchenMapper.GetAllByProductId(Convert.ToInt32(Session["loggedusercompanyId"].ToString()), c.ProductId).ToList().Count;
                });
            }
            catch (Exception ex)
            {
                Session["MessageId"] = "4";
                Session["Message"] = ex.Message;
            }

            return View(result);
        }

        [Authorize(Roles = "PrdCreatee")]
        public ActionResult AddKitchen(int id)
        {
            var result = new ProductMapperToKitchen();

            try
            {
                result.GeneralLocationId = 0;
                result.Product = _bllproduct.GetProductById(id);
                result.KitchenLocationList = _blllocation.GetActiveKitchenLocations(Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ToList();
                result.ProductKitchenMapper = _bLL_ProductKitchenMapper.GetAllByProductId(Convert.ToInt32(Session["loggedusercompanyId"].ToString()), id);
                result.KitchenLocationList = _bLL_ProductKitchenMapper.MapperdLocationSelect(result);
            }
            catch (Exception ex)
            {
                Session["MessageId"] = "4";
                Session["Message"] = ex.Message;
            }

            return View(result);
        }
   
        [Authorize(Roles = "PrdCreatee")]
        [HttpPost]
        public ActionResult AddKitchen(int id, ProductMapperToKitchen entity)
        {
            try
            {
                entity.CreatedDate = DateTime.Now;
                entity.DataTransfer = 0;
                entity.CreatedUser = Session["loggeduser"].ToString();
                entity.ModifiedDate = DateTime.Now;
                entity.ModifiedUser = Session["loggeduser"].ToString();
                entity.IsActive = true;
                entity.Product = _bllproduct.GetProductById(entity.Product.ProductId);
                entity.ProductKitchenMapper = _bLL_ProductKitchenMapper.GetAllByProductId(Convert.ToInt32(Session["loggedusercompanyId"].ToString()), entity.Product.ProductId);

                if (_bLL_ProductKitchenMapper.SaveSubLocation(entity) == 1)
                {
                    Session["MessageId"] = "1";
                    return RedirectToAction("AddKitchen", "Product", new { @id = id });
                }
                else
                {
                    Session["MessageId"] = "2";
                }
            }
            catch (Exception ex)
            {
                Session["MessageId"] = "4";
                Session["Message"] = ex.Message;
            }
            return View(entity);
        }

        // GET: Product ddd

        [Authorize(Roles = "PrdCreatee")]
        public ActionResult Index()
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var loc = _blllocation.GetActiveLocations(companyid);
            var product = new Product();

            foreach (var l in loc)
            {
                var vm = new ProductLocationViewModel();
                vm.LocationId = l.SysLocationID;
                vm.Location = l.LocationName;
                product.ProductLocationViewModel.Add(vm);
            }
            @ViewBag.PurchasingUnitId = 0;
            return View("Create", product);
        }

        [Authorize(Roles = "PrdView")]
        public ActionResult ViewAllProducts()
         {
            if (!_appmanager.SetPermissions(0, Session["loggeduserempcode"].ToString(), "UpdateRecipes"))
            {
                @ViewBag.Update = "0";
            }
            else
            {
                @ViewBag.Update = "1";
            }

            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var products = _bllproduct.GetProducts(companyid);

            return View(products);
        }

        [Authorize(Roles = "PrdView")]
        public void HMSProducts()
        {
            if (!_appmanager.SetPermissions(0, Session["loggeduserempcode"].ToString(), "UpdateRecipes"))
            {
                @ViewBag.Update = "0";
            }
            else
            {
                @ViewBag.Update = "1";
            }

            //int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            //var products = _bllproduct.GetProducts(companyid);


            /*
            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Products");
            Sheet.Cells["A1"].Value = "ProductCode";
            Sheet.Cells["B1"].Value = "ProductName";
            Sheet.Cells["C1"].Value = "IsActive";
            Sheet.Cells["D1"].Value = "IsRowMaterial";

            int row = 2;
            foreach (var item in products)
            {
                Sheet.Cells[string.Format("A{0}", row)].Value = item.ProductCode;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.ProductName;
                Sheet.Cells[string.Format("C{0}", row)].Value = item.IsActive;
                Sheet.Cells[string.Format("D{0}", row)].Value = item.IsRowMaterial;

                row++;
            }

            Sheet.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.Buffer = true;
            Response.Charset = "";
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: attachment;filename=HMSProducts.xlsx");

            using (MemoryStream MyMemoryStream = new MemoryStream())
            {
                Ep.SaveAs(MyMemoryStream);
                MyMemoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }
             * */



            using (ExcelPackage pck = new ExcelPackage())
            {


                int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
                var products = _bllproduct.GetProducts(companyid);


                //int compayid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
                //var exists = _customer.GetCustomers(compayid);


                ExcelPackage Ep = new ExcelPackage();
                ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Products");

                string compName, Address1, Address2, Address3, Tele, Fax, website, ReportHead = "";
                compName = _location.GetCompanyDetails().CompanyName;
                Address1 = _location.GetCompanyDetails().Address1;
                Address2 = _location.GetCompanyDetails().Address2;
                Address3 = _location.GetCompanyDetails().Address3;
                Tele = _location.GetCompanyDetails().Telephone;
                Fax = _location.GetCompanyDetails().Fax;
                website = _location.GetCompanyDetails().Website;

                #region

                Sheet.Cells[1, 2].Value = compName;
                Sheet.Cells[1, 2, 3, 12].Merge = true;
                Sheet.Cells[1, 2, 3, 12].Style.Font.Size = 12;
                Sheet.Cells[1, 2, 3, 12].Style.VerticalAlignment = ExcelVerticalAlignment.Bottom;

                Sheet.Cells[4, 2].Value = Address1 + " " + Address2 + " " + Address3;
                ;
                Sheet.Cells[4, 2, 4, 12].Merge = true;
                Sheet.Cells[4, 2, 4, 12].Style.Font.Size = 10;

                Sheet.Cells[5, 2].Value = "Tel:- " + Tele + " / " + ",  Fax:- " + Fax + ",  Web Site:- " + website;
                Sheet.Cells[5, 2, 5, 12].Merge = true;
                Sheet.Cells[5, 2, 5, 12].Style.Font.Size = 10;

                Sheet.Cells[6, 2].Value = "Product List Report";
                Sheet.Cells[6, 2, 6, 12].Merge = true;
                Sheet.Cells[6, 2, 6, 12].Style.Font.Size = 12;

                var businessUnitDetail = Sheet.Cells[1, 2, 6, 12];
                businessUnitDetail.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                businessUnitDetail.Style.Font.Bold = true;
                businessUnitDetail.Style.Font.Name = "Calibri";

                #endregion

                #region Print Detail Box

                var printDetailBox = Sheet.Cells[3, 13, 5, 14];
                printDetailBox.Style.Font.Size = 8;
                Sheet.Cells[3, 13, 5, 13].Style.Font.Bold = true;

                Sheet.Cells[3, 13].Value = "Date";
                Sheet.Cells[4, 13].Value = "Time";
                Sheet.Cells[5, 13].Value = "Req. By";

                Sheet.Cells[3, 14].Value = DateTime.Now.Date.ToShortDateString();
                Sheet.Cells[4, 14].Value = DateTime.Now.ToString("h:mm tt");

                if (Session["loggeduser"] != null)
                    Sheet.Cells[5, 14].Value = Session["loggeduser"].ToString();
                else
                    Sheet.Cells[5, 14].Value = " ";


                #endregion

                Sheet.Cells[8, 1].Value = "Prodcut Code";
                Sheet.Cells[8, 2].Value = "Product Name";
                Sheet.Cells[8, 3].Value = "Bar Code";
                Sheet.Cells[8, 4].Value = "Department Code";
                Sheet.Cells[8, 5].Value = "Category Code";
                Sheet.Cells[8, 6].Value = "Sub-Category Code";
                Sheet.Cells[8, 7].Value = "UOM";

                Sheet.Cells[8, 8].Value = "Cost Price";
                Sheet.Cells[8, 9].Value = "Selling Price";
                Sheet.Cells[8, 10].Value = "Maximum Price";
                Sheet.Cells[8, 11].Value = "Minimum Price";




                Sheet.Cells[8, 12].Value = "Is Active";
                Sheet.Cells[8, 13].Value = "Is Row Material";


                int row = 9;
                foreach (var item in products)
                {

                    Sheet.Cells[row, 1].Value = item.ProductCode;
                    Sheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    Sheet.Cells[row, 2].Value = item.ProductName;
                    Sheet.Cells[row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    Sheet.Cells[row, 3].Value = item.Barcode;
                    Sheet.Cells[row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    Sheet.Cells[row, 4].Value = item.DepartmnetCode;
                    Sheet.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;


                    Sheet.Cells[row, 5].Value = item.CategoryCode;
                    Sheet.Cells[row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    Sheet.Cells[row, 6].Value = item.SubCategoryCode;
                    Sheet.Cells[row, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    Sheet.Cells[row, 7].Value = item.UOM;
                    Sheet.Cells[row, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    if (item.CostPrice > 0)
                    {
                        Sheet.Cells[row, 8].Value = item.CostPrice;
                        Sheet.Cells[row, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    }
                    else
                    {
                        Sheet.Cells[row, 8].Value = "";
                        Sheet.Cells[row, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    }


                    if (item.SellingPrice > 0)
                    {
                        Sheet.Cells[row, 9].Value = item.SellingPrice;
                        Sheet.Cells[row, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    }
                    else
                    {
                        Sheet.Cells[row, 9].Value = "";
                        Sheet.Cells[row, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    }

                    if (item.MaxPrice > 0)
                    {
                        Sheet.Cells[row, 10].Value = item.MaxPrice;
                        Sheet.Cells[row, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    }
                    else
                    {
                        Sheet.Cells[row, 10].Value = "";
                        Sheet.Cells[row, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    }

                    if (item.MinPrice > 0)
                    {

                        Sheet.Cells[row, 11].Value = item.MinPrice;
                        Sheet.Cells[row, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    }
                    else
                    {
                        Sheet.Cells[row, 11].Value = "";
                        Sheet.Cells[row, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    }



                    if (item.IsActive == true)
                    {
                        Sheet.Cells[row, 12].Value = "Yes";
                        Sheet.Cells[row, 12].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    }
                    else
                    {
                        Sheet.Cells[row, 12].Value = "No";
                        Sheet.Cells[row, 12].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    }
                    if (item.IsRowMaterial == true)
                    {
                        Sheet.Cells[row, 13].Value = "Yes";
                        Sheet.Cells[row, 13].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    }
                    else
                    {
                        Sheet.Cells[row, 13].Value = "No";
                        Sheet.Cells[row, 13].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    }


                    row++;
                }


                #region
                System.Drawing.Color colFromHexHeading = System.Drawing.ColorTranslator.FromHtml("#919089");

                Sheet.Cells[8, 1, 8, 13].Style.Fill.PatternType = ExcelFillStyle.Solid;
                Sheet.Cells[8, 1, 8, 13].Style.Fill.BackgroundColor.SetColor(colFromHexHeading);

                var table = Sheet.Cells[8, 1, 8, 13];
                table.Style.Border.Top.Style =
                table.Style.Border.Left.Style =
                table.Style.Border.Right.Style =
                table.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                table.Style.Font.Bold = true;
                table.Style.Font.Name = "Calibri";
                table.AutoFitColumns();

                #endregion

                Sheet.Cells["A:AZ"].AutoFitColumns();
                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "";
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment: attachment;filename=HMSProductList.xlsx");

                using (MemoryStream MyMemoryStream = new MemoryStream())
                {
                    Ep.SaveAs(MyMemoryStream);
                    MyMemoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }
            }


        }

        public ActionResult ViewFinishGoods()
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var finishedgoods = _bllproduct.GetFinishGoods(companyid).ToList();

            return Json(JsonConvert.SerializeObject(finishedgoods, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetLocationStockbyProductID(long id)
        {

             int LocationID= Convert.ToInt32(Session["loggeduserlocId"].ToString());
            var finishedgoods = _bllproduct.GetLocationStockbyProductID(id, LocationID).ToList();

           

            return Json(JsonConvert.SerializeObject(finishedgoods, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }





        public ActionResult GetServingUnits(long id)
        {
            var servingunits = _bllproduct.GetServingUnits(id);
            List<ProductServingUnit> newsulist = new List<ProductServingUnit>();
            foreach (var s in servingunits)
            {
                ProductServingUnit newsu = new ProductServingUnit();
                if (!newsulist.Select(s1 => s1.ServingUnit).Contains(s.ServingUnit))
                {
                    newsu = s;
                    newsulist.Add(newsu);
                }
            }

            return Json(JsonConvert.SerializeObject(newsulist, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetProductServingServingUnits(long id)
        {
            var servingunits = _bllproduct.GetServingUnits(id);

            return Json(JsonConvert.SerializeObject(servingunits, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetRowMaterials()
        {
            var rowmaterials = _bllproduct.GetRowMaterials();
            rowmaterials.ToList().ForEach(p =>
            {
                if (p.PurchasingUnit != 0)
                {
                    p.UOM = _bllproduct.GetUOMById(p.PurchasingUnit);
                }
            }
           );
            return Json(JsonConvert.SerializeObject(rowmaterials, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAddons()
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var rowmaterials = _bllproduct.GetAddons(companyid);
            rowmaterials.ToList().ForEach(p =>
            {
                if (p.PurchasingUnit != 0)
                {
                    p.UOM = _bllproduct.GetUOMById(p.PurchasingUnit);
                }
            }
           );
            return Json(JsonConvert.SerializeObject(rowmaterials, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "PrdEdit")]
        [HttpPost]
        public ActionResult Edit(Product prd)
        {
            var id = RouteData.Values["id"] + Request.Url.Query;
              bool Save = true;
            var existsproduct = _bllproduct.GetProductById(Convert.ToInt64(id));
            // var existsproduct = _productservice.GetProductById(prd.ProductId);

            existsproduct.ProductCode = prd.ProductCode;
           
            existsproduct.ProductName = prd.ProductName;
            if (!string.IsNullOrEmpty(prd.ProductDesp))
            {
                existsproduct.ProductDesp = prd.ProductDesp;
            }
            else
            {
                existsproduct.ProductDesp = string.Empty;
            }
           
            existsproduct.NameOnInvoice = prd.NameOnInvoice;
            existsproduct.ProductNameInSinhala = prd.ProductNameInSinhala;
            existsproduct.RefCode01 = prd.RefCode01;
            existsproduct.RefCode02 = prd.RefCode02;
            existsproduct.DepartmentId = prd.DepartmentId;
            existsproduct.CategoryId = prd.CategoryId;
            existsproduct.SubCategoryId = prd.SubCategoryId;

            existsproduct.IsActive = prd.IsActive;
            existsproduct.IsRowMaterial = prd.IsRowMaterial;
            existsproduct.IsOpenItem = prd.IsOpenItem;
            existsproduct.IsAddon = prd.IsAddon;
            existsproduct.IsCountable = prd.IsCountable;
            existsproduct.IsDelete = prd.IsDelete;
            existsproduct.IsItemLock = prd.IsItemLock;
            existsproduct.IsScaleItem = prd.IsScaleItem;
            existsproduct.IsCostOnReceipe = prd.IsCostOnReceipe;
            existsproduct.IsDiscount = prd.IsDiscount;
            existsproduct.IsPackItem = prd.IsPackItem;
            existsproduct.IsPromotion = prd.IsPromotion;
            existsproduct.IsFreeIssue = prd.IsFreeIssue;
            existsproduct.IsExpiry = prd.IsExpiry;
            existsproduct.IsTax = prd.IsTax;
            existsproduct.IsTaxInclude = prd.IsTaxInclude;
            //  existsproduct.IsTaxOnTax = prd.IsTaxOnTax;

            existsproduct.IsUnderCost = prd.IsUnderCost;
            existsproduct.IsBundle = prd.IsBundle;

            //    existsproduct.Receipes = prd.Receipes;

            existsproduct.ProductServingUnit = prd.ProductServingUnit;
            existsproduct.ProductTax = prd.ProductTax;

            existsproduct.PackSize = prd.PackSize;
            existsproduct.PackPrice = prd.PackPrice;
            existsproduct.WeightPerUnit = prd.WeightPerUnit;
            existsproduct.MaxPrice = prd.MaxPrice;
            existsproduct.MinPrice = prd.MinPrice;
            existsproduct.DiscountPrecentage = prd.DiscountPrecentage;
            existsproduct.MaximumDiscount = prd.MaximumDiscount;
            existsproduct.FixedDiscountPercentage = prd.FixedDiscountPercentage;
            existsproduct.FixedDiscountAmount = prd.FixedDiscountAmount;
            existsproduct.MaximumDiscountPercentage = prd.MaximumDiscountPercentage;

            existsproduct.CostPrice = prd.CostPrice;
            existsproduct.SellingPrice = prd.SellingPrice;
            existsproduct.ReOrderLevel = prd.ReOrderLevel;
            existsproduct.ReOrderQuantity = existsproduct.ReOrderQuantity;
            existsproduct.LocationWiseStock = prd.LocationWiseStock;
            existsproduct.WastagePrc = prd.WastagePrc;
            existsproduct.PurchasingUnit = prd.PurchasingUnit;

            existsproduct.ModifiedDate = DateTime.Now;
            //GetExistsproduct(existsproduct).ModifiedDate = DateTime.Now;  show to Chamodi .. :D
            existsproduct.ModifiedUser = Session["loggeduser"].ToString();
            existsproduct.DataTransfer = 0;

            existsproduct.Target_Qty = prd.Target_Qty;
            if(prd.TypeIdTargetPeriod != null)
            {
                existsproduct.TypeIdTargetPeriod = prd.TypeIdTargetPeriod;
            }
            else
            {
                existsproduct.TypeIdTargetPeriod = "";
            }

            if (prd.TypeIdTargetType != null)
            {
                existsproduct.TypeIdTargetType = prd.TypeIdTargetType;
            }
            else
            {
                existsproduct.TypeIdTargetType = "";
            }

            //     existsproduct.Receipes = prd.Receipes;

            existsproduct.ProductLocationViewModel = prd.ProductLocationViewModel;
            existsproduct.ProductLocationViewModel.ForEach(s => { s.ProductId = existsproduct.ProductId; });

            existsproduct.ProductServingUnit = prd.ProductServingUnit;
            existsproduct.ProductTax = prd.ProductTax;
            existsproduct.SupplierProduct = prd.SupplierProduct;
            existsproduct.PrinterTypeId = prd.PrinterTypeId;
            existsproduct.AddonCategoryMasterId = prd.AddonCategoryMasterId;
            existsproduct.AutoProduction = prd.AutoProduction;
            existsproduct.KitchenLocationCount = 0;
            existsproduct.KitchenPrinters_Modl1 = prd.KitchenPrinters_Modl1;
            existsproduct.KitchenPrinters_Modl = prd.KitchenPrinters_Modl;
            var PriceLevelLists = _bllproduct.GetPriceLevelListProductId(Convert.ToInt32(id));
            existsproduct.PriceLevelLists = PriceLevelLists;

            existsproduct.PriceLevelTypes = prd.PriceLevelTypes;
            existsproduct.PriceLevelLists = prd.PriceLevelLists;
            int LocationID = 0;
            int PriceLevelID = 0;
            int UnitID = 0;

            List<InvPriceLevelList> price = new List<InvPriceLevelList>();
            price.Clear();
            foreach (var item in existsproduct.PriceLevelTypes)
            {
                var pricelevels = new InvPriceLevelList();

                LocationID = _bllproduct.GetPrinterByLocation(item.LocationName).SysLocationID;
                PriceLevelID = Convert.ToInt32(_bllproduct.GetPriceLevels(item.PriceLevelName).InvPriceLevelID);
                if (item.ServingUnit != "0" && item.ServingUnit != null)
                {
                    UnitID = Convert.ToInt32(_bllproduct.GetServingUnits(item.ServingUnit).ServingUnitId);
                }
                else
                {
                    UnitID = 0;
                }
                pricelevels.LocationId = LocationID;
                pricelevels.PriceLevelID = PriceLevelID;
                pricelevels.ServingUnitID = UnitID;
                pricelevels.CompanyID = item.CompanyID;
                pricelevels.CostPrice = item.CostPrice;
                pricelevels.CreatedUser = Session["loggeduser"].ToString();
                pricelevels.DataTransfer = 0;
                pricelevels.LocationName = item.LocationName;
                pricelevels.ModifiedUser = Session["loggeduser"].ToString();
                pricelevels.Qty = item.Qty;
                pricelevels.SellingPrice = item.SellingPrice;
                pricelevels.ProductID = Convert.ToInt32(id);
                price.Add(pricelevels);

            }

            existsproduct.PriceLevelLists = price;

            //existsproduct.PriceLevelTypes = prd.PriceLevelTypes;//**
            if (existsproduct.KitchenPrinters_Modl != null)
            {
                List<KitchenPrinterTypes> ktp = new List<KitchenPrinterTypes>();

                if (existsproduct.KitchenPrinters_Modl.Count > 0)
                {

                    foreach (var item in existsproduct.KitchenPrinters_Modl)
                    {
                        var printertype = new KitchenPrinterTypes();
                        printertype.PrinterID = Convert.ToInt32(item.KitchenID);
                        printertype.ProductID = prd.ProductId;
                        printertype.PrinterName = Convert.ToString(item.KitchenPrinterName);
                        printertype.LocationID = 0;
                        printertype.CreatedDate = DateTime.Now;
                        printertype.CreatedUser = Session["loggeduser"].ToString();
                        printertype.ModifiedDate = DateTime.Now;
                        printertype.ModifiedUser = Session["loggeduser"].ToString();
                        ktp.Add(printertype);
                    }

                    if (existsproduct.KitchenPrinters_Modl1 != null)
                    {
                        foreach (var item in existsproduct.KitchenPrinters_Modl1)
                        {
                            var printertype = new KitchenPrinterTypes();
                            printertype.PrinterID = Convert.ToInt32(item.PrinterID);
                            printertype.ProductID = prd.ProductId;
                            printertype.PrinterName = Convert.ToString(item.PrinterName);
                            printertype.LocationID = 0;
                            printertype.CreatedDate = DateTime.Now;
                            printertype.CreatedUser = Session["loggeduser"].ToString();
                            printertype.ModifiedDate = DateTime.Now;
                            printertype.ModifiedUser = Session["loggeduser"].ToString();
                            ktp.Add(printertype);
                        }
                    }

                    existsproduct.KitchenPrinters_Modl1 = ktp;
                }
            }

            if (!string.IsNullOrEmpty(prd.KitchenCode))
            {
                existsproduct.KitchenCode = prd.KitchenCode;
            }
            else
            {
                existsproduct.KitchenCode = string.Empty;
            }

            if (existsproduct.KitchenPrinters_Modl1 != null)
            {
                existsproduct.KitchenPrinters_Modl1.ForEach(k =>
                {
                    k.ModifiedDate = DateTime.Now;
                    k.ModifiedUser = Session["loggeduser"].ToString();
                    k.CreatedUser = Session["loggeduser"].ToString();
                    k.CreatedDate = DateTime.Now;
                    k.ProductID = existsproduct.ProductId;
                }
               );
            }

            existsproduct.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());

            existsproduct.SupplierProduct.ForEach(s =>
            {
                s.ModifiedDate = DateTime.Now;
                s.ModifiedUser = Session["loggeduser"].ToString();
               
            }
            );

       
            existsproduct.ProductTax.ForEach(t =>
            {
                t.ModifiedUser = Session["loggeduser"].ToString();
                t.ModifiedDate = DateTime.Now;
            });

            existsproduct.ProductServingUnit.ForEach(s => { s.ModifiedDate = DateTime.Now; s.ModifiedUser = Session["loggeduser"].ToString(); });

            if (existsproduct.IsTaxInclude)
            {
                existsproduct.IsTax = true;
            }

            if (prd.ProductImage != null)
            {
                byte[] newlogo;
                using (BinaryReader br = new BinaryReader(prd.Photograph.InputStream))
                {
                    newlogo = br.ReadBytes(prd.Photograph.ContentLength);
                    prd.ProductImage = newlogo;
                    prd.ProductImageName = prd.Photograph.FileName;
                    prd.ProductImageType = prd.Photograph.ContentType;
                }

                if (prd.ProductImageName != existsproduct.ProductImageName)
                {
                    byte[] pic;
                    using (BinaryReader br = new BinaryReader(prd.Photograph.InputStream))
                    {
                        pic = br.ReadBytes(prd.Photograph.ContentLength);
                        existsproduct.ProductImage = pic;
                        existsproduct.ProductImageName = prd.Photograph.FileName;
                        existsproduct.ProductImageType = prd.Photograph.ContentType;

                      

                    }
                }
            }
            else
            {
              
                if (prd.Photograph != null)
                {
                    byte[] photo;
                    using (BinaryReader br = new BinaryReader(prd.Photograph.InputStream))
                    {
                        photo = br.ReadBytes(prd.Photograph.ContentLength);
                        existsproduct.ProductImage = photo;
                        existsproduct.ProductImageName = prd.Photograph.FileName;
                        existsproduct.ProductImageType = prd.Photograph.ContentType;

                        try
                        {

                            string DebugFilePath = System.Configuration.ConfigurationManager.AppSettings["ImageFolder"].ToString();

                            if (DebugFilePath != "")
                            {
                                double dblSByte = prd.Photograph.ContentLength;
                                dblSByte = dblSByte / 1024.0;
                                if (50 > dblSByte || dblSByte < 100)
                                {
                                    string OldFilePath = existsproduct.ProductImageName;
                                    string NewFileName = Convert.ToString(existsproduct.ProductCode);
                                    string FileExtension = Path.GetExtension(OldFilePath);
                                    string NewFilePath = Path.Combine(DebugFilePath, Path.GetDirectoryName(OldFilePath), NewFileName + FileExtension);
                                    string name = Path.Combine(Path.GetDirectoryName(OldFilePath), NewFileName + FileExtension);



                                    string _FileName = Path.GetFileName(name);
                                    string _path = Path.Combine(DebugFilePath, _FileName);


                                    if (System.IO.File.Exists(_path))
                                    {
                                        System.IO.File.Delete(_path);
                                    }

                                    prd.Photograph.SaveAs(_path);

                                    existsproduct.ImagePath = NewFilePath;
                                }
                                else
                                {
                                    //ViewBag.Message = "5";
                                    //return View();
                                    ModelState.AddModelError("Photograph", "Image size must be greater than 50KB or less than 100KB !");
                                    Save = false;
                                }
                            }
                            else
                            {
                                ViewBag.Message = "Missing folder path!!";
                                return View();
                            }
                        }
                        catch (Exception e)
                        {
                            ViewBag.Message = "File upload failed!!";
                            return View();
                        }
                    }
                }
            }
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var locations = _blllocation.GetActiveLocations(companyid);
            List<ProductLocationViewModel> vvm = new List<ProductLocationViewModel>();
            if (prd.ProductLocationViewModel.Count > 0)
            {
                locations.ToList().ForEach(l =>
                {
                    var vm = new ProductLocationViewModel();
                    vm.LocationId = l.SysLocationID;
                    vm.Location = l.LocationName;
                    foreach (var pl in prd.ProductLocationViewModel)
                    {
                        if (pl.LocationId == l.SysLocationID)
                        {
                            vm.CostPrice = pl.CostPrice;
                            vm.SellingPrice = pl.SellingPrice;
                            vm.ReOrdderLevel = prd.ReOrderLevel;
                            vm.ReOrderQuantity = pl.ReOrderQuantity;
                            vm.ReOrderPeriod = pl.ReOrderPeriod;
                            vm.MaxPrice = pl.MaxPrice;
                            vm.MinPrice = prd.MinPrice;
                            vm.DiscountPrc = prd.FixedDiscountPercentage;
                            vm.ForignCustomerPrice = pl.ForignCustomerPrice;
                            vm.AverageCost = pl.AverageCost;
                            vm.PrinterType_Id = pl.PrinterType_Id;
                        }
                    }
                    vvm.Add(vm);
                }
                );
                foreach (var p in prd.ProductLocationViewModel)
                {
                    //if (p.CostPrice == 0 && !prd.IsRowMaterial)
                    //{
                    //    ModelState.AddModelError("ProductLocationViewModel", "Please enter cost price !");

                    //    ViewBag.DepartmentId = prd.DepartmentId;
                    //    ViewBag.CategoryId = prd.CategoryId;
                    //    ViewBag.SubCategoryId = prd.SubCategoryId;

                    //    ViewBag.PurchasingUnitId = prd.PurchasingUnit;
                    //    ViewBag.WeightPerUnit = prd.WeightPerUnit;
                    //    ViewBag.PrinterTypeId = prd.PrinterTypeId;
                    //    ViewBag.AddonCategoryMasterId = prd.AddonCategoryMasterId;
                    //    prd.PrinterTypes = _bllproduct.GetPrinterTypes().ToList();
                    //    prd.ProductLocationViewModel = vvm;
                    //    return View(prd);
                    //}
                    //if (p.SellingPrice == 0 && !prd.IsRowMaterial)
                    //{
                    //    ModelState.AddModelError("ProductLocationViewModel", "Please enter selling price !");
                    //    ViewBag.DepartmentId = prd.DepartmentId;
                    //    ViewBag.CategoryId = prd.CategoryId;
                    //    ViewBag.SubCategoryId = prd.SubCategoryId;

                    //    ViewBag.PurchasingUnitId = prd.PurchasingUnit;
                    //    ViewBag.WeightPerUnit = prd.WeightPerUnit;
                    //    ViewBag.PrinterTypeId = prd.PrinterTypeId;
                    //    ViewBag.AddonCategoryMasterId = prd.AddonCategoryMasterId;
                    //    prd.PrinterTypes = _bllproduct.GetPrinterTypes().ToList();
                    //    prd.ProductLocationViewModel = vvm;
                    //    return View(prd);
                    //}
                    //if (p.PrinterType_Id == 0)
                    //{
                    //    @ViewBag.Message = "4";
                    //    ModelState.AddModelError("ProductLocationViewModel", "Please select printer type !");
                    //    ViewBag.DepartmentId = prd.DepartmentId;
                    //    ViewBag.CategoryId = prd.CategoryId;
                    //    ViewBag.SubCategoryId = prd.SubCategoryId;

                    //    ViewBag.PurchasingUnitId = prd.PurchasingUnit;
                    //    ViewBag.WeightPerUnit = prd.WeightPerUnit;
                    //    ViewBag.PrinterTypeId = prd.PrinterTypeId;
                    //    ViewBag.AddonCategoryMasterId = prd.AddonCategoryMasterId;
                    //    prd.PrinterTypes = _bllproduct.GetPrinterTypes().ToList();
                    //    prd.ProductLocationViewModel = vvm;
                    //    return View(prd);
                    //}
                    //if (p.SellingPrice > prd.MaxPrice && !prd.IsRowMaterial)
                    //{
                    //    ModelState.AddModelError("MaxPrice", "Max price cannot be less than the selling !");
                    //    ViewBag.DepartmentId = prd.DepartmentId;
                    //    ViewBag.CategoryId = prd.CategoryId;
                    //    ViewBag.SubCategoryId = prd.SubCategoryId;
                    //    ViewBag.PurchasingUnitId = prd.PurchasingUnit;
                    //    ViewBag.WeightPerUnit = prd.WeightPerUnit;
                    //    ViewBag.PrinterTypeId = prd.PrinterTypeId;
                    //    ViewBag.AddonCategoryMasterId = prd.AddonCategoryMasterId;
                    //    prd.PrinterTypes = _bllproduct.GetPrinterTypes().ToList();
                    //    prd.ProductLocationViewModel = vvm;
                    //    return View(prd);
                    //}
                    //if (p.SellingPrice < prd.MinPrice && !prd.IsRowMaterial)
                    //{
                    //    ModelState.AddModelError("MinPrice", "Minimum price cannot be grater than the selling !");
                    //    ViewBag.DepartmentId = prd.DepartmentId;
                    //    ViewBag.CategoryId = prd.CategoryId;
                    //    ViewBag.SubCategoryId = prd.SubCategoryId;
                    //    ViewBag.PurchasingUnitId = prd.PurchasingUnit;
                    //    ViewBag.WeightPerUnit = prd.WeightPerUnit;
                    //    ViewBag.PrinterTypeId = prd.PrinterTypeId;
                    //    ViewBag.AddonCategoryMasterId = prd.AddonCategoryMasterId;
                    //    prd.PrinterTypes = _bllproduct.GetPrinterTypes().ToList();
                    //    prd.ProductLocationViewModel = vvm;
                    //    return View(prd);
                    //}
                    //if (p.SellingPrice < p.CostPrice && !prd.IsRowMaterial)
                    //{
                    //    ModelState.AddModelError("ProductLocationViewModel", "Cost Price cannot be greater than the Selling Price !");
                    //    ViewBag.DepartmentId = prd.DepartmentId;
                    //    ViewBag.CategoryId = prd.CategoryId;
                    //    ViewBag.SubCategoryId = prd.SubCategoryId;
                    //    ViewBag.PurchasingUnitId = prd.PurchasingUnit;
                    //    ViewBag.WeightPerUnit = prd.WeightPerUnit;
                    //    ViewBag.PrinterTypeId = prd.PrinterTypeId;
                    //    ViewBag.AddonCategoryMasterId = prd.AddonCategoryMasterId;
                    //    prd.PrinterTypes = _bllproduct.GetPrinterTypes().ToList();
                    //    prd.ProductLocationViewModel = vvm;
                    //    return View(prd);
                    //}
                    //if (p.SellingPrice < prd.FixedDiscountAmount && !prd.IsRowMaterial)
                    //{
                    //    ModelState.AddModelError("FixedDiscountAmount", "Discounts cannot exceed the selling price !");
                    //    ViewBag.DepartmentId = prd.DepartmentId;
                    //    ViewBag.CategoryId = prd.CategoryId;
                    //    ViewBag.SubCategoryId = prd.SubCategoryId;
                    //    ViewBag.PurchasingUnitId = prd.PurchasingUnit;
                    //    ViewBag.WeightPerUnit = prd.WeightPerUnit;
                    //    ViewBag.PrinterTypeId = prd.PrinterTypeId;
                    //    ViewBag.AddonCategoryMasterId = prd.AddonCategoryMasterId;
                    //    prd.PrinterTypes = _bllproduct.GetPrinterTypes().ToList();
                    //    prd.ProductLocationViewModel = vvm;
                    //    return View(prd);
                    //}
                }
            }
            else
            {
                locations.ToList().ForEach(
                    lk =>
                    {
                        var vm = new ProductLocationViewModel();
                        vm.LocationId = lk.SysLocationID;
                        vm.Location = lk.LocationName;
                        vvm.Add(vm);
                    }
                );
            }
            if (prd.IsActive == true && prd.IsDelete == true)
            {
                @ViewBag.Message = "5";
                ViewBag.DepartmentId = prd.DepartmentId;
                ViewBag.CategoryId = prd.CategoryId;
                ViewBag.SubCategoryId = prd.SubCategoryId;

                ViewBag.PurchasingUnitId = prd.PurchasingUnit;
                ViewBag.WeightPerUnit = prd.WeightPerUnit;
                ViewBag.PrinterTypeId = prd.PrinterTypeId;
                ViewBag.AddonCategoryMasterId = prd.AddonCategoryMasterId;
                prd.PrinterTypes = _bllproduct.GetPrinterTypes().ToList();
                prd.ProductLocationViewModel = vvm;
                prd.KitchenPrinters_Modl1 = new List<KitchenPrinterTypes>();
                return View(prd);
            }
            if (prd.IsDelete == true)
            {
                var dbReceipes = _bllReceipe.CheckReceipesExistByProductCode(prd.ProductCode, companyid);
                if (dbReceipes.Count() > 0)
                {
                    @ViewBag.Message = "6";
                    ViewBag.DepartmentId = prd.DepartmentId;
                    ViewBag.CategoryId = prd.CategoryId;
                    ViewBag.SubCategoryId = prd.SubCategoryId;

                    ViewBag.PurchasingUnitId = prd.PurchasingUnit;
                    ViewBag.WeightPerUnit = prd.WeightPerUnit;
                    ViewBag.PrinterTypeId = prd.PrinterTypeId;
                    ViewBag.AddonCategoryMasterId = prd.AddonCategoryMasterId;
                    prd.PrinterTypes = _bllproduct.GetPrinterTypes().ToList();
                    prd.ProductLocationViewModel = vvm;
                    prd.KitchenPrinters_Modl1 = new List<KitchenPrinterTypes>();
                    return View(prd);
                }
            }



            if (Save == true)
            {
                if (ModelState.IsValid)
                {
                    ViewBag.Message = _bllproduct.UpdateProduct(existsproduct) ? "1" : "0";
                    ViewBag.PrdCode = prd.ProductCode;

                    ModelState.Clear();
                    // ViewBag.PurchasingUnitId = 0;
                    ViewBag.DepartmentId = 0;
                    ViewBag.CategoryId = 0;
                    ViewBag.SubCategoryId = 0;

                    ViewBag.PurchasingUnitId = 0;
                    ViewBag.WeightPerUnit = 0;
                    ViewBag.PrinterTypeId = 0;
                    ViewBag.AddonCategoryMasterId = 0;
                    var loc = _blllocation.GetActiveLocations(companyid);
                    var nwProduct = new Product();
                    nwProduct.KitchenPrinters_Modl1 = new List<KitchenPrinterTypes>();

                    foreach (var l in loc)
                    {
                        var vm = new ProductLocationViewModel();
                        vm.LocationId = l.SysLocationID;
                        vm.Location = l.LocationName;
                        nwProduct.ProductLocationViewModel.Add(vm);
                    }
                    return View(nwProduct);

                }
                else
                {
                    ViewBag.DepartmentId = prd.DepartmentId;
                    ViewBag.CategoryId = prd.CategoryId;
                    ViewBag.SubCategoryId = prd.SubCategoryId;
                    ViewBag.PurchasingUnitId = prd.PurchasingUnit;
                    ViewBag.WeightPerUnit = prd.WeightPerUnit;
                    ViewBag.PrinterTypeId = prd.PrinterTypeId;
                    ViewBag.AddonCategoryMasterId = prd.AddonCategoryMasterId;
                    prd.PrinterTypes = _bllproduct.GetPrinterTypes().ToList();
                    prd.KitchenPrinters_Modl1 = new List<KitchenPrinterTypes>();
                    prd.ProductLocationViewModel = vvm;
                    return View(prd);
                }
            }
            else
            {
                ViewBag.DepartmentId = prd.DepartmentId;
                ViewBag.CategoryId = prd.CategoryId;
                ViewBag.SubCategoryId = prd.SubCategoryId;
                ViewBag.PurchasingUnitId = prd.PurchasingUnit;
                ViewBag.WeightPerUnit = prd.WeightPerUnit;
                ViewBag.PrinterTypeId = prd.PrinterTypeId;
                ViewBag.AddonCategoryMasterId = prd.AddonCategoryMasterId;
                prd.PrinterTypes = _bllproduct.GetPrinterTypes().ToList();
                prd.KitchenPrinters_Modl1 = new List<KitchenPrinterTypes>();
                prd.ProductLocationViewModel = vvm;
                return View(prd);
            }
        }


        private static Product GetExistsproduct(Product existsproduct)
        {
            return existsproduct;
        }

        [Authorize(Roles = "PrdEdit")]
        [HttpGet]
        public ActionResult Edit(Int32 id)
        {
            var product = _bllproduct.GetProductById(id);
            @ViewBag.FileName = product.ProductImageName;

            List<ProductServingUnit> newsulist = new List<ProductServingUnit>();
            foreach (var s in _bllproduct.GetservingUnitsByProductId(id))
            {
                ProductServingUnit newsu = new ProductServingUnit();
                if (!newsulist.Select(s1 => s1.ServingUnit).Contains(s.ServingUnit))
                {
                    newsu = s;
                    newsulist.Add(newsu);
                }
            }



            //var servingunits = _bllproduct.GetservingUnitsByProductId(id);
            int LocationID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var servingunits = newsulist;
            var producttaxes = _bllproduct.GetProductTaxByProductId(id);
            //var productsuppliers = _bllproduct.GetProductSuppliersByProductId(id);

            // Start - BUG 12617
            // var productsuppliers = _bllproduct.ProductSuppliersByProductId(id, LocationID);
            
            //GET Product saved, Location Id            
            if (product != null)
            {
                int productLocationId = product.LocationId;
                LocationID = productLocationId;
            }
            
            var productsuppliers = _bllproduct.ProductSuppliersByProductId(id, LocationID);

            // End - BUG 12617

            var productlocations = _bllproduct.GetProductStockMasterByProductId(id);
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var locations = _blllocation.GetActiveLocations(companyid);
            var PrinterTypes = _bllproduct.GetProductKitchenPrinterTypesByProductId(id);
            var PriceLevelLists = _bllproduct.GetPriceLevelListProductId(id);

            //recepies.ForEach
            //(
            //    r =>
            //    {
            //        r.ProductName = _productservice.GetProductById(r.MaterialId).ProductName;
            //    }
            //);

            product.PrinterTypes = _bllproduct.GetPrinterTypes().ToList();

            producttaxes.ForEach(
                t => { t.TaxDescription = _blltax.GetTaxById(t.TaxId).TaxName; }
                );
            //Load suppliers to selected product in product (master files>>Item & product>>select the product>> Edit>> suppliers tab
            productsuppliers.ForEach
            (
              s =>
               {
                   s.Supplier = _bllSupplier.GetSupplierById(s.SupplierId).SupplierName;
            }

          );

            PrinterTypes.ForEach
                (
                  k =>
                  {
                      k.PrinterName = _bllproduct.GetPrinterById(k.PrinterID).KitchenPrinterName; // 02
                  }
                );


            List<ProductLocationViewModel> vvm = new List<ProductLocationViewModel>();
            if (productlocations.Count > 0)
            {
                locations.ToList().ForEach(l =>
                {
                    var vm = new ProductLocationViewModel();
                    vm.LocationId = l.SysLocationID;
                    vm.Location = l.LocationName;
                    foreach (var p in productlocations)
                    {
                        if (p.LocationId == l.SysLocationID)
                        {
                            vm.CostPrice = p.CostPrice;
                            vm.SellingPrice = p.SellingPrice;
                            vm.ReOrdderLevel = p.ReOrderLevel;
                            vm.ReOrderQuantity = p.ReOrderQuantity;
                            vm.ReOrderPeriod = p.ReOrderPeriod;
                            vm.MaxPrice = p.MaxPrice;
                            vm.MinPrice = p.MinimumPrice;
                            vm.DiscountPrc = p.DiscountPrc;
                            vm.ForignCustomerPrice = p.ForignCustomerPrice;
                            vm.AverageCost = p.AvgCost;
                            vm.PrinterType_Id = p.PrinterType_Id;
                        }
                    }
                    vvm.Add(vm);
                }
                );
            }
            else
            {
                locations.ToList().ForEach(
                    lk =>
                    {
                        var vm = new ProductLocationViewModel();
                        vm.LocationId = lk.SysLocationID;
                        vm.Location = lk.LocationName;
                        vvm.Add(vm);
                    }
                );
            }

            //  product.Receipes = recepies;
            product.ProductServingUnit = servingunits;
            product.SupplierProduct = productsuppliers;
            product.ProductTax = producttaxes;
            product.ProductLocationViewModel = vvm;
            ViewBag.DepartmentId = product.DepartmentId;
            ViewBag.CategoryId = product.CategoryId;
            ViewBag.SubCategoryId = product.SubCategoryId;
            ViewBag.PrdCode = id;
            ViewBag.PurchasingUnitId = product.PurchasingUnit;
            product.TempProductId = id;
            ViewBag.WeightPerUnit = product.WeightPerUnit;
            ViewBag.PrinterTypeId = product.PrinterTypeId;
            ViewBag.AddonCategoryMasterId = product.AddonCategoryMasterId;
            product.KitchenPrinters_Modl1 = PrinterTypes; //03

            ViewBag.TypeIdTargetPeriod = product.TypeIdTargetPeriod;
            ViewBag.TypeIdTargetType = product.TypeIdTargetType;

            product.PriceLevelLists = PriceLevelLists;

            List<InvPriceLevel> PL = new List<InvPriceLevel>();

            foreach (var p in PriceLevelLists)
            {
                var level = new InvPriceLevel();
                level.LocationName = _bllproduct.GetLocations(p.LocationId).LocationName;
                level.LocationId = p.LocationId;

                level.PriceLevelName = _bllproduct.GetPriceLevelName(p.PriceLevelID).PriceLevelName;
                level.InvPriceLevelID = p.PriceLevelID;


                if (p.ServingUnitID != 0)
                {
                    level.ServingUnit = _bllproduct.GetservingUnitsByPriceLevelId(p.ServingUnitID).ServingUnitName;
                    level.ServingUnitID = p.ServingUnitID;

                }
                else
                {
                    level.ServingUnit = "";
                    level.ServingUnitID = 0;
                }
                level.CostPrice = _bllproduct.GetPriceLevelPriceList(p.InvPriceLevelListID).CostPrice;
                level.SellingPrice = _bllproduct.GetPriceLevelPriceList(p.InvPriceLevelListID).SellingPrice;
                level.Qty = _bllproduct.GetPriceLevelPriceList(p.InvPriceLevelListID).Qty;
                PL.Add(level);
            }

            product.PriceLevelTypes = PL;


            //if (product.OrderType == "NONE")
            //{
            //    ViewBag.OrderType = "0";
            //} else if(product.OrderType=="KOT")
            //{
            //    ViewBag.OrderType = "1";
            //}
            //else if (product.OrderType == "BOT")
            //{
            //    ViewBag.OrderType = "2";
            //}

            //product.ProductId =Convert.ToInt32(id);
            return View(product);
        }

        [HttpGet]
        public JsonResult SubmitData(string selectedValue)
        {
            // Process the data
            var responseMessage = $"Received Selected Value: {selectedValue}";

            return Json(responseMessage);
        }



        [Authorize(Roles = "PrdCreatee")]
        [HttpPost]
        public ActionResult Create(Product product)
        {
            // Stopwatch stopwatch = new Stopwatch();
            // stopwatch.Start();

            // LocationService locationservice = new LocationService();
             

            if (string.IsNullOrEmpty(product.ProductDesp))
            {

                product.ProductDesp = string.Empty;
            }

            if(product.TypeIdTargetPeriod != null)
            {
             
            }
            else
            {
                product.TypeIdTargetPeriod = "";
            }
         

            if(product.TypeIdTargetType != null)
            {
              
            }
            else
            {
                product.TypeIdTargetType = "";
            }
          
            

            product.ProductLocationViewModel.ForEach(
                ps =>
                {
                    ps.Location = _blllocation.GetLocationById(ps.LocationId).LocationName;
                }
            );

            if (string.IsNullOrEmpty(product.KitchenCode))
            {
                //   product.KitchenCode = product.KitchenCode;
                product.KitchenCode = string.Empty;
            }
            //else
            //{
            //    product.KitchenCode = string.Empty;
            //}

            product.CreatedUser = Session["loggeduser"].ToString();
            product.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
            product.IsActive = true;
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            product.CompanyID = companyid;
            product.ModifiedUser = Session["loggeduser"].ToString();

           
            if (_bllproduct.CheckProductCodeExists(product.ProductCode, companyid))
            {
                // ModelState.Clear();
                ViewBag.Message = "2";
                ViewBag.PrdCode = product.ProductCode;
                var loc = _blllocation.GetActiveLocations(companyid);
                return View("Create", product);
            }

            // start - check validations without together
            if (product.SupplierProduct.Count == 0)
            {
                @ViewBag.Message = "3";
                ViewBag.PrdCode = product.ProductCode;
                ViewBag.DepartmentId = product.DepartmentId;
                ViewBag.CategoryId = product.CategoryId;
                ViewBag.SubCategoryId = product.SubCategoryId;
                @ViewBag.PrinterTypeId = product.PrinterTypeId;
                @ViewBag.PurchasingUnitId = product.PurchasingUnit;
                @ViewBag.WeightPerUnit = product.WeightPerUnit;
                var loc = _blllocation.GetActiveLocations(companyid);
                return View("Create", product);
            }            

            // end - check validations without together

            if (product.SupplierProduct.Count == 0 && product.IsRowMaterial)
            {
                @ViewBag.Message = "3";
                ViewBag.PrdCode = product.ProductCode;
                ViewBag.DepartmentId = product.DepartmentId;
                ViewBag.CategoryId = product.CategoryId;
                ViewBag.SubCategoryId = product.SubCategoryId;
                @ViewBag.PrinterTypeId = product.PrinterTypeId;
                @ViewBag.PurchasingUnitId = product.PurchasingUnit;
                @ViewBag.WeightPerUnit = product.WeightPerUnit;
                var loc = _blllocation.GetActiveLocations(companyid);
                return View("Create", product);
            }

            if (product.IsAddon == true && product.AddonCategoryMasterId == 0)
            {
                ModelState.AddModelError("AddonCategoryMasterId", "Please select a Addon Category !");
                return View("Create", product);
            }
            var locations = _blllocation.GetActiveLocations(companyid);
            List<ProductLocationViewModel> vvm = new List<ProductLocationViewModel>();


            if (product.ProductLocationViewModel.Count > 0)
            {
                locations.ToList().ForEach(l =>
                {
                    var vm = new ProductLocationViewModel();
                    vm.LocationId = l.SysLocationID;
                    vm.Location = l.LocationName;
                    foreach (var pl in product.ProductLocationViewModel)
                    {
                        if (pl.LocationId == l.SysLocationID)
                        {
                            vm.CostPrice = pl.CostPrice;
                            vm.SellingPrice = pl.SellingPrice;
                            // vm.ReOrdderLevel = product.ReOrderLevel;
                            vm.ReOrdderLevel = pl.ReOrdderLevel;
                            vm.ReOrderQuantity = pl.ReOrderQuantity;
                            vm.ReOrderPeriod = pl.ReOrderPeriod;
                            vm.MaxPrice = pl.MaxPrice;
                            vm.MinPrice = product.MinPrice;
                            vm.DiscountPrc = product.FixedDiscountPercentage;
                            vm.ForignCustomerPrice = pl.ForignCustomerPrice;
                            vm.AverageCost = pl.CostPrice;
                            vm.PrinterType_Id = pl.PrinterType_Id;
                        }
                    }
                    vvm.Add(vm);
                }
              );
            }
            else
            {
                locations.ToList().ForEach(
                    lk =>
                    {
                        var vm = new ProductLocationViewModel();
                        vm.LocationId = lk.SysLocationID;
                        vm.Location = lk.LocationName;
                        vvm.Add(vm);
                    }
                );
            }
            bool Save = true;
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            if (ModelState.IsValid)
            {
                if (product.Photograph != null)
                {
                    byte[] photo;
                    using (BinaryReader br = new BinaryReader(product.Photograph.InputStream))
                    {
                        photo = br.ReadBytes(product.Photograph.ContentLength);
                        product.ProductImage = photo;
                        product.ProductImageName = product.Photograph.FileName;
                        product.ProductImageType = product.Photograph.ContentType;
                        string debugFilePaths = System.Configuration.ConfigurationManager.AppSettings["ImageFolder"].ToString();
                        try
                        {
                            if (debugFilePaths != "")
                            {
                                double dblSByte = product.Photograph.ContentLength;
                                dblSByte = dblSByte / 1024.0;
                                if (50 > dblSByte   || dblSByte < 100)
                                { 
                                    string oldImageName = product.ProductImageName;
                                    string ProductCode = Convert.ToString(product.ProductCode);

                                    string OldFileName = Path.GetExtension(oldImageName);
                                    string NewImageName = Path.Combine(debugFilePaths, Path.GetDirectoryName(oldImageName), ProductCode + OldFileName);

                                    string imagePath = NewImageName;
                                   // Image loadedImage = Image.FromFile(imagePath);


                                    string _FileName = Path.GetFileName(NewImageName);
                                    string _path = Path.Combine(debugFilePaths, _FileName);
                                    product.Photograph.SaveAs(_path);
                                }
                                else
                                {
                                    //ViewBag.Message = "5";
                                    //return View();
                                    ModelState.AddModelError("Photograph", "Image size must be greater than 50KB or less than 100KB !");
                                    Save = false;
                                }
                            }
                            
                        }
                        catch
                        {
                            ViewBag.Message = "File upload failed!!";
                            return View();
                        }
                    }
                }

                if (product.IsTaxInclude)
                {
                    product.IsTax = true;
                }
                product.ProductLocationViewModel = vvm;
                  
                Product newProduct = new Product();

                product.ProductServingUnit.ForEach(s => { s.CreatedUser = Session["loggeduser"].ToString(); });
                product.SupplierProduct.ForEach(s => { s.CreatedUser = Session["loggeduser"].ToString(); });
                product.ProductTax.ForEach(t => { t.CreatedUser = Session["loggeduser"].ToString(); });


                try
                {
                    string DebugFilePath = System.Configuration.ConfigurationManager.AppSettings["ImageFolder"].ToString();

                    string OldFilePath = product.ProductImageName;
                    string NewFileName = Convert.ToString(product.ProductCode);

                    string FileExtension = Path.GetExtension(OldFilePath);
                    string NewFilePath = Path.Combine(DebugFilePath, Path.GetDirectoryName(OldFilePath), NewFileName + FileExtension);
                    string name = Path.Combine(Path.GetDirectoryName(OldFilePath), NewFileName + FileExtension);

                    product.ImagePath = NewFilePath;
                }
                catch (Exception ex)
                {

                   // throw;
                }                                                                                 

                MemoryStream stream = new MemoryStream();

                //  System.IO.File.Copy(newFilePath, Path.Combine(debugFilePath, Path.GetFileName(newFilePath)), true); 



                if (Save == true)
                {
                    if (_bllproduct.SaveProduct(product))
                    {
                        ModelState.Clear();
                        ViewBag.Message = "1";
                        ViewBag.PrdCode = product.ProductCode;
                        ViewBag.PurchasingUnitId = 0;
                        var loc = _blllocation.GetActiveLocations(companyid);

                        foreach (var l in loc)
                        {
                            var vm = new ProductLocationViewModel();
                            vm.LocationId = l.SysLocationID;
                            vm.Location = l.LocationName;
                            newProduct.ProductLocationViewModel.Add(vm);
                            //this is added by Aruna Start
                            ViewBag.PrdCode = product.ProductCode;
                            ViewBag.DepartmentId = product.DepartmentId;
                            ViewBag.CategoryId = product.CategoryId;
                            ViewBag.SubCategoryId = product.SubCategoryId;
                            ViewBag.PurchasingUnitId = product.PurchasingUnit;
                            ViewBag.WeightPerUnit = product.WeightPerUnit;
                            ViewBag.PrinterTypeId = product.PrinterTypeId;
                            ViewBag.AddonCategoryMasterId = product.AddonCategoryMasterId;
                            product.PrinterTypes = _bllproduct.GetPrinterTypes().ToList();

                            product.ProductCode = "";
                            product.ProductName = "";
                            product.ProductDesp = "";
                            product.NameOnInvoice = "";
                            product.ProductNameInSinhala = "";
                            product.RefCode01 = "";
                            product.RefCode02 = "";
                            product.Barcode = "";
                            product.ProductLocationViewModel = vvm;
                            //this is added by Aruna End
                        }
                    }
                    else
                    {
                        ViewBag.Message = "0";
                        ViewBag.PrdCode = product.ProductCode;
                        ViewBag.DepartmentId = product.DepartmentId;
                        ViewBag.CategoryId = product.CategoryId;
                        ViewBag.SubCategoryId = product.SubCategoryId;
                        ViewBag.PurchasingUnitId = product.PurchasingUnit;
                        ViewBag.WeightPerUnit = product.WeightPerUnit;
                        ViewBag.PrinterTypeId = product.PrinterTypeId;
                        ViewBag.AddonCategoryMasterId = product.AddonCategoryMasterId;
                        product.PrinterTypes = _bllproduct.GetPrinterTypes().ToList();
                        product.ProductLocationViewModel = vvm;
                        //product.ProductServingUnit =
                    }
                }
                // stopwatch.Stop();
                // Console.WriteLine("Elapsed Time is {0} ms", stopwatch.ElapsedMilliseconds);
                //return View("Create", newProduct); //this is the original one
                ViewBag.DepartmentId = product.DepartmentId;
                ViewBag.CategoryId = product.CategoryId;
                ViewBag.SubCategoryId = product.SubCategoryId;
                ViewBag.PurchasingUnitId = product.PurchasingUnit;
                ViewBag.WeightPerUnit = product.WeightPerUnit;
                ViewBag.PrinterTypeId = product.PrinterTypeId;
                ViewBag.AddonCategoryMasterId = product.AddonCategoryMasterId;
                product.PrinterTypes = _bllproduct.GetPrinterTypes().ToList();
                product.ProductLocationViewModel = vvm;
                return View("Create", product);// this is added by Aruna
            }
            else
            {
                ViewBag.DepartmentId = product.DepartmentId;
                ViewBag.CategoryId = product.CategoryId;
                ViewBag.SubCategoryId = product.SubCategoryId;
                ViewBag.PurchasingUnitId = product.PurchasingUnit;
                ViewBag.WeightPerUnit = product.WeightPerUnit;
                ViewBag.PrinterTypeId = product.PrinterTypeId;
                ViewBag.AddonCategoryMasterId = product.AddonCategoryMasterId;
                product.PrinterTypes = _bllproduct.GetPrinterTypes().ToList();
                product.ProductLocationViewModel = vvm;

                // stopwatch.Stop();
                // Console.WriteLine("Elapsed Time is {0} ms", stopwatch.ElapsedMilliseconds);

                return View("Create", product);
            }
        }

        [HttpGet]
        public JsonResult GetActiveProducts()
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var product = _bllproduct.GetActiveProducts(companyid).ToList();
            product.ToList().ForEach(p =>
            {
                if (p.PurchasingUnit != 0)
                {
                    p.UOM = _bllproduct.GetUOMById(p.PurchasingUnit);
                }
            }
            );
            return Json(JsonConvert.SerializeObject(product, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetSubcatByCatId(long id)
        {
            // SubCategoryService reporsitory = new SubCategoryService();
            var subCategories1 = _bllsubcategory.GetByCategoryId(id);
            return Json(JsonConvert.SerializeObject(subCategories1, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetMenuByDeptId(long id)
        {
            var deptdata = _bllproduct.GetMenuByDepartmentId(id);
            return Json(JsonConvert.SerializeObject(deptdata, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetProductsByDeptId(long id)
        {
            var deptdata = _bllproduct.GetProductByDepartmentId(id, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            return Json(JsonConvert.SerializeObject(deptdata, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetMenuByDeptCatId(long deptid, long catid)
        {
            var deptcatdata = _bllproduct.GetMenuByDeptCatId(deptid, catid);
            return Json(JsonConvert.SerializeObject(deptcatdata, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetMenuByDeptCatScatId(long deptid, long catid, long scatid)
        {
            var deptcatscatdata = _bllproduct.GetMenuByDeptCatSCatId(deptid, catid, scatid);
            return Json(JsonConvert.SerializeObject(deptcatscatdata, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetActiveSubCategories()
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var actsubCategories = _bllsubcategory.GetActiveSubCategories(companyid);
            return Json(JsonConvert.SerializeObject(actsubCategories, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetLocationWiseProduct(long frmloc, long toloc)
        {
            List<ProductStockMasterViewModel> vvm = new List<ProductStockMasterViewModel>();
            vvm = _bllproduct.GetLocationProductsByLocId(frmloc, toloc);
            //  return Json(vvm);
            return Json(JsonConvert.SerializeObject(vvm, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }
       
        [HttpGet]
        public JsonResult GetLocationWiseMenues(long frmloc, long toloc, string foo)
        {
            List<ProductStockMasterViewModel> vvm = new List<ProductStockMasterViewModel>();

            // vvm = _bllproduct.GetLocationMenuesByLocId(frmloc, toloc, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            if (foo == "Production")
            {
                vvm = _bllproduct.GetLocationMenuesByLocId(frmloc, toloc,
                    Convert.ToInt32(Session["loggedusercompanyId"].ToString())
                    ).Where(p => p.IsRowMaterial == false).ToList();
            }
            else if (foo == "TOG")
            {
                vvm = _bllproduct.GetLocationMenuesByLocId(frmloc, toloc,
                  Convert.ToInt32(Session["loggedusercompanyId"].ToString())
                  );
            }
            else if (foo == "PO")
            {
              
                if (_bllconfiguration.GetConfiguration("WithoutRowMaterialInPOBasedRequestNote", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn)
                {
                    vvm = _bllproduct.GetLocationMenuesByLocId(frmloc, toloc,
                  Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                }
                else
                {
                    vvm = _bllproduct.GetLocationMenuesByLocId(frmloc, toloc,
               Convert.ToInt32(Session["loggedusercompanyId"].ToString())
              ).Where(p => p.IsRowMaterial == true).ToList();
                }
            }
            return Json(JsonConvert.SerializeObject(vvm, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetLocationProduct(long locid)
        {
            List<ProductStockMasterViewModel> vvm = new List<ProductStockMasterViewModel>();

            _bllproduct.GetProductsByLocId(locid).ForEach(

                p =>
                {
                    ProductStockMasterViewModel vm = new ProductStockMasterViewModel();
                    vm.ProductId = p.ProductId;
                    vm.ProductName = p.ProductName;
                    vm.UOM = _bllproduct.GetUOMById(_bllproduct.GetProductById(vm.ProductId).PurchasingUnit);
                    vvm.Add(vm);
                }
            );
            return Json(JsonConvert.SerializeObject(vvm, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetLocationRowMaterials(long locid)
        {
            //List<ProductStockMasterViewModel> vvm = new List<ProductStockMasterViewModel>();

            //_productservice.GetProductsByLocId(locid).ForEach(

            //    p => {
            //        ProductStockMasterViewModel vm = new ProductStockMasterViewModel();
            //        vm.ProductId = p.ProductId;
            //        vm.ProductName = p.ProductName;
            //        vm.UOM = _productservice.GetUOMById(_productservice.GetProductById(vm.ProductId).PurchasingUnit);
            //        vvm.Add(vm);

            //    }
            //);

            return Json(JsonConvert.SerializeObject(_bllproduct.GetRowMaterialsByLocId(locid), Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetProductionItems(long locid)
        {
            var nonrowmaterials = _bllproduct.GetProductionItems(locid, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            return Json(JsonConvert.SerializeObject(nonrowmaterials, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        ProductStockViewModel getallProduct()
        {
            // RPTStockLoadingModel modeData = new RPTStockLoadingModel();
            ProductStockViewModel modeData = new ProductStockViewModel();

            //
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            // var stock = _bllproduct.GetActiveProducts(1);
            var stock = _bllproduct.GetStockReport(0, 0, companyid, "0", "0").Where(s => s.Stock != 0);
            foreach (var s in stock)
            {
                ProductStockViewModel v = new ProductStockViewModel();
                v.ProductId = s.ProductId;
                v.ProductName = s.ProductName;
                //v.Location = _blllocation.GetLocationById(s.LocationId).LocationName;
                v.ProductDbStock = s.Stock;
                v.ProductCode = s.ProductCode;
                v.ProductCostPrice = s.CostPrice;
                v.ProductSellingPrice = s.SellingPrice;

                //v.AverageCostPrice = s.AvgCost; //--- Added By Nipuna Francisku #2619
                // v.AverageCostValue = (s.AvgCost * s.Stock);  //--- Added By Nipuna Francisku #2619
                modeData.stockmodel.Add(v);
            }
            return modeData;

        }


        [Authorize(Roles = "Reports")]
        public ActionResult RPTStockDetails()
        {
            if (_bllconfiguration.GetConfiguration("UReports", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn)
            {
                if (!_appmanager.CheckReportPermissions(999, Session["loggeduserempcode"].ToString(), "RPTStockDetails"))
                {
                    @ViewBag.Permissions = "No user permissions to View Stock Details";
                    return View("~/Views/Account/AccessDenied.cshtml");
                }
            }

            // RPTStockLoadingModel modeData = new RPTStockLoadingModel();
            ProductStockViewModel modeData = new ProductStockViewModel();

            ////
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            //// var stock = _bllproduct.GetActiveProducts(1);
            //var stock = _bllproduct.GetStockReport(0, 0, companyid, "0", "0").Where(s => s.Stock != 0);
            //foreach (var s in stock)
            //{
            //    ProductStockViewModel v = new ProductStockViewModel();
            //    v.ProductId = s.ProductId;
            //    v.ProductName = s.ProductName;
            //    //v.Location = _blllocation.GetLocationById(s.LocationId).LocationName;
            //    v.ProductDbStock = s.Stock;
            //    v.ProductCode = s.ProductCode;
            //    v.ProductCostPrice = s.CostPrice;
            //    v.ProductSellingPrice = s.SellingPrice;

            //    //v.AverageCostPrice = s.AvgCost; //--- Added By Nipuna Francisku #2619
            //    // v.AverageCostValue = (s.AvgCost * s.Stock);  //--- Added By Nipuna Francisku #2619
            //    modeData.stockmodel.Add(v);
            //}

            //
            modeData = getallProduct();
            ViewBag.productdata = _bllproduct.GetActiveProducts(companyid).Select(s => new
            { s.ProductId, s.ProductCode, s.ProductName, s.LocationId }
            );

            TempData["PopUpProductLoad"] = modeData;

            return View("~/Views/Reports/Stock/RPTStockDetails.cshtml", modeData);
        }

        [Authorize(Roles = "Reports")]
        public ActionResult RPTReceipe()
        {
            if (_bllconfiguration.GetConfiguration("UReports", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn)
            {
                if (!_appmanager.CheckReportPermissions(999, Session["loggeduserempcode"].ToString(), "RPTReceipe"))
                {
                    @ViewBag.Permissions = "No user permissions to View Recipes";
                    return View("~/Views/Account/AccessDenied.cshtml");
                }
            }
            return View("~/Views/Reports/Stock/RPTReceipe.cshtml", new Receipe());
        }

        [Authorize(Roles = "Reports")]
        [HttpPost]
        public ActionResult RPTStockDetails(ProductStockViewModel vvm)
        {

            vvm.StockCodeFrom = Request.Form["StockCodeFrom"];
            string principle = Request["StockCodeFrom"].ToString();

            TempData["SelectionCriteria"] = vvm;//rerquire values for excel file generation

            if (_bllconfiguration.GetConfiguration("UReports", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn)
            {
                if (!_appmanager.CheckReportPermissions(999, Session["loggeduserempcode"].ToString(), "RPTStockDetails"))
                {
                    @ViewBag.Permissions = "No user permissions to View Stock Details";
                    return View("~/Views/Account/AccessDenied.cshtml");
                }
            }


            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            ViewBag.productdata = _bllproduct.GetActiveProducts(companyid).Select(s => new
            {
                s.ProductId,
                s.ProductCode,
                s.ProductName,
                s.LocationId


            }
            );
            if (vvm.StockCodeFrom == "" || vvm.StockCodeFrom == null)
            {
                vvm.StockCodeFrom = "0";
            }
            else if (vvm.StockCodeTO == "" || vvm.StockCodeTO == null)
            {
                vvm.StockCodeTO = "0";
            }

            var stock = _bllproduct.GetStockReport(vvm.LocationId, vvm.ProductId, companyid, vvm.StockCodeFrom, vvm.StockCodeTO).Where(s => s.Stock != 0);
            // List<ProductStockViewModel> stockmodel = new List<ProductStockViewModel>();

            ProductStockViewModel stockmodel = new ProductStockViewModel();
            decimal totalCost = 0, AverageCostValue = 0;
            foreach (var s in stock)
            {
                ProductStockViewModel v = new ProductStockViewModel();
                v.ProductId = s.ProductId;
                v.ProductName = s.ProductName;
                v.Location = _blllocation.GetLocationById(s.LocationId).LocationName;
                v.ProductDbStock = s.Stock;
                v.ProductCode = s.ProductCode;
                v.ProductCostPrice = s.CostPrice;
                v.AverageCostPrice = s.AvgCost; //--- Added By Nipuna Francisku #2619
                v.AverageCostValue = (s.AvgCost * s.Stock);  //--- Added By Nipuna Francisku #2619

                totalCost += s.Stock * s.CostPrice; // added by aruna
                AverageCostValue += s.AvgCost * s.Stock; // added by thebuwana

                stockmodel.stockresultmodel.Add(v);
            }
            ViewBag.TotalCost = totalCost;// added by aruna total cost passing to view as a view bag
            ViewBag.TotalAverageCost = AverageCostValue;
            if (stockmodel.stockresultmodel.Count > 0)
            {
                if (vvm.LocationId == 0 && vvm.ProductId == 0) { @ViewBag.ReportSummary = "All Products at All Locations"; }
                if (vvm.LocationId != 0 && vvm.ProductId == 0) { @ViewBag.ReportSummary = "All Products at Location: " + stockmodel.stockresultmodel.First().Location; }
                if (vvm.LocationId == 0 && vvm.ProductId != 0) { @ViewBag.ReportSummary = "Product: " + stockmodel.stockresultmodel.First().ProductName + " in every location"; }
                if (vvm.LocationId != 0 && vvm.ProductId != 0) { @ViewBag.ReportSummary = "Product: " + stockmodel.stockresultmodel.First().ProductName + " in Location : " + stockmodel.stockresultmodel.First().Location; }
            }
            else
            {
                @ViewBag.ReportSummary = "No stock exists in this location";
            }
            @ViewBag.ProductId = vvm.ProductId;
            @ViewBag.LocationId = vvm.LocationId;
            //TempData["SelectionCriteria"] = vvm;//rerquire values for excel file generation
            StockViewModelSelection = vvm;//rerquire values for excel file generation

            ProductStockViewModel TList = new ProductStockViewModel();
            if (TempData["PopUpProductLoad"] != null)
            {
                TList = (ProductStockViewModel)TempData["PopUpProductLoad"];
                stockmodel.stockmodel = TList.stockmodel;
            }
            else
            {
                TList = getallProduct();
                stockmodel.stockmodel = TList.stockmodel;

            }
            TempData["ReportData"] = stockmodel;



            return View("~/Views/Reports/Stock/RPTStockDetails.cshtml", stockmodel);
        }

        public FileResult DownloadFile(string fileName)
        {
            string path = Server.MapPath("~/docs/") + fileName;
            byte[] bytes = System.IO.File.ReadAllBytes(path);

            return File(bytes, "application/octet-stream", fileName);
        }

        [HttpPost]
        public ActionResult VerifyExcel()
        {
            Session["ProductUploadedFile"] = null;
            List<DataUploadProductViewModel> listproductvm = new List<DataUploadProductViewModel>();
            List<DataUploadProductViewModel> invalidproductlist = new List<DataUploadProductViewModel>();

            List<DataUploadRecipePriceChangeViewModel> listrecipepricevm = new List<DataUploadRecipePriceChangeViewModel>();
            List<DataUploadRecipePriceChangeViewModel> invalidrecipepricelist = new List<DataUploadRecipePriceChangeViewModel>();

            List<DataUploadProductTaxViewModel> listproducttaxvm = new List<DataUploadProductTaxViewModel>();
            List<DataUploadProductTaxViewModel> invalidproducttaxlist = new List<DataUploadProductTaxViewModel>();

            List<DataUploadRecipeViewModel> listrecipeuploadvm = new List<DataUploadRecipeViewModel>();
            List<DataUploadRecipeViewModel> listinvalidrecipeuploadvm = new List<DataUploadRecipeViewModel>();

            if (Request != null)
            {
                HttpPostedFileBase file = Request.Files["ProductUploadFile"];
                ViewBag.ExcelFile = file.FileName;
                if (file.FileName == string.Empty)
                {
                    
                    ViewBag.statuscode = 0;
                    ViewBag.status = "Browse the Excel File and Verify..";
                    return View("~/Views/Product/UploadProducts.cshtml", new DataUploadViewModel());
                }
                if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                {
                    Session["ProductUploadedFile"] = file;
                    string fileName = file.FileName;
                    string fileContentType = file.ContentType;
                    byte[] fileBytes = new byte[file.ContentLength];
                    var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));

                    using (var package = new ExcelPackage(file.InputStream))
                    {
                        var sheets = package.Workbook.Worksheets;
                        foreach (var currentSheet in sheets)
                        {
                            var worksheet = currentSheet;
                            int colcount = 0;
                            int rowcount = 0;
                            if (worksheet.Dimension != null)
                            {
                                colcount = worksheet.Dimension.End.Column;
                                rowcount = worksheet.Dimension.End.Row;
                            }
                            for (int rowIterator = 2; rowIterator <= rowcount; rowIterator++)
                            {
                                if (worksheet.Name == "ProductSupplierLocation")
                                {
                                    DataUploadProductViewModel productvm = new DataUploadProductViewModel();
                                    productvm.LineNo = rowIterator;
                                    productvm.DepartmentCode = Convert.ToString(worksheet.Cells[rowIterator, 6].Value).Trim();
                                    productvm.RstCategoryCode = Convert.ToString(worksheet.Cells[rowIterator, 7].Value).Trim();
                                    productvm.RstSubCategoryCode = Convert.ToString(worksheet.Cells[rowIterator, 8].Value).Trim();
                                    productvm.UnitOfMeasureCode = Convert.ToString(worksheet.Cells[rowIterator, 9].Value).Trim();
                                    productvm.SubUnit = Convert.ToString(worksheet.Cells[rowIterator, 10].Value).Trim();
                                    productvm.SupplierCode = Convert.ToString(worksheet.Cells[rowIterator, 23].Value).Trim();
                                    productvm.LocationCode = Convert.ToString(worksheet.Cells[rowIterator, 24].Value).Trim();
                                    listproductvm.Add(productvm);
                                }
                                else if (worksheet.Name == "RecipeUpload")
                                {
                                    DataUploadRecipeViewModel recipevm = new DataUploadRecipeViewModel();
                                    recipevm.LineNo = rowIterator;
                                    recipevm.LocationCode = Convert.ToString(worksheet.Cells[rowIterator, 1].Value).Trim();
                                    recipevm.ProductCode = Convert.ToString(worksheet.Cells[rowIterator, 2].Value).Trim();
                                    recipevm.ServingUint = Convert.ToString(worksheet.Cells[rowIterator, 3].Value).Trim();

                                    var pq = worksheet.Cells[rowIterator, 4].Value;
                                    if (pq == null) { pq = 0; }
                                    recipevm.ProductQuantity = Convert.ToDecimal(pq);

                                    var sp = worksheet.Cells[rowIterator, 5].Value;
                                    if (sp == null) { sp = 0; }
                                    recipevm.SellingPrice = Convert.ToDecimal(sp);

                                    recipevm.MaterialCode = Convert.ToString(worksheet.Cells[rowIterator, 6].Value).Trim();

                                    var mq = worksheet.Cells[rowIterator, 5].Value;
                                    if (mq == null) { sp = 0; }
                                    recipevm.MaterialQuantity = Convert.ToDecimal(mq);

                                    recipevm.SubUnit = Convert.ToString(worksheet.Cells[rowIterator, 8].Value).Trim();

                                    listrecipeuploadvm.Add(recipevm);
                                }
                                else if (worksheet.Name == "ProductTaxes")
                                {
                                    DataUploadProductTaxViewModel prdtaxvm = new DataUploadProductTaxViewModel();
                                    prdtaxvm.LineNo = rowIterator;
                                    prdtaxvm.ProductCode = Convert.ToString(worksheet.Cells[rowIterator, 1].Value).Trim();
                                    prdtaxvm.TaxCode = Convert.ToString(worksheet.Cells[rowIterator, 2].Value).Trim();

                                    listproducttaxvm.Add(prdtaxvm);
                                }
                                else if (worksheet.Name == "RecipePriceChange")
                                {
                                    DataUploadRecipePriceChangeViewModel recipevm = new DataUploadRecipePriceChangeViewModel();
                                    recipevm.LineNo = rowIterator;
                                    recipevm.ProductCode = Convert.ToString(worksheet.Cells[rowIterator, 1].Value).Trim();
                                    recipevm.ServingUint = Convert.ToString(worksheet.Cells[rowIterator, 2].Value).Trim();
                                    recipevm.LocationCode = Convert.ToString(worksheet.Cells[rowIterator, 3].Value).Trim();
                                    var cp = worksheet.Cells[rowIterator, 4].Value;
                                    if (cp == null) { cp = 0; }
                                    recipevm.CostPrice = Convert.ToDecimal(cp);

                                    var sp = worksheet.Cells[rowIterator, 5].Value;
                                    if (sp == null) { sp = 0; }
                                    recipevm.SellingPrice = Convert.ToDecimal(0);
                                    listrecipepricevm.Add(recipevm);
                                }
                            }
                        }
                    }
                    int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
                    var dbdepartments = _blldepartment.GetActiveDepartments(companyid).Select(d => d.DepartmentCode);
                    var dbcategories = _bllcategory.GetActiveCategory(companyid).Select(c => c.RstCategoryCode);
                    var dbsubcategories = _bllsubcategory.GetActiveSubCategories(companyid).Select(c => c.RstSubCategoryCode);
                    var dbuom = _bllunitofmeasure.GetActiveUnitOfMeasures(companyid).Select(u => u.UnitOfMeasureCode);
                    var dbsubunits = _bllunitconversion.GetUnitConversions(companyid).Select(su => su.SubUnit);
                    var dbsupplieres = _bllSupplier.GetActiveSuppliers(companyid).Select(s => s.SupplierCode);
                    var dblocations = _blllocation.GetActiveLocations(companyid).Select(l => l.LocationCode);
                    var dbservingunits = _bllServingUnits.GetActiveServingUnits(companyid).Select(su => su.ServingUnitName);
                    var dbproducts = _bllproduct.GetFinishGoods(companyid).Select(p => p.ProductCode);        // isrowmaterial=0
                    var dballproducts = _bllproduct.GetActiveProducts(companyid).Select(p => p.ProductCode);
                    var dbtaxes = _blltax.GetActiveTaxes(companyid).Select(t => t.TaxCode);

                    foreach (var p in listproductvm)
                    {
                        if (!dbdepartments.Contains(p.DepartmentCode))
                        {
                            p.InCorrectDepartmentCode = true;
                        }

                        if (!dbcategories.Contains(p.RstCategoryCode))
                        {
                            p.InCorrectRstCategoryCode = true;
                        }

                        if (!dbsubcategories.Contains(p.RstSubCategoryCode))
                        {
                            p.InCorrectRstSubCategoryCode = true;
                        }

                        if (!dbuom.Contains(p.UnitOfMeasureCode))
                        {
                            p.InCorrectUnitOfMeasureName = true;
                        }

                        if (!dbsubunits.Contains(p.SubUnit))
                        {
                            p.InCorrectSubUnit = true;
                        }

                        if (!dbsupplieres.Contains(p.SupplierCode))
                        {
                            p.InCorrectSuppliereCode = true;
                        }

                        if (!dblocations.Contains(p.LocationCode))
                        {
                            p.InCorrectLocationCode = true;
                        }

                        if (p.InCorrectDepartmentCode == true || p.InCorrectRstCategoryCode == true ||
                            p.InCorrectRstSubCategoryCode == true || p.InCorrectUnitOfMeasureName == true ||
                            p.InCorrectSubUnit == true || p.InCorrectSuppliereCode == true || p.InCorrectLocationCode == true)
                        {
                            invalidproductlist.Add(p);
                        }
                    }

                    foreach (var rp in listrecipepricevm)
                    {
                        if (!dbproducts.Contains(rp.ProductCode))
                        {
                            rp.InCorrectProductCode = true;
                        }
                        if (!dbservingunits.Contains(rp.ServingUint))
                        {
                            rp.InCorrectServingUnit = true;
                        }
                        if (!dblocations.Contains(rp.LocationCode))
                        {
                            rp.InCorrectLocationCode = true;
                        }

                        if (rp.InCorrectLocationCode == true || rp.InCorrectProductCode == true || rp.InCorrectServingUnit)
                        {
                            invalidrecipepricelist.Add(rp);
                        }
                    }

                    foreach (var pt in listproducttaxvm)
                    {
                        if (!dballproducts.Contains(pt.ProductCode))
                        {
                            pt.InCorrectProductCode = true;
                        }
                        if (!dbtaxes.Contains(pt.TaxCode))
                        {
                            pt.InCorrectTaxCode = true;
                        }

                        if (pt.InCorrectProductCode == true || pt.InCorrectTaxCode == true)
                        {
                            invalidproducttaxlist.Add(pt);
                        }
                    }

                    foreach (var rec in listrecipeuploadvm)
                    {
                        if (!dballproducts.Contains(rec.ProductCode))
                        {
                            rec.InCorrectProductCode = true;
                        }
                        if (!dblocations.Contains(rec.LocationCode))
                        {
                            rec.InCorrectLocationCode = true;
                        }
                        if (!dbservingunits.Contains(rec.ServingUint))
                        {
                            rec.InCorrectServingUnitCode = true;
                        }
                        if (!dballproducts.Contains(rec.MaterialCode))
                        {
                            rec.InCorrectMaterialCode = true;
                        }
                        if (!dbsubunits.Contains(rec.SubUnit))
                        {
                            rec.InCorrectSubUnitCode = true;
                        }

                        if (rec.InCorrectLocationCode == true || rec.InCorrectProductCode == true || rec.InCorrectServingUnitCode == true ||
                            rec.InCorrectMaterialCode == true || rec.InCorrectSubUnitCode == true)
                        {
                            listinvalidrecipeuploadvm.Add(rec);
                        }
                    }
                }
            }

            DataUploadViewModel vmdataupload = new DataUploadViewModel();
            vmdataupload.WithData = true;
            if (invalidproductlist.Count != 0)
            {
                vmdataupload.VerifyMessage = "Invalid Referance Codes Detected..!";
                vmdataupload.ProductTaxVerified = false;
                vmdataupload.DataUploadProductViewModel = invalidproductlist;
            }
            else
            {
                vmdataupload.VerifyMessage = "Excel Sheet Verified..!";
                vmdataupload.ProductTaxVerified = true;
                vmdataupload.DataUploadProductViewModel = listproductvm;

            }
           

            if (invalidrecipepricelist.Count != 0)
            {
                vmdataupload.VerifyMessage = "Invalid Referance Codes Detected..!";
                vmdataupload.ProductTaxVerified = false;
                vmdataupload.DataUploadRecipePriceChangeViewModel = invalidrecipepricelist;
            }
            else
            {
                vmdataupload.VerifyMessage = "Excel Sheet Verified..!";
                vmdataupload.ProductTaxVerified = true;
                vmdataupload.DataUploadRecipePriceChangeViewModel = listrecipepricevm;
            }
           // vmdataupload.DataUploadRecipePriceChangeViewModel = invalidrecipepricelist;

            if (invalidproducttaxlist.Count != 0)
            {
                vmdataupload.VerifyMessage = "Invalid Referance Codes Detected..!";
                vmdataupload.ProductTaxVerified = false;
                vmdataupload.DataUploadProductTaxViewModel = invalidproducttaxlist;
            }
            else
            {
                vmdataupload.VerifyMessage = "Excel Sheet Verified..!";
                vmdataupload.ProductTaxVerified = true;
                vmdataupload.DataUploadProductTaxViewModel = listproducttaxvm;
            }
            //vmdataupload.DataUploadProductTaxViewModel = invalidproducttaxlist;

            if (listinvalidrecipeuploadvm.Count != 0)
            {
                vmdataupload.VerifyMessage = "Invalid Referance Codes Detected..!";
                vmdataupload.ProductTaxVerified = false;
                vmdataupload.DataUploadRecipeViewModel = listinvalidrecipeuploadvm;
            }
            else
            {
                vmdataupload.VerifyMessage = "Excel Sheet Verified..!";
                vmdataupload.ProductTaxVerified = true;
                vmdataupload.DataUploadRecipeViewModel = listrecipeuploadvm;
            }
         //   vmdataupload.DataUploadRecipeViewModel = listinvalidrecipeuploadvm;

         
            return View("~/Views/Product/UploadProducts.cshtml", vmdataupload);
        }

        [HttpPost]
        public ActionResult VerifyProductPriceChnageExcel()
        {
            List<DataUploadProductViewModel> listproductvm = new List<DataUploadProductViewModel>();
            List<DataUploadProductViewModel> invalidproductlist = new List<DataUploadProductViewModel>();

            List<DataUploadRecipePriceChangeViewModel> listrecipepricevm = new List<DataUploadRecipePriceChangeViewModel>();
            List<DataUploadRecipePriceChangeViewModel> invalidrecipepricelist = new List<DataUploadRecipePriceChangeViewModel>();

            List<DataUploadProductTaxViewModel> listproducttaxvm = new List<DataUploadProductTaxViewModel>();
            List<DataUploadProductTaxViewModel> invalidproducttaxlist = new List<DataUploadProductTaxViewModel>();

            List<DataUploadRecipeViewModel> listrecipeuploadvm = new List<DataUploadRecipeViewModel>();
            List<DataUploadRecipeViewModel> listinvalidrecipeuploadvm = new List<DataUploadRecipeViewModel>();
            DataTable dt = new DataTable();

            if (Request != null)
            {
                HttpPostedFileBase file = Request.Files["ProductUploadFile"];
                ViewBag.ExcelFile = file.FileName;
                if (file.FileName == string.Empty)
                {
                          ViewBag.statuscode = 0;
                          ViewBag.status = "Browse the Excel File and Verify..";
                            return View("~/Views/Product/UploadProductsPriceChnage.cshtml", new DataUploadViewModel());
                }
                if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                {
                    string fileName = file.FileName;
                    string fileContentType = file.ContentType;
                    byte[] fileBytes = new byte[file.ContentLength];
                    var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));

                             using (var package = new ExcelPackage(file.InputStream))
                          {
                             var sheets = package.Workbook.Worksheets;
                             foreach (var currentSheet in sheets)
                             {
                            var worksheet = currentSheet;
                              int colcount = 0;
                             int rowcount = 0;
                                if (worksheet.Dimension != null)
                                {
                                     colcount = worksheet.Dimension.End.Column;
                                     rowcount = worksheet.Dimension.End.Row;
                                }
                                  for (int rowIterator = 2; rowIterator <= rowcount; rowIterator++)
                                {
                                  if (worksheet.Name == "Product Price Change")
                                       {
                                    List<string> errorMessages = new List<string>();
                                    DataUploadProductViewModel productvm = new DataUploadProductViewModel();

                                    // Check for null or empty values
                                    if (worksheet.Cells[rowIterator, 1].Value == null) { }
                                    // errorMessages.Add($"Line {rowIterator}: LocationID is required.");
                                    else
                                    {
                                        int Loc = 0;
                                        int.TryParse(worksheet.Cells[rowIterator, 1].Value.ToString(), out Loc);
                                        if (Loc > 0)
                                        { 
                                            productvm.LocationID = Loc;// Convert.ToInt32(worksheet.Cells[rowIterator, 1].Value);
                                        }
                                        else
                                        {
                                            errorMessages.Add($"This Line {rowIterator}: LocationID Cannot Enter - '" + worksheet.Cells[rowIterator, 1].Value.ToString() + "'");
                                        }
                                    }

                                    if (worksheet.Cells[rowIterator, 2].Value == null) { }
                                    // errorMessages.Add($"Line {rowIterator}: ProductId is required.");
                                    else
                                    {
                                        int ProID = 0;
                                        bool isParsed = int.TryParse(worksheet.Cells[rowIterator, 2].Value.ToString(), out ProID);
                                        if (isParsed)
                                        {
                                            productvm.ProductId = Convert.ToInt32(worksheet.Cells[rowIterator, 2].Value);
                                        }
                                        else
                                        {
                                            errorMessages.Add($"This Line {rowIterator}: ProductID Cannot Enter - '" + worksheet.Cells[rowIterator, 2].Value.ToString() + "'");
                                        }
                                    }

                                    if (string.IsNullOrWhiteSpace(Convert.ToString(worksheet.Cells[rowIterator, 3].Value))) { }
                                    //  errorMessages.Add($"Line {rowIterator}: ProductCode is required.");
                                    else
                                        productvm.ProductCode = Convert.ToString(worksheet.Cells[rowIterator, 3].Value).Trim();

                                    if (string.IsNullOrWhiteSpace(Convert.ToString(worksheet.Cells[rowIterator, 4].Value))) { }
                                    // errorMessages.Add($"Line {rowIterator}: ProductName is required.");
                                    else
                                        productvm.ProductName = Convert.ToString(worksheet.Cells[rowIterator, 4].Value).Trim();

                                    if (worksheet.Cells[rowIterator, 5].Value == null) { }
                                    //  errorMessages.Add($"Line {rowIterator}: IsRawMaterial is required.");
                                    else
                                    {
                                        
                                            productvm.IsRawMaterial = Convert.ToBoolean(worksheet.Cells[rowIterator, 5].Value);
                                        
                                    }

                                    if (worksheet.Cells[rowIterator, 6].Value == null) { }
                                    // errorMessages.Add($"Line {rowIterator}: IsActive is required.");
                                    else
                                    {
                                         
                                            productvm.IsActive = Convert.ToBoolean(worksheet.Cells[rowIterator, 6].Value);
                                        
                                    }

                                    if (worksheet.Cells[rowIterator, 7].Value == null) { }
                                    // errorMessages.Add($"Line {rowIterator}: DepartmentID is required.");
                                    else
                                    {
                                        
                                            productvm.DepartmentID = Convert.ToInt32(worksheet.Cells[rowIterator, 7].Value);
                                       
                                    }

                                    if (worksheet.Cells[rowIterator, 8].Value == null) { }
                                    // errorMessages.Add($"Line {rowIterator}: CategoryID is required.");
                                    else
                                    {
                                        
                                            productvm.CategoryID = Convert.ToInt32(worksheet.Cells[rowIterator, 8].Value);
                                        
                                    }

                                    if (worksheet.Cells[rowIterator, 9].Value == null) { }
                                    //  errorMessages.Add($"Line {rowIterator}: SubCategoryID is required.");
                                    else
                                    {
                                       
                                            productvm.SubCategoryID = Convert.ToInt32(worksheet.Cells[rowIterator, 9].Value);
                                         
                                    }

                                    if (worksheet.Cells[rowIterator, 10].Value == null) { }
                                    //  errorMessages.Add($"Line {rowIterator}: CostPrice is required.");
                                    else
                                    {
                                        decimal cost = 0;
                                        bool isParsed = decimal.TryParse(worksheet.Cells[rowIterator, 10].Value?.ToString(), out cost);

                                        if (isParsed)
                                        {
                                            productvm.CostPrice = cost;
                                        }
                                        else
                                        {
                                            string cellValue = worksheet.Cells[rowIterator, 10].Value?.ToString() ?? "NULL";
                                            errorMessages.Add($"This Line {rowIterator}: CostPrice Cannot Enter - '{cellValue}'");
                                        }
                                    }

                                    if (worksheet.Cells[rowIterator, 11].Value == null) { }
                                    // errorMessages.Add($"Line {rowIterator}: SellingPrice is required.");
                                    else
                                    {
                                        decimal selling = 0;
                                        bool isParsed = decimal.TryParse(worksheet.Cells[rowIterator, 11].Value?.ToString(), out selling);

                                        if (isParsed)
                                        {
                                            productvm.SellingPrice = selling;
                                        }
                                        else
                                        {
                                            string cellValue = worksheet.Cells[rowIterator, 11].Value?.ToString() ?? "NULL";
                                            errorMessages.Add($"This Line {rowIterator}: SellingPrice Cannot Enter - '{cellValue}'");
                                        }
                                    }

                                    if (worksheet.Cells[rowIterator, 12].Value == null) { }
                                    //  errorMessages.Add($"Line {rowIterator}: SupplierCode is required.");
                                    else
                                        productvm.SupplierCode = worksheet.Cells[rowIterator, 12].Value.ToString();

                                    if (!errorMessages.Any())
                                    {
                                        listproductvm.Add(productvm);
                                    }
                                    else
                                    {
                                        // Store error messages in ViewBag
                                        ViewBag.ErrorMessages = string.Join("<br/>", errorMessages);

                                    }

                                }
                                
                               }
                            
                            if (listproductvm.Count > 0)
                            {
                                dt.Columns.Add("LocationID", typeof(int));
                            dt.Columns.Add("ProductId", typeof(int));
                            dt.Columns.Add("ProductCode", typeof(string));
                            dt.Columns.Add("ProductName", typeof(string));
                            dt.Columns.Add("IsRawMaterial", typeof(bool));
                            dt.Columns.Add("IsActive", typeof(bool));
                            dt.Columns.Add("DepartmentID", typeof(int));
                            dt.Columns.Add("CategoryID", typeof(int));
                            dt.Columns.Add("SubCategoryID", typeof(int));
                            dt.Columns.Add("CostPrice", typeof(decimal));
                            dt.Columns.Add("SellingPrice", typeof(decimal));
                            dt.Columns.Add("SupplierCode", typeof(string));
                           
                                foreach (DataUploadProductViewModel item in listproductvm)
                                {
                                    DataRow row = dt.NewRow();
                                    row["LocationID"] = item.LocationID;
                                    row["ProductId"] = item.ProductId;
                                    row["ProductCode"] = item.ProductCode;
                                    row["ProductName"] = item.ProductName;
                                    row["IsRawMaterial"] = item.IsRawMaterial;
                                    row["IsActive"] = item.IsActive;
                                    row["DepartmentID"] = item.DepartmentID;
                                    row["CategoryID"] = item.CategoryID;
                                    row["SubCategoryID"] = item.SubCategoryID;
                                    row["CostPrice"] = item.CostPrice;
                                    row["SellingPrice"] = item.SellingPrice;
                                     row["SupplierCode"] = item.SupplierCode;
                                    dt.Rows.Add(row);
                                }
                            }
                            else
                            {
                                 
                            }
                            // Add other necessary columns
                        }
                    }

                    if (ViewBag.ErrorMessages == null)
                    {
                        if (dt.Rows.Count > 0)
                        {
                            var recipecount = _bllproduct.UpdateProductPriceChanges(dt, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                            if (recipecount.Item1 == "Success")
                            {
                                ViewBag.Message = "4";

                            }
                            else if (recipecount.Item2 == 2)
                            {
                                ViewBag.Message = "5";
                                ViewBag.Item1 = recipecount.Item1;
                            }
                            else if (recipecount.Item2 == 3)
                            {
                                ViewBag.Message = "6";
                                ViewBag.Item1 = recipecount.Item1;
                            }
                            else
                            {
                                ViewBag.Message = "0";
                            }
                        }
                        else
                        {
                            ViewBag.Message = "3";


                        }
                    }
                }
            }
            DataUploadViewModel vmdataupload = new DataUploadViewModel();
            vmdataupload.WithData = true;


            

            return View("~/Views/Product/UploadProductsPriceChnage.cshtml", vmdataupload);
        }

        public ActionResult VerifyStockAdjustmentExcel(DataUploadViewModel dataModel)
        {
            // check permissions
            if (!_appmanager.SetPermissions(8, Session["loggeduserempcode"].ToString(), "SACreatee"))
            {
                @ViewBag.Permissions = "No user permissions to Create Stock Adjustments";
                return View("~/Views/Account/AccessDenied.cshtml");
            }
            if (dataModel.StockLocationId == 0)
            {
                ModelState.AddModelError("StockLocationId", "Please select the location !");
                return View("~/Views/Product/StockAdjustment.cshtml", dataModel);
            }

            //List<DataUploadProductViewModel> listproductvm = new List<DataUploadProductViewModel>();
            //DataTable dt = new DataTable();

            RIT.HMS.Domain.Transactions.StockAdjustmentHeader stockheader = new RIT.HMS.Domain.Transactions.StockAdjustmentHeader();
            List<RIT.HMS.Domain.Transactions.StockAdjustmentDetail> stockAdjDetailList = new List<RIT.HMS.Domain.Transactions.StockAdjustmentDetail>();

            if (Request != null)
            {
                HttpPostedFileBase file = Request.Files["ProductUploadFile"];
                ViewBag.ExcelFile = file.FileName;
                if (file.FileName == string.Empty)
                {
                    ViewBag.statuscode = 0;
                    ViewBag.status = "Browse the Excel File and Verify..";
                    return View("~/Views/Product/StockAdjustment.cshtml", new DataUploadViewModel());
                }
                if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                {
                    string fileName = file.FileName;
                    string fileContentType = file.ContentType;
                    byte[] fileBytes = new byte[file.ContentLength];
                    var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));

                    using (var package = new ExcelPackage(file.InputStream))
                    {
                        var sheets = package.Workbook.Worksheets;
                        foreach (var currentSheet in sheets)
                        {
                            var worksheet = currentSheet;
                            int colcount = 0;
                            int rowcount = 0;
                            if (worksheet.Dimension != null)
                            {
                                colcount = worksheet.Dimension.End.Column;
                                rowcount = worksheet.Dimension.End.Row;
                            }

                            for (int rowIterator = 2; rowIterator <= rowcount; rowIterator++)
                            {
                                if (worksheet.Name == "Stock Adjustment")
                                {
                                    List<string> errorMessages = new List<string>();
                                    RIT.HMS.Domain.Transactions.StockAdjustmentDetail stockAdjustments = new RIT.HMS.Domain.Transactions.StockAdjustmentDetail();

                                    if (worksheet.Cells[rowIterator, 2].Value == null) { }
                                    // errorMessages.Add($"Line {rowIterator}: ProductId is required.");
                                    else
                                    {
                                        int ProID = 0;
                                        bool isParsed = int.TryParse(worksheet.Cells[rowIterator, 1].Value.ToString(), out ProID);
                                        if (isParsed)
                                        {
                                            stockAdjustments.ProductId = Convert.ToInt32(worksheet.Cells[rowIterator, 1].Value);
                                        }
                                        else
                                        {
                                            errorMessages.Add($"This Line {rowIterator}: ProductID Cannot Enter - '" + worksheet.Cells[rowIterator, 2].Value.ToString() + "'");
                                        }
                                    }

                                    //if (string.IsNullOrWhiteSpace(Convert.ToString(worksheet.Cells[rowIterator, 1].Value))) { }
                                    ////  errorMessages.Add($"Line {rowIterator}: ProductCode is required.");
                                    //else
                                    //    stockAdjustments.pro = Convert.ToString(worksheet.Cells[rowIterator, 1].Value).Trim();

                                    if (string.IsNullOrWhiteSpace(Convert.ToString(worksheet.Cells[rowIterator, 3].Value))) { }
                                    // errorMessages.Add($"Line {rowIterator}: ProductName is required.");
                                    else
                                        stockAdjustments.ProductName = Convert.ToString(worksheet.Cells[rowIterator, 3].Value).Trim();

                                    if (worksheet.Cells[rowIterator, 4].Value == null) { }
                                    //  errorMessages.Add($"Line {rowIterator}: CostPrice is required.");
                                    else
                                    {
                                        decimal newStock = 0;
                                        bool isParsed = decimal.TryParse(worksheet.Cells[rowIterator, 4].Value?.ToString(), out newStock);

                                        if (isParsed)
                                        {
                                            stockAdjustments.NewStock = newStock;
                                        }
                                        else
                                        {
                                            string cellValue = worksheet.Cells[rowIterator, 4].Value?.ToString() ?? "NULL";
                                            errorMessages.Add($"This Line {rowIterator}: NewStock Cannot Enter - '{cellValue}'");
                                        }
                                    }

                                    if (!errorMessages.Any())
                                    {
                                        stockAdjDetailList.Add(stockAdjustments);
                                    }
                                    else
                                    {
                                        // Store error messages in ViewBag
                                        ViewBag.ErrorMessages = string.Join("<br/>", errorMessages);

                                    }

                                }

                            }
                        }
                    }
                    stockheader.StockAdjDetail = stockAdjDetailList;

                    stockheader.CreatedUser = Session["loggeduser"].ToString();
                    stockheader.CreatedDate = stockheader.CreatedDate;
                    stockheader.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
                    stockheader.StockLocationId = dataModel.StockLocationId;
                    stockheader.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
                    var res = _bLL_StockAdjustment.SubmitStockAdjustment_(stockheader);
                    if (ViewBag.ErrorMessages == null)
                    {
                        if (res)
                        {
                            ModelState.Clear();
                            ViewBag.Message = "4";
                        }
                        else
                        {
                            ViewBag.Message = "3";

                        }
                    }
                }
            }
            DataUploadViewModel vmdataupload = new DataUploadViewModel();
            vmdataupload.WithData = true;

            return View("~/Views/Product/StockAdjustment.cshtml", vmdataupload);
        }

        [HttpPost]
        public FileResult DownloadProductFormat(DataUploadViewModel vm)
        {
            //FileInfo file = new FileInfo(Server.MapPath("~/docs/") + "HMSProductUpload.xlsx");
            //using (ExcelPackage excelPackage = new ExcelPackage(file))
            //{
            //    ExcelWorkbook excelWorkBook = excelPackage.Workbook;
            //    ExcelWorksheet excelWorksheet = excelWorkBook.Worksheets.First();
            //    excelWorksheet.Cells[1, 1].Value = "Test";
            //    excelWorksheet.Cells[3, 2].Value = "Test2";
            //    excelWorksheet.Cells[3, 3].Value = "Test3";

            //    excelPackage.Save();
            //}

            string path = Server.MapPath("~/docs/") + "HMSProductUpload.xlsx";
            byte[] bytes = System.IO.File.ReadAllBytes(path);
            return File(bytes, "application/octet-stream", "HMSProductUpload.xlsx");
        }
        [HttpPost]
        public void HMSProductPriceChangeFormat(DataUploadViewModel vm)
        {
            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("SavingUnitPriceChange");

            Sheet.Cells["A1"].Value = "Location code";
            Sheet.Cells["B1"].Value = "Product Code";
            Sheet.Cells["C1"].Value = "Serving Unit";
            Sheet.Cells["D1"].Value = "Cost Price";
            Sheet.Cells["E1"].Value = "Selleing Price";
            Sheet.Cells["F1"].Value = "Product Serving Unit Id";
            Sheet.Cells["G1"].Value = "Product Name";
            if (vm.WithData)
            {
                var dbproducts = _bllproduct.DownloadServingUnitPriceData(Convert.ToInt32(Session["loggedusercompanyId"].ToString()));

                int row = 2;
                foreach (var item in dbproducts)
                {
                    Sheet.Cells[string.Format("A{0}", row)].Value = item.LocationId;
                    Sheet.Cells[string.Format("B{0}", row)].Value = item.ProductId;
                    Sheet.Cells[string.Format("C{0}", row)].Value = item.ServingUnit;
                    Sheet.Cells[string.Format("D{0}", row)].Value = item.CostPrice;
                    Sheet.Cells[string.Format("E{0}", row)].Value = item.SellingPrice;
                    Sheet.Cells[string.Format("F{0}", row)].Value = item.ProductServingUnitId;
                    Sheet.Cells[string.Format("G{0}", row)].Value = item.ProductName;
                    row++;
                }
            }

            Sheet.Cells["A:G"].AutoFitColumns();
            Response.Clear();
            Response.Buffer = true;
            Response.Charset = "";
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: attachment;filename=HMSProductPriceChangeFormat.xlsx");

            using (MemoryStream MyMemoryStream = new MemoryStream())
            {
                Ep.SaveAs(MyMemoryStream);
                MyMemoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }
        }
      

        [HttpPost]
        public void HMSProductPriceTakeawayUberFormat(DataUploadViewModel vm)
        {
            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("TakeawayUber");

            Sheet.Cells["A1"].Value = "Location code";
            Sheet.Cells["B1"].Value = "Product Code";
            Sheet.Cells["C1"].Value = "Product Name";
            Sheet.Cells["D1"].Value = "Small";
            Sheet.Cells["E1"].Value = "Large";
            Sheet.Cells["F1"].Value = "Uber-Small";
            Sheet.Cells["G1"].Value = "Uber-Large";
            if (vm.WithData)
            {
                var dbproducts = _bllproduct.DownloadServingUnitPriceData(Convert.ToInt32(Session["loggedusercompanyId"].ToString()));

                int row = 2;
                foreach (var item in dbproducts)
                {
                    Sheet.Cells[string.Format("A{0}", row)].Value = item.LocationId;
                    Sheet.Cells[string.Format("B{0}", row)].Value = item.ProductId;
                    Sheet.Cells[string.Format("C{0}", row)].Value = item.ServingUnit;
                    Sheet.Cells[string.Format("D{0}", row)].Value = item.CostPrice;
                    Sheet.Cells[string.Format("E{0}", row)].Value = item.SellingPrice;
                    Sheet.Cells[string.Format("F{0}", row)].Value = item.ProductServingUnitId;
                    Sheet.Cells[string.Format("G{0}", row)].Value = item.ProductName;
                    row++;
                }
            }

            Sheet.Cells["A:G"].AutoFitColumns();
            Response.Clear();
            Response.Buffer = true;
            Response.Charset = "";
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: attachment;filename=HMSProductPriceTakeawayUberFormat.xlsx");

            using (MemoryStream MyMemoryStream = new MemoryStream())
            {
                Ep.SaveAs(MyMemoryStream);
                MyMemoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }
        }
        [HttpPost]
        public void HMSProductPriceTakeawayKelaniyaFormat(DataUploadViewModel vm)
        {
            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("TakeawayKelaniya");

            Sheet.Cells["A1"].Value = "Location code";
            Sheet.Cells["B1"].Value = "Product Code";
            Sheet.Cells["C1"].Value = "Product Name";
            Sheet.Cells["D1"].Value = "Small";
            Sheet.Cells["E1"].Value = "Large";
            Sheet.Cells["F1"].Value = "Res 01-Small";
            Sheet.Cells["G1"].Value = "Res 01-Large";
            if (vm.WithData)
            {
                var dbproducts = _bllproduct.DownloadServingUnitPriceData(Convert.ToInt32(Session["loggedusercompanyId"].ToString()));

                int row = 2;
                foreach (var item in dbproducts)
                {
                    Sheet.Cells[string.Format("A{0}", row)].Value = item.LocationId;
                    Sheet.Cells[string.Format("B{0}", row)].Value = item.ProductId;
                    Sheet.Cells[string.Format("C{0}", row)].Value = item.ServingUnit;
                    Sheet.Cells[string.Format("D{0}", row)].Value = item.CostPrice;
                    Sheet.Cells[string.Format("E{0}", row)].Value = item.SellingPrice;
                    Sheet.Cells[string.Format("F{0}", row)].Value = item.ProductServingUnitId;
                    Sheet.Cells[string.Format("G{0}", row)].Value = item.ProductName;
                    row++;
                }
            }

            Sheet.Cells["A:G"].AutoFitColumns();
            Response.Clear();
            Response.Buffer = true;
            Response.Charset = "";
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: attachment;filename=HMSProductPriceTakeawayKelaniyaFormat.xlsx");

            using (MemoryStream MyMemoryStream = new MemoryStream())
            {
                Ep.SaveAs(MyMemoryStream);
                MyMemoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }
        }
        [HttpPost]
        public void HMSProductUploadFormat(DataUploadViewModel vm)
        {
            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("ProductSupplierLocation");

            Sheet.Cells["A1"].Value = "ProductCode";
            Sheet.Cells["B1"].Value = "ProductName";
            Sheet.Cells["C1"].Value = "NameOnInvoice";
            Sheet.Cells["D1"].Value = "IsRowMaterial";
            Sheet.Cells["E1"].Value = "IsScaleItem";
            Sheet.Cells["F1"].Value = "DepartmentCode";

            Sheet.Cells["G1"].Value = "CategoryCode";
            Sheet.Cells["H1"].Value = "SubCategoryCode";
            Sheet.Cells["I1"].Value = "PurchasingUnitCode";
            Sheet.Cells["J1"].Value = "SubUnit";
            Sheet.Cells["K1"].Value = "PrinterType";

            Sheet.Cells["L1"].Value = "IsDiscount";
            Sheet.Cells["M1"].Value = "IsCostOnReceipe";
            Sheet.Cells["N1"].Value = "IsAddon";
            Sheet.Cells["O1"].Value = "IsPromotion";
            Sheet.Cells["P1"].Value = "IsExpiry";

            Sheet.Cells["Q1"].Value = "IsTax";
            Sheet.Cells["R1"].Value = "IsUnderCost";
            Sheet.Cells["S1"].Value = "IsTaxInclude";
            Sheet.Cells["T1"].Value = "IsOpenItem";
            Sheet.Cells["U1"].Value = "AutoProduction";

            Sheet.Cells["V1"].Value = "IsNoEffectCostforMenu";
            Sheet.Cells["W1"].Value = "SupplierCode";
            Sheet.Cells["X1"].Value = "LocationCode";
            Sheet.Cells["Y1"].Value = "Stock";
            Sheet.Cells["Z1"].Value = "CostPrice";

            Sheet.Cells["AA1"].Value = "AverageCost";
            Sheet.Cells["AB1"].Value = "SellingPrice";
            Sheet.Cells["AC1"].Value = "ReOrderLevel";
            Sheet.Cells["AD1"].Value = "ReOrderQuantity";

            if (vm.WithData)
            {
                var dbproducts = _bllproduct.DownloadProductUploadData(Convert.ToInt32(Session["loggedusercompanyId"].ToString()));

                int row = 2;
                foreach (var item in dbproducts)
                {
                    Sheet.Cells[string.Format("A{0}", row)].Value = item.ProductCode;
                    Sheet.Cells[string.Format("B{0}", row)].Value = item.ProductName;
                    Sheet.Cells[string.Format("C{0}", row)].Value = item.NameOnInvoice;
                    Sheet.Cells[string.Format("D{0}", row)].Value = item.IsRawMaterial;
                    Sheet.Cells[string.Format("E{0}", row)].Value = item.IsScaleItem;
                    Sheet.Cells[string.Format("F{0}", row)].Value = item.DepartmentCode;

                    Sheet.Cells[string.Format("G{0}", row)].Value = item.RstCategoryCode;
                    Sheet.Cells[string.Format("H{0}", row)].Value = item.RstSubCategoryCode;
                    Sheet.Cells[string.Format("I{0}", row)].Value = item.UnitOfMeasureCode;
                    Sheet.Cells[string.Format("J{0}", row)].Value = item.SubUnit;
                    Sheet.Cells[string.Format("K{0}", row)].Value = item.PrinterType;

                    Sheet.Cells[string.Format("L{0}", row)].Value = item.IsDiscount;
                    Sheet.Cells[string.Format("M{0}", row)].Value = item.IsCostOnReceipe;
                    Sheet.Cells[string.Format("N{0}", row)].Value = item.IsAddon;
                    Sheet.Cells[string.Format("O{0}", row)].Value = item.IsPromotion;
                    Sheet.Cells[string.Format("P{0}", row)].Value = item.IsExpiry;

                    Sheet.Cells[string.Format("Q{0}", row)].Value = item.IsTax;
                    Sheet.Cells[string.Format("R{0}", row)].Value = item.IsUnderCost;
                    Sheet.Cells[string.Format("S{0}", row)].Value = item.IsTaxInclude;
                    Sheet.Cells[string.Format("T{0}", row)].Value = item.IsOpenItem;
                    Sheet.Cells[string.Format("U{0}", row)].Value = item.AutoProduction;

                    Sheet.Cells[string.Format("V{0}", row)].Value = item.IsNoEffectCostforMenu;
                    Sheet.Cells[string.Format("W{0}", row)].Value = item.SupplierCode;
                    Sheet.Cells[string.Format("X{0}", row)].Value = item.LocationCode;
                    Sheet.Cells[string.Format("Y{0}", row)].Value = item.SellingPrice;
                    Sheet.Cells[string.Format("Z{0}", row)].Value = item.CostPrice;

                    Sheet.Cells[string.Format("AA{0}", row)].Value = item.AvgCost;
                    Sheet.Cells[string.Format("AB{0}", row)].Value = item.SellingPrice;
                    Sheet.Cells[string.Format("AC{0}", row)].Value = item.ReOrderLevel;
                    Sheet.Cells[string.Format("AD{0}", row)].Value = item.ReOrderQuantity;

                    row++;
                }
            }

            Sheet.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.Buffer = true;
            Response.Charset = "";
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: attachment;filename=HMSProductUploadFormat.xlsx");

            using (MemoryStream MyMemoryStream = new MemoryStream())
            {
                Ep.SaveAs(MyMemoryStream);
                MyMemoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }
        }
        [HttpPost]
        public void HMSRecipePriceChangeFormat(DataUploadViewModel vm)
        {
            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("RecipePriceChange");

            Sheet.Cells["A1"].Value = "ProductCode";
            Sheet.Cells["B1"].Value = "ServingUnit";
            Sheet.Cells["C1"].Value = "LocationCode";
            Sheet.Cells["D1"].Value = "Cost Price";
            Sheet.Cells["E1"].Value = "Selling Price";

            if (vm.WithData)
            {
                var dbproducts = _bllReceipe.DownloadRecipePriceData(Convert.ToInt32(Session["loggedusercompanyId"].ToString()));

                int row = 2;
                foreach (var item in dbproducts)
                {
                    Sheet.Cells[string.Format("A{0}", row)].Value = item.ProductCode;
                    Sheet.Cells[string.Format("B{0}", row)].Value = item.ServingUint;
                    Sheet.Cells[string.Format("C{0}", row)].Value = item.LocationCode;
                    Sheet.Cells[string.Format("D{0}", row)].Value = item.CostPrice;
                    Sheet.Cells[string.Format("E1{0}", row)].Value = item.SellingPrice;

                    row++;
                }
            }

            Sheet.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.Buffer = true;
            Response.Charset = "";
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: attachment;filename=HMSRecipePriceChangeFormat.xlsx");

            using (MemoryStream MyMemoryStream = new MemoryStream())
            {
                Ep.SaveAs(MyMemoryStream);
                MyMemoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }
        }
        [HttpPost]
        public void ProductPriceChangeExcel(DataUploadViewModel vm)
        {
            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Product Price Change");

            // Set headers
            Sheet.Cells["A1"].Value = "LocationID";
            Sheet.Cells["B1"].Value = "ProductID";
            Sheet.Cells["C1"].Value = "ProductCode";
            Sheet.Cells["D1"].Value = "ProductName";
            Sheet.Cells["E1"].Value = "IsRowMaterial";
            Sheet.Cells["F1"].Value = "IsActive";
            Sheet.Cells["G1"].Value = "DepartmentID";
            Sheet.Cells["H1"].Value = "CategoryID";
            Sheet.Cells["I1"].Value = "SubCategoryID";
            Sheet.Cells["J1"].Value = "CostPrice";
            Sheet.Cells["K1"].Value = "SellingPrice";
            Sheet.Cells["L1"].Value = "SupplierCode";

            if (vm.WithData)
            {
                var dbproducts = _bllproduct
                    .DownloadProductPriceChnageUploadData(Convert.ToInt32(Session["loggedusercompanyId"].ToString()))
                    .OrderBy(p => p.LocationID)
                    .ToList();

                int row = 2;
                foreach (var item in dbproducts)
                {
                    Sheet.Cells[string.Format("A{0}", row)].Value = item.LocationID;
                    Sheet.Cells[string.Format("B{0}", row)].Value = item.ProductId;
                    Sheet.Cells[string.Format("C{0}", row)].Value = item.ProductCode;
                    Sheet.Cells[string.Format("D{0}", row)].Value = item.ProductName;
                    Sheet.Cells[string.Format("E{0}", row)].Value = item.IsRawMaterial;
                    Sheet.Cells[string.Format("F{0}", row)].Value = item.IsActive;
                    Sheet.Cells[string.Format("G{0}", row)].Value = item.DepartmentID;
                    Sheet.Cells[string.Format("H{0}", row)].Value = item.CategoryID;
                    Sheet.Cells[string.Format("I{0}", row)].Value = item.SubCategoryID;
                    Sheet.Cells[string.Format("J{0}", row)].Value = item.CostPrice;
                    Sheet.Cells[string.Format("K{0}", row)].Value = item.SellingPrice;
                    Sheet.Cells[string.Format("L{0}", row)].Value = item.SupplierCode;

                    // Apply decimal format (remove currency format)
                    Sheet.Cells[string.Format("J{0}", row)].Style.Numberformat.Format = "#,##0.00";
                    Sheet.Cells[string.Format("K{0}", row)].Style.Numberformat.Format = "#,##0.00";

                    row++;
                }
            }

            // AutoFit columns
            Sheet.Cells["A:AZ"].AutoFitColumns();

            // Prepare the response
            Response.Clear();
            Response.Buffer = true;
            Response.Charset = "";
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: attachment;filename=ProductPriceChangeExcel.xlsx");

            using (MemoryStream MyMemoryStream = new MemoryStream())
            {
                Ep.SaveAs(MyMemoryStream);
                MyMemoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }
        }

        [HttpPost]
        public void StockAdjustmentExcel(DataUploadViewModel vm)
        {
            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Stock Adjustment");

            // Set headers
            Sheet.Cells["A1"].Value = "ProductID";
            Sheet.Cells["B1"].Value = "ProductCode";
            Sheet.Cells["C1"].Value = "ProductName";
            Sheet.Cells["D1"].Value = "StocksInHand";

            if (vm.WithData)
            {
                var dbproducts = _bllproduct
                    .DownloadProductStockListUploadData(Convert.ToInt32(Session["loggedusercompanyId"].ToString()),vm.StockLocationId)
                    .OrderBy(p => p.ProductCode)
                    .ToList();

                int row = 2;
                foreach (var item in dbproducts)
                {
                    Sheet.Cells[string.Format("A{0}", row)].Value = item.ProductId;
                    Sheet.Cells[string.Format("B{0}", row)].Value = item.ProductCode;
                    Sheet.Cells[string.Format("C{0}", row)].Value = item.ProductName;
                    Sheet.Cells[string.Format("D{0}", row)].Value = item.Stock;
                    row++;
                }
            }

            // AutoFit columns
            Sheet.Cells["A:AZ"].AutoFitColumns();

            // Prepare the response
            Response.Clear();
            Response.Buffer = true;
            Response.Charset = "";
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: attachment;filename=StockAdjustmentExcel.xlsx");

            using (MemoryStream MyMemoryStream = new MemoryStream())
            {
                Ep.SaveAs(MyMemoryStream);
                MyMemoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }
        }

        [HttpPost]
        public void HMSProductTaxFormat(DataUploadViewModel vm)
        {
            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("ProductTaxes");

            Sheet.Cells["A1"].Value = "ProductCode";
            Sheet.Cells["B1"].Value = "TaxCode";

            if (vm.WithData)
            {
                var dbtaxes = _bllproduct.DownloadProductTaxData(Convert.ToInt32(Session["loggedusercompanyId"].ToString()));

                int row = 2;
                foreach (var item in dbtaxes)
                {
                    Sheet.Cells[string.Format("A{0}", row)].Value = item.ProductCode;
                    Sheet.Cells[string.Format("B{0}", row)].Value = item.TaxCode;

                    row++;
                }
            }

            Sheet.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.Buffer = true;
            Response.Charset = "";
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: attachment;filename=HMSProductTaxFormat.xlsx");

            using (MemoryStream MyMemoryStream = new MemoryStream())
            {
                Ep.SaveAs(MyMemoryStream);
                MyMemoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }
        }

        [HttpPost]
        public void HMSRecipeUploadFormat(DataUploadViewModel vm)
        {
            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("RecipeUpload");

            Sheet.Cells["A1"].Value = "LocationCode";
            Sheet.Cells["B1"].Value = "RecipeCode";
            Sheet.Cells["C1"].Value = "ServingUnit";
            Sheet.Cells["D1"].Value = "RecipeQuantity";
            Sheet.Cells["E1"].Value = "SellingPrice";
            Sheet.Cells["F1"].Value = "MaterialCode";
            Sheet.Cells["G1"].Value = "MaterialQuantity";
            Sheet.Cells["H1"].Value = "SubUnit";

            if (vm.WithData)
            {
                var dbrecipes = _bllReceipe.DownloadRecipeData(Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                int row = 2;
                foreach (var item in dbrecipes)
                {
                    Sheet.Cells[string.Format("A{0}", row)].Value = item.LocationCode;
                    Sheet.Cells[string.Format("B{0}", row)].Value = item.ProductCode;
                    Sheet.Cells[string.Format("C{0}", row)].Value = item.ServingUint;
                    Sheet.Cells[string.Format("D{0}", row)].Value = item.ProductQuantity;
                    Sheet.Cells[string.Format("E{0}", row)].Value = item.SellingPrice;
                    Sheet.Cells[string.Format("F{0}", row)].Value = item.MaterialCode;
                    Sheet.Cells[string.Format("G{0}", row)].Value = item.MaterialQuantity;
                    Sheet.Cells[string.Format("H{0}", row)].Value = item.SubUnit;

                    row++;
                }
            }

            Sheet.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.Buffer = true;
            Response.Charset = "";
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: attachment;filename=RecipeUpload.xlsx");

            using (MemoryStream MyMemoryStream = new MemoryStream())
            {
                Ep.SaveAs(MyMemoryStream);
                MyMemoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }
        }

        [HttpPost]
        public FileResult DownloadProductTaxFormate(DataUploadViewModel vm)
        {
            string path = Server.MapPath("~/docs/") + "HMSProductTaxUpload.xlsx";
            byte[] bytes = System.IO.File.ReadAllBytes(path);
            return File(bytes, "application/octet-stream", "HMSProductTaxUpload.xlsx");
        }

        [HttpPost]
        public FileResult DownloadRecipePriceChangeFormate(DataUploadViewModel vm)
        {
            string path = Server.MapPath("~/docs/") + "HMSRecipePriceChangeUpload.xlsx";
            byte[] bytes = System.IO.File.ReadAllBytes(path);
            return File(bytes, "application/octet-stream", "HMSRecipePriceChangeUpload.xlsx");
        }

        [HttpPost]
        public FileResult DownloadRecipeUploadFormate(DataUploadViewModel vm)
        {
            string path = Server.MapPath("~/docs/") + "HMSRecipeUpload.xlsx";
            byte[] bytes = System.IO.File.ReadAllBytes(path);
            return File(bytes, "application/octet-stream", "HMSRecipeUpload.xlsx");
        }

        public ActionResult UploadProductsFromExcel()
        {
            return View("~/Views/Product/UploadProducts.cshtml", new DataUploadViewModel() { WithData = true });
        }
        public ActionResult UploadProductsPriceChnageFromExcel()
        {
            return View("~/Views/Product/UploadProductsPriceChnage.cshtml", new DataUploadViewModel() { WithData = true });
        }
        public ActionResult StockAdjustment()
        {
            return View("~/Views/Product/StockAdjustment.cshtml",new DataUploadViewModel() { WithData = true});
        }

        [HttpPost]
        public ActionResult UploadProductsFromExcel(FormCollection formCollection)
        {
            string worksheetNew = "";
            if (Request != null)
            {

                HttpPostedFileBase file = Session["ProductUploadedFile"] as HttpPostedFileBase;
                if (file != null)
                {
                }
                else
                {
                    ViewBag.statuscode = 0;
                    ViewBag.status = "Browse the Excel File and Upload..";
                    return View("~/Views/Product/UploadProducts.cshtml", new DataUploadViewModel());
                }



               //     HttpPostedFileBase file = Request.Files["ProductUploadFile"];
                //HttpPostedFileBase file = Request.Files["ProductUploadFile"];
                //ViewBag.ExcelFile = file.FileName;
                
                //if (file.FileName == string.Empty)
                //{
                //    ViewBag.statuscode = 0;
                //    ViewBag.status = "Browse the Excel File and Upload..";
                //    return View("~/Views/Product/UploadProducts.cshtml", new DataUploadViewModel());
                //}

                string docpath = ConfigurationManager.AppSettings["ExcelPath"].ToString();
                string path = Server.MapPath(docpath);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                var k = DateTime.Now.ToString().Replace('/', '-').Trim().Replace(':', ' ') + " " + Convert.ToString(Session["loggeduser"]) + " ";
                file.SaveAs(path + Path.GetFileName(k + file.FileName));

                List<Product> listproduct = new List<Product>();
                List<ProductStockMaster> listproductstockmaster = new List<ProductStockMaster>();
                List<SupplierProduct> listsupplierproducts = new List<SupplierProduct>();
                List<ProductTax> listproducttaxes = new List<ProductTax>();
                List<ReceipeViewModel> listRecipe = new List<ReceipeViewModel>();
                List<ReceipeViewModel> listRecipeUpload = new List<ReceipeViewModel>();

                if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                {
                    string fileName = file.FileName;
                    string fileContentType = file.ContentType;
                    byte[] fileBytes = new byte[file.ContentLength];
                    var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));

                    using (var package = new ExcelPackage(file.InputStream))
                    {
                        var sheets = package.Workbook.Worksheets;
                        foreach (var currentSheet in sheets)
                        {
                            var worksheet = currentSheet;
                            int colcount = 0;
                            int rowcount = 0;
                            if (worksheet.Dimension != null)
                            {
                                colcount = worksheet.Dimension.End.Column;
                                rowcount = worksheet.Dimension.End.Row;
                            }
                            for (int rowIterator = 2; rowIterator <= rowcount; rowIterator++)
                            {
                                if (worksheet.Name == "ProductSupplierLocation")
                                {
                                    Product product = new Product();
                                    ProductStockMaster productstockmaster = new ProductStockMaster();
                                    SupplierProduct supplierproduct = new SupplierProduct();

                                    product.ProductCode = Convert.ToString(worksheet.Cells[rowIterator, 1].Value).Trim();
                                    product.ProductName = Convert.ToString(worksheet.Cells[rowIterator, 2].Value);
                                    product.NameOnInvoice = Convert.ToString(worksheet.Cells[rowIterator, 3].Value);
                                    product.IsRowMaterial = Convert.ToBoolean(worksheet.Cells[rowIterator, 4].Value);
                                    product.IsScaleItem = Convert.ToBoolean(worksheet.Cells[rowIterator, 5].Value);

                                    // departments
                                    string deptcode = Convert.ToString(worksheet.Cells[rowIterator, 6].Value).Trim();
                                    var department = _blldepartment.GetDeptByCode(deptcode, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                                    if (department != null)
                                    {
                                        product.DepartmentId = department.RstDepartmentID;
                                        product.DepartmnetCode = department.DepartmentCode;
                                    }
                                    else
                                    {
                                        product.DepartmentId = 0;
                                        product.DepartmnetCode = deptcode;
                                    }
                                    // end departments

                                    // category
                                    string catcode = Convert.ToString(worksheet.Cells[rowIterator, 7].Value).Trim();
                                    var category = _bllcategory.GetCatByCode(catcode, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                                    if (category != null)
                                    {
                                        product.CategoryId = category.RstCategoryID;
                                        product.CategoryCode = category.RstCategoryCode;
                                    }
                                    else
                                    {
                                        product.CategoryId = 0;
                                        product.CategoryCode = catcode;
                                    }

                                    // end category

                                    // Sub category
                                    string subcatcode = Convert.ToString(worksheet.Cells[rowIterator, 8].Value).Trim();
                                    var subcategory = _bllsubcategory.GetSubCatByCode(subcatcode, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                                    if (subcategory != null)
                                    {
                                        product.SubCategoryId = subcategory.RstSubCategoryID;
                                        product.SubCategoryCode = subcategory.RstSubCategoryCode;
                                    }
                                    else
                                    {
                                        product.SubCategoryId = 0;
                                        product.SubCategoryCode = subcatcode;
                                    }
                                    // end Sub Category

                                    // UOM
                                    string UOMCode = Convert.ToString(worksheet.Cells[rowIterator, 9].Value).Trim();
                                    var UOM = _bllunitofmeasure.GetUnitOfMeasureByCode(UOMCode, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                                    if (UOM != null)
                                    {
                                        product.PurchasingUnit = (Int32)UOM.UnitOfMeasureId;
                                        product.UOMCode = UOM.UnitOfMeasureCode;
                                    }
                                    else
                                    {
                                        product.PurchasingUnit = 0;
                                        product.UOMCode = UOMCode;
                                    }
                                    // end UOM

                                    // Sub unit
                                    string SubUnitcode = Convert.ToString(worksheet.Cells[rowIterator, 10].Value).Trim();
                                    var SubUnit = _bllunitconversion.GetConversionByCode(SubUnitcode, product.PurchasingUnit, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                                    if (SubUnit != null)
                                    {
                                        product.WeightPerUnit = (Int32)SubUnit.UnitConversionId;
                                        product.SubUnitCode = SubUnit.SubUnit;
                                    }
                                    else
                                    {
                                        product.WeightPerUnit = 0;
                                        product.SubUnitCode = SubUnitcode;
                                    }
                                    // end Sub Unit

                                    // PrinterTypeID
                                    string printercode = Convert.ToString(worksheet.Cells[rowIterator, 11].Value).Trim();
                                    var printer = _bllproduct.GetPrinterByName(printercode);
                                    if (printer != null)
                                    {
                                        product.PrinterTypeId = (Int32)printer.PrinterTypeId;
                                        product.PrinterCode = printer.PrinterTypeName;
                                    }
                                    else
                                    {
                                        product.PrinterTypeId = 0;
                                        product.PrinterCode = printercode;
                                    }
                                    // PrinterTypeID
                                    product.IsDiscount = Convert.ToBoolean(worksheet.Cells[rowIterator, 12].Value);
                                    product.IsCostOnReceipe = Convert.ToBoolean(worksheet.Cells[rowIterator, 13].Value);
                                    product.IsAddon = Convert.ToBoolean(worksheet.Cells[rowIterator, 14].Value);
                                    product.IsPromotion = Convert.ToBoolean(worksheet.Cells[rowIterator, 15].Value);
                                    product.IsExpiry = Convert.ToBoolean(worksheet.Cells[rowIterator, 16].Value);
                                    product.IsTax = Convert.ToBoolean(worksheet.Cells[rowIterator, 17].Value);
                                    product.IsUnderCost = Convert.ToBoolean(worksheet.Cells[rowIterator, 18].Value);
                                    product.IsTaxInclude = Convert.ToBoolean(worksheet.Cells[rowIterator, 19].Value);
                                    product.IsOpenItem = Convert.ToBoolean(worksheet.Cells[rowIterator, 20].Value);
                                    product.AutoProduction = Convert.ToBoolean(worksheet.Cells[rowIterator, 21].Value);
                                    product.IsNoEffectCostforMenu = Convert.ToBoolean(worksheet.Cells[rowIterator, 22].Value);

                                    listproduct.Add(product);

                                    // Stock
                                    // LocationId
                                    string locationcode = Convert.ToString(worksheet.Cells[rowIterator, 24].Value).Trim();
                                    var location = _blllocation.GetLocByCode(locationcode, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                                    if (location != null)
                                    {
                                        productstockmaster.LocationId = (Int32)location.SysLocationID;
                                        productstockmaster.LocationCode = location.LocationCode;
                                    }
                                    else
                                    {
                                        productstockmaster.LocationId = 0;
                                        productstockmaster.LocationCode = locationcode;
                                    }
                                    // LocationId
                                    productstockmaster.IsActive = true;
                                    productstockmaster.CostCentreId = productstockmaster.LocationId;
                                    productstockmaster.StockCode = product.ProductCode;
                                    productstockmaster.ProductCode = product.ProductCode;
                                    productstockmaster.CostPrice = Convert.ToDecimal(worksheet.Cells[rowIterator, 26].Value);
                                    productstockmaster.AvgCost = Convert.ToDecimal(worksheet.Cells[rowIterator, 27].Value);
                                    productstockmaster.Stock = Convert.ToDecimal(worksheet.Cells[rowIterator, 25].Value);
                                    productstockmaster.SellingPrice = Convert.ToDecimal(worksheet.Cells[rowIterator, 28].Value);
                                    productstockmaster.ReOrderLevel = Convert.ToDecimal(worksheet.Cells[rowIterator, 29].Value);
                                    productstockmaster.ReOrderQuantity = Convert.ToDecimal(worksheet.Cells[rowIterator, 30].Value);
                                    listproductstockmaster.Add(productstockmaster);

                                    string supplercode = Convert.ToString(worksheet.Cells[rowIterator, 23].Value).Trim();
                                    var supplier = _bllSupplier.GetSupplierByCode(supplercode, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                                    if (supplier != null)
                                    {
                                        supplierproduct.SupplierId = (Int32)supplier.SupplierID;
                                        supplierproduct.SupplierCode = supplier.SupplierCode;
                                    }
                                    else
                                    {
                                        supplierproduct.SupplierId = 0;
                                        supplierproduct.SupplierCode = supplercode;
                                    }
                                    supplierproduct.ProductCode = Convert.ToString(worksheet.Cells[rowIterator, 1].Value).Trim();
                                    listsupplierproducts.Add(supplierproduct);

                                    // End Stock
                                }
                                else if (worksheet.Name == "ProductSuppliers")
                                {
                                    SupplierProduct supplierproduct = new SupplierProduct();

                                    // supplier products
                                    string supplercode = Convert.ToString(worksheet.Cells[rowIterator, 2].Value).Trim();
                                    var supplier = _bllSupplier.GetSupplierByCode(supplercode, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                                    if (supplier != null)
                                    {
                                        supplierproduct.SupplierId = (Int32)supplier.SupplierID;
                                        supplierproduct.SupplierCode = supplier.SupplierCode;
                                    }
                                    else
                                    {
                                        supplierproduct.SupplierId = 0;
                                        supplierproduct.SupplierCode = supplercode;
                                    }
                                    supplierproduct.ProductCode = Convert.ToString(worksheet.Cells[rowIterator, 1].Value).Trim();
                                    listsupplierproducts.Add(supplierproduct);
                                    //end supplier products
                                }
                                else if (worksheet.Name == "SavingUnitPriceChange")
                                {
                                    worksheetNew = "SavingUnitPriceChange";
                                    ReceipeViewModel RecipeViewModel = new ReceipeViewModel();
                                    RecipeViewModel.ProductCode = Convert.ToString(worksheet.Cells[rowIterator, 2].Value).Trim();
                                    RecipeViewModel.ServingUnitName = Convert.ToString(worksheet.Cells[rowIterator, 4].Value).Trim();
                                    // Stock
                                    // LocationId
                                    string locationcode = Convert.ToString(worksheet.Cells[rowIterator, 1].Value).Trim();
                                    //var location = _blllocation.GetLocByCode(locationcode, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                                    //if (location != null)
                                    //{
                                    //RecipeViewModel.LocationId = (Int32)location.SysLocationID;
                                    //RecipeViewModel.LocationCode = location.LocationCode;
                                    //}
                                    //else
                                    //{
                                    RecipeViewModel.LocationId = 0;
                                    RecipeViewModel.LocationCode = locationcode;
                                    //}
                                    // LocationId

                                    //var cp = worksheet.Cells[rowIterator, 5].Value;
                                    //if (cp == null) { cp = 0; }
                                    //RecipeViewModel.CostPrice = Convert.ToDecimal(cp);

                                    //var sp = worksheet.Cells[rowIterator, 6].Value;
                                    //if (sp == null) { sp = 0; }
                                    //RecipeViewModel.SellingPrice = Convert.ToDecimal(sp);

                                    listRecipe.Add(RecipeViewModel);
                                }
                                else if (worksheet.Name == "TakeawayUber")
                                {
                                    worksheetNew = "TakeawayUber";
                                    ReceipeViewModel RecipeViewModel = new ReceipeViewModel();
                                    RecipeViewModel.ProductCode = Convert.ToString(worksheet.Cells[rowIterator, 1].Value).Trim();
                                    RecipeViewModel.ServingUnitName = Convert.ToString(worksheet.Cells[rowIterator, 2].Value).Trim();
                                    string locationcode = Convert.ToString(worksheet.Cells[rowIterator, 3].Value).Trim();
                                    RecipeViewModel.LocationId = Convert.ToInt32(worksheet.Cells[rowIterator, 3].Value);
                                    RecipeViewModel.LocationCode = locationcode;
                                    listRecipe.Add(RecipeViewModel);
                                }
                                else if (worksheet.Name == "ProductTaxes")
                                {
                                    ProductTax producttax = new ProductTax();

                                    // product taxes
                                    string taxcode = Convert.ToString(worksheet.Cells[rowIterator, 2].Value).Trim();
                                    var tax = _blltax.GetTaxByCode(taxcode, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                                    if (tax != null)
                                    {
                                        producttax.TaxId = (Int32)tax.TaxId;
                                        producttax.TaxCode = tax.TaxCode;
                                    }
                                    else
                                    {
                                        producttax.TaxId = 0;
                                        producttax.TaxCode = taxcode;
                                    }
                                    producttax.ProductCode = Convert.ToString(worksheet.Cells[rowIterator, 1].Value).Trim();
                                    listproducttaxes.Add(producttax);
                                    //end product taxes
                                }
                                else if (worksheet.Name == "RecipePriceChange")
                                {

                                    ReceipeViewModel RecipeViewModel = new ReceipeViewModel();
                                    RecipeViewModel.ProductCode = Convert.ToString(worksheet.Cells[rowIterator, 1].Value).Trim();
                                    RecipeViewModel.ServingUnitName = Convert.ToString(worksheet.Cells[rowIterator, 2].Value).Trim();
                                    // Stock
                                    // LocationId
                                    string locationcode = Convert.ToString(worksheet.Cells[rowIterator, 3].Value).Trim();
                                    var location = _blllocation.GetLocByCode(locationcode, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                                    if (location != null)
                                    {
                                        RecipeViewModel.LocationId = (Int32)location.SysLocationID;
                                        RecipeViewModel.LocationCode = location.LocationCode;
                                    }
                                    else
                                    {
                                        RecipeViewModel.LocationId = 0;
                                        RecipeViewModel.LocationCode = locationcode;
                                    }
                                    // LocationId

                                    var cp = worksheet.Cells[rowIterator, 4].Value;
                                    if (cp == null) { cp = 0; }
                                    RecipeViewModel.CostPrice = Convert.ToDecimal(cp);

                                    var sp = worksheet.Cells[rowIterator, 5].Value;
                                    if (sp == null) { sp = 0; }
                                    RecipeViewModel.SellingPrice = Convert.ToDecimal(sp);

                                    listRecipe.Add(RecipeViewModel);
                                    //end product taxes
                                }
                                else if (worksheet.Name == "RecipeUpload")
                                {
                                    ReceipeViewModel RecipeViewModel = new ReceipeViewModel();
                                    RecipeViewModel.ProductCode = Convert.ToString(worksheet.Cells[rowIterator, 2].Value).Trim();
                                    RecipeViewModel.ServingUnitName = Convert.ToString(worksheet.Cells[rowIterator, 3].Value).Trim();
                                    // Stock
                                    // LocationId
                                    string locationcode = Convert.ToString(worksheet.Cells[rowIterator, 1].Value).Trim();
                                    var location = _blllocation.GetLocByCode(locationcode, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                                    if (location != null)
                                    {
                                        RecipeViewModel.LocationId = (Int32)location.SysLocationID;
                                        RecipeViewModel.LocationCode = location.LocationCode;
                                    }
                                    else
                                    {
                                        RecipeViewModel.LocationId = 0;
                                        RecipeViewModel.LocationCode = locationcode;
                                    }
                                    // LocationId
                                    var sp = worksheet.Cells[rowIterator, 5].Value;
                                    if (sp == null) { sp = 0; }
                                    RecipeViewModel.SellingPrice = Convert.ToDecimal(sp);

                                    RecipeViewModel.MaterialCode = Convert.ToString(worksheet.Cells[rowIterator, 6].Value).Trim();

                                    var matqty = worksheet.Cells[rowIterator, 7].Value;
                                    if (matqty == null) { matqty = 0; }
                                    RecipeViewModel.Quantity = Convert.ToDecimal(matqty);

                                    var recqty = worksheet.Cells[rowIterator, 4].Value;
                                    if (recqty == null) { recqty = 0; }
                                    RecipeViewModel.RecipeQuantity = Convert.ToDecimal(recqty);

                                    listRecipeUpload.Add(RecipeViewModel);
                                }
                            }
                        }
                    }
                }

                //if (listproduct.Count==0)
                //{
                //         ViewBag.statuscode = 0;
                //         ViewBag.status ="Fill the ProductStock sheet in excel file.";
                //        return View("~/Views/Product/UploadProducts.cshtml", new DataUploadViewModel());
                // }

                ProductUploadViewModel productuploadviewmodel = new ProductUploadViewModel();
                productuploadviewmodel.ProductList = listproduct;
                productuploadviewmodel.ProductStockMasterList = listproductstockmaster;
                productuploadviewmodel.SupplierProductList = listsupplierproducts;
                productuploadviewmodel.ProductTaxList = listproducttaxes;
                productuploadviewmodel.RecipeList = listRecipe;
                productuploadviewmodel.RecipeUploadList = listRecipeUpload;
                productuploadviewmodel.CompanyId = Convert.ToInt32(Session["loggedusercompanyId"].ToString());

                productuploadviewmodel.ProductList.ForEach(p =>
                {
                    p.IsActive = true;
                    p.IsDelete = false;
                    p.CreatedUser = Convert.ToString(Session["loggeduser"]);
                    p.CreatedDate = DateTime.Now;
                    p.KitchenCode = string.Empty;
                    p.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
                    p.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
                });
                productuploadviewmodel.ProductStockMasterList.ForEach(p =>
                {
                    p.IsActive = true;
                    p.IsDelete = false;
                    p.CreatedUser = Convert.ToString(Session["loggeduser"]);
                    p.CreatedDate = DateTime.Now;
                    p.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
                    p.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
                });
                productuploadviewmodel.SupplierProductList.ForEach(p =>
                {
                    p.CreatedUser = Convert.ToString(Session["loggeduser"]);
                    p.CreatedDate = DateTime.Now;
                    p.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
                    p.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
                });
                productuploadviewmodel.ProductTaxList.ForEach(p =>
                {
                    p.CreatedUser = Convert.ToString(Session["loggeduser"]);
                    p.CreatedDate = DateTime.Now;
                    p.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
                    p.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
                });
                productuploadviewmodel.RecipeList.ForEach(p =>
                {
                    p.CreatedUser = Convert.ToString(Session["loggeduser"]);
                    p.CreateDate = DateTime.Now;
                    p.CompanyId = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
                });
                productuploadviewmodel.RecipeUploadList.ForEach(p =>
                {
                    p.CreatedUser = Convert.ToString(Session["loggeduser"]);
                    p.CreateDate = DateTime.Now;
                    p.CompanyId = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
                });
                if (worksheetNew == "SavingUnitPriceChange")
                {
                    if (Request != null)
                    {
                        List<ProductServingUnit> ProductServingUnitList = new List<ProductServingUnit>();
                        if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                        {
                            string fileName = file.FileName;
                            string fileContentType = file.ContentType;
                            byte[] fileBytes = new byte[file.ContentLength];
                            var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));

                            using (var package = new ExcelPackage(file.InputStream))
                            {
                                var sheets = package.Workbook.Worksheets;
                                foreach (var currentSheet in sheets)
                                {
                                    var worksheet = currentSheet;
                                    int colcount = 0;
                                    int rowcount = 0;
                                    if (worksheet.Dimension != null)
                                    {
                                        colcount = worksheet.Dimension.End.Column;
                                        rowcount = worksheet.Dimension.End.Row;
                                    }
                                    for (int rowIterator = 2; rowIterator <= rowcount; rowIterator++)
                                    {
                                        if (worksheet.Cells[rowIterator, 1].Value != null && worksheet.Cells[rowIterator, 2].Value != null && worksheet.Cells[rowIterator, 3].Value != null && worksheet.Cells[rowIterator, 4].Value != null && worksheet.Cells[rowIterator, 5].Value != null && worksheet.Cells[rowIterator, 6].Value != null)
                                        {
                                            decimal d;
                                            if (decimal.TryParse(worksheet.Cells[rowIterator, 4].Value.ToString(), out d))
                                            {
                                                decimal d2;
                                                if (decimal.TryParse(worksheet.Cells[rowIterator, 5].Value.ToString(), out d2))
                                                {
                                                    ProductServingUnit ProductServingUnit = new ProductServingUnit();
                                                    ProductServingUnit.ProductServingUnitId = Convert.ToInt32(worksheet.Cells[rowIterator, 6].Value);
                                                    ProductServingUnit.ProductId = Convert.ToInt64(worksheet.Cells[rowIterator, 2].Value);
                                                    ProductServingUnit.ServingUnit = worksheet.Cells[rowIterator, 3].Value.ToString();
                                                    ProductServingUnit.CostPrice = Convert.ToDecimal(worksheet.Cells[rowIterator, 4].Value);
                                                    ProductServingUnit.SellingPrice = Convert.ToDecimal(worksheet.Cells[rowIterator, 5].Value);
                                                    ProductServingUnit.LocationId = Convert.ToInt32(worksheet.Cells[rowIterator, 1].Value);
                                                    ProductServingUnit.CreatedUser = Convert.ToString(Session["loggeduser"]);
                                                    ProductServingUnit.CreatedDate = DateTime.Now;
                                                    ProductServingUnitList.Add(ProductServingUnit);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            var res = _bllproduct.ProductSavingUnitsExcelUpload(ProductServingUnitList, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                            if (res.Item2 == 1)
                            {
                                ViewBag.status = res.Item1;
                                ViewBag.statuscode = res.Item2;
                            }
                            else
                            {
                                ViewBag.status = res.Item1;
                                ViewBag.statuscode = res.Item2;
                            }
                        }
                    }
                }
                if (worksheetNew == "TakeawayUber")
                {
                    if (Request != null)
                    {
                        List<ProductServingUnit> ProductServingUnitList = new List<ProductServingUnit>();
                        if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                        {
                            string fileName = file.FileName;
                            string fileContentType = file.ContentType;
                            byte[] fileBytes = new byte[file.ContentLength];
                            var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));

                            using (var package = new ExcelPackage(file.InputStream))
                            {
                                var sheets = package.Workbook.Worksheets;
                                foreach (var currentSheet in sheets)
                                {
                                    var worksheet = currentSheet;
                                    int colcount = 0;
                                    int rowcount = 0;
                                    if (worksheet.Dimension != null)
                                    {
                                        colcount = worksheet.Dimension.End.Column;
                                        rowcount = worksheet.Dimension.End.Row;
                                    }
                                    //PRODUCT SERVING UNIT ID   
                                    int DineinSmall = Convert.ToInt32(_bllproduct.GetServingUnits("Dine in Small").ServingUnitId);
                                    int DineinLarge = Convert.ToInt32(_bllproduct.GetServingUnits("Dine in Large").ServingUnitId);
                                    int TakeAwaySmall = Convert.ToInt32(_bllproduct.GetServingUnits("Take Away Small").ServingUnitId);
                                    int TakeAwayLarge = Convert.ToInt32(_bllproduct.GetServingUnits("Take Away Large").ServingUnitId);
                                    int ACSmall = Convert.ToInt32(_bllproduct.GetServingUnits("AC Small").ServingUnitId);
                                    int ACLarge = Convert.ToInt32(_bllproduct.GetServingUnits("AC Large").ServingUnitId);
                                    //
                                    for (int rowIterator = 2; rowIterator <= rowcount; rowIterator++)
                                    {
                                        if (worksheet.Cells[rowIterator, 1].Value != null && worksheet.Cells[rowIterator, 2].Value != null && worksheet.Cells[rowIterator, 3].Value != null
                                            && worksheet.Cells[rowIterator, 4].Value != null && worksheet.Cells[rowIterator, 5].Value != null && worksheet.Cells[rowIterator, 6].Value != null
                                            && worksheet.Cells[rowIterator, 7].Value != null && worksheet.Cells[rowIterator, 8].Value != null && worksheet.Cells[rowIterator, 9].Value != null)
                                        {
                                            decimal d;
                                            if (decimal.TryParse(worksheet.Cells[rowIterator, 4].Value.ToString(), out d))
                                            {
                                                if (d > 0)
                                                {
                                                    ProductServingUnit ProductServingUnit = new ProductServingUnit();
                                                    ProductServingUnit.ProductServingUnitId = DineinSmall;
                                                    ProductServingUnit.ProductId = Convert.ToInt64(worksheet.Cells[rowIterator, 1].Value);
                                                    ProductServingUnit.ServingUnit = "Dine in Small";
                                                    ProductServingUnit.CostPrice = Convert.ToDecimal(worksheet.Cells[rowIterator, 4].Value);
                                                    ProductServingUnit.SellingPrice = Convert.ToDecimal(worksheet.Cells[rowIterator, 4].Value);
                                                    ProductServingUnit.LocationId = Convert.ToInt32(worksheet.Cells[rowIterator, 3].Value);
                                                    ProductServingUnit.CreatedUser = Convert.ToString(Session["loggeduser"]);
                                                    ProductServingUnit.CreatedDate = DateTime.Now;
                                                    ProductServingUnitList.Add(ProductServingUnit);
                                                }
                                            }
                                            decimal d2;
                                            if (decimal.TryParse(worksheet.Cells[rowIterator, 5].Value.ToString(), out d2))
                                            {
                                                if (d2 > 0)
                                                {
                                                    ProductServingUnit ProductServingUnit = new ProductServingUnit();
                                                    ProductServingUnit.ProductServingUnitId = DineinLarge;
                                                    ProductServingUnit.ProductId = Convert.ToInt64(worksheet.Cells[rowIterator, 1].Value);
                                                    ProductServingUnit.ServingUnit = "Dine in Large";
                                                    ProductServingUnit.CostPrice = Convert.ToDecimal(worksheet.Cells[rowIterator, 5].Value);
                                                    ProductServingUnit.SellingPrice = Convert.ToDecimal(worksheet.Cells[rowIterator, 5].Value);
                                                    ProductServingUnit.LocationId = Convert.ToInt32(worksheet.Cells[rowIterator, 3].Value);
                                                    ProductServingUnit.CreatedUser = Convert.ToString(Session["loggeduser"]);
                                                    ProductServingUnit.CreatedDate = DateTime.Now;
                                                    ProductServingUnitList.Add(ProductServingUnit);
                                                }
                                            }
                                            decimal d3;
                                            if (decimal.TryParse(worksheet.Cells[rowIterator, 6].Value.ToString(), out d3))
                                            {
                                                if (d3 > 0)
                                                {
                                                    ProductServingUnit ProductServingUnit = new ProductServingUnit();
                                                    ProductServingUnit.ProductServingUnitId = TakeAwaySmall;
                                                    ProductServingUnit.ProductId = Convert.ToInt64(worksheet.Cells[rowIterator, 1].Value);
                                                    ProductServingUnit.ServingUnit = "Take Away Small";
                                                    ProductServingUnit.CostPrice = Convert.ToDecimal(worksheet.Cells[rowIterator, 6].Value);
                                                    ProductServingUnit.SellingPrice = Convert.ToDecimal(worksheet.Cells[rowIterator, 6].Value);
                                                    ProductServingUnit.LocationId = Convert.ToInt32(worksheet.Cells[rowIterator, 3].Value);
                                                    ProductServingUnit.CreatedUser = Convert.ToString(Session["loggeduser"]);
                                                    ProductServingUnit.CreatedDate = DateTime.Now;
                                                    ProductServingUnitList.Add(ProductServingUnit);
                                                }
                                            }
                                            decimal d4;
                                            if (decimal.TryParse(worksheet.Cells[rowIterator, 7].Value.ToString(), out d4))
                                            {
                                                if (d4 > 0)
                                                {
                                                    ProductServingUnit ProductServingUnit = new ProductServingUnit();
                                                    ProductServingUnit.ProductServingUnitId = TakeAwayLarge;
                                                    ProductServingUnit.ProductId = Convert.ToInt64(worksheet.Cells[rowIterator, 1].Value);
                                                    ProductServingUnit.ServingUnit = "Take Away Large";
                                                    ProductServingUnit.CostPrice = Convert.ToDecimal(worksheet.Cells[rowIterator, 7].Value);
                                                    ProductServingUnit.SellingPrice = Convert.ToDecimal(worksheet.Cells[rowIterator, 7].Value);
                                                    ProductServingUnit.LocationId = Convert.ToInt32(worksheet.Cells[rowIterator, 3].Value);
                                                    ProductServingUnit.CreatedUser = Convert.ToString(Session["loggeduser"]);
                                                    ProductServingUnit.CreatedDate = DateTime.Now;
                                                    ProductServingUnitList.Add(ProductServingUnit);
                                                }
                                            }
                                            decimal d5;
                                            if (decimal.TryParse(worksheet.Cells[rowIterator, 8].Value.ToString(), out d5))
                                            {
                                                if (d5 > 0)
                                                {
                                                    ProductServingUnit ProductServingUnit = new ProductServingUnit();
                                                    ProductServingUnit.ProductServingUnitId = ACSmall;
                                                    ProductServingUnit.ProductId = Convert.ToInt64(worksheet.Cells[rowIterator, 1].Value);
                                                    ProductServingUnit.ServingUnit = "AC Small";
                                                    ProductServingUnit.CostPrice = Convert.ToDecimal(worksheet.Cells[rowIterator, 8].Value);
                                                    ProductServingUnit.SellingPrice = Convert.ToDecimal(worksheet.Cells[rowIterator, 8].Value);
                                                    ProductServingUnit.LocationId = Convert.ToInt32(worksheet.Cells[rowIterator, 3].Value);
                                                    ProductServingUnit.CreatedUser = Convert.ToString(Session["loggeduser"]);
                                                    ProductServingUnit.CreatedDate = DateTime.Now;
                                                    ProductServingUnitList.Add(ProductServingUnit);
                                                }
                                            }
                                            decimal d6;
                                            if (decimal.TryParse(worksheet.Cells[rowIterator, 9].Value.ToString(), out d6))
                                            {
                                                if (d6 > 0)
                                                {
                                                    ProductServingUnit ProductServingUnit = new ProductServingUnit();
                                                    ProductServingUnit.ProductServingUnitId = ACLarge;
                                                    ProductServingUnit.ProductId = Convert.ToInt64(worksheet.Cells[rowIterator, 1].Value);
                                                    ProductServingUnit.ServingUnit = "AC Large";
                                                    ProductServingUnit.CostPrice = Convert.ToDecimal(worksheet.Cells[rowIterator, 9].Value);
                                                    ProductServingUnit.SellingPrice = Convert.ToDecimal(worksheet.Cells[rowIterator, 9].Value);
                                                    ProductServingUnit.LocationId = Convert.ToInt32(worksheet.Cells[rowIterator, 3].Value);
                                                    ProductServingUnit.CreatedUser = Convert.ToString(Session["loggeduser"]);
                                                    ProductServingUnit.CreatedDate = DateTime.Now;
                                                    ProductServingUnitList.Add(ProductServingUnit);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            var res = _bllproduct.ProductSavingUnitsExcelUpload(ProductServingUnitList, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                            if (res.Item2 == 1)
                            {
                                ViewBag.status = res.Item1;
                                ViewBag.statuscode = res.Item2;
                            }
                            else
                            {
                                ViewBag.status = res.Item1;
                                ViewBag.statuscode = res.Item2;
                            }
                        }
                    }
                }
                else
                {
                    var res = _bllproduct.ProductExcelUpload(productuploadviewmodel);
                    if (res.Item2 == 1)
                    {
                        ViewBag.status = "The records have been successfully updated.";
                        ViewBag.statuscode = res.Item2;
                        Session["ProductUploadedFile"] = null;
                    }
                    else
                    {
                        ViewBag.status = res.Item1;
                        ViewBag.statuscode = res.Item2;
                    }
                }
            }

            return View("~/Views/Product/UploadProducts.cshtml", new DataUploadViewModel());
        }

        [HttpPost]
        public ActionResult UploadProductsPriceChnageFromExcel(FormCollection formCollection)
        {
            string worksheetNew = "";
            if (Request != null)
            {
                HttpPostedFileBase file = Request.Files["ProductUploadFile"];
                if (file.FileName == string.Empty)
                {
                    ViewBag.statuscode = 0;
                    ViewBag.status = "Browse the Excel File and Upload..";
                    return View("~/Views/Product/UploadProductsPriceChnage.cshtml", new DataUploadViewModel());
                }

                string docpath = ConfigurationManager.AppSettings["ExcelPath"].ToString();
                string path = Server.MapPath(docpath);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                var k = DateTime.Now.ToString().Replace('/', '-').Trim().Replace(':', ' ') + " " + Convert.ToString(Session["loggeduser"]) + " ";
                file.SaveAs(path + Path.GetFileName(k + file.FileName));

                List<Product> listproduct = new List<Product>();
                List<ProductStockMaster> listproductstockmaster = new List<ProductStockMaster>();
                List<SupplierProduct> listsupplierproducts = new List<SupplierProduct>();
                List<ProductTax> listproducttaxes = new List<ProductTax>();
                List<ReceipeViewModel> listRecipe = new List<ReceipeViewModel>();
                List<ReceipeViewModel> listRecipeUpload = new List<ReceipeViewModel>();

                if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                {
                    string fileName = file.FileName;
                    string fileContentType = file.ContentType;
                    byte[] fileBytes = new byte[file.ContentLength];
                    var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));

                    using (var package = new ExcelPackage(file.InputStream))
                    {
                        var sheets = package.Workbook.Worksheets;
                        foreach (var currentSheet in sheets)
                        {
                            var worksheet = currentSheet;
                            int colcount = 0;
                            int rowcount = 0;
                            if (worksheet.Dimension != null)
                            {
                                colcount = worksheet.Dimension.End.Column;
                                rowcount = worksheet.Dimension.End.Row;
                            }
                            for (int rowIterator = 2; rowIterator <= rowcount; rowIterator++)
                            {
                                if (worksheet.Name == "ProductSupplierLocation")
                                {
                                    Product product = new Product();
                                    ProductStockMaster productstockmaster = new ProductStockMaster();
                                    SupplierProduct supplierproduct = new SupplierProduct();

                                    product.ProductCode = Convert.ToString(worksheet.Cells[rowIterator, 1].Value).Trim();
                                    product.ProductName = Convert.ToString(worksheet.Cells[rowIterator, 2].Value);
                                    product.NameOnInvoice = Convert.ToString(worksheet.Cells[rowIterator, 3].Value);
                                    product.IsRowMaterial = Convert.ToBoolean(worksheet.Cells[rowIterator, 4].Value);
                                    product.IsScaleItem = Convert.ToBoolean(worksheet.Cells[rowIterator, 5].Value);

                                    // departments
                                    string deptcode = Convert.ToString(worksheet.Cells[rowIterator, 6].Value).Trim();
                                    var department = _blldepartment.GetDeptByCode(deptcode, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                                    if (department != null)
                                    {
                                        product.DepartmentId = department.RstDepartmentID;
                                        product.DepartmnetCode = department.DepartmentCode;
                                    }
                                    else
                                    {
                                        product.DepartmentId = 0;
                                        product.DepartmnetCode = deptcode;
                                    }
                                    // end departments

                                    // category
                                    string catcode = Convert.ToString(worksheet.Cells[rowIterator, 7].Value).Trim();
                                    var category = _bllcategory.GetCatByCode(catcode, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                                    if (category != null)
                                    {
                                        product.CategoryId = category.RstCategoryID;
                                        product.CategoryCode = category.RstCategoryCode;
                                    }
                                    else
                                    {
                                        product.CategoryId = 0;
                                        product.CategoryCode = catcode;
                                    }

                                    // end category

                                    // Sub category
                                    string subcatcode = Convert.ToString(worksheet.Cells[rowIterator, 8].Value).Trim();
                                    var subcategory = _bllsubcategory.GetSubCatByCode(subcatcode, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                                    if (subcategory != null)
                                    {
                                        product.SubCategoryId = subcategory.RstSubCategoryID;
                                        product.SubCategoryCode = subcategory.RstSubCategoryCode;
                                    }
                                    else
                                    {
                                        product.SubCategoryId = 0;
                                        product.SubCategoryCode = subcatcode;
                                    }
                                    // end Sub Category

                                    // UOM
                                    string UOMCode = Convert.ToString(worksheet.Cells[rowIterator, 9].Value).Trim();
                                    var UOM = _bllunitofmeasure.GetUnitOfMeasureByCode(UOMCode, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                                    if (UOM != null)
                                    {
                                        product.PurchasingUnit = (Int32)UOM.UnitOfMeasureId;
                                        product.UOMCode = UOM.UnitOfMeasureCode;
                                    }
                                    else
                                    {
                                        product.PurchasingUnit = 0;
                                        product.UOMCode = UOMCode;
                                    }
                                    // end UOM

                                    // Sub unit
                                    string SubUnitcode = Convert.ToString(worksheet.Cells[rowIterator, 10].Value).Trim();
                                    var SubUnit = _bllunitconversion.GetConversionByCode(SubUnitcode, product.PurchasingUnit, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                                    if (SubUnit != null)
                                    {
                                        product.WeightPerUnit = (Int32)SubUnit.UnitConversionId;
                                        product.SubUnitCode = SubUnit.SubUnit;
                                    }
                                    else
                                    {
                                        product.WeightPerUnit = 0;
                                        product.SubUnitCode = SubUnitcode;
                                    }
                                    // end Sub Unit

                                    // PrinterTypeID
                                    string printercode = Convert.ToString(worksheet.Cells[rowIterator, 11].Value).Trim();
                                    var printer = _bllproduct.GetPrinterByName(printercode);
                                    if (printer != null)
                                    {
                                        product.PrinterTypeId = (Int32)printer.PrinterTypeId;
                                        product.PrinterCode = printer.PrinterTypeName;
                                    }
                                    else
                                    {
                                        product.PrinterTypeId = 0;
                                        product.PrinterCode = printercode;
                                    }
                                    // PrinterTypeID
                                    product.IsDiscount = Convert.ToBoolean(worksheet.Cells[rowIterator, 12].Value);
                                    product.IsCostOnReceipe = Convert.ToBoolean(worksheet.Cells[rowIterator, 13].Value);
                                    product.IsAddon = Convert.ToBoolean(worksheet.Cells[rowIterator, 14].Value);
                                    product.IsPromotion = Convert.ToBoolean(worksheet.Cells[rowIterator, 15].Value);
                                    product.IsExpiry = Convert.ToBoolean(worksheet.Cells[rowIterator, 16].Value);
                                    product.IsTax = Convert.ToBoolean(worksheet.Cells[rowIterator, 17].Value);
                                    product.IsUnderCost = Convert.ToBoolean(worksheet.Cells[rowIterator, 18].Value);
                                    product.IsTaxInclude = Convert.ToBoolean(worksheet.Cells[rowIterator, 19].Value);
                                    product.IsOpenItem = Convert.ToBoolean(worksheet.Cells[rowIterator, 20].Value);
                                    product.AutoProduction = Convert.ToBoolean(worksheet.Cells[rowIterator, 21].Value);
                                    product.IsNoEffectCostforMenu = Convert.ToBoolean(worksheet.Cells[rowIterator, 22].Value);

                                    listproduct.Add(product);

                                    // Stock
                                    // LocationId
                                    string locationcode = Convert.ToString(worksheet.Cells[rowIterator, 24].Value).Trim();
                                    var location = _blllocation.GetLocByCode(locationcode, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                                    if (location != null)
                                    {
                                        productstockmaster.LocationId = (Int32)location.SysLocationID;
                                        productstockmaster.LocationCode = location.LocationCode;
                                    }
                                    else
                                    {
                                        productstockmaster.LocationId = 0;
                                        productstockmaster.LocationCode = locationcode;
                                    }
                                    // LocationId
                                    productstockmaster.IsActive = true;
                                    productstockmaster.CostCentreId = productstockmaster.LocationId;
                                    productstockmaster.StockCode = product.ProductCode;
                                    productstockmaster.ProductCode = product.ProductCode;
                                    productstockmaster.CostPrice = Convert.ToDecimal(worksheet.Cells[rowIterator, 26].Value);
                                    productstockmaster.AvgCost = Convert.ToDecimal(worksheet.Cells[rowIterator, 27].Value);
                                    productstockmaster.Stock = Convert.ToDecimal(worksheet.Cells[rowIterator, 25].Value);
                                    productstockmaster.SellingPrice = Convert.ToDecimal(worksheet.Cells[rowIterator, 28].Value);
                                    productstockmaster.ReOrderLevel = Convert.ToDecimal(worksheet.Cells[rowIterator, 29].Value);
                                    productstockmaster.ReOrderQuantity = Convert.ToDecimal(worksheet.Cells[rowIterator, 30].Value);
                                    listproductstockmaster.Add(productstockmaster);

                                    string supplercode = Convert.ToString(worksheet.Cells[rowIterator, 23].Value).Trim();
                                    var supplier = _bllSupplier.GetSupplierByCode(supplercode, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                                    if (supplier != null)
                                    {
                                        supplierproduct.SupplierId = (Int32)supplier.SupplierID;
                                        supplierproduct.SupplierCode = supplier.SupplierCode;
                                    }
                                    else
                                    {
                                        supplierproduct.SupplierId = 0;
                                        supplierproduct.SupplierCode = supplercode;
                                    }
                                    supplierproduct.ProductCode = Convert.ToString(worksheet.Cells[rowIterator, 1].Value).Trim();
                                    listsupplierproducts.Add(supplierproduct);

                                    // End Stock
                                }
                                else if (worksheet.Name == "ProductSuppliers")
                                {
                                    SupplierProduct supplierproduct = new SupplierProduct();

                                    // supplier products
                                    string supplercode = Convert.ToString(worksheet.Cells[rowIterator, 2].Value).Trim();
                                    var supplier = _bllSupplier.GetSupplierByCode(supplercode, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                                    if (supplier != null)
                                    {
                                        supplierproduct.SupplierId = (Int32)supplier.SupplierID;
                                        supplierproduct.SupplierCode = supplier.SupplierCode;
                                    }
                                    else
                                    {
                                        supplierproduct.SupplierId = 0;
                                        supplierproduct.SupplierCode = supplercode;
                                    }
                                    supplierproduct.ProductCode = Convert.ToString(worksheet.Cells[rowIterator, 1].Value).Trim();
                                    listsupplierproducts.Add(supplierproduct);
                                    //end supplier products
                                }
                                else if (worksheet.Name == "SavingUnitPriceChange")
                                {
                                    worksheetNew = "SavingUnitPriceChange";
                                    ReceipeViewModel RecipeViewModel = new ReceipeViewModel();
                                    RecipeViewModel.ProductCode = Convert.ToString(worksheet.Cells[rowIterator, 2].Value).Trim();
                                    RecipeViewModel.ServingUnitName = Convert.ToString(worksheet.Cells[rowIterator, 4].Value).Trim();
                                    // Stock
                                    // LocationId
                                    string locationcode = Convert.ToString(worksheet.Cells[rowIterator, 1].Value).Trim();
                                    //var location = _blllocation.GetLocByCode(locationcode, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                                    //if (location != null)
                                    //{
                                    //RecipeViewModel.LocationId = (Int32)location.SysLocationID;
                                    //RecipeViewModel.LocationCode = location.LocationCode;
                                    //}
                                    //else
                                    //{
                                    RecipeViewModel.LocationId = 0;
                                    RecipeViewModel.LocationCode = locationcode;
                                    //}
                                    // LocationId

                                    //var cp = worksheet.Cells[rowIterator, 5].Value;
                                    //if (cp == null) { cp = 0; }
                                    //RecipeViewModel.CostPrice = Convert.ToDecimal(cp);

                                    //var sp = worksheet.Cells[rowIterator, 6].Value;
                                    //if (sp == null) { sp = 0; }
                                    //RecipeViewModel.SellingPrice = Convert.ToDecimal(sp);

                                    listRecipe.Add(RecipeViewModel);
                                }
                                else if (worksheet.Name == "TakeawayUber")
                                {
                                    worksheetNew = "TakeawayUber";
                                    ReceipeViewModel RecipeViewModel = new ReceipeViewModel();
                                    RecipeViewModel.ProductCode = Convert.ToString(worksheet.Cells[rowIterator, 1].Value).Trim();
                                    RecipeViewModel.ServingUnitName = Convert.ToString(worksheet.Cells[rowIterator, 2].Value).Trim();
                                    string locationcode = Convert.ToString(worksheet.Cells[rowIterator, 3].Value).Trim();
                                    RecipeViewModel.LocationId = Convert.ToInt32(worksheet.Cells[rowIterator, 3].Value);
                                    RecipeViewModel.LocationCode = locationcode;
                                    listRecipe.Add(RecipeViewModel);
                                }
                                else if (worksheet.Name == "ProductTaxes")
                                {
                                    ProductTax producttax = new ProductTax();

                                    // product taxes
                                    string taxcode = Convert.ToString(worksheet.Cells[rowIterator, 2].Value).Trim();
                                    var tax = _blltax.GetTaxByCode(taxcode, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                                    if (tax != null)
                                    {
                                        producttax.TaxId = (Int32)tax.TaxId;
                                        producttax.TaxCode = tax.TaxCode;
                                    }
                                    else
                                    {
                                        producttax.TaxId = 0;
                                        producttax.TaxCode = taxcode;
                                    }
                                    producttax.ProductCode = Convert.ToString(worksheet.Cells[rowIterator, 1].Value).Trim();
                                    listproducttaxes.Add(producttax);
                                    //end product taxes
                                }
                                else if (worksheet.Name == "RecipePriceChange")
                                {

                                    ReceipeViewModel RecipeViewModel = new ReceipeViewModel();
                                    RecipeViewModel.ProductCode = Convert.ToString(worksheet.Cells[rowIterator, 1].Value).Trim();
                                    RecipeViewModel.ServingUnitName = Convert.ToString(worksheet.Cells[rowIterator, 2].Value).Trim();
                                    // Stock
                                    // LocationId
                                    string locationcode = Convert.ToString(worksheet.Cells[rowIterator, 3].Value).Trim();
                                    var location = _blllocation.GetLocByCode(locationcode, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                                    if (location != null)
                                    {
                                        RecipeViewModel.LocationId = (Int32)location.SysLocationID;
                                        RecipeViewModel.LocationCode = location.LocationCode;
                                    }
                                    else
                                    {
                                        RecipeViewModel.LocationId = 0;
                                        RecipeViewModel.LocationCode = locationcode;
                                    }
                                    // LocationId

                                    var cp = worksheet.Cells[rowIterator, 4].Value;
                                    if (cp == null) { cp = 0; }
                                    RecipeViewModel.CostPrice = Convert.ToDecimal(cp);

                                    var sp = worksheet.Cells[rowIterator, 5].Value;
                                    if (sp == null) { sp = 0; }
                                    RecipeViewModel.SellingPrice = Convert.ToDecimal(sp);

                                    listRecipe.Add(RecipeViewModel);
                                    //end product taxes
                                }
                                else if (worksheet.Name == "RecipeUpload")
                                {
                                    ReceipeViewModel RecipeViewModel = new ReceipeViewModel();
                                    RecipeViewModel.ProductCode = Convert.ToString(worksheet.Cells[rowIterator, 2].Value).Trim();
                                    RecipeViewModel.ServingUnitName = Convert.ToString(worksheet.Cells[rowIterator, 3].Value).Trim();
                                    // Stock
                                    // LocationId
                                    string locationcode = Convert.ToString(worksheet.Cells[rowIterator, 1].Value).Trim();
                                    var location = _blllocation.GetLocByCode(locationcode, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                                    if (location != null)
                                    {
                                        RecipeViewModel.LocationId = (Int32)location.SysLocationID;
                                        RecipeViewModel.LocationCode = location.LocationCode;
                                    }
                                    else
                                    {
                                        RecipeViewModel.LocationId = 0;
                                        RecipeViewModel.LocationCode = locationcode;
                                    }
                                    // LocationId
                                    var sp = worksheet.Cells[rowIterator, 5].Value;
                                    if (sp == null) { sp = 0; }
                                    RecipeViewModel.SellingPrice = Convert.ToDecimal(sp);

                                    RecipeViewModel.MaterialCode = Convert.ToString(worksheet.Cells[rowIterator, 6].Value).Trim();

                                    var matqty = worksheet.Cells[rowIterator, 7].Value;
                                    if (matqty == null) { matqty = 0; }
                                    RecipeViewModel.Quantity = Convert.ToDecimal(matqty);

                                    var recqty = worksheet.Cells[rowIterator, 4].Value;
                                    if (recqty == null) { recqty = 0; }
                                    RecipeViewModel.RecipeQuantity = Convert.ToDecimal(recqty);

                                    listRecipeUpload.Add(RecipeViewModel);
                                }
                            }
                        }
                    }
                }

                //if (listproduct.Count==0)
                //{
                //         ViewBag.statuscode = 0;
                //         ViewBag.status ="Fill the ProductStock sheet in excel file.";
                //        return View("~/Views/Product/UploadProducts.cshtml", new DataUploadViewModel());
                // }

                ProductUploadViewModel productuploadviewmodel = new ProductUploadViewModel();
                productuploadviewmodel.ProductList = listproduct;
                productuploadviewmodel.ProductStockMasterList = listproductstockmaster;
                productuploadviewmodel.SupplierProductList = listsupplierproducts;
                productuploadviewmodel.ProductTaxList = listproducttaxes;
                productuploadviewmodel.RecipeList = listRecipe;
                productuploadviewmodel.RecipeUploadList = listRecipeUpload;
                productuploadviewmodel.CompanyId = Convert.ToInt32(Session["loggedusercompanyId"].ToString());

                productuploadviewmodel.ProductList.ForEach(p =>
                {
                    p.IsActive = true;
                    p.IsDelete = false;
                    p.CreatedUser = Convert.ToString(Session["loggeduser"]);
                    p.CreatedDate = DateTime.Now;
                    p.KitchenCode = string.Empty;
                    p.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
                    p.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
                });
                productuploadviewmodel.ProductStockMasterList.ForEach(p =>
                {
                    p.IsActive = true;
                    p.IsDelete = false;
                    p.CreatedUser = Convert.ToString(Session["loggeduser"]);
                    p.CreatedDate = DateTime.Now;
                    p.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
                    p.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
                });
                productuploadviewmodel.SupplierProductList.ForEach(p =>
                {
                    p.CreatedUser = Convert.ToString(Session["loggeduser"]);
                    p.CreatedDate = DateTime.Now;
                    p.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
                    p.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
                });
                productuploadviewmodel.ProductTaxList.ForEach(p =>
                {
                    p.CreatedUser = Convert.ToString(Session["loggeduser"]);
                    p.CreatedDate = DateTime.Now;
                    p.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
                    p.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
                });
                productuploadviewmodel.RecipeList.ForEach(p =>
                {
                    p.CreatedUser = Convert.ToString(Session["loggeduser"]);
                    p.CreateDate = DateTime.Now;
                    p.CompanyId = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
                });
                productuploadviewmodel.RecipeUploadList.ForEach(p =>
                {
                    p.CreatedUser = Convert.ToString(Session["loggeduser"]);
                    p.CreateDate = DateTime.Now;
                    p.CompanyId = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
                });
                if (worksheetNew == "SavingUnitPriceChange")
                {
                    if (Request != null)
                    {
                        List<ProductServingUnit> ProductServingUnitList = new List<ProductServingUnit>();
                        if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                        {
                            string fileName = file.FileName;
                            string fileContentType = file.ContentType;
                            byte[] fileBytes = new byte[file.ContentLength];
                            var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));

                            using (var package = new ExcelPackage(file.InputStream))
                            {
                                var sheets = package.Workbook.Worksheets;
                                foreach (var currentSheet in sheets)
                                {
                                    var worksheet = currentSheet;
                                    int colcount = 0;
                                    int rowcount = 0;
                                    if (worksheet.Dimension != null)
                                    {
                                        colcount = worksheet.Dimension.End.Column;
                                        rowcount = worksheet.Dimension.End.Row;
                                    }
                                    for (int rowIterator = 2; rowIterator <= rowcount; rowIterator++)
                                    {
                                        if (worksheet.Cells[rowIterator, 1].Value != null && worksheet.Cells[rowIterator, 2].Value != null && worksheet.Cells[rowIterator, 3].Value != null && worksheet.Cells[rowIterator, 4].Value != null && worksheet.Cells[rowIterator, 5].Value != null && worksheet.Cells[rowIterator, 6].Value != null)
                                        {
                                            decimal d;
                                            if (decimal.TryParse(worksheet.Cells[rowIterator, 4].Value.ToString(), out d))
                                            {
                                                decimal d2;
                                                if (decimal.TryParse(worksheet.Cells[rowIterator, 5].Value.ToString(), out d2))
                                                {
                                                    ProductServingUnit ProductServingUnit = new ProductServingUnit();
                                                    ProductServingUnit.ProductServingUnitId = Convert.ToInt32(worksheet.Cells[rowIterator, 6].Value);
                                                    ProductServingUnit.ProductId = Convert.ToInt64(worksheet.Cells[rowIterator, 2].Value);
                                                    ProductServingUnit.ServingUnit = worksheet.Cells[rowIterator, 3].Value.ToString();
                                                    ProductServingUnit.CostPrice = Convert.ToDecimal(worksheet.Cells[rowIterator, 4].Value);
                                                    ProductServingUnit.SellingPrice = Convert.ToDecimal(worksheet.Cells[rowIterator, 5].Value);
                                                    ProductServingUnit.LocationId = Convert.ToInt32(worksheet.Cells[rowIterator, 1].Value);
                                                    ProductServingUnit.CreatedUser = Convert.ToString(Session["loggeduser"]);
                                                    ProductServingUnit.CreatedDate = DateTime.Now;
                                                    ProductServingUnitList.Add(ProductServingUnit);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            var res = _bllproduct.ProductSavingUnitsExcelUpload(ProductServingUnitList, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                            if (res.Item2 == 1)
                            {
                                ViewBag.status = res.Item1;
                                ViewBag.statuscode = res.Item2;
                            }
                            else
                            {
                                ViewBag.status = res.Item1;
                                ViewBag.statuscode = res.Item2;
                            }
                        }
                    }
                }
                if (worksheetNew == "TakeawayUber")
                {
                    if (Request != null)
                    {
                        List<ProductServingUnit> ProductServingUnitList = new List<ProductServingUnit>();
                        if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                        {
                            string fileName = file.FileName;
                            string fileContentType = file.ContentType;
                            byte[] fileBytes = new byte[file.ContentLength];
                            var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));

                            using (var package = new ExcelPackage(file.InputStream))
                            {
                                var sheets = package.Workbook.Worksheets;
                                foreach (var currentSheet in sheets)
                                {
                                    var worksheet = currentSheet;
                                    int colcount = 0;
                                    int rowcount = 0;
                                    if (worksheet.Dimension != null)
                                    {
                                        colcount = worksheet.Dimension.End.Column;
                                        rowcount = worksheet.Dimension.End.Row;
                                    }
                                    //PRODUCT SERVING UNIT ID   
                                    int DineinSmall = Convert.ToInt32(_bllproduct.GetServingUnits("Dine in Small").ServingUnitId);
                                    int DineinLarge = Convert.ToInt32(_bllproduct.GetServingUnits("Dine in Large").ServingUnitId);
                                    int TakeAwaySmall = Convert.ToInt32(_bllproduct.GetServingUnits("Take Away Small").ServingUnitId);
                                    int TakeAwayLarge = Convert.ToInt32(_bllproduct.GetServingUnits("Take Away Large").ServingUnitId);
                                    int ACSmall = Convert.ToInt32(_bllproduct.GetServingUnits("AC Small").ServingUnitId);
                                    int ACLarge = Convert.ToInt32(_bllproduct.GetServingUnits("AC Large").ServingUnitId);
                                    //
                                    for (int rowIterator = 2; rowIterator <= rowcount; rowIterator++)
                                    {
                                        if (worksheet.Cells[rowIterator, 1].Value != null && worksheet.Cells[rowIterator, 2].Value != null && worksheet.Cells[rowIterator, 3].Value != null
                                            && worksheet.Cells[rowIterator, 4].Value != null && worksheet.Cells[rowIterator, 5].Value != null && worksheet.Cells[rowIterator, 6].Value != null
                                            && worksheet.Cells[rowIterator, 7].Value != null && worksheet.Cells[rowIterator, 8].Value != null && worksheet.Cells[rowIterator, 9].Value != null)
                                        {
                                            decimal d;
                                            if (decimal.TryParse(worksheet.Cells[rowIterator, 4].Value.ToString(), out d))
                                            {
                                                if (d > 0)
                                                {
                                                    ProductServingUnit ProductServingUnit = new ProductServingUnit();
                                                    ProductServingUnit.ProductServingUnitId = DineinSmall;
                                                    ProductServingUnit.ProductId = Convert.ToInt64(worksheet.Cells[rowIterator, 1].Value);
                                                    ProductServingUnit.ServingUnit = "Dine in Small";
                                                    ProductServingUnit.CostPrice = Convert.ToDecimal(worksheet.Cells[rowIterator, 4].Value);
                                                    ProductServingUnit.SellingPrice = Convert.ToDecimal(worksheet.Cells[rowIterator, 4].Value);
                                                    ProductServingUnit.LocationId = Convert.ToInt32(worksheet.Cells[rowIterator, 3].Value);
                                                    ProductServingUnit.CreatedUser = Convert.ToString(Session["loggeduser"]);
                                                    ProductServingUnit.CreatedDate = DateTime.Now;
                                                    ProductServingUnitList.Add(ProductServingUnit);
                                                }
                                            }
                                            decimal d2;
                                            if (decimal.TryParse(worksheet.Cells[rowIterator, 5].Value.ToString(), out d2))
                                            {
                                                if (d2 > 0)
                                                {
                                                    ProductServingUnit ProductServingUnit = new ProductServingUnit();
                                                    ProductServingUnit.ProductServingUnitId = DineinLarge;
                                                    ProductServingUnit.ProductId = Convert.ToInt64(worksheet.Cells[rowIterator, 1].Value);
                                                    ProductServingUnit.ServingUnit = "Dine in Large";
                                                    ProductServingUnit.CostPrice = Convert.ToDecimal(worksheet.Cells[rowIterator, 5].Value);
                                                    ProductServingUnit.SellingPrice = Convert.ToDecimal(worksheet.Cells[rowIterator, 5].Value);
                                                    ProductServingUnit.LocationId = Convert.ToInt32(worksheet.Cells[rowIterator, 3].Value);
                                                    ProductServingUnit.CreatedUser = Convert.ToString(Session["loggeduser"]);
                                                    ProductServingUnit.CreatedDate = DateTime.Now;
                                                    ProductServingUnitList.Add(ProductServingUnit);
                                                }
                                            }
                                            decimal d3;
                                            if (decimal.TryParse(worksheet.Cells[rowIterator, 6].Value.ToString(), out d3))
                                            {
                                                if (d3 > 0)
                                                {
                                                    ProductServingUnit ProductServingUnit = new ProductServingUnit();
                                                    ProductServingUnit.ProductServingUnitId = TakeAwaySmall;
                                                    ProductServingUnit.ProductId = Convert.ToInt64(worksheet.Cells[rowIterator, 1].Value);
                                                    ProductServingUnit.ServingUnit = "Take Away Small";
                                                    ProductServingUnit.CostPrice = Convert.ToDecimal(worksheet.Cells[rowIterator, 6].Value);
                                                    ProductServingUnit.SellingPrice = Convert.ToDecimal(worksheet.Cells[rowIterator, 6].Value);
                                                    ProductServingUnit.LocationId = Convert.ToInt32(worksheet.Cells[rowIterator, 3].Value);
                                                    ProductServingUnit.CreatedUser = Convert.ToString(Session["loggeduser"]);
                                                    ProductServingUnit.CreatedDate = DateTime.Now;
                                                    ProductServingUnitList.Add(ProductServingUnit);
                                                }
                                            }
                                            decimal d4;
                                            if (decimal.TryParse(worksheet.Cells[rowIterator, 7].Value.ToString(), out d4))
                                            {
                                                if (d4 > 0)
                                                {
                                                    ProductServingUnit ProductServingUnit = new ProductServingUnit();
                                                    ProductServingUnit.ProductServingUnitId = TakeAwayLarge;
                                                    ProductServingUnit.ProductId = Convert.ToInt64(worksheet.Cells[rowIterator, 1].Value);
                                                    ProductServingUnit.ServingUnit = "Take Away Large";
                                                    ProductServingUnit.CostPrice = Convert.ToDecimal(worksheet.Cells[rowIterator, 7].Value);
                                                    ProductServingUnit.SellingPrice = Convert.ToDecimal(worksheet.Cells[rowIterator, 7].Value);
                                                    ProductServingUnit.LocationId = Convert.ToInt32(worksheet.Cells[rowIterator, 3].Value);
                                                    ProductServingUnit.CreatedUser = Convert.ToString(Session["loggeduser"]);
                                                    ProductServingUnit.CreatedDate = DateTime.Now;
                                                    ProductServingUnitList.Add(ProductServingUnit);
                                                }
                                            }
                                            decimal d5;
                                            if (decimal.TryParse(worksheet.Cells[rowIterator, 8].Value.ToString(), out d5))
                                            {
                                                if (d5 > 0)
                                                {
                                                    ProductServingUnit ProductServingUnit = new ProductServingUnit();
                                                    ProductServingUnit.ProductServingUnitId = ACSmall;
                                                    ProductServingUnit.ProductId = Convert.ToInt64(worksheet.Cells[rowIterator, 1].Value);
                                                    ProductServingUnit.ServingUnit = "AC Small";
                                                    ProductServingUnit.CostPrice = Convert.ToDecimal(worksheet.Cells[rowIterator, 8].Value);
                                                    ProductServingUnit.SellingPrice = Convert.ToDecimal(worksheet.Cells[rowIterator, 8].Value);
                                                    ProductServingUnit.LocationId = Convert.ToInt32(worksheet.Cells[rowIterator, 3].Value);
                                                    ProductServingUnit.CreatedUser = Convert.ToString(Session["loggeduser"]);
                                                    ProductServingUnit.CreatedDate = DateTime.Now;
                                                    ProductServingUnitList.Add(ProductServingUnit);
                                                }
                                            }
                                            decimal d6;
                                            if (decimal.TryParse(worksheet.Cells[rowIterator, 9].Value.ToString(), out d6))
                                            {
                                                if (d6 > 0)
                                                {
                                                    ProductServingUnit ProductServingUnit = new ProductServingUnit();
                                                    ProductServingUnit.ProductServingUnitId = ACLarge;
                                                    ProductServingUnit.ProductId = Convert.ToInt64(worksheet.Cells[rowIterator, 1].Value);
                                                    ProductServingUnit.ServingUnit = "AC Large";
                                                    ProductServingUnit.CostPrice = Convert.ToDecimal(worksheet.Cells[rowIterator, 9].Value);
                                                    ProductServingUnit.SellingPrice = Convert.ToDecimal(worksheet.Cells[rowIterator, 9].Value);
                                                    ProductServingUnit.LocationId = Convert.ToInt32(worksheet.Cells[rowIterator, 3].Value);
                                                    ProductServingUnit.CreatedUser = Convert.ToString(Session["loggeduser"]);
                                                    ProductServingUnit.CreatedDate = DateTime.Now;
                                                    ProductServingUnitList.Add(ProductServingUnit);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            var res = _bllproduct.ProductSavingUnitsExcelUpload(ProductServingUnitList, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                            if (res.Item2 == 1)
                            {
                                ViewBag.status = res.Item1;
                                ViewBag.statuscode = res.Item2;
                            }
                            else
                            {
                                ViewBag.status = res.Item1;
                                ViewBag.statuscode = res.Item2;
                            }
                        }
                    }
                }
                else
                {
                    var res = _bllproduct.ProductExcelUpload(productuploadviewmodel);
                    if (res.Item2 == 1)
                    {
                        ViewBag.status = res.Item1;
                        ViewBag.statuscode = res.Item2;
                    }
                    else
                    {
                        ViewBag.status = res.Item1;
                        ViewBag.statuscode = res.Item2;
                    }
                }
            }

            return View("~/Views/Product/UploadProductsFromExcel.cshtml", new DataUploadViewModel());
        }

        [HttpPost]
        public ActionResult UploadStockFromExcel(FormCollection formCollection)
        {
            if (Request != null)
            {
                List<ProductStockMaster> liststock = new List<ProductStockMaster>();
                HttpPostedFileBase file = Request.Files["ProductStockUploadFile"];
                if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                {
                    string fileName = file.FileName;
                    string fileContentType = file.ContentType;
                    byte[] fileBytes = new byte[file.ContentLength];
                    var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));

                    using (var package = new ExcelPackage(file.InputStream))
                    {
                        var sheets = package.Workbook.Worksheets;
                        foreach (var currentSheet in sheets)
                        {
                            var worksheet = currentSheet;
                            int colcount = 0;
                            int rowcount = 0;
                            if (worksheet.Dimension != null)
                            {
                                colcount = worksheet.Dimension.End.Column;
                                rowcount = worksheet.Dimension.End.Row;
                            }
                            for (int rowIterator = 2; rowIterator <= rowcount; rowIterator++)
                            {
                                if (worksheet.Name == "Stock") // checks the tab name in excel workbook
                                {
                                    ProductStockMaster stock = new ProductStockMaster();

                                    //  stock.CostCentreId = Convert.ToInt32(worksheet.Cells[rowIterator, 1].Value);
                                    // location
                                    string locationcode = Convert.ToString(worksheet.Cells[rowIterator, 1].Value);
                                    var location = _blllocation.GetLocByCode(locationcode, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                                    if (location != null)
                                    {
                                        stock.CostCentreId = location.SysLocationID;
                                        stock.LocationCode = location.LocationCode;
                                        stock.LocationId = location.SysLocationID;
                                    }
                                    else
                                    {
                                        stock.CostCentreId = 0;
                                        stock.LocationCode = locationcode;
                                        stock.LocationId = 0;
                                    }
                                    // end location

                                    // product
                                    string productcode = Convert.ToString(worksheet.Cells[rowIterator, 2].Value);
                                    var product = _bllproduct.GetProductByCode(productcode, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                                    if (product != null)
                                    {
                                        stock.ProductId = product.ProductId;
                                        stock.ProductCode = product.ProductCode;
                                        stock.StockCode = product.ProductCode;
                                        stock.ProductName = product.ProductName;
                                    }
                                    else
                                    {
                                        stock.ProductId = 0;
                                        stock.ProductCode = productcode;
                                        stock.StockCode = "";
                                        stock.ProductName = product.ProductName;
                                    }
                                    // end product
                                    stock.Stock = Convert.ToDecimal(worksheet.Cells[rowIterator, 3].Value);
                                    stock.CostPrice = Convert.ToDecimal(worksheet.Cells[rowIterator, 4].Value);
                                    stock.SellingPrice = Convert.ToDecimal(worksheet.Cells[rowIterator, 5].Value);
                                    stock.ReOrderLevel = Convert.ToDecimal(worksheet.Cells[rowIterator, 6].Value);
                                    stock.ReOrderQuantity = Convert.ToDecimal(worksheet.Cells[rowIterator, 7].Value);
                                    stock.ReOrderPeriod = Convert.ToDecimal(worksheet.Cells[rowIterator, 8].Value);

                                    //   stock.UomId = Convert.ToInt32(worksheet.Cells[rowIterator, 9].Value);

                                    // UOM
                                    string UOMCode = Convert.ToString(worksheet.Cells[rowIterator, 9].Value).Trim();
                                    var UOM = _bllunitofmeasure.GetUnitOfMeasureByCode(UOMCode, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                                    if (UOM != null)
                                    {
                                        stock.UomId = (Int32)UOM.UnitOfMeasureId;
                                        stock.UOMDesc = UOM.UnitOfMeasureCode;
                                    }
                                    else
                                    {
                                        stock.UomId = 0;
                                        stock.UOMDesc = UOMCode;
                                    }

                                    // Sub category
                                    string SubUnitcode = Convert.ToString(worksheet.Cells[rowIterator, 10].Value).Trim();
                                    var SubUnit = _bllunitconversion.GetConversionByCode(SubUnitcode, stock.UomId, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                                    if (SubUnit != null)
                                    {
                                        stock.WeightPerunit = (Int32)SubUnit.UnitConversionId;
                                        stock.SubUnit = SubUnit.SubUnit;
                                    }
                                    else
                                    {
                                        stock.WeightPerunit = 0;
                                        stock.SubUnit = SubUnitcode;
                                    }
                                    // end Sub Unit

                                    //    stock.WeightPerunit = Convert.ToInt32(worksheet.Cells[rowIterator, 10].Value);

                                    stock.DiscountPrc = Convert.ToDecimal(worksheet.Cells[rowIterator, 11].Value);
                                    stock.ForignCustomerPrice = Convert.ToDecimal(worksheet.Cells[rowIterator, 12].Value);
                                    stock.MaximumDiscount = Convert.ToDecimal(worksheet.Cells[rowIterator, 13].Value);
                                    stock.FixedDiscountPercentage = Convert.ToDecimal(worksheet.Cells[rowIterator, 14].Value);
                                    stock.FixedDiscountAmount = Convert.ToDecimal(worksheet.Cells[rowIterator, 15].Value);
                                    stock.MaximumDiscountPercentage = Convert.ToDecimal(worksheet.Cells[rowIterator, 16].Value);

                                    //audit
                                    stock.GroupOfCompanyID = 1;
                                    stock.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
                                    //   stock.LocationId = Convert.ToInt32(worksheet.Cells[rowIterator, 1].Value);
                                    stock.CreatedUser = Session["loggeduser"].ToString();
                                    stock.CreatedDate = DateTime.Now;
                                    stock.DataTransfer = 0;
                                    stock.IsActive = true;
                                    stock.LastUpdatedDate = DateTime.Now;
                                    //

                                    liststock.Add(stock);
                                }
                            }
                        }
                    }

                    var res = _bllproduct.StockExcelUpload(liststock, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                    if (res.Item2 == 1)
                    {
                        ViewBag.status = res.Item1;
                        ViewBag.statuscode = res.Item2;
                    }
                    else
                    {
                        ViewBag.status = res.Item1;
                        ViewBag.statuscode = res.Item2;
                    }
                }
            }
            return View("~/Views/Product/UploadProducts.cshtml");
        }

        [HttpPost]
        public ActionResult UploadSupplierFromExcel(FormCollection formCollection)
        {
            if (Request != null)
            {
                List<SupplierProduct> supplierproducts = new List<SupplierProduct>();
                HttpPostedFileBase file = Request.Files["ProductSupplierUploadFile"];
                if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                {
                    string fileName = file.FileName;
                    string fileContentType = file.ContentType;
                    byte[] fileBytes = new byte[file.ContentLength];
                    var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));

                    using (var package = new ExcelPackage(file.InputStream))
                    {
                        var sheets = package.Workbook.Worksheets;
                        foreach (var currentSheet in sheets)
                        {
                            var worksheet = currentSheet;
                            int colcount = 0;
                            int rowcount = 0;
                            if (worksheet.Dimension != null)
                            {
                                colcount = worksheet.Dimension.End.Column;
                                rowcount = worksheet.Dimension.End.Row;
                            }
                            for (int rowIterator = 2; rowIterator <= rowcount; rowIterator++)
                            {
                                if (worksheet.Name == "SupplierProducts") // checks the tab name in excel workbook
                                {
                                    SupplierProduct SupplierProduct = new SupplierProduct();

                                    SupplierProduct.SupplierId = Convert.ToInt32(worksheet.Cells[rowIterator, 1].Value);
                                    SupplierProduct.ProductId = Convert.ToInt32(worksheet.Cells[rowIterator, 2].Value);
                                    SupplierProduct.IsPreferredSupplier = Convert.ToBoolean(worksheet.Cells[rowIterator, 3].Value);

                                    //audit
                                    SupplierProduct.GroupOfCompanyID = 1;
                                    SupplierProduct.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
                                    SupplierProduct.LocationId = Convert.ToInt32(Session["loggeduserlocid"].ToString());
                                    SupplierProduct.CreatedUser = Session["loggeduser"].ToString();
                                    SupplierProduct.CreatedDate = DateTime.Now;
                                    SupplierProduct.DataTransfer = 0;
                                    //

                                    supplierproducts.Add(SupplierProduct);
                                }
                            }
                        }
                    }

                    var res = _bllproduct.SupplierProductExcelUpload(supplierproducts, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                    if (res.Item2 == 1)
                    {
                        ViewBag.status = res.Item1;
                        ViewBag.statuscode = res.Item2;
                    }
                    else
                    {
                        ViewBag.status = res.Item1;
                        ViewBag.statuscode = res.Item2;
                    }
                }
            }
            return View("~/Views/Product/UploadProducts.cshtml");
        }
        [HttpPost]
        public ActionResult UploadProductTaxesFromExcel(FormCollection formCollection)
        {
            if (Request != null)
            {
                List<ProductTax> producttaxeslist = new List<ProductTax>();
                HttpPostedFileBase file = Request.Files["ProductTaxesUploadFile"];
                if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                {
                    string fileName = file.FileName;
                    string fileContentType = file.ContentType;
                    byte[] fileBytes = new byte[file.ContentLength];
                    var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));

                    using (var package = new ExcelPackage(file.InputStream))
                    {
                        var sheets = package.Workbook.Worksheets;
                        foreach (var currentSheet in sheets)
                        {
                            var worksheet = currentSheet;
                            int colcount = 0;
                            int rowcount = 0;
                            if (worksheet.Dimension != null)
                            {
                                colcount = worksheet.Dimension.End.Column;
                                rowcount = worksheet.Dimension.End.Row;
                            }
                            for (int rowIterator = 2; rowIterator <= rowcount; rowIterator++)
                            {
                                if (worksheet.Name == "ProductTaxes") // checks the tab name in excel workbook
                                {
                                    ProductTax producttax = new ProductTax();

                                    producttax.ProductId = Convert.ToInt32(worksheet.Cells[rowIterator, 1].Value);
                                    producttax.TaxId = Convert.ToInt32(worksheet.Cells[rowIterator, 2].Value);
                                    producttax.TaxSequence = rowIterator - 1;

                                    //audit
                                    producttax.GroupOfCompanyID = 1;
                                    producttax.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
                                    producttax.LocationId = Convert.ToInt32(Session["loggeduserlocid"].ToString());
                                    producttax.CreatedUser = Session["loggeduser"].ToString();
                                    producttax.CreatedDate = DateTime.Now;
                                    producttax.DataTransfer = 0;
                                    //

                                    producttaxeslist.Add(producttax);
                                }
                            }
                        }
                    }

                    var res = _bllproduct.ProductTaxesExcelUpload(producttaxeslist, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                    if (res.Item2 == 1)
                    {
                        ViewBag.status = res.Item1;
                        ViewBag.statuscode = res.Item2;
                    }
                    else
                    {
                        ViewBag.status = res.Item1;
                        ViewBag.statuscode = res.Item2;
                    }
                }
            }
            return View("~/Views/Product/UploadProducts.cshtml");
        }

        // Loaders .....................................................................................//

        public JsonResult GetNotRawProducts()
        {
            //  ProductService reporsitory = new ProductService();
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var products = _bllproduct.GetNotRawProducts(companyid);
            return Json(JsonConvert.SerializeObject(products, Formatting.None, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetActiveInterDepartments()
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var products = _bllproduct.GetProductAddons(companyid);
            return Json(JsonConvert.SerializeObject(products, Formatting.None, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPrinterTypes()
        {
            var printertypes = _bllproduct.GetPrinterTypes();

            return Json(JsonConvert.SerializeObject(printertypes, Formatting.None,
                                new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }),
                                JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAddonCategories()
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var AddonCategories = _bllAddonCategory.GetAddonCategory(companyid);

            return Json(JsonConvert.SerializeObject(AddonCategories, Formatting.None,
                                new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }),
                                JsonRequestBehavior.AllowGet);
        }

        public JsonResult ReceipeDet(int id, decimal qty, decimal UnitConvertion, int locationid)
        {
            if (locationid == 0)
            {
                locationid = Convert.ToInt32(Session["loggeduserlocId"].ToString());
            }

            return new JsonResult
            {
                Data = _bllproduct.GetReceipeDetails(locationid, id, qty, UnitConvertion, Convert.ToInt32(Session["loggedusercompanyId"].ToString())),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult CostPriceByLocId(int id, decimal qty, decimal UnitConvertion, int locid)
        {
            return new JsonResult
            {
                Data = _bllproduct.GetReceipeDetails(locid, id, qty, UnitConvertion, Convert.ToInt32(Session["loggedusercompanyId"].ToString())),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult CostPriceForAllLocations(int productid, decimal qty, decimal UnitConvertion)
        {
            // int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var locationprices = _bllproduct.CostPriceForAllLocations(productid, qty, UnitConvertion, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));

            return Json(JsonConvert.SerializeObject(locationprices, Formatting.None,
                                new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }),
                                JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult VaidateProductCode(string productcode)
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var dbproductcode = _bllproduct.FindByCode(productcode, companyid);

            return new JsonResult { Data = dbproductcode, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public ActionResult GetRowMaterialsForReceipe()
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var rowmaterials = _bllproduct.GetRowMaterialsForRecipe(companyid);

            return Json(JsonConvert.SerializeObject(rowmaterials, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetProductById(long id)
        {
            var deptdata = _bllproduct.GetProductById(id);
            return new JsonResult { Data = deptdata, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpGet]
        public JsonResult GetAllActiveProducts()
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var deptdata = _bllproduct.GetActiveProducts(companyid);
            // return new JsonResult { Data = deptdata, JsonRequestBehavior = JsonRequestBehavior.AllowGet }

            ViewBag.productdata = deptdata;

            // JavaScriptSerializer jsJson = new JavaScriptSerializer();
            // jsJson.MaxJsonLength = 2147483644;

            return Json(JsonConvert.SerializeObject(deptdata, Formatting.None,
                        new JsonSerializerSettings
                        { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, }), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAllServingUnits()
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var ServingUnitsdata = _bllServingUnits.GetActiveServingUnits(companyid);
            return Json(JsonConvert.SerializeObject(ServingUnitsdata, Formatting.None,
                        new JsonSerializerSettings
                        { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult CheckReceipesExists(string ProductCode, string ProductServingUnit)
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var dbReceipes = _bllReceipe.CheckReceipesExist(ProductCode, ProductServingUnit, companyid);
            int rowcount = 0;
            if (dbReceipes.Count() == 0)
            {
                var rmcount = _bllproduct.DeleteServingUnitsByProductIdAndServingUnit(ProductCode, ProductServingUnit);
                rowcount = 1;
            }
            return new JsonResult { Data = rowcount, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpGet]
        public JsonResult GetReceipeByProductId(string productCode)
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var dbReceipes = _bllReceipe.CheckReceipesExistByProductCode(productCode, companyid);
            int rowcount = 0;
            if (dbReceipes.Count() == 0)
            {
                rowcount = 1;
            }
            return new JsonResult { Data = rowcount, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpGet]
        public JsonResult GetProductsByStockLocId(int stocklocid)
        {
            var stockproducts = _bllproduct.GetProductsStockByLocId(stocklocid, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            return Json(JsonConvert.SerializeObject(stockproducts, Formatting.None,
                        new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }),
                        JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult UpdateRecipe(int productid)
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            int locationid = Convert.ToInt32(Session["loggeduserlocid"].ToString());
            // var dbproductcode = _bllproduct.FindByCode(productcode, companyid);
            var recipecount = _bllproduct.UpdateRecipeCostPrice(productid, companyid, locationid);
            return new JsonResult { Data = recipecount, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public JsonResult GetActiveKitchens()
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var kitchens = _bllproduct.GetKitchens(companyid);
            return Json(JsonConvert.SerializeObject(kitchens, Formatting.None,
                        new JsonSerializerSettings
                        { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, }), JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetPrinters()
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            //int locationid = Convert.ToInt32(Session["loggeduserlocid"].ToString());
            //var kitchens = _bllproduct.GetPrinters(locationid);
            var kitchens = _bllproduct.GetPrinters();
            return Json(JsonConvert.SerializeObject(kitchens, Formatting.None,
                        new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, }), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPriceLevels()
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var PriceLevels = _bllproduct.GetPriceLevels();
            return Json(JsonConvert.SerializeObject(PriceLevels, Formatting.None, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, }), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetLocations()
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var loc = _blllocation.GetAllActiveLocations();
            return Json(JsonConvert.SerializeObject(loc, Formatting.None, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, }), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetUnitOfMeasures()
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var loc = _bllunitofmeasure.GetUnitOfMeasures(companyid);
            return Json(JsonConvert.SerializeObject(loc, Formatting.None, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, }), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAllServingUnits1()
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var loc = _bllServingUnits.GetAllServingUnits(companyid);
            return Json(JsonConvert.SerializeObject(loc, Formatting.None, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, }), JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetActiveKitchensByPrinterId(int id)
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var kitchens = _bllproduct.GetKitchens(companyid).Where(k => k.KitchenPrinterType == id);
            return Json(JsonConvert.SerializeObject(kitchens, Formatting.None,
                        new JsonSerializerSettings
                        { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, }), JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult UpdateTargetType(string TypeIdTargetPeriod)
        {
           
            return Json(new { success = true, selectedValue = TypeIdTargetPeriod });
        }

        [HttpPost]
        public ActionResult UpdateTargetPeriod(string TypeIdTargetType)
        {

            return Json(new { success = true, selectedValue = TypeIdTargetType });
        }


        //public JsonResult GetTargertPeriodId(int id)
        //{

        //    int TargertTypeID = 0;
        //    int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
        //    var TargertTypeId = id;// _bllproduct.GetKitchens(companyid).Where(k => k.KitchenPrinterType == id);
        //    TargertTypeID = TargertTypeId;
        //    Product product = new Product();

        //    return Json(JsonConvert.SerializeObject(TargertTypeId, Formatting.None,
        //                new JsonSerializerSettings
        //                { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, }), JsonRequestBehavior.AllowGet);
        //}

        //public JsonResult GetTargertTypeId(int id)
        //{
        //    Product x = new Product();

        //    string TragetTypeName = "";
        //    if(id == 1)
        //    {
        //        TragetTypeName = "Qty";
        //        x.Target_PeriodID = TragetTypeName;
        //    }
        //    else if(id == 2)
        //    {
        //        TragetTypeName = "Value";
        //        x.Target_PeriodID = TragetTypeName;
        //    }


        //    return Json(JsonConvert.SerializeObject(TragetTypeName, Formatting.None,
        //                new JsonSerializerSettings
        //                { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, }), JsonRequestBehavior.AllowGet);
        //}

        public ActionResult ExportToExcel(ProductStockViewModel vvm)
        {

            ProductStockViewModel t1 = StockViewModelSelection;

            vvm = TempData["SelectionCriteria"] as ProductStockViewModel;

            //var det = _bllcAlert.GetConfigDetails();
            byte[] excelFileArry = null;

            excelFileArry = GenerateExcelSheet(vvm);
            // Pass above test data and get excel file as byte array.
            try

            {
                //  byte[] document = this.StreamFile(filePath);
                Response.Clear();
                Response.AddHeader("content-disposition", "attachment;filename='" + "Stock Report" + "'" + Convert.ToString(DateTime.Now.ToFileTimeUtc()) + ".xls");
                Response.Charset = "";
                Response.Cache.SetNoServerCaching();
                Response.ContentType = "application/ms-excel";
                Response.BinaryWrite(excelFileArry);
                Response.End();
            }

            catch
            {
                try
                {
                    //stop processing the script and return the current result
                    Response.End();
                }

                catch (Exception)
                {
                }

                finally
                {
                    //Log.Error("inside web client -finally");
                    //Sends the response buffer
                    Response.Flush();
                    // Prevents any other content from being sent to the browser
                    Response.SuppressContent = true;
                    //Directs the thread to finish, bypassing additional processing
                    // Response.CompleteRequest();
                    //Suspends the current thread
                    Thread.Sleep(1);
                }
            }
            return View("Index");


        }

        private byte[] GenerateExcelSheet(ProductStockViewModel vvm)
        {
            using (ExcelPackage pck = new ExcelPackage())
            {
                //Create the worksheet
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Stock Report");
                #region Logo

                string filename = Server.MapPath(@"~/bin/LOGO.jpg");
                /*
                if (!File.Exists(filename))
                {
                   image.Save(filename);
                }*/

                try
                {

                    //AddImage(ws, 0, 0, filename);
                    ws.Cells[1, 1, 6, 1].Merge = true;
                }
                catch
                {

                }

                #endregion

                #region Company Detail
                //  BLL_Company _bllcompany=new BLL_Company();
                var stock = _bllproduct.GetStockReport(vvm.LocationId, vvm.ProductId, 1, vvm.StockCodeFrom, vvm.StockCodeTO).Where(s => s.Stock != 0);
                //   DataTable dtCompanyDetails = AR.GetCompanyDetails(1);
                foreach (var s in stock)
                {
                    ProductStockViewModel v = new ProductStockViewModel();
                    v.ProductId = s.ProductId;
                    v.ProductName = s.ProductName;
                    v.Location = _blllocation.GetLocationById(s.LocationId).LocationName;
                    v.ProductDbStock = s.Stock;
                    v.ProductCode = s.ProductCode;
                    v.ProductCostPrice = s.CostPrice;
                    v.AverageCostPrice = s.AvgCost; //--- Added By Nipuna Francisku #2619
                    v.AverageCostValue = (s.AvgCost * s.Stock);  //--- Added By Nipuna Francisku #2619

                    // total += s.CostPrice; // added by aruna
                }   //stockmodel.Add(v);

                //get the Document heading details

                string compName, Address1, Address2, Address3, Tele, Fax, website, ReportHead = "";
                compName = _blllocation.GetCompanyDetails().CompanyName;
                Address1 = _blllocation.GetCompanyDetails().Address1;
                Address2 = _blllocation.GetCompanyDetails().Address2;
                Address3 = _blllocation.GetCompanyDetails().Address3;
                Tele = _blllocation.GetCompanyDetails().Telephone;
                Fax = _blllocation.GetCompanyDetails().Fax;
                website = _blllocation.GetCompanyDetails().Website;

                if (stock != null)
                {
                    ws.Cells[1, 2].Value = compName;
                    ws.Cells[1, 2, 3, 12].Merge = true;
                    ws.Cells[1, 2, 3, 12].Style.Font.Size = 12;
                    ws.Cells[1, 2, 3, 12].Style.VerticalAlignment = ExcelVerticalAlignment.Bottom;

                    ws.Cells[4, 2].Value = Address1 + " " + Address2 + " " + Address3;
                    ;
                    ws.Cells[4, 2, 4, 12].Merge = true;
                    ws.Cells[4, 2, 4, 12].Style.Font.Size = 10;

                    ws.Cells[5, 2].Value = "Tel:- " + Tele + " / " + ",  Fax:- " + Fax + ",  Web Site:- " + website;
                    ws.Cells[5, 2, 5, 12].Merge = true;
                    ws.Cells[5, 2, 5, 12].Style.Font.Size = 10;

                    ws.Cells[6, 2].Value = "Stock Report";
                    ws.Cells[6, 2, 6, 12].Merge = true;
                    ws.Cells[6, 2, 6, 12].Style.Font.Size = 12;

                    var businessUnitDetail = ws.Cells[1, 2, 6, 12];
                    businessUnitDetail.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    businessUnitDetail.Style.Font.Bold = true;
                    businessUnitDetail.Style.Font.Name = "Calibri";
                }

                #endregion

                #region Print Detail Box

                var printDetailBox = ws.Cells[3, 13, 5, 14];
                printDetailBox.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                printDetailBox.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                printDetailBox.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                printDetailBox.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                printDetailBox.Style.Font.Size = 8;
                ws.Cells[3, 13, 5, 13].Style.Font.Bold = true;

                ws.Cells[3, 13].Value = "Date";
                ws.Cells[4, 13].Value = "Time";
                ws.Cells[5, 13].Value = "Req. By";

                // ws.Cells[3, 14].Value = DateTime.Now.Date.ToString("yyyy-MM-dd");

                ws.Cells[3, 14].Value = DateTime.Now.Date.ToShortDateString();

                ws.Cells[4, 14].Value = DateTime.Now.ToString("h:mm tt");

                if (Session["loggeduser"] != null)
                    ws.Cells[5, 14].Value = Session["loggeduser"].ToString();
                else
                    ws.Cells[5, 14].Value = " ";


                #endregion


                // #endregion

                ws.Cells[12, 1].Value = "Location";
                ws.Cells[12, 2].Value = "Product Code";
                ws.Cells[12, 3].Value = "Product Name";
                ws.Cells[12, 4].Value = "Stock";

                ws.Cells[12, 5].Value = "Cost Price";
                ws.Cells[12, 6].Value = "Average Cost Price";
                ws.Cells[12, 7].Value = "Cost Value";
                ws.Cells[12, 8].Value = "Average Cost Value";


                Color colFromHexHeading = System.Drawing.ColorTranslator.FromHtml("#919089");
                //ws.Cells[12, 1, 12, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
                //ws.Cells[12, 1, 12, 4].Style.Fill.BackgroundColor.SetColor(colFromHexHeading);

                ws.Cells[12, 1, 12, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[12, 1, 12, 8].Style.Fill.BackgroundColor.SetColor(colFromHexHeading);


                //var table = ws.Cells[12, 1, 12, 4];
                var table = ws.Cells[12, 1, 12, 8];
                table.Style.Border.Top.Style =
                table.Style.Border.Left.Style =
                table.Style.Border.Right.Style =
                table.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                table.Style.Font.Bold = true;
                table.Style.Font.Name = "Calibri";
                table.AutoFitColumns();


                // Set data.
                int rowIndex = 14;
                decimal TotalcostValueExcel = 0;
                decimal TotalAverageCostValueExcel = 0;
                foreach (var dr in stock) //
                {
                    string locname = "";
                    locname = _blllocation.GetLocationById(dr.LocationId).LocationName;
                    ws.Cells[rowIndex, 1].Value = locname;

                    ws.Cells[rowIndex, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    //  ws.Cells[rowIndex, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    ws.Cells[rowIndex, 2].Value = dr.ProductCode;
                    ws.Cells[rowIndex, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    //ws.Cells[rowIndex, 2].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    ws.Cells[rowIndex, 3].Value = dr.ProductName;
                    ws.Cells[rowIndex, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    //  ws.Cells[rowIndex,3].Style.VerticalAlignment = ExcelVerticalAlignment.Center;


                    ws.Cells[rowIndex, 4].Value = dr.Stock;
                    ws.Cells[rowIndex, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    //   ws.Cells[rowIndex, 4].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    ws.Cells[rowIndex, 5].Value = dr.CostPrice;
                    ws.Cells[rowIndex, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    ws.Cells[rowIndex, 6].Value = dr.AvgCost;
                    ws.Cells[rowIndex, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    ws.Cells[rowIndex, 7].Value = (dr.Stock * dr.CostPrice);
                    ws.Cells[rowIndex, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    ws.Cells[rowIndex, 8].Value = (dr.AvgCost * dr.Stock);
                    ws.Cells[rowIndex, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    rowIndex++;
                    TotalcostValueExcel += (dr.Stock * dr.CostPrice);// calculate average cost value added by Aruna
                    TotalAverageCostValueExcel += (dr.Stock * dr.AvgCost);// calculate average cost value added by thebuwana
                }
                //var table1 = ws.Cells[14, 1, rowIndex, 4];
                ws.Cells[rowIndex, 1].Value = "Total Cost value";
                ws.Cells[rowIndex, 7].Value = TotalcostValueExcel;
                ws.Cells[rowIndex, 8].Value = TotalAverageCostValueExcel;
                Color colFromHexHeading1 = System.Drawing.ColorTranslator.FromHtml("#919089");
                //ws.Cells[12, 1, 12, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
                //ws.Cells[12, 1, 12, 4].Style.Fill.BackgroundColor.SetColor(colFromHexHeading);

                ws.Cells[rowIndex, 1, rowIndex, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[rowIndex, 1, rowIndex, 8].Style.Fill.BackgroundColor.SetColor(colFromHexHeading);
                //var table2 = ws.Cells[rowIndex, 1, rowIndex, 8];
                //table2.Style.Border.Top.Style =
                //table2.Style.Border.Left.Style =
                //table2.Style.Border.Right.Style =
                //table2.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                //table2.AutoFitColumns();

                var table1 = ws.Cells[14, 1, rowIndex, 8];
                table1.Style.Border.Top.Style =
                table1.Style.Border.Left.Style =
                table1.Style.Border.Right.Style =
                table1.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                table1.AutoFitColumns();
                table1.Style.Font.Name = "Calibri";
                return pck.GetAsByteArray();
            }

        }
        public ActionResult Details(string UniInvNo)
        {

            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            List<ProductStockViewModel> stockmodel = new List<ProductStockViewModel>();
            // var stock = _bllproduct.GetActiveProducts(1);
            var stock = _bllproduct.GetStockReport(0, 0, companyid, "0", "0").Where(s => s.Stock != 0);
            foreach (var s in stock)
            {
                ProductStockViewModel v = new ProductStockViewModel();
                v.ProductId = s.ProductId;
                v.ProductName = s.ProductName;
                //v.Location = _blllocation.GetLocationById(s.LocationId).LocationName;
                v.ProductDbStock = s.Stock;
                v.ProductCode = s.ProductCode;
                v.ProductCostPrice = s.CostPrice;
                v.ProductSellingPrice = s.SellingPrice;

                //v.AverageCostPrice = s.AvgCost; //--- Added By Nipuna Francisku #2619
                // v.AverageCostValue = (s.AvgCost * s.Stock);  //--- Added By Nipuna Francisku #2619
                stockmodel.Add(v);
            }
            //return View("~/Views/Reports/Stock/SearchPopup.cshtml", stockmodel);
            return PartialView("~/Views/Reports/Stock/SearchPopup.cshtml", stockmodel);
        }

        [HttpGet]
        public ActionResult SearchByKeyword(string searchQuery)
        {
            if (searchQuery == "" || searchQuery == null)
            {
                searchQuery = "0";
            }

            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());

            var itemList = _bllproduct.GetStockReportForPopUpSearch(0, 0, companyid, searchQuery, "0").Where(s => s.Stock != 0);
            List<ProductStockViewModel> stockmodel = new List<ProductStockViewModel>();

            foreach (var s in itemList)
            {
                ProductStockViewModel v = new ProductStockViewModel();
                v.ProductId = s.ProductId;
                v.ProductName = s.ProductName;
                //v.Location = _blllocation.GetLocationById(s.LocationId).LocationName;
                v.ProductDbStock = s.Stock;
                v.ProductCode = s.ProductCode;
                v.ProductCostPrice = s.CostPrice;
                v.ProductSellingPrice = s.SellingPrice;

                //v.AverageCostPrice = s.AvgCost; //--- Added By Nipuna Francisku #2619
                // v.AverageCostValue = (s.AvgCost * s.Stock);  //--- Added By Nipuna Francisku #2619
                stockmodel.Add(v);
            }
            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                return View("~/Views/Reports/Stock/SearchPopup.cshtml", stockmodel);
                //return PartialView("~/Views/Reports/Stock/SearchPopup.cshtml", stockmodel);
            }

            return View("~/Views/Reports/Stock/SearchPopup.cshtml", stockmodel);
            //return PartialView("~/Views/Reports/Stock/SearchPopup.cshtml", stockmodel);
        }
    }
}