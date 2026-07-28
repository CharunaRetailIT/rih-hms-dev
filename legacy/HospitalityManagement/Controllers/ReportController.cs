
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using RIT.HMS.BLL.Common;
using RIT.HMS.BLL.Configurations;
using RIT.HMS.BLL.MasterData;
using RIT.HMS.BLL.Reports;
using RIT.HMS.Domain.Reports;
using RIT.HMS.Domain.ViewModels;
using RIT.HMS.Domain.ViewModels.Reports;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

namespace HospitalityManagement.Controllers
{
    [SessionTimeout]
    public class ReportController : Controller
    {
        
        BLL_Common _bllcommon;
        private readonly BLL_Reports _bllreports;
        private readonly BLL_Configuration _bllconfiguration;
        private readonly BLL_Product _bllproduct;
        private readonly BLL_Department _blldepartment;
        private AppManager _appmanager;
        private readonly BLL_UserMaster _bllusermaster;
        private readonly BLL_Location _blllocation;
        private readonly BLL_Receipe _bllrecipe;
        private StockBinCardViewModel StockBinCardViewModelSelection;
        private ItemUsageViewModel itemUsageViewModel;
        public ReportController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _bllcommon = new BLL_Common(cn);
            _bllreports = new BLL_Reports(cn);
            _bllconfiguration = new BLL_Configuration(cn);
            _bllproduct = new BLL_Product(cn);
            _blldepartment = new BLL_Department(cn);
            _appmanager = new AppManager(cn);
            _bllusermaster = new BLL_UserMaster(cn);
            _blllocation = new BLL_Location(cn);
            _bllrecipe = new BLL_Receipe(cn);
            StockBinCardViewModelSelection = new StockBinCardViewModel();
            itemUsageViewModel = new ItemUsageViewModel();
        }

        [HttpGet]
        [Authorize(Roles = "Reports")]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public ActionResult GetReports()
        {
            return View("~/Views/Reports/Reports.cshtml");
        }



