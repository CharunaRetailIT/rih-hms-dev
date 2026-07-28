using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Newtonsoft.Json;
using RIT.HMS.BLL.Common;
using RIT.HMS.BLL.TransactionData;
using RIT.HMS.BLL.MasterData;
using RIT.HMS.Domain.ViewModels;
using RIT.HMS.Domain.ViewModels.Reports;
using RIT.HMS.Domain.Transactions;
using RIT.HMS.BLL.Configurations;
using OfficeOpenXml;
using System.IO;
using System.Data;
using OfficeOpenXml.Style;
using System.Drawing;


namespace HospitalityManagement.Controllers.Transactions
{
    [SessionTimeout]
    public class POController : Controller
    {
        // GET: PO

        //CommonService commonService = new CommonService();

        private readonly BLL_Common _bllcommon;
        private readonly BLL_PurchaseOrder _bllpurchaseorder;
        private readonly BLL_Product _bllproduct;
        private readonly BLL_Company _bllcompany;
        private readonly BLL_Location _blllocation;
        private readonly BLL_Supplier _bllsupplier;
        private readonly BLL_GRN _bllgrn;
        private readonly BLL_Configuration _bllconfiguration;
        private readonly BLL_DocStatus _blldocStatus;
        private readonly BLL_MonthEnd _bllmonthend;
        private readonly BLL_Event _bllevents;
        private readonly BLL_PaymentTerm _bllpaymentterm;


        private readonly BLL_RequestNote _bllrequestnote;


        BLL_Tax _blltax;
        private AppManager _appmanager;
        BLL_Product _bllInvProduct;

        public POController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _bllcommon = new BLL_Common(cn);
            _bllpurchaseorder = new BLL_PurchaseOrder(cn);
            _bllproduct = new BLL_Product(cn);
            _bllcompany = new BLL_Company(cn);
             _blllocation = new BLL_Location(cn);
            _bllsupplier = new BLL_Supplier(cn);
            _bllgrn = new BLL_GRN(cn);
            _bllconfiguration = new BLL_Configuration(cn);
            _blldocStatus = new BLL_DocStatus(cn);
            _bllmonthend = new BLL_MonthEnd(cn);
            _bllevents = new BLL_Event(cn);
            _bllpaymentterm = new BLL_PaymentTerm(cn);
             _appmanager = new AppManager(cn);
           _blltax = new BLL_Tax(cn);
           _bllInvProduct = new BLL_Product(cn);
            _bllrequestnote = new BLL_RequestNote(cn);
        }

