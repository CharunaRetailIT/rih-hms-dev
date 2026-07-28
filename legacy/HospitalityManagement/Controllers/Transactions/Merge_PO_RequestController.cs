using Newtonsoft.Json;
using RIT.HMS.BLL.Common;
using RIT.HMS.BLL.MasterData;
using RIT.HMS.BLL.TransactionData;
using RIT.HMS.Domain;
using RIT.HMS.Domain.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Web.UI.WebControls;
using RIT.HMS.BLL.Configurations;
using System.Transactions;

namespace HospitalityManagement.Controllers.Transactions
{
    public class Merge_PO_RequestController : Controller
    {
        // GET: Merge_PO_Request
        private readonly BLL_Common _bllcommon;
        private readonly BLL_RequestNote _bllrequestNote;
        private readonly BLL_PurchaseOrder _bllpurchaseorder;
        private readonly BLL_Supplier _bllsupplier;
        private BLL_Location _blllocation;
        private BLL_LocationType _blllocationType;
        private BLL_LocationMapper _bLL_LocationMapper;
        private readonly BLL_Configuration _bllconfiguration;
        public object Datatable { get; private set; }
 
        public Merge_PO_RequestController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _bllrequestNote = new BLL_RequestNote(cn);
            _blllocation = new BLL_Location(cn);
            _blllocationType = new BLL_LocationType(cn);
            _bLL_LocationMapper = new BLL_LocationMapper(cn);
            _bllcommon = new BLL_Common(cn);
            _bllpurchaseorder = new BLL_PurchaseOrder(cn);
            _bllconfiguration = new BLL_Configuration(cn);
        }


        public ActionResult Index()
        {
            return View();
        }


        [HttpGet]
        public ActionResult Create()
        {
            var Merge_PO_RequestController = new Mearge_PO_Header();
            return View("~/Views/Transactions/Merge_PO_Request/Merge_PO.cshtml", Merge_PO_RequestController);
        }

        [HttpGet]
        public ActionResult Merge_PO()
        {


            ViewBag.locationId = Convert.ToInt32(Session["loggeduserlocId"]);

            if (Session["MPOFromDate"] != null)
            {
                string Fromdate = Session["MPOFromDate"].ToString().Trim();
                DateTime From_date = new DateTime();
                DateTime.TryParse(Fromdate, out From_date);
                ViewBag.DateFrom = From_date.ToString("yyyy-MM-dd");

            }

            if (Session["MPOToDate"] != null)
            {
                string Todate = Session["MPOToDate"].ToString().Trim();
                DateTime To_date = new DateTime();
                DateTime.TryParse(Todate, out To_date);
                ViewBag.DateTo = To_date.ToString("yyyy-MM-dd");

            }
            if (Session["MPOSelectFromLocation"] != null)

            {
                int fromLocID = 0;
                int.TryParse(Session["MPOSelectFromLocation"].ToString().Trim(), out fromLocID);

                ViewBag.SelectFromLocation = fromLocID;
            }

            if (Session["MPOSelectToLocation"] != null)

            {
                int ToLocID = 0;
                int.TryParse(Session["MPOSelectToLocation"].ToString().Trim(), out ToLocID);

                ViewBag.SelectToLocation = ToLocID;
            }

            if (Session["MPOAppStatus"] != null)

            {
                int AppStatus = 0;
                int.TryParse(Session["MPOAppStatus"].ToString().Trim(), out AppStatus);

                ViewBag.SelectAppStatus = AppStatus;
            }










            return View("~/Views/Transactions/Merge_PO_Request/Merge_PO.cshtml");
        }

       // [HttpGet]
        public JsonResult GetMergePORequestData(long id, long requestidTo,int AppStatus, DateTime from, DateTime to)
        {

            try
            {
                Session["ARNFromDate"] = from;
                Session["ARNToDate"] = to;
                Session["ARNSelectFromLocation"] = id;
                Session["ARNSelectToLocation"] = requestidTo;
                Session["ARNAppStatus"] = AppStatus;

            }
            catch (Exception ex)
            { }



            var requestnotedata = _bllrequestNote.GetLocationWiseRequestPO(id,  requestidTo, from, to, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));

