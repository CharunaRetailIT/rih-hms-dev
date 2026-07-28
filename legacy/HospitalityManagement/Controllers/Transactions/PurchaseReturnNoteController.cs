using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using RIT.HMS.BLL.MasterData;
using RIT.HMS.BLL.Common;
using RIT.HMS.BLL.TransactionData;
using RIT.HMS.Domain.Transactions;
using RIT.HMS.Domain.ViewModels;
using RIT.HMS.Domain.ViewModels.Reports;
using RIT.HMS.Domain;
using RIT.HMS.BLL.Configurations;

namespace HospitalityManagement.Controllers.Transactions
{
    [SessionTimeout]
    public class PurchaseReturnNoteController : Controller
    {
        //          
        // GET: /PurchaseReturnNote/

        private  BLL_Common _bllcommon;
        private  BLL_PurchaseReturn _bllpurchasereturn;
        private  BLL_GRN _bllgrn;
        private  BLL_Company _bllcompany;
        private readonly BLL_Product _bllproduct;
        private readonly BLL_Location _blllocation;
        private readonly BLL_Supplier _bllsupplier;
        private readonly BLL_Configuration _bllconfiguration;
        private readonly BLL_UserMaster _bllusermanster;
        private readonly BLL_DocStatus _blldocstatus;
        private readonly BLL_MonthEnd _bllmonthend;
        private readonly BLL_ProductStock _bllproductstock;
        private AppManager _appmanager;

