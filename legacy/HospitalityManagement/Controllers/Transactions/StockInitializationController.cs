using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RIT.HMS.Domain.Transactions;
using RIT.HMS.BLL.MasterData;
using RIT.HMS.BLL.Common;
using RIT.HMS.BLL.Configurations;
using RIT.HMS.BLL.TransactionData;
using RIT.HMS.Domain.ViewModels;
using RIT.HMS.Domain.ViewModels.Reports;
using System.Drawing;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Threading;

namespace HospitalityManagement.Controllers.Transactions
{
    [SessionTimeout]
    public class StockInitializationController : Controller
    {
        private readonly BLL_Product _bllproduct;
        private readonly BLL_Common _bllcommon;
        private readonly BLL_Configuration _bllconfiguration;
        private readonly BLL_StockAdjustment _bllstockAdjustment;
        private readonly BLL_Company _bllcompany;
        private readonly BLL_Location _blllocation;
        private AppManager _appmanager;
        private StockMovementViewModel StockMovementViewModelSelection;//
        public StockInitializationController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
             _bllproduct = new BLL_Product(cn);
            _bllcommon = new BLL_Common(cn);
            _bllconfiguration = new BLL_Configuration(cn);
            _bllstockAdjustment = new BLL_StockAdjustment(cn);
            _bllcompany = new BLL_Company(cn);
            _blllocation = new BLL_Location(cn);
            _appmanager = new AppManager(cn);
            StockMovementViewModelSelection = new StockMovementViewModel();
        }

