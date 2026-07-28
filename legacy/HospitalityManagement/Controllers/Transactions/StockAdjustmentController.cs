
using RIT.HMS.BLL.Common;
using RIT.HMS.BLL.TransactionData;
using RIT.HMS.Domain.Transactions;
using RIT.HMS.Domain.ViewModels;
using RIT.HMS.Domain.ViewModels.Reports;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RIT.HMS.BLL.MasterData;
using RIT.HMS.BLL.Configurations;
using System.Drawing;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Threading;

namespace HospitalityManagement.Controllers.Transactions
{
    [SessionTimeout]
    public class StockAdjustmentController : Controller
    {
        private readonly BLL_StockAdjustment _bllstockAdjustment;
        private readonly BLL_Product _bllproduct;
        private readonly BLL_Common _bllcommon;
        private readonly BLL_MonthEnd _bllmonthend;
        private readonly BLL_Company _bllcompany;
        private readonly BLL_Location _blllocation;
        private readonly BLL_Configuration _bllconfiguration;
        private StockAdjustmentReportViewModel StockAdjustmentReportViewModelSelection;

        private AppManager _appmanager;
        public StockAdjustmentController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _bllstockAdjustment = new BLL_StockAdjustment(cn);
            _bllproduct = new BLL_Product(cn);
            _bllcommon = new BLL_Common(cn);
            _bllmonthend = new BLL_MonthEnd(cn);
            _bllcompany = new BLL_Company(cn);
            _blllocation = new BLL_Location(cn);
            _bllconfiguration = new BLL_Configuration(cn);
            _appmanager = new AppManager(cn);
            StockAdjustmentReportViewModelSelection = new StockAdjustmentReportViewModel();
        }
        public ActionResult Index()
        {
            var header = new StockAdjustmentHeader();
            Session["POWizadUI"] = _bllconfiguration.GetConfiguration("UIWZ", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            int docid = _bllcommon.GetDcumentId("StockAdjustments", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            var docnum = _bllcommon.GetDocumentTemporyNo("StockAdjustments", Convert.ToInt32(Session["loggeduserlocId"]), "1", docid, true, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            header.DocumentNo = docnum;
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            ViewBag.BDSA = _bllconfiguration.GetConfiguration("BDSA", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
          //  ViewBag.productdata = _bllproduct.GetActiveProducts(companyid);
            // Finishgoods dropdown
            List<SelectListItem> FinishGoods = new List<SelectListItem>();
            SelectListItem defaultfinishgood = new SelectListItem();
            defaultfinishgood.Text = "-- Select --";
            defaultfinishgood.Value = "0";
            FinishGoods.Add(defaultfinishgood);

            foreach (var prd in _bllproduct.GetActiveProducts(companyid))
            {
                SelectListItem dbprd = new SelectListItem();
                dbprd.Text = (prd.ProductCode + "-" + prd.ProductName);
                dbprd.Value = prd.ProductId.ToString();
                FinishGoods.Add(dbprd);
            }
            Session["Products"] = FinishGoods;



            return View("~/Views/Transactions/StockAdjustment/StockAdjustment.cshtml", header);
        }

     //   [Authorize(Roles = "SAView")]
        [HttpGet]
        public ActionResult Print(int id, int printstatus)
        {
            StockAdjustmentHeader stkheader = new StockAdjustmentHeader();
            stkheader = _bllstockAdjustment.GetHeaderById(id);
            stkheader.StockAdjDetail = _bllstockAdjustment.GetDetailByHeaderId(stkheader.StockAdjustmentHeaderId);
            stkheader.Company = _bllcompany.GetCompanyById(stkheader.CompanyID).CompanyName;
            stkheader.Location = _blllocation.GetLocationById(stkheader.StockLocationId).LocationName;
            stkheader.SysCompany = _bllcompany.GetCompanyById(stkheader.CompanyID);
            //poheader.Supplier = _bllsupplier.GetSupplierById(poheader.SupplierId);
            stkheader.PrintStatus = printstatus;
            //stkheader.DocState = _blldocStatus.GetDocStatusById(poheader.DocumentStatus).Description;
            //    var paymantterm = _bllpaymentterm.GetPaymentTermById(poheader.PaymentTermId);
            //  if (paymantterm != null)
            //       poheader.PaymentTerm = paymantterm.PaymentTermName;
            //  else poheader.PaymentTerm = "N/A";
            //  var polocation = _blllocation.GetLocationById(poheader.POLocationId);
            //
            //poheader.POLocationAddress = polocation.Address1 + "," + polocation.Address2 + "," + polocation.Address3 + ".";
            stkheader.StockAdjDetail.ToList().ForEach(d =>
            {
                var prd = _bllproduct.GetProductById(d.ProductId);
                d.ProductName = prd.ProductName;
                d.ProductCode = prd.ProductCode;

            });

            return PartialView("~/Views/Receipts/StockADJView.cshtml", stkheader);
        }

       // [Authorize(Roles = "SACreatee")]
        [HttpPost]
        public ActionResult SubmitAdjustment(StockAdjustmentHeader stockheader)
        {

            // check permissions
            if (!_appmanager.SetPermissions(8, Session["loggeduserempcode"].ToString(), "SACreatee"))
            {
                @ViewBag.Permissions = "No user permissions to Create Stock Adjustments";
                return View("~/Views/Account/AccessDenied.cshtml");
            }
            //end check permissions

            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            //ViewBag.productdata = _bllproduct.GetActiveProducts(companyid);
            if (stockheader.StockLocationId == 0)
            {
                ModelState.AddModelError("StockLocationId", "Please select the location !");
                return View("~/Views/Transactions/StockAdjustment/StockAdjustment.cshtml", stockheader);
            }
            if (stockheader.StockAdjDetail.Count == 0)
            {
                ViewBag.StockLocationId = stockheader.StockLocationId;
                stockheader.Remark = stockheader.Remark;
                ModelState.AddModelError("StockAdjDetail", "Please enter stock adjustment details !");
                return View("~/Views/Transactions/StockAdjustment/StockAdjustment.cshtml", stockheader);
            }


            // check Month End Stataus
            bool EnableMonthEndProcess = _bllconfiguration.GetConfiguration("EnableMonthEndProcess", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;

            if (EnableMonthEndProcess)
            {

                //int year = poheader.PODate.Year;
                int monthendstatus = _bllmonthend.CheckMonthEnd(Convert.ToInt32(stockheader.StockLocationId), stockheader.CreatedDate.Year, stockheader.CreatedDate.Month);
                if (monthendstatus != 1)
                {
                    ModelState.Clear();

                    ViewBag.StockLocationId = stockheader.StockLocationId;
                    stockheader.Remark = stockheader.Remark;
                    ModelState.AddModelError("StockAdjDetail", "Please enter stock adjustment details !");
                    ViewBag.Message = "4";
                    return View("~/Views/Transactions/StockAdjustment/StockAdjustment.cshtml", stockheader);

                }
            }



            int docid = _bllcommon.GetDcumentId("StockAdjustments", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            //var docnum = _bllcommon.GetDocumentNumber("StockAdjustments",
            //                                            Convert.ToInt32(Session["loggeduserlocId"]),                                                      
            //                                            "1", 
            //                                            docid);
            var docnum = _bllcommon.GetDocumentNo("StockAdjustments",
                                                    Convert.ToInt32(Session["loggeduserlocId"]),
                                                    Convert.ToString(stockheader.StockLocationId),                                                   
                                                    docid,false, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));

            //var docnum = _bllcommon.GetDocumentNo("Purchase Order",
            //                                   Convert.ToInt32(Session["loggeduserlocId"]),
            //                                   Convert.ToString(poheader.POLocationId),
            //                                   docid,
            //                                   false);

            stockheader.DocumentId = docid;
            stockheader.DocumentNo = docnum;
            stockheader.CreatedUser = Session["loggeduser"].ToString();
            stockheader.CreatedDate = stockheader.CreatedDate;
            stockheader.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
            stockheader.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var res = _bllstockAdjustment.SubmitStockAdjustment(stockheader);
            @ViewBag.PCode = stockheader.DocumentNo;
            if (res == true)
            {
                @ViewBag.Message = "1";
                ModelState.Clear();
                docid = _bllcommon.GetDcumentId("StockAdjustments", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                docnum = _bllcommon.GetDocumentNo("StockAdjustments", 
                    Convert.ToInt32(Session["loggeduserlocId"]),
                    "1",
                    docid, 
                    true, Convert.ToInt32(Session["loggedusercompanyId"].ToString())
                    );
               // docnum = _bllcommon.GetDocumentTemporyNo("StockAdjustments", Convert.ToInt32(Session["loggeduserlocId"]), "1", docid, true);

                StockAdjustmentHeader stockAdjheader = new StockAdjustmentHeader();
                stockAdjheader.DocumentNo = docnum;
                @ViewBag.HeaderId = stockheader.StockAdjustmentHeaderId;
                Index();
                return View("~/Views/Transactions/StockAdjustment/StockAdjustment.cshtml", stockAdjheader);
            }
            else
            {
                @ViewBag.Message = "3";
                ViewBag.StockLocationId = stockheader.StockLocationId;
                stockheader.Remark = stockheader.Remark;
            }
            return View("~/Views/Transactions/StockAdjustment/StockAdjustment.cshtml", stockheader);
        }

        [HttpGet]
        public JsonResult GetAdjustmentData(long productid, long locid, decimal adjStock, string type)
        {
            List<StockAdjustmentViewModel> vvm = new List<StockAdjustmentViewModel>();
            var prd = _bllstockAdjustment.GetProductDetails(productid, locid, adjStock, type, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            return new JsonResult { Data = prd.FirstOrDefault(), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpGet]
        public JsonResult GetAdjustmentTypes()
        {
            var types = _bllstockAdjustment.GetTypes();
            return Json(JsonConvert.SerializeObject(types, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetAdjustmentTypesById(int Id)
        {
            var types = _bllstockAdjustment.GetTypesById(Id);
            return Json(JsonConvert.SerializeObject(types, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetAdjustmentTypesByBaseType(string BType)
        {
            var types = _bllstockAdjustment.GetTypesByBaseType(BType);
            return Json(JsonConvert.SerializeObject(types, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize(Roles = "Reports")]
        public ActionResult StockAdjustmentReport()
        {
            if (_bllconfiguration.GetConfiguration("UReports", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn)
            {

                if (!_appmanager.CheckReportPermissions(999, Session["loggeduserempcode"].ToString(), "RPTStockAdjustment"))
                {
                    @ViewBag.Permissions = "No user permissions to View Stock Adjustments";
                    return View("~/Views/Account/AccessDenied.cshtml");
                }

            }

            return View("~/Views/Reports/Stock/RPTStockAdjustment.cshtml");
        }

        [HttpGet]
        public JsonResult GetDocNoByLocId(long locid,DateTime frm, DateTime to)
        {
            //DateTime FromDate = frm.Date;
            //DateTime ToDate = to.Date;
            var types = _bllstockAdjustment.GetDocNoByLocId(locid, Convert.ToInt32(Session["loggedusercompanyId"].ToString())).Where(s => s.CreatedDate.Date >= frm.Date && s.CreatedDate.Date <= to.Date);
            return Json(JsonConvert.SerializeObject(types, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Authorize(Roles = "Reports")]
        public ActionResult StockAdjustmentReport(StockAdjustmentReportViewModel vm)
        {
            TempData["SelectionCriteria"] = vm;//rerquire values for excel file generation
            if (_bllconfiguration.GetConfiguration("UReports", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn)
            {

                if (!_appmanager.CheckReportPermissions(999, Session["loggeduserempcode"].ToString(), "RPTStockAdjustment"))
                {
                    @ViewBag.Permissions = "No user permissions to View Stock Adjustments";
                    return View("~/Views/Account/AccessDenied.cshtml");
                }

            }
            
            decimal totalCost = 0,TotalQTyOld=0, TotalQTyNew = 0, TotalQTy = 0;
            var reportdata = _bllstockAdjustment.StockAdjustmentReport(vm.LocationId, vm.DocumentId,
                                                                    vm.DateFrom, vm.DateTo, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            foreach (var s in reportdata)
            {
                totalCost += s.Detail.ToList().Sum(X => X.costvalue); //calculating total cost
                TotalQTyOld += s.Detail.ToList().Sum(X => X.AdjustStock);
                TotalQTyNew+= s.Detail.ToList().Sum(X => X.NewStock);
            }

            ViewBag.TotalCost = totalCost;//passing value to viewbag and get it from View
            ViewBag.TotalQTyOld = TotalQTyOld;
            ViewBag.TotalQTyNew = TotalQTyNew;
            ViewBag.TotalQty = TotalQTyOld+ TotalQTyNew;

            StockAdjustmentReportViewModelSelection = vm;//rerquire values for excel file generation

            return View("~/Views/Reports/Stock/RPTStockAdjustment.cshtml", reportdata);
        }

        public ActionResult ExportToExcel(StockAdjustmentReportViewModel vvm)
        {

            StockAdjustmentReportViewModel t1 = StockAdjustmentReportViewModelSelection;

            vvm = TempData["SelectionCriteria"] as StockAdjustmentReportViewModel;

            //var det = _bllcAlert.GetConfigDetails();
            byte[] excelFileArry = null;

            excelFileArry = GenerateExcelSheet(vvm);
            // Pass above test data and get excel file as byte array.
            try

            {
                //  byte[] document = this.StreamFile(filePath);
                Response.Clear();
                Response.AddHeader("content-disposition", "attachment;filename='" + "Stock Adjustment Report" + "'" + Convert.ToString(DateTime.Now.ToFileTimeUtc()) + ".xls");
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

        private byte[] GenerateExcelSheet(StockAdjustmentReportViewModel vvm)
        {
            using (ExcelPackage pck = new ExcelPackage())
            {
                //Create the worksheet
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Stock Adjustment Report");
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
                var stock = _bllstockAdjustment.StockAdjustmentReport(vvm.LocationId, vvm.DocumentId,
                                                                    vvm.DateFrom, vvm.DateTo, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));

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

                    ws.Cells[6, 2].Value = "Stock Adjustment Report";
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

                ws.Cells[12, 1].Value = "Product Code";
                ws.Cells[12, 2].Value = "Product";
                ws.Cells[12, 3].Value = "Old Stock";
                ws.Cells[12, 4].Value = "Adjusted Stock";
                ws.Cells[12, 5].Value = "New Stock";
                ws.Cells[12, 6].Value = "Type";
                ws.Cells[12, 7].Value = "Reason";
                ws.Cells[12, 8].Value = "Cost Price";
                ws.Cells[12, 9].Value = "Cost Value";


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
                decimal SumOfOldStock = 0;
                decimal SumOfDocumentOldStock = 0;
                decimal SumOfNewStock = 0;
                decimal TotalCostValue = 0;
                foreach (var dr in stock) //
                {
                    SumOfDocumentOldStock =0;
                    foreach (var s in dr.Detail)
                    {
                        ws.Cells[rowIndex, 1].Value = s.ProductCode;
                        ws.Cells[rowIndex, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                        ws.Cells[rowIndex, 2].Value = s.ProductName;
                        ws.Cells[rowIndex, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                        ws.Cells[rowIndex, 3].Value = s.CurrentStock.ToString("n2");
                        ws.Cells[rowIndex, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                        ws.Cells[rowIndex, 4].Value = s.AdjustStock.ToString("n2");
                        ws.Cells[rowIndex, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                        ws.Cells[rowIndex, 5].Value = s.NewStock.ToString("n2");
                        ws.Cells[rowIndex, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                        ws.Cells[rowIndex, 6].Value = s.BaseType;
                        ws.Cells[rowIndex, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                        ws.Cells[rowIndex, 7].Value = s.Reason;
                        ws.Cells[rowIndex, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                        ws.Cells[rowIndex, 8].Value = s.CostPrice.ToString("n2");
                        ws.Cells[rowIndex, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                        ws.Cells[rowIndex, 9].Value = s.costvalue.ToString("n2");
                        ws.Cells[rowIndex, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                        rowIndex++;
                        SumOfOldStock += s.CurrentStock;
                        SumOfNewStock = s.NewStock;
                        TotalCostValue += s.costvalue;
                        SumOfDocumentOldStock = SumOfOldStock;

                    }

                    ws.Cells[rowIndex, 1].Value = "Document Total:";
                    ws.Cells[rowIndex, 3].Value = SumOfDocumentOldStock;
                    //Color colFromHexHeading2 = System.Drawing.ColorTranslator.FromHtml("#919089");
                    //ws.Cells[rowIndex, 1, rowIndex , 9].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    //ws.Cells[rowIndex , 1, rowIndex , 9].Style.Fill.BackgroundColor.SetColor(colFromHexHeading);

                }
                    //var table1 = ws.Cells[14, 1, rowIndex, 4];
                    ws.Cells[rowIndex, 1].Value = "All value Total:";
                    ws.Cells[rowIndex, 3].Value = SumOfOldStock;
                    ws.Cells[rowIndex, 5].Value = SumOfNewStock;
                    ws.Cells[rowIndex, 6].Value = "TotalQTy: ";
                    ws.Cells[rowIndex, 7].Value = SumOfOldStock + SumOfNewStock;
                    ws.Cells[rowIndex, 9].Value = TotalCostValue;

                    Color colFromHexHeading1 = System.Drawing.ColorTranslator.FromHtml("#919089");

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
    }
}