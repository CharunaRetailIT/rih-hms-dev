using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
//using HospitalityManagement.Service;
//using HospitalityManagement.Models;
using RIT.HMS.Domain;
using Newtonsoft.Json;
using RIT.HMS.BLL.MasterData;
using RIT.HMS.Domain.ViewModels;
using RIT.HMS.BLL.Configurations;
using OfficeOpenXml;
using System.IO;
using System.Threading;
using OfficeOpenXml.Style;
using System.Drawing;
using System.Data;

namespace HospitalityManagement.Controllers
{
    [SessionTimeout]
    public class ReceipeController : Controller
    {
        BLL_Receipe _bllReceipe;
        BLL_Location _bllLocation;
        BLL_Product _bllProduct;        
        private AppManager _appmanager;
        private readonly BLL_Configuration _bllconfiguration;
        private Receipe ReceipeViewModelSelection;
        private readonly BLL_Location _blllocation;

        public ReceipeController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _bllReceipe = new BLL_Receipe(cn);
            _bllLocation = new BLL_Location(cn);
            _bllProduct = new BLL_Product(cn);
            _appmanager = new AppManager(cn);
            _bllconfiguration = new BLL_Configuration(cn);
            _blllocation = new BLL_Location(cn);
            ReceipeViewModelSelection = new Receipe();
        }

        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "RecipeCreatee")]
        public ActionResult CreateReceipe()
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            ViewBag.productdata = _bllProduct.GetFinishGoods(companyid).ToList();
            ViewBag.rowmaterials = _bllProduct.GetRowMaterialsForRecipe(companyid);
            return View(new Receipe());
        }

        [Authorize(Roles = "RecipeCreatee")]
        [HttpPost]
        public ActionResult CreateReceipe(Receipe receipe)
        {

            receipe.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());

            if (receipe.Receipes.Count != 0)
            {
                List<ReceipeViewModel> Receipes1 = new List<ReceipeViewModel>();
                foreach (var p in receipe.Receipes)
                {
                    ReceipeViewModel rc = new ReceipeViewModel();
                    rc.ReceipeId = p.ReceipeId;
                    rc.ProductId = p.ProductId;
                    rc.MaterialId = p.MaterialId;
                    rc.Quantity = p.Quantity;
                    rc.ProductCode = _bllProduct.GetProductById(p.MaterialId).ProductCode;
                    rc.ProductName = p.ProductName;
                    rc.UOM = p.UOM;
                    rc.ServingUnitName = p.ServingUnitName;
                    rc.ServingUnitCP = p.ServingUnitCP;
                    rc.ServingUnitSP = p.ServingUnitSP;
                    rc.CostPrice = p.CostPrice;
                    rc.SellingPrice = p.SellingPrice;
                    rc.UnitConvertion = p.UnitConvertion;
                    rc.IsActive = true;
                    Receipes1.Add(rc);
                }
                receipe.Receipes = Receipes1;
            }
            if (receipe.ProductId == 0 || receipe.ProductServingUnitId == 0 || receipe.ProductQty == 0 || receipe.TotSellingPrice == 0)
            {
                if (receipe.ProductId == 0)
                {
                    ModelState.AddModelError("ProductId", "Please select a Product !");
                }
                else if (receipe.ProductServingUnitId == 0)
                {
                    ModelState.AddModelError("ProductServingUnitId", "Please select a Product Serving Unit !");
                }
                else if (receipe.ProductQty == 0)
                {
                    ModelState.AddModelError("ProductQty", "Please select a Product Quntity !");
                }
                else if (receipe.TotSellingPrice == 0)
                {
                    ModelState.AddModelError("TotSellingPrice", "Please Enter Selling Price !");
                }
                @ViewBag.Message = "4";
                ViewBag.productdata = _bllProduct.GetFinishGoods(receipe.CompanyID).ToList();
                ViewBag.rowmaterials = _bllProduct.GetRowMaterialsForRecipe(receipe.CompanyID);
                @ViewBag.ProductId = receipe.ProductId;
                @ViewBag.ProductServingUnitId = receipe.ProductServingUnitId;
                @ViewBag.ProductQty = receipe.ProductQty;
                return View(receipe);
            }
            //if (_bllReceipe.CheckReceipesExists(receipe.ProductId, receipe.ProductServingUnitId,receipe.ProductQty))
            //{
            //    ViewBag.Message = "3";
            //    ViewBag.productdata = _bllProduct.GetFinishGoods(receipe.CompanyID).ToList();
            //    ViewBag.rowmaterials = _bllProduct.GetRowMaterialsForRecipe(receipe.CompanyID);
            //    @ViewBag.ProductId = receipe.ProductId;
            //    @ViewBag.ProductServingUnitId = receipe.ProductServingUnitId;
            //    @ViewBag.ProductQty = receipe.ProductQty;
            //    return View(receipe);
            //}
            //Service.ReceipeService ser = new Service.ReceipeService();
            if (receipe.ApplyForAllLocation == false && receipe.LocationId == 0)
            {
                @ViewBag.Message = "5";
                ViewBag.productdata = _bllProduct.GetFinishGoods(receipe.CompanyID).ToList();
                ViewBag.rowmaterials = _bllProduct.GetRowMaterialsForRecipe(receipe.CompanyID);
                @ViewBag.ProductId = receipe.ProductId;
                @ViewBag.ProductServingUnitId = receipe.ProductServingUnitId;
                @ViewBag.ProductQty = receipe.ProductQty;
                return View(receipe);
            }
            if (receipe.Receipes.Count == 0)
            {
                @ViewBag.Message = "4";
                ViewBag.productdata = _bllProduct.GetFinishGoods(receipe.CompanyID).ToList();
                ViewBag.rowmaterials = _bllProduct.GetRowMaterialsForRecipe(receipe.CompanyID);
                @ViewBag.ProductId = receipe.ProductId;
                @ViewBag.ProductServingUnitId = receipe.ProductServingUnitId;
                @ViewBag.ProductQty = receipe.ProductQty;
                return View(receipe);
            }
            // receipe.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
            receipe.GroupOfCompanyID = Convert.ToInt32(Session["loggedusergorupofcompanyId"].ToString());
            receipe.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            receipe.CreatedUser = Session["loggeduser"].ToString();
            receipe.ModifiedUser = Session["loggeduser"].ToString();

            if (_bllReceipe.SaveReceipe(receipe) != 0)
            {
                @ViewBag.Message = "1";
                ViewBag.productdata = _bllProduct.GetFinishGoods(receipe.CompanyID).ToList();
                ViewBag.rowmaterials = _bllProduct.GetRowMaterialsForRecipe(receipe.CompanyID);
                // return View(new Receipe());
                receipe.TotSellingPrice = 0;

                return View(receipe);
            }
            else
            {
                @ViewBag.Message = "2";
                ViewBag.productdata = _bllProduct.GetFinishGoods(receipe.CompanyID).ToList();
                ViewBag.rowmaterials = _bllProduct.GetRowMaterialsForRecipe(receipe.CompanyID);
                @ViewBag.ProductId = receipe.ProductId;
                @ViewBag.ProductServingUnitId = receipe.ProductServingUnitId;
                @ViewBag.ProductQty = receipe.ProductQty;
                return View(receipe);
            }
        }

        [Authorize(Roles = "RecipeEdit")]
        [HttpGet]
        public ActionResult Edit(long id, long suid, decimal spqty, int locationid)
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());

            Receipe receipe = new Receipe();
            ViewBag.productdata = _bllProduct.GetFinishGoods(companyid).ToList();
            ViewBag.rowmaterials = _bllProduct.GetRowMaterialsForRecipe(companyid);
            var receipes = _bllReceipe.GetItems(id, suid,
                locationid,
                Convert.ToInt32(Session["loggedusercompanyId"].ToString()),
                Convert.ToInt32(Session["loggedusergorupofcompanyId"].ToString()),
                spqty

                );
            receipes.ToList().ForEach(r =>
            {
                var prod = new Product();
                prod = _bllProduct.GetProductById(r.MaterialId);
                r.ProductCode = prod.ProductCode;
                r.UOM = _bllProduct.GetUnitConvertionById(prod.WeightPerUnit);
                r.ProductName = prod.ProductName + " (" + r.UOM + ")" + " [" + r.SellingPrice + "]";
            });
            receipe.Receipes = receipes;
            @ViewBag.ProductId = id;
            var servingunit = _bllReceipe.GetServingUnit(suid);
            @ViewBag.ServingUnit = servingunit.ServingUnit;
            @ViewBag.ProductServingUnitId = suid;
            receipe.ProductQty = spqty;
            //receipe.TotCostPrice = servingunit.CostPrice;
            receipe.TotSellingPrice = servingunit.SellingPrice;
            receipe.LocationId = locationid;
            receipe.ServingUnitName = servingunit.ServingUnit;
            //receipe.ActualTotalCost = spqty * servingunit.CostPrice;
            decimal costprice = 0;
            for (int i = 0; i < receipes.Count; i++)
            {
                costprice += receipes[i].CostPrice;
            }
            
            receipe.ActualTotalCost = Math.Round(costprice, 2);
            receipe.TotCostPrice = Math.Round(costprice, 2) / spqty;
            return View("EditReceipe", receipe);
        }

        [Authorize(Roles = "RecipeEdit")]
        [HttpPost]
        public ActionResult EditReceipe(Receipe receipe)
        {
            //  receipe.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
            receipe.GroupOfCompanyID = Convert.ToInt32(Session["loggedusergorupofcompanyId"].ToString());
            receipe.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            receipe.CreatedUser = Session["loggeduser"].ToString();
            receipe.ModifiedUser = Session["loggeduser"].ToString();
            if (receipe.Receipes.Count == 0)
            {
                @ViewBag.Message = "4";
                ViewBag.productdata = _bllProduct.GetFinishGoods(receipe.CompanyID).ToList();
                ViewBag.rowmaterials = _bllProduct.GetRowMaterialsForRecipe(receipe.CompanyID);
                @ViewBag.ProductId = receipe.ProductId;
                @ViewBag.ProductServingUnitId = receipe.ProductServingUnitId;
                @ViewBag.ProductQty = receipe.ProductQty;

                return View(receipe);
            }

            if (_bllReceipe.SaveReceipe(receipe) != 0)
            {
                @ViewBag.Message = "1";
                return View(new Receipe());
            }
            else
            {
                ViewBag.productdata = _bllProduct.GetFinishGoods(receipe.CompanyID).ToList();
                ViewBag.rowmaterials = _bllProduct.GetRowMaterialsForRecipe(receipe.CompanyID);
                @ViewBag.ProductId = receipe.ProductId;
                @ViewBag.ProductServingUnitId = receipe.ProductServingUnitId;
                @ViewBag.ProductQty = receipe.ProductQty;
                @ViewBag.Message = "2";
                return View(receipe);
            }
        }

        [Authorize(Roles = "RecipeView")]
        public ActionResult ViewReceipes()
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
            // row materialdropdown
            List<SelectListItem> RowMaterials = new List<SelectListItem>();
            SelectListItem defaultmaterial = new SelectListItem();
            defaultmaterial.Text = "-- Select --";
            defaultmaterial.Value = "0";
            RowMaterials.Add(defaultmaterial);

            foreach (var prd in _bllProduct.GetRowMaterialsForRecipe(companyid))
            {
                SelectListItem dbprd = new SelectListItem();
                dbprd.Text = (prd.ProductCode + "-" + prd.ProductName + " (" + prd.UOM + ")" + " [" + prd.PackSize + "]");
                dbprd.Value = prd.ProductId.ToString();
                RowMaterials.Add(dbprd);
            }
            Session["RMs"] = RowMaterials;

            // Finishgoods dropdown
            List<SelectListItem> FinishGoods = new List<SelectListItem>();
            SelectListItem defaultfinishgood = new SelectListItem();
            defaultfinishgood.Text = "-- Select --";
            defaultfinishgood.Value = "0";
            FinishGoods.Add(defaultfinishgood);

            foreach (var prd in _bllProduct.GetFinishGoods(companyid))
            {
                SelectListItem dbprd = new SelectListItem();
                dbprd.Text = (prd.ProductCode + "-" + prd.ProductName);
                dbprd.Value = prd.ProductId.ToString();
                FinishGoods.Add(dbprd);
            }
            Session["FGs"] = FinishGoods;

            var receipes = _bllReceipe.GetReceipes(companyid).ToList();
            return View(receipes);
        }

        [Authorize(Roles = "RecipeView")]
        public void HMSRecipes()
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
            // row materialdropdown
            List<SelectListItem> RowMaterials = new List<SelectListItem>();
            SelectListItem defaultmaterial = new SelectListItem();
            defaultmaterial.Text = "-- Select --";
            defaultmaterial.Value = "0";
            RowMaterials.Add(defaultmaterial);

            foreach (var prd in _bllProduct.GetRowMaterialsForRecipe(companyid))
            {
                SelectListItem dbprd = new SelectListItem();
                dbprd.Text = (prd.ProductCode + "-" + prd.ProductName + " (" + prd.UOM + ")" + " [" + prd.PackSize + "]");
                dbprd.Value = prd.ProductId.ToString();
                RowMaterials.Add(dbprd);
            }
            Session["RMs"] = RowMaterials;

            // Finishgoods dropdown
            List<SelectListItem> FinishGoods = new List<SelectListItem>();
            SelectListItem defaultfinishgood = new SelectListItem();
            defaultfinishgood.Text = "-- Select --";
            defaultfinishgood.Value = "0";
            FinishGoods.Add(defaultfinishgood);

            foreach (var prd in _bllProduct.GetFinishGoods(companyid))
            {
                SelectListItem dbprd = new SelectListItem();
                dbprd.Text = (prd.ProductCode + "-" + prd.ProductName);
                dbprd.Value = prd.ProductId.ToString();
                FinishGoods.Add(dbprd);
            }
            Session["FGs"] = FinishGoods;

            var receipes = _bllReceipe.GetReceipes(companyid).ToList();

            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Recipes");
            //---------------2023/03/27 ----------Tharaka---------------
            string compName, Address1, Address2, Address3, Tele, Fax, website = "";
            compName = _blllocation.GetCompanyDetails().CompanyName;
            Address1 = _blllocation.GetCompanyDetails().Address1;
            Address2 = _blllocation.GetCompanyDetails().Address2;
            Address3 = _blllocation.GetCompanyDetails().Address3;
            Tele = _blllocation.GetCompanyDetails().Telephone;
            Fax = _blllocation.GetCompanyDetails().Fax;
            website = _blllocation.GetCompanyDetails().Website;
            #region Headings

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

            Sheet.Cells[6, 2].Value = "Receipe Report";
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
            Sheet.Cells[8, 1].Value = "Location";
            Sheet.Cells[8, 2].Value = "Product";
            Sheet.Cells[8, 3].Value = "Create Date";
            Sheet.Cells[8, 4].Value = "Serving Unit";
            Sheet.Cells[8, 5].Value = "Product Quantity";
            Sheet.Cells[8, 6].Value = "Recipe Cost Price";
            Sheet.Cells[8, 7].Value = "Recipe Selling Price";

            //Sheet.Cells["A1"].Value = "Location";
            //Sheet.Cells["B1"].Value = "Product";
            //Sheet.Cells["C1"].Value = "CreateDate";
            //Sheet.Cells["D1"].Value = "ServingUnit";
            //Sheet.Cells["E1"].Value = "ProductQuantity";
            //Sheet.Cells["F1"].Value = "RecipeCostPrice";
            //Sheet.Cells["G1"].Value = "REcipeSellingPrice";

            int row = 9;
            foreach (var item in receipes)
            {
                Sheet.Cells[string.Format("A{0}", row)].Value = item.LocationName;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.ProductName;
                Sheet.Cells[string.Format("C{0}", row)].Value = item.CreatedDate.ToShortDateString();
                Sheet.Cells[string.Format("D{0}", row)].Value = item.ServingUnitName;
                Sheet.Cells[string.Format("E{0}", row)].Value = item.ProductQty;
                Sheet.Cells[string.Format("F{0}", row)].Value = item.TotCostPrice;
                Sheet.Cells[string.Format("G{0}", row)].Value = item.TotSellingPrice;

                row++;
            }
            #region Header Bold
            Color colFromHexHeading = System.Drawing.ColorTranslator.FromHtml("#919089");

            Sheet.Cells[8, 1, 8, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
            Sheet.Cells[8, 1, 8, 7].Style.Fill.BackgroundColor.SetColor(colFromHexHeading);

            var table = Sheet.Cells[8, 1, 8, 7];
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
            Response.AddHeader("content-disposition", "attachment: attachment;filename=HMSRecipes.xlsx");

            using (MemoryStream MyMemoryStream = new MemoryStream())
            {
                Ep.SaveAs(MyMemoryStream);
                MyMemoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }



        }

        public JsonResult ViewAllReceipes()
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var products = _bllProduct.GetActiveProducts(companyid).Where(p => p.IsRowMaterial == false).ToList();

            return Json(JsonConvert.SerializeObject(products, Formatting.None, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Reports")]
        [HttpPost]
        public ActionResult RPTReceipe(Receipe vvm)
        {
            TempData["SelectionCriteria"] = vvm;//rerquire values for excel file generation


            if (_bllconfiguration.GetConfiguration("UReports", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn)
            {

                if (!_appmanager.CheckReportPermissions(999, Session["loggeduserempcode"].ToString(), "RPTReceipe"))
                {
                    @ViewBag.Permissions = "No user permissions to View Recipe Report";
                    return View("~/Views/Account/AccessDenied.cshtml");
                }

            }

            Receipe receipe = new Receipe();
            if (vvm.LocationId == 0 & vvm.ProductId == 0)
            {
                return View("~/Views/Reports/Stock/RPTReceipe.cshtml", receipe);
            }

            receipe.ReceipeReport = _bllReceipe.GetReceipeReport(vvm.LocationId, vvm.ProductId);
            receipe.ServingUnits = _bllReceipe.GetServingUnitByPrductId(vvm.ProductId);

            var prd = _bllProduct.GetProductById(vvm.ProductId);
            if (prd != null)
            {
                receipe.ProductName = prd.ProductName;
                receipe.ProductCode = prd.ProductCode;
            }

            //List<Models.ViewModels.Reports.ProductStockViewModel> stockmodel = new List<Models.ViewModels.Reports.ProductStockViewModel>();
            foreach (var s in receipe.ReceipeReport)
            {
                var prds = _bllProduct.GetProductById(s.MaterialId);
                //if (prd != null)
                if (prds != null)
                {
                    s.MatCode = prds.ProductCode;
                    s.MatName = prds.ProductName;
                }

            }
            ReceipeViewModelSelection = vvm;//rerquire values for excel file generation
            //if (stockmodel.Count > 0)
            //{
            //    if (vvm.LocationId == 0 && vvm.ProductId == 0) { @ViewBag.ReportSummary = "All Products at All Locations"; }
            //    if (vvm.LocationId != 0 && vvm.ProductId == 0) { @ViewBag.ReportSummary = "All Products at Location: " + stockmodel.First().Location; }
            //    if (vvm.LocationId == 0 && vvm.ProductId != 0) { @ViewBag.ReportSummary = "Product: " + stockmodel.First().ProductName + " in every location"; }
            //    if (vvm.LocationId != 0 && vvm.ProductId != 0) { @ViewBag.ReportSummary = "Product: " + stockmodel.First().ProductName + " in Location : " + stockmodel.First().Location; }
            //}
            //else
            //{
            //    @ViewBag.ReportSummary = "Product not exists in this location";
            //}
            receipe.LocationId = vvm.LocationId;
            receipe.ProductId = vvm.ProductId;
            return View("~/Views/Reports/Stock/RPTReceipe.cshtml", receipe);
        }

        [HttpGet]
        public JsonResult UpdateRecipe(int productid, decimal recipeqty, int servingunit)
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            int locationid = Convert.ToInt32(Session["loggeduserlocid"].ToString());
            var recipecount = _bllReceipe.UpdateRecipe(productid, recipeqty, servingunit, companyid, locationid);
            return new JsonResult { Data = recipecount, JsonRequestBehavior = JsonRequestBehavior.AllowGet };


        }
        [HttpGet]
        public JsonResult UpdateAllRecipes()
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            int locationid = Convert.ToInt32(Session["loggeduserlocid"].ToString());
            var recipecount = _bllReceipe.UpdateAllRecipes(companyid, locationid);
            return new JsonResult { Data = recipecount, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpGet]
        public JsonResult InactiveRecipe(int locationid, int productid, decimal recipeqty, int servingunit)
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var recipecount = _bllReceipe.ActiveInactiveRecipe(companyid, locationid, productid, recipeqty, servingunit, false);
            return new JsonResult { Data = recipecount, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpGet]
        public JsonResult ActiveRecipe(int locationid, int productid, decimal recipeqty, int servingunit)
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var recipecount = _bllReceipe.ActiveInactiveRecipe(companyid, locationid, productid, recipeqty, servingunit, true);
            return new JsonResult { Data = recipecount, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public ActionResult GetReceipeByProductId(long productid, int servingunietid)
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var recipes = _bllReceipe.GetReceipes(companyid).Where(a => a.ProductId.Equals(productid));

            List<Receipe> newsulist = new List<Receipe>();
            foreach (var s in recipes)
            {
                Receipe newsu = new Receipe();
                if (!newsulist.Select(s1 => s1.ProductId).Contains(s.ProductId) || !newsulist.Select(s1 => s1.ProductQty).Contains(s.ProductQty))
                {
                    newsu = s;
                    newsulist.Add(newsu);
                }
            }

            return Json(JsonConvert.SerializeObject(newsulist, Formatting.None, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        public ActionResult ExportToExcel(Receipe vvm)
        {

            Receipe t1 = ReceipeViewModelSelection;

            vvm = TempData["SelectionCriteria"] as Receipe;

            //var det = _bllcAlert.GetConfigDetails();
            byte[] excelFileArry = null;

            excelFileArry = GenerateExcelSheet(vvm);
            // Pass above test data and get excel file as byte array.
            try

            {
                //  byte[] document = this.StreamFile(filePath);
                Response.Clear();
                Response.AddHeader("content-disposition", "attachment;filename='" + "Receipe Report" + "'" + Convert.ToString(DateTime.Now.ToFileTimeUtc()) + ".xls");
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

        private byte[] GenerateExcelSheet(Receipe vvm)
        {
            using (ExcelPackage pck = new ExcelPackage())
            {
                //Create the worksheet
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Receipe Report");
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
                //var stock = _bllproduct.GetStockReport(vvm.LocationId, vvm.ProductId, 1, vvm.StockCodeFrom, vvm.StockCodeTO).Where(s => s.Stock != 0);
                //   DataTable dtCompanyDetails = AR.GetCompanyDetails(1);
                Receipe receipe = new Receipe();
                receipe.ReceipeReport = _bllReceipe.GetReceipeReport(vvm.LocationId, vvm.ProductId);
                receipe.ServingUnits = _bllReceipe.GetServingUnitByPrductId(vvm.ProductId);

                //get the Document heading details

                string compName, Address1, Address2, Address3, Tele, Fax, website, ReportHead = "";
                compName = _blllocation.GetCompanyDetails().CompanyName;
                Address1 = _blllocation.GetCompanyDetails().Address1;
                Address2 = _blllocation.GetCompanyDetails().Address2;
                Address3 = _blllocation.GetCompanyDetails().Address3;
                Tele = _blllocation.GetCompanyDetails().Telephone;
                Fax = _blllocation.GetCompanyDetails().Fax;
                website = _blllocation.GetCompanyDetails().Website;

                if (receipe.ReceipeReport != null)
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

                    ws.Cells[6, 2].Value = "Receipe Report";
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

                ws.Cells[12, 1].Value = "Material Code";
                ws.Cells[12, 2].Value = "Material Name";
                ws.Cells[12, 3].Value = "Quantity";


                Color colFromHexHeading = System.Drawing.ColorTranslator.FromHtml("#919089");

                ws.Cells[12, 1, 12, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[12, 1, 12, 3].Style.Fill.BackgroundColor.SetColor(colFromHexHeading);


                //var table = ws.Cells[12, 1, 12, 4];
                var table = ws.Cells[12, 1, 12, 3];
                table.Style.Border.Top.Style =
                table.Style.Border.Left.Style =
                table.Style.Border.Right.Style =
                table.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                table.Style.Font.Bold = true;
                table.Style.Font.Name = "Calibri";
                table.AutoFitColumns();


                // Set data.
                int rowIndex = 14;
                foreach (var dr in receipe.ReceipeReport) //
                {
                    var prds = _bllProduct.GetProductById(dr.MaterialId);
                    if (prds != null)
                    {
                        dr.MatCode = prds.ProductCode;
                        dr.MatName = prds.ProductName;
                    }

                    ws.Cells[rowIndex, 1].Value = dr.MatCode;
                    ws.Cells[rowIndex, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    ws.Cells[rowIndex, 2].Value = dr.MatName;
                    ws.Cells[rowIndex, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    ws.Cells[rowIndex, 3].Value = dr.Quantity;
                    ws.Cells[rowIndex, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    rowIndex++;
                    
                }
                
                Color colFromHexHeading1 = System.Drawing.ColorTranslator.FromHtml("#919089");
                //ws.Cells[12, 1, 12, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
                //ws.Cells[12, 1, 12, 4].Style.Fill.BackgroundColor.SetColor(colFromHexHeading);

                ws.Cells[rowIndex, 1, rowIndex, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[rowIndex, 1, rowIndex, 3].Style.Fill.BackgroundColor.SetColor(colFromHexHeading);
                //var table2 = ws.Cells[rowIndex, 1, rowIndex, 8];
                //table2.Style.Border.Top.Style =
                //table2.Style.Border.Left.Style =
                //table2.Style.Border.Right.Style =
                //table2.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                //table2.AutoFitColumns();

                var table1 = ws.Cells[14, 1, rowIndex, 3];
                table1.Style.Border.Top.Style =
                table1.Style.Border.Left.Style =
                table1.Style.Border.Right.Style =
                table1.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                table1.AutoFitColumns();
                table1.Style.Font.Name = "Calibri";
                return pck.GetAsByteArray();
            }

        }
    }
}