            if(AppStatus==2)
            {

                requestnotedata = requestnotedata.FindAll(x => x.Approve == "Approved").OrderByDescending(x => x.CreateDate).ToList();

            }
            else if(AppStatus == 1)
            {
                requestnotedata = requestnotedata.FindAll(x => x.Approve == "Pending").OrderByDescending(x => x.CreateDate).ToList();
                
            }
            else if (AppStatus == 3)
            {
                requestnotedata = requestnotedata.FindAll(x => x.Approve == "Canceled").OrderByDescending(x => x.CreateDate).ToList();

            }

            //var orderedItems = requestnotedata.OrderBy(item => item.CreateDate).ToList();
            return new JsonResult { Data = requestnotedata, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        // GetApprovedRequestNote

        public JsonResult GetApprovedRequestNote(long id, long requestidTo,int AppStatus, DateTime from, DateTime to)
        {

            try
            {
                Session["MPOFromDate"] = from;
                Session["MPOToDate"] = to;
                Session["MPOSelectFromLocation"] = id;
                Session["MPOSelectToLocation"] = requestidTo;
                Session["MPOAppStatus"] = AppStatus;

            }
            catch (Exception ex)
            { }







            var requestnotedata = _bllrequestNote.GetApprovedRequestData(id, requestidTo, from, to, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));

            if (AppStatus == 2)
            {

                requestnotedata = requestnotedata.FindAll(x => x.Approve == "PO Generated").ToList();

            }
            else if (AppStatus == 1)
            {
                requestnotedata = requestnotedata.FindAll(x => x.Approve == "Pending to Generate PO").ToList();

            }
            var orderedItems = requestnotedata.OrderBy(item => item.CreateDate).ToList();

            ViewBag.locationId = Convert.ToInt32(Session["loggeduserlocId"]);
            return new JsonResult { Data = orderedItems, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }




        //  [Authorize(Roles = "DCreatee")]
        /// [HttpPost]
        /// 
        public bool CreatePO(List<Mearge_PO_Header> POmodes)
        {
            List<RequestNoteParameter> mylist = new List<RequestNoteParameter>();
            bool Check = false;

            if (POmodes != null)
            {
                if (POmodes.Count > 0)
                {
                    int checkcount = 0;

                    foreach (var item in POmodes)
                    {
                        bool isChecked = Convert.ToBoolean(item.IsActive);

                        if (isChecked)
                        {
                            RequestNoteParameter obj = new RequestNoteParameter();
                            var RequestNoteHeaderID1 = _bllrequestNote.GetRequestPOHeaderIDs( item.RequestnoteHeaderId, item.DocumentNo, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));

                            obj.RequestNoteHeaderID = RequestNoteHeaderID1.RequestnoteHeaderId;
                            mylist.Add(obj);
                            checkcount++;

                        }
                    }

                    if (mylist.Count > 0)
                    {
                        DataTable dtsupplier = _bllrequestNote.GetAllSuppliersByDocumentNo(mylist);//get supplier

                        foreach (DataRow row in dtsupplier.Rows)
                        {
                            int supllierID = 0;
                            string RequestNoteDocumentNo = "";
                            int.TryParse(row["SupplierID"].ToString(), out supllierID);
                            //   RequestNoteDocumentNo = Convert.ToString(row["DocumentNo"]);
                            DataTable dtPoData = _bllrequestNote.GetAllPODataBySupplier(mylist, supllierID);
                            Check = GeneratePO(dtPoData, RequestNoteDocumentNo);

                        }
                    }
                    else
                    {
                        POmodes = null;
                    }
                }

            }
            if (Check == true)
            {
                if (POmodes != null)
                {
                    // @ViewBag.Message = "1";
                    return true;
                }
                else
                {
                    //  @ViewBag.Message = "0";
                    return false;

                }
            }
            else
            {
                return false;
            }
          //  return View("~/Views/Transactions/Merge_PO_Request/Merge_PO.cshtml",new List<Mearge_PO_Header>());

        }