        public PurchaseReturnNoteController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _bllcommon = new BLL_Common(cn);
            _bllpurchasereturn = new BLL_PurchaseReturn(cn);
             _bllgrn = new BLL_GRN(cn);
            _bllcompany = new BLL_Company(cn);
             _bllproduct = new BLL_Product(cn);
            _blllocation = new BLL_Location(cn);
             _bllsupplier = new BLL_Supplier(cn);
            _bllconfiguration = new BLL_Configuration(cn);
            _bllusermanster = new BLL_UserMaster(cn);
             _blldocstatus = new BLL_DocStatus(cn);
           _bllmonthend = new BLL_MonthEnd(cn);
          _bllproductstock = new BLL_ProductStock(cn);
            _appmanager = new AppManager(cn);

        }
        //  [Authorize(Roles = "PRNCreatee")]
        [HttpGet]
        public ActionResult CreatePRN()
        {
            //if (!_appmanager.SetPermissions(6, Session["loggeduserempcode"].ToString(), "PRNCreatee"))
            //{
            //    @ViewBag.Permissions = "No user permissions to Create PRNs";
            //    return View("~/Views/Account/AccessDenied.cshtml");
            //}
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            Session["POWizadUI"] = _bllconfiguration.GetConfiguration("UIWZ", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            ViewBag.IsAllItems = _bllconfiguration.GetConfiguration("DITEMS", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            Session["APPPRN"] = _bllconfiguration.GetConfiguration("APPPRN", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
           // ViewBag.productdata = _bllproduct.GetActiveProducts(companyid);
            var pheader = new PurchaseHeader();
            // var docnum = commonService.GetDocumentNo("PurchaseReturnNote", 1, "1", 2, true);
            int docid = _bllcommon.GetDcumentId("PurchaseReturnNote", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            var docnum = _bllcommon.GetDocumentNo("PurchaseReturnNote",
                                                    Convert.ToInt32(Session["loggeduserlocId"]),
                                                    "1",
                                                     docid,
                                                     true, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            pheader.DocumentNo = docnum;
            pheader.GRNDate = DateTime.Now;


            List<SelectListItem> FinishGoods = new List<SelectListItem>();
            SelectListItem defaultfinishgood = new SelectListItem();
            defaultfinishgood.Text = "-- Select an Item --";
            defaultfinishgood.Value = "0";
            FinishGoods.Add(defaultfinishgood);
            if (ViewBag.IsAllItems == true)
            {
                foreach (var prd in _bllproduct.GetActiveProducts(companyid))
                {
                    SelectListItem dbprd = new SelectListItem();
                    dbprd.Text = (prd.ProductCode + "-" + prd.ProductName);
                    dbprd.Value = prd.ProductId.ToString();
                    FinishGoods.Add(dbprd);
                }
            }
            Session["AllItemsToPRN"] = FinishGoods;


            return View("~/Views/Transactions/PurchaseReturnNote/CreatePRN.cshtml", pheader);
        }

      //  [Authorize(Roles = "PRNView")]
        [HttpGet]
        public ActionResult ViewAllPRN()
        {
            ViewBag.datefrom = DateTime.Now;
            ViewBag.dateTo = DateTime.Now; 

            // check permissions
            if (!_appmanager.SetPermissions(6, Session["loggeduserempcode"].ToString(), "PRNView"))
            {
                @ViewBag.Permissions = "No user permissions to View PRNs";
                return View("~/Views/Account/AccessDenied.cshtml");
            }
            //end check permissions

            var prns = _bllpurchasereturn.GetAllPRNs(Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            prns.ToList().ForEach(
                p =>
                {
                    if (p.DocumentStatus == 0)
                    {
                        p.DocState = "N/A";
                    }
                    else
                    {
                        p.DocState = _blldocstatus.GetDocStatusById(Convert.ToInt32(p.DocumentStatus)).Description;
                    }
                }
            );
            return View("~/Views/Transactions/PurchaseReturnNote/ViewAllPRNs.cshtml", prns);
        }

      //  [Authorize(Roles = "PRNView")]
        [HttpGet]
        public ActionResult FilterPRNs(int statusid, DateTime datefrom, DateTime dateto)
        {
            ViewBag.datefrom = datefrom;
            ViewBag.dateTo = dateto; 
            // check permissions
            //if (!_appmanager.SetPermissions(6, Session["loggeduserempcode"].ToString(), "PRNView"))
            //{
            //    @ViewBag.Permissions = "No user permissions to View PRNs";
            //    return View("~/Views/Account/AccessDenied.cshtml");
            //}
            //end check permissions


            DateTime FromDate = datefrom.Date;
            DateTime ToDate = dateto.Date;
            //var prns = _bllpurchasereturn.GetAllPRNs().ToList().Where(
            //    p => p.DocumentStatus == statusid &&
            //    p.GRNDate >= FromDate && p.GRNDate <= ToDate


            //    );
            var prns = _bllpurchasereturn.FilterAllPRNs(Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ToList().Where(
               p => p.DocumentStatus == statusid &&
               p.GRNDate >= FromDate && p.GRNDate <= ToDate
               );

            prns.ToList().ForEach(p =>
            {

                if (p.DocumentStatus != 0)
                {
                    p.DocState = _blldocstatus.GetDocStatusById(p.DocumentStatus).Description;
                }

            });


            return View("~/Views/Transactions/PurchaseReturnNote/ViewAllPRNs.cshtml", prns);
        }

       // [Authorize(Roles = "PRNView")]
        [HttpGet]
        public ActionResult Print(long id,int printstatus)
        {

            // check permissions
            //if (!_appmanager.SetPermissions(6, Session["loggeduserempcode"].ToString(), "PRNView"))
            //{
            //    @ViewBag.Permissions = "No user permissions to View PRNs";
            //    return View("~/Views/Account/AccessDenied.cshtml");
            //}
            //end check permissions

            PurchaseHeader prnheader = new PurchaseHeader();
            prnheader = _bllgrn.GetGRNById(id);            
            prnheader.Location = _blllocation.GetLocationById(prnheader.GRNLocationId).LocationName;

            prnheader.SysCompany = _bllcompany.GetCompanyById(prnheader.CompanyID);
            prnheader.Company = prnheader.SysCompany.CompanyName;
            prnheader.Supplier = _bllsupplier.GetSupplierById(prnheader.SupplierID);
            prnheader.PrintStatus = printstatus;

            if (prnheader.DocumentStatus != 0)
            {
                prnheader.DocState = _blldocstatus.GetDocStatusById(prnheader.DocumentStatus).Description;
            }
            else
            {
                prnheader.DocState = "N/A";
            }
            if(prnheader.GRNId!=0)
            {
                prnheader.BaseDocNo = _bllpurchasereturn.GetBaseDocNo(prnheader.GRNId).DocumentNo;
            }
            else
            {
                prnheader.BaseDocNo = "N/A";
            }
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());

            PurchaseHeader objPurchaseHeader = _bllgrn.GetGRNByDocNo(prnheader.BaseDocNo, companyid);
            if (objPurchaseHeader != null)
            {
                long GRNHeadID = _bllgrn.GetGRNByDocNo(prnheader.BaseDocNo, companyid).PurchaseHeaderId;

                List<PurchaseDetail> Qtys = _bllgrn.GetGRNDet(prnheader.GRNId);

                // Make sure both lists have the same count
                if (prnheader.PurchaseDetail.Count != Qtys.Count)
                {
                    // Handle the case where counts do not match
                    // You may throw an exception, log a warning, or handle it according to your requirements
                }
                for (int i = 0; i < prnheader.PurchaseDetail.Count; i++)
                {
                    // Match each item from poheader.PODetail with corresponding item from Qtys
                    prnheader.PurchaseDetail.ElementAt(i).GRNQuantity = Qtys[i].OrderQty;
                    prnheader.PurchaseDetail.ElementAt(i).OrderQty = Qtys[i].OrderQty;
                    var prd = _bllproduct.GetProductById(prnheader.PurchaseDetail.ElementAt(i).ProductID);
                    prnheader.PurchaseDetail.ElementAt(i).ProductName = prd.ProductName;
                    prnheader.PurchaseDetail.ElementAt(i).ProductCode = prd.ProductCode;
                }
            }





           


            //prnheader.PurchaseDetail.ToList().ForEach(d => {
            //    var prd = _bllproduct.GetProductById(d.ProductID);
            //    d.ProductName = prd.ProductName;
            //    d.ProductCode = prd.ProductCode;
            //});

            ViewBag.PurchaseHeaderId = prnheader.PurchaseHeaderId;
            return PartialView("~/Views/Receipts/PRNView.cshtml", prnheader);

            // return PartialView("~/Views/Receipts/PRNReceipt.cshtml", prnheader);
        }


      //  [Authorize(Roles = "PRNEdit")]
        [HttpGet]
        public ActionResult Edit(long id)
        {

            // check permissions
            //if (!_appmanager.SetPermissions(6, Session["loggeduserempcode"].ToString(), "PRNEdit"))
            //{
            //    @ViewBag.Permissions = "No user permissions to Edit PRNs";
            //    return View("~/Views/Account/AccessDenied.cshtml");
            //}
            //end check permissions


            Session["POWizadUI"] = _bllconfiguration.GetConfiguration("UIWZ", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            Session["APPPRN"] = _bllconfiguration.GetConfiguration("APPPRN", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            var prn = _bllpurchasereturn.GetPRNById(id);
            prn.GRNDetail = _bllpurchasereturn.GetPRNDetById(id).ToList();           
            prn.GRNDetail.ForEach
            (
                r =>
                {
                    r.ProductName = _bllproduct.GetProductById(r.ProductID).ProductName;

                }
            );
            prn.DocState = _blldocstatus.GetDocStatusById(prn.DocumentStatus).Description;
            ViewBag.GRNId = prn.GRNId;
            ViewBag.SupplierId = prn.SupplierID;
            ViewBag.CurrencyId = prn.CurrencyID;
            ViewBag.PaymentTermId = prn.PaymentTermID;
            @ViewBag.paymentMethodsId = prn.PaymentMethodId;
            @ViewBag.locationId = prn.GRNLocationId;
            @ViewBag.InitNetAmount = prn.NetAmount;
            @ViewBag.PRNType = prn.PRNType;
            ViewBag.EventId = prn.EventId;
            return View("~/Views/Transactions/PurchaseReturnNote/EditPRN.cshtml", prn);
        }


     //   [Authorize(Roles = "PRNEdit")]
        [HttpPost]
        public ActionResult Edit(PurchaseHeader prnheader)
        {

            // check permissions
            if (!_appmanager.SetPermissions(6, Session["loggeduserempcode"].ToString(), "PRNEdit"))
            {
                @ViewBag.Permissions = "No user permissions to Edit PRNs";
                return View("~/Views/Account/AccessDenied.cshtml");
            }
            //end check permissions
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var deptdata = _bllproduct.GetActiveProducts(companyid);
            ViewBag.IsAllItems = _bllconfiguration.GetConfiguration("DITEMS", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            ViewBag.productdata = deptdata;

            ViewBag.supplierId = prnheader.SupplierID;
            ViewBag.CurrencyId = prnheader.CurrencyID;
            ViewBag.PaymentTermId = prnheader.PaymentTermID;
            ViewBag.paymentMethodsId = prnheader.PaymentMethodId;
            ViewBag.locationId = prnheader.GRNLocationId;
            ViewBag.EventId = prnheader.EventId;


            bool EnableMonthEndProcess = _bllconfiguration.GetConfiguration("EnableMonthEndProcess", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;

            if (EnableMonthEndProcess)
            {
                int monthendstatus = _bllmonthend.CheckMonthEnd(Convert.ToInt32(prnheader.GRNLocationId), prnheader.GRNDate.Year, prnheader.GRNDate.Month);
                if (monthendstatus != 1)
                {
                    //ModelState.Clear();
                    //var pheader = new PurchaseHeader();           
                    //pheader.DocumentNo = prnheader.DocumentNo;
                    //pheader.GRNDate = DateTime.Now;
                    //// ViewBag.PurchaseHeaderId = prnheader.PurchaseHeaderId;
                    @ViewBag.Message = "4";
                    ViewBag.EventId = prnheader.EventId;
                    //ViewBag.CurrencyId = prnheader.CurrencyID;
                    //ViewBag.supplierId = prnheader.SupplierID;
                    //ViewBag.locationId = prnheader.GRNLocationId;
                    //ViewBag.PaymentTermId = prnheader.PaymentTermID;
                    //ViewBag.paymentMethodsId = prnheader.PaymentMethodId;
                    ViewBag.GRNId = prnheader.GRNId;
                    prnheader.Company = _bllcompany.GetCompanyById(prnheader.CompanyID).CompanyName;
                    var location = _blllocation.GetLocationById(prnheader.GRNLocationId);
                    if (location != null)
                        prnheader.Location = location.LocationName;
                    prnheader.GRNDetail.ToList().ForEach(d =>
                    {
                        var prd = _bllproduct.GetProductById(d.ProductID);
                        d.ProductName = prd.ProductName;
                        d.ProductCode = prd.ProductCode;
                    });


                    return View("~/Views/Transactions/PurchaseReturnNote/CreatePRN.cshtml", prnheader);
                }
            }


            if (prnheader.GRNDetail.Count == 0)
            {
                @ViewBag.Message = "3";
                return View("~/Views/Transactions/PurchaseReturnNote/EditPRN.cshtml", prnheader);
            }

            var id = RouteData.Values["id"] + Request.Url.Query;
          //  prnheader.PurchaseHeaderId = Convert.ToInt64(id);
            prnheader.ModifiedDate = DateTime.Now;
            prnheader.ModifiedUser = Session["loggeduser"].ToString();

            // grnheader.DocumentNo = grnheader.DocumentNo;

            // prnheader.DocumentNo = commonService.GetDocumentNo("PurchaseReturnNote", 1, "1", 2, false);
            int docid = _bllcommon.GetDcumentId("PurchaseReturnNote", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            var docnum = _bllcommon.GetDocumentNo("PurchaseReturnNote",
                                                    Convert.ToInt32(Session["loggeduserlocId"]),
                                                    Convert.ToString(prnheader.GRNLocationId),
                                                    docid,
                                                    false, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));

            prnheader.DocumentNo = docnum;
            prnheader.DocumentDate = DateTime.Now;
            prnheader.BatchNo = "0";
            prnheader.CompanyID= Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            if (Convert.ToBoolean(Session["APPPRN"]) == true)
            {
                prnheader.DocumentStatus = 2;
                prnheader.IsTempPRN = false;
            }
            else
            {
                prnheader.DocumentStatus = 3;
                prnheader.IsTempPRN = false;
            }

           // prnheader.DocumentStatus = 2;
            @ViewBag.PRNCode = prnheader.DocumentNo;
            //grnheader.JobClassId = 1;
            //grnheader.ExpiryDate = DateTime.Now;
            long GRNHeadID = _bllgrn.GetGRNByDocNo(prnheader.DocumentNo, companyid).PurchaseHeaderId;
            List<PurchaseDetail> Qtys = _bllgrn.GetGRNDet(GRNHeadID);

            // Make sure both lists have the same count
            if (prnheader.PurchaseDetail.Count != Qtys.Count)
            {
                // Handle the case where counts do not match
                // You may throw an exception, log a warning, or handle it according to your requirements
            }

            for (int i = 0; i < prnheader.PurchaseDetail.Count; i++)
            {
                // Match each item from poheader.PODetail with corresponding item from Qtys

                prnheader.PurchaseDetail.ElementAt(i).OrderQty = Qtys[i].OrderQty;
            }
            if (_bllpurchasereturn.UpdatePRN(prnheader))
            {
                @ViewBag.Message = "1";
                prnheader.Company = _bllcompany.GetCompanyById(prnheader.CompanyID).CompanyName;
                prnheader.Location = _blllocation.GetLocationById(prnheader.GRNLocationId).LocationName;
                prnheader.GRNDetail.ToList().ForEach(d => {
                    var prd = _bllproduct.GetProductById(d.ProductID);
                    d.ProductName = prd.ProductName;
                    d.ProductCode = prd.ProductCode;
                });
                ViewBag.PurchaseHeaderId = prnheader.PurchaseHeaderId;
                return View("~/Views/Transactions/PurchaseReturnNote/EditPRN.cshtml", prnheader);
                // return PartialView("~/Views/Receipts/PRNReceipt.cshtml", prnheader);
            }
            else
            {
                @ViewBag.Message = "2";
                return View("~/Views/Transactions/PurchaseReturnNote/EditPRN.cshtml", prnheader);
            }
          
           
        }

    //    [Authorize(Roles = "PRNEdit")]
        [HttpPost]
        public ActionResult TempEdit(PurchaseHeader prnheader)
        {

            // check permissions
            if (!_appmanager.SetPermissions(6, Session["loggeduserempcode"].ToString(), "PRNEdit"))
            {
                @ViewBag.Permissions = "No user permissions to Edit PRNs";
                return View("~/Views/Account/AccessDenied.cshtml");
            }
            //end check permissions

            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var deptdata = _bllproduct.GetActiveProducts(companyid);
            ViewBag.IsAllItems = _bllconfiguration.GetConfiguration("DITEMS", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;

            ViewBag.productdata = deptdata;
            ViewBag.supplierId = prnheader.SupplierID;
            ViewBag.CurrencyId = prnheader.CurrencyID;
            ViewBag.PaymentTermId = prnheader.PaymentTermID;
            ViewBag.paymentMethodsId = prnheader.PaymentMethodId;
            ViewBag.locationId = prnheader.GRNLocationId;
            ViewBag.EventId = prnheader.EventId;

            bool EnableMonthEndProcess = _bllconfiguration.GetConfiguration("EnableMonthEndProcess", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;

            if (EnableMonthEndProcess)
            {

                int monthendstatus = _bllmonthend.CheckMonthEnd(Convert.ToInt32(prnheader.GRNLocationId), prnheader.GRNDate.Year, prnheader.GRNDate.Month);
                if (monthendstatus != 1)
                {
                    @ViewBag.Message = "4";

                    ViewBag.EventId = prnheader.EventId;
                    ViewBag.GRNId = prnheader.GRNId;
                    prnheader.Company = _bllcompany.GetCompanyById(prnheader.CompanyID).CompanyName;
                    var location = _blllocation.GetLocationById(prnheader.GRNLocationId);
                    if (location != null)
                        prnheader.Location = location.LocationName;
                    prnheader.GRNDetail.ToList().ForEach(d =>
                    {
                        var prd = _bllproduct.GetProductById(d.ProductID);
                        d.ProductName = prd.ProductName;
                        d.ProductCode = prd.ProductCode;
                    });


                    return View("~/Views/Transactions/PurchaseReturnNote/CreatePRN.cshtml", prnheader);
                }
            }

            
            if (prnheader.GRNDetail.Count == 0)
            {
                @ViewBag.Message = "3";
                return View("~/Views/Transactions/PurchaseReturnNote/EditPRN.cshtml", prnheader);
            }

            var id = RouteData.Values["id"] + Request.Url.Query;
            //  prnheader.PurchaseHeaderId = Convert.ToInt64(id);
            prnheader.ModifiedDate = DateTime.Now;
            prnheader.ModifiedUser = Session["loggeduser"].ToString();

            // grnheader.DocumentNo = grnheader.DocumentNo;

            // prnheader.DocumentNo = commonService.GetDocumentNo("PurchaseReturnNote", 1, "1", 2, false);
            //int docid = commonService.GetDcumentId("PurchaseReturnNote");
            //var docnum = commonService.GetDocumentNo("PurchaseReturnNote",
            //                                        Convert.ToInt32(Session["loggeduserlocId"]),
            //                                        "1",
            //                                         docid,
            //                                         false);

           // prnheader.DocumentNo = docnum;
            prnheader.DocumentDate = DateTime.Now;
            prnheader.BatchNo = "0";
            @ViewBag.PRNCode = prnheader.DocumentNo;

            prnheader.IsTempPRN = true;
            prnheader.DocumentStatus = 1;

            //grnheader.JobClassId = 1;
            //grnheader.ExpiryDate = DateTime.Now;
            if (_bllpurchasereturn.UpdatePRN(prnheader))
            {
                // @ViewBag.Message = "1";
                prnheader.Company = _bllcompany.GetCompanyById(prnheader.CompanyID).CompanyName;
                prnheader.Location = _blllocation.GetLocationById(prnheader.GRNLocationId).LocationName;
                prnheader.GRNDetail.ToList().ForEach(d => {
                    var prd = _bllproduct.GetProductById(d.ProductID);
                    d.ProductName = prd.ProductName;
                    d.ProductCode = prd.ProductCode;
                });
                ViewBag.PurchaseHeaderId = prnheader.PurchaseHeaderId;
                return View("~/Views/Transactions/PurchaseReturnNote/EditPRN.cshtml", prnheader);
                // return PartialView("~/Views/Receipts/PRNReceipt.cshtml", prnheader);
            }
            else
            {
                @ViewBag.Message = "2";
                return View("~/Views/Transactions/PurchaseReturnNote/EditPRN.cshtml", prnheader);
            }


        }

        // [Authorize(Roles = "PRNApprove")]
     //   [Authorize(Roles = "PRNEdit")]
        [HttpPost]
        public ActionResult Approve(PurchaseHeader prnheader)
        {

            // check permissions
            if (!_appmanager.SetPermissions(6, Session["loggeduserempcode"].ToString(), "PRNApprove"))
            {
                @ViewBag.Permissions = "No user permissions to Approve PRNs";
                return View("~/Views/Account/AccessDenied.cshtml");
            }
            //end check permissions

            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var deptdata = _bllproduct.GetActiveProducts(companyid);
            ViewBag.IsAllItems = _bllconfiguration.GetConfiguration("DITEMS", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            ViewBag.productdata = deptdata;
            ViewBag.supplierId = prnheader.SupplierID;
            ViewBag.CurrencyId = prnheader.CurrencyID;
            ViewBag.PaymentTermId = prnheader.PaymentTermID;
            ViewBag.paymentMethodsId = prnheader.PaymentMethodId;
            ViewBag.locationId = prnheader.GRNLocationId;
            if (prnheader.GRNDetail.Count == 0)
            {
                @ViewBag.Message = "3";
                return View("~/Views/Transactions/PurchaseReturnNote/EditPRN.cshtml", prnheader);
            }

            var id = RouteData.Values["id"] + Request.Url.Query;
            //  prnheader.PurchaseHeaderId = Convert.ToInt64(id);
            prnheader.ModifiedDate = DateTime.Now;
            prnheader.ModifiedUser = Session["loggeduser"].ToString();

            
            prnheader.DocumentDate = DateTime.Now;

            // doc status : saved
            prnheader.DocumentStatus = 3;

            prnheader.BatchNo = "0";
            @ViewBag.PRNCode = prnheader.DocumentNo;
            prnheader.IsTempPRN = false;

            long GRNHeadID = _bllgrn.GetGRNByDocNo(prnheader.DocumentNo, companyid).PurchaseHeaderId;
            List<PurchaseDetail> Qtys = _bllgrn.GetGRNDet(GRNHeadID);

            // Make sure both lists have the same count
            if (prnheader.GRNDetail.Count != Qtys.Count)
            {
                // Handle the case where counts do not match
                // You may throw an exception, log a warning, or handle it according to your requirements
            }

            for (int i = 0; i < prnheader.GRNDetail.Count; i++)
            {
                // Match each item from poheader.PODetail with corresponding item from Qtys

                prnheader.GRNDetail.ElementAt(i).OrderQty = Qtys[i].OrderQty;
            }

            //grnheader.JobClassId = 1;
            //grnheader.ExpiryDate = DateTime.Now;
            if (_bllpurchasereturn.UpdatePRN(prnheader))
            {
                
                prnheader.Company = _bllcompany.GetCompanyById(prnheader.CompanyID).CompanyName;
                prnheader.Location = _blllocation.GetLocationById(prnheader.GRNLocationId).LocationName;
                prnheader.GRNDetail.ToList().ForEach(d => {
                    var prd = _bllproduct.GetProductById(d.ProductID);
                    d.ProductName = prd.ProductName;
                    d.ProductCode = prd.ProductCode;
                });
                ViewBag.PurchaseHeaderId = prnheader.PurchaseHeaderId;
                return View("~/Views/Transactions/PurchaseReturnNote/EditPRN.cshtml", prnheader);
             
            }
            else
            {
                @ViewBag.Message = "2";
                return View("~/Views/Transactions/PurchaseReturnNote/EditPRN.cshtml", prnheader);
            }


        }


        // [Authorize(Roles = "PRNReject")]
      //  [Authorize(Roles = "PRNEdit")]
        [HttpPost]
        public ActionResult Reject(PurchaseHeader prnheader)
        {


            // check permissions
            if (!_appmanager.SetPermissions(6, Session["loggeduserempcode"].ToString(), "PRNReject"))
            {
                @ViewBag.Permissions = "No user permissions to Reject PRNs";
                return View("~/Views/Account/AccessDenied.cshtml");
            }
            //end check permissions
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var deptdata = _bllproduct.GetActiveProducts(companyid);
            ViewBag.IsAllItems = _bllconfiguration.GetConfiguration("DITEMS", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            ViewBag.productdata = deptdata;
            ViewBag.supplierId = prnheader.SupplierID;
            ViewBag.CurrencyId = prnheader.CurrencyID;
            ViewBag.PaymentTermId = prnheader.PaymentTermID;
            ViewBag.paymentMethodsId = prnheader.PaymentMethodId;
            ViewBag.locationId = prnheader.GRNLocationId;
            ViewBag.GRNId = prnheader.GRNId;
            if (prnheader.GRNDetail.Count == 0)
            {
                @ViewBag.Message = "3";
                return View("~/Views/Transactions/PurchaseReturnNote/EditPRN.cshtml", prnheader);
            }

            if (string.IsNullOrEmpty(prnheader.RejectReason))
            {
                @ViewBag.Message = "5";

                prnheader.GRNDetail.ToList().ForEach(d => {
                    var prd = _bllproduct.GetProductById(d.ProductID);
                    d.ProductName = prd.ProductName;
                    d.ProductCode = prd.ProductCode;
                });


                return View("~/Views/Transactions/PurchaseReturnNote/EditPRN.cshtml", prnheader);
            }

                var id = RouteData.Values["id"] + Request.Url.Query;
            //  prnheader.PurchaseHeaderId = Convert.ToInt64(id);
            prnheader.ModifiedDate = DateTime.Now;
            prnheader.ModifiedUser = Session["loggeduser"].ToString();


            prnheader.DocumentDate = DateTime.Now;

            // doc status : Rejected
            prnheader.DocumentStatus = 4;

            prnheader.BatchNo = "0";
            @ViewBag.PRNCode = prnheader.DocumentNo;
            prnheader.IsTempPRN = false;
            //grnheader.JobClassId = 1;
            //grnheader.ExpiryDate = DateTime.Now;
            if (_bllpurchasereturn.UpdatePRN(prnheader))
            {

                prnheader.Company = _bllcompany.GetCompanyById(prnheader.CompanyID).CompanyName;
                prnheader.Location = _blllocation.GetLocationById(prnheader.GRNLocationId).LocationName;
                prnheader.GRNDetail.ToList().ForEach(d => {
                    var prd = _bllproduct.GetProductById(d.ProductID);
                    d.ProductName = prd.ProductName;
                    d.ProductCode = prd.ProductCode;
                });
                ViewBag.PurchaseHeaderId = prnheader.PurchaseHeaderId;
                return View("~/Views/Transactions/PurchaseReturnNote/EditPRN.cshtml", prnheader);

            }
            else
            {
                @ViewBag.Message = "2";
                return View("~/Views/Transactions/PurchaseReturnNote/EditPRN.cshtml", prnheader);
            }


        }


        // [Authorize(Roles = "PRNCancel")]
      //  [Authorize(Roles = "PRNEdit")]
        [HttpPost]
        public ActionResult Cancel(PurchaseHeader prnheader)
        {

            // check permissions
            if (!_appmanager.SetPermissions(6, Session["loggeduserempcode"].ToString(), "PRNCancel"))
            {
                @ViewBag.Permissions = "No user permissions to Cancel PRNs";
                return View("~/Views/Account/AccessDenied.cshtml");
            }
            //end check permissions
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var deptdata = _bllproduct.GetActiveProducts(companyid);
            ViewBag.IsAllItems = _bllconfiguration.GetConfiguration("DITEMS", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            ViewBag.productdata = deptdata;
            ViewBag.supplierId = prnheader.SupplierID;
            ViewBag.CurrencyId = prnheader.CurrencyID;
            ViewBag.PaymentTermId = prnheader.PaymentTermID;
            ViewBag.paymentMethodsId = prnheader.PaymentMethodId;
            ViewBag.locationId = prnheader.GRNLocationId;
            ViewBag.GRNId = prnheader.GRNId;
            ViewBag.EventId = prnheader.EventId;
            if (prnheader.GRNDetail.Count == 0)
            {
                @ViewBag.Message = "3";
                return View("~/Views/Transactions/PurchaseReturnNote/EditPRN.cshtml", prnheader);
            }

            if (string.IsNullOrEmpty(prnheader.CancelReason))
            {
                @ViewBag.Message = "6";
                prnheader.GRNDetail.ToList().ForEach(d => {
                    var prd = _bllproduct.GetProductById(d.ProductID);
                    d.ProductName = prd.ProductName;
                    d.ProductCode = prd.ProductCode;
                });
                return View("~/Views/Transactions/PurchaseReturnNote/EditPRN.cshtml", prnheader);
            }
                var id = RouteData.Values["id"] + Request.Url.Query;
            //  prnheader.PurchaseHeaderId = Convert.ToInt64(id);
            prnheader.ModifiedDate = DateTime.Now;
            prnheader.ModifiedUser = Session["loggeduser"].ToString();
            prnheader.DocumentDate = DateTime.Now;

            // doc status : Cancelled
            prnheader.DocumentStatus = 5;

            prnheader.BatchNo = "0";
            @ViewBag.PRNCode = prnheader.DocumentNo;
            prnheader.IsTempPRN = false;
            //grnheader.JobClassId = 1;
            //grnheader.ExpiryDate = DateTime.Now;
            if (_bllpurchasereturn.UpdatePRN(prnheader))
            {

                prnheader.Company = _bllcompany.GetCompanyById(prnheader.CompanyID).CompanyName;
                prnheader.Location = _blllocation.GetLocationById(prnheader.GRNLocationId).LocationName;
                prnheader.GRNDetail.ToList().ForEach(d => {
                    var prd = _bllproduct.GetProductById(d.ProductID);
                    d.ProductName = prd.ProductName;
                    d.ProductCode = prd.ProductCode;
                });
                ViewBag.PurchaseHeaderId = prnheader.PurchaseHeaderId;
                return View("~/Views/Transactions/PurchaseReturnNote/EditPRN.cshtml", prnheader);

            }
            else
            {
                @ViewBag.Message = "2";
                return View("~/Views/Transactions/PurchaseReturnNote/EditPRN.cshtml", prnheader);
            }


        }

        // [Authorize(Roles = "PRNReOpen")]
      //  [Authorize(Roles = "PRNEdit")]
        [HttpPost]
        public ActionResult ReOpene(PurchaseHeader prnheader)
        {


            // check permissions
            if (!_appmanager.SetPermissions(6, Session["loggeduserempcode"].ToString(), "PRNReOpen"))
            {
                @ViewBag.Permissions = "No user permissions to Re Open PRNs";
                return View("~/Views/Account/AccessDenied.cshtml");
            }
            //end check permissions
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var deptdata = _bllproduct.GetActiveProducts(companyid);
            ViewBag.IsAllItems = _bllconfiguration.GetConfiguration("DITEMS", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            ViewBag.productdata = deptdata;
            ViewBag.supplierId = prnheader.SupplierID;
            ViewBag.CurrencyId = prnheader.CurrencyID;
            ViewBag.PaymentTermId = prnheader.PaymentTermID;
            ViewBag.paymentMethodsId = prnheader.PaymentMethodId;
            ViewBag.locationId = prnheader.GRNLocationId;
            ViewBag.EventId = prnheader.EventId;
            if (prnheader.GRNDetail.Count == 0)
            {
                @ViewBag.Message = "3";
                return View("~/Views/Transactions/PurchaseReturnNote/EditPRN.cshtml", prnheader);
            }

            bool EnableMonthEndProcess = _bllconfiguration.GetConfiguration("EnableMonthEndProcess", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;

            if (EnableMonthEndProcess)
            {

                // check Month End Stataus
                // int year = poheader.PODate.Year;
                int monthendstatus = _bllmonthend.CheckMonthEnd(Convert.ToInt32(prnheader.GRNLocationId),
                prnheader.GRNDate.Year, prnheader.GRNDate.Month);
                if (monthendstatus != 1)
                {
                    @ViewBag.Message = "4";
                    return View("~/Views/Transactions/PurchaseReturnNote/EditPRN.cshtml", prnheader);
                }
            }


            var id = RouteData.Values["id"] + Request.Url.Query;
            //  prnheader.PurchaseHeaderId = Convert.ToInt64(id);
            prnheader.ModifiedDate = DateTime.Now;
            prnheader.ModifiedUser = Session["loggeduser"].ToString();


            prnheader.DocumentDate = DateTime.Now;

            // doc status : Re opened
            prnheader.DocumentStatus = 6;

            prnheader.BatchNo = "0";
            @ViewBag.PRNCode = prnheader.DocumentNo;
            prnheader.IsTempPRN = false;
            //grnheader.JobClassId = 1;
            //grnheader.ExpiryDate = DateTime.Now;
            if (_bllpurchasereturn.UpdatePRN(prnheader))
            {

                prnheader.Company = _bllcompany.GetCompanyById(prnheader.CompanyID).CompanyName;
                prnheader.Location = _blllocation.GetLocationById(prnheader.GRNLocationId).LocationName;
                prnheader.GRNDetail.ToList().ForEach(d => {
                    var prd = _bllproduct.GetProductById(d.ProductID);
                    d.ProductName = prd.ProductName;
                    d.ProductCode = prd.ProductCode;
                });
                ViewBag.PurchaseHeaderId = prnheader.PurchaseHeaderId;
                return View("~/Views/Transactions/PurchaseReturnNote/EditPRN.cshtml", prnheader);

            }
            else
            {
                @ViewBag.Message = "2";
                return View("~/Views/Transactions/PurchaseReturnNote/EditPRN.cshtml", prnheader);
            }
        }

        //[Authorize(Roles = "PRNForsefullyClose")]
      //  [Authorize(Roles = "PRNEdit")]
        [HttpPost]
        public ActionResult ForcefullyClose(PurchaseHeader prnheader)
        {

            // check permissions
            if (!_appmanager.SetPermissions(6, Session["loggeduserempcode"].ToString(), "PRNForsefullyClose"))
            {
                @ViewBag.Permissions = "No user permissions to Forcefully Close PRNs";
                return View("~/Views/Account/AccessDenied.cshtml");
            }
            //end check permissions
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var deptdata = _bllproduct.GetActiveProducts(companyid);
            ViewBag.IsAllItems = _bllconfiguration.GetConfiguration("DITEMS", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            ViewBag.productdata = deptdata;
            ViewBag.supplierId = prnheader.SupplierID;
            ViewBag.CurrencyId = prnheader.CurrencyID;
            ViewBag.PaymentTermId = prnheader.PaymentTermID;
            ViewBag.paymentMethodsId = prnheader.PaymentMethodId;
            ViewBag.locationId = prnheader.GRNLocationId;
            if (prnheader.GRNDetail.Count == 0)
            {
                @ViewBag.Message = "3";
                return View("~/Views/Transactions/PurchaseReturnNote/EditPRN.cshtml", prnheader);
            }

            var id = RouteData.Values["id"] + Request.Url.Query;
            //  prnheader.PurchaseHeaderId = Convert.ToInt64(id);
            prnheader.ModifiedDate = DateTime.Now;
            prnheader.ModifiedUser = Session["loggeduser"].ToString();


            prnheader.DocumentDate = DateTime.Now;

            // doc status : Cancelled
            prnheader.DocumentStatus = 7;

            prnheader.BatchNo = "0";
            @ViewBag.PRNCode = prnheader.DocumentNo;
            prnheader.IsTempPRN = false;
            //grnheader.JobClassId = 1;
            //grnheader.ExpiryDate = DateTime.Now;
            if (_bllpurchasereturn.UpdatePRN(prnheader))
            {

                prnheader.Company = _bllcompany.GetCompanyById(prnheader.CompanyID).CompanyName;
                prnheader.Location = _blllocation.GetLocationById(prnheader.GRNLocationId).LocationName;
                prnheader.GRNDetail.ToList().ForEach(d => {
                    var prd = _bllproduct.GetProductById(d.ProductID);
                    d.ProductName = prd.ProductName;
                    d.ProductCode = prd.ProductCode;
                });
                ViewBag.PurchaseHeaderId = prnheader.PurchaseHeaderId;
                return View("~/Views/Transactions/PurchaseReturnNote/EditPRN.cshtml", prnheader);

            }
            else
            {
                @ViewBag.Message = "2";
                return View("~/Views/Transactions/PurchaseReturnNote/EditPRN.cshtml", prnheader);
            }


        }

   //     [Authorize(Roles = "PRNCreatee")]
        [HttpPost]
        public ActionResult CreatePRN(PurchaseHeader prnheader)
        {

            // check permissions
            if (!_appmanager.SetPermissions(6, Session["loggeduserempcode"].ToString(), "PRNCreatee"))
            {
                @ViewBag.Permissions = "No user permissions to Create PRNs";
                return View("~/Views/Account/AccessDenied.cshtml");
            }
            //end check permissions
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var deptdata = _bllproduct.GetActiveProducts(companyid);
            ViewBag.IsAllItems = _bllconfiguration.GetConfiguration("DITEMS", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            ViewBag.productdata = deptdata;

            //ViewBag.supplierId = prnheader.SupplierID;
            //ViewBag.CurrencyId = prnheader.CurrencyID;
            //ViewBag.PaymentTermId = prnheader.PaymentTermID;
            //ViewBag.paymentMethodsId = prnheader.PaymentMethodId;
            //ViewBag.locationId = prnheader.GRNLocationId;
            //ViewBag.EventId = prnheader.EventId;


            bool EnableMonthEndProcess = _bllconfiguration.GetConfiguration("EnableMonthEndProcess", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;

            if (EnableMonthEndProcess)
            {

                // check Month End Stataus
                // int year = poheader.PODate.Year;
                int monthendstatus = _bllmonthend.CheckMonthEnd(Convert.ToInt32(prnheader.GRNLocationId), prnheader.GRNDate.Year, prnheader.GRNDate.Month);
                if (monthendstatus != 1)
                {
                    //ModelState.Clear();
                    //var pheader = new PurchaseHeader();           
                    //pheader.DocumentNo = prnheader.DocumentNo;
                    //pheader.GRNDate = DateTime.Now;
                    //// ViewBag.PurchaseHeaderId = prnheader.PurchaseHeaderId;
                    @ViewBag.Message = "4";

                    ViewBag.EventId = prnheader.EventId;
                    ViewBag.CurrencyId = prnheader.CurrencyID;
                    ViewBag.supplierId = prnheader.SupplierID;
                    ViewBag.locationId = prnheader.GRNLocationId;
                    ViewBag.PaymentTermId = prnheader.PaymentTermID;
                    ViewBag.paymentMethodsId = prnheader.PaymentMethodId;
                    ViewBag.GRNId = prnheader.GRNId;
                    prnheader.Company = _bllcompany.GetCompanyById(prnheader.CompanyID).CompanyName;
                    var location = _blllocation.GetLocationById(prnheader.GRNLocationId);
                    if (location != null)
                        prnheader.Location = location.LocationName;
                    prnheader.GRNDetail.ToList().ForEach(d =>
                    {
                        var prd = _bllproduct.GetProductById(d.ProductID);
                        d.ProductName = prd.ProductName;
                        d.ProductCode = prd.ProductCode;
                    });


                    return View("~/Views/Transactions/PurchaseReturnNote/CreatePRN.cshtml", prnheader);
                }
            }




            if (prnheader.GRNDetail.Count == 0)
            {
                @ViewBag.Message = "3";
                return View("~/Views/Transactions/PurchaseReturnNote/CreatePRN.cshtml", prnheader);
            }

            prnheader.CreatedUser = Session["loggeduser"].ToString();
            prnheader.CreatedDate = DateTime.Now;
            prnheader.ModifiedDate = DateTime.Now;
            prnheader.ModifiedUser = Session["loggeduser"].ToString();
            prnheader.DataTransfer = 0;
            prnheader.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
            //prnheader.GroupOfCompanyID = 1;
            //prnheader.CompanyID = 1;
            if (Convert.ToBoolean(Session["APPPRN"]) == true)
            {
                prnheader.DocumentStatus = 2;
            }
            else
            {
                prnheader.DocumentStatus = 3;
            }

            if (ModelState.IsValid == false)
            {
                ViewBag.EventId = prnheader.EventId;
                ViewBag.CurrencyId = prnheader.CurrencyID;
                ViewBag.supplierId = prnheader.SupplierID;
                ViewBag.locationId = prnheader.GRNLocationId;
                ViewBag.PaymentTermId = prnheader.PaymentTermID;
                ViewBag.paymentMethodsId = prnheader.PaymentMethodId;
                ViewBag.GRNId = prnheader.GRNId;
                prnheader.Company = _bllcompany.GetCompanyById(prnheader.CompanyID).CompanyName;
                prnheader.Location = _blllocation.GetLocationById(prnheader.GRNLocationId).LocationName;
                prnheader.GRNDetail.ToList().ForEach(d =>
                {
                    var prd = _bllproduct.GetProductById(d.ProductID);
                    d.ProductName = prd.ProductName;
                    d.ProductCode = prd.ProductCode;
                });

                @ViewBag.Message = "2";
                return View("~/Views/Transactions/PurchaseReturnNote/CreatePRN.cshtml", prnheader);
            }

            //   var docnum = commonService.GetDocumentNo("PurchaseReturnNote", 1, "1", 2, false);
            int docid = _bllcommon.GetDcumentId("PurchaseReturnNote", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            var docnum = _bllcommon.GetDocumentNo("PurchaseReturnNote",
                                                   Convert.ToInt32(Session["loggeduserlocId"]),
                                                    Convert.ToString(prnheader.GRNLocationId),
                                                   docid,
                                                   false, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));

            prnheader.DocumentID = docid;
            prnheader.DocumentNo = docnum;
            prnheader.DocumentDate = DateTime.Now;
            prnheader.BatchNo = "0";
            prnheader.OtherDocNo = "0";
            prnheader.CompanyID= Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            // prnheader.GRNId=
            @ViewBag.GRNCode = prnheader.DocumentNo;
           // long GRNHeadID = _bllgrn.GetGRNByDocNo(prnheader.DocumentNo, companyid).PurchaseHeaderId;
            List<PurchaseDetail> Qtys = _bllgrn.GetGRNDet(prnheader.GRNId);

            // Make sure both lists have the same count
            if (prnheader.GRNDetail.Count != Qtys.Count)
            {
                // Handle the case where counts do not match
                // You may throw an exception, log a warning, or handle it according to your requirements
            }

            if (Qtys.Count > 0)
            {
                for (int i = 0; i < prnheader.GRNDetail.Count; i++)
                {
                    // Match each item from poheader.PODetail with corresponding item from Qtys

                    prnheader.GRNDetail.ElementAt(i).OrderQty = Qtys[i].OrderQty;
                }
            }
            if (_bllpurchasereturn.SavePRN(prnheader))
            {
                @ViewBag.Message = "1";

                //prnheader.Company = _bllcompany.GetCompanyById(prnheader.CompanyID).CompanyName;
                //prnheader.Location = _blllocation.GetLocationById(prnheader.GRNLocationId).LocationName;
                //prnheader.GRNDetail.ToList().ForEach(d => {
                //    var prd = _bllproduct.GetProductById(d.ProductID);
                //    d.ProductName = prd.ProductName;
                //    d.ProductCode = prd.ProductCode;
                //});
                ModelState.Clear();
                var pheader = new PurchaseHeader();
                // var docnum = commonService.GetDocumentNo("PurchaseReturnNote", 1, "1", 2, true);
                docid = _bllcommon.GetDcumentId("PurchaseReturnNote", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                docnum = _bllcommon.GetDocumentNo("PurchaseReturnNote",
                                                        Convert.ToInt32(Session["loggeduserlocId"]),
                                                        "1",
                                                         docid,
                                                         true, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));

                pheader.DocumentNo = docnum;
                pheader.GRNDate = DateTime.Now;


                ViewBag.PurchaseHeaderId = prnheader.PurchaseHeaderId;
                return View("~/Views/Transactions/PurchaseReturnNote/CreatePRN.cshtml", pheader);
            }
            else
            {
                ViewBag.EventId = prnheader.EventId;
                ViewBag.CurrencyId = prnheader.CurrencyID;
                ViewBag.supplierId = prnheader.SupplierID;
                ViewBag.locationId = prnheader.GRNLocationId;
                ViewBag.PaymentTermId = prnheader.PaymentTermID;
                ViewBag.paymentMethodsId = prnheader.PaymentMethodId;
                ViewBag.GRNId = prnheader.GRNId;
                prnheader.Company = _bllcompany.GetCompanyById(prnheader.CompanyID).CompanyName;
                prnheader.Location = _blllocation.GetLocationById(prnheader.GRNLocationId).LocationName;
                prnheader.GRNDetail.ToList().ForEach(d =>
                {
                    var prd = _bllproduct.GetProductById(d.ProductID);
                    d.ProductName = prd.ProductName;
                    d.ProductCode = prd.ProductCode;
                });

                @ViewBag.Message = "2";
                return View("~/Views/Transactions/PurchaseReturnNote/CreatePRN.cshtml", prnheader);
            }


        }


      //  [Authorize(Roles = "PRNCreatee")]
        [HttpPost]
        public ActionResult TempSavePRN(PurchaseHeader prnheader)
        {
            // check permissions

            if (!_appmanager.SetPermissions(6, Session["loggeduserempcode"].ToString(), "PRNCreatee"))
            {
                @ViewBag.Permissions = "No user permissions to Create PRNs";
                return View("~/Views/Account/AccessDenied.cshtml");
            }

            //end check permissions
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var deptdata = _bllproduct.GetActiveProducts(companyid);
            ViewBag.IsAllItems = _bllconfiguration.GetConfiguration("DITEMS", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            ViewBag.productdata = deptdata;


            bool EnableMonthEndProcess = _bllconfiguration.GetConfiguration("EnableMonthEndProcess", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;

            if (EnableMonthEndProcess)
            {
                int monthendstatus = _bllmonthend.CheckMonthEnd(Convert.ToInt32(prnheader.GRNLocationId),
                                                                            prnheader.GRNDate.Year,
                                                                            prnheader.GRNDate.Month);
                prnheader.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
                if (monthendstatus != 1)
                {
                    //ModelState.Clear();
                    //var pheader = new PurchaseHeader();
                    //pheader.DocumentNo = prnheader.DocumentNo;
                    //pheader.GRNDate = DateTime.Now;

                    ViewBag.Message = "4";
                    // ViewBag.PurchaseHeaderId = prnheader.PurchaseHeaderId;

                    ViewBag.EventId = prnheader.EventId;
                    ViewBag.CurrencyId = prnheader.CurrencyID;
                    ViewBag.supplierId = prnheader.SupplierID;
                    ViewBag.locationId = prnheader.GRNLocationId;
                    ViewBag.PaymentTermId = prnheader.PaymentTermID;
                    ViewBag.paymentMethodsId = prnheader.PaymentMethodId;
                    ViewBag.GRNId = prnheader.GRNId;
                    prnheader.Company = _bllcompany.GetCompanyById(prnheader.CompanyID).CompanyName;
                    var location = _blllocation.GetLocationById(prnheader.GRNLocationId);
                    if (location != null)
                        prnheader.Location = location.LocationName;
                    prnheader.GRNDetail.ToList().ForEach(d =>
                    {
                        var prd = _bllproduct.GetProductById(d.ProductID);
                        d.ProductName = prd.ProductName;
                        d.ProductCode = prd.ProductCode;
                    });


                    return View("~/Views/Transactions/PurchaseReturnNote/CreatePRN.cshtml", prnheader);
                }
            }

            if (prnheader.GRNDetail.Count == 0)
            {
                ViewBag.EventId = prnheader.EventId;
                ViewBag.CurrencyId = prnheader.CurrencyID;
                ViewBag.supplierId = prnheader.SupplierID;
                ViewBag.locationId = prnheader.LocationId;
                ViewBag.PaymentTermId = prnheader.PaymentTermID;
                ViewBag.paymentMethodsId = prnheader.PaymentMethodId;
                ViewBag.GRNId = prnheader.GRNId;
                @ViewBag.Message = "3";
                return View("~/Views/Transactions/PurchaseReturnNote/CreatePRN.cshtml", prnheader);
            }

           

            prnheader.CreatedUser = Session["loggeduser"].ToString();
            prnheader.CreatedDate = DateTime.Now;
            prnheader.ModifiedDate = DateTime.Now;
            prnheader.ModifiedUser = Session["loggeduser"].ToString();
            prnheader.DataTransfer = 0;
            prnheader.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
            //prnheader.GroupOfCompanyID = 1;
            //prnheader.CompanyID = 1;
            prnheader.DocumentStatus = 1;
            prnheader.IsTempPRN = true;


            //  var docnum = commonService.GetDocumentNo("PurchaseReturnNote", 1, "1", 2, true);
            int docid = _bllcommon.GetDcumentId("PurchaseReturnNote", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
           // var docnum = _bllcommon.GetDocumentNo("PurchaseReturnNote",
                                                //   Convert.ToInt32(Session["loggeduserlocId"]),
                                                //   "1",
                                                //   docid,
                                                //   true);

            prnheader.DocumentID = docid;
           // prnheader.DocumentNo = docnum;
            prnheader.DocumentDate = DateTime.Now;
            prnheader.OtherDocNo = "0";
            //grnheader.JobClassId = 1;
            //grnheader.ExpiryDate = DateTime.Now;
            @ViewBag.GRNCode = prnheader.DocumentNo;
            if (_bllpurchasereturn.SavePRN(prnheader))
            {


                //prnheader.Company = _bllcompany.GetCompanyById(prnheader.CompanyID).CompanyName;
                //prnheader.Location = _blllocation.GetLocationById(prnheader.GRNLocationId).LocationName;
                //prnheader.GRNDetail.ToList().ForEach(d => {
                //    var prd = _bllproduct.GetProductById(d.ProductID);
                //    d.ProductName = prd.ProductName;
                //    d.ProductCode = prd.ProductCode;
                //});

                ModelState.Clear();
                PurchaseHeader newprnheader =  new PurchaseHeader();
               // newprnheader.DocumentID = docid;
               // newprnheader.DocumentNo = docnum;
                newprnheader.DocumentDate = DateTime.Now;
                newprnheader.GRNDate = DateTime.Now;
                newprnheader.OtherDocNo = "0";
                newprnheader.DocumentNo = _bllcommon.GetDocumentNo("PurchaseReturnNote",
                                                                  Convert.ToInt32(Session["loggeduserlocId"]),
                                                                  "1",
                                                                  docid,
                                                                  true, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));

                ViewBag.PurchaseHeaderId = prnheader.PurchaseHeaderId;


                return View("~/Views/Transactions/PurchaseReturnNote/CreatePRN.cshtml", newprnheader);
               

            }
            else
            {
                ViewBag.EventId = prnheader.EventId;
                ViewBag.CurrencyId = prnheader.CurrencyID;
                ViewBag.supplierId = prnheader.SupplierID;
                ViewBag.locationId = prnheader.LocationId;
                ViewBag.PaymentTermId = prnheader.PaymentTermID;
                ViewBag.paymentMethodsId = prnheader.PaymentMethodId;
                ViewBag.GRNId = prnheader.GRNId;
                prnheader.Company = _bllcompany.GetCompanyById(prnheader.CompanyID).CompanyName;
                prnheader.Location = _blllocation.GetLocationById(prnheader.GRNLocationId).LocationName;
                prnheader.GRNDetail.ToList().ForEach(d =>
                {
                    var prd = _bllproduct.GetProductById(d.ProductID);
                    d.ProductName = prd.ProductName;
                    d.ProductCode = prd.ProductCode;
                });

                @ViewBag.Message = "2";
                return View("~/Views/Transactions/PurchaseReturnNote/CreatePRN.cshtml", prnheader);
            }


        }


      //  [Authorize(Roles = "PRNEdit")]
        [HttpPost]
        public ActionResult TempPRN(PurchaseHeader prndet)
        {

            // check permissions
            if (!_appmanager.SetPermissions(6, Session["loggeduserempcode"].ToString(), "PRNEdit"))
            {
                @ViewBag.Permissions = "No user permissions Edit PRNs";
                return View("~/Views/Account/AccessDenied.cshtml");
            }
            //end check permissions

            return View("~/Views/Transactions/PurchaseReturnNote/EditPRN.cshtml");
        }

        [HttpGet]
        public JsonResult GetActivePaytmentMethods()
        {
            //PurchaseReturnService reporsitory = new PurchaseReturnService();
            var paymentMethods = _bllpurchasereturn.GetActivePaymentMethods(Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            return Json(JsonConvert.SerializeObject(paymentMethods, Formatting.None, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetActivePRNs()
        {
            //PurchaseReturnService reporsitory = new PurchaseReturnService();
            var grns = _bllpurchasereturn.GetAllActivePRNs(Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            return Json(JsonConvert.SerializeObject(grns, Formatting.None, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetDataTest(long id)
        {


            //List<PurchaseDetail> grn = _bllgrn.GetGRNDet(id);

            //foreach (var item in grn)
            //{
            //    item.ProductCode = _bllproduct.GetProductById(item.ProductID).ProductCode;
            //    item.ProductName = _bllproduct.GetProductById(item.ProductID).ProductName;

            //    if (item.DiscountType == null || item.DiscountType == "")
            //    {
            //        item.DiscountType = "n/a";

            //    }
            //    else
            //    {
            //        if (item.DiscountType == "Prc") { item.Discount = (item.CostPrice * item.Discount / 100) * item.GRNQuantity; }
            //        if (item.DiscountType == "Amt") { item.Discount = item.DiscountAmount * item.GRNQuantity; }
            //    }

            //    var header = _bllgrn.GetGRNById(item.PurchaseHeaderID);
            //    item.CurrancyId = header.CurrencyID;
            //    item.CurrancyRate = header.CurrencyRate;
            //    item.PaymentMethodId = header.PaymentMethodId;
            //    item.PaymentTermId = header.PaymentTermID;
            //    //   r.OrderQty = r.GRNQuantity - r.OrderQty;
            //    item.GRNLocationId = Convert.ToInt32(header.GRNLocationId);
            //    item.OrderQty = (item.GRNQuantity - item.PRNQuantity - item.TOGQty);
            //    item.CurrnetStock = _bllproductstock.GetProductStockMasterByProductIdLocid(item.ProductID, item.GRNLocationId).Stock;
            //    item.DiscountAmount = header.DiscountAmount;
            //    item.HeaderTaxTotal = header.TotalTax;
            //}

            var grn = _bllgrn.GetGRNDetById(id).Where(r => r.GRNQuantity >= r.PRNQuantity && r.GRNQuantity >= r.TOGQty);
            grn.ToList().ForEach
           (
               r =>
               {
                   r.ProductCode = _bllproduct.GetProductById(r.ProductID).ProductCode;
                   r.ProductName = _bllproduct.GetProductById(r.ProductID).ProductName;
                   if (r.DiscountType == null || r.DiscountType == "")
                   {
                       r.DiscountType = "n/a";

                   }
                   else
                   {
                       if (r.DiscountType == "Prc") { r.Discount = (r.CostPrice * r.Discount / 100) * r.GRNQuantity; }
                       if (r.DiscountType == "Amt") { r.Discount = r.DiscountAmount * r.GRNQuantity; }
                   }
                   var header = _bllgrn.GetGRNById(r.PurchaseHeaderID);
                   r.CurrancyId = header.CurrencyID;
                   r.CurrancyRate = header.CurrencyRate;
                   r.PaymentMethodId = header.PaymentMethodId;
                   r.PaymentTermId = header.PaymentTermID;
                   //   r.OrderQty = r.GRNQuantity - r.OrderQty;
                   r.GRNLocationId = Convert.ToInt32(header.GRNLocationId);
                   r.OrderQty = (r.GRNQuantity - r.PRNQuantity - r.TOGQty);
                   r.CurrnetStock = _bllproductstock.GetProductStockMasterByProductIdLocid(r.ProductID, r.GRNLocationId).Stock;
                   r.DiscountAmount = header.DiscountAmount;
                   r.HeaderTaxTotal = header.TotalTax;

               }


           );

            return new JsonResult { Data = grn, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpGet]
        public JsonResult GetActivePaymentMethods()
        {
            //PurchaseReturnService reporsitory = new PurchaseReturnService();
            var pm = _bllpurchasereturn.GetPaymentMethods(Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            return Json(JsonConvert.SerializeObject(pm, Formatting.None, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetActivePaymentTerms()
        {
            //PurchaseReturnService reporsitory = new PurchaseReturnService();
            var pt = _bllpurchasereturn.GetPaymentterms(Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            return Json(JsonConvert.SerializeObject(pt, Formatting.None, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        //public ActionResult RPTPOSummary()
        //{
        //    return View("~/Views/Reports/Stock/RPTPOSummary.cshtml");
        //}

        //[HttpPost]
        [Authorize(Roles = "Reports")]
        public ActionResult RPTPRNSummary(PRNSummaryViewModel vvm)
        {
            if (_bllconfiguration.GetConfiguration("UReports", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn)
            {

                if (!_appmanager.CheckReportPermissions(999, Session["loggeduserempcode"].ToString(), "RPTPRNSummary"))
                {
                    @ViewBag.Permissions = "No user permissions to View PRN Summary";
                    return View("~/Views/Account/AccessDenied.cshtml");
                }

            }

            var prn = _bllpurchasereturn.GetPRNSummaryReport(vvm.LocationId, vvm.DocumentId,vvm.DateFrom,vvm.DateTo).Where(p=>p.DocumentID==6);
            List<PRNSummaryViewModel> prnsummarymodel = new List<PRNSummaryViewModel>();
            foreach (var s in prn)
            {
                PRNSummaryViewModel v = new PRNSummaryViewModel();
                v.PurchaseHeaderId = s.PurchaseHeaderId;
                v.DocumentNo = s.DocumentNo;
                v.Location = _blllocation.GetLocationById(s.GRNLocationId).LocationName;
                v.DocumentDate = s.GRNDate;
                v.NetAmount = s.NetAmount;
                v.GrossAmount = s.GrossAmount;
                var docstatus = _blldocstatus.GetDocStatusById(s.DocumentStatus);
                if (docstatus != null)
                {
                    v.Status = docstatus.Description;
                }
                prnsummarymodel.Add(v);
            }

            return View("~/Views/Reports/PurchaseReturnNote/RPTPRNSummary.cshtml", prnsummarymodel);
        }

        [HttpGet]
        [Authorize(Roles = "Reports")]
        public ActionResult GetPRNDetailReport()
        {
            if (_bllconfiguration.GetConfiguration("UReports", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn)
            {

                if (!_appmanager.CheckReportPermissions(999, Session["loggeduserempcode"].ToString(), "RPTPRNDetail"))
                {
                    @ViewBag.Permissions = "No user permissions to View PRN Detail";
                    return View("~/Views/Account/AccessDenied.cshtml");
                }

            }

            return View("~/Views/Reports/PurchaseReturnNote/RPTPRNDetails.cshtml");
        }

        [HttpGet]
        public JsonResult GetDocNoByLocId(long locid,DateTime from,DateTime to)
        {
            var types = _bllpurchasereturn.GetDocNoByLocId(locid, Convert.ToInt32(Session["loggedusercompanyId"].ToString())).Where(
                p=>p.GRNDate.Date>=from.Date && p.GRNDate.Date<=to.Date && p.DocumentID==6
                ).Select(p => new { p.PurchaseHeaderId, p.DocumentNo });
            return Json(JsonConvert.SerializeObject(types, Formatting.None, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Authorize(Roles = "Reports")]
        public ActionResult GetPRNDetailReport(PRNDetailViewModel vm)
        {
            if (_bllconfiguration.GetConfiguration("UReports", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn)
            {

                if (!_appmanager.CheckReportPermissions(999, Session["loggeduserempcode"].ToString(), "RPTPRNDetail"))
                {
                    @ViewBag.Permissions = "No user permissions to View PRN Detail";
                    return View("~/Views/Account/AccessDenied.cshtml");
                }

            }

            var reportdata = _bllpurchasereturn.GetPRNDetailReport(vm.LocationId, vm.DocumentId,
                                                                    vm.DateFrom, vm.DateTo);
            return View("~/Views/Reports/PurchaseReturnNote/RPTPRNDetails.cshtml", reportdata);
        }

        [HttpGet]
        public JsonResult CheckForManualPRN()
        {
            var res = _bllusermanster.GetUserFunctionsByEmpCode(Convert.ToString(Session["loggeduserempcode"])
                                                               
                                                                ).Where(p=>p.FunctionName == "ManualPRN");
            bool manualprn = false;
            int count = res.Count();
            if (res.Count()!=0)
            {
                manualprn = true;
            }
            return new JsonResult { Data = manualprn, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }



    }
}