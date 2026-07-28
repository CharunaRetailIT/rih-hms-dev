using RIT.HMS.BLL.TransactionData;
using RIT.HMS.Domain.Transactions;
using RIT.HMS.BLL.MasterData;
using RIT.HMS.BLL.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RIT.HMS.Domain.ViewModels;
using RIT.HMS.BLL.Configurations;
using HospitalityManagement.Models;

namespace HospitalityManagement.Controllers.Transactions
{
    [SessionTimeout]
    public class RequestNoteController : Controller
    {
        private readonly BLL_RequestNote _bllrequestNote;
        private readonly BLL_Location _blllocation;
        private readonly BLL_Department _blldepartment;
        private readonly BLL_TransferNote _blltransfernote;
        private readonly BLL_Product _bllproduct;
        private readonly BLL_UnitOfMeasure _bllunitofmeasure;
        private readonly BLL_Configuration _bllconfiguration;
        private readonly BLL_DocStatus _blldocStatus;
        private readonly BLL_Common _bllcommon;
        private readonly BLL_ServingUnit _bllservingunit;
        private AppManager _appmanager;
        private readonly BLL_ProductStock _bllproductstock;
        private readonly BLL_Company _bllcompany;
        private readonly BLL_DocStatus _blldocstatus;
        public RequestNoteController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
           _bllrequestNote = new BLL_RequestNote(cn);
           _blllocation = new BLL_Location(cn);
           _blldepartment = new BLL_Department(cn);
           _blltransfernote = new BLL_TransferNote(cn);
           _bllproduct = new BLL_Product(cn);
           _bllunitofmeasure = new BLL_UnitOfMeasure(cn);
           _bllconfiguration = new BLL_Configuration(cn);
           _blldocStatus = new BLL_DocStatus(cn);
           _bllcommon = new BLL_Common(cn);
           _bllservingunit = new BLL_ServingUnit(cn);
           _appmanager = new AppManager(cn);
           _bllproductstock = new BLL_ProductStock(cn);
           _bllcompany = new BLL_Company(cn);
           _blldocstatus = new BLL_DocStatus(cn);
        }
        public ActionResult CreateRequestNote()
        {
            // check permissions
            //if (!_appmanager.SetPermissions(9, Session["loggeduserempcode"].ToString(), "RQCreatee"))
            //{
            //    @ViewBag.Permissions = "No user permissions to Create Request Notes";
            //    return View("~/Views/Account/AccessDenied.cshtml");
            //}
            //end check permissions

            Session["POWizadUI"] = _bllconfiguration.GetConfiguration("UIWZ", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            Session["APPRN"] = _bllconfiguration.GetConfiguration("APPRN", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            Session["RequestNoteAllowOnlyPO"] = _bllconfiguration.GetConfiguration("RequestNoteAllowOnlyPO", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;

            Session["DisableServingUnit"] = _bllconfiguration.GetConfiguration("DisableServingUnit", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            Session["DisableRequestedBy"] = _bllconfiguration.GetConfiguration("DisableRequestedBy", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;

            Session["DisablePriceRequestNote"] = _bllconfiguration.GetConfiguration("DisablePriceRequestNote", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;

            Session["EnableRequestNoteLineRemark"] = _bllconfiguration.GetConfiguration("EnableRequestNoteLineRemark", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            Session["EnableRequestNoteCancelOnly"] = _bllconfiguration.GetConfiguration("EnableRequestNoteCancelOnly", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;

            Session["DisableFromDepartmentAndToDepartment"] = _bllconfiguration.GetConfiguration("DisableFromDepartmentAndToDepartment", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;



            var requestnoteheader = new RequestNoteHeader();

            int docid = _bllcommon.GetDcumentId("Request", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            var docnum = _bllcommon.GetDocumentTemporyNo("Request", Convert.ToInt32(Session["loggeduserlocId"]), "1", docid, true, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            requestnoteheader.DocumentNo = docnum;
            ViewBag.locationId = Convert.ToInt32(Session["loggeduserlocId"]);
            ViewBag.toLocationId = 0;
            ViewBag.ToDepartmentId = 0;
            bool DisableFromDepartmentAndToDepartment = _bllconfiguration.GetConfiguration("DisableFromDepartmentAndToDepartment", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            if (DisableFromDepartmentAndToDepartment == true)
            {
                ViewBag.DisableFromDepartmentAndToDepartment = DisableFromDepartmentAndToDepartment;
            }
            else
            {
                ViewBag.DisableFromDepartmentAndToDepartment = DisableFromDepartmentAndToDepartment;
            }
 
            return View("~/Views/Transactions/RequestNote/CreateRequestNote.cshtml", requestnoteheader);
        }
  
        public JsonResult GetMergePORequestData(long fromlocation, DateTime from, DateTime to)
        {

            var requestnotedata = _bllrequestNote.GetRequestedNoteLocationWise(fromlocation, from, to, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            return new JsonResult { Data = requestnotedata, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpGet]

        public ActionResult ViewAllRequestNotes()
        {
            var UserLocationID = Convert.ToInt32(Session["loggeduserlocId"]);

            if (Session["FromDate"] != null)
            {
                string Fromdate = Session["FromDate"].ToString().Trim();
                DateTime From_date = new DateTime();
                DateTime.TryParse(Fromdate, out From_date);
                ViewBag.DateFrom = From_date.ToString("yyyy-MM-dd");

            }

            if (Session["ToDate"] != null)
            {
                string Todate = Session["ToDate"].ToString().Trim();
                DateTime To_date = new DateTime();
                DateTime.TryParse(Todate, out To_date);
                ViewBag.DateTo = To_date.ToString("yyyy-MM-dd");

            }
            if(Session["SelectFromLocation"] != null)
               
            {
                int fromLocID = 0;
                int.TryParse(Session["SelectFromLocation"].ToString().Trim(), out fromLocID);

                ViewBag.SelectFromLocation = fromLocID;
            }

            if (Session["SelectToLocation"] != null)

            {
                int ToLocID = 0;
                int.TryParse(Session["SelectToLocation"].ToString().Trim(), out ToLocID);

                ViewBag.SelectToLocation = ToLocID;
            }

            if (Session["AppStatus"] != null)

            {
                int AppStatus = 0;
                int.TryParse(Session["AppStatus"].ToString().Trim(), out AppStatus);

                ViewBag.SelectAppStatus = AppStatus;
            }



            ViewBag.locationId = UserLocationID;
           

               
            return View("~/Views/Transactions/RequestNote/ViewRequestsNotes.cshtml");
        }

        //  [Authorize(Roles = "RQView")]
        [HttpGet]
        public ActionResult ViewAllRequestNotes1(long id, long requestidTo, int AppStatus, DateTime from, DateTime to)
        {
            try
            {
                Session["FromDate"] = from;
                Session["ToDate"] = to;
                Session["SelectFromLocation"] = id;
                Session["SelectToLocation"] = requestidTo;
                Session["AppStatus"] = AppStatus;

            }          
            catch (Exception ex)
            { }

            // check permissions
            if (!_appmanager.SetPermissions(9, Session["loggeduserempcode"].ToString(), "RQView"))
            {
                @ViewBag.Permissions = "No user permissions to View Request Notes";
                return View("~/Views/Account/AccessDenied.cshtml");
            }
            //end check permissions


            var requestnotedata = _bllrequestNote.GetRequestNoteDetailsView(id, requestidTo, from, to, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));

            if (AppStatus == 2)
            {

                requestnotedata = requestnotedata.FindAll(x => x.Approve == "Approved").ToList();

            }
            else if (AppStatus == 1)
            {
                requestnotedata = requestnotedata.FindAll(x => x.Approve == "Pending" || x.Approve== "Pending [Tempory Saved]").ToList();

            }

            else if (AppStatus == 3)
            {
                requestnotedata = requestnotedata.FindAll(x => x.Approve == "Cancelled").ToList();

            }
            //var orderedItems = requestnotedata.OrderBy(item => item.CreateDate).ToList();
            return new JsonResult { Data = requestnotedata, JsonRequestBehavior = JsonRequestBehavior.AllowGet };


        }

        [HttpGet]
        public JsonResult GetActiveInterDepartments()
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var interdepts = _bllrequestNote.GetActiveInterDepartments(companyid);
            return Json(JsonConvert.SerializeObject(interdepts, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }
 

        





        //  [Authorize(Roles = "RQCreatee")]
        [HttpPost]
        public ActionResult CreateRequestNote(RequestNoteHeader header)
        {

            Session["APPRN"] = _bllconfiguration.GetConfiguration("APPRN", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;

            Session["RequestNoteAllowOnlyPO"] = _bllconfiguration.GetConfiguration("RequestNoteAllowOnlyPO", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;



            Session["DisableServingUnit"] = _bllconfiguration.GetConfiguration("DisableServingUnit", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            Session["DisableRequestedBy"] = _bllconfiguration.GetConfiguration("DisableRequestedBy", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            Session["DisablePriceRequestNote"] = _bllconfiguration.GetConfiguration("DisablePriceRequestNote", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            Session["EnableRequestNoteLineRemark"] = _bllconfiguration.GetConfiguration("EnableRequestNoteLineRemark", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;

            Session["EnableRequestNoteCancelOnly"] = _bllconfiguration.GetConfiguration("EnableRequestNoteCancelOnly", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            Session["DisableFromDepartmentAndToDepartment"] = _bllconfiguration.GetConfiguration("DisableFromDepartmentAndToDepartment", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;




            // check permissions
            if (!_appmanager.SetPermissions(9, Session["loggeduserempcode"].ToString(), "RQCreatee"))
                {
                    @ViewBag.Permissions = "No user permissions to Create Request Notes";
                    return View("~/Views/Account/AccessDenied.cshtml");
                }
            //end check permissions
         
            foreach (var item in header.RequestDetail)
            {

                var x = _bllproduct.GetProductBySupplierId(item.ProductId);
                if (x != null)
                {
                    var supplierID = _bllproduct.GetProductBySupplierId(item.ProductId).SupplierId;
                    item.SupplierId = supplierID;
                }
                else
                {
                    item.SupplierId = 0;
                }


         
            }
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            if (!ModelState.IsValid)
            {
                var deptdata = _bllproduct.GetActiveProducts(companyid);
                ViewBag.locationId = header.FromLocationId;
                ViewBag.FromDepartmentId =  header.FromDepartmentId;
                ViewBag.toLocationId = header.ToLocationId;
                ViewBag.ToDepartmentId =  header.ToDepartmentId;
                ViewBag.productdata = deptdata;
                return View("~/Views/Transactions/RequestNote/CreateRequestNote.cshtml", header);
            }

            bool DisableFromDepartmentAndToDepartment = _bllconfiguration.GetConfiguration("DisableFromDepartmentAndToDepartment", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            if (DisableFromDepartmentAndToDepartment == true)
            {
                ViewBag.DisableFromDepartmentAndToDepartment = DisableFromDepartmentAndToDepartment;

                if (header.FromLocationId == 0  || header.ToLocationId == 0)
                {
                    var deptdata = _bllproduct.GetActiveProducts(companyid);
                    ViewBag.locationId = header.FromLocationId;
                    ViewBag.FromDepartmentId = header.FromDepartmentId;
                    ViewBag.toLocationId = header.ToLocationId;
                    ViewBag.ToDepartmentId = header.ToDepartmentId;
                    ViewBag.productdata = deptdata;
                    return View("~/Views/Transactions/RequestNote/CreateRequestNote.cshtml", header);
                }
                if (header.RequestDetail.Count == 0)
                {
                    var deptdata = _bllproduct.GetActiveProducts(companyid);
                    ViewBag.locationId = header.FromLocationId;
                    ViewBag.FromDepartmentId = header.FromDepartmentId;
                    ViewBag.toLocationId = header.ToLocationId;
                    ViewBag.ToDepartmentId = header.ToDepartmentId;
                    ViewBag.productdata = deptdata;
                    @ViewBag.Message = "5";
                    ModelState.AddModelError("RequestDetail", "Please enter request details !");
                    return View("~/Views/Transactions/RequestNote/CreateRequestNote.cshtml", header);
                }
            }
            else
            {
                ViewBag.DisableFromDepartmentAndToDepartment = DisableFromDepartmentAndToDepartment;

                if (header.FromLocationId == 0 || header.FromDepartmentId == 0 || header.ToLocationId == 0 || header.ToDepartmentId == 0)
                {
                    var deptdata = _bllproduct.GetActiveProducts(companyid);
                    ViewBag.locationId = header.FromLocationId;
                    ViewBag.FromDepartmentId = header.FromDepartmentId;
                    ViewBag.toLocationId = header.ToLocationId;
                    ViewBag.ToDepartmentId = header.ToDepartmentId;
                    ViewBag.productdata = deptdata;
                    return View("~/Views/Transactions/RequestNote/CreateRequestNote.cshtml", header);
                }
                if (header.RequestDetail.Count == 0)
                {
                    var deptdata = _bllproduct.GetActiveProducts(companyid);
                    ViewBag.locationId = header.FromLocationId;
                    ViewBag.FromDepartmentId = header.FromDepartmentId;
                    ViewBag.toLocationId = header.ToLocationId;
                    ViewBag.ToDepartmentId = header.ToDepartmentId;
                    ViewBag.productdata = deptdata;
                    @ViewBag.Message = "5";
                    ModelState.AddModelError("RequestDetail", "Please enter request details !");
                    return View("~/Views/Transactions/RequestNote/CreateRequestNote.cshtml", header);
                }
            }


            
            
            header.IsTempRequest = false;
            header.DocumentDate = DateTime.Now;
            header.IsPoTransfer = false;
            if (Convert.ToBoolean(Session["APPRN"]) == true)
            {
                header.DocumentStatus = 2;
            }
            else
            {
                header.DocumentStatus = 3;
            }
            int docid = _bllcommon.GetDcumentId("Request", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            var docnum = _bllcommon.GetDocumentNumber("Request", Convert.ToInt32(Session["loggeduserlocId"]), "1", docid, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));

            header.DocumentId = docid;
            header.DocumentNo = docnum;
            header.IsTempRequest = false;
            header.CompanyId = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            @ViewBag.Code = header.DocumentNo;
            if (_bllrequestNote.SubmitRequest(header))
            {
                if (header.DocumentStatus == 3)
                {
                    RequestNoteAccptanceHeader accheader = new RequestNoteAccptanceHeader();
                    accheader.DocumentDate = DateTime.Now;
                    accheader.FromLocationId = header.FromLocationId;
                    accheader.FromDepartmentId = header.FromDepartmentId;
                    accheader.DocumentNo = header.DocumentNo;
                    var requestnotedata = _bllrequestNote.GetReceipesByRequestId(header.RequestnoteHeaderId, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                    List<RequestNoteAcceptanceDetail> acceptanceDetail = new List<RequestNoteAcceptanceDetail>();
                    var LineNo = 0;
                    foreach (var item in requestnotedata) 
                    {
                        var vm = new RequestNoteAcceptanceDetail();
                        LineNo = LineNo + 1;
                        vm.RequestNoteAccptanceHeaderId = header.RequestnoteHeaderId;
                        vm.LineNo = LineNo;
                        vm.ProductId = item.ProductId;
                        vm.MaterialId = item.MaterialId;
                        vm.CostPrice = item.MaterialCostPrice;
                        vm.SellingPrice = item.MaterialSellingPrice;
                        vm.MaterialQty = item.MaterialQuantity;
                        vm.IssueQty = item.MaterialQuantity;
                        vm.UnitOfMeasureId = item.MaterialUOMId;
                        vm.ProductName = item.ProductName;

                        accheader.AcceptanceDetail.Add(vm);
                    }
                    accheader.CompanyId = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
                    if (_bllrequestNote.AcceptRequest(accheader))
                    { }
                }
                @ViewBag.Message = "1";
                ModelState.Clear();
                var deptdata = _bllproduct.GetActiveProducts(companyid);
                ViewBag.productdata = deptdata;
                var requestnoteheader = new RequestNoteHeader();
                docid = _bllcommon.GetDcumentId("Request", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                docnum = _bllcommon.GetDocumentNo("Request",
                                                Convert.ToInt32(Session["loggeduserlocId"]),
                                                Convert.ToString(header.FromLocationId),
                                                docid, 
                                                false, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                //docnum = _bllcommon.GetDocumentTemporyNo("Request", 
                //                                            Convert.ToInt32(Session["loggeduserlocId"]), 
                //                                            "1",
                //                                            docid, 
                //                                            true);
                requestnoteheader.DocumentNo = docnum;
                ViewBag.locationId = Convert.ToInt32(Session["loggeduserlocId"]);
                return View("~/Views/Transactions/RequestNote/CreateRequestNote.cshtml", requestnoteheader);
            }
            else
            {
                var deptdata = _bllproduct.GetActiveProducts(companyid);
                ViewBag.locationId = header.FromLocationId;
                ViewBag.FromDepartmentId = header.FromDepartmentId;
                ViewBag.toLocationId = header.ToLocationId;
                ViewBag.ToDepartmentId = header.ToDepartmentId;
                ViewBag.productdata = deptdata;
                @ViewBag.Message = "3";
                return View("~/Views/Transactions/RequestNote/CreateRequestNote.cshtml", header);
            }
        }
        //     [Authorize(Roles = "RQEdit")]

        public ActionResult EditPendingRequerst(string requestnoteHeaderId)
        
        
        {
            var requestnotedata = _bllrequestNote.GetPendingRequest( long.Parse(requestnoteHeaderId));
            Session["EnableRequestNoteCancelOnly"] = _bllconfiguration.GetConfiguration("EnableRequestNoteCancelOnly", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            ViewBag.documentNo = requestnotedata.DocumentNo;
            return View("~/Views/Transactions/RequestNote/EditPendingRequestNotes.cshtml", requestnotedata);
        }


        public ActionResult ViewSelectedRequestnote(string requestnoteHeaderId)


        {

            Session["DisableServingUnit"] = _bllconfiguration.GetConfiguration("DisableServingUnit", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            Session["DisableRequestedBy"] = _bllconfiguration.GetConfiguration("DisableRequestedBy", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;

            Session["DisablePriceRequestNote"] = _bllconfiguration.GetConfiguration("DisablePriceRequestNote", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;

            var requestnotedata = _bllrequestNote.GetPendingRequest(long.Parse(requestnoteHeaderId));
            Session["EnableRequestNoteCancelOnly"] = _bllconfiguration.GetConfiguration("EnableRequestNoteCancelOnly", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            ViewBag.documentNo = requestnotedata.DocumentNo;
            return View("~/Views/Transactions/RequestNote/ViewSelectedRequestNote.cshtml", requestnotedata);
        }

        public ActionResult Edit(long id)
        {
           
            // check permissions
            //if (!_appmanager.SetPermissions(9, Session["loggeduserempcode"].ToString(), "RQEdit"))
            //{
            //    @ViewBag.Permissions = "No user permissions to Edit Request Notes";
            //    return View("~/Views/Account/AccessDenied.cshtml");
            //}
            //end check permissions

            Session["APPRN"] = _bllconfiguration.GetConfiguration("APPRN", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;

            Session["RequestNoteAllowOnlyPO"] = _bllconfiguration.GetConfiguration("RequestNoteAllowOnlyPO", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;



            Session["DisableServingUnit"] = _bllconfiguration.GetConfiguration("DisableServingUnit", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            Session["DisableRequestedBy"] = _bllconfiguration.GetConfiguration("DisableRequestedBy", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            Session["DisablePriceRequestNote"] = _bllconfiguration.GetConfiguration("DisablePriceRequestNote", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            Session["EnableRequestNoteLineRemark"] = _bllconfiguration.GetConfiguration("EnableRequestNoteLineRemark", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;

            Session["EnableRequestNoteCancelOnly"] = _bllconfiguration.GetConfiguration("EnableRequestNoteCancelOnly", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;


            Session["DisableFromDepartmentAndToDepartment"] = _bllconfiguration.GetConfiguration("DisableFromDepartmentAndToDepartment", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;

            var dbheader = _bllrequestNote.GetRequestNoteById(id);
            dbheader.IsCreateBasedOnThis = false;
            foreach (var d in dbheader.RequestDetail)
            {
                d.UOM = _bllunitofmeasure.GetUnitOfMeasureById(_bllproduct.GetProductById(d.ProductId).PurchasingUnit).UnitOfMeasureName;

               d.SIH = _bllproductstock.GetProductStockMasterByProductIdLocid(d.ProductId, Convert.ToInt32(dbheader.FromLocationId)).Stock;
                d.CostValue = d.CostPrice * d.RequestQty;

            }
            foreach (var item in dbheader.RequestDetail)
            {

                //var supplierID = _bllproduct.GetProductBySupplierId(item.ProductId).SupplierId;
                //item.SupplierId = supplierID;

                var x = _bllproduct.GetProductBySupplierId(item.ProductId);
                if (x != null)
                {
                    var supplierID = _bllproduct.GetProductBySupplierId(item.ProductId).SupplierId;
                    item.SupplierId = supplierID;
                }
                else
                {
                    item.SupplierId = 0;
                }


            }
            ViewBag.locationId = dbheader.FromLocationId;
            ViewBag.ToLocationId = dbheader.ToLocationId;
            ViewBag.FromDepartmentId = dbheader.FromDepartmentId;
            ViewBag.ToDepartmentId = dbheader.ToDepartmentId;

            bool DisableFromDepartmentAndToDepartment = _bllconfiguration.GetConfiguration("DisableFromDepartmentAndToDepartment", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            if (DisableFromDepartmentAndToDepartment == true)
            {
                ViewBag.DisableFromDepartmentAndToDepartment = DisableFromDepartmentAndToDepartment;
            }
            else
            {
                ViewBag.DisableFromDepartmentAndToDepartment = DisableFromDepartmentAndToDepartment;
            }


            try
            {
                return View("~/Views/Transactions/RequestNote/EditRequestNotes.cshtml", dbheader);
            }
            catch (Exception ex)
            {
                // Log exception or handle it
                return new HttpStatusCodeResult(500, "Internal server error");
            }
        }
        //  [Authorize(Roles = "RQEdit")]


        public ActionResult CreateNewbasedOn(long id)
        {

            // check permissions
            //if (!_appmanager.SetPermissions(9, Session["loggeduserempcode"].ToString(), "RQEdit"))
            //{
            //    @ViewBag.Permissions = "No user permissions to Edit Request Notes";
            //    return View("~/Views/Account/AccessDenied.cshtml");
            //}
            //end check permissions

            Session["APPRN"] = _bllconfiguration.GetConfiguration("APPRN", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;

            Session["RequestNoteAllowOnlyPO"] = _bllconfiguration.GetConfiguration("RequestNoteAllowOnlyPO", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;


            Session["DisableServingUnit"] = _bllconfiguration.GetConfiguration("DisableServingUnit", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            Session["DisableRequestedBy"] = _bllconfiguration.GetConfiguration("DisableRequestedBy", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            Session["DisablePriceRequestNote"] = _bllconfiguration.GetConfiguration("DisablePriceRequestNote", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            Session["EnableRequestNoteLineRemark"] = _bllconfiguration.GetConfiguration("EnableRequestNoteLineRemark", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            Session["EnableRequestNoteCancelOnly"] = _bllconfiguration.GetConfiguration("EnableRequestNoteCancelOnly", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            Session["DisableFromDepartmentAndToDepartment"] = _bllconfiguration.GetConfiguration("DisableFromDepartmentAndToDepartment", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;

            var dbheader = _bllrequestNote.GetRequestNoteById(id);
            dbheader.IsCreateBasedOnThis = true;
            foreach (var d in dbheader.RequestDetail)
            {
                d.UOM = _bllunitofmeasure.GetUnitOfMeasureById(_bllproduct.GetProductById(d.ProductId).PurchasingUnit).UnitOfMeasureName;

                d.SIH = _bllproductstock.GetProductStockMasterByProductIdLocid(d.ProductId, Convert.ToInt32(dbheader.FromLocationId)).Stock;
                d.CostValue = d.CostPrice * d.RequestQty;

            }
            foreach (var item in dbheader.RequestDetail)
            {

                // var supplierID = _bllproduct.GetProductBySupplierId(item.ProductId).SupplierId;
                // item.SupplierId = supplierID;

                var x = _bllproduct.GetProductBySupplierId(item.ProductId);
                if (x != null)
                {
                    var supplierID = _bllproduct.GetProductBySupplierId(item.ProductId).SupplierId;
                    item.SupplierId = supplierID;
                }
                else
                {
                    item.SupplierId = 0;
                }


            }
            ViewBag.locationId = dbheader.FromLocationId;
            ViewBag.ToLocationId = dbheader.ToLocationId;
            ViewBag.FromDepartmentId = dbheader.FromDepartmentId;
            ViewBag.ToDepartmentId = dbheader.ToDepartmentId;

            dbheader.BasedOnRequestNoteNo = dbheader.DocumentNo;
            int docid = _bllcommon.GetDcumentId("Request", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            var docnum = _bllcommon.GetDocumentTemporyNo("Request", Convert.ToInt32(Session["loggeduserlocId"]), "1", docid, true, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            dbheader.DocumentNo = docnum;


            bool DisableFromDepartmentAndToDepartment = _bllconfiguration.GetConfiguration("DisableFromDepartmentAndToDepartment", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            if (DisableFromDepartmentAndToDepartment == true)
            {
                ViewBag.DisableFromDepartmentAndToDepartment = DisableFromDepartmentAndToDepartment;
            }
            else
            {
                ViewBag.DisableFromDepartmentAndToDepartment = DisableFromDepartmentAndToDepartment;
            }


            try
            {
                return View("~/Views/Transactions/RequestNote/EditRequestNotes.cshtml", dbheader);
            }
            catch (Exception ex)
            {
                // Log exception or handle it
                return new HttpStatusCodeResult(500, "Internal server error");
            }
        }

        public ActionResult ModifyPendingRequestNote()
        {
            return View("~/Views/Transactions/RequestNote/EditPendingRequestNotes.cshtml");
        }
        [HttpPost]
        public ActionResult ModifyPendingRequestNote(RequestNoteAccptanceHeader accheader)
        {
            if (ModelState.IsValid)
            {
                ViewBag.Code = accheader.DocumentNo;
             

                if (_bllrequestNote.AcceptRequest(accheader))
                {
                    ModelState.Clear();
                    ViewBag.Message = "1";
                    ViewBag.locationId = Convert.ToInt32(Session["loggeduserlocId"]);
                    ViewBag.FromDepartmentId = 0;
                    ViewBag.RequestNoteId = 0;

                    var newAccHeader = new RequestNoteAccptanceHeader();
                    newAccHeader.DocumentNo = accheader.DocumentNo;
                    newAccHeader.DocumentDate = accheader.DocumentDate;
                    ViewBag.Code = accheader.DocumentNo;
                   // accheader.DocumentNo = "";
                    TempData["SuccessMessage"] = "Request approved successfully!";
                    //return View("~/Views/Transactions/RequestNote/EditPendingRequestNotes.cshtml");

                    var requestnotedata = _bllrequestNote.GetPendingRequest(accheader.RequestnoteHeaderId);

                    ViewBag.documentNo = accheader.DocumentNo;

                    return View("~/Views/Transactions/RequestNote/EditPendingRequestNotes.cshtml", requestnotedata);
                }
                else
                {


                    var errors = ModelState
        .Where(ms => ms.Value.Errors.Any())
        .Select(ms => new
        {
            Key = ms.Key,
            Errors = ms.Value.Errors.Select(e => e.ErrorMessage).ToList()
        })
        .ToList();


                    ViewBag.locationId = accheader.FromLocationId;
                    ViewBag.FromDepartmentId = accheader.FromDepartmentId;
                    ViewBag.RequestNoteId = accheader.DocumentNo;
                    ViewBag.Message = "3";

                    return View("~/Views/Transactions/RequestNote/EditPendingRequestNotes.cshtml", accheader);
                }
            }
       
            // Handle invalid ModelState case here if needed
            return View(accheader);
        }

        [HttpPost]
        public ActionResult ModifyRequestNote(RequestNoteHeader header)
        {
            // check permissions
            if (!_appmanager.SetPermissions(9, Session["loggeduserempcode"].ToString(), "RQEdit"))
            {
                @ViewBag.Permissions = "No user permissions to Edit Request Notes";
                return View("~/Views/Account/AccessDenied.cshtml");
            }
            //end check permissions
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            if (!ModelState.IsValid)
            {
                var deptdata = _bllproduct.GetActiveProducts(companyid);
                ViewBag.locationId = header.FromLocationId;
                ViewBag.ToLocationId = header.ToLocationId;
                ViewBag.FromDepartmentId = header.FromDepartmentId;
                ViewBag.ToDepartmentId = header.ToDepartmentId;
                ViewBag.productdata = deptdata;
                return View("~/Views/Transactions/RequestNote/EditRequestNotes.cshtml", header);
            }
            if (header.RequestDetail.Count == 0)
            {
                var deptdata = _bllproduct.GetActiveProducts(companyid);
                ViewBag.locationId = header.FromLocationId;
                ViewBag.FromDepartmentId = header.FromDepartmentId;
                ViewBag.toLocationId = header.ToLocationId;
                ViewBag.ToDepartmentId = header.ToDepartmentId;
                ViewBag.productdata = deptdata;
                @ViewBag.Message = "5";
                ModelState.AddModelError("RequestDetail", "Please enter request details !");
                return View("~/Views/Transactions/RequestNote/EditRequestNotes.cshtml", header);
            }
            var dbheader = _bllrequestNote.GetRequestNoteById(header.RequestnoteHeaderId).DocumentStatus;
            header.DocumentStatus = dbheader;
            int docid = 0;
            var docnum = "";
            if (header.DocumentStatus != 4)
            {
                docid = _bllcommon.GetDcumentId("Request", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                docnum = _bllcommon.GetDocumentNumber("Request", 
                                                    Convert.ToInt32(Session["loggeduserlocId"]), 
                                                    "1", 
                                                    docid, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                header.DocumentId = docid;
                header.DocumentNo = docnum;
            }

            foreach (var item in header.RequestDetail)
            {

                //  var supplierID = _bllproduct.GetProductBySupplierId(item.ProductId).SupplierId;
                //  item.SupplierId = supplierID;


                var x = _bllproduct.GetProductBySupplierId(item.ProductId);
                if (x != null)
                {
                    var supplierID = _bllproduct.GetProductBySupplierId(item.ProductId).SupplierId;
                    item.SupplierId = supplierID;
                }
                else
                {
                    item.SupplierId = 0;
                }


            }

            if (Convert.ToBoolean(Session["APPRN"]) == true)
            {
                header.DocumentStatus = 2;
            }
            else
            {
                header.DocumentStatus = 3;
            }
            header.IsTempRequest = false;
            @ViewBag.Code = header.DocumentNo;
            header.CompanyId = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            if (_bllrequestNote.ModifyRequest(header))
            {
                if (header.DocumentStatus == 3)
                {
                    RequestNoteAccptanceHeader accheader = new RequestNoteAccptanceHeader();
                    accheader.DocumentDate = DateTime.Now;
                    accheader.FromLocationId = header.FromLocationId;
                    accheader.FromDepartmentId = header.FromDepartmentId;
                    accheader.DocumentNo = header.DocumentNo;
                    var requestnotedata = _bllrequestNote.GetReceipesByRequestId(header.RequestnoteHeaderId, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                    List<RequestNoteAcceptanceDetail> acceptanceDetail = new List<RequestNoteAcceptanceDetail>();
                    var LineNo = 0;
                    foreach (var item in requestnotedata)
                    {
                        var vm = new RequestNoteAcceptanceDetail();
                        LineNo = LineNo + 1;
                        vm.RequestNoteAccptanceHeaderId = header.RequestnoteHeaderId;
                        vm.LineNo = LineNo;
                        vm.ProductId = item.ProductId;
                        vm.MaterialId = item.MaterialId;
                        vm.CostPrice = item.MaterialCostPrice;
                        vm.SellingPrice = item.MaterialSellingPrice;
                        vm.MaterialQty = item.MaterialQuantity;
                        vm.IssueQty = item.MaterialQuantity;
                        vm.UnitOfMeasureId = item.MaterialUOMId;
                        vm.ProductName = item.ProductName;

                        accheader.AcceptanceDetail.Add(vm);
                    }
                    if (_bllrequestNote.AcceptRequest(accheader))
                    { }
                }
                docid = _bllcommon.GetDcumentId("Request", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                if (dbheader != 4)
                {
                    docnum = _bllcommon.GetDocumentNo("Request", 
                        Convert.ToInt32(Session["loggeduserlocId"]),
                        Convert.ToString(header.FromLocationId),
                        docid, 
                        false, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                }
                ModelState.Clear();

                ViewBag.Message = "1";
                ViewBag.locationId = Convert.ToInt32(Session["loggeduserlocId"]);
                ViewBag.toLocationId = 0;
                ViewBag.ToDepartmentId = 0;
                var requestnoteheader = new RequestNoteHeader();
                bool DisableFromDepartmentAndToDepartment = _bllconfiguration
              .GetConfiguration("DisableFromDepartmentAndToDepartment", Convert.ToInt32(Session["loggedusercompanyId"].ToString()))
              .ConfigurationOn;

                if (DisableFromDepartmentAndToDepartment)
                {
                    docnum = _bllcommon.GetDocumentTemporyNo("Request", Convert.ToInt32(Session["loggeduserlocId"]), "1", docid, true, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                    requestnoteheader.DocumentNo = docnum;
                    ViewBag.DisableFromDepartmentAndToDepartment = DisableFromDepartmentAndToDepartment;
                    return View("~/Views/Transactions/RequestNote/CreateRequestNote.cshtml", requestnoteheader);
                }
                else
                {
                    docnum = _bllcommon.GetDocumentTemporyNo("Request", Convert.ToInt32(Session["loggeduserlocId"]), "1", docid, true, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                    requestnoteheader.DocumentNo = docnum;
                    ViewBag.DisableFromDepartmentAndToDepartment = DisableFromDepartmentAndToDepartment;
                    return View("~/Views/Transactions/RequestNote/CreateRequestNote.cshtml", requestnoteheader);
                }
            }
            else
            {
                var deptdata = _bllproduct.GetActiveProducts(companyid);
                ViewBag.locationId = header.FromLocationId;
                ViewBag.FromDepartmentId = header.FromDepartmentId;
                ViewBag.toLocationId = header.ToLocationId;
                ViewBag.ToDepartmentId = header.ToDepartmentId;
                ViewBag.productdata = deptdata;
                ViewBag.Message = "3";
                return View("~/Views/Transactions/RequestNote/EditRequestNotes.cshtml", header);
            }
        }

        [HttpPost]
        public ActionResult CancelRequestNoteAccept(RequestNoteAccptanceHeader accheader)
        {
            if (ModelState.IsValid)
            {
                ViewBag.Code = accheader.DocumentNo;

             
                bool DisableFromDepartmentAndToDepartment = _bllconfiguration
           
                    .GetConfiguration("DisableFromDepartmentAndToDepartment", Convert.ToInt32(Session["loggedusercompanyId"].ToString()))
           
                    .ConfigurationOn;
            
                ViewBag.DisableFromDepartmentAndToDepartment = DisableFromDepartmentAndToDepartment;
              
                @ViewBag.Code = accheader.DocumentNo;
                var ReqHeader = _bllrequestNote.GetPendingRequest(accheader.RequestnoteHeaderId);

                ReqHeader.DocumentStatus = 4;
                ReqHeader.CanceleddBy = Session["loggeduser"].ToString();
                ReqHeader.CanceledDate = DateTime.Now;


                if (_bllrequestNote.CancelRequest(ReqHeader))
                  //  if (_bllrequestNote.AcceptRequest(accheader))
                {
                    ModelState.Clear();
                    ViewBag.Message = "2";
                    ViewBag.locationId = Convert.ToInt32(Session["loggeduserlocId"]);
                    ViewBag.FromDepartmentId = 0;
                    ViewBag.RequestNoteId = 0;

                    var newAccHeader = new RequestNoteAccptanceHeader();
                    newAccHeader.DocumentNo = accheader.DocumentNo;
                    newAccHeader.DocumentDate = accheader.DocumentDate;
                    ViewBag.Code = accheader.DocumentNo;
                    // accheader.DocumentNo = "";
                    TempData["SuccessMessage"] = "Request note successfully canceled.";
                    //return View("~/Views/Transactions/RequestNote/EditPendingRequestNotes.cshtml");

                    var requestnotedata = _bllrequestNote.GetPendingRequest(accheader.RequestnoteHeaderId);

                    ViewBag.documentNo = accheader.DocumentNo;

                    return View("~/Views/Transactions/RequestNote/EditPendingRequestNotes.cshtml", requestnotedata);
                }
                else
                {
                    ViewBag.locationId = accheader.FromLocationId;
                    ViewBag.FromDepartmentId = accheader.FromDepartmentId;
                    ViewBag.RequestNoteId = accheader.DocumentNo;
                    ViewBag.Message = "3";

                    return View("~/Views/Transactions/RequestNote/EditPendingRequestNotes.cshtml", accheader);
                }

                
            }
            else
            {

                var errors = ModelState.Values.SelectMany(v => v.Errors);

                foreach (var error in errors)
                {
                    // You can log the error or add it to a view model
                    Console.WriteLine(error.ErrorMessage);
                }


                //ViewBag.locationId = accheader.FromLocationId;
                //ViewBag.FromDepartmentId = accheader.FromDepartmentId;
                //ViewBag.RequestNoteId = accheader.DocumentNo;
                ////  ViewBag.Message = "3";

                //Session["EnableRequestNoteCancelOnly"] = _bllconfiguration.GetConfiguration("EnableRequestNoteCancelOnly", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
                //return View("~/Views/Transactions/RequestNote/EditPendingRequestNotes.cshtml", accheader);

                var requestnotedata = _bllrequestNote.GetPendingRequest(accheader.RequestnoteHeaderId);
                Session["EnableRequestNoteCancelOnly"] = _bllconfiguration.GetConfiguration("EnableRequestNoteCancelOnly", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
                ViewBag.documentNo = requestnotedata.DocumentNo;
                return View("~/Views/Transactions/RequestNote/EditPendingRequestNotes.cshtml", requestnotedata);


            }
            // Handle invalid ModelState case here if needed
           
        }


            [HttpPost]
        public ActionResult CancelRequestNote(RequestNoteHeader header)
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            if (!ModelState.IsValid)
            {
                var deptdata = _bllproduct.GetActiveProducts(companyid);
                ViewBag.locationId = header.FromLocationId;
                ViewBag.ToLocationId = header.ToLocationId;
                ViewBag.FromDepartmentId = header.FromDepartmentId;
                ViewBag.ToDepartmentId = header.ToDepartmentId;
                ViewBag.productdata = deptdata;
                return View("~/Views/Transactions/RequestNote/EditRequestNotes.cshtml", header);
            }
            if (header.RequestDetail.Count == 0)
            {
                var deptdata = _bllproduct.GetActiveProducts(companyid);
                ViewBag.locationId = header.FromLocationId;
                ViewBag.FromDepartmentId = header.FromDepartmentId;
                ViewBag.toLocationId = header.ToLocationId;
                ViewBag.ToDepartmentId = header.ToDepartmentId;
                ViewBag.productdata = deptdata;
                @ViewBag.Message = "5";
                ModelState.AddModelError("RequestDetail", "Please enter request details !");
                return View("~/Views/Transactions/RequestNote/EditRequestNotes.cshtml", header);
            }
            var dbheader = _bllrequestNote.GetRequestNoteById(header.RequestnoteHeaderId);
            header.DocumentStatus = 4;
            header.CanceleddBy = Session["loggeduser"].ToString();
            header.CanceledDate = DateTime.Now;

            bool DisableFromDepartmentAndToDepartment = _bllconfiguration
             .GetConfiguration("DisableFromDepartmentAndToDepartment", Convert.ToInt32(Session["loggedusercompanyId"].ToString()))
             .ConfigurationOn;
            ViewBag.DisableFromDepartmentAndToDepartment = DisableFromDepartmentAndToDepartment;
            @ViewBag.Code = header.DocumentNo;
            if (_bllrequestNote.CancelRequest(header))
            {
                ModelState.Clear();

                ViewBag.Message = "6";
                ViewBag.locationId = Convert.ToInt32(Session["loggeduserlocId"]);
                ViewBag.toLocationId = 0;
                ViewBag.ToDepartmentId = 0;
              
                return View("~/Views/Transactions/RequestNote/EditRequestNotes.cshtml", header);
            }
            else
            {
                var deptdata = _bllproduct.GetActiveProducts(companyid);
                ViewBag.locationId = header.FromLocationId;
                ViewBag.FromDepartmentId = header.FromDepartmentId;
                ViewBag.toLocationId = header.ToLocationId;
                ViewBag.ToDepartmentId = header.ToDepartmentId;
                ViewBag.productdata = deptdata;
                ViewBag.Message = "3";
                return View("~/Views/Transactions/RequestNote/EditRequestNotes.cshtml", header);


            }
                
        }

            [HttpPost]
        public ActionResult ReOpenRequestNote(RequestNoteHeader header)
        {
            // check permissions
            if (!_appmanager.SetPermissions(9, Session["loggeduserempcode"].ToString(), "RQReOpen"))
            {
                @ViewBag.Permissions = "No user permissions to Re Open Request Notes";
                return View("~/Views/Account/AccessDenied.cshtml");
            }
            //end check permissions
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            if (!ModelState.IsValid)
            {
                var deptdata = _bllproduct.GetActiveProducts(companyid);
                ViewBag.locationId = header.FromLocationId;
                ViewBag.ToLocationId = header.ToLocationId;
                ViewBag.FromDepartmentId = header.FromDepartmentId;
                ViewBag.ToDepartmentId = header.ToDepartmentId;
                ViewBag.productdata = deptdata;
                return View("~/Views/Transactions/RequestNote/EditRequestNotes.cshtml", header);
            }
            if (header.RequestDetail.Count == 0)
            {
                var deptdata = _bllproduct.GetActiveProducts(companyid);
                ViewBag.locationId = header.FromLocationId;
                ViewBag.FromDepartmentId = header.FromDepartmentId;
                ViewBag.toLocationId = header.ToLocationId;
                ViewBag.ToDepartmentId = header.ToDepartmentId;
                ViewBag.productdata = deptdata;
                @ViewBag.Message = "5";
                ModelState.AddModelError("RequestDetail", "Please enter request details !");
                return View("~/Views/Transactions/RequestNote/EditRequestNotes.cshtml", header);
            }
            var dbheader = _bllrequestNote.GetRequestNoteById(header.RequestnoteHeaderId).DocumentStatus;
            header.DocumentStatus = dbheader;
            int docid = 0;
            var docnum = "";

            //if (header.DocumentStatus != 4)
            //{
            //    docid = _bllcommon.GetDcumentId("Request");
            //    docnum = _bllcommon.GetDocumentNumber("Request",
            //                                        Convert.ToInt32(Session["loggeduserlocId"]),
            //                                        "1",
            //                                        docid);
            //    header.DocumentId = docid;
            //    header.DocumentNo = docnum;
            //}

            //if (Convert.ToBoolean(Session["APPRN"]) == true)
            //{
            //    header.DocumentStatus = 2;
            //}
            //else
            //{
            //    header.DocumentStatus = 3;
            //}
            header.DocumentId= _bllcommon.GetDcumentId("Request", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            header.DocumentStatus = 6;
            header.IsTempRequest = false;
            header.CompanyId = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            @ViewBag.Code = header.DocumentNo;
            if (_bllrequestNote.ModifyRequest(header))
            {
                if (header.DocumentStatus == 3)
                {
                    RequestNoteAccptanceHeader accheader = new RequestNoteAccptanceHeader();
                    accheader.DocumentDate = DateTime.Now;
                    accheader.FromLocationId = header.FromLocationId;
                    accheader.FromDepartmentId = header.FromDepartmentId;
                    accheader.DocumentNo = header.DocumentNo;
                    var requestnotedata = _bllrequestNote.GetReceipesByRequestId(header.RequestnoteHeaderId, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                    List<RequestNoteAcceptanceDetail> acceptanceDetail = new List<RequestNoteAcceptanceDetail>();
                    var LineNo = 0;
                    foreach (var item in requestnotedata)
                    {
                        var vm = new RequestNoteAcceptanceDetail();
                        LineNo = LineNo + 1;
                        vm.RequestNoteAccptanceHeaderId = header.RequestnoteHeaderId;
                        vm.LineNo = LineNo;
                        vm.ProductId = item.ProductId;
                        vm.MaterialId = item.MaterialId;
                        vm.CostPrice = item.MaterialCostPrice;
                        vm.SellingPrice = item.MaterialSellingPrice;
                        vm.MaterialQty = item.MaterialQuantity;
                        vm.IssueQty = item.MaterialQuantity;
                        vm.UnitOfMeasureId = item.MaterialUOMId;
                        vm.ProductName = item.ProductName;

                        accheader.AcceptanceDetail.Add(vm);
                    }
                    if (_bllrequestNote.AcceptRequest(accheader))
                    { }
                }
                docid = _bllcommon.GetDcumentId("Request", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                if (dbheader != 4)
                {
                    docnum = _bllcommon.GetDocumentNo("Request",
                        Convert.ToInt32(Session["loggeduserlocId"]),
                        Convert.ToString(header.FromLocationId),
                        docid,
                        false, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                }
                ModelState.Clear();

                ViewBag.Message = "1";
                ViewBag.locationId = Convert.ToInt32(Session["loggeduserlocId"]);
                ViewBag.toLocationId = 0;
                ViewBag.ToDepartmentId = 0;
                var requestnoteheader = new RequestNoteHeader();
                docnum = _bllcommon.GetDocumentTemporyNo("Request", Convert.ToInt32(Session["loggeduserlocId"]), "1", docid, true, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                requestnoteheader.DocumentNo = docnum;
                return View("~/Views/Transactions/RequestNote/CreateRequestNote.cshtml", requestnoteheader);
            }
            else
            {
                var deptdata = _bllproduct.GetActiveProducts(companyid);
                ViewBag.locationId = header.FromLocationId;
                ViewBag.FromDepartmentId = header.FromDepartmentId;
                ViewBag.toLocationId = header.ToLocationId;
                ViewBag.ToDepartmentId = header.ToDepartmentId;
                ViewBag.productdata = deptdata;
                ViewBag.Message = "3";
                return View("~/Views/Transactions/RequestNote/EditRequestNotes.cshtml", header);
            }
        }


        //    [Authorize(Roles = "RQEdit")]
        [HttpPost]
        public ActionResult TempSaveRequestNote(RequestNoteHeader header)
        {
            foreach (var item in header.RequestDetail)
            {

                // var supplierID = _bllproduct.GetProductBySupplierId(item.ProductId).SupplierId;
                // item.SupplierId = supplierID;


                var x = _bllproduct.GetProductBySupplierId(item.ProductId);
                if (x != null)
                {
                    var supplierID = _bllproduct.GetProductBySupplierId(item.ProductId).SupplierId;
                    item.SupplierId = supplierID;
                }
                else
                {
                    item.SupplierId = 0;
                }


            }
            // check permissions
            if (!_appmanager.SetPermissions(9, Session["loggeduserempcode"].ToString(), "RQCreatee"))
            {
                @ViewBag.Permissions = "No user permissions to Create Request Notes";
                return View("~/Views/Account/AccessDenied.cshtml");
            }
            //end check permissions
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            if (!ModelState.IsValid)
            {
                var deptdata = _bllproduct.GetActiveProducts(companyid);
                ViewBag.locationId = header.FromLocationId;
                ViewBag.ToLocationId = header.ToLocationId;
                ViewBag.FromDepartmentId = header.FromDepartmentId;
                ViewBag.ToDepartmentId = header.ToDepartmentId;
                ViewBag.productdata = deptdata;
                return View("~/Views/Transactions/RequestNote/CreateRequestNote.cshtml", header);
            }
            if (header.RequestDetail.Count == 0)
            {
                var deptdata = _bllproduct.GetActiveProducts(companyid);
                ViewBag.locationId = header.FromLocationId;
                ViewBag.FromDepartmentId = header.FromDepartmentId;
                ViewBag.toLocationId = header.ToLocationId;
                ViewBag.ToDepartmentId = header.ToDepartmentId;
                ViewBag.productdata = deptdata;
                @ViewBag.Message = "5";
                ModelState.AddModelError("RequestDetail", "Please enter request details !");
                return View("~/Views/Transactions/RequestNote/CreateRequestNote.cshtml", header);
            }
            header.IsTempRequest = true;
            header.DocumentDate = DateTime.Now;
            header.DocumentStatus = 1;

            int docid = _bllcommon.GetDcumentId("Request", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            var docnum = _bllcommon.GetDocumentTemporyNo("Request", Convert.ToInt32(Session["loggeduserlocId"]), "1", docid, true, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            header.DocumentNo = docnum;
            @ViewBag.Code = header.DocumentNo;
            // header.DocumentNo = commonService.GetDocumentNo("Request", 1, "1", 2, true);
            header.CompanyId= Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            if (_bllrequestNote.SubmitRequest(header))
            {
                ModelState.Clear();
                var reHead = new RequestNoteHeader();
                docid = _bllcommon.GetDcumentId("Request", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                docnum = _bllcommon.GetDocumentNo("Request",
                    Convert.ToInt32(Session["loggeduserlocId"]), 
                    "1", 
                    docid,
                    true, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));

                reHead.DocumentId = docid;
                reHead.DocumentNo = docnum;
                @ViewBag.Message = "2";
                ViewBag.locationId = Convert.ToInt32(Session["loggeduserlocId"]);
                ViewBag.toLocationId = 0;
                ViewBag.ToDepartmentId = 0;
                bool DisableFromDepartmentAndToDepartment = _bllconfiguration.GetConfiguration("DisableFromDepartmentAndToDepartment", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
                if (DisableFromDepartmentAndToDepartment == true)
                {
                    ViewBag.DisableFromDepartmentAndToDepartment = DisableFromDepartmentAndToDepartment;
                }
                else
                {
                    ViewBag.DisableFromDepartmentAndToDepartment = DisableFromDepartmentAndToDepartment;
                }
                return View("~/Views/Transactions/RequestNote/CreateRequestNote.cshtml", reHead);
            }
            else
            {
                @ViewBag.Message = "3";
                var deptdata = _bllproduct.GetActiveProducts(companyid);
                ViewBag.locationId = header.FromLocationId;
                ViewBag.FromDepartmentId = header.FromDepartmentId;
                ViewBag.toLocationId = header.ToLocationId;
                ViewBag.ToDepartmentId = header.ToDepartmentId;
                ViewBag.productdata = deptdata;
                return View("~/Views/Transactions/RequestNote/CreateRequestNote.cshtml", header);
            }
        }
     //   [Authorize(Roles = "RQEdit")]
        [HttpPost]
        public ActionResult TempSaveAgainRequestNote(RequestNoteHeader header)
        {
            // check permissions
                if (!_appmanager.SetPermissions(9, Session["loggeduserempcode"].ToString(), "RQEdit"))
                {
                    @ViewBag.Permissions = "No user permissions to Edit Request Notes";
                    return View("~/Views/Account/AccessDenied.cshtml");
                }
            //end check permissions
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            if (!ModelState.IsValid)
            {
                var deptdata = _bllproduct.GetActiveProducts(companyid);
                ViewBag.locationId = header.FromLocationId;
                ViewBag.ToLocationId = header.ToLocationId;
                ViewBag.FromDepartmentId = header.FromDepartmentId;
                ViewBag.ToDepartmentId = header.ToDepartmentId;
                ViewBag.productdata = deptdata;
                return View("~/Views/Transactions/RequestNote/CreateRequestNote.cshtml", header);
            }
            if (header.RequestDetail.Count == 0)
            {
                var deptdata = _bllproduct.GetActiveProducts(companyid);
                ViewBag.locationId = header.FromLocationId;
                ViewBag.FromDepartmentId = header.FromDepartmentId;
                ViewBag.toLocationId = header.ToLocationId;
                ViewBag.ToDepartmentId = header.ToDepartmentId;
                ViewBag.productdata = deptdata;
                @ViewBag.Message = "5";
                ModelState.AddModelError("RequestDetail", "Please enter request details !");
                return View("~/Views/Transactions/RequestNote/CreateRequestNote.cshtml", header);
            }
            header.IsTempRequest = true;
            header.DocumentDate = DateTime.Now;
            header.DocumentStatus = 1;
            @ViewBag.Code = header.DocumentNo;

            // header.DocumentNo = commonService.GetDocumentNo("Request", 1, "1", 2, true);
            header.CompanyId = Convert.ToInt32(Session["loggedusercompanyId"].ToString());

            if(header.IsCreateBasedOnThis)
            {
                header.RequestnoteHeaderId = 0;
            }


            if (_bllrequestNote.ModifyRequest(header))
            {
                ModelState.Clear();
                var reHead = new RequestNoteHeader();
                int docid = _bllcommon.GetDcumentId("Request", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                var docnum = _bllcommon.GetDocumentTemporyNo("Request", Convert.ToInt32(Session["loggeduserlocId"]), 
                                                                        "1", docid, true, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                bool DisableFromDepartmentAndToDepartment = _bllconfiguration.GetConfiguration("DisableFromDepartmentAndToDepartment", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
                if (DisableFromDepartmentAndToDepartment == true)
                {
                    ViewBag.DisableFromDepartmentAndToDepartment = DisableFromDepartmentAndToDepartment;
                }
                else
                {
                    ViewBag.DisableFromDepartmentAndToDepartment = DisableFromDepartmentAndToDepartment;
                }


                reHead.DocumentId = docid;
                reHead.DocumentNo = docnum;
                @ViewBag.Message = "2";
                ViewBag.locationId = Convert.ToInt32(Session["loggeduserlocId"]);
                ViewBag.toLocationId = 0;
                ViewBag.ToDepartmentId = 0;
                return View("~/Views/Transactions/RequestNote/CreateRequestNote.cshtml", reHead);
            }
            else
            {
                @ViewBag.Message = "3";
                var deptdata = _bllproduct.GetActiveProducts(companyid);
                ViewBag.locationId = header.FromLocationId;
                ViewBag.FromDepartmentId = header.FromDepartmentId;
                ViewBag.toLocationId = header.ToLocationId;
                ViewBag.ToDepartmentId = header.ToDepartmentId;
                ViewBag.productdata = deptdata;
                return View("~/Views/Transactions/RequestNote/CreateRequestNote.cshtml", header);
            }
        }
        // [Authorize(Roles = "RQApprove")]
     //   [Authorize(Roles = "RQEdit")]
        public ActionResult AcceptRequestNote()
        {

            //Session["ARNFromDate"] = from;
            //Session["ARNToDate"] = to;
            //Session["ARNSelectFromLocation"] = id;
            //Session["ARNSelectToLocation"] = requestidTo;
            //Session["ARNAppStatus"] = AppStatus;


            if (Session["ARNFromDate"] != null)
            {
                string Fromdate = Session["ARNFromDate"].ToString().Trim();
                DateTime From_date = new DateTime();
                DateTime.TryParse(Fromdate, out From_date);
                ViewBag.DateFrom = From_date.ToString("yyyy-MM-dd");

            }

            if (Session["ARNToDate"] != null)
            {
                string Todate = Session["ARNToDate"].ToString().Trim();
                DateTime To_date = new DateTime();
                DateTime.TryParse(Todate, out To_date);
                ViewBag.DateTo = To_date.ToString("yyyy-MM-dd");

            }
            if (Session["ARNSelectFromLocation"] != null)

            {
                int fromLocID = 0;
                int.TryParse(Session["ARNSelectFromLocation"].ToString().Trim(), out fromLocID);

                ViewBag.SelectFromLocation = fromLocID;
            }

            if (Session["ARNSelectToLocation"] != null)

            {
                int ToLocID = 0;
                int.TryParse(Session["ARNSelectToLocation"].ToString().Trim(), out ToLocID);

                ViewBag.SelectToLocation = ToLocID;
            }

            if (Session["ARNAppStatus"] != null)

            {
                int AppStatus = 0;
                int.TryParse(Session["ARNAppStatus"].ToString().Trim(), out AppStatus);

                ViewBag.SelectAppStatus = AppStatus;
            }

            // check permissions
            if (!_appmanager.SetPermissions(9, Session["loggeduserempcode"].ToString(), "RQApprove"))
            {
                @ViewBag.Permissions = "No user permissions to Approve Request Notes";
                return View("~/Views/Account/AccessDenied.cshtml");
            }
            //end check permissions

            Session["APPRN"] = _bllconfiguration.GetConfiguration("APPRN", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            Session["RequestNoteAllowOnlyPO"] = _bllconfiguration.GetConfiguration("RequestNoteAllowOnlyPO", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;


            Session["DisableServingUnit"] = _bllconfiguration.GetConfiguration("DisableServingUnit", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            Session["DisableRequestedBy"] = _bllconfiguration.GetConfiguration("DisableRequestedBy", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            Session["DisablePriceRequestNote"] = _bllconfiguration.GetConfiguration("DisablePriceRequestNote", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            Session["EnableRequestNoteLineRemark"] = _bllconfiguration.GetConfiguration("EnableRequestNoteLineRemark", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            Session["EnableRequestNoteCancelOnly"] = _bllconfiguration.GetConfiguration("EnableRequestNoteCancelOnly", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            Session["DisableFromDepartmentAndToDepartment"] = _bllconfiguration.GetConfiguration("DisableFromDepartmentAndToDepartment", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;


            var acceptanceheader = new RequestNoteAccptanceHeader();
           // var requestheader = _bllrequestNote.GetAllActiveRequestNotes();
            ViewBag.locationId = Convert.ToInt32(Session["loggeduserlocId"]);
            ViewBag.FromDepartmentId = 0;
            return View("~/Views/Transactions/RequestNote/Accept_Request_Note.cshtml", acceptanceheader);
        }
     
        
        // [Authorize(Roles = "RQApprove")]
        //      [Authorize(Roles = "RQEdit")]
        [HttpPost]
        public ActionResult AcceptRequestNote(RequestNoteAccptanceHeader accheader)
        {
            // check permissions
            if (!_appmanager.SetPermissions(9, Session["loggeduserempcode"].ToString(), "RQApprove"))
            {
                @ViewBag.Permissions = "No user permissions to Approve Request Notes";
                return View("~/Views/Account/AccessDenied.cshtml");
            }
            //end check permissions

            @ViewBag.Code = accheader.DocumentNo;
            accheader.DocumentDate = DateTime.Now;
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            if (!ModelState.IsValid)
            {
                ViewBag.locationId = accheader.FromLocationId;
                ViewBag.FromDepartmentId = accheader.FromDepartmentId;
                ViewBag.RequestNoteId = accheader.DocumentNo;
                @ViewBag.Message = "2";
                ModelState.AddModelError("RequestDetail", "Please enter request details !");
                return View("~/Views/Transactions/RequestNote/AcceptRequestNote.cshtml", accheader);
            }
            if (accheader.FromLocationId == 0 || accheader.FromDepartmentId == 0 || accheader.DocumentNo == "")
            {
                ViewBag.locationId = accheader.FromLocationId;
                ViewBag.FromDepartmentId = accheader.FromDepartmentId;
                ViewBag.RequestNoteId = accheader.DocumentNo;
                @ViewBag.Message = "2";
                ModelState.AddModelError("RequestDetail", "Please enter request details !");
                return View("~/Views/Transactions/RequestNote/AcceptRequestNote.cshtml", accheader);
            }
            if (accheader.AcceptanceDetail.Count == 0)
            {
                ViewBag.locationId = accheader.FromLocationId;
                ViewBag.FromDepartmentId = accheader.FromDepartmentId;
                ViewBag.RequestNoteId = accheader.DocumentNo;
                @ViewBag.Message = "2";
                ModelState.AddModelError("RequestDetail", "Please enter request details !");
                return View("~/Views/Transactions/RequestNote/AcceptRequestNote.cshtml", accheader);
            }
            accheader.CompanyId = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            if (_bllrequestNote.AcceptRequest(accheader))
            {
                RequestNoteAccptanceHeader accheader_ = new RequestNoteAccptanceHeader();
                ModelState.Clear();
                @ViewBag.Message = "1";
                ViewBag.locationId = Convert.ToInt32(Session["loggeduserlocId"]);
                ViewBag.FromDepartmentId = 0;
                ViewBag.RequestNoteId = 0;
                var requestnoteAccptanceHeader = new RequestNoteAccptanceHeader();
                requestnoteAccptanceHeader.DocumentNo = accheader_.DocumentNo;
                return View("~/Views/Transactions/RequestNote/AcceptRequestNote.cshtml", accheader_);
            }
            else
            {
                ViewBag.locationId = accheader.FromLocationId;
                ViewBag.FromDepartmentId = accheader.FromDepartmentId;
                ViewBag.RequestNoteId = accheader.DocumentNo;
                @ViewBag.Message = "3";
                return View("~/Views/Transactions/RequestNote/AcceptRequestNote.cshtml", accheader);
            }
        }
        //[Authorize(Roles = "RQReject")]
       // [Authorize(Roles = "RQEdit")]
        [HttpPost]
        public ActionResult Reject(RequestNoteAccptanceHeader accheader)
        {

            // check permissions
            if (!_appmanager.SetPermissions(9, Session["loggeduserempcode"].ToString(), "RQReject"))
            {
                @ViewBag.Permissions = "No user permissions to Reject Request Notes";
                return View("~/Views/Account/AccessDenied.cshtml");
            }
            //end check permissions


            @ViewBag.Code = accheader.DocumentNo;
            accheader.DocumentDate = DateTime.Now;
            if (!ModelState.IsValid)
            {
                ViewBag.locationId = accheader.FromLocationId;
                ViewBag.FromDepartmentId = accheader.FromDepartmentId;
                ViewBag.RequestNoteId = accheader.DocumentNo;
                @ViewBag.Message = "2";
                ModelState.AddModelError("RequestDetail", "Please enter request details !");
                return View("~/Views/Transactions/RequestNote/AcceptRequestNote.cshtml", accheader);
            }
            if (accheader.FromLocationId == 0 || accheader.FromDepartmentId == 0 || accheader.DocumentNo == "")
            {
                ViewBag.locationId = accheader.FromLocationId;
                ViewBag.FromDepartmentId = accheader.FromDepartmentId;
                ViewBag.RequestNoteId = accheader.DocumentNo;
                @ViewBag.Message = "2";
                ModelState.AddModelError("RequestDetail", "Please enter request details !");
                return View("~/Views/Transactions/RequestNote/AcceptRequestNote.cshtml", accheader);
            }
            if (accheader.AcceptanceDetail.Count == 0)
            {
                ViewBag.locationId = accheader.FromLocationId;
                ViewBag.FromDepartmentId = accheader.FromDepartmentId;
                ViewBag.RequestNoteId = accheader.DocumentNo;
                @ViewBag.Message = "2";
                ModelState.AddModelError("RequestDetail", "Please enter request details !");
                return View("~/Views/Transactions/RequestNote/AcceptRequestNote.cshtml", accheader);
            }
            accheader.CompanyId = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            if (_bllrequestNote.UpdateRequestStatusReject(accheader) == 1)
            {
                RequestNoteAccptanceHeader accheader_ = new RequestNoteAccptanceHeader();
                ModelState.Clear();
                @ViewBag.Message = "4";
                ViewBag.locationId = Convert.ToInt32(Session["loggeduserlocId"]);
                ViewBag.FromDepartmentId = 0;
                ViewBag.RequestNoteId = 0;
                var requestnoteAccptanceHeader = new RequestNoteAccptanceHeader();
                requestnoteAccptanceHeader.DocumentNo = accheader_.DocumentNo;
                return View("~/Views/Transactions/RequestNote/AcceptRequestNote.cshtml", accheader_);
            }
            else
            {
                ViewBag.locationId = accheader.FromLocationId;
                ViewBag.FromDepartmentId = accheader.FromDepartmentId;
                ViewBag.RequestNoteId = accheader.DocumentNo;
                @ViewBag.Message = "3";
                return View("~/Views/Transactions/RequestNote/AcceptRequestNote.cshtml", accheader);
            }
        }
        // [Authorize(Roles = "RQCancel")]
     //   [Authorize(Roles = "RQEdit")]
        [HttpPost]
        public ActionResult Cancel(RequestNoteAccptanceHeader accheader)
        {

            // check permissions
            if (!_appmanager.SetPermissions(9, Session["loggeduserempcode"].ToString(), "RQCancel"))
            {
                @ViewBag.Permissions = "No user permissions to Cancel Request Notes";
                return View("~/Views/Account/AccessDenied.cshtml");
            }
            //end check permissions

            @ViewBag.Code = accheader.DocumentNo;
            accheader.DocumentDate = DateTime.Now;
            if (!ModelState.IsValid)
            {
                ViewBag.locationId = accheader.FromLocationId;
                ViewBag.FromDepartmentId = accheader.FromDepartmentId;
                ViewBag.RequestNoteId = accheader.DocumentNo;
                @ViewBag.Message = "2";
                ModelState.AddModelError("RequestDetail", "Please enter request details !");
                return View("~/Views/Transactions/RequestNote/AcceptRequestNote.cshtml", accheader);
            }
            if (accheader.FromLocationId == 0 || accheader.FromDepartmentId == 0 || accheader.DocumentNo == "")
            {
                ViewBag.locationId = accheader.FromLocationId;
                ViewBag.FromDepartmentId = accheader.FromDepartmentId;
                ViewBag.RequestNoteId = accheader.DocumentNo;
                @ViewBag.Message = "2";
                ModelState.AddModelError("RequestDetail", "Please enter request details !");
                return View("~/Views/Transactions/RequestNote/AcceptRequestNote.cshtml", accheader);
            }
            if (accheader.AcceptanceDetail.Count == 0)
            {
                ViewBag.locationId = accheader.FromLocationId;
                ViewBag.FromDepartmentId = accheader.FromDepartmentId;
                ViewBag.RequestNoteId = accheader.DocumentNo;
                @ViewBag.Message = "2";
                ModelState.AddModelError("RequestDetail", "Please enter request details !");
                return View("~/Views/Transactions/RequestNote/AcceptRequestNote.cshtml", accheader);
            }
            accheader.CompanyId = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            if (_bllrequestNote.UpdateRequestStatusCancel(accheader) == 1)
            {
                RequestNoteAccptanceHeader accheader_ = new RequestNoteAccptanceHeader();
                ModelState.Clear();
                @ViewBag.Message = "5";
                ViewBag.locationId = Convert.ToInt32(Session["loggeduserlocId"]);
                ViewBag.FromDepartmentId = 0;
                ViewBag.RequestNoteId = 0;
                var requestnoteAccptanceHeader = new RequestNoteAccptanceHeader();
                requestnoteAccptanceHeader.DocumentNo = accheader_.DocumentNo;
                return View("~/Views/Transactions/RequestNote/AcceptRequestNote.cshtml", accheader_);
            }
            else
            {
                ViewBag.locationId = accheader.FromLocationId;
                ViewBag.FromDepartmentId = accheader.FromDepartmentId;
                ViewBag.RequestNoteId = accheader.DocumentNo;
                @ViewBag.Message = "3";
                return View("~/Views/Transactions/RequestNote/AcceptRequestNote.cshtml", accheader);
            }
        }
        //[Authorize(Roles = "RQForsefullyClose")]
      //  [Authorize(Roles = "RQEdit")]
        [HttpPost]
        public ActionResult ForcefullyClose(RequestNoteAccptanceHeader accheader)
        {

            // check permissions
            if (!_appmanager.SetPermissions(9, Session["loggeduserempcode"].ToString(), "RQForsefullyClose"))
            {
                @ViewBag.Permissions = "No user permissions to Forcefully Close Request Notes";
                return View("~/Views/Account/AccessDenied.cshtml");
            }
            //end check permissions


            @ViewBag.Code = accheader.DocumentNo;
            accheader.DocumentDate = DateTime.Now;
            if (!ModelState.IsValid)
            {
                ViewBag.locationId = accheader.FromLocationId;
                ViewBag.FromDepartmentId = accheader.FromDepartmentId;
                ViewBag.RequestNoteId = accheader.DocumentNo;
                @ViewBag.Message = "2";
                ModelState.AddModelError("RequestDetail", "Please enter request details !");
                return View("~/Views/Transactions/RequestNote/AcceptRequestNote.cshtml", accheader);
            }
            if (accheader.FromLocationId == 0 || accheader.FromDepartmentId == 0 || accheader.DocumentNo == "")
            {
                ViewBag.locationId = accheader.FromLocationId;
                ViewBag.FromDepartmentId = accheader.FromDepartmentId;
                ViewBag.RequestNoteId = accheader.DocumentNo;
                @ViewBag.Message = "2";
                ModelState.AddModelError("RequestDetail", "Please enter request details !");
                return View("~/Views/Transactions/RequestNote/AcceptRequestNote.cshtml", accheader);
            }
            if (accheader.AcceptanceDetail.Count == 0)
            {
                ViewBag.locationId = accheader.FromLocationId;
                ViewBag.FromDepartmentId = accheader.FromDepartmentId;
                ViewBag.RequestNoteId = accheader.DocumentNo;
                @ViewBag.Message = "2";
                ModelState.AddModelError("RequestDetail", "Please enter request details !");
                return View("~/Views/Transactions/RequestNote/AcceptRequestNote.cshtml", accheader);
            }
            accheader.CompanyId = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            if (_bllrequestNote.UpdateRequestStatusForcefullyClose(accheader) == 1)
            {
                RequestNoteAccptanceHeader accheader_ = new RequestNoteAccptanceHeader();
                ModelState.Clear();
                @ViewBag.Message = "5";
                ViewBag.locationId = Convert.ToInt32(Session["loggeduserlocId"]);
                ViewBag.FromDepartmentId = 0;
                ViewBag.RequestNoteId = 0;
                var requestnoteAccptanceHeader = new RequestNoteAccptanceHeader();
                requestnoteAccptanceHeader.DocumentNo = accheader_.DocumentNo;
                return View("~/Views/Transactions/RequestNote/AcceptRequestNote.cshtml", accheader_);
            }
            else
            {
                ViewBag.locationId = accheader.FromLocationId;
                ViewBag.FromDepartmentId = accheader.FromDepartmentId;
                ViewBag.RequestNoteId = accheader.DocumentNo;
                @ViewBag.Message = "3";
                return View("~/Views/Transactions/RequestNote/AcceptRequestNote.cshtml", accheader);
            }

         
        }

        [HttpGet]
        public JsonResult GetRequestData(long id)
        {
            var requestnotedata = _bllrequestNote.GetReceipesByRequestId(id, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            Session["EnableRequestNoteCancelOnly"] = _bllconfiguration.GetConfiguration("EnableRequestNoteCancelOnly", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            return Json(requestnotedata, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetLocWiseDept(long locid)
        {
            var requestnotes = _bllrequestNote.GetInterDeptByLocId(locid, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            return Json(JsonConvert.SerializeObject(requestnotes, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetDeptWiseRequests(long deptid, long locid)
        {
            //  var requestnotes = _bllrequestNote.GetRequestNotesByLocIdDeptId(locid, deptid, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            var requestnotes = _bllrequestNote.GetActiveRequestNotesByLocIdDeptId(locid, deptid, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            return Json(JsonConvert.SerializeObject(requestnotes, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetLocWiseAccRequest(long locid)
        {
            var requestnotes = _bllrequestNote.GetAcceptedRequestNotesByLocId(locid, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            return Json(JsonConvert.SerializeObject(requestnotes, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetLocWiseAccRequestForProduction(long locid)
        {
            var requestnotes = _bllrequestNote.GetAcceptedRequestNotesByLocIdForProduction(locid, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            //if (requestnotes != null)
            //{
            //    requestnotes.Where(r => r.RequestType.Equals("TOG"));
            //}
            return Json(JsonConvert.SerializeObject(requestnotes, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetLocWiseAccRequestForTOG(long locid, long locidTo)
        {
            var requestnotes = _bllrequestNote.GetAcceptedRequestNotesByLocIdForTOG(locid, locidTo, Convert.ToInt32(Session["loggedusercompanyId"].ToString())).Where(r=>r.IsTOG==false);
            //if (requestnotes != null)
            //{
            //    requestnotes.Where(r => r.RequestType.Equals("TOG"));
            //}

            if(requestnotes ==null)
            {
                requestnotes = new List<RequestNoteAccptanceHeader>();
            }




            return Json(JsonConvert.SerializeObject(requestnotes, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetLocWiseAccRequestForPO(long locid)
        {
            var requestnotes = _bllrequestNote.GetAcceptedRequestNotesByLocIdForPO(locid, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            //if (requestnotes != null) {
            //    requestnotes.Where(r => r.RequestType.Equals("PO"));
            //}
            return Json(JsonConvert.SerializeObject(requestnotes, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }
        public class Menu
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
        public static List<Menu> GetMenus(long fromLocation, long toLocation, string foo)
        {
            // Fetch data from database or any other source
            // For simplicity, returning a dummy list here
            return new List<Menu>
        {
            new Menu { Id = 1, Name = "Menu1" },
            new Menu { Id = 2, Name = "Menu2" }
        };
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
        //[HttpGet]
        //public ActionResult YourActionMethod()
        //{
        //    bool withoutRowMaterial = _bllconfiguration.GetConfiguration("WithoutRowMaterialInPOBasedRequestNote", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;

        //    ViewBag.WithoutRowMaterial = withoutRowMaterial;

        //    return View();
        //}
        [HttpGet]
        public JsonResult GetProductData(long id, decimal qty, long frmlocid, long tolocid,
                                      string fromlocname, string tolocname, string foo, string servingunit,string lineRemark)
        {
            TOGViewModel vm = new TOGViewModel();
           
            if (_blltransfernote.CheckProductExistsAtLoc(foo,id, frmlocid, tolocid,Convert.ToInt32(Session["loggedusercompanyId"].ToString())))
            {
                if (servingunit == "N/A")
                {
                    if (foo == "PO")
                    {
                        var tog = _blltransfernote.GetPOProductId(id, frmlocid, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                        vm.ItemId = id;
                        //Added by pavithra
                        vm.ItemDesc = tog.ProductCode + " - " + tog.ProductName;
                        vm.Qty = qty;
                        vm.SellingPrice = tog.SellingPrice;// * qty;
                        vm.CostPrice = tog.CostPrice;// * qty;
                        vm.UnitCost = tog.CostPrice;
                        vm.UnitSelling = tog.SellingPrice;
                        if (foo == "PO")
                        {
                            vm.CrrQty = qty;
                        }
                        else
                        {
                            vm.CrrQty = tog.Stock;
                        }
                        vm.ServingUnit = "N/A";

                        vm.CostValue = tog.CostPrice * qty;
                        vm.ProductCode = tog.ProductCode;
                        vm.ProductName = tog.ProductName;
                        vm.Remark = lineRemark;
                        vm.SIH=   _bllproductstock.GetProductStockMasterByProductIdLocid(id, Convert.ToInt32(frmlocid)).Stock;
                    }
                    else
                    {
                        var tog = _blltransfernote.GetTOGProductById(id, tolocid, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));

                        vm.ItemId = id;
                        //Added by pavithra
                        vm.ItemDesc = tog.ProductCode + " - " + tog.ProductName;
                        vm.Qty = qty;
                        vm.SellingPrice = tog.SellingPrice;// * qty;
                        vm.CostPrice = tog.CostPrice;// * qty;
                        vm.UnitCost = tog.CostPrice;
                        vm.UnitSelling = tog.SellingPrice;
                        if (foo == "PO")
                        {
                            vm.CrrQty = qty;
                        }
                        else
                        {
                            vm.CrrQty = tog.Stock;
                        }
                        vm.ServingUnit = "N/A";
                        vm.SIH = _bllproductstock.GetProductStockMasterByProductIdLocid(id, Convert.ToInt32(frmlocid)).Stock;
                        vm.ProductCode = tog.ProductCode;
                        vm.ProductName = tog.ProductName;
                        vm.CostValue = tog.CostPrice * qty;
                        vm.Remark = lineRemark;
                    }
                }
                else
                {
                    var servingunits = _bllservingunit.GetProductServingUnitsByPrdIdServingUnit(id,servingunit, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                    var prduct = _bllproduct.GetProductById(id);
                    vm.ItemId = id;
                    vm.ItemDesc = prduct.ProductCode + " - " + prduct.ProductName;
                    vm.Qty = qty;
                    vm.SellingPrice = servingunits.SellingPrice * qty;
                    vm.CostPrice = servingunits.CostPrice * qty;
                    vm.UnitCost = servingunits.CostPrice;
                    vm.UnitSelling = servingunits.SellingPrice;
                    vm.ServingUnit = servingunits.ServingUnit;
                    vm.ServingUnitId = servingunits.ProductServingUnitId;
                    vm.SIH = _bllproductstock.GetProductStockMasterByProductIdLocid(id, Convert.ToInt32(frmlocid)).Stock;
                    vm.CostValue = servingunits.CostPrice * qty;
                    vm.ProductCode = prduct.ProductCode;
                    vm.ProductName = prduct.ProductName;
                    vm.Remark = lineRemark;
                    // vm.CrrQty = tog.Stock;

                }
            }
            else
            {
                vm.ItemId = 0;
                vm.Qty = 0;
                vm.SellingPrice = 0;
                vm.CostPrice = 0;
                vm.CrrQty = 0;

            }
            vm.UOM = _bllproduct.GetUOMById(_bllproduct.GetProductById(id).PurchasingUnit);
            var ddd = new JsonResult { Data = vm, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            return new JsonResult { Data = vm, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpGet]
        public ActionResult Print(long id, int printstatus)
        {

            //if (!_appmanager.SetPermissions(4, Session["loggeduserempcode"].ToString(), "GrnView"))
            //{
            //    @ViewBag.Permissions = "No user permissions to  View GRNs";
            //    return View("~/Views/Account/AccessDenied.cshtml");
            //}

            RequestNoteHeader rnheader = new RequestNoteHeader();
            rnheader = _bllrequestNote.GetRequestNoteById(id);
            rnheader.FromLocation = _blllocation.GetLocationById(rnheader.FromLocationId).LocationName;
            rnheader.ToLocation = _blllocation.GetLocationById(rnheader.ToLocationId).LocationName;
            //rnheader.FromDepartment = _blldepartment.GetDepartmentById(rnheader.FromDepartmentId).DepartmentName;
            //rnheader.ToDepartment = _blldepartment.GetDepartmentById(rnheader.ToDepartmentId).DepartmentName;

            rnheader.SysCompany = _bllcompany.GetCompanyById(rnheader.CompanyId);
            rnheader.Company = rnheader.SysCompany.CompanyName;
            rnheader.PrintStatus = printstatus;
            if (rnheader.DocumentStatus != 0)
            {
                if (rnheader.IsApproved == true)
                {
                    rnheader.DocState = "Approved";
                }
                else
                {
                    if (rnheader.DocumentStatus == 4)
                        rnheader.DocState = "Cancelled";
                    else if (rnheader.IsTempRequest)
                        rnheader.DocState = "Pending [Tempory Saved]";
                    else
                        rnheader.DocState = "Pending";
                }
            }
            else
            {
                rnheader.DocState = "N/A";
            }

            rnheader.RequestDetail
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
            var requestNoteDetailsList = rnheader.RequestDetail.ToList();
            requestNoteDetailsList.RemoveAll(d =>
            {
                var prd = _bllproduct.GetActiveProductById(d.ProductId);
                return prd == null;  // Remove if prd is null
            });

            rnheader.RequestDetail = requestNoteDetailsList;
            //rnheader.TotCostPrice = 0;


            return PartialView("~/Views/Receipts/RequestNoteView.cshtml", rnheader);
        }
    }
}