        //[HttpGet]
        //public ActionResult Create()
        //{
            
        //            Mearge_PO_Header h = new Mearge_PO_Header();
        //            return View("~/Views/Transactions/Merge_PO_Request/Merge_PO.cshtml", h);
              
        //}


        [HttpPost]
        public ActionResult Create(List<Mearge_PO_Header> POmodes)
        {
            if (POmodes != null)
            {
                if (CreatePO(POmodes) == true)
                {
                    //  @ViewBag.Message = "1";
                    // Mearge_PO_Header h = new Mearge_PO_Header();
                    // return View("~/Views/Transactions/Merge_PO_Request/Merge_PO.cshtml", h);
                   //viewsuccessmessage();
                    return new JsonResult
                    {
                        Data = "Purchase Order Successfully Created.",
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,

                    };
                    //Mearge_PO_Header h = new Mearge_PO_Header();
                    //return View("~/Views/Transactions/Merge_PO_Request/Merge_PO.cshtml", h);
                }
                else
                {
                    return new JsonResult
                    {
                        Data = "Purchase Order Creation Failed. Please Try Again.",
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,

                    };
                }

               
            }
            else
            {

                //var Merge_PO_RequestController = new Mearge_PO_Header();
                return View("~/Views/Transactions/Merge_PO_Request/Merge_PO.cshtml");
            }
            //else
            //    return new JsonResult
            //    {
            //        Data = "Sucessfully Saved",
            //        JsonRequestBehavior = JsonRequestBehavior.AllowGet
            //    };
        //    return View("~/Views/Transactions/Merge_PO_Request/Merge_PO.cshtml");
        }
        public JsonResult viewsuccessmessage()
        {
            return new JsonResult
            {
                Data = "Sucessfully Saved",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,

            };
        }
        private bool Clear(List<Mearge_PO_Header> POmodes)
        {
            POmodes = null;
            return false;
          //  return View("~/Views/Transactions/Merge_PO_Request/Merge_PO.cshtml", POmodes);
        }