        // [Authorize(Roles = "POCreatee")]
        [HttpGet]
        public ActionResult CreatePO()
        {
            // check permissions
            // if(!_appmanager.SetPermissions(3,Session["loggeduserempcode"].ToString(), "POCreatee"))
            //{
            //    @ViewBag.Permissions = "No user permissions to Create PO";
            //    return View("~/Views/Account/AccessDenied.cshtml");
            //}
            //end check permissions
           string LoadBySupplierID_ = System.Configuration.ConfigurationManager.AppSettings["LoadBySupplierID"].ToString();
            ViewBag.LoadBySupplierID = LoadBySupplierID_.ToUpper();
            Session["POWizadUI"] = _bllconfiguration.GetConfiguration("UIWZ", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            ViewBag.IsAllItems = _bllconfiguration.GetConfiguration("DITEMS", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;                    
            Session["ConfigPOLineTax"] = _bllconfiguration.GetConfiguration("LINETAXPO", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            Session["POApprovels"] = _bllconfiguration.GetConfiguration("APPPO", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;

            Session["DisablePOReject"] = _bllconfiguration.GetConfiguration("DisablePOReject", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;

            Session["TOGGridAdditionalColumn"] = _bllconfiguration.GetConfiguration("TOGGridAdditionalColumn", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            Session["IsEventMandatory"] = _bllconfiguration.GetConfiguration("IsEventMandatory", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            



            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
          //  ViewBag.productdata = _bllproduct.GetActiveProducts(companyid);
            ViewBag.poLocationId = Convert.ToInt16(Session["loggeduserlocId"]);
              var poheader = new PurchaseOrderHeader();
           
            int docid = _bllcommon.GetDcumentId("Purchase Order", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            var docnum = _bllcommon.GetDocumentNo("Purchase Order", 
                                                    Convert.ToInt32(Session["loggeduserlocId"]), 
                                                    "1",
                                                     docid, 
                                                     true, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));  //true

            poheader.DocumentNo = docnum;
            poheader.ExpectedDate = DateTime.Now;
            poheader.PODate = DateTime.Now;
            poheader.ExpectedDate = DateTime.Now;


            //load from web config this LoadBySupplierID,  load product by supplierID if the LoadBySupplierID=true 
            string LoadBySupplierID = System.Configuration.ConfigurationManager.AppSettings["LoadBySupplierID"].ToString();

            List<SelectListItem> FinishGoods = new List<SelectListItem>();
            SelectListItem defaultfinishgood = new SelectListItem();
            defaultfinishgood.Text = "-- Select an Item --";
            defaultfinishgood.Value = "0";
            FinishGoods.Add(defaultfinishgood);
            if (ViewBag.IsAllItems==true)
            {

                var itemstoload = _bllproduct.GetActiveProducts(companyid); 
                foreach (var prd in itemstoload)
                {
                    SelectListItem dbprd = new SelectListItem();
                    dbprd.Text = (prd.ProductCode + "-" + prd.ProductName);
                    dbprd.Value = prd.ProductId.ToString();
                    FinishGoods.Add(dbprd);
                }
            }
            Session["AllItemsToPO"] = FinishGoods;



            return View("~/Views/Transactions/PO/CreatePO.cshtml", poheader);
        }

       // [Authorize(Roles = "POView")]
        [HttpGet]
        public ActionResult ViewAllPO()
        {
            DateTime From_date = new DateTime();
            DateTime To_date = new DateTime();
            int POstatus_id = 0;
            if (Session["POdatefrom"] != null)
            {
                string Fromdate = Session["POdatefrom"].ToString().Trim();
                
                DateTime.TryParse(Fromdate, out From_date);
                ViewBag.DateFrom = From_date.ToString("yyyy-MM-dd");

            }

            if (Session["POdateto"] != null)
            {
                string Todate = Session["POdateto"].ToString().Trim();
                
                DateTime.TryParse(Todate, out To_date);
                ViewBag.DateTo = To_date.ToString("yyyy-MM-dd");

            }
            if (Session["POstatusid"] != null)

            {
               
                int.TryParse(Session["POstatusid"].ToString().Trim(), out POstatus_id);

                ViewBag.POstatusid = POstatus_id;
            }

            Session["TOGGridAdditionalColumn"] = _bllconfiguration.GetConfiguration("TOGGridAdditionalColumn", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            Session["IsEventMandatory"] = _bllconfiguration.GetConfiguration("IsEventMandatory", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            //ViewBag.DateFrom = DateTime.Now;
            //ViewBag.DateTo = DateTime.Now;
           
            // check permissions
            if (!_appmanager.SetPermissions(3, Session["loggeduserempcode"].ToString(), "POView"))
                {
                    @ViewBag.Permissions = "No user permissions to View POs";
                    return View("~/Views/Account/AccessDenied.cshtml");
                }
                //end check permissions
                int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            //var pos = _bllpurchaseorder.GetAllTempPos(companyid).ToList().Where(p =>p.IsGRN == false);

            var pos = _bllpurchaseorder.GetAllPosCurrentMonth(companyid)
  .Where(p => (POstatus_id == 0 || p.DocumentStatus == POstatus_id)); 
              //  &&
              //p.PODate >= From_date &&
              //p.PODate < To_date.AddDays(1));




            pos.ToList().ForEach(p=> 
                {
                    p.SupplierName = _bllsupplier.GetSupplierById(p.SupplierId).SupplierName;
                    if (p.DocumentStatus != 0)
                    {
                        p.DocState = _blldocStatus.GetDocStatusById(p.DocumentStatus).Description;
                    }

                    List<InvRequestNotePOTransaction> _list = _bllgrn.GetInvRequestNotePOTransactionbyID(p.PurchaseOrderHeaderId);

                    if (_list != null && _list.Count > 0)

                    {
                        ViewBag.IsMergedPO = true;

                        RequestNoteHeader RH = _bllrequestnote.GetRequestNoteById(_list[0].RequestNoteHeaderID);
                        if (RH != null)
                        {
                            p.ExpectedDate = RH.ExpectedDeleveryDate;

                        }


                    }







                });

            if (pos == null || pos.ToList().Count == 0)
            {
                ViewBag.IsMergedPO = false;
            }


            string LoadBySupplierID = System.Configuration.ConfigurationManager.AppSettings["LoadBySupplierID"].ToString();

            List<SelectListItem> FinishGoods = new List<SelectListItem>();
            SelectListItem defaultfinishgood = new SelectListItem();
            defaultfinishgood.Text = "-- Select an Item --";
            defaultfinishgood.Value = "0";
            FinishGoods.Add(defaultfinishgood);
            if (ViewBag.IsAllItems == true)
            {

                var itemstoload = _bllproduct.GetActiveProducts(companyid);
                foreach (var prd in itemstoload)
                {
                    SelectListItem dbprd = new SelectListItem();
                    dbprd.Text = (prd.ProductCode + "-" + prd.ProductName);
                    dbprd.Value = prd.ProductId.ToString();
                    FinishGoods.Add(dbprd);
                }
            }
            Session["AllItemsToPO"] = FinishGoods;

            //var sortedList = pos.OrderByDescending(ord => DateTime.Parse(ord.DocumentDate.ToShortDateString())).ToList();
            var sortedList = pos.OrderByDescending(ord =>ord.DocumentDate).ToList();
            return View("~/Views/Transactions/PO/ViewAllPos.cshtml", sortedList);
        }


      
        public ActionResult HMSPurchaseOrders()
        {

            // check permissions
            if (!_appmanager.SetPermissions(3, Session["loggeduserempcode"].ToString(), "POView"))
            {
                @ViewBag.Permissions = "No user permissions to View POs";
                return View("~/Views/Account/AccessDenied.cshtml");
            }
            //end check permissions
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var pos = _bllpurchaseorder.GetAllTempPos(companyid).ToList().Where(p => p.IsGRN == false);
            pos.ToList().ForEach(p =>
            {
                p.SupplierName = _bllsupplier.GetSupplierById(p.SupplierId).SupplierName;
                if (p.DocumentStatus != 0)
                {
                    p.DocState = _blldocStatus.GetDocStatusById(p.DocumentStatus).Description;
                }
            });

            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("PurchaseOrders");
            //---------------2023/03/30 ----------Tharaka---------------
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

            Sheet.Cells[6, 2].Value = "PO Details Report";
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
            Sheet.Cells[8, 1].Value = "PO Number";
            Sheet.Cells[8, 2].Value = "Supplier";
            Sheet.Cells[8, 3].Value = "Expected Delivery Date";
            Sheet.Cells[8, 4].Value = "Value";
            Sheet.Cells[8, 5].Value = "Status";

            //Sheet.Cells["A1"].Value = "PONumber";
            //Sheet.Cells["B1"].Value = "Supplier";
            //Sheet.Cells["C1"].Value = "ExpectedDeliveryDate";
            //Sheet.Cells["D1"].Value = "Value";
            //Sheet.Cells["E1"].Value = "Status";


            int row = 9;
            foreach (var item in pos)
            {
                Sheet.Cells[string.Format("A{0}", row)].Value = item.DocumentNo;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.SupplierName;
                Sheet.Cells[string.Format("C{0}", row)].Value = item.ExpectedDate.ToShortDateString();
                Sheet.Cells[string.Format("D{0}", row)].Value = item.NetAmount.ToString("n2");
                Sheet.Cells[string.Format("E{0}", row)].Value = item.DocState;


                row++;
            }
            #region Header Bold
            Color colFromHexHeading = System.Drawing.ColorTranslator.FromHtml("#919089");

            Sheet.Cells[8, 1, 8, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
            Sheet.Cells[8, 1, 8, 5].Style.Fill.BackgroundColor.SetColor(colFromHexHeading);

            var table = Sheet.Cells[8, 1, 8, 5];
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
            Response.AddHeader("content-disposition", "attachment: attachment;filename=HMSPurchaseOrders.xlsx");

            using (MemoryStream MyMemoryStream = new MemoryStream())
            {
                Ep.SaveAs(MyMemoryStream);
                MyMemoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }



            return View("~/Views/Transactions/PO/ViewAllPos.cshtml", pos);
        }



        //  [Authorize(Roles = "POView")]
        [HttpGet]
        public ActionResult FilterPOs(int statusid, DateTime datefrom, DateTime dateto)
        {

            //ViewBag.DateFrom = datefrom;
           // ViewBag.DateTo = dateto;


            Session["POdatefrom"] = datefrom;
            Session["POdateto"] = dateto;
            Session["POstatusid"] = statusid;

            // check permissions
            if (!_appmanager.SetPermissions(3, Session["loggeduserempcode"].ToString(), "POView"))
            {
                @ViewBag.Permissions = "No user permissions to View POs";
                return View("~/Views/Account/AccessDenied.cshtml");
            }
            //end check permissions

            DateTime FromDate = datefrom.Date;
            DateTime ToDate = dateto.Date;
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());

            // commented on  2022/01/06 - pending po not view issue
            //var pos = _bllpurchaseorder.GetAllTempPos(companyid).ToList().Where(
            //    p => p.IsGRN == false && p.DocumentStatus==statusid &&
            //    p.PODate.Date >= FromDate && p.PODate.Date <= ToDate

            var pos = _bllpurchaseorder.GetAllPoswithDateRange(companyid, FromDate, ToDate)
     .Where(p => (statusid == 0 ||  p.DocumentStatus == statusid) &&
                 p.PODate >= FromDate &&
                 p.PODate < ToDate.AddDays(1));

            pos.ToList().ForEach(p => {
                p.SupplierName = _bllsupplier.GetSupplierById(p.SupplierId).SupplierName;
                if (p.DocumentStatus != 0)
                {
                    p.DocState = _blldocStatus.GetDocStatusById(p.DocumentStatus).Description;
                }

            });
            var sortedList = pos.OrderByDescending(ord => ord.DocumentDate).ToList();

            @ViewBag.DateFrom = datefrom.ToString("yyyy-MM-dd");
            @ViewBag.DateTo = dateto.ToString("yyyy-MM-dd");
            @ViewBag.POstatusid = statusid;
            return View("~/Views/Transactions/PO/ViewAllPos.cshtml", sortedList);
        }


     //   [Authorize(Roles = "POEdit")]
        [HttpGet]
        public ActionResult Edit(long id)
        {

            // check permissions
            //if (!_appmanager.SetPermissions(3, Session["loggeduserempcode"].ToString(), "POEdit"))
            //{
            //    @ViewBag.Permissions = "No user permissions to Edit POs";
            //    return View("~/Views/Account/AccessDenied.cshtml");
            //}
            //end check permissions

            Session["POWizadUI"] = _bllconfiguration.GetConfiguration("UIWZ", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            Session["ConfigPOLineTax"] = _bllconfiguration.GetConfiguration("LINETAXPO", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            ViewBag.IsAllItems = _bllconfiguration.GetConfiguration("DITEMS", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            Session["POApprovels"] = _bllconfiguration.GetConfiguration("APPPO", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;

            Session["DisablePOReject"] = _bllconfiguration.GetConfiguration("DisablePOReject", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            Session["TOGGridAdditionalColumn"] = _bllconfiguration.GetConfiguration("TOGGridAdditionalColumn", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            Session["IsEventMandatory"] = _bllconfiguration.GetConfiguration("IsEventMandatory", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;

            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            ViewBag.productdata = _bllproduct.GetActiveProducts(companyid);

            var po = _bllpurchaseorder.GetPOById(id);
            po.PODetail = _bllpurchaseorder.GetPODetById(id).ToList();
            po.DocState = _blldocStatus.GetDocStatusById(po.DocumentStatus).Description;
            po.PODetail.ForEach
            (
                r =>
                {
                    var product = _bllproduct.GetProductById(r.ProductId);
                    r.ProductName = product.ProductName;
                    r.ProductCode = product.ProductCode;
                    //r.UOM =r.OrderQty+" "+ _bllproduct.GetUOMById(_bllproduct.GetProductById(r.ProductId).PurchasingUnit);
                    r.UOM = _bllproduct.GetUOMById(_bllproduct.GetProductById(r.ProductId).PurchasingUnit);
                    //po.TotSellingPrice += r.SellingPrice;
                    po.TotCostPrice += r.CostPrice;
                    //po.TotDiscounts = r.DiscountAmount;
                }
            );

            if(po.POType==null)
            { po.POType = "Manual"; }


            ViewBag.DocNo = po.DocumentNo;
            ViewBag.Manual = po.POType;
            ViewBag.SupplierId = po.SupplierId;
            ViewBag.DeliveryLocationId = po.DeliveryLocationId;
            ViewBag.PaymentTermId = po.PaymentTermId;
            ViewBag.CurrencyId = po.CurrencyId;
            ViewBag.poLocationId = po.POLocationId;         
            ViewBag.PaymentMethodId = po.PaymentMethodId;
            ViewBag.EventId = po.EventId;

            List<InvRequestNotePOTransaction> _list = _bllgrn.GetInvRequestNotePOTransactionbyID(id);

            if (_list != null && _list.Count > 0)

            {
                ViewBag.IsMergedPO = true;

                RequestNoteHeader RH= _bllrequestnote.GetRequestNoteById(_list[0].RequestNoteHeaderID);
                if(RH!=null)
                {
                    po.ExpectedDate = RH.ExpectedDeleveryDate;

                }

                
            }

            else
            {
                ViewBag.IsMergedPO = false;
            }

            List<SelectListItem> FinishGoods = new List<SelectListItem>();
            SelectListItem defaultfinishgood = new SelectListItem();
            defaultfinishgood.Text = "-- Select an Item --";
            defaultfinishgood.Value = "0";
            FinishGoods.Add(defaultfinishgood);
            if (ViewBag.IsAllItems == true)
            {

                var itemstoload = _bllproduct.GetActiveProducts(companyid);
                foreach (var prd in itemstoload)
                {
                    SelectListItem dbprd = new SelectListItem();
                    dbprd.Text = (prd.ProductCode + "-" + prd.ProductName);
                    dbprd.Value = prd.ProductId.ToString();
                    FinishGoods.Add(dbprd);
                }
            }
            Session["AllItemsToPO"] = FinishGoods;

            return View("~/Views/Transactions/PO/EditPO.cshtml", po);
        }

      //  [Authorize(Roles = "POView")]
        [HttpGet]
        public ActionResult Print(int id,int printstatus)
        {

            // check permissions
                //if (!_appmanager.SetPermissions(3, Session["loggeduserempcode"].ToString(), "POView"))
                //{
                //    @ViewBag.Permissions = "No user permissions to View POs";
                //    return View("~/Views/Account/AccessDenied.cshtml");
                //}
            //end check permissions


            PurchaseOrderHeader poheader = new PurchaseOrderHeader();
            poheader = _bllpurchaseorder.GetPOById(id);
            poheader.Company = _bllcompany.GetCompanyById(poheader.CompanyID).CompanyName;
            poheader.Location = _blllocation.GetLocationById(poheader.POLocationId).LocationName;
            poheader.SysCompany = _bllcompany.GetCompanyById(poheader.CompanyID);
            poheader.Supplier = _bllsupplier.GetSupplierById(poheader.SupplierId);
            poheader.PrintStatus = printstatus;
            poheader.DocState = _blldocStatus.GetDocStatusById(poheader.DocumentStatus).Description;
            var paymantterm = _bllpaymentterm.GetPaymentTermById(poheader.PaymentTermId);
            if (paymantterm != null)
                poheader.PaymentTerm = paymantterm.PaymentTermName;
            else poheader.PaymentTerm = "N/A";
            var polocation = _blllocation.GetLocationById(poheader.POLocationId);
            poheader.POLocationAddress = polocation.Address1 + "," + polocation.Address2 + "," + polocation.Address3 + ".";


            //poheader.PurchaseOrderDetail.ToList().ForEach(d =>
            //{
            //    var prd = _bllproduct.GetProductById(d.ProductId);
            //    d.ProductName = prd.ProductName;
            //    d.ProductCode = prd.ProductCode;

            //});

            poheader.PurchaseOrderDetail
            .Where(d => _bllproduct.GetActiveProductById(d.ProductId) != null)  // Filter out items with null prd
            .ToList()
            .ForEach(d =>
            {
                var prd = _bllproduct.GetActiveProductById(d.ProductId);
                if (prd != null)  // Ensure prd is not null before assigning values
                {
                    d.ProductName = prd.ProductName;
                    d.ProductCode = prd.ProductCode;
                }
            });
            var purchaseOrderDetailsList = poheader.PurchaseOrderDetail.ToList();
            purchaseOrderDetailsList.RemoveAll(d =>
            {
                var prd = _bllproduct.GetActiveProductById(d.ProductId);
                return prd == null;  // Remove if prd is null
            });

            poheader.PurchaseOrderDetail = purchaseOrderDetailsList;

            poheader.TotCostPrice = poheader.PurchaseOrderDetail.Sum(d => d.CostValue);



            List<InvRequestNotePOTransaction> _list = _bllgrn.GetInvRequestNotePOTransactionbyID(id);

            if (_list != null && _list.Count > 0)
            {
                RequestNoteHeader RH = _bllrequestnote.GetRequestNoteById(_list[0].RequestNoteHeaderID);
                if (RH != null)
                {
                    poheader.ExpectedDate = RH.ExpectedDeleveryDate;

                }
            }

            return PartialView("~/Views/Receipts/POView.cshtml", poheader);
        }




        [HttpGet]
        public ActionResult PrintBreakDown(int id, int printstatus)
        {

            // check permissions
            //if (!_appmanager.SetPermissions(3, Session["loggeduserempcode"].ToString(), "POView"))
            //{
            //    @ViewBag.Permissions = "No user permissions to View POs";
            //    return View("~/Views/Account/AccessDenied.cshtml");
            //}
            //end check permissions

           



            PurchaseOrderHeader poheader = new PurchaseOrderHeader();
            poheader = _bllpurchaseorder.GetPOById(id);

            List<VMInvRequestNotePOTransactions> POTransaction = _bllgrn.GetRequestNoteDetailsforPO(id);

            VMInvRequestNotePOTransactionsHead Head = new VMInvRequestNotePOTransactionsHead();
            Head.POTransactionsDetails = POTransaction;



            Head.Company = _bllcompany.GetCompanyById(poheader.CompanyID).CompanyName;
            Head.Location = _blllocation.GetLocationById(poheader.POLocationId).LocationName;
            Head.SysCompany = _bllcompany.GetCompanyById(poheader.CompanyID);
            Head.Supplier = _bllsupplier.GetSupplierById(poheader.SupplierId);
            Head.PrintStatus = printstatus;
            Head.DocState = _blldocStatus.GetDocStatusById(poheader.DocumentStatus).Description;
            var paymantterm = _bllpaymentterm.GetPaymentTermById(poheader.PaymentTermId);
            if (paymantterm != null)
                Head.PaymentTerm = paymantterm.PaymentTermName;
            else Head.PaymentTerm = "N/A";
            var polocation = _blllocation.GetLocationById(poheader.POLocationId);
            Head.POLocationAddress = polocation.Address1 + "," + polocation.Address2 + "," + polocation.Address3 + ".";


            Head.DocumentNo = poheader.DocumentNo;
            Head.PODate = poheader.PODate;

            Head.ExpectedDate = poheader.ExpectedDate;

            List<InvRequestNotePOTransaction> _list = _bllgrn.GetInvRequestNotePOTransactionbyID(id);

            if (_list != null && _list.Count > 0)
            {
                RequestNoteHeader RH = _bllrequestnote.GetRequestNoteById(_list[0].RequestNoteHeaderID);
                if (RH != null)
                {
                    Head.ExpectedDate = RH.ExpectedDeleveryDate;

                }
            }

            Head.CreatedUser = poheader.CreatedUser;




            return PartialView("~/Views/Receipts/MergedPOView.cshtml", Head);
        }



        public Dictionary<string, Dictionary<string, int>> CrosstabData { get; private set; }
        [HttpGet]
        public ActionResult PrintBreakDownKitchen(int id, int printstatus)
        {

            // check permissions
            //if (!_appmanager.SetPermissions(3, Session["loggeduserempcode"].ToString(), "POView"))
            //{
            //    @ViewBag.Permissions = "No user permissions to View POs";
            //    return View("~/Views/Account/AccessDenied.cshtml");
            //}
            //end check permissions





            PurchaseOrderHeader poheader = new PurchaseOrderHeader();
            poheader = _bllpurchaseorder.GetPOById(id);

            List<VMInvRequestNotePOTransactions> POTransaction = _bllgrn.GetRequestNoteDetailsforPO(id);

            VMInvRequestNotePOTransactionsHead Head = new VMInvRequestNotePOTransactionsHead();
            Head.POTransactionsDetails = POTransaction;
                      

            string Remarks = "";

            string RequestNoteNos = "";
            // Initialize CrosstabData if not already initialized
            var CrosstabData = new Dictionary<string, Dictionary<string, int>>();

            


            foreach (var item in POTransaction)
            {
                if (!CrosstabData.ContainsKey(item.ProductCode.Trim() + " - " + item.ProductDesp.Trim()))
                {
                    CrosstabData[item.ProductCode.Trim() + " - " + item.ProductDesp.Trim()] = new Dictionary<string, int>();
                }

                //string TruncatedLocationName = item.ReqLocationName.Trim().Length > 4  ? item.ReqLocationName.Trim().Substring(0, 4) : item.ReqLocationName.Trim();


                if (!CrosstabData[item.ProductCode.Trim() + " - " + item.ProductDesp.Trim()].ContainsKey(item.ReqLocationCode.Trim() + " - " + item.ReqLocationName.Trim()))
                {
                    CrosstabData[item.ProductCode.Trim() + " - " + item.ProductDesp.Trim()][item.ReqLocationCode.Trim() + " - " + item.ReqLocationName.Trim()] = 0;
                }

                CrosstabData[item.ProductCode.Trim() + " - " + item.ProductDesp.Trim()][item.ReqLocationCode.Trim() + " - " + item.ReqLocationName.Trim()] += int.Parse(item.RequestedQty.ToString());

                
                //if (item.Remark != null)
                //{
                //    Remark += " Outlet " + item.ReqLocationCode.Trim() + " - " + item.ReqLocationName.Trim() + " " + item.Remark.Trim();
                //}

                //if (item.RequestNoteNo != null)
                //{
                //    RequestNoteNos += " Outlet " + item.ReqLocationCode.Trim() + " - " + item.ReqLocationName.Trim() + " " + item.RequestNoteNo.Trim();
                //}

            }


            var DisReqNotes = POTransaction.Select(x => x.RequestNoteNo).Distinct().ToList();
            foreach (var note in DisReqNotes)
            {
                if (RequestNoteNos == "")
                {
                    RequestNoteNos +=  note.ToString();
                }
                else
                {
                    RequestNoteNos += " , " + note.ToString();
                }

            }

            var RequestNoteRemark = POTransaction
                                    .GroupBy(x => x.RequestNoteNo)
                                    .Select(g => new
                                    {
                                        ReqLocationCode = g.FirstOrDefault()?.ReqLocationCode,
                                        ReqLocationName = g.FirstOrDefault()?.ReqLocationName,
                                        Remark = g.FirstOrDefault()?.Remark 
                                    })
                                    .ToList();

            foreach (var remark in RequestNoteRemark)
            {
                if (remark.Remark != null)
                {
                    Remarks += " Outlet " + remark.ReqLocationCode.Trim() + " - " + remark.ReqLocationName.Trim() + " " + remark.Remark.Trim() + " / ";
                }

            }




            ViewBag.CrosstabData = CrosstabData;
            //ViewBag.Outlets = Locations;
              //  ViewBag.Products = Products;


            Head.Company = _bllcompany.GetCompanyById(poheader.CompanyID).CompanyName;
            Head.Location = _blllocation.GetLocationById(poheader.POLocationId).LocationName;
            Head.SysCompany = _bllcompany.GetCompanyById(poheader.CompanyID);
            Head.Supplier = _bllsupplier.GetSupplierById(poheader.SupplierId);
            Head.PrintStatus = printstatus;
            Head.DocState = _blldocStatus.GetDocStatusById(poheader.DocumentStatus).Description;
            var paymantterm = _bllpaymentterm.GetPaymentTermById(poheader.PaymentTermId);
            if (paymantterm != null)
                Head.PaymentTerm = paymantterm.PaymentTermName;
            else Head.PaymentTerm = "N/A";
            var polocation = _blllocation.GetLocationById(poheader.POLocationId);
            Head.POLocationAddress = polocation.Address1 + "," + polocation.Address2 + "," + polocation.Address3 + ".";


            Head.DocumentNo = poheader.DocumentNo;
            Head.PODate = poheader.PODate;
            Head.ExpectedDate = poheader.ExpectedDate;

            List<InvRequestNotePOTransaction> _list = _bllgrn.GetInvRequestNotePOTransactionbyID(id);

            if (_list != null && _list.Count > 0)
            {
                RequestNoteHeader RH = _bllrequestnote.GetRequestNoteById(_list[0].RequestNoteHeaderID);
                if (RH != null)
                {
                    Head.ExpectedDate = RH.ExpectedDeleveryDate;

                }
            }

            Head.CreatedUser = poheader.CreatedUser;
            Head.Remark = Remarks;
            Head.RequestNoteNos = RequestNoteNos;


            return PartialView("~/Views/Receipts/MergedPOViewKitchen.cshtml", Head);
        }





        //  [Authorize(Roles = "POEdit")] Manual
        [HttpPost]
        public ActionResult MultipleSavePO(PurchaseOrderHeader header)
        {

            // check permissions
            if (!_appmanager.SetPermissions(3, Session["loggeduserempcode"].ToString(), "POEdit"))
            {
                @ViewBag.Permissions = "No user permissions to Edit POs";
                return View("~/Views/Account/AccessDenied.cshtml");
            }
            //end check permissions

            Session["POWizadUI"] = _bllconfiguration.GetConfiguration("UIWZ", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var deptdata = _bllproduct.GetActiveProducts(companyid);
            ViewBag.IsAllItems = _bllconfiguration.GetConfiguration("DITEMS", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            ViewBag.productdata = deptdata;


            


            //ViewBag.DocNo = header.DocumentNo;
            //ViewBag.Manual = header.POType;
            //ViewBag.SupplierId = header.SupplierId;
            //ViewBag.DeliveryLocationId = header.DeliveryLocationId;
            //ViewBag.PaymentTermId = header.PaymentTermId;
            //ViewBag.CurrencyId = header.CurrencyId;
            //ViewBag.poLocationId = header.POLocationId;
            //ViewBag.PaymentMethodId = header.PaymentMethodId;

            //var id = RouteData.Values["id"] + Request.Url.Query;
            var po = _bllpurchaseorder.GetPOById(header.PurchaseOrderHeaderId);
            var poheader = header;
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            if (!ModelState.IsValid || poheader.PODetail.Count == 0)
            {
                if (poheader.PODetail.Count == 0)
                {
                    @ViewBag.Message = "4";
                }

                //ViewBag.CurrencyId = poheader.CurrencyId;
                //ViewBag.SupplierId = poheader.SupplierId;
                //ViewBag.DeliveryLocationId = poheader.DeliveryLocationId;
                //ViewBag.poLocationId = poheader.POLocationId;
                //ViewBag.PaymentTermId = poheader.PaymentTermId;
                //ViewBag.PaymentMethodId = poheader.PaymentMethodId;

                ViewBag.DocNo = header.DocumentNo;
                ViewBag.Manual = header.POType;
                ViewBag.SupplierId = header.SupplierId;
                ViewBag.DeliveryLocationId = header.DeliveryLocationId;
                ViewBag.PaymentTermId = header.PaymentTermId;
                ViewBag.CurrencyId = header.CurrencyId;
                ViewBag.poLocationId = header.POLocationId;
                ViewBag.PaymentMethodId = header.PaymentMethodId;
                ViewBag.EventId = header.EventId;

                poheader.Company = _bllcompany.GetCompanyById(poheader.CompanyID).CompanyName;
                poheader.Location = _blllocation.GetLocationById(poheader.POLocationId).LocationName;
                poheader.PODetail.ToList().ForEach(d =>
                {
                    var prd = _bllproduct.GetProductById(d.ProductId);
                    d.ProductName = prd.ProductName;
                    d.ProductCode = prd.ProductCode;
                    d.UOM = d.OrderQty + _bllproduct.GetUOMById(_bllproduct.GetProductById(d.ProductId).PurchasingUnit);
                });

                return View("~/Views/Transactions/PO/CreatePO.cshtml", poheader);
            }


            bool EnableMonthEndProcess = _bllconfiguration.GetConfiguration("EnableMonthEndProcess", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;

            if (EnableMonthEndProcess)
            {
                int monthendstatus = _bllmonthend.CheckMonthEnd(Convert.ToInt32(poheader.POLocationId), poheader.PODate.Year, poheader.PODate.Month);
                if (monthendstatus != 1)
                {
                    ModelState.Clear();

                    ViewBag.CurrencyId = poheader.CurrencyId;
                    ViewBag.SupplierId = poheader.SupplierId;
                    ViewBag.DeliveryLocationId = poheader.DeliveryLocationId;
                    ViewBag.poLocationId = poheader.POLocationId;
                    ViewBag.PaymentTermId = poheader.PaymentTermId;
                    ViewBag.PaymentMethodId = poheader.PaymentMethodId;
                    ViewBag.EventId = poheader.EventId;
                    ViewBag.poLocationId = Convert.ToInt16(Session["loggeduserlocId"]);

                    poheader.PODetail.ToList().ForEach(d =>
                    {
                        var prd = _bllproduct.GetProductById(d.ProductId);
                        d.ProductName = prd.ProductName;
                        d.ProductCode = prd.ProductCode;
                        d.UOM = d.OrderQty + _bllproduct.GetUOMById(_bllproduct.GetProductById(d.ProductId).PurchasingUnit);
                    });

                    ViewBag.Message = "5";
                    return View("~/Views/Transactions/PO/EditPO.cshtml", poheader);
                }
            }

            //if (po.IsTempPO == true)
            //{
            //    po.DocumentNo = commonService.GetDocumentNo("Purchase Order", 1, "1", 2, true);
            //}
            po.IsTempPO = true;
            po.DocumentNo = poheader.DocumentNo;
            po.PODetail = poheader.PODetail;
            po.POLocationId = poheader.POLocationId;
            po.DeliveryLocationId = poheader.DeliveryLocationId;
            po.POType = poheader.POType;
            po.SupplierId = poheader.SupplierId;
            po.PaymentExpectedDate = poheader.PaymentExpectedDate;
            po.PaymentTermId = poheader.PaymentTermId;
            po.PaymentMethodId = poheader.PaymentMethodId;
            po.CurrencyId = poheader.CurrencyId;
            po.ExpectedDate = poheader.ExpectedDate;
            po.ValidityPeriod = poheader.PaymentTermId;
            po.ValidityPeriod = poheader.ValidityPeriod;
            po.TotSellingPrice = poheader.TotSellingPrice;
            po.TotCostPrice = poheader.TotCostPrice;
            po.TotDiscounts = poheader.TotDiscounts;
            po.GrossAmount = poheader.GrossAmount;
            po.TotalTaxAmount = poheader.TotalTaxAmount;
            po.OtherCharges = poheader.OtherCharges;
            po.Addition = poheader.Addition;
            po.Deduction = poheader.Deduction;
            po.NetAmount = poheader.NetAmount;
            po.DeliveryDetail = poheader.DeliveryDetail;
            po.ReferenceNo = poheader.ReferenceNo;
            po.Remark = poheader.Remark;
            po.RejectReason = poheader.RejectReason;
            po.CancelReason = poheader.CancelReason;
            po.EventId = poheader.EventId;

            po.CreatedUser = poheader.CreatedUser;
            po.CreatedDate = poheader.CreatedDate;
            po.ModifiedDate = DateTime.Now;
            po.ModifiedUser = Session["loggeduser"].ToString();
            po.DataTransfer = 0;
            po.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
            po.GroupOfCompanyID = 1;
          //  po.CompanyID = 1;
            po.DocumentStatus = 1;
            po.RequestedBy = Session["loggeduser"].ToString();
            po.PaymentExpectedDate = DateTime.Now;
            po.DeliveryAddress = poheader.DeliveryAddress;
            po.PODate = poheader.PODate;
            po.CompanyID =Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var res = _bllpurchaseorder.EditPo(po);



            
                ViewBag.IsMergedPO = false;
            



            ViewBag.POCode = po.DocumentNo;
            if (res)
            {


                ViewBag.DocNo = header.DocumentNo;
                ViewBag.Manual = header.POType;
                ViewBag.SupplierId = header.SupplierId;
                ViewBag.DeliveryLocationId = header.DeliveryLocationId;
                ViewBag.PaymentTermId = header.PaymentTermId;
                ViewBag.CurrencyId = header.CurrencyId;
                ViewBag.poLocationId = header.POLocationId;
                ViewBag.PaymentMethodId = header.PaymentMethodId;
                ViewBag.EventId = header.EventId;
                po.Company = _bllcompany.GetCompanyById(poheader.CompanyID).CompanyName;
                po.Location = _blllocation.GetLocationById(poheader.POLocationId).LocationName;
                po.PODetail.ToList().ForEach(d =>
                {
                    var prd = _bllproduct.GetProductById(d.ProductId);
                    d.ProductName = prd.ProductName;
                    d.ProductCode = prd.ProductCode;
                    d.UOM = d.OrderQty + _bllproduct.GetUOMById(_bllproduct.GetProductById(d.ProductId).PurchasingUnit);
                });

                ViewBag.POHeaderId = po.PurchaseOrderHeaderId;
                @ViewBag.Message = "1";
                //PurchaseOrderHeader _poheader = new PurchaseOrderHeader();
                //_poheader.PODetail = new List<PurchaseOrderDetail>();
                //_poheader.PurchaseOrderHeaderId = poheader.PurchaseOrderHeaderId;
                //_poheader.PODate = DateTime.Now;
                //_poheader.DeliveryAddress = string.Empty;
                //_poheader.ExpectedDate = DateTime.Now;
                return View("~/Views/Transactions/PO/EditPO.cshtml", po);

            }
            else
            {
                ViewBag.DocNo = header.DocumentNo;
                ViewBag.Manual = header.POType;
                ViewBag.SupplierId = header.SupplierId;
                ViewBag.DeliveryLocationId = header.DeliveryLocationId;
                ViewBag.PaymentTermId = header.PaymentTermId;
                ViewBag.CurrencyId = header.CurrencyId;
                ViewBag.poLocationId = header.POLocationId;
                ViewBag.PaymentMethodId = header.PaymentMethodId;
                ViewBag.EventId = header.EventId;
                poheader.Company = _bllcompany.GetCompanyById(poheader.CompanyID).CompanyName;
                poheader.Location = _blllocation.GetLocationById(poheader.POLocationId).LocationName;
                poheader.PODetail.ToList().ForEach(d =>
                {
                    var prd = _bllproduct.GetProductById(d.ProductId);
                    d.ProductName = prd.ProductName;
                    d.ProductCode = prd.ProductCode;
                    d.UOM = d.OrderQty + _bllproduct.GetUOMById(_bllproduct.GetProductById(d.ProductId).PurchasingUnit);
                });
                ViewBag.Message = "2";
                return View("~/Views/Transactions/PO/EditPO.cshtml", po);
            }


        }


       // [Authorize(Roles = "POEdit")]    
        [HttpPost]
        public ActionResult Edit(PurchaseOrderHeader poheader)
        {


            // check permissions
            if (!_appmanager.SetPermissions(3, Session["loggeduserempcode"].ToString(), "POEdit"))
            {
                @ViewBag.Permissions = "No user permissions to Edit POs";
                return View("~/Views/Account/AccessDenied.cshtml");
            }
            //end check permissions

            Session["POWizadUI"] = _bllconfiguration.GetConfiguration("UIWZ", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var deptdata = _bllproduct.GetActiveProducts(companyid);
            ViewBag.IsAllItems = _bllconfiguration.GetConfiguration("DITEMS", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            ViewBag.productdata = deptdata;

            //  var id = RouteData.Values["id"] + Request.Url.Query;
            var id = poheader.PurchaseOrderHeaderId;
            var po = _bllpurchaseorder.GetPOById(Convert.ToInt64(id));


            if (!ModelState.IsValid || poheader.PODetail.Count==0)
            {
                if (poheader.PODetail.Count == 0)
                {
                    @ViewBag.Message = "4";
                }

                ViewBag.CurrencyId = poheader.CurrencyId;
                ViewBag.SupplierId = poheader.SupplierId;
                ViewBag.DeliveryLocationId = poheader.DeliveryLocationId;
                ViewBag.poLocationId = poheader.POLocationId;
                ViewBag.PaymentTermId = poheader.PaymentTermId;
                ViewBag.PaymentMethodId = poheader.PaymentMethodId;
                ViewBag.EventId = poheader.EventId;
                poheader.PODetail.ToList().ForEach(d =>
                {
                    var prd = _bllproduct.GetProductById(d.ProductId);
                    d.ProductName = prd.ProductName;
                    d.ProductCode = prd.ProductCode;
                    d.UOM = d.OrderQty + _bllproduct.GetUOMById(_bllproduct.GetProductById(d.ProductId).PurchasingUnit);
                });
                return View("~/Views/Transactions/PO/EditPO.cshtml", poheader);
            }

            bool EnableMonthEndProcess = _bllconfiguration.GetConfiguration("EnableMonthEndProcess", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;

            if (EnableMonthEndProcess)
            {
                int monthendstatus = _bllmonthend.CheckMonthEnd(Convert.ToInt32(poheader.POLocationId), poheader.PODate.Year, poheader.PODate.Month);
                if (monthendstatus != 1)
                {
                    ModelState.Clear();

                    ViewBag.CurrencyId = poheader.CurrencyId;
                    ViewBag.SupplierId = poheader.SupplierId;
                    ViewBag.DeliveryLocationId = poheader.DeliveryLocationId;
                    ViewBag.poLocationId = poheader.POLocationId;
                    ViewBag.PaymentTermId = poheader.PaymentTermId;
                    ViewBag.PaymentMethodId = poheader.PaymentMethodId;
                    ViewBag.EventId = poheader.EventId;
                    ViewBag.poLocationId = Convert.ToInt16(Session["loggeduserlocId"]);

                    poheader.PODetail.ToList().ForEach(d =>
                    {
                        var prd = _bllproduct.GetProductById(d.ProductId);
                        d.ProductName = prd.ProductName;
                        d.ProductCode = prd.ProductCode;
                        d.UOM = d.OrderQty + _bllproduct.GetUOMById(_bllproduct.GetProductById(d.ProductId).PurchasingUnit);
                    });

                    ViewBag.Message = "5";
                    return View("~/Views/Transactions/PO/EditPO.cshtml", poheader);
                }
            }


            if (po.IsTempPO == true)
            {
               
                int docid = _bllcommon.GetDcumentId("Purchase Order", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                var docnum = _bllcommon.GetDocumentNo("Purchase Order",
                                                        Convert.ToInt32(Session["loggeduserlocId"]),
                                                        Convert.ToString(poheader.POLocationId),
                                                         docid,
                                                         false, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));

                po.DocumentNo = docnum;
                po.DocumentId = docid;
                po.IsTempPO = false;
            }
            po.PODetail = poheader.PODetail;
            po.POLocationId = poheader.POLocationId;
            po.DeliveryLocationId = poheader.DeliveryLocationId;
            po.POType = poheader.POType;
            po.SupplierId = poheader.SupplierId;
            po.PaymentExpectedDate = poheader.PaymentExpectedDate;
            po.PaymentTermId = poheader.PaymentTermId;
            po.PaymentMethodId = poheader.PaymentMethodId;
            po.CurrencyId = poheader.CurrencyId;
            po.ExpectedDate = poheader.ExpectedDate;
            po.ValidityPeriod = poheader.PaymentTermId;
            po.ValidityPeriod = poheader.ValidityPeriod;
            po.TotSellingPrice = poheader.TotSellingPrice;
            po.TotCostPrice = poheader.TotCostPrice;
            po.TotDiscounts = poheader.TotDiscounts;
            po.GrossAmount = poheader.GrossAmount;
            po.TotalTaxAmount = poheader.TotalTaxAmount;
            po.OtherCharges = poheader.OtherCharges;
            po.Addition = poheader.Addition;
            po.Deduction = poheader.Deduction;
            po.NetAmount = poheader.NetAmount;
            po.DeliveryDetail = poheader.DeliveryDetail;
            po.ReferenceNo = poheader.ReferenceNo;
            po.Remark = poheader.Remark;
            po.EventId = poheader.EventId;
            po.RejectReason = poheader.RejectReason;
            po.CancelReason = poheader.CancelReason;

            po.CreatedUser = poheader.CreatedUser;
            po.CreatedDate = poheader.CreatedDate;
            po.ModifiedDate = DateTime.Now;
            po.ModifiedUser = Session["loggeduser"].ToString();
            po.DataTransfer = 0;
            po.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
            po.GroupOfCompanyID = 1;
           // po.CompanyID = 1;



            if (Convert.ToBoolean(Session["POApprovels"]) == true)
            {
                po.DocumentStatus = 2;
            }
            else
            {
                po.DocumentStatus = 3;
            }

               

            po.RequestedBy = Session["loggeduser"].ToString();
            po.PaymentExpectedDate = DateTime.Now;

            var res = _bllpurchaseorder.EditPo(po);

            ViewBag.POCode = po.DocumentNo;
            if (res)
            {
                
                //poheader.Company = _bllcompany.GetCompanyById(poheader.CompanyID).CompanyName;
                //poheader.Location = _blllocation.GetLocationById(poheader.POLocationId).LocationName;
                //poheader.PODetail.ToList().ForEach(d => {
                //    var prd = _bllproduct.GetProductById(d.ProductId);
                //    d.ProductName = prd.ProductName;
                //    d.ProductCode = prd.ProductCode;
                //});
                @ViewBag.POHeaderId = po.PurchaseOrderHeaderId;
                @ViewBag.Message = "1";
                PurchaseOrderHeader _poheader = new PurchaseOrderHeader();
                _poheader.PODetail = new List<PurchaseOrderDetail>();
                _poheader.PurchaseOrderHeaderId = poheader.PurchaseOrderHeaderId;
                _poheader.PODate = DateTime.Now;
                _poheader.DeliveryAddress = string.Empty;
                _poheader.ExpectedDate = DateTime.Now;
                return View("~/Views/Transactions/PO/EditPO.cshtml", _poheader);

            }
            else
            {
                ViewBag.CurrencyId = poheader.CurrencyId;
                ViewBag.SupplierId = poheader.SupplierId;
                ViewBag.DeliveryLocationId = poheader.DeliveryLocationId;
                ViewBag.poLocationId = poheader.POLocationId;
                ViewBag.PaymentTermId = poheader.PaymentTermId;
                ViewBag.PaymentMethodId = poheader.PaymentMethodId;
                ViewBag.EventId = poheader.EventId;
                po.Company = _bllcompany.GetCompanyById(poheader.CompanyID).CompanyName;
                po.Location = _blllocation.GetLocationById(poheader.POLocationId).LocationName;
                po.PODetail.ToList().ForEach(d =>
                {
                    var prd = _bllproduct.GetProductById(d.ProductId);
                    d.ProductName = prd.ProductName;
                    d.ProductCode = prd.ProductCode;
                    d.UOM = d.OrderQty + _bllproduct.GetUOMById(_bllproduct.GetProductById(d.ProductId).PurchasingUnit);
                });

                ViewBag.Message = "2";
                return View("~/Views/Transactions/PO/EditPO.cshtml", po);
            }
        }


        // [Authorize(Roles = "POApprove")]
      //  [Authorize(Roles = "POEdit")]
        [HttpPost]
        public ActionResult Approve(PurchaseOrderHeader poheader)
        {

            // check permissions
            if (!_appmanager.SetPermissions(3, Session["loggeduserempcode"].ToString(), "POApprove"))
            {
                @ViewBag.Permissions = "No user permissions to Approve POs";
                return View("~/Views/Account/AccessDenied.cshtml");
            }
            //end check permissions
            Session["POWizadUI"] = _bllconfiguration.GetConfiguration("UIWZ", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var deptdata = _bllproduct.GetActiveProducts(companyid);
            ViewBag.IsAllItems = _bllconfiguration.GetConfiguration("DITEMS", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            ViewBag.productdata = deptdata;

            var id = RouteData.Values["id"] + Request.Url.Query;
            var po = _bllpurchaseorder.GetPOById(poheader.PurchaseOrderHeaderId);

            var errors = ModelState.Values.SelectMany(v => v.Errors);

            List<PurchaseOrderDetail> Qtys = _bllpurchaseorder.GetPOIdByQtys(poheader.PurchaseOrderHeaderId);

            // Make sure both lists have the same count
            if (poheader.PODetail.Count != Qtys.Count)
            {
                // Handle the case where counts do not match
                // You may throw an exception, log a warning, or handle it according to your requirements
            }

            for (int i = 0; i < poheader.PODetail.Count; i++)
            {
                // Match each item from poheader.PODetail with corresponding item from Qtys
                //poheader.PODetail[i].OrderQty = Qtys[i].OrderQty;
            }

            if (!ModelState.IsValid || poheader.PODetail.Count == 0)
            {
                if (poheader.PODetail.Count == 0)
                {
                    @ViewBag.Message = "4";
                }

                ViewBag.CurrencyId = poheader.CurrencyId;
                ViewBag.SupplierId = poheader.SupplierId;
                ViewBag.DeliveryLocationId = poheader.DeliveryLocationId;
                ViewBag.poLocationId = poheader.POLocationId;
                ViewBag.PaymentTermId = poheader.PaymentTermId;
                ViewBag.PaymentMethodId = poheader.PaymentMethodId;
                ViewBag.EventId = poheader.EventId;
                poheader.PODetail.ToList().ForEach(d =>
                {
                    var prd = _bllproduct.GetProductById(d.ProductId);
                    d.ProductName = prd.ProductName;
                    d.ProductCode = prd.ProductCode;
                    d.UOM = d.OrderQty + _bllproduct.GetUOMById(_bllproduct.GetProductById(d.ProductId).PurchasingUnit);
                });
                
                return View("~/Views/Transactions/PO/EditPO.cshtml", poheader);
            }

            if (po.IsTempPO == true)
            {

                int docid = _bllcommon.GetDcumentId("Purchase Order", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                var docnum = _bllcommon.GetDocumentNo("Purchase Order",
                                                        Convert.ToInt32(Session["loggeduserlocId"]),
                                                        Convert.ToString(poheader.POLocationId),
                                                         docid,
                                                         false, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));

                po.DocumentNo = docnum;
                po.DocumentId = docid;
                po.IsTempPO = false;
            }
            po.PODetail = poheader.PODetail;
            po.POLocationId = poheader.POLocationId;
            po.DeliveryLocationId = poheader.DeliveryLocationId;
            po.POType = po.POType;
            po.SupplierId = poheader.SupplierId;
            po.PaymentExpectedDate = poheader.PaymentExpectedDate;
            po.PaymentTermId = poheader.PaymentTermId;
            po.PaymentMethodId = poheader.PaymentMethodId;
            po.CurrencyId = poheader.CurrencyId;
            po.ExpectedDate = poheader.ExpectedDate;
            po.ValidityPeriod = poheader.PaymentTermId;
            po.ValidityPeriod = poheader.ValidityPeriod;
            po.TotSellingPrice = poheader.TotSellingPrice;
            po.TotCostPrice = poheader.TotCostPrice;
            po.TotDiscounts = poheader.TotDiscounts;
            po.GrossAmount = poheader.GrossAmount;
            po.TotalTaxAmount = poheader.TotalTaxAmount;
            po.OtherCharges = poheader.OtherCharges;
            po.Addition = poheader.Addition;
            po.Deduction = poheader.Deduction;
            po.NetAmount = poheader.NetAmount;
            po.DeliveryDetail = poheader.DeliveryDetail;
            po.ReferenceNo = poheader.ReferenceNo;
            po.Remark = poheader.Remark;
            po.EventId = poheader.EventId;
            po.RejectReason = poheader.RejectReason;
            po.CancelReason = poheader.CancelReason;

            po.CreatedUser = poheader.CreatedUser;
            po.CreatedDate = poheader.CreatedDate;
            po.ModifiedDate = DateTime.Now;
            po.ModifiedUser = Session["loggeduser"].ToString();
            po.DataTransfer = 0;
            po.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
            po.GroupOfCompanyID = 1;
          //  po.CompanyID = 1;
            po.DocumentStatus = 3;
            po.RequestedBy = Session["loggeduser"].ToString();
            po.PaymentExpectedDate = DateTime.Now;

            var res = _bllpurchaseorder.EditPo(po);

            ViewBag.POCode = po.DocumentNo;
            if (res)
            {
          
                @ViewBag.POHeaderId = po.PurchaseOrderHeaderId;
                @ViewBag.Message = "1";
                PurchaseOrderHeader _poheader = new PurchaseOrderHeader();
                _poheader.PODetail = new List<PurchaseOrderDetail>();
                _poheader.PurchaseOrderHeaderId = poheader.PurchaseOrderHeaderId;
                _poheader.PODate = DateTime.Now;
                _poheader.DeliveryAddress = string.Empty;
                _poheader.ExpectedDate = DateTime.Now;
                return View("~/Views/Transactions/PO/EditPO.cshtml", _poheader);

            }
            else
            {
                ViewBag.CurrencyId = poheader.CurrencyId;
                ViewBag.SupplierId = poheader.SupplierId;
                ViewBag.DeliveryLocationId = poheader.DeliveryLocationId;
                ViewBag.poLocationId = poheader.POLocationId;
                ViewBag.PaymentTermId = poheader.PaymentTermId;
                ViewBag.PaymentMethodId = poheader.PaymentMethodId;
                ViewBag.EventId = poheader.EventId;
                po.Company = _bllcompany.GetCompanyById(poheader.CompanyID).CompanyName;
                po.Location = _blllocation.GetLocationById(poheader.POLocationId).LocationName;
                po.PODetail.ToList().ForEach(d =>
                {
                    var prd = _bllproduct.GetProductById(d.ProductId);
                    d.ProductName = prd.ProductName;
                    d.ProductCode = prd.ProductCode;
                    d.UOM = d.OrderQty + _bllproduct.GetUOMById(_bllproduct.GetProductById(d.ProductId).PurchasingUnit);
                });

                ViewBag.Message = "2";
                return View("~/Views/Transactions/PO/EditPO.cshtml", po);
            }
        }

        //  [Authorize(Roles = "POReject")]
       // [Authorize(Roles = "POEdit")]
        [HttpPost]
        public ActionResult Reject(PurchaseOrderHeader poheader)
        {

            // check permissions
            if (!_appmanager.SetPermissions(3, Session["loggeduserempcode"].ToString(), "POReject"))
            {
                @ViewBag.Permissions = "No user permissions to Reject POs";
                return View("~/Views/Account/AccessDenied.cshtml");
            }
            //end check permissions
            Session["POWizadUI"] = _bllconfiguration.GetConfiguration("UIWZ", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var deptdata = _bllproduct.GetActiveProducts(companyid);
            ViewBag.IsAllItems = _bllconfiguration.GetConfiguration("DITEMS", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            ViewBag.productdata = deptdata;

            var id = RouteData.Values["id"] + Request.Url.Query;
            var po = _bllpurchaseorder.GetPOById(poheader.PurchaseOrderHeaderId);

            var errors = ModelState.Values.SelectMany(v => v.Errors);
            if (!ModelState.IsValid || poheader.PODetail.Count == 0)
            {
                if (poheader.PODetail.Count == 0)
                {
                    @ViewBag.Message = "4";
                }

                ViewBag.CurrencyId = poheader.CurrencyId;
                ViewBag.SupplierId = poheader.SupplierId;
                ViewBag.DeliveryLocationId = poheader.DeliveryLocationId;
                ViewBag.poLocationId = poheader.POLocationId;
                ViewBag.PaymentTermId = poheader.PaymentTermId;
                ViewBag.PaymentMethodId = poheader.PaymentMethodId;
                ViewBag.EventId = poheader.EventId;
                poheader.PODetail.ToList().ForEach(d =>
                {
                    var prd = _bllproduct.GetProductById(d.ProductId);
                    d.ProductName = prd.ProductName;
                    d.ProductCode = prd.ProductCode;
                    d.UOM = d.OrderQty + _bllproduct.GetUOMById(_bllproduct.GetProductById(d.ProductId).PurchasingUnit);
                });

                return View("~/Views/Transactions/PO/EditPO.cshtml", poheader);
            }

            if (string.IsNullOrEmpty(poheader.RejectReason))
            {
                @ViewBag.Message = "5";

                ViewBag.CurrencyId = poheader.CurrencyId;
                ViewBag.SupplierId = poheader.SupplierId;
                ViewBag.DeliveryLocationId = poheader.DeliveryLocationId;
                ViewBag.poLocationId = poheader.POLocationId;
                ViewBag.PaymentTermId = poheader.PaymentTermId;
                ViewBag.PaymentMethodId = poheader.PaymentMethodId;
                ViewBag.EventId = poheader.EventId;
                poheader.PODetail.ToList().ForEach(d =>
                {
                    var prd = _bllproduct.GetProductById(d.ProductId);
                    d.ProductName = prd.ProductName;
                    d.ProductCode = prd.ProductCode;
                    d.UOM = d.OrderQty + _bllproduct.GetUOMById(_bllproduct.GetProductById(d.ProductId).PurchasingUnit);
                });

                return View("~/Views/Transactions/PO/EditPO.cshtml", poheader);
            }

                if (po.IsTempPO == true)
            {

                int docid = _bllcommon.GetDcumentId("Purchase Order", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                var docnum = _bllcommon.GetDocumentNo("Purchase Order",
                                                        Convert.ToInt32(Session["loggeduserlocId"]),
                                                         Convert.ToString(poheader.POLocationId),
                                                         docid,
                                                         false, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));

                po.DocumentNo = docnum;
                po.DocumentId = docid;
                po.IsTempPO = false;
            }
            po.PODetail = poheader.PODetail;
            po.POLocationId = poheader.POLocationId;
            po.DeliveryLocationId = poheader.DeliveryLocationId;
            po.POType = poheader.POType;
            po.SupplierId = poheader.SupplierId;
            po.PaymentExpectedDate = poheader.PaymentExpectedDate;
            po.PaymentTermId = poheader.PaymentTermId;
            po.PaymentMethodId = poheader.PaymentMethodId;
            po.CurrencyId = poheader.CurrencyId;
            po.ExpectedDate = poheader.ExpectedDate;
            po.ValidityPeriod = poheader.PaymentTermId;
            po.ValidityPeriod = poheader.ValidityPeriod;
            po.TotSellingPrice = poheader.TotSellingPrice;
            po.TotCostPrice = poheader.TotCostPrice;
            po.TotDiscounts = poheader.TotDiscounts;
            po.GrossAmount = poheader.GrossAmount;
            po.TotalTaxAmount = poheader.TotalTaxAmount;
            po.OtherCharges = poheader.OtherCharges;
            po.Addition = poheader.Addition;
            po.Deduction = poheader.Deduction;
            po.NetAmount = poheader.NetAmount;
            po.DeliveryDetail = poheader.DeliveryDetail;
            po.ReferenceNo = poheader.ReferenceNo;
            po.Remark = poheader.Remark;
            po.EventId = poheader.EventId;
            po.RejectReason = poheader.RejectReason;
            po.CancelReason = poheader.CancelReason;

            po.CreatedUser = poheader.CreatedUser;
            po.CreatedDate = poheader.CreatedDate;
            po.ModifiedDate = DateTime.Now;
            po.ModifiedUser = Session["loggeduser"].ToString();
            po.DataTransfer = 0;
            po.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
            po.GroupOfCompanyID = 1;
          //  po.CompanyID = 1;
            po.DocumentStatus = 4;
            po.RequestedBy = Session["loggeduser"].ToString();
            po.PaymentExpectedDate = DateTime.Now;

            var res = _bllpurchaseorder.EditPo(po);

            ViewBag.POCode = po.DocumentNo;
            if (res)
            {

                @ViewBag.POHeaderId = po.PurchaseOrderHeaderId;
                @ViewBag.Message = "1";
                PurchaseOrderHeader _poheader = new PurchaseOrderHeader();
                _poheader.PODetail = new List<PurchaseOrderDetail>();
                _poheader.PurchaseOrderHeaderId = poheader.PurchaseOrderHeaderId;
                _poheader.PODate = DateTime.Now;
                _poheader.DeliveryAddress = string.Empty;
                _poheader.ExpectedDate = DateTime.Now;
                return View("~/Views/Transactions/PO/EditPO.cshtml", _poheader);

            }
            else
            {
                ViewBag.CurrencyId = poheader.CurrencyId;
                ViewBag.SupplierId = poheader.SupplierId;
                ViewBag.DeliveryLocationId = poheader.DeliveryLocationId;
                ViewBag.poLocationId = poheader.POLocationId;
                ViewBag.PaymentTermId = poheader.PaymentTermId;
                ViewBag.PaymentMethodId = poheader.PaymentMethodId;
                ViewBag.EventId = poheader.EventId;
                po.Company = _bllcompany.GetCompanyById(poheader.CompanyID).CompanyName;
                po.Location = _blllocation.GetLocationById(poheader.POLocationId).LocationName;
                po.PODetail.ToList().ForEach(d =>
                {
                    var prd = _bllproduct.GetProductById(d.ProductId);
                    d.ProductName = prd.ProductName;
                    d.ProductCode = prd.ProductCode;
                    d.UOM = d.OrderQty + _bllproduct.GetUOMById(_bllproduct.GetProductById(d.ProductId).PurchasingUnit);
                });

                ViewBag.Message = "2";
                return View("~/Views/Transactions/PO/EditPO.cshtml", po);
            }
        }

        // [Authorize(Roles = "POCancel")]
       // [Authorize(Roles = "POEdit")]
        [HttpPost]
        public ActionResult Cancel(PurchaseOrderHeader poheader)
        {
            // check permissions
            if (!_appmanager.SetPermissions(3, Session["loggeduserempcode"].ToString(), "POCancel"))
            {
                @ViewBag.Permissions = "No user permissions to Cancel POs";
                return View("~/Views/Account/AccessDenied.cshtml");
            }
            //end check permissions
            Session["POWizadUI"] = _bllconfiguration.GetConfiguration("UIWZ", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var deptdata = _bllproduct.GetActiveProducts(companyid);
            ViewBag.IsAllItems = _bllconfiguration.GetConfiguration("DITEMS", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            ViewBag.productdata = deptdata;

            var id = RouteData.Values["id"] + Request.Url.Query;
            var po = _bllpurchaseorder.GetPOById(poheader.PurchaseOrderHeaderId);

            var errors = ModelState.Values.SelectMany(v => v.Errors);
            if (!ModelState.IsValid || poheader.PODetail.Count == 0)
            {
                if (poheader.PODetail.Count == 0)
                {
                    @ViewBag.Message = "4";
                }

                ViewBag.CurrencyId = poheader.CurrencyId;
                ViewBag.SupplierId = poheader.SupplierId;
                ViewBag.DeliveryLocationId = poheader.DeliveryLocationId;
                ViewBag.poLocationId = poheader.POLocationId;
                ViewBag.PaymentTermId = poheader.PaymentTermId;
                ViewBag.PaymentMethodId = poheader.PaymentMethodId;
                ViewBag.EventId = poheader.EventId;
                poheader.PODetail.ToList().ForEach(d =>
                {
                    var prd = _bllproduct.GetProductById(d.ProductId);
                    d.ProductName = prd.ProductName;
                    d.ProductCode = prd.ProductCode;
                    d.UOM = d.OrderQty + _bllproduct.GetUOMById(_bllproduct.GetProductById(d.ProductId).PurchasingUnit);
                });
                return View("~/Views/Transactions/PO/EditPO.cshtml", poheader);
            }

            if(string.IsNullOrEmpty(poheader.CancelReason))
            {
                @ViewBag.Message = "6";

                ViewBag.CurrencyId = poheader.CurrencyId;
                ViewBag.SupplierId = poheader.SupplierId;
                ViewBag.DeliveryLocationId = poheader.DeliveryLocationId;
                ViewBag.poLocationId = poheader.POLocationId;
                ViewBag.PaymentTermId = poheader.PaymentTermId;
                ViewBag.PaymentMethodId = poheader.PaymentMethodId;
                ViewBag.EventId = poheader.EventId;
                poheader.PODetail.ToList().ForEach(d =>
                {
                    var prd = _bllproduct.GetProductById(d.ProductId);
                    d.ProductName = prd.ProductName;
                    d.ProductCode = prd.ProductCode;
                    d.UOM = d.OrderQty + _bllproduct.GetUOMById(_bllproduct.GetProductById(d.ProductId).PurchasingUnit);
                });
                return View("~/Views/Transactions/PO/EditPO.cshtml", poheader);

            }

            if (po.IsTempPO == true)
            {

                int docid = _bllcommon.GetDcumentId("Purchase Order", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                var docnum = _bllcommon.GetDocumentNo("Purchase Order",
                                                        Convert.ToInt32(Session["loggeduserlocId"]),
                                                         Convert.ToString(poheader.POLocationId),
                                                         docid,
                                                         false, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));

                po.DocumentNo = docnum;
                po.DocumentId = docid;
                po.IsTempPO = false;
            }
            po.PODetail = poheader.PODetail;
            po.POLocationId = poheader.POLocationId;
            po.DeliveryLocationId = poheader.DeliveryLocationId;
            po.POType = poheader.POType;
            po.SupplierId = poheader.SupplierId;
            po.PaymentExpectedDate = poheader.PaymentExpectedDate;
            po.PaymentTermId = poheader.PaymentTermId;
            po.PaymentMethodId = poheader.PaymentMethodId;
            po.CurrencyId = poheader.CurrencyId;
            po.ExpectedDate = poheader.ExpectedDate;
            po.ValidityPeriod = poheader.PaymentTermId;
            po.ValidityPeriod = poheader.ValidityPeriod;
            po.TotSellingPrice = poheader.TotSellingPrice;
            po.TotCostPrice = poheader.TotCostPrice;
            po.TotDiscounts = poheader.TotDiscounts;
            po.GrossAmount = poheader.GrossAmount;
            po.TotalTaxAmount = poheader.TotalTaxAmount;
            po.OtherCharges = poheader.OtherCharges;
            po.Addition = poheader.Addition;
            po.Deduction = poheader.Deduction;
            po.NetAmount = poheader.NetAmount;
            po.DeliveryDetail = poheader.DeliveryDetail;
            po.ReferenceNo = poheader.ReferenceNo;
            po.Remark = poheader.Remark;
            po.EventId = poheader.EventId;
            po.RejectReason = poheader.RejectReason;
            po.CancelReason = poheader.CancelReason;

            po.CreatedUser = poheader.CreatedUser;
            po.CreatedDate = poheader.CreatedDate;
            po.ModifiedDate = DateTime.Now;
            po.ModifiedUser = Session["loggeduser"].ToString();
            po.DataTransfer = 0;
            po.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
            po.GroupOfCompanyID = 1;
           // po.CompanyID = 1;
            po.DocumentStatus = 5;
            po.RequestedBy = Session["loggeduser"].ToString();
            po.PaymentExpectedDate = DateTime.Now;

            var res = _bllpurchaseorder.EditPo(po);

            ViewBag.POCode = po.DocumentNo;
            if (res)
            {

                @ViewBag.POHeaderId = po.PurchaseOrderHeaderId;
                @ViewBag.Message = "1";
                PurchaseOrderHeader _poheader = new PurchaseOrderHeader();
                _poheader.PODetail = new List<PurchaseOrderDetail>();
                _poheader.PurchaseOrderHeaderId = poheader.PurchaseOrderHeaderId;
                _poheader.PODate = DateTime.Now;
                _poheader.DeliveryAddress = string.Empty;
                _poheader.ExpectedDate = DateTime.Now;
                return View("~/Views/Transactions/PO/EditPO.cshtml", _poheader);

            }
            else
            {
                ViewBag.CurrencyId = poheader.CurrencyId;
                ViewBag.SupplierId = poheader.SupplierId;
                ViewBag.DeliveryLocationId = poheader.DeliveryLocationId;
                ViewBag.poLocationId = poheader.POLocationId;
                ViewBag.PaymentTermId = poheader.PaymentTermId;
                ViewBag.PaymentMethodId = poheader.PaymentMethodId;
                ViewBag.EventId = poheader.EventId;
                po.Company = _bllcompany.GetCompanyById(poheader.CompanyID).CompanyName;
                po.Location = _blllocation.GetLocationById(poheader.POLocationId).LocationName;
                po.PODetail.ToList().ForEach(d =>
                {
                    var prd = _bllproduct.GetProductById(d.ProductId);
                    d.ProductName = prd.ProductName;
                    d.ProductCode = prd.ProductCode;
                    d.UOM = d.OrderQty + _bllproduct.GetUOMById(_bllproduct.GetProductById(d.ProductId).PurchasingUnit);
                });

                ViewBag.Message = "2";
                return View("~/Views/Transactions/PO/EditPO.cshtml", po);
            }
        }

        // [Authorize(Roles = "POReOpen")]
       // [Authorize(Roles = "POEdit")]
        [HttpPost]
        public ActionResult ReOpen(PurchaseOrderHeader poheader)
        {

            // check permissions
            if (!_appmanager.SetPermissions(3, Session["loggeduserempcode"].ToString(), "POReOpen"))
            {
                @ViewBag.Permissions = "No user permissions to Re Open POs";
                return View("~/Views/Account/AccessDenied.cshtml");
            }
            //end check permissions
            Session["POWizadUI"] = _bllconfiguration.GetConfiguration("UIWZ", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var deptdata = _bllproduct.GetActiveProducts(companyid);
            ViewBag.IsAllItems = _bllconfiguration.GetConfiguration("DITEMS", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            ViewBag.productdata = deptdata;

            var id = RouteData.Values["id"] + Request.Url.Query;
            var po = _bllpurchaseorder.GetPOById(poheader.PurchaseOrderHeaderId);

            var errors = ModelState.Values.SelectMany(v => v.Errors);
            if (!ModelState.IsValid || poheader.PODetail.Count == 0)
            {
                if (poheader.PODetail.Count == 0)
                {
                    @ViewBag.Message = "4";
                }

                ViewBag.CurrencyId = poheader.CurrencyId;
                ViewBag.SupplierId = poheader.SupplierId;
                ViewBag.DeliveryLocationId = poheader.DeliveryLocationId;
                ViewBag.poLocationId = poheader.POLocationId;
                ViewBag.PaymentTermId = poheader.PaymentTermId;
                ViewBag.PaymentMethodId = poheader.PaymentMethodId;
                ViewBag.EventId = poheader.EventId;
                poheader.PODetail.ToList().ForEach(d =>
                {
                    var prd = _bllproduct.GetProductById(d.ProductId);
                    d.ProductName = prd.ProductName;
                    d.ProductCode = prd.ProductCode;
                    d.UOM = d.OrderQty + _bllproduct.GetUOMById(_bllproduct.GetProductById(d.ProductId).PurchasingUnit);
                });
                return View("~/Views/Transactions/PO/EditPO.cshtml", poheader);
            }

            if (po.IsTempPO == true)
            {

                int docid = _bllcommon.GetDcumentId("Purchase Order", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                var docnum = _bllcommon.GetDocumentNo("Purchase Order",
                                                        Convert.ToInt32(Session["loggeduserlocId"]),
                                                         Convert.ToString(poheader.POLocationId),
                                                         docid,
                                                         false, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));

                po.DocumentNo = docnum;
                po.DocumentId = docid;
                po.IsTempPO = false;
            }
            po.PODetail = poheader.PODetail;
            po.POLocationId = poheader.POLocationId;
            po.DeliveryLocationId = poheader.DeliveryLocationId;
            po.POType = poheader.POType;
            po.SupplierId = poheader.SupplierId;
            po.PaymentExpectedDate = poheader.PaymentExpectedDate;
            po.PaymentTermId = poheader.PaymentTermId;
            po.PaymentMethodId = poheader.PaymentMethodId;
            po.CurrencyId = poheader.CurrencyId;
            po.ExpectedDate = poheader.ExpectedDate;
            po.ValidityPeriod = poheader.PaymentTermId;
            po.ValidityPeriod = poheader.ValidityPeriod;
            po.TotSellingPrice = poheader.TotSellingPrice;
            po.TotCostPrice = poheader.TotCostPrice;
            po.TotDiscounts = poheader.TotDiscounts;
            po.GrossAmount = poheader.GrossAmount;
            po.TotalTaxAmount = poheader.TotalTaxAmount;
            po.OtherCharges = poheader.OtherCharges;
            po.Addition = poheader.Addition;
            po.Deduction = poheader.Deduction;
            po.NetAmount = poheader.NetAmount;
            po.DeliveryDetail = poheader.DeliveryDetail;
            po.ReferenceNo = poheader.ReferenceNo;
            po.Remark = poheader.Remark;
            po.EventId = poheader.EventId;
            po.RejectReason = poheader.RejectReason;
            po.CancelReason = poheader.CancelReason;

            po.CreatedUser = poheader.CreatedUser;
            po.CreatedDate = poheader.CreatedDate;
            po.ModifiedDate = DateTime.Now;
            po.ModifiedUser = Session["loggeduser"].ToString();
            po.DataTransfer = 0;
            po.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
            po.GroupOfCompanyID = 1;
           // po.CompanyID = 1;
            po.DocumentStatus = 6;
            po.RequestedBy = Session["loggeduser"].ToString();
            po.PaymentExpectedDate = DateTime.Now;

            var res = _bllpurchaseorder.EditPo(po);

            ViewBag.POCode = po.DocumentNo;
            if (res)
            {

                @ViewBag.POHeaderId = po.PurchaseOrderHeaderId;
                @ViewBag.Message = "1";
                PurchaseOrderHeader _poheader = new PurchaseOrderHeader();
                _poheader.PODetail = new List<PurchaseOrderDetail>();
                _poheader.PurchaseOrderHeaderId = poheader.PurchaseOrderHeaderId;
                _poheader.PODate = DateTime.Now;
                _poheader.DeliveryAddress = string.Empty;
                _poheader.ExpectedDate = DateTime.Now;
                return View("~/Views/Transactions/PO/EditPO.cshtml", _poheader);

            }
            else
            {
                ViewBag.CurrencyId = poheader.CurrencyId;
                ViewBag.SupplierId = poheader.SupplierId;
                ViewBag.DeliveryLocationId = poheader.DeliveryLocationId;
                ViewBag.poLocationId = poheader.POLocationId;
                ViewBag.PaymentTermId = poheader.PaymentTermId;
                ViewBag.PaymentMethodId = poheader.PaymentMethodId;
                ViewBag.EventId = poheader.EventId;
                po.Company = _bllcompany.GetCompanyById(poheader.CompanyID).CompanyName;
                po.Location = _blllocation.GetLocationById(poheader.POLocationId).LocationName;
                po.PODetail.ToList().ForEach(d =>
                {
                    var prd = _bllproduct.GetProductById(d.ProductId);
                    d.ProductName = prd.ProductName;
                    d.ProductCode = prd.ProductCode;
                    d.UOM = d.OrderQty + _bllproduct.GetUOMById(_bllproduct.GetProductById(d.ProductId).PurchasingUnit);
                });

                ViewBag.Message = "2";
                return View("~/Views/Transactions/PO/EditPO.cshtml", po);
            }
        }

        //[Authorize(Roles = "POForsefullyClose")]
      //  [Authorize(Roles = "POEdit")]
        [HttpPost]
        public ActionResult ForceFullyClose(PurchaseOrderHeader poheader)
        {
            // check permissions
            if (!_appmanager.SetPermissions(3, Session["loggeduserempcode"].ToString(), "POForsefullyClose"))
            {
                @ViewBag.Permissions = "No user permissions to Forcefully Close POs";
                return View("~/Views/Account/AccessDenied.cshtml");
            }
            //end check permissions

            Session["POWizadUI"] = _bllconfiguration.GetConfiguration("UIWZ", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var deptdata = _bllproduct.GetActiveProducts(companyid);
            ViewBag.IsAllItems = _bllconfiguration.GetConfiguration("DITEMS", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            ViewBag.productdata = deptdata;

            var id = RouteData.Values["id"] + Request.Url.Query;
            var po = _bllpurchaseorder.GetPOById(poheader.PurchaseOrderHeaderId);

            var errors = ModelState.Values.SelectMany(v => v.Errors);
            if (!ModelState.IsValid || poheader.PODetail.Count == 0)
            {
                if (poheader.PODetail.Count == 0)
                {
                    @ViewBag.Message = "4";
                }

                ViewBag.CurrencyId = poheader.CurrencyId;
                ViewBag.SupplierId = poheader.SupplierId;
                ViewBag.DeliveryLocationId = poheader.DeliveryLocationId;
                ViewBag.poLocationId = poheader.POLocationId;
                ViewBag.PaymentTermId = poheader.PaymentTermId;
                ViewBag.PaymentMethodId = poheader.PaymentMethodId;
                ViewBag.EventId = poheader.EventId;
                poheader.PODetail.ToList().ForEach(d =>
                {
                    var prd = _bllproduct.GetProductById(d.ProductId);
                    d.ProductName = prd.ProductName;
                    d.ProductCode = prd.ProductCode;
                    d.UOM = d.OrderQty + _bllproduct.GetUOMById(_bllproduct.GetProductById(d.ProductId).PurchasingUnit);
                });
                return View("~/Views/Transactions/PO/EditPO.cshtml", poheader);
            }

            if (po.IsTempPO == true)
            {

                int docid = _bllcommon.GetDcumentId("Purchase Order", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                var docnum = _bllcommon.GetDocumentNo("Purchase Order",
                                                        Convert.ToInt32(Session["loggeduserlocId"]),
                                                         Convert.ToString(poheader.POLocationId),
                                                         docid,
                                                         false, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));

                po.DocumentNo = docnum;
                po.DocumentId = docid;
                po.IsTempPO = false;
            }
            po.PODetail = poheader.PODetail;
            po.POLocationId = poheader.POLocationId;
            po.DeliveryLocationId = poheader.DeliveryLocationId;
            po.POType = poheader.POType;
            po.SupplierId = poheader.SupplierId;
            po.PaymentExpectedDate = poheader.PaymentExpectedDate;
            po.PaymentTermId = poheader.PaymentTermId;
            po.PaymentMethodId = poheader.PaymentMethodId;
            po.CurrencyId = poheader.CurrencyId;
            po.ExpectedDate = poheader.ExpectedDate;
            po.ValidityPeriod = poheader.PaymentTermId;
            po.ValidityPeriod = poheader.ValidityPeriod;
            po.TotSellingPrice = poheader.TotSellingPrice;
            po.TotCostPrice = poheader.TotCostPrice;
            po.TotDiscounts = poheader.TotDiscounts;
            po.GrossAmount = poheader.GrossAmount;
            po.TotalTaxAmount = poheader.TotalTaxAmount;
            po.OtherCharges = poheader.OtherCharges;
            po.Addition = poheader.Addition;
            po.Deduction = poheader.Deduction;
            po.NetAmount = poheader.NetAmount;
            po.DeliveryDetail = poheader.DeliveryDetail;
            po.ReferenceNo = poheader.ReferenceNo;
            po.Remark = poheader.Remark;
            po.EventId = poheader.EventId;
            po.RejectReason = poheader.RejectReason;
            po.CancelReason = poheader.CancelReason;

            po.CreatedUser = poheader.CreatedUser;
            po.CreatedDate = poheader.CreatedDate;
            po.ModifiedDate = DateTime.Now;
            po.ModifiedUser = Session["loggeduser"].ToString();
            po.DataTransfer = 0;
            po.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
            po.GroupOfCompanyID = 1;
          //  po.CompanyID = 1;
            po.DocumentStatus = 7;
            po.RequestedBy = Session["loggeduser"].ToString();
            po.PaymentExpectedDate = DateTime.Now;

            var res = _bllpurchaseorder.EditPo(po);

            ViewBag.POCode = po.DocumentNo;
            if (res)
            {

                @ViewBag.POHeaderId = po.PurchaseOrderHeaderId;
                @ViewBag.Message = "1";
                PurchaseOrderHeader _poheader = new PurchaseOrderHeader();
                _poheader.PODetail = new List<PurchaseOrderDetail>();
                _poheader.PurchaseOrderHeaderId = poheader.PurchaseOrderHeaderId;
                _poheader.PODate = DateTime.Now;
                _poheader.DeliveryAddress = string.Empty;
                _poheader.ExpectedDate = DateTime.Now;
                return View("~/Views/Transactions/PO/EditPO.cshtml", _poheader);

            }
            else
            {
                ViewBag.CurrencyId = poheader.CurrencyId;
                ViewBag.SupplierId = poheader.SupplierId;
                ViewBag.DeliveryLocationId = poheader.DeliveryLocationId;
                ViewBag.poLocationId = poheader.POLocationId;
                ViewBag.PaymentTermId = poheader.PaymentTermId;
                ViewBag.PaymentMethodId = poheader.PaymentMethodId;
                ViewBag.EventId = poheader.EventId;
                po.Company = _bllcompany.GetCompanyById(poheader.CompanyID).CompanyName;
                po.Location = _blllocation.GetLocationById(poheader.POLocationId).LocationName;
                po.PODetail.ToList().ForEach(d =>
                {
                    var prd = _bllproduct.GetProductById(d.ProductId);
                    d.ProductName = prd.ProductName;
                    d.ProductCode = prd.ProductCode;
                    d.UOM = d.OrderQty + _bllproduct.GetUOMById(_bllproduct.GetProductById(d.ProductId).PurchasingUnit);
                });

                ViewBag.Message = "2";
                return View("~/Views/Transactions/PO/EditPO.cshtml", po);
            }
        }

       // [Authorize(Roles = "POCreatee")]
        [HttpPost]
        public ActionResult CreatePO(PurchaseOrderHeader poheader, string sbt)
        {
            // check permissions
            if (!_appmanager.SetPermissions(3, Session["loggeduserempcode"].ToString(), "POCreatee"))
            {
                @ViewBag.Permissions = "No user permissions to Create POs";
                return View("~/Views/Account/AccessDenied.cshtml");
            }

            if(poheader.PODate.Year < 2000)
            {
                poheader.PODate = DateTime.Now;

            }



            //end check permissions
            Session["POWizadUI"] = _bllconfiguration.GetConfiguration("UIWZ", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var deptdata = _bllproduct.GetActiveProducts(companyid);
            ViewBag.IsAllItems = _bllconfiguration.GetConfiguration("DITEMS", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            ViewBag.productdata = deptdata;

            ViewBag.poLocationId = Convert.ToInt16(Session["loggeduserlocId"]);
            if (!ModelState.IsValid || poheader.PODetail.Count == 0)
            {
                if (poheader.PODetail.Count == 0)
                {
                    @ViewBag.Message = "4";
                }

                ViewBag.CurrencyId = poheader.CurrencyId;
                ViewBag.SupplierId = poheader.SupplierId;
                ViewBag.DeliveryLocationId = poheader.DeliveryLocationId;
                ViewBag.poLocationId = poheader.POLocationId;
                ViewBag.PaymentTermId = poheader.PaymentTermId;
                ViewBag.PaymentMethodId = poheader.PaymentMethodId;
                ViewBag.EventId = poheader.EventId;
                ViewBag.poLocationId = Convert.ToInt16(Session["loggeduserlocId"]);

                poheader.PODetail.ToList().ForEach(d =>
                {
                    var prd = _bllproduct.GetProductById(d.ProductId);
                    d.ProductName = prd.ProductName;
                    d.ProductCode = prd.ProductCode;
                    d.UOM = d.OrderQty + _bllproduct.GetUOMById(_bllproduct.GetProductById(d.ProductId).PurchasingUnit);
                });
                
                return View("~/Views/Transactions/PO/CreatePO.cshtml", poheader);
            }

            // check Month End Stataus
            // int year = poheader.PODate.Year;


            bool EnableMonthEndProcess = _bllconfiguration.GetConfiguration("EnableMonthEndProcess", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;

            if (EnableMonthEndProcess)
            {
                int monthendstatus = _bllmonthend.CheckMonthEnd(Convert.ToInt32(poheader.POLocationId), poheader.PODate.Year, poheader.PODate.Month);
                if (monthendstatus != 1)
                {
                    ModelState.Clear();

                    // PurchaseOrderHeader _poheader = new PurchaseOrderHeader();           
                    //_poheader.DocumentNo = poheader.DocumentNo;
                    //_poheader.PODetail = new List<PurchaseOrderDetail>();
                    //_poheader.PurchaseOrderHeaderId = poheader.PurchaseOrderHeaderId;
                    //_poheader.PODate = DateTime.Now;
                    //_poheader.DeliveryAddress = string.Empty;
                    //_poheader.ExpectedDate = DateTime.Now;

                    ViewBag.CurrencyId = poheader.CurrencyId;
                    ViewBag.SupplierId = poheader.SupplierId;
                    ViewBag.DeliveryLocationId = poheader.DeliveryLocationId;
                    ViewBag.poLocationId = poheader.POLocationId;
                    ViewBag.PaymentTermId = poheader.PaymentTermId;
                    ViewBag.PaymentMethodId = poheader.PaymentMethodId;
                    ViewBag.EventId = poheader.EventId;
                    ViewBag.poLocationId = Convert.ToInt16(Session["loggeduserlocId"]);

                    poheader.PODetail.ToList().ForEach(d =>
                    {
                        var prd = _bllproduct.GetProductById(d.ProductId);
                        d.ProductName = prd.ProductName;
                        d.ProductCode = prd.ProductCode;
                        d.UOM = d.OrderQty + _bllproduct.GetUOMById(_bllproduct.GetProductById(d.ProductId).PurchasingUnit);
                    });

                    ViewBag.Message = "5";
                    return View("~/Views/Transactions/PO/CreatePO.cshtml", poheader);
                }
            }

            // End check Month End Stataus

            poheader.Company = _bllcompany.GetCompanyById(poheader.CompanyID).CompanyName;
            poheader.Location = _blllocation.GetLocationById(poheader.POLocationId).LocationName;
            poheader.PODetail.ToList().ForEach(d =>
            {
                var prd = _bllproduct.GetProductById(d.ProductId);
                d.ProductName = prd.ProductName;
                d.ProductCode = prd.ProductCode;
                d.UOM = d.OrderQty + _bllproduct.GetUOMById(_bllproduct.GetProductById(d.ProductId).PurchasingUnit);
            });



            poheader.CreatedUser = Session["loggeduser"].ToString();
            poheader.CreatedDate = DateTime.Now;
            poheader.ModifiedDate = DateTime.Now;
            poheader.ModifiedUser = "";
            poheader.DataTransfer = 0;
            poheader.LocationId = Convert.ToInt32(Convert.ToInt32(Session["loggeduserlocId"]));
            poheader.GroupOfCompanyID = 1;
          //  poheader.CompanyID = 1;

            if (Convert.ToBoolean(Session["POApprovels"]) == true)
            {
                poheader.DocumentStatus = 2;
            }
            else
            {
                poheader.DocumentStatus = 3;
            }

            poheader.TotDiscounts = 0;
            poheader.RequestedBy = Session["loggeduser"].ToString();
            poheader.PaymentExpectedDate = DateTime.Now;
                       
            int docid = _bllcommon.GetDcumentId("Purchase Order", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            var docnum = _bllcommon.GetDocumentNo("Purchase Order",
                                                  Convert.ToInt32(Session["loggeduserlocId"]),
                                                  Convert.ToString(poheader.POLocationId),
                                                  docid,
                                                  false, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            poheader.DocumentId = docid;
            poheader.DocumentNo = docnum;
            poheader.DocumentDate = DateTime.Now;
            poheader.JobClassId = 1;
            poheader.ExpiryDate = DateTime.Now;
            poheader.CompanyID= Convert.ToInt32(Session["loggedusercompanyId"].ToString());


            bool res = _bllpurchaseorder.SavePurchaseOrder(poheader);

            ViewBag.POCode = docnum;
            if (res)
            {
                //poheader.Company = _bllcompany.GetCompanyById(poheader.CompanyID).CompanyName;
                //poheader.Location = _blllocation.GetLocationById(poheader.POLocationId).LocationName;
                //poheader.PODetail.ToList().ForEach(d =>
                //{
                //    var prd = _bllproduct.GetProductById(d.ProductId);
                //    d.ProductName = prd.ProductName;
                //    d.ProductCode = prd.ProductCode;
                //});

                ModelState.Clear();
                ViewBag.POHeaderId = poheader.PurchaseOrderHeaderId;
                PurchaseOrderHeader _poheader = new PurchaseOrderHeader();
                _poheader.DocumentNo = _bllcommon.GetDocumentNo("Purchase Order",
                                                   Convert.ToInt32(Session["loggeduserlocId"]),
                                                   "1",
                                                    docid,
                                                    true, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));

                _poheader.PODetail = new List<PurchaseOrderDetail>();
                _poheader.PurchaseOrderHeaderId = poheader.PurchaseOrderHeaderId;
                _poheader.PODate = DateTime.Now;
                _poheader.DeliveryAddress =string.Empty;
                _poheader.ExpectedDate = DateTime.Now;      
                return View("~/Views/Transactions/PO/CreatePO.cshtml", _poheader);

            }
            else
            {
                ViewBag.CurrencyId = poheader.CurrencyId;
                ViewBag.SupplierId = poheader.SupplierId;
                ViewBag.DeliveryLocationId = poheader.DeliveryLocationId;
                ViewBag.poLocationId = poheader.POLocationId;
                ViewBag.PaymentTermId = poheader.PaymentTermId;
                ViewBag.PaymentMethodId = poheader.PaymentMethodId;
                ViewBag.EventId = poheader.EventId;

                ViewBag.Message = "2";
                return View("~/Views/Transactions/PO/CreatePO.cshtml", poheader);
            }


        }

      //  [Authorize(Roles = "POCreatee")]
        [HttpPost]
        public ActionResult TempSavePO(PurchaseOrderHeader poheader)
        {

            // check permissions
            if (!_appmanager.SetPermissions(3, Session["loggeduserempcode"].ToString(), "POCreatee"))
            {
                @ViewBag.Permissions = "No user permissions to Create POs";
                return View("~/Views/Account/AccessDenied.cshtml");
            }

            if(poheader.PODate.Year <2024)
            {
                poheader.PODate = DateTime.Now;
            }

            //end check permissions
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var deptdata = _bllproduct.GetActiveProducts(companyid);
            ViewBag.IsAllItems = _bllconfiguration.GetConfiguration("DITEMS", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            ViewBag.productdata = deptdata;
      

            if (!ModelState.IsValid || poheader.PODetail.Count == 0)
            {
                if (poheader.PODetail.Count == 0)
                {
                    @ViewBag.Message = "4";
                }
                ViewBag.CurrencyId = poheader.CurrencyId;
                ViewBag.SupplierId = poheader.SupplierId;
                ViewBag.DeliveryLocationId = poheader.DeliveryLocationId;
                ViewBag.poLocationId = poheader.POLocationId;
                ViewBag.PaymentTermId = poheader.PaymentTermId;
                ViewBag.PaymentMethodId = poheader.PaymentMethodId;
                ViewBag.EventId = poheader.EventId;
               
                poheader.PODetail.ToList().ForEach(d =>
                {
                    var prd = _bllproduct.GetProductById(d.ProductId);
                    d.ProductName = prd.ProductName;
                    d.ProductCode = prd.ProductCode;
                    d.UOM = d.OrderQty + _bllproduct.GetUOMById(_bllproduct.GetProductById(d.ProductId).PurchasingUnit);
                });
                //return View("~/Views/Transactions/PO/EditPO.cshtml", poheader);
                return View("~/Views/Transactions/PO/CreatePO.cshtml", poheader);
                
            }

            bool EnableMonthEndProcess = _bllconfiguration.GetConfiguration("EnableMonthEndProcess", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;

            if (EnableMonthEndProcess)
            {
                // check Month End Stataus
                // int year = poheader.PODate.Year;
                int monthendstatus = _bllmonthend.CheckMonthEnd(Convert.ToInt32(poheader.POLocationId), poheader.PODate.Year, poheader.PODate.Month);
                if (monthendstatus != 1)
                {
                    ModelState.Clear();

                    //PurchaseOrderHeader _poheader = new PurchaseOrderHeader();
                    //_poheader.DocumentNo = poheader.DocumentNo;
                    //_poheader.PODetail = new List<PurchaseOrderDetail>();
                    //_poheader.PurchaseOrderHeaderId = poheader.PurchaseOrderHeaderId;
                    //_poheader.PODate = DateTime.Now;
                    //_poheader.DeliveryAddress = string.Empty;
                    //_poheader.ExpectedDate = DateTime.Now;
                    ViewBag.CurrencyId = poheader.CurrencyId;
                    ViewBag.SupplierId = poheader.SupplierId;
                    ViewBag.DeliveryLocationId = poheader.DeliveryLocationId;
                    ViewBag.poLocationId = poheader.POLocationId;
                    ViewBag.PaymentTermId = poheader.PaymentTermId;
                    ViewBag.PaymentMethodId = poheader.PaymentMethodId;
                    ViewBag.EventId = poheader.EventId;

                    poheader.PODetail.ToList().ForEach(d =>
                    {
                        var prd = _bllproduct.GetProductById(d.ProductId);
                        d.ProductName = prd.ProductName;
                        d.ProductCode = prd.ProductCode;
                        d.UOM = d.OrderQty + _bllproduct.GetUOMById(_bllproduct.GetProductById(d.ProductId).PurchasingUnit);
                    });

                    ViewBag.Message = "5";
                    return View("~/Views/Transactions/PO/CreatePO.cshtml", poheader);
                }
            }

            // End check Month End Stataus



            poheader.Company = _bllcompany.GetCompanyById(poheader.CompanyID).CompanyName;
            poheader.Location = _blllocation.GetLocationById(poheader.POLocationId).LocationName;
            poheader.PODetail.ToList().ForEach(d => {
                var prd = _bllproduct.GetProductById(d.ProductId);
                d.ProductName = prd.ProductName;
                d.ProductCode = prd.ProductCode;
                //d.UOM = d.OrderQty + _bllproduct.GetUOMById(_bllproduct.GetProductById(d.ProductId).PurchasingUnit);
                 d.UOM = _bllproduct.GetUOMById(_bllproduct.GetProductById(d.ProductId).PurchasingUnit);
            });

            poheader.CreatedUser = Session["loggeduser"].ToString();
            poheader.CreatedDate = DateTime.Now;
            poheader.ModifiedDate = DateTime.Now;
            poheader.ModifiedUser = "";
            poheader.DataTransfer = 0;
            poheader.LocationId = 1;
            poheader.GroupOfCompanyID = 1;
            poheader.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            poheader.DocumentStatus = 1;
            poheader.RequestedBy = Session["loggeduser"].ToString();
            poheader.PaymentExpectedDate = DateTime.Now;


            //int docid = _bllcommon.GetDcumentId("Purchase Order");
            //var docnum = _bllcommon.GetDocumentNo("Purchase Order",
            //                                        Convert.ToInt32(Session["loggeduserlocId"]),
            //                                        "1",
            //                                         docid,
            //                                         true);
            //poheader.DocumentNo = docnum;

            poheader.DocumentDate = DateTime.Now;
            poheader.JobClassId = 1;
            poheader.ExpiryDate = DateTime.Now;
            poheader.IsTempPO = true;

            bool res = _bllpurchaseorder.SavePurchaseOrder(poheader);
            //ViewBag.POCode = docnum;
            ViewBag.POCode = poheader.DocumentNo;
            if (res)
            {
                ViewBag.Message = "1";

                //poheader.Company = _bllcompany.GetCompanyById(poheader.CompanyID).CompanyName;
                //poheader.Location = _blllocation.GetLocationById(poheader.POLocationId).LocationName;
                //poheader.PODetail.ToList().ForEach(d => {
                //    var prd = _bllproduct.GetProductById(d.ProductId);
                //    d.ProductName = prd.ProductName;
                //    d.ProductCode = prd.ProductCode;
                //});

                int docid = _bllcommon.GetDcumentId("Purchase Order", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                var docnum = _bllcommon.GetDocumentNo("Purchase Order",
                                                        Convert.ToInt32(Session["loggeduserlocId"]),
                                                        "1",
                                                         docid,
                                                         true, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));

                ViewBag.POHeaderId = poheader.PurchaseOrderHeaderId;
                ViewBag.poLocationId = Convert.ToInt16(Session["loggeduserlocId"]);
                ModelState.Clear();
                 PurchaseOrderHeader _poheader = new PurchaseOrderHeader();
                _poheader.PODetail = new List<PurchaseOrderDetail>();
                _poheader.PurchaseOrderHeaderId = poheader.PurchaseOrderHeaderId;
                _poheader.PODate = DateTime.Now;
                _poheader.DeliveryAddress = string.Empty;
                _poheader.ExpectedDate = DateTime.Now;
               
                _poheader.DocumentNo = docnum;
                 return View("~/Views/Transactions/PO/CreatePO.cshtml", _poheader);
              //  return RedirectToAction("CreatePO");
            }
            else
            {
                poheader.Company = _bllcompany.GetCompanyById(poheader.CompanyID).CompanyName;
                poheader.Location = _blllocation.GetLocationById(poheader.POLocationId).LocationName;
                poheader.PODetail.ToList().ForEach(d =>
                {
                    var prd = _bllproduct.GetProductById(d.ProductId);
                    d.ProductName = prd.ProductName;
                    d.ProductCode = prd.ProductCode;
                    d.UOM = d.OrderQty + _bllproduct.GetUOMById(_bllproduct.GetProductById(d.ProductId).PurchasingUnit);
                });
                ViewBag.Message = "2";
                return View("~/Views/Transactions/PO/CreatePO.cshtml", poheader);
            }


        }

       // [Authorize(Roles = "POEdit")]
        [HttpPost]
        public ActionResult TempPO(PurchaseOrderHeader podet)
        {
            // check permissions
            if (!_appmanager.SetPermissions(3, Session["loggeduserempcode"].ToString(), "POEdit"))
            {
                @ViewBag.Permissions = "No user permissions to Edit POs";
                return View("~/Views/Account/AccessDenied.cshtml");
            }
            //end check permissions

            return View("~/Views/Transactions/PO/EditPO.cshtml");
        }

        [HttpGet]
        public JsonResult GetActivePaytmentMethods()
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var paymentMethods = _bllpurchaseorder.GetActivePaymentMethods(companyid);
            return Json(JsonConvert.SerializeObject(paymentMethods, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetActivePaytmentTerms()
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var paymentTerms = _bllpurchaseorder.GetActivePaymentterms(companyid);
            return Json(JsonConvert.SerializeObject(paymentTerms, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }
        public JsonResult ApplyTaxes(long id, decimal qty, long polocid)
        {
            //ProductService reporsitory = new ProductService();
            //PurchaseOrderService porepository = new PurchaseOrderService();
            //TaxService taxservice = new TaxService();
         
          
            List<POViewModel> poviewmodel = new List<POViewModel>();
         
            
            var product = _bllpurchaseorder.GetProductDetails(id, polocid, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            var producttaxes = _blltax.GetProductTaxByProductId(id).ToList();
            //Added by pavithra
            var invProduct = _bllInvProduct.GetProductById(id);
            decimal totaltaxamount = 0;
            //A
            //Existing tax calculation Commented by pavithra 2019-12-05
            //foreach (var t in producttaxes)
            //{
            //    decimal taxamount = 0;
            //    taxamount = (product.CostPrice * t.TaxPrc / 100);
            //    totaltaxamount += taxamount;

            //}

            //Added by pavithra 2019-12-05
            //B
            decimal netAmount = 0;
            decimal costvalue = 0;
            costvalue = product.CostPrice * qty;
            foreach (var t in producttaxes)
            {
                decimal taxamount = 0;
                taxamount = (costvalue * t.TaxPrc / 100);
                netAmount = costvalue + taxamount;
                costvalue = netAmount;
                totaltaxamount += taxamount;

            }


            PORepoViewModel poRepoViewModel = new PORepoViewModel();
            poRepoViewModel.CostPrice = product.CostPrice;
            poRepoViewModel.CostValue= product.CostPrice * qty;
            poRepoViewModel.SellingPrice = product.SellingPrice;
            poRepoViewModel.UOM = _bllproduct.GetUOMById(_bllproduct.GetProductById(id).PurchasingUnit);
            poRepoViewModel.ItemId= id;
            var dbproduct = _bllproduct.GetProductById(id);
            poRepoViewModel.ItemDesc = dbproduct.ProductCode + " - " +dbproduct.ProductName;
            //A
            //poRepoViewModel.TaxAmount = totaltaxamount * qty;
            //B
            poRepoViewModel.TaxAmount = totaltaxamount;
            poRepoViewModel.RecQty = qty;
            poRepoViewModel.IsExpiry = _bllproduct.GetProductById(id).IsExpiry;


            poRepoViewModel.POQuantity = 0;

            //Added by pavithra
            //poRepoViewModel.FixedDiscountAmount = invProduct.FixedDiscountAmount;
            //poRepoViewModel.FixedDiscountPercentage = invProduct.FixedDiscountPercentage;

            //if(poRepoViewModel.FixedDiscountPercentage > 0)
            //{
            //    poRepoViewModel.DiscountType = 0;
            //    poRepoViewModel.Discounts = poRepoViewModel.FixedDiscountPercentage;
            //}
            //else if(poRepoViewModel.FixedDiscountAmount > 0)
            //{
            //    poRepoViewModel.DiscountType = 1;
            //    poRepoViewModel.Discounts = poRepoViewModel.FixedDiscountAmount;
            //}


            return new JsonResult { Data = poRepoViewModel, JsonRequestBehavior = JsonRequestBehavior.AllowGet  };

        }

        public JsonResult ApplyPOTaxes(long id, decimal qty, long polocid)
        {
            

           
            List<POViewModel> poviewmodel = new List<POViewModel>();
         

            var product = _bllpurchaseorder.GetProductDetails(id, polocid, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            var producttaxes = _blltax.GetProductTaxByProductId(id).ToList();
            //Added by pavithra
            var invProduct = _bllInvProduct.GetProductById(id);
            decimal totaltaxamount = 0;
            //A
            //Existing tax calculation Commented by pavithra 2019-12-05
            //foreach (var t in producttaxes)
            //{
            //    decimal taxamount = 0;
            //    taxamount = (product.CostPrice * t.TaxPrc / 100);
            //    totaltaxamount += taxamount;

            //}

            //Added by pavithra 2019-12-05
            //B
            decimal netAmount = 0;
            decimal costvalue = 0;
            costvalue = product.CostPrice * qty;
            foreach (var t in producttaxes)
            {
                decimal taxamount = 0;
                taxamount = (costvalue * t.TaxPrc / 100);
                netAmount = costvalue + taxamount;
                costvalue = netAmount;
                totaltaxamount += taxamount;

            }


            PORepoViewModel poRepoViewModel = new PORepoViewModel();
            poRepoViewModel.CostPrice = product.CostPrice;
            poRepoViewModel.CostValue = product.CostPrice * qty;
            poRepoViewModel.SellingPrice = product.SellingPrice;
            // poRepoViewModel.UOM = qty+" "+_bllproduct.GetUOMById(_bllproduct.GetProductById(id).PurchasingUnit);
            poRepoViewModel.UOM = _bllproduct.GetUOMById(_bllproduct.GetProductById(id).PurchasingUnit);

            poRepoViewModel.ItemId = id;
            var dbproduct = _bllproduct.GetProductById(id);
            poRepoViewModel.ItemDesc = dbproduct.ProductCode + " - " + dbproduct.ProductName;
            //A
            //poRepoViewModel.TaxAmount = totaltaxamount * qty;
            //B
            poRepoViewModel.TaxAmount = totaltaxamount;
            poRepoViewModel.RecQty = qty;
            poRepoViewModel.IsExpiry = _bllproduct.GetProductById(id).IsExpiry;

           
            return new JsonResult { Data = poRepoViewModel, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

        }

        public JsonResult CheckPrices(long id, decimal qty, long polocid,long grnid)
        {
            //ProductService reporsitory = new ProductService();
           
           // Service.Transactions.PurchaseService grnrepository = new Service.Transactions.PurchaseService();
         
            List<POViewModel> poviewmodel = new List<POViewModel>();


            var product = _bllpurchaseorder.GetProductDetails(id, polocid, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            var producttaxes = _blltax.GetProductTaxByProductId(id).ToList();

            decimal totaltaxamount = 0;
            foreach (var t in producttaxes)
            {
                decimal taxamount = 0;
                taxamount = (product.CostPrice * t.TaxPrc / 100);
                totaltaxamount += taxamount;

            }


            PORepoViewModel poRepoViewModel = new PORepoViewModel();
            poRepoViewModel.CostPrice = product.CostPrice;
            poRepoViewModel.CostValue = product.CostPrice * qty;
            //  poRepoViewModel.SellingPrice = product.SellingPrice * qty;
            poRepoViewModel.SellingPrice = product.SellingPrice;
            poRepoViewModel.UOM = qty + _bllproduct.GetUOMById(_bllproduct.GetProductById(id).PurchasingUnit);
            poRepoViewModel.ItemId = id;
            poRepoViewModel.ItemDesc = _bllproduct.GetProductById(id).ProductName;
            //poRepoViewModel.Discounts = discountamt * qty;
            poRepoViewModel.TaxAmount = totaltaxamount * qty;
            poRepoViewModel.RecQty = qty;

            var grn = _bllgrn.GetGRNDetById(grnid).Where(g=>g.ProductID== id).FirstOrDefault();
            poRepoViewModel.OrderQuantity = grn.OrderQty;
            poRepoViewModel.TOGQuantity = grn.TOGQty;


            return new JsonResult
            {
                Data = poRepoViewModel,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

        }

        public JsonResult ApplyNewTaxes(long id, decimal qty, long polocid,decimal newcostprice)
        {
          
           
           
            List<POViewModel> poviewmodel = new List<POViewModel>();

            var product = _bllpurchaseorder.GetProductDetails(id, polocid, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            var producttaxes = _blltax.GetProductTaxByProductId(id).ToList();
            decimal totaltaxamount = 0;

            //foreach (var t in producttaxes)
            //{
            //    decimal taxamount = 0;
            //    taxamount = (newcostprice * t.TaxPrc / 100);
            //    totaltaxamount += taxamount;

            //}

            decimal netAmount = 0;
            decimal costvalue = 0;
            costvalue = newcostprice * qty;
            foreach (var t in producttaxes)
            {
                decimal taxamount = 0;
                taxamount = (costvalue * t.TaxPrc / 100);
                netAmount = costvalue + taxamount;
                costvalue = netAmount;
                totaltaxamount += taxamount;

            }


            PORepoViewModel poRepoViewModel = new PORepoViewModel();
            poRepoViewModel.CostPrice = newcostprice;
            poRepoViewModel.CostValue = newcostprice * qty;
            //  poRepoViewModel.SellingPrice = product.SellingPrice * qty;
            poRepoViewModel.SellingPrice = product.SellingPrice;
            poRepoViewModel.UOM = qty + _bllproduct.GetUOMById(_bllproduct.GetProductById(id).PurchasingUnit);
            poRepoViewModel.ItemId = id;
            poRepoViewModel.ItemDesc = _bllproduct.GetProductById(id).ProductName;
            //poRepoViewModel.Discounts = discountamt * qty;
            poRepoViewModel.TaxAmount = totaltaxamount;
            poRepoViewModel.RecQty = qty;

           return new JsonResult
            {
                Data = poRepoViewModel,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };



        }

        [HttpGet]
        public JsonResult ApplyTaxes1(long id, decimal qty, long polocid)
        {
          
         
            List<POViewModel> poviewmodel = new List<POViewModel>();

            //  var product = reporsitory.GetProductById(id);
            var taxes = _bllpurchaseorder.GetProductTaxes(id, polocid, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            decimal taxontaxsp = 0, taxontaxamount = 0;
            decimal directtaxsp = 0, directtaxamount = 0;
            decimal discountamt = 0;

            foreach (var tax in taxes)
            {
                discountamt = tax.SellingPrice * tax.DiscountPrc / 100;

                if (tax.IsTaxOnTax)
                {

                    if (taxontaxsp == 0)
                    {
                        taxontaxsp = ((tax.SellingPrice - discountamt) * (tax.TaxPrc / 100)) + (tax.SellingPrice - discountamt);
                        taxontaxamount += ((tax.SellingPrice - discountamt) * (tax.TaxPrc / 100));
                    }
                    else
                    {
                        taxontaxamount += (taxontaxsp * (tax.TaxPrc / 100));
                        taxontaxsp = (taxontaxsp * (tax.TaxPrc / 100)) + taxontaxsp;

                    }
                }
                else
                {

                    //directtaxsp += (tax.SellingPrice * (tax.TaxPrc / 100)) + tax.SellingPrice;
                    directtaxsp += ((tax.SellingPrice - discountamt) * (tax.TaxPrc / 100));
                    directtaxamount += ((tax.SellingPrice - discountamt) * (tax.TaxPrc / 100));
                }


            }

            PORepoViewModel poRepoViewModel = new PORepoViewModel();
            poRepoViewModel.CostPrice = taxes.First().CostPrice * qty;
            poRepoViewModel.SellingPrice = (taxontaxsp + directtaxsp) * qty;
            poRepoViewModel.Discounts = discountamt * qty;
            poRepoViewModel.TaxAmount = (taxontaxamount + directtaxamount) * qty;
            poRepoViewModel.RecQty = qty;

            return new JsonResult { Data = poRepoViewModel, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

            //return Json(JsonConvert.SerializeObject(product, Formatting.None, new JsonSerializerSettings
            //   { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);

        }

        public JsonResult AddSupplierProducts1(long supplierid, long polocid)
        {
           
          
            List<POViewModel> poviewmodel = new List<POViewModel>();

            List<PORepoViewModel> taxapplieditems = new List<PORepoViewModel>();
            var xx = _bllpurchaseorder.GetReOrderLevelExceededProductBySupplierId(supplierid, polocid, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            foreach (var node in xx)
            {
                PORepoViewModel poRepoViewModel = new PORepoViewModel();
                var taxes = _bllpurchaseorder.GetProductTaxesBySupplierProductId(node.ItemId, polocid, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                decimal qty = node.ReOrderQty;

                decimal taxontaxsp = 0, taxontaxamount = 0;
                decimal directtaxsp = 0, directtaxamount = 0;
                decimal discountamt = 0;

                foreach (var tax in taxes)
                {
                    discountamt = tax.SellingPrice * tax.DiscountPrc / 100;

                    if (tax.IsTaxOnTax)
                    {

                        if (taxontaxsp == 0)
                        {
                            taxontaxsp = ((tax.SellingPrice - discountamt) * (tax.TaxPrc / 100)) + (tax.SellingPrice - discountamt);
                            taxontaxamount += ((tax.SellingPrice - discountamt) * (tax.TaxPrc / 100));
                        }
                        else
                        {
                            taxontaxamount += (taxontaxsp * (tax.TaxPrc / 100));
                            taxontaxsp = (taxontaxsp * (tax.TaxPrc / 100)) + taxontaxsp;

                        }
                    }
                    else
                    {

                        //directtaxsp += (tax.SellingPrice * (tax.TaxPrc / 100)) + tax.SellingPrice;
                        directtaxsp += ((tax.SellingPrice - discountamt) * (tax.TaxPrc / 100));
                        directtaxamount += ((tax.SellingPrice - discountamt) * (tax.TaxPrc / 100));
                    }


                }

                poRepoViewModel.CostPrice = taxes.First().CostPrice * qty;
                poRepoViewModel.SellingPrice = (taxontaxsp + directtaxsp) * qty;
                poRepoViewModel.Discounts = discountamt * qty;
                poRepoViewModel.TaxAmount = (taxontaxamount + directtaxamount) * qty;
                poRepoViewModel.RecQty = qty;
                poRepoViewModel.ItemId = node.ItemId;
                poRepoViewModel.ItemDesc = node.ItemDesc;
                taxapplieditems.Add(poRepoViewModel);
            }
            return new JsonResult { Data = taxapplieditems, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            // return Json(taxapplieditems);
        }

        public JsonResult AddSupplierProducts(long supplierid, long polocid)
        {
           // ProductService reporsitory = new ProductService();
            //PurchaseOrderService porepository = new PurchaseOrderService();
            List<POViewModel> poviewmodel = new List<POViewModel>();
          
            List<PORepoViewModel> reorderitems = new List<PORepoViewModel>();

            var xx = _bllpurchaseorder.GetReOrderLevelExceededProductBySupplierId(supplierid, polocid, Convert.ToInt32(Session["loggedusercompanyId"].ToString())).
                                                    ToList().Where(p=>(p.CurrentStock+p.POStock)<=p.ROL);

            foreach (var node in xx)
            {
                PORepoViewModel poRepoViewModel = new PORepoViewModel();
                var producttaxes = _blltax.GetProductTaxByProductId(node.ItemId).ToList();

                decimal totaltaxamount = 0;

                //foreach (var t in producttaxes)
                //{
                //    decimal taxamount = 0;
                //    taxamount = (node.CostPrice * t.TaxPrc / 100);
                //    totaltaxamount += taxamount;

                //}

                decimal netAmount = 0;
                decimal costvalue = 0;
                costvalue = node.CostPrice * node.ReOrderQty;
                foreach (var t in producttaxes)
                {
                    decimal taxamount = 0;
                    taxamount = (costvalue * t.TaxPrc / 100);
                    netAmount = costvalue + taxamount;
                    costvalue = netAmount;
                    totaltaxamount += taxamount;

                }



                poRepoViewModel.CostPrice = node.CostPrice;
                poRepoViewModel.SellingPrice = node.SellingPrice;
                //  poRepoViewModel.Discounts = 
                // poRepoViewModel.TaxAmount = (taxontaxamount + directtaxamount) * qty;
                poRepoViewModel.CostValue = node.ReOrderQty * node.CostPrice;
                poRepoViewModel.RecQty = node.ReOrderQty;
                poRepoViewModel.ItemId = node.ItemId;
                poRepoViewModel.ItemDesc = node.ItemDesc;
                poRepoViewModel.TaxAmount = totaltaxamount;
                poRepoViewModel.UOM = node.ReOrderQty + _bllproduct.GetUOMById(_bllproduct.GetProductById(node.ItemId).PurchasingUnit);
                reorderitems.Add(poRepoViewModel);
            }

            return new JsonResult { Data = reorderitems, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

            // return Json(taxapplieditems);


        }


        public JsonResult AddRequestNoteItemsBySupplierId(long supplierid, long requestnoteid)
        {
            
            List<POViewModel> poviewmodel = new List<POViewModel>();
          
            List<PORepoViewModel> reorderitems = new List<PORepoViewModel>();

            var xx = _bllpurchaseorder.GetRequestNoteItemsBySupplierId(supplierid,requestnoteid, Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ToList();

            foreach (var node in xx)
            {
                PORepoViewModel poRepoViewModel = new PORepoViewModel();
                var producttaxes = _blltax.GetProductTaxByProductId(node.ItemId).ToList();
                decimal totaltaxamount = 0;
                
                decimal netAmount = 0;
                decimal costvalue = 0;
                costvalue = node.CostPrice * node.ReOrderQty;
                foreach (var t in producttaxes)
                {
                    decimal taxamount = 0;
                    taxamount = (costvalue * t.TaxPrc / 100);
                    netAmount = costvalue + taxamount;
                    costvalue = netAmount;
                    totaltaxamount += taxamount;

                }
                poRepoViewModel.CostPrice = node.CostPrice;
                poRepoViewModel.SellingPrice = node.SellingPrice;            
                poRepoViewModel.CostValue = node.ReOrderQty * node.CostPrice;
                poRepoViewModel.RecQty = node.ReOrderQty;
                poRepoViewModel.ItemId = node.ItemId;
                poRepoViewModel.ItemDesc = node.ItemDesc;
                poRepoViewModel.TaxAmount = totaltaxamount;
                poRepoViewModel.UOM = node.ReOrderQty + _bllproduct.GetUOMById(_bllproduct.GetProductById(node.ItemId).PurchasingUnit);
                reorderitems.Add(poRepoViewModel);
            }

            return new JsonResult { Data = reorderitems, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

            // return Json(taxapplieditems);


        }

        [HttpPost]
        public ActionResult GetData(long id, long qty)
        {
            //Create a list of your objects
            List<TestViewModel> objects = new List<TestViewModel>();

            //Add some fake ones
            objects.Add(new TestViewModel() { Name = "A", Description = "Description of A" });
            objects.Add(new TestViewModel() { Name = "B", Description = "Description of B" });
            objects.Add(new TestViewModel() { Name = "C", Description = "Description of C" });

            //Return your JSON object
            return Json(objects);
        }

        [HttpGet]
        public JsonResult GetActivePOs()
        {
          //  PurchaseOrderService reporsitory = new PurchaseOrderService();
          //  var pos = reporsitory.GetAllPos().Where(p=>p.IsTempPO==false);
            var pos = _bllpurchaseorder.GetAllPos(Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            return Json(JsonConvert.SerializeObject(pos, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetNewPOs()
        {
            //  PurchaseOrderService reporsitory = new PurchaseOrderService();
            //  var pos = reporsitory.GetAllPos().Where(p=>p.IsTempPO==false);
            var pos = _bllpurchaseorder.GetNewPos(Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            return Json(JsonConvert.SerializeObject(pos, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetSavedPOs(long supplierid)
        {



            
            var pos = _bllpurchaseorder.GetSavedAllPosBySupplierId(supplierid, Convert.ToInt32(Session["loggedusercompanyId"].ToString())).Where(po=>po.DocumentStatus==3 && po.IsGRN == false);

            List<PONos> pns = new List<PONos>();

            PONos pn;

            foreach (PurchaseOrderHeader ph in pos)
            {

                pn = new PONos();

                pn.DocumentNo = ph.DocumentNo;

                pn.PurchaseOrderHeaderId = ph.PurchaseOrderHeaderId;

                pns.Add(pn);

            }

            JsonResult jm = Json(JsonConvert.SerializeObject(pns, Formatting.None, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);



            return jm;

        }

        //[HttpGet]
        //public ActionResult RPTPOSummary()
        //{
        //    return View("~/Views/Reports/PO/RPTPOSummary.cshtml", new List<POSummaryViewModel>());
        //}

     //   [HttpGet]
        [Authorize(Roles = "Reports")]
        public ActionResult RPTPOSummary(POSummaryViewModel vvm)
        {

            if (_bllconfiguration.GetConfiguration("UReports", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn)
            {

                if (!_appmanager.CheckReportPermissions(999, Session["loggeduserempcode"].ToString(), "RPTPOSummary"))
                {
                    @ViewBag.Permissions = "No user permissions to View PO Summary";
                    return View("~/Views/Account/AccessDenied.cshtml");
                }

            }

            var po = _bllpurchaseorder.GetPOSummaryReport(vvm.LocationId, vvm.DocumentId, vvm.DateFrom, vvm.DateTo);
            List<POSummaryViewModel> posummarymodel = new List<POSummaryViewModel>();
            foreach (var s in po)
            {
                POSummaryViewModel v = new POSummaryViewModel();
                v.PurchaseOrderHeaderId = s.PurchaseOrderHeaderId;
                v.DocumentNo = s.DocumentNo;                
                v.Location = _blllocation.GetLocationById(s.POLocationId).LocationName;
                v.DocumentDate = s.DocumentDate;
                v.PODate = s.PODate;
                v.DeliveryDate = s.ExpectedDate;
                v.NetAmount = s.NetAmount;
                v.Status = _blldocStatus.GetDocStatusById(s.DocumentStatus).Description;
                posummarymodel.Add(v);
            }
            if (vvm.DateFrom.Date.ToShortDateString() == "01/01/0001" &&
                vvm.DateTo.Date.ToShortDateString() == "01/01/0001")
            {
                ViewBag.DateFrom = DateTime.Now;
                ViewBag.DateTo = DateTime.Now;
            }
            else
            {
                ViewBag.DateFrom = vvm.DateFrom;
                ViewBag.DateTo = vvm.DateTo;
            }
            ViewBag.DocumentId = vvm.DocumentId;
            var docno = vvm.DocumentNo;
            ViewBag.POLocationId = vvm.LocationId;

            return View("~/Views/Reports/PO/RPTPOSummary.cshtml", posummarymodel);
        }
       
        [HttpGet]
        public JsonResult GetDocNoByLocId(long locid,DateTime from,DateTime to)
        {

            var types = _bllpurchaseorder.GetDocNoByLocId(locid, Convert.ToInt32(Session["loggedusercompanyId"].ToString())
                ).Where(p=>p.PODate.Date>=from.Date && p.PODate.Date<=to.Date).Select(p => new { p.PurchaseOrderHeaderId, p.DocumentNo }); ;
            return Json(JsonConvert.SerializeObject(types, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }


        [Authorize(Roles = "Reports")]
        [HttpGet]
        public ActionResult GetPODetailReport()
        {
            if (_bllconfiguration.GetConfiguration("UReports", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn)
            {

                if (!_appmanager.CheckReportPermissions(999, Session["loggeduserempcode"].ToString(), "RPTPODetail"))
                {
                    @ViewBag.Permissions = "No user permissions to View PO Details";
                    return View("~/Views/Account/AccessDenied.cshtml");
                }

            }

            return View("~/Views/Reports/PO/RPTPODetails.cshtml");
        }

        [Authorize(Roles = "Reports")]
        [HttpPost]
        public ActionResult GetPODetailReport(PODetailViewModel vm)
        {

            if (_bllconfiguration.GetConfiguration("UReports", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn)
            {

                if (!_appmanager.CheckReportPermissions(999, Session["loggeduserempcode"].ToString(), "RPTPODetail"))
                {
                    @ViewBag.Permissions = "No user permissions to View PO Details";
                    return View("~/Views/Account/AccessDenied.cshtml");
                }

            }

            var reportdata = _bllpurchaseorder.GetPODetailReport(vm.LocationId, vm.DocumentId,
                                                                    vm.DateFrom, vm.DateTo, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            return View("~/Views/Reports/PO/RPTPODetails.cshtml", reportdata);
        }

        [HttpGet]
        public JsonResult VaidateQuantity(long id,decimal qty, long polocid)
        {
            var dbproduct = _bllproduct.GetProductsByLocIdProductId(polocid,id).FirstOrDefault();

            return new JsonResult { Data = dbproduct, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpGet]
        public JsonResult VaidateQuantityForReturn(long id, long polocid)
        {
            //var dbproduct = _bllproduct.GetProductsByLocIdProductId(polocid, id).FirstOrDefault();
            var stock = _bllproduct.GetActualProductStock(polocid, id);

            return new JsonResult { Data = stock, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        public class StockValidationResult
        {
            public decimal Stock { get; set; }
            public decimal Hold_Stock { get; set; }
        }

        public JsonResult VaidateQuantityForTOG(long id, long polocid)
        {
            decimal HoldStock = 0;


            var stock = _bllproduct.GetActualProductStockTOG(polocid, id,ref HoldStock);
            var result = new StockValidationResult
            {
                Stock = stock,
                Hold_Stock = HoldStock
            };

            return Json(result, JsonRequestBehavior.AllowGet);
            //return new JsonResult { Data = stock, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpGet]   
        public JsonResult VaidateQuantityForRequest(long id, long polocid)
        {
            var dbproduct = _bllproduct.GetProductsByLocIdProductId(polocid, id).FirstOrDefault().Stock;          
            return new JsonResult { Data = dbproduct, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        [HttpGet]
        public JsonResult GetLocationProducts(long locid)
        {

            // var product1 = purchaseOrderService.GetLocationProducts(locid);
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var product = _bllpurchaseorder.GetLocationProductNames(locid, companyid).Select(p=> new{p.ProductId,p.ProductCode,p.ProductName,p.UOMDesc });
            //product.ToList().ForEach(p =>
            //{
            //    p.UOMDesc = _productservice.GetUOMById(_productservice.GetProductById(p.ProductId).PurchasingUnit);
            //}
            //);
            return Json(JsonConvert.SerializeObject(product, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetSupplierProducts(long supplierid)
        {
            // var product1 = purchaseOrderService.GetLocationProducts(locid);
            var product = _bllpurchaseorder.GetSupplierProductsNames(supplierid, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            return Json(JsonConvert.SerializeObject(product, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Inactive(long id)
        {
            _bllpurchaseorder.ActiveInactivePO(id,false,Session["loggeduser"].ToString());
            //var pos = _bllpurchaseorder.GetAllTempPos().ToList().Where(p => p.IsTempPO == true && p.IsGRN == false);
            var pos = _bllpurchaseorder.GetAllTempPos(Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ToList().Where(p => p.IsGRN == false);
            return View("~/Views/Transactions/PO/ViewAllPos.cshtml", pos);

        }

        public ActionResult Active(long id)
        {
            _bllpurchaseorder.ActiveInactivePO(id,true, Session["loggeduser"].ToString());
            //var pos = _bllpurchaseorder.GetAllTempPos().ToList().Where(p => p.IsTempPO == true && p.IsGRN == false);
            var pos = _bllpurchaseorder.GetAllTempPos(Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ToList().Where(p => p.IsGRN == false);
            return View("~/Views/Transactions/PO/ViewAllPos.cshtml", pos);

        }

        [HttpGet]
        public JsonResult GetEvents()
        {
            var events = _bllevents.GetActiveEvents(Convert.ToInt32(Session["loggedusercompanyId"].ToString())).Where(e=>e.IsPOS==false);
            return Json(JsonConvert.SerializeObject(events, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetDocStatus()
        {
            var docstatuses = _blldocStatus.GetActiveDocStatuses();
            return Json(JsonConvert.SerializeObject(docstatuses, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        public class PONos
        {

            public long PurchaseOrderHeaderId { get; set; }

            public string DocumentNo { get; set; }

        }
    }
}