        public ActionResult Index()
        {
            //// check permissions
            if (!_appmanager.SetPermissions(15, Session["loggeduserempcode"].ToString(), "StockInit"))
            {
                @ViewBag.Permissions = "No user permissions to Create Stock Initializations";
                return View("~/Views/Account/AccessDenied.cshtml");
            }
            //end check permissions
            

            var header = new StockAdjustmentHeader();
            Session["POWizadUI"] = _bllconfiguration.GetConfiguration("UIWZ", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            int docid = _bllcommon.GetDcumentId("Stock Initialization", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));

            var docnum = _bllcommon.GetDocumentTemporyNo("Stock Initialization", 
                                                Convert.ToInt32(Session["loggeduserlocId"]), "1", docid, true, 
                                                Convert.ToInt32(Session["loggedusercompanyId"].ToString())
                                                );

            header.DocumentNo = docnum;
            header.DocumentId = docid;
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());

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
            header.CreatedDate = DateTime.Now;
            return View("~/Views/Transactions/StockInitialization/StockInitialization.cshtml",header);
        }

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

            return PartialView("~/Views/Receipts/StockInitView.cshtml", stkheader);
        }


        [HttpPost]
        public ActionResult SubmitInitialization(StockAdjustmentHeader stockheader)
        {

            // check permissions
            if (!_appmanager.SetPermissions(15, Session["loggeduserempcode"].ToString(), "StockInit"))
            {
                @ViewBag.Permissions = "No user permissions to Create Stock Initializations";
                return View("~/Views/Account/AccessDenied.cshtml");
            }
            //end check permissions

            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            //ViewBag.productdata = _bllproduct.GetActiveProducts(companyid);
            if (stockheader.StockLocationId == 0)
            {
                ModelState.AddModelError("StockLocationId", "Please select the location !");
                return View("~/Views/Transactions/StockInitialization/StockInitialization.cshtml", stockheader);
            }

            var last = _bllstockAdjustment.GetLastInitDate(Convert.ToInt32(Session["loggedusercompanyId"].ToString()),
                                                             Convert.ToInt32(Session["loggeduserlocId"].ToString()));

            if (!stockheader.IsExists)
            {
                DateTime lastdate = stockheader.CreatedDate.Date;
                if (last != null)
                {
                    lastdate = last.CreatedDate;
                }

                if (lastdate.Date > stockheader.CreatedDate.Date)
                {

                    ViewBag.StockLocationId = stockheader.StockLocationId;
                    stockheader.Remark = stockheader.Remark;
                    ModelState.AddModelError("StockAdjDetail", "Can not backdate the Stock Initialization !");
                    return View("~/Views/Transactions/StockInitialization/StockInitialization.cshtml", stockheader);

                }
            }

            if (stockheader.StockAdjDetail.Count == 0)
            {
                ViewBag.StockLocationId = stockheader.StockLocationId;
                stockheader.Remark = stockheader.Remark;
                ModelState.AddModelError("StockAdjDetail", "Please enter stock adjustment details !");
                return View("~/Views/Transactions/StockInitialization/StockInitialization.cshtml", stockheader);
            }
            int docid = 0;
            string docnum = "";
            if (stockheader.IsExists == false)
            {
                docid = _bllcommon.GetDcumentId("Stock Initialization", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                docnum = _bllcommon.GetDocumentNo("Stock Initialization",
                                                       Convert.ToInt32(Session["loggeduserlocId"]),
                                                       Convert.ToString(stockheader.StockLocationId),
                                                       docid, false, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));


                stockheader.DocumentId = docid;
                stockheader.DocumentNo = docnum;
            }
            else
            {
                if (stockheader.DocumentStatus==1)
                {
                    docid = _bllcommon.GetDcumentId("Stock Initialization", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                    docnum = _bllcommon.GetDocumentNo("Stock Initialization",
                                                           Convert.ToInt32(Session["loggeduserlocId"]),
                                                           Convert.ToString(stockheader.StockLocationId),
                                                           docid, false, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));


                    stockheader.DocumentId = docid;
                    stockheader.DocumentNo = docnum;
                }
            }

            stockheader.CreatedUser = Session["loggeduser"].ToString();
            //stockheader.CreatedDate = DateTime.Now;
            stockheader.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
            stockheader.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            stockheader.DocumentStatus = 2; // 2 is for submit 
            var res = _bllstockAdjustment.SubmitStockInitialization(stockheader);
            @ViewBag.PCode = stockheader.DocumentNo;
            if (res != 0)
            {
                @ViewBag.Message = "1";
                ModelState.Clear();

                docid = _bllcommon.GetDcumentId("Stock Initialization", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                docnum = _bllcommon.GetDocumentNo("Stock Initialization",
                    Convert.ToInt32(Session["loggeduserlocId"]),
                    "1",
                    docid,
                    true, Convert.ToInt32(Session["loggedusercompanyId"].ToString())
                    );
                             
                StockAdjustmentHeader stockAdjheader = new StockAdjustmentHeader();
                stockAdjheader.DocumentNo = docnum;
                // @ViewBag.HeaderId = stockheader.StockAdjustmentHeaderId;
                @ViewBag.HeaderId = res;
                return View("~/Views/Transactions/StockInitialization/StockInitialization.cshtml", stockAdjheader);
            }
            else
            {
                @ViewBag.Message = "3";
                ViewBag.StockLocationId = stockheader.StockLocationId;
                stockheader.Remark = stockheader.Remark;
            }
            return View("~/Views/Transactions/StockInitialization/StockInitialization.cshtml", stockheader);
        }

        public ActionResult ViewAllTempInits()
        {
            var tempinits = _bllstockAdjustment.GetInitializations(Convert.ToInt32(Session["loggedusercompanyId"].ToString()),
                 Convert.ToInt32(Session["loggeduserlocId"]),1); // 1 is for temp inits 
            return View("~/Views/Transactions/StockInitialization/ViewTempInitializations.cshtml", tempinits);
        }

        public ActionResult Edit(int initheaderid)
        {
            //// check permissions
            if (!_appmanager.SetPermissions(15, Session["loggeduserempcode"].ToString(), "StockInit"))
            {
                @ViewBag.Permissions = "No user permissions to Create Stock Initializations";
                return View("~/Views/Account/AccessDenied.cshtml");
            }
            //end check permissions
            
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var header = new StockAdjustmentHeader();
            header = _bllstockAdjustment.GetInitializationById(initheaderid, companyid, Convert.ToInt32(Session["loggeduserlocId"]));

            ViewBag.StockLocationId = header.StockLocationId;
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
            header.IsExists = true;
            return View("~/Views/Transactions/StockInitialization/StockInitialization.cshtml", header);
        }

        [HttpPost]
        public ActionResult TempSave(StockAdjustmentHeader stockheader)
        {

            // check permissions
            if (!_appmanager.SetPermissions(15, Session["loggeduserempcode"].ToString(), "StockInit"))
            {
                @ViewBag.Permissions = "No user permissions to Create Stock Initializations";
                return View("~/Views/Account/AccessDenied.cshtml");
            }
            //end check permissions

            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            //ViewBag.productdata = _bllproduct.GetActiveProducts(companyid);
            if (stockheader.StockLocationId == 0)
            {
                ModelState.AddModelError("StockLocationId", "Please select the location !");
                return View("~/Views/Transactions/StockInitialization/StockInitialization.cshtml", stockheader);
            }

            var last = _bllstockAdjustment.GetLastInitDate(Convert.ToInt32(Session["loggedusercompanyId"].ToString()),
                                                              Convert.ToInt32(Session["loggeduserlocId"].ToString()));

            if (!stockheader.IsExists)
            {
                DateTime lastdate = stockheader.CreatedDate.Date;
                if (last != null)
                {
                    lastdate = last.CreatedDate;
                }

                if (lastdate.Date > stockheader.CreatedDate.Date)
                {

                    ViewBag.StockLocationId = stockheader.StockLocationId;
                    stockheader.Remark = stockheader.Remark;
                    ModelState.AddModelError("StockAdjDetail", "Can not backdate the Stock Initialization !");
                    return View("~/Views/Transactions/StockInitialization/StockInitialization.cshtml", stockheader);

                }
            }
            if (stockheader.StockAdjDetail.Count == 0)
            {
                ViewBag.StockLocationId = stockheader.StockLocationId;
                stockheader.Remark = stockheader.Remark;
                ModelState.AddModelError("StockAdjDetail", "Please enter stock adjustment details !");
                return View("~/Views/Transactions/StockInitialization/StockInitialization.cshtml", stockheader);
            }
            int docid = 0;
            string docnum = "";

            stockheader.CreatedUser = Session["loggeduser"].ToString();
           // stockheader.CreatedDate = DateTime.Now;
            stockheader.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
            stockheader.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            stockheader.DocumentStatus = 1; // 1 is for temp save 
            var res = _bllstockAdjustment.SubmitStockInitialization(stockheader);
            @ViewBag.PCode = stockheader.DocumentNo;
            if (res != 0)
            {
                @ViewBag.Message = "1";
                ModelState.Clear();
                docid = _bllcommon.GetDcumentId("Stock Initialization", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                docnum = _bllcommon.GetDocumentNo("Stock Initialization",
                    Convert.ToInt32(Session["loggeduserlocId"]),
                    "1",
                    docid,
                    true, Convert.ToInt32(Session["loggedusercompanyId"].ToString())
                    );
                // docnum = _bllcommon.GetDocumentTemporyNo("StockAdjustments", Convert.ToInt32(Session["loggeduserlocId"]), "1", docid, true);
                StockAdjustmentHeader stockAdjheader = new StockAdjustmentHeader();
                stockAdjheader.DocumentNo = docnum;
                //@ViewBag.HeaderId = stockheader.StockAdjustmentHeaderId;
                @ViewBag.HeaderId = res;
                return View("~/Views/Transactions/StockInitialization/StockInitialization.cshtml", stockAdjheader);
            }
            else
            {
                @ViewBag.Message = "3";
                ViewBag.StockLocationId = stockheader.StockLocationId;
                stockheader.Remark = stockheader.Remark;
            }
            return View("~/Views/Transactions/StockInitialization/StockInitialization.cshtml", stockheader);
        }
        [HttpGet]
        public JsonResult GetInitData(long productid, long locid, decimal adjStock, string type)
        {
            List<StockAdjustmentViewModel> vvm = new List<StockAdjustmentViewModel>();
            var prd = _bllstockAdjustment.GetProductInitDetails(productid, locid, adjStock, type, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            return new JsonResult { Data = prd.FirstOrDefault(), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [Authorize(Roles = "Reports")]
        public ActionResult RPTStockMovement()
        {
            if (_bllconfiguration.GetConfiguration("UReports", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn)
            {

                if (!_appmanager.CheckReportPermissions(999, Session["loggeduserempcode"].ToString(), "RPTStockMovement"))
                {
                    @ViewBag.Permissions = "No user permissions to View Stock Movements";
                    return View("~/Views/Account/AccessDenied.cshtml");
                }

            }

            StockMovementViewModel vmmovement = new StockMovementViewModel()
            {
                DateFrom = DateTime.Now,
                DateTo = DateTime.Now,
                LocationId = Convert.ToInt32(Session["loggeduserlocId"]),
            };

            return View("~/Views/Reports/Stock/RPTStockMovement.cshtml", vmmovement);
        }

        [HttpPost]
        [Authorize(Roles = "Reports")]
        public ActionResult RPTStockMovement(StockMovementViewModel movement)
        {
            if (_bllconfiguration.GetConfiguration("UReports", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn)
            {

                if (!_appmanager.CheckReportPermissions(999, Session["loggeduserempcode"].ToString(), "RPTStockMovement"))
                {
                    @ViewBag.Permissions = "No user permissions to View Stock Movements";
                    return View("~/Views/Account/AccessDenied.cshtml");
                }

            }
            TempData["SelectionCriteria"] = movement;//rerquire values for excel file generation
            StockMovementViewModelSelection = movement;//rerquire values for excel file generation

            movement.CompanyId = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            return View("~/Views/Reports/Stock/RPTStockMovement.cshtml",_bllstockAdjustment.StockMovementReport(movement));
                                                   
        }

        public ActionResult ExportToExcel(StockMovementViewModel vvm)
        {

            StockMovementViewModel t1 = StockMovementViewModelSelection;

            vvm = TempData["SelectionCriteria"] as StockMovementViewModel;

            //var det = _bllcAlert.GetConfigDetails();
            byte[] excelFileArry = null;

            excelFileArry = GenerateExcelSheet(vvm);
            // Pass above test data and get excel file as byte array.
            try

            {
                //  byte[] document = this.StreamFile(filePath);
                Response.Clear();
                Response.AddHeader("content-disposition", "attachment;filename='" + "StockMovementReport" + "'" + Convert.ToString(DateTime.Now.ToFileTimeUtc()) + ".xls");
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

        private byte[] GenerateExcelSheet(StockMovementViewModel movement)
        {
            using (ExcelPackage pck = new ExcelPackage())
            {
                //Create the worksheet
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Stock Movement Report");
                #region Logo

                string filename = Server.MapPath(@"~/bin/LOGO.jpg");

                try
                {
                    ws.Cells[1, 1, 6, 1].Merge = true;
                }
                catch
                {

                }

                #endregion

                #region Company Detail set to heading

                var stock = _bllstockAdjustment.StockMovementReport(movement);
              
                //get the Document heading details

                string compName, Address1, Address2, Address3, Tele, Fax, Email, ReportHead = "";
                compName = _blllocation.GetLocationById(movement.LocationId).LocationName;
                Address1 = _blllocation.GetLocationById(movement.LocationId).Address1;
                Address2 = _blllocation.GetLocationById(movement.LocationId).Address2;
                Address3 = _blllocation.GetLocationById(movement.LocationId).Address3;
                Tele = _blllocation.GetLocationById(movement.LocationId).Telephone;
                Fax = _blllocation.GetLocationById(movement.LocationId).Fax;
                Email = _blllocation.GetLocationById(movement.LocationId).Email;

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

                    ws.Cells[5, 2].Value = "Tel:- " + Tele + " / " + ",  Fax:- " + Fax + ",  E mail:- " + Email;
                    ws.Cells[5, 2, 5, 12].Merge = true;
                    ws.Cells[5, 2, 5, 12].Style.Font.Size = 10;

                    ws.Cells[6, 2].Value = "Stock Movement Report";
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

                ws.Cells[3, 14].Value = DateTime.Now.Date.ToShortDateString();

                ws.Cells[4, 14].Value = DateTime.Now.ToString("h:mm tt");

                if (Session["loggeduser"] != null)
                    ws.Cells[5, 14].Value = Session["loggeduser"].ToString();
                else
                    ws.Cells[5, 14].Value = " ";


                #endregion

                #region Excel heading set here
                ws.Cells[12, 1].Value = "Item Code";
                ws.Cells[12, 2].Value = "Item";
                ws.Cells[12, 3].Value = "Opening Balance";
                ws.Cells[12, 4].Value = "GRN Qty";
                ws.Cells[12, 5].Value = "Return Qty";
                ws.Cells[12, 6].Value = "Stock Adjustment";
                ws.Cells[12, 7].Value = "Transfer In";
                ws.Cells[12, 8].Value = "Transfer Out";
                ws.Cells[12, 9].Value = "Sales";
                ws.Cells[12, 10].Value = "Sales Returns";
                ws.Cells[12, 11].Value = "Balance Qty";
                ws.Cells[12, 12].Value = "Sold Qty";
                ws.Cells[12, 13].Value = "Closing Balance"+"-" +stock.DateTo.ToString("yyyy-MM-dd");
                ws.Cells[12, 14].Value = "Rate";
                ws.Cells[12, 15].Value = "Total Amount";

                Color colFromHexHeading = System.Drawing.ColorTranslator.FromHtml("#919089");// excel heading row highlited
                ws.Cells[12, 1, 12, 15].Style.Fill.PatternType = ExcelFillStyle.Solid; // excel heading row highlited
                ws.Cells[12, 1, 12, 15].Style.Fill.BackgroundColor.SetColor(colFromHexHeading);// excel heading row highlited

                var table = ws.Cells[12, 1, 12, 15];
                table.Style.Border.Top.Style =
                table.Style.Border.Left.Style =
                table.Style.Border.Right.Style =
                table.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                table.Style.Font.Bold = true;
                table.Style.Font.Name = "Calibri";
                table.AutoFitColumns();
                #endregion Excel heading set here

                int rowIndex = 14; //Data is inserted from this row

                #region AssignValues to excel rows and column
                foreach (var dr in stock.Details) //
                {
                                       
                    decimal soldqty = ((dr.OpeningBalance + dr.TotalQty + dr.GRNQty - dr.ReturnQty + dr.StockAdjustment + dr.TransferIn) - dr.OpeningBalanceToDate - dr.TransferOut - dr.SalesInQty + dr.SalesOutQty);

                    ws.Cells[rowIndex, 1].Value = dr.ProductCode;
                    ws.Cells[rowIndex, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    ws.Cells[rowIndex, 2].Value = dr.ProductDesc;
                    ws.Cells[rowIndex, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    ws.Cells[rowIndex, 3].Value = dr.OpeningBalance;
                    ws.Cells[rowIndex, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    ws.Cells[rowIndex, 4].Value = dr.GRNQty;
                    ws.Cells[rowIndex, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    ws.Cells[rowIndex, 5].Value = dr.ReturnQty;
                    ws.Cells[rowIndex, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    ws.Cells[rowIndex, 6].Value = dr.StockAdjustment;
                    ws.Cells[rowIndex, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    ws.Cells[rowIndex, 7].Value = dr.TransferIn;
                    ws.Cells[rowIndex, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    ws.Cells[rowIndex, 8].Value = dr.TransferOut;
                    ws.Cells[rowIndex, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    ws.Cells[rowIndex, 9].Value = dr.SalesInQty;
                    ws.Cells[rowIndex, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    ws.Cells[rowIndex, 10].Value = dr.SalesOutQty;
                    ws.Cells[rowIndex, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    //ws.Cells[rowIndex, 11].Value = (dr.OpeningBalance + dr.TotalQty + dr.GRNQty - dr.ReturnQty + dr.StockAdjustment + dr.TransferIn - dr.TransferOut - dr.SalesInQty + dr.SalesOutQty);
                    //ws.Cells[rowIndex, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    ws.Cells[rowIndex, 11].Value = dr.TotalQty;
                    ws.Cells[rowIndex, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    // ws.Cells[rowIndex, 12].Value = soldqty;
                    ws.Cells[rowIndex, 12].Value = dr.SoldQty;
                    ws.Cells[rowIndex, 12].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    ws.Cells[rowIndex, 13].Value = dr.OpeningBalanceToDate ;
                    ws.Cells[rowIndex, 13].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    ws.Cells[rowIndex, 14].Value = dr.Rate;
                    ws.Cells[rowIndex, 14].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    //ws.Cells[rowIndex, 15].Value = (soldqty*dr.Rate);
                    ws.Cells[rowIndex, 15].Value = dr.TotalAmount;
                    ws.Cells[rowIndex, 15].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    rowIndex++;
                }
                #endregion #region AssignValues to excel rows and column
                ws.Cells[rowIndex, 1, rowIndex, 15].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[rowIndex, 1, rowIndex, 15].Style.Fill.BackgroundColor.SetColor(colFromHexHeading);

                var table1 = ws.Cells[14, 1, rowIndex, 15];
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