        [HttpPost]
        public ActionResult ViewAll(List<Mearge_PO_Header> POmodes)
        {
            Mearge_PO_Header h = new Mearge_PO_Header();
            return View("~/Views/Transactions/Merge_PO_Request/Merge_PO.cshtml", h);
        }
        private bool GeneratePO(DataTable TablePoData, string RequestNoteDocumentNo)
        {
            using (TransactionScope transaction = new TransactionScope(TransactionScopeOption.Required, new System.TimeSpan(0, 15, 0)))
            {
                try
                {
                    PurchaseOrderHeader _poheader = new PurchaseOrderHeader();
                    PurchaseOrderDetail _podetail = new PurchaseOrderDetail();
                    InvRequestNotePOTransaction _RequestNotePOT = new InvRequestNotePOTransaction();

                    if (TablePoData.Rows.Count > 0)
                    {
                        var rowsToRemove = TablePoData.AsEnumerable()
                                    .Where(row => row.Field<decimal>("OrderQty") == 0)
                                    .ToList();

                        foreach (var row in rowsToRemove)
                        {
                            TablePoData.Rows.Remove(row);
                        }

                        decimal GrossAmount = TablePoData.AsEnumerable().Sum(row => row.Field<decimal>("GrossAmount"));
                        int supplierID = Convert.ToInt32(TablePoData.Rows[0]["SupplierID"]);
                        int ToLocationID = Convert.ToInt32(TablePoData.Rows[0]["ToLocationID"]);
                        int LocationID = Convert.ToInt32(TablePoData.Rows[0]["FromLocationID"]);
                        string DocumentNo = TablePoData.Rows[0]["DocumentNo"].ToString();
                        long RequestnoteHeaderId = long.Parse(TablePoData.Rows[0]["RequestnoteHeaderId"].ToString());

                        //Convert.ToDecimal(row["OrderQty"]) * Convert.ToDecimal(row["CostPrice"])

                        decimal TotCostPrice = TablePoData.AsEnumerable().Sum(row => row.Field<decimal>("OrderQty") * row.Field<decimal>("CostPrice"));

                        decimal TotSellingPrice = TablePoData.AsEnumerable().Sum(row => row.Field<decimal>("OrderQty") * row.Field<decimal>("SellingPrice"));


                        int RequestnoteHId = (Int32)_bllrequestNote.GetDocumentID(DocumentNo, RequestnoteHeaderId).RequestnoteHeaderId;
                        var docnum = _bllcommon.GetDocumentNo("Purchase Order",
                                                        Convert.ToInt32(ToLocationID),
                                                        Convert.ToString(ToLocationID),
                                                        3,
                                                        false, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));

                        _poheader.JobClassId = 1;
                        _poheader.DocumentNo = docnum;
                        _poheader.DocumentDate = DateTime.Now;
                        _poheader.SupplierId = supplierID;

                        // Set Expected Date from Request Note Header
                        RequestNoteHeader RH = _bllrequestNote.GetRequestNoteById(RequestnoteHeaderId);
                        if (RH != null)
                        {
                            _poheader.ExpectedDate = RH.ExpectedDeleveryDate;
                        }
                        else
                        {
                            _poheader.ExpectedDate = DateTime.Now;
                        }

                        _poheader.ExpiryDate = DateTime.Now;
                        _poheader.PaymentExpectedDate = DateTime.Now;
                        _poheader.ValidityPeriod = 0;
                        _poheader.GrossAmount = GrossAmount;
                        _poheader.OtherCharges = 0;
                        _poheader.DiscountAmount = 0;
                        _poheader.DiscountPercentage = 0;
                        _poheader.Addition = 0;
                        _poheader.Deduction = 0;
                        _poheader.NetAmount = GrossAmount;
                        _poheader.RequestedBy = Session["loggeduser"].ToString();
                        _poheader.DeliveryLocationId = Convert.ToInt32(LocationID);
                        _poheader.LineDiscountTotal = 0;
                        _poheader.PaymentTermId = 1;
                        _poheader.PaymentPeriod = 0;
                        _poheader.CurrencyId = 22;
                        _poheader.CurrencyRate = 0;
                        _poheader.DeliveryDetail = "";
                        _poheader.ReferenceNo = "";
                        _poheader.Remark = "";
                        _poheader.DocumentStatus = 2;
                        _poheader.LastAuthorizedBy = "";
                        _poheader.LastAuthorizedDate = DateTime.Now;
                        _poheader.GroupOfCompanyID = 1;
                        _poheader.CompanyID = 1;
                        _poheader.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
                        _poheader.CreatedUser = Session["loggeduser"].ToString();
                        _poheader.CreatedDate = DateTime.Now;
                        _poheader.ModifiedUser = Session["loggeduser"].ToString();
                        _poheader.ModifiedDate = DateTime.Now;
                        _poheader.DataTransfer = 0;
                        _poheader.TotalTaxAmount = 0;
                        _poheader.TotSellingPrice = TotSellingPrice;
                        _poheader.TotCostPrice = TotCostPrice;
                        _poheader.TotDiscounts = 0;
                        _poheader.POType = "RequestNoteBased";
                        _poheader.POLocationId = ToLocationID; //Convert.ToInt32(Session["loggeduserlocId"].ToString());
                        _poheader.PaymentMethodId = 65;
                        _poheader.TempDocNumber = "";
                        _poheader.IsTempPO = false;
                        _poheader.IsGRN = false;
                        _poheader.DocumentId = 3;
                        _poheader.IsActive = false;
                        _poheader.DeliveryAddress = "";
                        _poheader.PODate = DateTime.Now;
                        _poheader.EventId = 0;
                        _poheader.NewDocNumber = "";
                        _poheader.RejectReason = "";
                        _poheader.CancelReason = "";
                        _poheader.RequestNoteId = RequestnoteHId;
                        _poheader.ReqDocNo = DocumentNo;
                        _poheader.ReqFromLocation = LocationID;

                        _bllpurchaseorder.SavedPurchaseOrderHeader(_poheader);


                        bool Check = false;
                        int linenumber = 0;



                        if (_bllconfiguration.GetConfiguration("ReqestNoteBasedPOItemMergeEnable", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn)
                        {
                            foreach (DataRow row in TablePoData.Rows)
                            {
                                string DocNo = row["DocumentNo"].ToString();
                                //Check = _bllpurchaseorder.UpdatePurchaseOrderDetail(true, DocNo, ToLocationID, _poheader.CompanyID, Convert.ToInt32(row["ProductID"]));
                                Check = _bllpurchaseorder.UpdatePurchaseOrderDetail(true, DocNo, Convert.ToInt32(row["ToLocationID"]), _poheader.CompanyID, Convert.ToInt32(row["ProductID"]));
                                _RequestNotePOT.RequestNoteHeaderID = Convert.ToInt32(row["RequestnoteHeaderId"]); // _poheader.RequestnoteHeaderId;
                                _RequestNotePOT.PurchaseOrderDetailID = _podetail.PurchaseOrderDetailId;// _podetail.PurchaseOrderDetailId;
                                _RequestNotePOT.PurchaseOrderHeaderID = _poheader.PurchaseOrderHeaderId;
                                _RequestNotePOT.PurchaseOrderDocumentNo = docnum;
                                _RequestNotePOT.RequestNoteDocumentNo = DocNo;
                                _RequestNotePOT.LocationID = Convert.ToInt32(row["ToLocationID"]);
                                _RequestNotePOT.FromLocationID = Convert.ToInt32(row["FromLocationID"]);

                                _RequestNotePOT.ProductID = Convert.ToInt32(row["ProductID"]);
                                _RequestNotePOT.QTY = Convert.ToDecimal(row["OrderQty"]);
                                //string ReqNoteCreatedDate = "2000-01-01"; //DateTime.Now.ToString("yyyy-MM-dd");
                                _RequestNotePOT.ReqNoteCreatedDate = DateTime.Now;
                                _RequestNotePOT.ReqNoteAcceptedDate = DateTime.Now;
                                _RequestNotePOT.POCreateDate = DateTime.Now;
                                _RequestNotePOT.BalanceQtY = Convert.ToDecimal(row["OrderQty"]);
                                _RequestNotePOT.IssueQtY = 0;

                                _bllpurchaseorder.SaveInvRequestNotePOTransaction(_RequestNotePOT);
                                linenumber++;
                            }

                            var result = TablePoData.AsEnumerable()
                           .GroupBy(row => new
                           {
                               ProductID = row.Field<long>("ProductID"),
                               StockCode = row.Field<string>("StockCode")
                           })
                           .Select(g => new
                           {
                               ProductID = g.Key.ProductID,
                               StockCode = g.Key.StockCode,
                               TotalOrderQty = g.Sum(row => row.IsNull("OrderQty") ? 0 : row.Field<decimal>("OrderQty"))
                           });

                            decimal TotCostValue = 0; decimal TotSelleingValue = 0; decimal TotGrossValue = 0;

                            foreach (var item in result)
                            {
                                linenumber = linenumber + 1;
                                _podetail.PurchaseOrderHeaderId = _poheader.PurchaseOrderHeaderId;
                                _podetail.ProductId = Convert.ToInt32(item.ProductID);
                                _podetail.IsBatch = false;
                                _podetail.StockCode = item.StockCode.ToString();
                                _podetail.OrderQty = Convert.ToDecimal(item.TotalOrderQty);
                                _podetail.FreeQty = 0;// PODetail.FreeQty;
                                _podetail.CurrentQty = 0;// PODetail.CurrentQty;
                                _podetail.BalanceQty = 0;// PODetail.BalanceQty;
                                _podetail.BalanceFreeQty = 0;// PODetail.BalanceFreeQty;


                                //  int noofItem = TablePoData.AsEnumerable()
                                //.Select(row => row.Field<long>("ProductID"))
                                //.Where(productId => productId == Convert.ToInt64(item.ProductID))

                                //.Count();

                                decimal totalCostAmount = TablePoData.AsEnumerable()
                                .Where(row => row.Field<long>("productId") == Convert.ToInt64(item.ProductID))
                                    .Sum(row => row.Field<decimal>("CostPrice") * row.Field<decimal>("OrderQty"));

                                decimal UnitCostPriceAvg = 0;
                                if (item.TotalOrderQty != 0)
                                {
                                    UnitCostPriceAvg = totalCostAmount / item.TotalOrderQty;
                                }

                                decimal totalSellingAmount = TablePoData.AsEnumerable()
                                     .Where(row => row.Field<long>("productId") == Convert.ToInt64(item.ProductID))
                                .Sum(row => row.Field<decimal>("SellingPrice") * row.Field<decimal>("OrderQty"));

                                decimal UnitSellingPriceAvg = 0;
                                if (item.TotalOrderQty != 0)
                                {
                                    UnitSellingPriceAvg = totalSellingAmount / item.TotalOrderQty;
                                }

                                TotCostValue = TotCostValue + Math.Round(UnitCostPriceAvg * item.TotalOrderQty, 2);
                                TotSelleingValue = TotSelleingValue + Math.Round(UnitSellingPriceAvg * item.TotalOrderQty, 2);
                                TotGrossValue = TotGrossValue + Math.Round(UnitCostPriceAvg * item.TotalOrderQty, 2);


                                _podetail.CostPrice = Math.Round(UnitCostPriceAvg, 2);
                                _podetail.SellingPrice = Math.Round(UnitSellingPriceAvg, 2);
                                _podetail.PackSize = "0";// PODetail.PackSize;
                                _podetail.UnitOfMeasureId = 0;
                                _podetail.BaseUnitId = 0;// PODetail.BaseUnitId;
                                _podetail.ConvertFactor = 0;// PODetail.ConvertFactor;
                                _podetail.GrossAmount = Math.Round(UnitCostPriceAvg * item.TotalOrderQty, 2); // PODetail.GrossAmount;
                                _podetail.DiscountPercentage = 0;// PODetail.DiscountPercentage;
                                _podetail.DiscountAmount = 0;// PODetail.DiscountAmount;
                                _podetail.SubTotalDiscount = 0;// PODetail.SubTotalDiscount;
                                _podetail.NetAmount = Math.Round(UnitCostPriceAvg * item.TotalOrderQty, 2);// PODetail.NetAmount;
                                _podetail.LineNo = linenumber;
                                _podetail.BatchNo = "0"; // PODetail.BatchNo;
                                _podetail.ProductRemark = "";// PODetail.ProductRemark;
                                _podetail.ItemTaxTotal = 0;// PODetail.ItemTaxTotal;
                                _podetail.CostValue = Math.Round(UnitCostPriceAvg * item.TotalOrderQty, 2); // PODetail.CostValue;
                                _podetail.GRNQuantity = 0;// PODetail.GRNQuantity;
                                _podetail.IsGRN = false;// PODetail.IsGRN;
                                _podetail.LocationID = Convert.ToInt32(ToLocationID);
                                _podetail.CompanyID = _poheader.CompanyID;
                                _podetail.IsTempPO = _poheader.IsTempPO;
                                _podetail.SupplierId = Convert.ToInt32(supplierID);
                                Check = _bllpurchaseorder.SavePurchaseOrderDetail(_podetail);



                                Check = _bllpurchaseorder.UpdateInvRequestNotePOTransactions(_podetail.PurchaseOrderHeaderId, _podetail.ProductId, _podetail.PurchaseOrderDetailId);


                            }


                            Check = _bllpurchaseorder.UpdatePurchaseOrderForAveragePrices(_poheader.PurchaseOrderHeaderId, TotCostValue, TotSelleingValue, TotGrossValue);


                        }

                        else

                        {

                            foreach (DataRow row in TablePoData.Rows)
                            {
                                linenumber = linenumber + 1;
                                _podetail.PurchaseOrderHeaderId = _poheader.PurchaseOrderHeaderId;
                                _podetail.ProductId = Convert.ToInt32(row["ProductID"]);
                                _podetail.IsBatch = false;
                                _podetail.StockCode = row["StockCode"].ToString();
                                _podetail.OrderQty = Convert.ToDecimal(row["OrderQty"]);
                                _podetail.FreeQty = 0;// PODetail.FreeQty;
                                _podetail.CurrentQty = 0;// PODetail.CurrentQty;
                                _podetail.BalanceQty = 0;// PODetail.BalanceQty;
                                _podetail.BalanceFreeQty = 0;// PODetail.BalanceFreeQty;
                                _podetail.CostPrice = Convert.ToDecimal(row["CostPrice"]);
                                _podetail.SellingPrice = Convert.ToDecimal(row["SellingPrice"]);
                                _podetail.PackSize = "0";// PODetail.PackSize;
                                _podetail.UnitOfMeasureId = 0;
                                _podetail.BaseUnitId = 0;// PODetail.BaseUnitId;
                                _podetail.ConvertFactor = 0;// PODetail.ConvertFactor;
                                _podetail.GrossAmount = Convert.ToDecimal(row["GrossAmount"]); // PODetail.GrossAmount;
                                _podetail.DiscountPercentage = 0;// PODetail.DiscountPercentage;
                                _podetail.DiscountAmount = 0;// PODetail.DiscountAmount;
                                _podetail.SubTotalDiscount = 0;// PODetail.SubTotalDiscount;
                                _podetail.NetAmount = Convert.ToDecimal(row["GrossAmount"]);// PODetail.NetAmount;
                                _podetail.LineNo = linenumber;
                                _podetail.BatchNo = "0"; // PODetail.BatchNo;
                                _podetail.ProductRemark = "";// PODetail.ProductRemark;
                                _podetail.ItemTaxTotal = 0;// PODetail.ItemTaxTotal;
                                _podetail.CostValue = Convert.ToDecimal(row["OrderQty"]) * Convert.ToDecimal(row["CostPrice"]);// PODetail.CostValue;
                                _podetail.GRNQuantity = 0;// PODetail.GRNQuantity;
                                _podetail.IsGRN = false;// PODetail.IsGRN;
                                _podetail.LocationID = Convert.ToInt32(ToLocationID);
                                _podetail.CompanyID = _poheader.CompanyID;
                                _podetail.IsTempPO = _poheader.IsTempPO;
                                _podetail.SupplierId = Convert.ToInt32(supplierID);
                                string DocNo = row["DocumentNo"].ToString();
                                _bllpurchaseorder.SavePurchaseOrderDetail(_podetail);
                                // long RequestnoteHeaderId = Convert.ToInt64(row["RequestnoteHeaderId"]);

                                Check = _bllpurchaseorder.UpdatePurchaseOrderDetail(true, DocNo, ToLocationID, _poheader.CompanyID, Convert.ToInt32(row["ProductID"]));

                                _RequestNotePOT.RequestNoteHeaderID = _poheader.PurchaseOrderHeaderId;
                                _RequestNotePOT.PurchaseOrderDetailID = _podetail.PurchaseOrderDetailId;// _podetail.PurchaseOrderDetailId;
                                _RequestNotePOT.PurchaseOrderDocumentNo = docnum;
                                _RequestNotePOT.RequestNoteDocumentNo = DocumentNo;
                                _RequestNotePOT.LocationID = Convert.ToInt32(ToLocationID);
                                _RequestNotePOT.ProductID = Convert.ToInt32(row["ProductID"]);
                                _RequestNotePOT.QTY = Convert.ToDecimal(row["OrderQty"]);
                                //string ReqNoteCreatedDate = "2000-01-01"; //DateTime.Now.ToString("yyyy-MM-dd");
                                _RequestNotePOT.ReqNoteCreatedDate = DateTime.Now;
                                _RequestNotePOT.ReqNoteAcceptedDate = DateTime.Now;
                                _RequestNotePOT.POCreateDate = DateTime.Now;
                                _RequestNotePOT.BalanceQtY = Convert.ToDecimal(row["OrderQty"]);
                                _RequestNotePOT.IssueQtY = 0;

                                _bllpurchaseorder.SaveInvRequestNotePOTransaction(_RequestNotePOT);
                                linenumber++;
                            }


                        }

                    }
                    transaction.Complete();
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Dispose();
                    return false;
                }
            }
        }

    }

    }