        [HttpGet]
        public JsonResult GetRptInfoIdByRptCatId(long rptid)
        {
            var types = _bllreports.GetRptInfoIdByRptCatId(rptid).OrderBy(s => s.OrderId).ToList();
            if (_bllconfiguration.GetConfiguration("UReports", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn)
            {
                var reportpermissions = _bllusermaster.GetUserFunctionsByEmpCode(Convert.ToString(Session["loggeduserempcode"])
                                                                              ).Where(p => p.FormId == 999 && p.FunctionDescription.Contains("SSRS")).ToList();
                List<ReportInfo> ssrslist = new List<ReportInfo>();
                foreach (var r in types)
                {                   
                    if (reportpermissions.Select(s => s.FunctionName).Contains(r.ReportName))
                    {                     
                        ssrslist.Add(r);
                    }                    
                }

                return Json(JsonConvert.SerializeObject(ssrslist, Formatting.None,
               new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }),
               JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(JsonConvert.SerializeObject(types, Formatting.None,
                    new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }),
                    JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetRptCategories()
       {
            var types = _bllreports.GetRptCategories();
            return Json(JsonConvert.SerializeObject(types, Formatting.None, 
                new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), 
                JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetRecipts()
        {
            var recipts = _bllreports.GetRecipts(Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            return Json(JsonConvert.SerializeObject(recipts, Formatting.None,
                new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }),
                JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult GetUnitNumbersByLocId(int locid)
        {
            var units = _bllreports.GetUnitNumbersByLocId(locid);
            return Json(JsonConvert.SerializeObject(units, Formatting.None,
                new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }),
                JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetUnitNumbers()
        {
            var units = _bllreports.GetUnitNumbers();
            return Json(JsonConvert.SerializeObject(units, Formatting.None,
                new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }),
                JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetReciptsByLocId(int locid)
        {
            var recipts = _bllreports.GetReciptsByLocId(locid);
            return Json(JsonConvert.SerializeObject(recipts, Formatting.None,
                new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }),
                JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Authorize(Roles = "Reports")]
        public void GetReports(ReportInfo r)
        {
            var servername = _bllconfiguration.GetConfiguration("RptServer", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationDescription;
            var dbname = _bllconfiguration.GetConfiguration("RptDb", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationDescription;
            
            if (r.ReportCategoryId != 0 && r.ReportInfoId != 0)
            {
                var reportdata = _bllreports.GetReportURL(r.ReportCategoryId, r.ReportInfoId, Convert.ToInt32(Session["loggedusercompanyId"].ToString())).First();
               // var url = reportdata.ReportURL+ "&CompanyID=" + Convert.ToInt32(Session["loggedusercompanyId"].ToString()) + "&DBName=" + dbname;

                var url = reportdata.ReportURL + "&CompanyID=" + Convert.ToInt32(Session["loggedusercompanyId"].ToString()) ;
               
                
                    

               
                if (!string.IsNullOrEmpty(reportdata.ReportURL))
                    Response.Redirect(url);
            }
            else
            {
                Response.Redirect("~/Report/GetReports");
              
            }
               
        }


        [HttpGet]
        [Authorize(Roles = "Reports")]
        public ActionResult RPTSalesRegister()
        {
            if (_bllconfiguration.GetConfiguration("UReports", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn)
            {
               
                if (!_appmanager.CheckReportPermissions(999, Session["loggeduserempcode"].ToString(), "RPTSalesRegister"))
                {
                    @ViewBag.Permissions = "No user permissions to View Sales Register";
                    return View("~/Views/Account/AccessDenied.cshtml");
                }
              
            }


            @ViewBag.FromDate = DateTime.Now.ToShortDateString();
            @ViewBag.ToDate = DateTime.Now.ToShortDateString();
            SalesRegisterView salesRegisterView = new SalesRegisterView();
            salesRegisterView.FromDate = System.DateTime.Now;
            salesRegisterView.ToDate = System.DateTime.Now;
            salesRegisterView.ToTime = System.DateTime.Now;

            return View("~/Views/Reports/SalesRegister/RPTSalesRegister.cshtml", salesRegisterView);
        }
        [HttpGet]
        [Authorize(Roles = "Reports")]
        public ActionResult RPTBudgetVsSales()
        {
            if (_bllconfiguration.GetConfiguration("UReports", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn)
            {

                if (!_appmanager.CheckReportPermissions(999, Session["loggeduserempcode"].ToString(), "RPTBudgetVsSales"))
                {
                    @ViewBag.Permissions = "No user permissions to View Sales Register";
                    return View("~/Views/Account/AccessDenied.cshtml");
                }

            }


            @ViewBag.FromDate = DateTime.Now.ToShortDateString();
            @ViewBag.ToDate = DateTime.Now.ToShortDateString();
            SalesRegisterView salesRegisterView = new SalesRegisterView();
            salesRegisterView.FromDate = System.DateTime.Now;
            salesRegisterView.ToDate = System.DateTime.Now;
            salesRegisterView.ToTime = System.DateTime.Now;

            return View("~/Views/Reports/SalesRegister/RPTBudgetVsSales.cshtml", salesRegisterView);
        }


        [HttpGet]
        [Authorize(Roles = "Reports")]
        public ActionResult RPTJournal()
        {

            if (_bllconfiguration.GetConfiguration("UReports", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn)
            {

                if (!_appmanager.CheckReportPermissions(999, Session["loggeduserempcode"].ToString(), "RPTJournal"))
                {
                    @ViewBag.Permissions = "No user permissions to View Sales Journal";
                    return View("~/Views/Account/AccessDenied.cshtml");
                }

            }
            @ViewBag.FromDate = DateTime.Now.ToShortDateString();
            @ViewBag.ToDate = DateTime.Now.ToShortDateString();
            JournalViewModel journalviewmodel = new JournalViewModel();
            journalviewmodel.Date = DateTime.Now;
            journalviewmodel.DateFrom = DateTime.Now;
            journalviewmodel.DateTo = DateTime.Now;

            return View("~/Views/Reports/Journal/Journal.cshtml", journalviewmodel);
        }

        [HttpPost]
        [Authorize(Roles = "Reports")]
        public ActionResult RPTRecipts(JournalViewModel journalviewmodel)
        {
            var recipts = _bllreports.GetReciptsByLocIdUnitNo(journalviewmodel.LocationId, 
                                                              journalviewmodel.UnitNo,
                                                              journalviewmodel.DateFrom,
                                                              journalviewmodel.DateTo);

            recipts.Date = journalviewmodel.Date;
            recipts.DateFrom = journalviewmodel.DateFrom;
            recipts.DateTo = journalviewmodel.DateTo;
            recipts.LocationId = journalviewmodel.LocationId;
            recipts.UnitNo = journalviewmodel.UnitNo;

            return View("~/Views/Reports/Journal/Journal.cshtml", recipts);
        }


        [HttpGet]
        [Authorize(Roles = "Reports")]
        public ActionResult Print(int id, string reciptNo, string date, string unitNo, string zno,int LocationId)
        {

            var r = _bllreports.GetJournal(id, reciptNo, date, unitNo, zno, Convert.ToInt32(Session["loggedusercompanyId"].ToString()), LocationId);

            return PartialView("~/Views/Reports/Journal/Recipt.cshtml", r);
        }


        [HttpPost]
        [Authorize(Roles = "Reports")]
        public ActionResult RPTJournal(JournalViewModel journalviewmodel)
        {
            if (_bllconfiguration.GetConfiguration("UReports", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn)
            {

                if (!_appmanager.CheckReportPermissions(999, Session["loggeduserempcode"].ToString(), "RPTJournal"))
                {
                    @ViewBag.Permissions = "No user permissions to View Sales Journal";
                    return View("~/Views/Account/AccessDenied.cshtml");
                }

            }


            if (ModelState.IsValid == false)
            {
                return View("~/Views/Reports/Journal/Journal.cshtml", journalviewmodel);
            }
            if (string.IsNullOrEmpty(journalviewmodel.ReciptNo))
            {
                ModelState.AddModelError("ReciptNo", "Select A Recipt");
                return View("~/Views/Reports/Journal/Journal.cshtml", journalviewmodel);
            }


            //  var r = _bllreports.GetJournal(1, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));

            var r = _bllreports.GetJournal(0, journalviewmodel.ReciptNo, journalviewmodel.Date.ToShortDateString(), journalviewmodel.UnitNo, journalviewmodel.Zno.ToString(), Convert.ToInt32(Session["loggedusercompanyId"].ToString()), journalviewmodel.LocationId);

            return View("~/Views/Reports/Journal/Journal.cshtml", r);
        }
        [HttpPost]
        [Authorize(Roles = "Reports")]
        public ActionResult RPTSalesRegister(SalesRegisterView vvm)
        {
            if (_bllconfiguration.GetConfiguration("UReports", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn)
            {

                if (!_appmanager.CheckReportPermissions(999, Session["loggeduserempcode"].ToString(), "RPTSalesRegister"))
                {
                    @ViewBag.Permissions = "No user permissions to View Sales Register";
                    return View("~/Views/Account/AccessDenied.cshtml");
                }

            }

            if (vvm.IsAsAtDate == true)
            {
                var sales = _bllreports.GetSalesRegistryReport("sales", vvm.FromDate, vvm.ToDate, vvm.IsAsAtDate, 
                                                                vvm.FromTime, vvm.ToTime, vvm.LocationIDF, vvm.LocationIDT, 
                                                                vvm.DepartmentId, vvm.CategoryId, vvm.SubCategoryId, 
                                                                vvm.CustomerIDS, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));

                List<SalesRegisterViewModel> salesmodel = new List<SalesRegisterViewModel>();
                vvm.salesmodelList = sales;
                @ViewBag.FromDate = vvm.FromDate;
                @ViewBag.ToDate = vvm.ToDate;
                @ViewBag.IsAsAtDate = vvm.IsAsAtDate;
                @ViewBag.FromTime = vvm.FromTime;
                @ViewBag.ToTime = vvm.ToTime;
                @ViewBag.LocationIDF = vvm.LocationIDF;
                @ViewBag.LocationIDT = vvm.LocationIDT;
                @ViewBag.DepartmentId = vvm.DepartmentId;
                @ViewBag.CategoryId = vvm.CategoryId;
                @ViewBag.SubCategoryId = vvm.SubCategoryId;
                @ViewBag.CustomerIDS = vvm.CustomerIDS;
            }
            else
            {
                var sales = _bllreports.GetSalesRegistryReport("sales", vvm.FromDate, vvm.ToDate, vvm.IsAsAtDate, 
                                                                vvm.FromTime, vvm.ToTime, vvm.LocationIDF, 
                                                                vvm.LocationIDT, vvm.DepartmentId, 
                                                                vvm.CategoryId, vvm.SubCategoryId, vvm.CustomerIDS, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));

                List<SalesRegisterViewModel> salesmodel = new List<SalesRegisterViewModel>();
                vvm.salesmodelList = sales;
                @ViewBag.FromDate = vvm.FromDate;
                @ViewBag.ToDate = vvm.ToDate;
                @ViewBag.IsAsAtDate = vvm.IsAsAtDate;
                @ViewBag.FromTime = vvm.FromTime;
                @ViewBag.ToTime = vvm.ToTime;
                @ViewBag.LocationIDF = vvm.LocationIDF;
                @ViewBag.LocationIDT = vvm.LocationIDT;
                @ViewBag.DepartmentId = vvm.DepartmentId;
                @ViewBag.CategoryId = vvm.CategoryId;
                @ViewBag.SubCategoryId = vvm.SubCategoryId;
                @ViewBag.CustomerIDS = vvm.CustomerIDS;
            }
            return View("~/Views/Reports/SalesRegister/RPTSalesRegister.cshtml", vvm);
        }

        [HttpGet]
        [Authorize(Roles = "Reports")]
        public ActionResult RPTDailySales()
        {

            if (_bllconfiguration.GetConfiguration("UReports", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn)
            {

                if (!_appmanager.CheckReportPermissions(999, Session["loggeduserempcode"].ToString(), "RPTDailySales"))
                {
                    @ViewBag.Permissions = "No user permissions to View Daily Sales";
                    return View("~/Views/Account/AccessDenied.cshtml");
                }

            }

            DailySalesViewMdel dailySalesViewModel = new DailySalesViewMdel();
            @ViewBag.Date = DateTime.Now.ToShortDateString();
            dailySalesViewModel.Date = System.DateTime.Now;   
            return View("~/Views/Reports/SalesRegister/RPTDailySales.cshtml", dailySalesViewModel);
        }

        [HttpGet]
        [Authorize(Roles = "Reports")]
        public ActionResult RPTGivenDateSales()
        {

            if (_bllconfiguration.GetConfiguration("UReports", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn)
            {

                if (!_appmanager.CheckReportPermissions(999, Session["loggeduserempcode"].ToString(), "RPTDailySales"))
                {
                    @ViewBag.Permissions = "No user permissions to View Daily Sales";
                    return View("~/Views/Account/AccessDenied.cshtml");
                }

            }

            DailySalesViewMdel dailySalesViewModel = new DailySalesViewMdel();
            @ViewBag.Date = DateTime.Now.ToShortDateString();
            dailySalesViewModel.Date = DateTime.Now;
            dailySalesViewModel.DateTo = DateTime.Now;
            return View("~/Views/Reports/SalesRegister/RPT_GivenDateSalses.cshtml", dailySalesViewModel);
        }


        [HttpPost]
        [Authorize(Roles = "Reports")]
        public ActionResult RPTGivenDateSales(DailySalesViewMdel salesdataviewmodel)
        {
            if (_bllconfiguration.GetConfiguration("UReports", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn)
            {

                if (!_appmanager.CheckReportPermissions(999, Session["loggeduserempcode"].ToString(), "RPTDailySales"))
                {
                    @ViewBag.Permissions = "No user permissions to View Daily Sales";
                    return View("~/Views/Account/AccessDenied.cshtml");
                }

            }

            DailySalesViewMdel dailySalesViewModel = new DailySalesViewMdel();
            @ViewBag.Date = DateTime.Now.ToShortDateString();
            dailySalesViewModel.Date = System.DateTime.Now;
            DailySalesViewMdel dailysales = new DailySalesViewMdel();
            dailysales.Date = DateTime.Now;
            @ViewBag.LocationId = salesdataviewmodel.Locations;
            dailysales.SalesDataList = _bllreports.GetGivenSalesReport(salesdataviewmodel);
            dailysales.Date = salesdataviewmodel.Date;
            dailysales.DateTo = salesdataviewmodel.DateTo;
            dailysales.Locations = salesdataviewmodel.Locations;

            string locations = string.Empty;

            ViewBag.locations = "[";
            if (salesdataviewmodel.Locations != null)
            {


                foreach (var i in salesdataviewmodel.Locations)
                {
                    ViewBag.locations += i.ToString() + ",";
                }
            }
            else
            {
                locations = "0";
            }

            ViewBag.locations += "]";

            return View("~/Views/Reports/SalesRegister/RPT_GivenDateSalses.cshtml", dailysales);
        }

        [HttpPost]
        [Authorize(Roles = "Reports")]
        public ActionResult RPTDailySales(DailySalesViewMdel salesdataviewmodel)
        {
            if (_bllconfiguration.GetConfiguration("UReports", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn)
            {

                if (!_appmanager.CheckReportPermissions(999, Session["loggeduserempcode"].ToString(), "RPTDailySales"))
                {
                    @ViewBag.Permissions = "No user permissions to View Daily Sales";
                    return View("~/Views/Account/AccessDenied.cshtml");
                }

            }

            DailySalesViewMdel dailySalesViewModel = new DailySalesViewMdel();       
            @ViewBag.Date = DateTime.Now.ToShortDateString();
            dailySalesViewModel.Date = System.DateTime.Now;
            DailySalesViewMdel dailysales = new DailySalesViewMdel();
            dailysales.Date = salesdataviewmodel.Date;
            @ViewBag.LocationId = salesdataviewmodel.LocationId;
            dailysales.SalesDataList = _bllreports.GetDailySalesReport(salesdataviewmodel.Date, salesdataviewmodel.LocationId);
            return View("~/Views/Reports/SalesRegister/RPTDailySales.cshtml", dailysales);
        }


        [HttpGet]
        [Authorize(Roles = "Reports")]
        public ActionResult FoodCosting()
        {
            if (_bllconfiguration.GetConfiguration("UReports", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn)
            {

                if (!_appmanager.CheckReportPermissions(999, Session["loggeduserempcode"].ToString(), "RPTFoodCosting"))
                {
                    @ViewBag.Permissions = "No user permissions to View Foor Cost Report";
                    return View("~/Views/Account/AccessDenied.cshtml");
                }

            }

            FoodCostingViewModel vmfoodcosting = new FoodCostingViewModel();
            vmfoodcosting.DateFrom = DateTime.Now;
            vmfoodcosting.DateTo = DateTime.Now;
            return View("~/Views/Reports/Costing/FoodCosting.cshtml", vmfoodcosting);
        }

        [HttpPost]
        [Authorize(Roles = "Reports")]
        public ActionResult FoodCosting(FoodCostingViewModel vmfoodcosting)
        {

            if (_bllconfiguration.GetConfiguration("UReports", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn)
            {

                if (!_appmanager.CheckReportPermissions(999, Session["loggeduserempcode"].ToString(), "RPTFoodCosting"))
                {
                    @ViewBag.Permissions = "No user permissions to View Foor Cost Report";
                    return View("~/Views/Account/AccessDenied.cshtml");
                }

            }

            DashboardViewModel.DashboardParms parms = new DashboardViewModel.DashboardParms();
            parms.FromDate = vmfoodcosting.DateFrom;
            parms.ToDate = vmfoodcosting.DateTo;
            parms.CompanyId = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            parms.LocationId =Convert.ToInt32(vmfoodcosting.LocationId);
            parms.DepartmentId = Convert.ToInt32(vmfoodcosting.DepartmentId);
            vmfoodcosting.FoodCostDetails = _bllreports.GetFoodCostingReport(parms);
            return View("~/Views/Reports/Costing/FoodCosting.cshtml",vmfoodcosting);
        }

        [HttpGet]
        [Authorize(Roles = "Reports")]
        public ActionResult ItemConsumption()
        {
            if (_bllconfiguration.GetConfiguration("UReports", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn)
            {

                if (!_appmanager.CheckReportPermissions(999, Session["loggeduserempcode"].ToString(), "RPTItemConsumption"))
                {
                    @ViewBag.Permissions = "No user permissions to View Item Consumption Report";
                    return View("~/Views/Account/AccessDenied.cshtml");
                }

            }

            FoodCostingViewModel vmfoodcosting = new FoodCostingViewModel();
            vmfoodcosting.DateFrom = DateTime.Now;
            vmfoodcosting.DateTo = DateTime.Now;
            return View("~/Views/Reports/Consumption/ItemConsumption.cshtml", vmfoodcosting);
        }

        [HttpPost]
        [Authorize(Roles = "Reports")]
        public ActionResult ItemConsumption(FoodCostingViewModel vmfoodcosting)
        {

            if (_bllconfiguration.GetConfiguration("UReports", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn)
            {

                if (!_appmanager.CheckReportPermissions(999, Session["loggeduserempcode"].ToString(), "RPTItemConsumption"))
                {
                    @ViewBag.Permissions = "No user permissions to View Item Consumption Report";
                    return View("~/Views/Account/AccessDenied.cshtml");
                }

            }
            DashboardViewModel.DashboardParms parms = new DashboardViewModel.DashboardParms();
            parms.FromDate = vmfoodcosting.DateFrom;
            parms.ToDate = vmfoodcosting.DateTo;
            parms.CompanyId = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            parms.LocationId = Convert.ToInt32(vmfoodcosting.LocationId);
            parms.DepartmentId = Convert.ToInt32(vmfoodcosting.DepartmentId);
            vmfoodcosting.FoodCostDetails = _bllreports.GetConsumptionReport(parms);
            return View("~/Views/Reports/Consumption/ItemConsumption.cshtml", vmfoodcosting);
        }

        [HttpGet]
        [Authorize(Roles = "Reports")]
        public ActionResult StockBinCard()
        {

            if (_bllconfiguration.GetConfiguration("UReports", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn)
            {

                if (!_appmanager.CheckReportPermissions(999, Session["loggeduserempcode"].ToString(), "RPTStockBinCard"))
                {
                    @ViewBag.Permissions = "No user permissions to View Stock Bin Card";
                    return View("~/Views/Account/AccessDenied.cshtml");
                }

            }

            List<SelectListItem> FinishGoods = new List<SelectListItem>();
            SelectListItem defaultfinishgood = new SelectListItem();
            defaultfinishgood.Text = "-- All Products --";
            defaultfinishgood.Value = "0";
            FinishGoods.Add(defaultfinishgood);       
            var itemstoload = _bllproduct.GetActiveProducts(Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            foreach (var prd in itemstoload)
            {
                SelectListItem dbprd = new SelectListItem();
                dbprd.Text = (prd.ProductCode + "-" + prd.ProductName);
                dbprd.Value = prd.ProductId.ToString();
                FinishGoods.Add(dbprd);
            }         
            Session["AllItemsToPO"] = FinishGoods;

            List<SelectListItem> Departments = new List<SelectListItem>();
            SelectListItem defaultdept = new SelectListItem();
            defaultdept.Text = "-- All Departments --";
            defaultdept.Value = "0";
            Departments.Add(defaultdept);
            var depts = _blldepartment.GetActiveDepartments(Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            foreach (var dept in depts)
            {
                SelectListItem deptitem = new SelectListItem();
                deptitem.Text = (dept.DepartmentCode + "-" + dept.DepartmentName);
                deptitem.Value = dept.RstDepartmentID.ToString();
                Departments.Add(deptitem);
            }
            Session["Departments"] = Departments;

            StockBinCardViewModel stockbincard = new StockBinCardViewModel();
            stockbincard.DateFrom = DateTime.Now;
            stockbincard.DateTo = DateTime.Now;
            return View("~/Views/Reports/Stock/RPTStockBinCard.cshtml", stockbincard);
        }

        [HttpPost]
        [Authorize(Roles = "Reports")]
        public ActionResult StockBinCard(StockBinCardViewModel stockbincardvm)
        {
            TempData["SelectionCriteria"] = stockbincardvm;//rerquire values for excel file generation
            if (_bllconfiguration.GetConfiguration("UReports", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn)
            {

                if (!_appmanager.CheckReportPermissions(999, Session["loggeduserempcode"].ToString(), "RPTStockBinCard"))
                {
                    @ViewBag.Permissions = "No user permissions to View Stock Bin Card";
                    return View("~/Views/Account/AccessDenied.cshtml");
                }

            }

            StockBinCardViewModel stockbincard = new StockBinCardViewModel();
            stockbincard.DateFrom = stockbincardvm.DateFrom;
            stockbincard.DateTo = stockbincardvm.DateTo;
            stockbincard.DepartmentId = stockbincardvm.DepartmentId;
            stockbincard.LocationId = stockbincardvm.LocationId;
            stockbincard.ProductId = stockbincardvm.ProductId;
            stockbincard.Details = _bllreports.GetBinCardReport(stockbincard);
            
            StockBinCardViewModelSelection = stockbincardvm;//rerquire values for excel file generation
            return View("~/Views/Reports/Stock/RPTStockBinCard.cshtml", stockbincard);
        }



        [HttpGet]
        [Authorize(Roles = "Reports")]
        public ActionResult AsAtStockBalance()
        {

            if (_bllconfiguration.GetConfiguration("UReports", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn)
            {

                if (!_appmanager.CheckReportPermissions(999, Session["loggeduserempcode"].ToString(), "RPTAsAtStockBalance"))
                {
                    @ViewBag.Permissions = "No user permissions to View As At Stock Balance";
                    return View("~/Views/Account/AccessDenied.cshtml");
                }

            }

            List<SelectListItem> FinishGoods = new List<SelectListItem>();
            SelectListItem defaultfinishgood = new SelectListItem();
            defaultfinishgood.Text = "-- All Products --";
            defaultfinishgood.Value = "0";
            FinishGoods.Add(defaultfinishgood);
            var itemstoload = _bllproduct.GetActiveProducts(Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            foreach (var prd in itemstoload)
            {
                SelectListItem dbprd = new SelectListItem();
                dbprd.Text = (prd.ProductCode + "-" + prd.ProductName);
                dbprd.Value = prd.ProductId.ToString();
                FinishGoods.Add(dbprd);
            }
            Session["AllItemsToPO"] = FinishGoods;

            List<SelectListItem> Departments = new List<SelectListItem>();
            SelectListItem defaultdept = new SelectListItem();
            defaultdept.Text = "-- All Departments --";
            defaultdept.Value = "0";
            Departments.Add(defaultdept);
            var depts = _blldepartment.GetActiveDepartments(Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            foreach (var dept in depts)
            {
                SelectListItem deptitem = new SelectListItem();
                deptitem.Text = (dept.DepartmentCode + "-" + dept.DepartmentName);
                deptitem.Value = dept.RstDepartmentID.ToString();
                Departments.Add(deptitem);
            }
            Session["Departments"] = Departments;

            StockBinCardViewModel stockbincard = new StockBinCardViewModel();
            stockbincard.DateFrom = DateTime.Now;
            stockbincard.DateTo = DateTime.Now;
            stockbincard.WithZeroBalances = true;
            return View("~/Views/Reports/Stock/RPTAsAtStockBalance.cshtml", stockbincard);
        }
        [HttpPost]
        [Authorize(Roles = "Reports")]
        public ActionResult AsAtStockBalance(StockBinCardViewModel stockbincardvm)
        {
            TempData["SelectionCriteria"] = stockbincardvm;//rerquire values for excel file generation
            if (_bllconfiguration.GetConfiguration("UReports", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn)
            {

                if (!_appmanager.CheckReportPermissions(999, Session["loggeduserempcode"].ToString(), "RPTAsAtStockBalance"))
                {
                    @ViewBag.Permissions = "No user permissions to View As At Stock Balance";
                    return View("~/Views/Account/AccessDenied.cshtml");
                }

            }

            StockBinCardViewModel AsAtStock = new StockBinCardViewModel();

            AsAtStock.DateTo = stockbincardvm.DateTo;
            AsAtStock.DepartmentId = stockbincardvm.DepartmentId;
            AsAtStock.LocationId = stockbincardvm.LocationId;
            AsAtStock.ProductId = stockbincardvm.ProductId;
            AsAtStock.CompanyId = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            AsAtStock.WithZeroBalances = stockbincardvm.WithZeroBalances;

            AsAtStock.Details = _bllreports.GetAsAtStockBalanceReport(AsAtStock);
            AsAtStock.Unit = stockbincardvm.Unit;
            //AsAtStock.Unit = stockbincardvm.Unit;
           ////////////////////////////////////////////////////
           //Added by Aruna  for calculating the cost value
            decimal totalCost = 0;
            foreach (var s in AsAtStock.Details)
            {
                totalCost += s.CostPrice * s.Stock;
            }
            ViewBag.TotalCost = totalCost;
            StockBinCardViewModelSelection = stockbincardvm;//rerquire values for excel file generation
            ////////////////////////////////////////////////////
            return View("~/Views/Reports/Stock/RPTAsAtStockBalance.cshtml", AsAtStock);
        }





        [HttpGet]
        [Authorize(Roles = "Reports")]
        public ActionResult RPTItemUsage()
        {

            if (_bllconfiguration.GetConfiguration("UReports", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn)
            {

                if (!_appmanager.CheckReportPermissions(999, Session["loggeduserempcode"].ToString(), "RPTItemUsage"))
                {
                    @ViewBag.Permissions = "No user permissions to View Item Usage Report";
                    return View("~/Views/Account/AccessDenied.cshtml");
                }

            }
            ItemUsageViewModel vmitemusage = new ItemUsageViewModel();

            List<SelectListItem> FinishGoods = new List<SelectListItem>();
            SelectListItem defaultfinishgood = new SelectListItem();
            defaultfinishgood.Text = "-- All Products --";
            defaultfinishgood.Value = "0";
            FinishGoods.Add(defaultfinishgood);
            var itemstoload = _bllproduct.GetActiveProducts(Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            foreach (var prd in itemstoload)
            {
                SelectListItem dbprd = new SelectListItem();
                dbprd.Text = (prd.ProductCode + "-" + prd.ProductName);
                dbprd.Value = prd.ProductId.ToString();
                FinishGoods.Add(dbprd);
            }
            vmitemusage.Products = FinishGoods;



            List<SelectListItem> Kitchens = new List<SelectListItem>();
            SelectListItem defultkitchen = new SelectListItem();
            defultkitchen.Text = "-- All Kitchens --";
            defultkitchen.Value = "0";
            Kitchens.Add(defultkitchen);
            var kitchenstodisplay = _blllocation.GetActiveKitchens(Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            foreach (var k in kitchenstodisplay)
            {
                SelectListItem kit = new SelectListItem();
                kit.Text = (k.KitchenCode + "-" + k.KitchenDesc);
                kit.Value = k.KitchenCode;
                Kitchens.Add(kit);
            }
            vmitemusage.Kitchens = Kitchens;


            List<SelectListItem> Departments = new List<SelectListItem>();
            SelectListItem defaultdept = new SelectListItem();
            defaultdept.Text = "-- All Departments --";
            defaultdept.Value = "0";
            Departments.Add(defaultdept);
            var depts = _blldepartment.GetActiveDepartments(Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            foreach (var dept in depts)
            {
                SelectListItem deptitem = new SelectListItem();
                deptitem.Text = (dept.DepartmentCode + "-" + dept.DepartmentName);
                deptitem.Value = dept.RstDepartmentID.ToString();
                Departments.Add(deptitem);
            }
            vmitemusage.Departments = Departments;

            vmitemusage.CompanyId = Convert.ToInt32(Session["loggedusercompanyId"]);
            vmitemusage.LocationId = Convert.ToInt32(Session["loggeduserlocId"]);
            vmitemusage.DateFrom = DateTime.Now;
            vmitemusage.DateTo = DateTime.Now;
            return View("~/Views/Reports/Stock/RPTItemUsage.cshtml", vmitemusage);
        }

        [HttpPost]
        [Authorize(Roles = "Reports")]
        public ActionResult RPTItemUsage(ItemUsageViewModel vmitemusage)
        {
            TempData["SelectionCriteria"] = vmitemusage;//rerquire values for excel file generation
            var sss = vmitemusage;
            var usage = _bllrecipe.ItemUsage(sss);
            ItemUsageViewModel filtereddetail = new ItemUsageViewModel();

            if (vmitemusage.ProductId != 0)
            {
                filtereddetail.Details = usage.Details.Where(p => p.ProductId == vmitemusage.ProductId).ToList();
            }
            else
            {
                filtereddetail.Details = usage.Details;
            }

            if (vmitemusage.DepartmentId != 0)
            {
                filtereddetail.Details = usage.Details.Where(d => d.DepartmentId == vmitemusage.DepartmentId).ToList();
            }
            else
            {
                filtereddetail.Details = usage.Details;
            }

            vmitemusage.Details = filtereddetail.Details;

            List<SelectListItem> FinishGoods = new List<SelectListItem>();
            SelectListItem defaultfinishgood = new SelectListItem();
            defaultfinishgood.Text = "-- All Products --";
            defaultfinishgood.Value = "0";
            FinishGoods.Add(defaultfinishgood);
            var itemstoload = _bllproduct.GetActiveProducts(Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            foreach (var prd in itemstoload)
            {
                SelectListItem dbprd = new SelectListItem();
                dbprd.Text = (prd.ProductCode + "-" + prd.ProductName);
                dbprd.Value = prd.ProductId.ToString();
                FinishGoods.Add(dbprd);
            }
            vmitemusage.Products = FinishGoods;



            List<SelectListItem> Kitchens = new List<SelectListItem>();
            SelectListItem defultkitchen = new SelectListItem();
            defultkitchen.Text = "-- All Kitchens --";
            defultkitchen.Value = "0";
            Kitchens.Add(defultkitchen);
            var kitchenstodisplay = _blllocation.GetActiveKitchens(Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            foreach (var k in kitchenstodisplay)
            {
                SelectListItem kit = new SelectListItem();
                kit.Text = (k.KitchenCode + "-" + k.KitchenDesc);
                kit.Value = k.KitchenCode;
                Kitchens.Add(kit);
            }
            vmitemusage.Kitchens = Kitchens;


            List<SelectListItem> Departments = new List<SelectListItem>();
            SelectListItem defaultdept = new SelectListItem();
            defaultdept.Text = "-- All Departments --";
            defaultdept.Value = "0";
            Departments.Add(defaultdept);
            var depts = _blldepartment.GetActiveDepartments(Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            foreach (var dept in depts)
            {
                SelectListItem deptitem = new SelectListItem();
                deptitem.Text = (dept.DepartmentCode + "-" + dept.DepartmentName);
                deptitem.Value = dept.RstDepartmentID.ToString();
                Departments.Add(deptitem);
            }
            vmitemusage.Departments = Departments;
            itemUsageViewModel = vmitemusage;//rerquire values for excel file generation

            return View("~/Views/Reports/Stock/RPTItemUsage.cshtml", vmitemusage);
        }

        public ActionResult ExportToExcel(StockBinCardViewModel vvm)
        {

            StockBinCardViewModel t1 = StockBinCardViewModelSelection;

            vvm = TempData["SelectionCriteria"] as StockBinCardViewModel;

            //var det = _bllcAlert.GetConfigDetails();
            byte[] excelFileArry = null;

            excelFileArry = GenerateExcelSheet(vvm);
            // Pass above test data and get excel file as byte array.
            try

            {
                //  byte[] document = this.StreamFile(filePath);
                Response.Clear();
                Response.AddHeader("content-disposition", "attachment;filename='" + "As At Stock Balance" + "'" + Convert.ToString(DateTime.Now.ToFileTimeUtc()) + ".xls");
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
        private byte[] GenerateExcelSheet(StockBinCardViewModel vvm)
        {
            using (ExcelPackage pck = new ExcelPackage())
            {
                //Create the worksheet
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("As At Stock Balance");
                #region Logo

                string filename = Server.MapPath(@"~/bin/LOGO.jpg");
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
                StockBinCardViewModel AsAtStock = new StockBinCardViewModel();
                AsAtStock.Details = _bllreports.GetAsAtStockBalanceReport(vvm);

                //stockmodel.Add(v);

                //get the Document heading details

                string compName, Address1, Address2, Address3, Tele, Fax, website, ReportHead = "";
                compName = _blllocation.GetCompanyDetails().CompanyName;
                Address1 = _blllocation.GetCompanyDetails().Address1;
                Address2 = _blllocation.GetCompanyDetails().Address2;
                Address3 = _blllocation.GetCompanyDetails().Address3;
                Tele = _blllocation.GetCompanyDetails().Telephone;
                Fax = _blllocation.GetCompanyDetails().Fax;
                website = _blllocation.GetCompanyDetails().Website;

                if (AsAtStock.Details != null)
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
                printDetailBox.Style.Font.Size = 8;
                ws.Cells[3, 13, 5, 13].Style.Font.Bold = true;

                ws.Cells[3, 13].Value = "Date";
                ws.Cells[4, 13].Value = "Time";
                ws.Cells[5, 13].Value = "Req. By";
                

                ws.Cells[3, 14].Value = DateTime.Now.Date.ToShortDateString();

                ws.Cells[4, 14].Value = DateTime.Now.ToString("h:mm tt");

                if (Session["loggeduser"] != null)
                    ws.Cells[5, 14].Value = Session["loggeduser"].ToString();
                else
                    ws.Cells[5, 14].Value = " ";


                #endregion
                
                ws.Cells[12, 1].Value = "Location";
                ws.Cells[12, 2].Value = "Department";
                ws.Cells[12, 3].Value = "Product Code";
                ws.Cells[12, 4].Value = "Produc Name";

                ws.Cells[12, 5].Value = "Stock";
                ws.Cells[12, 6].Value = "Cost Price";
                ws.Cells[12, 7].Value = "Cost Value";
                


                Color colFromHexHeading = System.Drawing.ColorTranslator.FromHtml("#919089");

                ws.Cells[12, 1, 12, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[12, 1, 12, 7].Style.Fill.BackgroundColor.SetColor(colFromHexHeading);


                //var table = ws.Cells[12, 1, 12, 4];
                var table = ws.Cells[12, 1, 12, 7];
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
                foreach (var dr in AsAtStock.Details) //
                {
                    string locname = "";
                    //locname = _blllocation.GetLocationById(dr.LocationId).LocationName;
                    locname =dr.Location;
                    ws.Cells[rowIndex, 1].Value = locname;

                    ws.Cells[rowIndex, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    ws.Cells[rowIndex, 2].Value = dr.Department;
                    ws.Cells[rowIndex, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    ws.Cells[rowIndex, 3].Value = dr.ProductCode;
                    ws.Cells[rowIndex, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    ws.Cells[rowIndex, 4].Value = dr.ProductName;
                    ws.Cells[rowIndex, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    //   ws.Cells[rowIndex, 4].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    ws.Cells[rowIndex, 5].Value = dr.Stock;
                    ws.Cells[rowIndex, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    ws.Cells[rowIndex, 6].Value = dr.CostPrice;
                    ws.Cells[rowIndex, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    ws.Cells[rowIndex, 7].Value = (dr.Stock * dr.CostPrice);
                    ws.Cells[rowIndex, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    
                    rowIndex++;
                    TotalcostValueExcel += (dr.Stock * dr.CostPrice);// calculate average cost value added by Aruna
                }

                ws.Cells[rowIndex, 1].Value = "Total Cost value";
                ws.Cells[rowIndex, 7].Value = TotalcostValueExcel;
                Color colFromHexHeading1 = System.Drawing.ColorTranslator.FromHtml("#919089");

                ws.Cells[rowIndex, 1, rowIndex, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[rowIndex, 1, rowIndex, 7].Style.Fill.BackgroundColor.SetColor(colFromHexHeading);

                var table1 = ws.Cells[14, 1, rowIndex, 7];
                table1.Style.Border.Top.Style =
                table1.Style.Border.Left.Style =
                table1.Style.Border.Right.Style =
                table1.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                table1.AutoFitColumns();
                table1.Style.Font.Name = "Calibri";
                return pck.GetAsByteArray();
            }

        }

        //Stock bin Card
        public ActionResult ExportToExcelBinCard(StockBinCardViewModel vvm)
        {

            StockBinCardViewModel t1 = StockBinCardViewModelSelection;

            vvm = TempData["SelectionCriteria"] as StockBinCardViewModel;

            //var det = _bllcAlert.GetConfigDetails();
            byte[] excelFileArry = null;

            excelFileArry = GenerateExcelSheetBinCard(vvm);
            // Pass above test data and get excel file as byte array.
            try

            {
                //  byte[] document = this.StreamFile(filePath);
                Response.Clear();
                Response.AddHeader("content-disposition", "attachment;filename='" + "Stock BinCard" + "'" + Convert.ToString(DateTime.Now.ToFileTimeUtc()) + ".xls");
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
        private byte[] GenerateExcelSheetBinCard(StockBinCardViewModel vvm)
        {
            using (ExcelPackage pck = new ExcelPackage())
            {
                //Create the worksheet
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Stock BinCard");
                #region Logo

                string filename = Server.MapPath(@"~/bin/LOGO.jpg");
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
                //stock bin card excel 
                StockBinCardViewModel AsAtStock = new StockBinCardViewModel();
                AsAtStock.Details=_bllreports.GetBinCardReport(vvm);
                
                //get the Document heading details

                string compName, Address1, Address2, Address3, Tele, Fax, website, ReportHead = "";
                compName = _blllocation.GetCompanyDetails().CompanyName;
                Address1 = _blllocation.GetCompanyDetails().Address1;
                Address2 = _blllocation.GetCompanyDetails().Address2;
                Address3 = _blllocation.GetCompanyDetails().Address3;
                Tele = _blllocation.GetCompanyDetails().Telephone;
                Fax = _blllocation.GetCompanyDetails().Fax;
                website = _blllocation.GetCompanyDetails().Website;

                if (AsAtStock.Details != null)
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

                    ws.Cells[6, 2].Value = "Stock Bin Card Report"; 

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
                printDetailBox.Style.Font.Size = 8;
                ws.Cells[3, 13, 5, 13].Style.Font.Bold = true;

                ws.Cells[3, 13].Value = "Date";
                ws.Cells[4, 13].Value = "Time";
                ws.Cells[5, 13].Value = "Req. By";


                ws.Cells[3, 14].Value = DateTime.Now.Date.ToShortDateString();

                ws.Cells[4, 14].Value = DateTime.Now.ToString("h:mm tt");

                if (Session["loggeduser"] != null)
                    ws.Cells[5, 14].Value = Session["loggeduser"].ToString();
                else
                    ws.Cells[5, 14].Value = " ";


                #endregion

                ws.Cells[12, 1].Value = "Product Code";
                ws.Cells[12, 2].Value = "Product Name";
                ws.Cells[12, 3].Value = "Date";
                ws.Cells[12, 4].Value = "Doc No";

                ws.Cells[12, 5].Value = "To Location";
                ws.Cells[12, 6].Value = "Description";
                ws.Cells[12, 7].Value = "Received";
                ws.Cells[12, 8].Value = "Issued";
                ws.Cells[12, 9].Value = "Balance";


                Color colFromHexHeading = System.Drawing.ColorTranslator.FromHtml("#919089");

                ws.Cells[12, 1, 12, 9].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[12, 1, 12, 9].Style.Fill.BackgroundColor.SetColor(colFromHexHeading);


                //var table = ws.Cells[12, 1, 12, 4];
                var table = ws.Cells[12, 1, 12, 9];
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
                decimal stock = 0;
                decimal runningTotal = 0;
                foreach (var dr in AsAtStock.Details) //
                {
                    string locname = "";
                    //locname = _blllocation.GetLocationById(dr.LocationId).LocationName;
                    //locname = dr.Location;
                    ws.Cells[rowIndex, 1].Value = dr.ProductCode;

                    ws.Cells[rowIndex, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    ws.Cells[rowIndex, 2].Value = dr.ProductName;
                    ws.Cells[rowIndex, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    ws.Cells[rowIndex, 3].Value = dr.TransactionDate;
                    ws.Cells[rowIndex, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    ws.Cells[rowIndex, 4].Value = dr.TransactionNo;
                    ws.Cells[rowIndex, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    ws.Cells[rowIndex, 5].Value = dr.ToLocationName;
                    ws.Cells[rowIndex, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    ws.Cells[rowIndex, 6].Value = dr.TransactionType;
                    ws.Cells[rowIndex, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    if(dr.StockQty > 0)
                    {
                        stock = dr.StockQty;
                    }
                    else
                    {
                        stock = 0;
                    }

                    ws.Cells[rowIndex, 7].Value = stock;
                    ws.Cells[rowIndex, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    
                    if (dr.StockQty < 0)
                    {
                        stock = dr.StockQty;
                    }
                    else
                    {
                        stock = 0;
                    }
                    ws.Cells[rowIndex, 8].Value = stock;
                    ws.Cells[rowIndex, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    runningTotal += dr.StockQty;

                    ws.Cells[rowIndex, 9].Value = runningTotal;
                    ws.Cells[rowIndex, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    rowIndex++;
                    TotalcostValueExcel += (dr.Stock * dr.CostPrice);// calculate average cost value added by Aruna
                }

                ws.Cells[rowIndex, 1, rowIndex, 9].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[rowIndex, 1, rowIndex, 9].Style.Fill.BackgroundColor.SetColor(colFromHexHeading);

                var table1 = ws.Cells[14, 1, rowIndex, 9];
                table1.Style.Border.Top.Style =
                table1.Style.Border.Left.Style =
                table1.Style.Border.Right.Style =
                table1.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                table1.AutoFitColumns();
                table1.Style.Font.Name = "Calibri";
                return pck.GetAsByteArray();
            }

        }

        //Item Usage

        public ActionResult ExportToExcelItemUsage(ItemUsageViewModel itemusage)
        {

            ItemUsageViewModel t1 = itemUsageViewModel;

            itemusage = TempData["SelectionCriteria"] as ItemUsageViewModel;

            //var det = _bllcAlert.GetConfigDetails();
            byte[] excelFileArry = null;

            excelFileArry = GenerateExcelSheetItemUsage(itemusage);
            // Pass above test data and get excel file as byte array.
            try

            {
                //  byte[] document = this.StreamFile(filePath);
                Response.Clear();
                Response.AddHeader("content-disposition", "attachment;filename='" + "Item Usage" + "'" + Convert.ToString(DateTime.Now.ToFileTimeUtc()) + ".xls");
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
        //Item usage
        private byte[] GenerateExcelSheetItemUsage(ItemUsageViewModel itemusage)
        {
            using (ExcelPackage pck = new ExcelPackage())
            {
                //Create the worksheet
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Item Usage");
                #region Logo

                string filename = Server.MapPath(@"~/bin/LOGO.jpg");
                try
                {

                    //AddImage(ws, 0, 0, filename);
                    ws.Cells[1, 1, 6, 1].Merge = true;
                }
                catch
                {

                }

                #endregion

                ItemUsageViewModel AsAtStock = new ItemUsageViewModel();
                //AsAtStock.Details = _bllreports.GetAsAtStockBalanceReport(itemusage);
                var usage = _bllrecipe.ItemUsage(itemusage);

                #region Company Detail

                //stockmodel.Add(v);

                //get the Document heading details

                string compName, Address1, Address2, Address3, Tele, Fax, website, ReportHead = "";
                compName = _blllocation.GetCompanyDetails().CompanyName;
                Address1 = _blllocation.GetCompanyDetails().Address1;
                Address2 = _blllocation.GetCompanyDetails().Address2;
                Address3 = _blllocation.GetCompanyDetails().Address3;
                Tele = _blllocation.GetCompanyDetails().Telephone;
                Fax = _blllocation.GetCompanyDetails().Fax;
                website = _blllocation.GetCompanyDetails().Website;

                if (usage != null)
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

                    ws.Cells[6, 2].Value = "Item Usage";
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
                printDetailBox.Style.Font.Size = 8;
                ws.Cells[3, 13, 5, 13].Style.Font.Bold = true;

                ws.Cells[3, 13].Value = "Date";
                ws.Cells[4, 13].Value = "Time";
                ws.Cells[5, 13].Value = "Req. By";


                ws.Cells[3, 14].Value = DateTime.Now.Date.ToShortDateString();

                ws.Cells[4, 14].Value = DateTime.Now.ToString("h:mm tt");

                if (Session["loggeduser"] != null)
                    ws.Cells[5, 14].Value = Session["loggeduser"].ToString();
                else
                    ws.Cells[5, 14].Value = " ";


                #endregion

                ws.Cells[12, 1].Value = "Recipe";
                ws.Cells[12, 2].Value = "Serving Unit";
                ws.Cells[12, 3].Value = "Recipe Qty";
                ws.Cells[12, 4].Value = "Material";
                ws.Cells[12, 5].Value = "UOM";
                ws.Cells[12, 6].Value = "Material Qty";
                //ws.Cells[12, 7].Value = "Cost Value";
                Color colFromHexHeading = System.Drawing.ColorTranslator.FromHtml("#919089");

                ws.Cells[12, 1, 12, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[12, 1, 12, 6].Style.Fill.BackgroundColor.SetColor(colFromHexHeading);

                var table = ws.Cells[12, 1, 12, 6];
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
                foreach (var dr in usage.Details) //
                {
                    //string locname = "";
                    //locname = _blllocation.GetLocationById(dr.LocationId).LocationName;
                    //locname = dr.ProductCode;
                    ws.Cells[rowIndex, 1].Value = dr.ProductCode; ;

                    ws.Cells[rowIndex, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    ws.Cells[rowIndex, 2].Value = dr.ServingUnit;
                    ws.Cells[rowIndex, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    ws.Cells[rowIndex, 3].Value = dr.ProductQty.ToString("n2");
                    ws.Cells[rowIndex, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    ws.Cells[rowIndex, 4].Value = dr.ItemName;
                    ws.Cells[rowIndex, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    //   ws.Cells[rowIndex, 4].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    ws.Cells[rowIndex, 5].Value = dr.SubUnit;
                    ws.Cells[rowIndex, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    ws.Cells[rowIndex, 6].Value = (dr.ProductQty*dr.ItemQty).ToString("n2");
                    ws.Cells[rowIndex, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    rowIndex++;
                    
                }

                //ws.Cells[rowIndex, 1].Value = "Total Cost value";
                //ws.Cells[rowIndex, 7].Value = TotalcostValueExcel;
                //Color colFromHexHeading1 = System.Drawing.ColorTranslator.FromHtml("#919089");

                ws.Cells[rowIndex, 1, rowIndex, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[rowIndex, 1, rowIndex, 6].Style.Fill.BackgroundColor.SetColor(colFromHexHeading);

                var table1 = ws.Cells[14, 1, rowIndex, 6];
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