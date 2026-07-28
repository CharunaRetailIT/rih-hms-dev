using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using RIT.HMS.BLL.Common;
using RIT.HMS.BLL.TransactionData;
using RIT.HMS.BLL.MasterData;
using RIT.HMS.Domain.ViewModels;
using RIT.HMS.Domain.ViewModels.Reports;
using RIT.HMS.Domain.Transactions;
using RIT.HMS.BLL.Configurations;

namespace HospitalityManagement.Controllers.Transactions
{
    [SessionTimeout]
    public class TOGController : Controller
    {
       
        BLL_Common _bllcommon;
        private BLL_TransferNote _blltransferNote;
        private BLL_Product _bllproduct;
        private readonly BLL_Location _blllocation;
        private readonly BLL_Company _bllcompany;
        private readonly BLL_GRN _bllgrn;
        private readonly BLL_MonthEnd _bllmonthend;
        private readonly BLL_Configuration _bllconfiguration;
        private readonly BLL_DocStatus _blldocstatus;
        private readonly BLL_RequestNote _bllrequest;
        private readonly BLL_ProductStock _bllproductstock;
        private readonly BLL_PurchaseOrder _bllpurchaseorder;
        private AppManager _appmanager;

        public TOGController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _bllcommon = new BLL_Common(cn);
            _blltransferNote = new BLL_TransferNote(cn);
            _bllproduct = new BLL_Product(cn);
            _blllocation = new BLL_Location(cn);
            _bllcompany = new BLL_Company(cn);
            _bllgrn = new BLL_GRN(cn);
            _bllmonthend = new BLL_MonthEnd(cn);
            _bllconfiguration = new BLL_Configuration(cn);
             _blldocstatus = new BLL_DocStatus(cn);
            _bllrequest = new BLL_RequestNote(cn);
            _bllproductstock = new BLL_ProductStock(cn);
            _bllpurchaseorder = new BLL_PurchaseOrder(cn);
            _appmanager = new AppManager(cn);

    }

        [HttpGet]
        public ActionResult CreateTOG()
        {

          
            //end check permissions
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var deptdata = _bllproduct.GetActiveProducts(companyid);
           // ViewBag.productdata = deptdata;
            Session["APPTOG"] = _bllconfiguration.GetConfiguration("APPTOG", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;

            Session["DisableTOGReject"] = _bllconfiguration.GetConfiguration("DisableTOGReject", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;

            Session["RequestNoteBasedTOG"] = _bllconfiguration.GetConfiguration("RequestNoteBasedTOG", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;

            Session["TOGGridAdditionalColumn"] = _bllconfiguration.GetConfiguration("TOGGridAdditionalColumn", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;

            Session["IsEventMandatory"] = _bllconfiguration.GetConfiguration("IsEventMandatory", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;

            Session["DisablePriceRequestNote"] = _bllconfiguration.GetConfiguration("DisablePriceRequestNote", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;


            var tnheader = new TransferNoteHeader();

            int docid = _bllcommon.GetDcumentId("TOG", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            var docnum = _bllcommon.GetDocumentTemporyNo("TOG", Convert.ToInt32(Session["loggeduserlocId"]), "1", docid, true, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            tnheader.DocumentNo = docnum;
            tnheader.TOGDate = DateTime.Now;
            @ViewBag.locationId = Convert.ToInt32(Session["loggeduserlocId"]);


            List<SelectListItem> FinishGoods = new List<SelectListItem>();
            SelectListItem defaultfinishgood = new SelectListItem();
            defaultfinishgood.Text = "-- Select an Item --";
            defaultfinishgood.Value = "0";
            FinishGoods.Add(defaultfinishgood);        
            foreach (var prd in deptdata)
            {
                SelectListItem dbprd = new SelectListItem();
                dbprd.Text = (prd.ProductCode + "-" + prd.ProductName);
                dbprd.Value = prd.ProductId.ToString();
                FinishGoods.Add(dbprd);
            }           
            Session["AllItemsToTOG"] = FinishGoods;


            return View("~/Views/Transactions/TOG/CreateTOG.cshtml", tnheader);
        }

       // [Authorize(Roles = "TogView")]
        [HttpGet]
        public ActionResult ViewAllTOG()
        {

            ViewBag.datefrom = DateTime.Now;
            ViewBag.dateTo = DateTime.Now;

            // check permissions
            if (!_appmanager.SetPermissions(5, Session["loggeduserempcode"].ToString(), "TOGView"))
            {
                @ViewBag.Permissions = "No user permissions to View TOG s";
                return View("~/Views/Account/AccessDenied.cshtml");
            }
            //end check permissions

            // var togs = _blltransferNote.GetAllTOGs(Convert.ToInt32(Session["loggedusercompanyId"].ToString())).Where(d=>d.DocumentStatus!=3);
            long locationID= Convert.ToInt32(Session["loggeduserlocId"]);
            bool IsHeadOffice = Convert.ToBoolean(Session["IsHeadOffice"]);


            // var togs = _blltransferNote.GetAllTOGs(Convert.ToInt32(Session["loggedusercompanyId"].ToString()), locationID, IsHeadOffice);
            var togs = _blltransferNote.GetAllTOGsCurrentDate(Convert.ToInt32(Session["loggedusercompanyId"].ToString()), locationID, IsHeadOffice);

                      


            togs.ToList().ForEach(g =>
            {

                g.FromLocationName = _blllocation.GetLocationById(g.LocationId).LocationName;
                g.ToLocationName = _blllocation.GetLocationById(g.ToLocationId).LocationName;
                if (g.DocumentStatus == 0)
                {
                    g.DocState = "N/A";
                }
                else
                {
                    g.DocState = _blldocstatus.GetDocStatusById(g.DocumentStatus).Description;
                }
                g.TOGDate = g.TOGDate.Date.Add(g.ModifiedDate.TimeOfDay);
            });
            //var sortedList = togs.OrderByDescending(ord => DateTime.Parse(ord.TOGDate.ToShortDateString())).ToList();
            var sortedList = togs.OrderByDescending(ord => ord.TOGDate).ToList();
            return View("~/Views/Transactions/TOG/ViewAllTOGs.cshtml", sortedList);


            //return View("~/Views/Transactions/TOG/ViewAllTOGs.cshtml", togs);
        }

       // [Authorize(Roles = "POView")]
        [HttpGet]
        public ActionResult FilterTog(int statusid, DateTime datefrom, DateTime dateto)
        {
            ViewBag.datefrom = datefrom;
            ViewBag.dateTo = dateto;

            // check permissions
            if (!_appmanager.SetPermissions(5, Session["loggeduserempcode"].ToString(), "TOGView"))
            {
                @ViewBag.Permissions = "No user permissions to View TOG s";
                return View("~/Views/Account/AccessDenied.cshtml");
            }
            //end check permissions

            DateTime FromDate = datefrom.Date;
            DateTime ToDate = dateto.Date;


            long locationID = Convert.ToInt32(Session["loggeduserlocId"]);
            bool IsHeadOffice = Convert.ToBoolean(Session["IsHeadOffice"]);




            var togs = _blltransferNote.GetAllTOGswithDateRange(Convert.ToInt32(Session["loggedusercompanyId"].ToString()), locationID,IsHeadOffice, FromDate, ToDate).ToList().Where(p=>
                (statusid == 0 || p.DocumentStatus == statusid) &&
                p.TOGDate >= FromDate.Date && p.TOGDate <= ToDate.Date.AddDays(1).AddTicks(-1)
                );
                togs.ToList().ForEach(g =>
                {
                    g.FromLocationName = _blllocation.GetLocationById(g.LocationId).LocationName;
                    g.ToLocationName = _blllocation.GetLocationById(g.ToLocationId).LocationName;
                    if (g.DocumentStatus == 0)
                    {
                        g.DocState = "N/A";
                    }
                    else
                    {
                        g.DocState = _blldocstatus.GetDocStatusById(g.DocumentStatus).Description;
                    }
                    g.TOGDate = g.TOGDate.Date.Add(g.ModifiedDate.TimeOfDay);
                });

                @ViewBag.datefrom = datefrom;
                @ViewBag.dateTo = dateto;
                @ViewBag.DocStatusId = statusid;
            //var sortedList = togs.OrderByDescending(ord => DateTime.Parse(ord.TOGDate.ToShortDateString())).ToList();
            var sortedList = togs.OrderByDescending(ord => ord.TOGDate).ToList();
            return View("~/Views/Transactions/TOG/ViewAllTOGs.cshtml", sortedList);
        }

     //   [Authorize(Roles = "TogEdit")]
        [HttpGet]
        public ActionResult Edit(long id)
        {

            // check permissions
            //if (!_appmanager.SetPermissions(5, Session["loggeduserempcode"].ToString(), "TogEdit"))
            //{
            //    @ViewBag.Permissions = "No user permissions to Edit TOG s";
            //    return View("~/Views/Account/AccessDenied.cshtml");
            //}
            //end check permissions

            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var deptdata = _bllproduct.GetActiveProducts(companyid);
            ViewBag.productdata = deptdata;
            Session["APPTOG"] = _bllconfiguration.GetConfiguration("APPTOG", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;

            Session["DisableTOGReject"] = _bllconfiguration.GetConfiguration("DisableTOGReject", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;

            Session["RequestNoteBasedTOG"] = _bllconfiguration.GetConfiguration("RequestNoteBasedTOG", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;

            Session["TOGGridAdditionalColumn"] = _bllconfiguration.GetConfiguration("TOGGridAdditionalColumn", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;

            Session["IsEventMandatory"] = _bllconfiguration.GetConfiguration("IsEventMandatory", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            Session["DisablePriceRequestNote"] = _bllconfiguration.GetConfiguration("DisablePriceRequestNote", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;

            var tog = _blltransferNote.GetTOGById(id);
            tog.TOGDetail = _blltransferNote.GetTOGDetById(id).ToList();
            foreach(TransferNoteDetail TD in tog.TOGDetail)
            {
                TD.CostValue = TD.CostPrice * TD.OrderQty;
                TD.SIH = _bllproductstock.GetProductStockMasterByProductIdLocid(TD.ProductId, Convert.ToInt32(tog.LocationId)).Stock;

            }


            int sequence = 0;
            tog.TOGDetail.ForEach
            (
                r =>
                {
                    sequence++;
                    r.sequence = sequence;
                    r.ProductName = _bllproduct.GetProductById(r.ProductId).ProductName;
                    r.UOM = r.OrderQty + _bllproduct.GetUOMById(_bllproduct.GetProductById(r.ProductId).PurchasingUnit);
                    //tog.TotSellingPrice += r.SellingPrice;
                    //tog.TotCostPrice += r.CostPrice;
                    //tog.TotDiscounts = r.DiscountAmount;
                }


            );
            tog.DocState = _blldocstatus.GetDocStatusById(tog.DocumentStatus).Description;
            ViewBag.Auto = tog.TOGType;
            ViewBag.Manual = tog.TOGType;
            ViewBag.FromLocationId = tog.FromLocationId;
            ViewBag.ToLocationId = tog.ToLocationId;
            ViewBag.TotQty = tog.TotQty;
            ViewBag.TOGType = tog.TOGType;
            ViewBag.DocumentNo = tog.DocumentNo;
            ViewBag.RequestNoteId = tog.ReferenceDocumentId;
            ViewBag.EventId = tog.EventId;
            if (tog.TOGType == "GRNBased")
            {
                if (tog.ReferenceDocumentId != 0)
                {
                    ViewBag.GRNNumber = _bllgrn.GetGRNById(tog.ReferenceDocumentId).DocumentNo;
                    ViewBag.GRNId = tog.ReferenceDocumentId;
                }

                if(tog.GRNNo != null )
                {
                    ViewBag.GRNNumber = tog.GRNNo;

                }



            }

            var fromlocproducts =  _bllpurchaseorder.GetLocationProductNames(Convert.ToInt32(Session["loggeduserlocId"]),
                                                                        companyid);
            List<SelectListItem> FinishGoods = new List<SelectListItem>();
            SelectListItem defaultfinishgood = new SelectListItem();
            defaultfinishgood.Text = "-- Select an Item --";
            defaultfinishgood.Value = "0";
            FinishGoods.Add(defaultfinishgood);
            foreach (var prd in fromlocproducts)
            {
                SelectListItem dbprd = new SelectListItem();
                dbprd.Text = (prd.ProductCode + "-" + prd.ProductName);
                dbprd.Value = prd.ProductId.ToString();
                FinishGoods.Add(dbprd);
            }
            Session["AllItemsToTOG"] = FinishGoods;

            // ViewBag.GRNId=tog
            return View("~/Views/Transactions/TOG/EditTOG.cshtml", tog);
        }


     //   [Authorize(Roles = "TogEdit")]
        [HttpPost]
        public ActionResult EditTOG(TransferNoteHeader togheader)
        {

            // check permissions
            if (!_appmanager.SetPermissions(5, Session["loggeduserempcode"].ToString(), "TOGEdit"))
            {
                @ViewBag.Permissions = "No user permissions to Edit TOG s";
                return View("~/Views/Account/AccessDenied.cshtml");
            }
            //end check permissions

            if (togheader.TOGDetail.Count == 0)
            {
                int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
                var deptdata = _bllproduct.GetActiveProducts(companyid);
                ViewBag.FromLocationId = togheader.FromLocationId;
                ViewBag.ToLocationId = togheader.ToLocationId;
                ViewBag.productdata = deptdata;
                ViewBag.EventId = togheader.EventId;
                @ViewBag.Message = "5";
                return View("~/Views/Transactions/TOG/EditTOG.cshtml", togheader);
            }
            var dbtog = _blltransferNote.GetTOGById(togheader.TransferNoteHeaderId);
           // dbtog.TOGDetail = _blltransferNote.GetTOGDetById(dbtog.TransferNoteHeaderId).ToList();
             dbtog.TOGDetail = togheader.TOGDetail;


            dbtog.ModifiedDate = DateTime.Now;
            dbtog.ModifiedUser = Session["loggeduser"].ToString();
            dbtog.DataTransfer = 0;


            int docid = _bllcommon.GetDcumentId("TOG", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            var docnum = _bllcommon.GetDocumentNo("TOG",
                                                    Convert.ToInt32(Session["loggeduserlocId"]),
                                                    Convert.ToString(togheader.FromLocationId),
                                                     docid,
                                                     false, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));

            dbtog.DocumentId = docid;
            dbtog.DocumentNo = docnum;



            if (Convert.ToBoolean(Session["APPTOG"]) == true) // Approval config ON
            {
                // Pending Approvals
                dbtog.DocumentStatus = 2;             
                dbtog.IsTempTOG = false;
            }
            else
            {
                // Aproved
                dbtog.DocumentStatus = 3;               
                dbtog.IsTempTOG = false;
            }

            dbtog.ReferenceDocumentId = togheader.ReferenceDocumentId;
            dbtog.ReferenceNo = togheader.ReferenceNo;
            dbtog.FromLocationId = togheader.FromLocationId;
            dbtog.ToLocationId = togheader.ToLocationId;
            dbtog.GrossAmount = togheader.GrossAmount;
            dbtog.Remark = togheader.Remark;
            dbtog.NetAmount = togheader.NetAmount;
            dbtog.IsDelete = togheader.IsDelete;
            dbtog.TotSellingPrice = togheader.TotSellingPrice;
            dbtog.TotCostPrice = togheader.TotCostPrice;
            dbtog.TOGType = togheader.TOGType;
            dbtog.TotQty = togheader.TotQty;
            dbtog.TOGDate = togheader.TOGDate;
            dbtog.EventId = togheader.EventId;

            PurchaseHeader PH = new PurchaseHeader();
            if (togheader.GRNNo != null)
            {
                PH = _bllgrn.GetGRNById(Convert.ToInt64(togheader.GRNNo));
                if (PH != null && PH.DocumentNo != null)
                    togheader.GRNNo = PH.DocumentNo;
            }


            bool res = _blltransferNote.UpdateTOG(dbtog,PH.POID);
            @ViewBag.TOGCode = dbtog.DocumentNo;
            if (res == true)
            {
                @ViewBag.Message = "1";
                TransferNoteHeader th = new TransferNoteHeader();
                th.TOGDate = DateTime.Now;
                ViewBag.TransferNoteHeaderId = togheader.TransferNoteHeaderId;
                return View("~/Views/Transactions/TOG/EditTOG.cshtml", th);
            }
            else
            {
                @ViewBag.Message = "3";
                return View("~/Views/Transactions/TOG/EditTOG.cshtml", togheader);
            }



        }

      //  [Authorize(Roles = "TogEdit")]
        [HttpPost]
        public ActionResult TempEdit(TransferNoteHeader togheader)
        {

            // check permissions
            if (!_appmanager.SetPermissions(5, Session["loggeduserempcode"].ToString(), "TOGEdit"))
            {
                @ViewBag.Permissions = "No user permissions to Edit TOG s";
                return View("~/Views/Account/AccessDenied.cshtml");
            }
            //end check permissions

            if (togheader.TOGDetail.Count() == 0)
            {
                int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
                var deptdata = _bllproduct.GetActiveProducts(companyid);
                ViewBag.locationId = togheader.FromLocationId;
                ViewBag.ToLocationId = togheader.ToLocationId;
                ViewBag.productdata = deptdata;
                ViewBag.EventId = togheader.EventId;
                @ViewBag.Message = "5";
                return View("~/Views/Transactions/TOG/EditTOG.cshtml", togheader);
            }
            var dbtog = _blltransferNote.GetTOGById(togheader.TransferNoteHeaderId);
            //  dbtog.TOGDetail = transferNoteService.GetTOGDetById(dbtog.TransferNoteHeaderId).ToList();
            dbtog.TOGDetail = togheader.TOGDetail;


            dbtog.ModifiedDate = DateTime.Now;
            dbtog.ModifiedUser = Session["loggeduser"].ToString();
            dbtog.DataTransfer = 0;


            //  dbtog = togheader;

            //  var docnum = commonService.GetDocumentNo("TOG", 1, "1", 2, false);
            //int docid = commonService.GetDcumentId("TOG");
            //var docnum = commonService.GetDocumentNo("TOG",
            //                                        Convert.ToInt32(Session["loggeduserlocId"]),
            //                                        "1",
            //                                         docid,
            //                                         false);

            //  dbtog.DocumentId = docid;
            dbtog.DocumentNo = togheader.DocumentNo;
            //  dbtog.IsTempTOG = false;

            dbtog.ReferenceDocumentId = togheader.ReferenceDocumentId;
            dbtog.ReferenceNo = togheader.ReferenceNo;
            dbtog.FromLocationId = togheader.FromLocationId;
            dbtog.ToLocationId = togheader.ToLocationId;
            dbtog.GrossAmount = togheader.GrossAmount;
            dbtog.Remark = togheader.Remark;
            dbtog.NetAmount = togheader.NetAmount;
            dbtog.IsDelete = togheader.IsDelete;
            dbtog.TotSellingPrice = togheader.TotSellingPrice;
            dbtog.TotCostPrice = togheader.TotCostPrice;
            dbtog.TOGType = togheader.TOGType;
            dbtog.TotQty = togheader.TotQty;
            dbtog.TOGDate = togheader.TOGDate;
            dbtog.EventId = togheader.EventId;
            dbtog.DocumentStatus = 1;
            dbtog.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());

            PurchaseHeader PH = new PurchaseHeader();
            if (togheader.GRNNo != null)
            {
                PH = _bllgrn.GetGRNById(Convert.ToInt64(togheader.GRNNo));
                if (PH != null && PH.DocumentNo != null)
                    togheader.GRNNo = PH.DocumentNo;
            }
            bool res = _blltransferNote.UpdateTOG(dbtog,PH.POID);
            @ViewBag.TOGCode = dbtog.DocumentNo;
            if (res == true)
            {
                @ViewBag.Message = "4";
                TransferNoteHeader th = new TransferNoteHeader();
                th.TOGDate = DateTime.Now;
                ViewBag.TransferNoteHeaderId = togheader.TransferNoteHeaderId;
                return View("~/Views/Transactions/TOG/EditTOG.cshtml", th);
            }
            else
            {
                @ViewBag.Message = "3";

                return View("~/Views/Transactions/TOG/EditTOG.cshtml", togheader);
            }



        }

        // [Authorize(Roles = "TogCreatee")]
      //  [Authorize(Roles = "TogEdit")]
        [HttpPost]
        public ActionResult CreateTOG(TransferNoteHeader togheader)
        {
            // check permissions
            if (!_appmanager.SetPermissions(5, Session["loggeduserempcode"].ToString(), "TOGCreatee"))
            {
                @ViewBag.Permissions = "No user permissions to Create TOG s";
                return View("~/Views/Account/AccessDenied.cshtml");
            }

            if(togheader.GrossAmount==0 && togheader.NetAmount ==0 )
            {
                @ViewBag.Message = "9";
                return View("~/Views/Transactions/TOG/CreateTOG.cshtml", togheader);

            }

            

            if (togheader.GRNNo != null && togheader.GRNNo != "0")
                TempData["GRNNO_New"] = togheader.GRNNo;
            else if(TempData["GRNNO_New"] != null)
                togheader.GRNNo = TempData["GRNNO_New"].ToString();

            //if (togheader.GRNId != 0)
            //    ViewBag.GRNID_New = togheader.GRNId;
            //else
            //    togheader.GRNId = ViewBag.GRNID_New;

            //end check permissions

            //kapila 28/5/2020
            //if (!ModelState.IsValid)
            //{
            //    var errors = ModelState.Values.SelectMany(v => v.Errors);
            //    ModelState.Clear();
            //    var deptdata = _bllproduct.GetActiveProducts();
            //    ViewBag.locationId = togheader.FromLocationId;
            //    ViewBag.ToLocationId = togheader.ToLocationId;
            //    ViewBag.productdata = deptdata;
            //    ViewBag.EventId = togheader.EventId;
            //    return View("~/Views/Transactions/TOG/CreateTOG.cshtml", togheader);
            //}
            // check Month End Stataus
            // int year = poheader.PODate.Year;
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var deptdata = _bllproduct.GetActiveProducts(companyid);         
            ViewBag.productdata = deptdata;

            PurchaseHeader PH = new PurchaseHeader();
            if (togheader.GRNNo != null)
            {
                PH = _bllgrn.GetGRNById(Convert.ToInt64(togheader.GRNNo));
                if( PH != null &&    PH.DocumentNo!=null)
                togheader.GRNNo = PH.DocumentNo;
            }

            foreach(TransferNoteDetail  TD in togheader.TOGDetail)
            {
               TD.ProductName=   _bllproduct.GetProductById(TD.ProductId).ProductName;
                TD.ProductCode = _bllproduct.GetProductById(TD.ProductId).ProductCode;
               // TD.TOGQty = TD.OrderQty;
            }

            bool EnableMonthEndProcess = _bllconfiguration.GetConfiguration("EnableMonthEndProcess", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;

            if (EnableMonthEndProcess)
            {

                int monthendstatus = _bllmonthend.CheckMonthEnd(Convert.ToInt32(togheader.FromLocationId), togheader.TOGDate.Year, togheader.TOGDate.Month);
                if (monthendstatus != 1)
                {
                    ModelState.Clear();
                    // var deptdata = _bllproduct.GetActiveProducts();
                    ViewBag.locationId = togheader.FromLocationId;
                    ViewBag.ToLocationId = togheader.ToLocationId;
                    // ViewBag.productdata = deptdata;
                    ViewBag.EventId = togheader.EventId;
                    @ViewBag.Message = "6";
                    return View("~/Views/Transactions/TOG/CreateTOG.cshtml", togheader);

                }

                //kapila 27/5/2020
                monthendstatus = _bllmonthend.CheckMonthEnd(Convert.ToInt32(togheader.ToLocationId), togheader.TOGDate.Year, togheader.TOGDate.Month);
                if (monthendstatus != 2)
                {
                    ModelState.Clear();
                    // var deptdata = _bllproduct.GetActiveProducts();
                    ViewBag.locationId = togheader.FromLocationId;
                    ViewBag.ToLocationId = togheader.ToLocationId;
                    // ViewBag.productdata = deptdata;
                    ViewBag.EventId = togheader.EventId;
                    @ViewBag.Message = "7";
                    return View("~/Views/Transactions/TOG/CreateTOG.cshtml", togheader);

                }
                // End check Month End Stataus
            }


            if (togheader.FromLocationId == 0)
            {
               // var deptdata = _bllproduct.GetActiveProducts();
                ViewBag.locationId = togheader.FromLocationId;
                ViewBag.ToLocationId = togheader.ToLocationId;
               // ViewBag.productdata = deptdata;
                ViewBag.EventId = togheader.EventId;

                @ViewBag.Message = "5";
                ModelState.AddModelError("FromLocationId", "Please select From Location !");
                return View("~/Views/Transactions/TOG/CreateTOG.cshtml", togheader);
            }
            if (togheader.ToLocationId == 0)
            {
              //  var deptdata = _bllproduct.GetActiveProducts();
                ViewBag.locationId = togheader.FromLocationId;
                ViewBag.ToLocationId = togheader.ToLocationId;
              //  ViewBag.productdata = deptdata;
                ViewBag.EventId = togheader.EventId;
                @ViewBag.Message = "5";
                ModelState.AddModelError("ToLocationId", "Please select To Location !");
                return View("~/Views/Transactions/TOG/CreateTOG.cshtml", togheader);
            }
            if (togheader.TOGDetail.Count == 0)
            {
              //  var deptdata = _bllproduct.GetActiveProducts();
                ViewBag.locationId = togheader.FromLocationId;
                ViewBag.ToLocationId = togheader.ToLocationId;
               // ViewBag.productdata = deptdata;
                ViewBag.EventId = togheader.EventId;
                @ViewBag.Message = "5";
                return View("~/Views/Transactions/TOG/CreateTOG.cshtml", togheader);
            }

            bool IsEventMandatory = _bllconfiguration.GetConfiguration("IsEventMandatory", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;


            if (togheader.EventId == 0 && IsEventMandatory)
            {
                //  var deptdata = _bllproduct.GetActiveProducts();
                ViewBag.locationId = togheader.FromLocationId;
                ViewBag.ToLocationId = togheader.ToLocationId;
                // ViewBag.productdata = deptdata;
                ViewBag.EventId = togheader.EventId;
                @ViewBag.Message = "8";
                return View("~/Views/Transactions/TOG/CreateTOG.cshtml", togheader);
            }


            togheader.CreatedUser = Session["loggeduser"].ToString();
            togheader.CreatedDate = DateTime.Now;
            togheader.ModifiedDate = DateTime.Now;
            togheader.ModifiedUser = Session["loggeduser"].ToString();
            togheader.DataTransfer = 0;
            togheader.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
            togheader.GroupOfCompanyID = 1;
            togheader.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());


            //   togheader.DocumentStatus = 1;

            if (Convert.ToBoolean(Session["APPTOG"]) == true)
            {
                // pending for approvals
                togheader.DocumentStatus = 2;
            }
            else
            {
                // Approved
                togheader.DocumentStatus = 3;
            }



            if (togheader.TOGType == "GRNBased")
            {
                togheader.ReferenceDocumentId = togheader.GRNId;
            }
            else if (togheader.TOGType == "RequestNoteBased")
            {
                togheader.ReferenceDocumentId = togheader.RequestNoteId;
                togheader.DocumentStatus = 3;
            }

            int docid = _bllcommon.GetDcumentId("TOG", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            var docnum = _bllcommon.GetDocumentNo("TOG",
                                                    Convert.ToInt32(Session["loggeduserlocId"]),
                                                    Convert.ToString(togheader.FromLocationId),
                                                     docid,
                                                     false, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));

            togheader.DocumentId = docid;
            togheader.DocumentNo = docnum;
            togheader.DocumentDate = DateTime.Now;
            togheader.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            long GRNHeadID = 0;
            PurchaseHeader P = _bllgrn.GetGRNByDocNo(togheader.GRNNo, companyid);
            if(P != null && P.PurchaseHeaderId != null)
             GRNHeadID = _bllgrn.GetGRNByDocNo(togheader.GRNNo, companyid).PurchaseHeaderId;  
            List<PurchaseDetail> Qtys = _bllgrn.GetGRNDet(GRNHeadID);
            togheader.GRNId = Convert.ToInt32( GRNHeadID);
            // Make sure both lists have the same count
            if (togheader.TOGDetail.Count != Qtys.Count)
            {
                // Handle the case where counts do not match
                // You may throw an exception, log a warning, or handle it according to your requirements
            }
            List<InvRequestNotePOTransaction> POTransaction = new List<InvRequestNotePOTransaction>();
           
            if ( PH != null )
            POTransaction = _bllgrn.GetRequestNoteDetailsforGRN(PH.POID, Convert.ToInt32(togheader.ToLocationId));

            if (POTransaction != null && POTransaction.Count > 0)
            {

            }
            else if(Qtys.Count == togheader.TOGDetail.Count)
            {

                for (int i = 0; i < togheader.TOGDetail.Count; i++)
                {
                    // Match each item from poheader.PODetail with corresponding item from Qtys
                    
                    togheader.TOGDetail[i].OrderQty = Qtys[i].OrderQty;
                }
            }


            bool res = _blltransferNote.SaveTransferNote(togheader);
            @ViewBag.TOGCode = togheader.DocumentNo;
            if (res == true)
            {
                TempData["GRNNO_New"] = null;
                @ViewBag.Message = "1";
                ViewBag.TransferNoteHeaderId = togheader.TransferNoteHeaderId;

                ModelState.Clear();
                TransferNoteHeader newtogheader = new TransferNoteHeader();
                int docid1 = _bllcommon.GetDcumentId("TOG", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                newtogheader.TOGDate = DateTime.Now;
                newtogheader.DocumentNo = _bllcommon.GetDocumentNo("TOG",
                                                   Convert.ToInt32(Session["loggeduserlocId"]),
                                                     "1",
                                                      docid1,
                                                     true, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                return View("~/Views/Transactions/TOG/CreateTOG.cshtml", newtogheader);
            }
            else
            {
                @ViewBag.Message = "3";
                return View("~/Views/Transactions/TOG/CreateTOG.cshtml", togheader);
            }
        }

        // [Authorize(Roles = "TOGCreatee")]
      //  [Authorize(Roles = "TogEdit")]
        [HttpPost]
        public ActionResult TempSaveTOG(TransferNoteHeader togheader)
        {

            // check permissions
                if (!_appmanager.SetPermissions(5, Session["loggeduserempcode"].ToString(), "TOGCreatee"))
                {
                    @ViewBag.Permissions = "No user permissions to Temp Save TOG s";
                    return View("~/Views/Account/AccessDenied.cshtml");
                }
            //end check permissions
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var deptdata = _bllproduct.GetActiveProducts(companyid);         
            ViewBag.productdata = deptdata;


            bool EnableMonthEndProcess = _bllconfiguration.GetConfiguration("EnableMonthEndProcess", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;

            if (EnableMonthEndProcess)
            {
                //kapila 27/5/2020
                int monthendstatus = _bllmonthend.CheckMonthEnd(Convert.ToInt32(togheader.FromLocationId),
                togheader.TOGDate.Year, togheader.TOGDate.Month);
                if (monthendstatus != 1)
                {
                    // var deptdata = _bllproduct.GetActiveProducts();
                    ViewBag.FromLocationId = togheader.FromLocationId;
                    ViewBag.ToLocationId = togheader.ToLocationId;
                    // ViewBag.productdata = deptdata;
                    ViewBag.EventId = togheader.EventId;
                    @ViewBag.Message = "6";
                    return View("~/Views/Transactions/TOG/EditTOG.cshtml", togheader);

                }
                //kapila 27/5/2020
                monthendstatus = _bllmonthend.CheckMonthEnd(Convert.ToInt32(togheader.ToLocationId), togheader.TOGDate.Year, togheader.TOGDate.Month);
                if (monthendstatus != 1)
                {
                    ModelState.Clear();
                    // var deptdata = _bllproduct.GetActiveProducts();
                    ViewBag.locationId = togheader.FromLocationId;
                    ViewBag.ToLocationId = togheader.ToLocationId;
                    //  ViewBag.productdata = deptdata;
                    ViewBag.EventId = togheader.EventId;
                    @ViewBag.Message = "7";
                    return View("~/Views/Transactions/TOG/CreateTOG.cshtml", togheader);

                }
            }

            if (togheader.TOGDetail.Count == 0)
            {
               // var deptdata = _bllproduct.GetActiveProducts();
                ViewBag.locationId = togheader.FromLocationId;
                ViewBag.ToLocationId = togheader.ToLocationId;
              //  ViewBag.productdata = deptdata;
                ViewBag.EventId = togheader.EventId;
                @ViewBag.Message = "5";
                return View("~/Views/Transactions/TOG/CreateTOG.cshtml", togheader);
            }


            togheader.CreatedUser = Session["loggeduser"].ToString();
            togheader.CreatedDate = DateTime.Now;
            togheader.ModifiedDate = DateTime.Now;
            togheader.ModifiedUser = "";
            togheader.DataTransfer = 0;
            togheader.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
            togheader.GroupOfCompanyID = 1;
            togheader.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());

            togheader.DocumentStatus = 1;
            togheader.IsTempTOG = true;

            

          //  int docid = _bllcommon.GetDcumentId("TOG");
          //  var docnum = _bllcommon.GetDocumentNo("TOG",
                                                 //   Convert.ToInt32(Session["loggeduserlocId"]),
                                                //    "1",
                                                //     docid,
                                                //     true);

           // togheader.DocumentId = docid;
          //  togheader.DocumentNo = docnum;

            togheader.DocumentDate = DateTime.Now;
            //  togheader.ReferenceDocumentId = togheader.GRNId;

            if (togheader.TOGType == "GRNBased")
            {
                togheader.ReferenceDocumentId = togheader.GRNId;
            }
            else if (togheader.TOGType == "RequestNoteBased")
            {
                togheader.ReferenceDocumentId = togheader.RequestNoteId;
              
            }


            bool res = _blltransferNote.SaveTransferNote(togheader);
            @ViewBag.TOGCode = togheader.DocumentNo;
            if (res == true)
            {
                @ViewBag.Message = "2";
                @ViewBag.TransferNoteHeaderId = togheader.TransferNoteHeaderId;
                ModelState.Clear();
                TransferNoteHeader newtogheader = new TransferNoteHeader();
                int docid = _bllcommon.GetDcumentId("TOG", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                newtogheader.TOGDate = DateTime.Now;
                newtogheader.DocumentNo= _bllcommon.GetDocumentNo("TOG",
                                                   Convert.ToInt32(Session["loggeduserlocId"]),
                                                     "1",
                                                      docid,
                                                     true, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                return View("~/Views/Transactions/TOG/CreateTOG.cshtml", newtogheader);
            }
            else
            {
                @ViewBag.Message = "3";
                return View("~/Views/Transactions/TOG/CreateTOG.cshtml", togheader);
            }

        }


        // [Authorize(Roles = "TOGApprove")]
       // [Authorize(Roles = "TogEdit")]
        [HttpPost]
        public ActionResult Approve(TransferNoteHeader togheader)
        {
            // check permissions
                if (!_appmanager.SetPermissions(5, Session["loggeduserempcode"].ToString(), "TOGApprove"))
                {
                    @ViewBag.Permissions = "No user permissions to Approve TOG s";
                    return View("~/Views/Account/AccessDenied.cshtml");
                }
            //end check permissions
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            //kapila 9/6/2020
            if (!string.IsNullOrEmpty(togheader.CancelReason) || !string.IsNullOrEmpty(togheader.RejectReason))
            {
                var deptdata = _bllproduct.GetActiveProducts(companyid);
                ViewBag.FromLocationId = togheader.FromLocationId;
                ViewBag.ToLocationId = togheader.ToLocationId;
                ViewBag.productdata = deptdata;
                ViewBag.EventId = togheader.EventId;
                @ViewBag.Message = "9";
                return View("~/Views/Transactions/TOG/EditTOG.cshtml", togheader);
            }
            if (togheader.TOGDetail.Count == 0)
            {
                var deptdata = _bllproduct.GetActiveProducts(companyid);
                ViewBag.FromLocationId = togheader.FromLocationId;
                ViewBag.ToLocationId = togheader.ToLocationId;
                ViewBag.productdata = deptdata;
                ViewBag.EventId = togheader.EventId;
                @ViewBag.Message = "5";
                return View("~/Views/Transactions/TOG/EditTOG.cshtml", togheader);
            }
            var dbtog = _blltransferNote.GetTOGById(togheader.TransferNoteHeaderId);
            // dbtog.TOGDetail = _blltransferNote.GetTOGDetById(dbtog.TransferNoteHeaderId).ToList();

            dbtog.TOGDetail = togheader.TOGDetail;

            dbtog.ModifiedDate = DateTime.Now;
            dbtog.ModifiedUser = Session["loggeduser"].ToString();
            dbtog.DataTransfer = 0;


           
          //  int docid = _bllcommon.GetDcumentId("TOG");
           // var docnum = _bllcommon.GetDocumentNo("TOG",
                                                  //  Convert.ToInt32(Session["loggeduserlocId"]),
                                                  //  "1",
                                                  //   docid,
                                                   //  false);

           // dbtog.DocumentId = docid;
           // dbtog.DocumentNo = docnum;

            // Aproved
            dbtog.DocumentStatus = 3;         
            dbtog.IsTempTOG = false;

            dbtog.ReferenceDocumentId = togheader.ReferenceDocumentId;
            dbtog.ReferenceNo = togheader.ReferenceNo;
            dbtog.FromLocationId = togheader.FromLocationId;
            dbtog.ToLocationId = togheader.ToLocationId;
            dbtog.GrossAmount = togheader.GrossAmount;
            dbtog.Remark = togheader.Remark;
            dbtog.NetAmount = togheader.NetAmount;
            dbtog.IsDelete = togheader.IsDelete;
            dbtog.TotSellingPrice = togheader.TotSellingPrice;
            dbtog.TotCostPrice = togheader.TotCostPrice;
            dbtog.TOGType = togheader.TOGType;
            dbtog.TotQty = togheader.TotQty;
            dbtog.TOGDate = togheader.TOGDate;
            dbtog.EventId = togheader.EventId;

            string GRNNO = _blltransferNote.GetBaseDocNo(togheader.TransferNoteHeaderId).GRNNo;

            PurchaseHeader GH = _bllgrn.GetGRNByDocNo(GRNNO, companyid);
            long GRNHeadID = 0;
            if(GH != null)
             GRNHeadID = _bllgrn.GetGRNByDocNo(GRNNO, companyid).PurchaseHeaderId;
            List<PurchaseDetail> Qtys = _bllgrn.GetGRNDet(GRNHeadID);

            // Make sure both lists have the same count
            if (togheader.TOGDetail.Count != Qtys.Count)
            {
                // Handle the case where counts do not match
                // You may throw an exception, log a warning, or handle it according to your requirements
            }
            PurchaseHeader PH = new PurchaseHeader();
            if (GRNHeadID != null)
            {
                PH = _bllgrn.GetGRNById(Convert.ToInt64(GRNHeadID));
                if (PH != null && PH.DocumentNo != null)
                    togheader.GRNNo = PH.DocumentNo;
            }

            List<InvRequestNotePOTransaction> POTransaction = new List<InvRequestNotePOTransaction>();

            if (PH != null)
                POTransaction = _bllgrn.GetRequestNoteDetailsforGRN(PH.POID, Convert.ToInt32(togheader.ToLocationId));

           

            if (POTransaction != null && POTransaction.Count > 0)
            {

            }
            else if (Qtys.Count == togheader.TOGDetail.Count)
            {

                for (int i = 0; i < togheader.TOGDetail.Count; i++)
                {
                    // Match each item from poheader.PODetail with corresponding item from Qtys

                    togheader.TOGDetail[i].OrderQty = Qtys[i].OrderQty;
                }
            }


            long POID = 0;
            if (PH != null && PH.POID != null)
                POID = PH.POID;

            bool res = _blltransferNote.UpdateTOG(dbtog, POID);
            @ViewBag.TOGCode = dbtog.DocumentNo;
            if (res == true)
            {
                @ViewBag.Message = "1";
                TransferNoteHeader th = new TransferNoteHeader();
                th.TOGDate = DateTime.Now;
                ViewBag.TransferNoteHeaderId = togheader.TransferNoteHeaderId;
                return View("~/Views/Transactions/TOG/EditTOG.cshtml", th);
            }
            else
            {
                @ViewBag.Message = "3";
                return View("~/Views/Transactions/TOG/EditTOG.cshtml", togheader);
            }

        }

        //[Authorize(Roles = "TOGReject")]
     //   [Authorize(Roles = "TogEdit")]
        [HttpPost]
        public ActionResult Reject(TransferNoteHeader togheader)
        {

            // check permissions
            if (!_appmanager.SetPermissions(5, Session["loggeduserempcode"].ToString(), "TOGReject"))
            {
                @ViewBag.Permissions = "No user permissions to Reject TOG s";
                return View("~/Views/Account/AccessDenied.cshtml");
            }
            //end check permissions
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            if (togheader.TOGDetail.Count == 0)
            {
                var deptdata = _bllproduct.GetActiveProducts(companyid);
                ViewBag.FromLocationId = togheader.FromLocationId;
                ViewBag.ToLocationId = togheader.ToLocationId;
                ViewBag.productdata = deptdata;
                ViewBag.EventId = togheader.EventId;
                @ViewBag.Message = "5";
                return View("~/Views/Transactions/TOG/EditTOG.cshtml", togheader);
            }
            if (string.IsNullOrEmpty(togheader.RejectReason))
            {
                var deptdata = _bllproduct.GetActiveProducts(companyid);
                ViewBag.FromLocationId = togheader.FromLocationId;
                ViewBag.ToLocationId = togheader.ToLocationId;
                ViewBag.productdata = deptdata;
                ViewBag.EventId = togheader.EventId;
                @ViewBag.Message = "7";
                return View("~/Views/Transactions/TOG/EditTOG.cshtml", togheader);
            }
            var dbtog = _blltransferNote.GetTOGById(togheader.TransferNoteHeaderId);
            // dbtog.TOGDetail = _blltransferNote.GetTOGDetById(dbtog.TransferNoteHeaderId).ToList();
            dbtog.TOGDetail = togheader.TOGDetail;


            dbtog.ModifiedDate = DateTime.Now;
            dbtog.ModifiedUser = Session["loggeduser"].ToString();
            dbtog.DataTransfer = 0;



          //  int docid = _bllcommon.GetDcumentId("TOG");
          //  var docnum = _bllcommon.GetDocumentNo("TOG",
                                                   // Convert.ToInt32(Session["loggeduserlocId"]),
                                                   // "1",
                                                   //  docid,
                                                   //  false);

          //  dbtog.DocumentId = docid;
           // dbtog.DocumentNo = docnum;

            // Aproved
            dbtog.DocumentStatus = 4;
            dbtog.IsTempTOG = false;

            dbtog.ReferenceDocumentId = togheader.ReferenceDocumentId;
            dbtog.ReferenceNo = togheader.ReferenceNo;
            dbtog.FromLocationId = togheader.FromLocationId;
            dbtog.ToLocationId = togheader.ToLocationId;
            dbtog.GrossAmount = togheader.GrossAmount;
            dbtog.Remark = togheader.Remark;
            dbtog.NetAmount = togheader.NetAmount;
            dbtog.IsDelete = togheader.IsDelete;
            dbtog.TotSellingPrice = togheader.TotSellingPrice;
            dbtog.TotCostPrice = togheader.TotCostPrice;
            dbtog.TOGType = togheader.TOGType;
            dbtog.TotQty = togheader.TotQty;
            dbtog.TOGDate = togheader.TOGDate;
            dbtog.EventId = togheader.EventId;

            PurchaseHeader PH = new PurchaseHeader();
            if (togheader.GRNNo != null)
            {
                PH = _bllgrn.GetGRNById(Convert.ToInt64(togheader.GRNNo));
                if(PH != null && PH.DocumentNo != null)
                    togheader.GRNNo = PH.DocumentNo;
            }
            bool res = _blltransferNote.UpdateTOG(dbtog, PH.POID);
            @ViewBag.TOGCode = dbtog.DocumentNo;
            if (res == true)
            {
                @ViewBag.Message = "1";
                TransferNoteHeader th = new TransferNoteHeader();
                th.TOGDate = DateTime.Now;
                ViewBag.TransferNoteHeaderId = togheader.TransferNoteHeaderId;
                return View("~/Views/Transactions/TOG/EditTOG.cshtml", th);
            }
            else
            {
                @ViewBag.Message = "3";
                return View("~/Views/Transactions/TOG/EditTOG.cshtml", togheader);
            }

        }

        //[Authorize(Roles = "TOGReOpen")]
      //  [Authorize(Roles = "TogEdit")]
        [HttpPost]
        public ActionResult Reopen(TransferNoteHeader togheader)
        {

            // check permissions
            if (!_appmanager.SetPermissions(5, Session["loggeduserempcode"].ToString(), "TOGReOpen"))
            {
                @ViewBag.Permissions = "No user permissions to Re Open TOG s";
                return View("~/Views/Account/AccessDenied.cshtml");
            }
            //end check permissions
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());

            bool EnableMonthEndProcess = _bllconfiguration.GetConfiguration("EnableMonthEndProcess", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;

            if (EnableMonthEndProcess)
            {

                int monthendstatus = _bllmonthend.CheckMonthEnd(Convert.ToInt32(togheader.FromLocationId), togheader.TOGDate.Year, togheader.TOGDate.Month);
                if (monthendstatus != 1)
                {
                    var deptdata = _bllproduct.GetActiveProducts(companyid);
                    ViewBag.FromLocationId = togheader.FromLocationId;
                    ViewBag.ToLocationId = togheader.ToLocationId;
                    ViewBag.productdata = deptdata;
                    ViewBag.EventId = togheader.EventId;
                    @ViewBag.Message = "6";
                    return View("~/Views/Transactions/TOG/EditTOG.cshtml", togheader);

                }
                //kapila 27/5/2020
                monthendstatus = _bllmonthend.CheckMonthEnd(Convert.ToInt32(togheader.ToLocationId), togheader.TOGDate.Year, togheader.TOGDate.Month);
                if (monthendstatus != 1)
                {
                    ModelState.Clear();
                    var deptdata = _bllproduct.GetActiveProducts(companyid);
                    ViewBag.locationId = togheader.FromLocationId;
                    ViewBag.ToLocationId = togheader.ToLocationId;
                    ViewBag.productdata = deptdata;
                    ViewBag.EventId = togheader.EventId;
                    @ViewBag.Message = "7";
                    return View("~/Views/Transactions/TOG/CreateTOG.cshtml", togheader);

                }
            }


            if (togheader.TOGDetail.Count == 0)
            {
                var deptdata = _bllproduct.GetActiveProducts(companyid);
                ViewBag.FromLocationId = togheader.FromLocationId;
                ViewBag.ToLocationId = togheader.ToLocationId;
                ViewBag.productdata = deptdata;
                ViewBag.EventId = togheader.EventId;
                @ViewBag.Message = "5";
                return View("~/Views/Transactions/TOG/EditTOG.cshtml", togheader);
            }
            var dbtog = _blltransferNote.GetTOGById(togheader.TransferNoteHeaderId);
            // dbtog.TOGDetail = _blltransferNote.GetTOGDetById(dbtog.TransferNoteHeaderId).ToList();
            dbtog.TOGDetail = togheader.TOGDetail;


            dbtog.ModifiedDate = DateTime.Now;
            dbtog.ModifiedUser = Session["loggeduser"].ToString();
            dbtog.DataTransfer = 0;



        //    int docid = _bllcommon.GetDcumentId("TOG");
         //   var docnum = _bllcommon.GetDocumentNo("TOG",
                                                 //   Convert.ToInt32(Session["loggeduserlocId"]),
                                                  //  "1",
                                                  //   docid,
                                                  //   false);

           // dbtog.DocumentId = docid;
           // dbtog.DocumentNo = docnum;

            // Aproved
            dbtog.DocumentStatus = 6;
            dbtog.IsTempTOG = false;

            dbtog.ReferenceDocumentId = togheader.ReferenceDocumentId;
            dbtog.ReferenceNo = togheader.ReferenceNo;
            dbtog.FromLocationId = togheader.FromLocationId;
            dbtog.ToLocationId = togheader.ToLocationId;
            dbtog.GrossAmount = togheader.GrossAmount;
            dbtog.Remark = togheader.Remark;
            dbtog.NetAmount = togheader.NetAmount;
            dbtog.IsDelete = togheader.IsDelete;
            dbtog.TotSellingPrice = togheader.TotSellingPrice;
            dbtog.TotCostPrice = togheader.TotCostPrice;
            dbtog.TOGType = togheader.TOGType;
            dbtog.TotQty = togheader.TotQty;
            dbtog.TOGDate = togheader.TOGDate;
            dbtog.EventId = togheader.EventId;

            PurchaseHeader PH = new PurchaseHeader();
            if (togheader.GRNNo != null)
            {
                PH = _bllgrn.GetGRNById(Convert.ToInt64(togheader.GRNNo));
                if (PH != null && PH.DocumentNo != null)
                    togheader.GRNNo = PH.DocumentNo;
            }
            bool res = _blltransferNote.UpdateTOG(dbtog,PH.POID);
            @ViewBag.TOGCode = dbtog.DocumentNo;
            if (res == true)
            {
                @ViewBag.Message = "1";
                TransferNoteHeader th = new TransferNoteHeader();
                th.TOGDate = DateTime.Now;
                ViewBag.TransferNoteHeaderId = togheader.TransferNoteHeaderId;
                return View("~/Views/Transactions/TOG/EditTOG.cshtml", th);
            }
            else
            {
                @ViewBag.Message = "3";
                return View("~/Views/Transactions/TOG/EditTOG.cshtml", togheader);
            }

        }

        // [Authorize(Roles = "TOGCancel")]
      //  [Authorize(Roles = "TogEdit")]
        [HttpPost]
        public ActionResult Cancel(TransferNoteHeader togheader)
        {

            // check permissions
            if (!_appmanager.SetPermissions(5, Session["loggeduserempcode"].ToString(), "TOGCancel"))
            {
                @ViewBag.Permissions = "No user permissions to Cancel TOG s";
                return View("~/Views/Account/AccessDenied.cshtml");
            }
            //end check permissions
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            if (togheader.TOGDetail.Count == 0)
            {
                var deptdata = _bllproduct.GetActiveProducts(companyid);
                ViewBag.FromLocationId = togheader.FromLocationId;
                ViewBag.ToLocationId = togheader.ToLocationId;
                ViewBag.productdata = deptdata;
                ViewBag.EventId = togheader.EventId;
                @ViewBag.Message = "5";
                return View("~/Views/Transactions/TOG/EditTOG.cshtml", togheader);
            }
            if (string.IsNullOrEmpty(togheader.CancelReason))
            {
                var deptdata = _bllproduct.GetActiveProducts(companyid);
                ViewBag.FromLocationId = togheader.FromLocationId;
                ViewBag.ToLocationId = togheader.ToLocationId;
                ViewBag.productdata = deptdata;
                ViewBag.EventId = togheader.EventId;
                @ViewBag.Message = "8";
                return View("~/Views/Transactions/TOG/EditTOG.cshtml", togheader);
            }
            var dbtog = _blltransferNote.GetTOGById(togheader.TransferNoteHeaderId);
            //  dbtog.TOGDetail = _blltransferNote.GetTOGDetById(dbtog.TransferNoteHeaderId).ToList();
            dbtog.TOGDetail = togheader.TOGDetail;


            dbtog.ModifiedDate = DateTime.Now;
            dbtog.ModifiedUser = Session["loggeduser"].ToString();
            dbtog.DataTransfer = 0;



          //  int docid = _bllcommon.GetDcumentId("TOG");
         //   var docnum = _bllcommon.GetDocumentNo("TOG",
                                                   // Convert.ToInt32(Session["loggeduserlocId"]),
                                                   // "1",
                                                   //  docid,
                                                   //  false);

          //  dbtog.DocumentId = docid;
           // dbtog.DocumentNo = docnum;

            // Aproved
            dbtog.DocumentStatus = 5;
            dbtog.IsTempTOG = false;

            dbtog.ReferenceDocumentId = togheader.ReferenceDocumentId;
            dbtog.ReferenceNo = togheader.ReferenceNo;
            dbtog.FromLocationId = togheader.FromLocationId;
            dbtog.ToLocationId = togheader.ToLocationId;
            dbtog.GrossAmount = togheader.GrossAmount;
            dbtog.Remark = togheader.Remark;
            dbtog.NetAmount = togheader.NetAmount;
            dbtog.IsDelete = togheader.IsDelete;
            dbtog.TotSellingPrice = togheader.TotSellingPrice;
            dbtog.TotCostPrice = togheader.TotCostPrice;
            dbtog.TOGType = togheader.TOGType;
            dbtog.TotQty = togheader.TotQty;
            dbtog.TOGDate = togheader.TOGDate;
            dbtog.EventId = togheader.EventId;

            PurchaseHeader PH = new PurchaseHeader();
            if (togheader.GRNNo != null)
            {
                PH = _bllgrn.GetGRNById(Convert.ToInt64(togheader.GRNNo));
                if (PH != null && PH.DocumentNo != null)
                    togheader.GRNNo = PH.DocumentNo;
            }
            bool res = _blltransferNote.UpdateTOG(dbtog, PH.POID);
            @ViewBag.TOGCode = dbtog.DocumentNo;
            if (res == true)
            {
                @ViewBag.Message = "1";
                TransferNoteHeader th = new TransferNoteHeader();
                th.TOGDate = DateTime.Now;
                ViewBag.TransferNoteHeaderId = togheader.TransferNoteHeaderId;
                return View("~/Views/Transactions/TOG/EditTOG.cshtml", th);
            }
            else
            {
                @ViewBag.Message = "3";
                return View("~/Views/Transactions/TOG/EditTOG.cshtml", togheader);
            }

        }

        // [Authorize(Roles = "TOGForsefullyClose")]
     //   [Authorize(Roles = "TogEdit")]
        [HttpPost]
        public ActionResult ForcefullyClose(TransferNoteHeader togheader)
        {

            // check permissions
            if (!_appmanager.SetPermissions(5, Session["loggeduserempcode"].ToString(), "TOGForsefullyClose"))
            {
                @ViewBag.Permissions = "No user permissions to Forcefully Colse TOG s";
                return View("~/Views/Account/AccessDenied.cshtml");
            }
            //end check permissions
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            if (togheader.TOGDetail.Count == 0)
            {
                var deptdata = _bllproduct.GetActiveProducts(companyid);
                ViewBag.FromLocationId = togheader.FromLocationId;
                ViewBag.ToLocationId = togheader.ToLocationId;
                ViewBag.productdata = deptdata;
                ViewBag.EventId = togheader.EventId;
                @ViewBag.Message = "5";
                return View("~/Views/Transactions/TOG/EditTOG.cshtml", togheader);
            }
            var dbtog = _blltransferNote.GetTOGById(togheader.TransferNoteHeaderId);
            // dbtog.TOGDetail = _blltransferNote.GetTOGDetById(dbtog.TransferNoteHeaderId).ToList();
            dbtog.TOGDetail = togheader.TOGDetail;


            dbtog.ModifiedDate = DateTime.Now;
            dbtog.ModifiedUser = Session["loggeduser"].ToString();
            dbtog.DataTransfer = 0;



            //  int docid = _bllcommon.GetDcumentId("TOG");
            //   var docnum = _bllcommon.GetDocumentNo("TOG",
            // Convert.ToInt32(Session["loggeduserlocId"]),
            // "1",
            //  docid,
            //  false);

            //  dbtog.DocumentId = docid;
            // dbtog.DocumentNo = docnum;

            // Aproved
            dbtog.DocumentStatus = 7;
            dbtog.IsTempTOG = false;

            dbtog.ReferenceDocumentId = togheader.ReferenceDocumentId;
            dbtog.ReferenceNo = togheader.ReferenceNo;
            dbtog.FromLocationId = togheader.FromLocationId;
            dbtog.ToLocationId = togheader.ToLocationId;
            dbtog.GrossAmount = togheader.GrossAmount;
            dbtog.Remark = togheader.Remark;
            dbtog.NetAmount = togheader.NetAmount;
            dbtog.IsDelete = togheader.IsDelete;
            dbtog.TotSellingPrice = togheader.TotSellingPrice;
            dbtog.TotCostPrice = togheader.TotCostPrice;
            dbtog.TOGType = togheader.TOGType;
            dbtog.TotQty = togheader.TotQty;
            dbtog.TOGDate = togheader.TOGDate;
            dbtog.EventId = togheader.EventId;

            PurchaseHeader PH = new PurchaseHeader();
            if (togheader.GRNNo != null)
            {
                PH = _bllgrn.GetGRNById(Convert.ToInt64(togheader.GRNNo));
                if (PH != null && PH.DocumentNo != null)
                    togheader.GRNNo = PH.DocumentNo;
            }
            bool res = _blltransferNote.UpdateTOG(dbtog,PH.POID);
            @ViewBag.TOGCode = dbtog.DocumentNo;
            if (res == true)
            {
                @ViewBag.Message = "1";
                TransferNoteHeader th = new TransferNoteHeader();
                th.TOGDate = DateTime.Now;
                ViewBag.TransferNoteHeaderId = togheader.TransferNoteHeaderId;
                return View("~/Views/Transactions/TOG/EditTOG.cshtml", th);
            }
            else
            {
                @ViewBag.Message = "3";
                return View("~/Views/Transactions/TOG/EditTOG.cshtml", togheader);
            }

        }

        private List<TOGViewModel> CheckStockMissMatches(TransferNoteHeader tog)
        {


            List<TOGViewModel> vvm = new List<TOGViewModel>();
            // TransferNoteService togservice = new TransferNoteService();
            foreach (var item in tog.TOGDetail)
            {
                TOGViewModel vm = new TOGViewModel();
                decimal actQuantity = _blltransferNote.CheckFromLocStock(tog.FromLocationId, item.ProductId, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                if (!_blltransferNote.CheckIfExist(tog.FromLocationId, tog.ToLocationId, item.ProductId, Convert.ToInt32(Session["loggedusercompanyId"].ToString())))
                {

                    vm.Message = "Item: " + item.ProductId + " is not avaliable in both locations";
                    vm.Availablility = false;

                }
                if (actQuantity < item.OrderQty)
                {
                    vm.CrrQty = actQuantity;
                    vm.Qty = item.OrderQty;
                    vm.StockMessage = "Order Quantity of Item -  " + item.ProductId + ": " + item.OrderQty + " is grater then actual Quantity: " + actQuantity;

                }


                vvm.Add(vm);

            }

            return vvm;
        }

     //   [Authorize(Roles = "TogEdit")]
        [HttpPost]
        public ActionResult TempTOG(TransferNoteHeader togdet)
        {

            // check permissions
            if (!_appmanager.SetPermissions(5, Session["loggeduserempcode"].ToString(), "TOGEdit"))
            {
                @ViewBag.Permissions = "No user permissions to Edit TOG s";
                return View("~/Views/Account/AccessDenied.cshtml");
            }
            //end check permissions

            return View("~/Views/Transactions/TOG/EditTOG.cshtml");
        }

        [HttpGet]
        public JsonResult GetActivePaytmentMethods()
        {
            //TransferNoteService reporsitory = new TransferNoteService();
            var paymentMethods = _blltransferNote.GetActivePaymentMethods(Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            return Json(JsonConvert.SerializeObject(paymentMethods, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult GetProductData(long id, decimal qty, long frmlocid, long tolocid,
                                        string fromlocname, string tolocname,string foo)
        {
            TOGViewModel vm = new TOGViewModel();
            if (_blltransferNote.CheckProductExistsAtLoc(foo,id, frmlocid, tolocid, Convert.ToInt32(Session["loggedusercompanyId"].ToString())))
            {
                //TransferNoteService reporsitory = new TransferNoteService();
                //ProductService _productservice = new ProductService();
                var tog = _blltransferNote.GetTOGProductById(id, frmlocid, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                vm.ItemId = id;
                //Added by pavithra
                vm.ItemDesc = tog.ProductCode + " - " + tog.ProductName;
                vm.Qty = qty;
                vm.SellingPrice = tog.SellingPrice * qty;
                //vm.CostPrice = tog.AvgCost * qty;
                vm.CostPrice = tog.CostPrice * qty;
                vm.UnitCost = tog.CostPrice;
                vm.UnitSelling = tog.SellingPrice;
                vm.CrrQty = tog.Stock;
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
            //vm.DiscountType = "0";
            vm.DiscountType = "N/A";
            var ddd = new JsonResult { Data = vm, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            return new JsonResult { Data = vm, JsonRequestBehavior = JsonRequestBehavior.AllowGet };


        }

        [HttpGet]
        //  public JsonResult GetGRNData(long id)
        public JsonResult GetGRNData(string id, string fromlocid, string tolocid)
        {
            // long tolocid = 0;
           
           

            PurchaseHeader PH = new PurchaseHeader();
            //   PH = _bllgrn.GetGRNByDocNo(id, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            PH = _bllgrn.GetGRNById(Convert.ToInt64(id));
            if (PH != null)
            {
                PH.GRNId = Convert.ToInt64(id);
                //if (_bllgrn.GetGRNByDocNo(id, Convert.ToInt32(Session["loggedusercompanyId"].ToString())).GRNLocationId != Convert.ToInt32(fromlocid))
                //{
                //    return new JsonResult { Data = 2, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                //}
                //if (_bllgrn.GetGRNByDocNo(id, Convert.ToInt32(Session["loggedusercompanyId"].ToString())).DocumentStatus != 3)
                //{
                //    return new JsonResult { Data = 1, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                //}

                if (_bllgrn.GetGRNByDocNo(PH.DocumentNo, Convert.ToInt32(Session["loggedusercompanyId"].ToString())).GRNLocationId != Convert.ToInt32(fromlocid))
                {
                    return new JsonResult { Data = 2, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                }
                //if (_bllgrn.GetGRNByDocNo(PH.DocumentNo, Convert.ToInt32(Session["loggedusercompanyId"].ToString())).DocumentStatus != 3)
                //{
                //    return new JsonResult { Data = 1, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                //}


            }
            List<TOGViewModel> vvm = new List<TOGViewModel>();

            
            if (PH != null)
            {
                List<InvRequestNotePOTransaction> POTransaction = _bllgrn.GetRequestNoteDetailsforGRN(PH.POID, Convert.ToInt32(tolocid));

                string RequestNoteDetails = "";

                if (POTransaction != null && POTransaction.Count > 0)
                {
                    int Sequence = 0;



                    foreach (var pd in _bllgrn.GetAllGRNProductsByDocNumber(PH.DocumentNo, Convert.ToInt32(Session["loggedusercompanyId"].ToString())).Where(g => g.GRNQuantity >= g.TOGQty && g.GRNQuantity >= g.PRNQuantity))
                    {
                        var product = _bllproduct.GetProductById(pd.ProductID);

                        var PList = POTransaction.FindAll(Prod => Prod.ProductID == pd.ProductID).ToList();

                        if (PList != null)
                        {


                            foreach (InvRequestNotePOTransaction P in PList)
                            {

                                RequestNoteDetails = RequestNoteDetails + P.RequestNoteDocumentNo + " - " + P.ReqNoteCreatedDate.ToShortDateString() + " ";
                                TOGViewModel vm = new TOGViewModel();
                                vm.ItemId = pd.ProductID;
                                Sequence++;
                                vm.Sequence = Sequence;

                                decimal GRNBalanceQty = (pd.GRNQuantity - pd.PRNQuantity);
                                decimal ReqNoteBalanceQty = (P.QTY - P.IssueQtY);

                                decimal lineQty = 0;

                                if (ReqNoteBalanceQty >= GRNBalanceQty)
                                    lineQty = GRNBalanceQty;
                                else if (GRNBalanceQty >= ReqNoteBalanceQty)
                                    lineQty = ReqNoteBalanceQty;


                                vm.ReqNoteCreatedDate = P.ReqNoteCreatedDate;
                                vm.RequestNoteDocumentNo = P.RequestNoteDocumentNo;
                                vm.RequestNoteHeaderID = P.RequestNoteHeaderID;

                                vm.Qty = lineQty;
                                vm.SellingPrice = pd.SellingPrice * vm.Qty;
                                vm.CostPrice = pd.CostPrice * vm.Qty;
                                vm.UnitCost = pd.CostPrice;
                                vm.UnitCost = pd.CostPrice;

                                vm.ItemDesc = product.ProductName;
                                vm.HasItem = _blltransferNote.CheckToLocation(pd.ProductID, Convert.ToInt64(tolocid), Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                                vm.ProductCode = product.ProductCode;
                                vm.PurchaseHeaderId = pd.PurchaseHeaderID;
                                vm.CurrentStock = _bllproductstock.GetProductStockMasterByProductIdLocid(pd.ProductID, Convert.ToInt32(fromlocid)).Stock;
                                vm.SIH = vm.CurrentStock;
                                vvm.Add(vm);
                            }
                        }
                    }
                    Session["RequestNoteDetails"] = RequestNoteDetails;
                    return new JsonResult { Data = vvm, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                }

                else
                {
                    int Sequence = 0;
                    foreach (var pd in _bllgrn.GetAllGRNProductsByDocNumber(PH.DocumentNo, Convert.ToInt32(Session["loggedusercompanyId"].ToString())).Where(g => g.GRNQuantity >= g.TOGQty && g.GRNQuantity >= g.PRNQuantity))
                    {
                        var product = _bllproduct.GetProductById(pd.ProductID);

                        TOGViewModel vm = new TOGViewModel();
                        Sequence++;
                        vm.Sequence = Sequence;

                        vm.ItemId = pd.ProductID;
                        //vm.Qty = pd.OrderQty - pd.TOGQty;
                        vm.Qty = pd.GRNQuantity - pd.TOGQty - pd.PRNQuantity;
                        vm.SellingPrice = pd.SellingPrice * vm.Qty;
                        vm.CostPrice = pd.CostPrice * vm.Qty;
                        vm.UnitCost = pd.CostPrice;
                        vm.ItemDesc = product.ProductName;
                        vm.HasItem = _blltransferNote.CheckToLocation(pd.ProductID, Convert.ToInt64(tolocid), Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                        vm.ProductCode = product.ProductCode;
                        vm.PurchaseHeaderId = pd.PurchaseHeaderID;
                        vm.CurrentStock = _bllproductstock.GetProductStockMasterByProductIdLocid(pd.ProductID, Convert.ToInt32(fromlocid)).Stock;
                        vm.SIH = vm.CurrentStock;
                        vvm.Add(vm);
                    }



                    //if (PH.IsPRN == true)
                    //{
                    //    var prn = _bllgrn.GetGRNById(PH.GRNId);
                    //    if (prn.DocumentStatus == 4 || prn.DocumentStatus == 5 || prn.DocumentStatus == 7)
                    //    {


                    //        foreach (var pd in _bllgrn.GetAllGRNProductsByDocNumber(id))
                    //        {
                    //            var product = _bllproduct.GetProductById(pd.ProductID);

                    //            TOGViewModel vm = new TOGViewModel();
                    //            vm.ItemId = pd.ProductID;

                    //            // vm.Qty = pd.GRNQuantity - pd.TOGQty - pd.PRNQuantity;
                    //            vm.Qty = pd.GRNQuantity;
                    //            vm.SellingPrice = pd.SellingPrice * vm.Qty;
                    //            vm.CostPrice = pd.CostPrice * vm.Qty;

                    //            vm.ItemDesc = product.ProductName;
                    //            vm.HasItem = _blltransferNote.CheckToLocation(pd.ProductID, Convert.ToInt64(tolocid));
                    //            vm.ProductCode = product.ProductCode;
                    //            vm.PurchaseHeaderId = pd.PurchaseHeaderID;
                    //            vm.CurrentStock = _bllproductstock.GetProductStockMasterByProductIdLocid(pd.ProductID, Convert.ToInt32(fromlocid)).Stock;
                    //            vvm.Add(vm);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        foreach (var pd in _bllgrn.GetAllGRNProductsByDocNumber(id).Where(g => g.GRNQuantity >= g.TOGQty && g.GRNQuantity >= g.PRNQuantity))
                    //        {
                    //            var product = _bllproduct.GetProductById(pd.ProductID);

                    //            TOGViewModel vm = new TOGViewModel();
                    //            vm.ItemId = pd.ProductID;
                    //            //vm.Qty = pd.OrderQty - pd.TOGQty;
                    //            vm.Qty = pd.GRNQuantity - pd.TOGQty - pd.PRNQuantity;
                    //            vm.SellingPrice = pd.SellingPrice * vm.Qty;
                    //            vm.CostPrice = pd.CostPrice * vm.Qty;

                    //            vm.ItemDesc = product.ProductName;
                    //            vm.HasItem = _blltransferNote.CheckToLocation(pd.ProductID, Convert.ToInt64(tolocid));
                    //            vm.ProductCode = product.ProductCode;
                    //            vm.PurchaseHeaderId = pd.PurchaseHeaderID;
                    //            vm.CurrentStock = _bllproductstock.GetProductStockMasterByProductIdLocid(pd.ProductID, Convert.ToInt32(fromlocid)).Stock;
                    //            vvm.Add(vm);
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    foreach (var pd in _bllgrn.GetAllGRNProductsByDocNumber(id).Where(g => g.GRNQuantity >= g.TOGQty && g.GRNQuantity >= g.PRNQuantity))
                    //    {
                    //        var product = _bllproduct.GetProductById(pd.ProductID);

                    //        TOGViewModel vm = new TOGViewModel();
                    //        vm.ItemId = pd.ProductID;
                    //        //vm.Qty = pd.OrderQty - pd.TOGQty;
                    //        vm.Qty = pd.GRNQuantity - pd.TOGQty - pd.PRNQuantity;
                    //        vm.SellingPrice = pd.SellingPrice * vm.Qty;
                    //        vm.CostPrice = pd.CostPrice * vm.Qty;

                    //        vm.ItemDesc = product.ProductName;
                    //        vm.HasItem = _blltransferNote.CheckToLocation(pd.ProductID, Convert.ToInt64(tolocid));
                    //        vm.ProductCode = product.ProductCode;
                    //        vm.PurchaseHeaderId = pd.PurchaseHeaderID;
                    //        vm.CurrentStock = _bllproductstock.GetProductStockMasterByProductIdLocid(pd.ProductID, Convert.ToInt32(fromlocid)).Stock;
                    //        vvm.Add(vm);
                    //    }
                    //}


                    //List<TOGViewModel> vvm = new List<TOGViewModel>();

                    return new JsonResult { Data = vvm, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

                }
            }
            else
            {
                return new JsonResult { Data = vvm, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }

        [HttpGet]
        public JsonResult GetRequestNoteData(long id)
        {
           
           
            List<TOGViewModel> vvm = new List<TOGViewModel>();
            foreach (var pd in _bllgrn.GetRequestNoteData(id))
            {
                TOGViewModel vm = new TOGViewModel();
                vm.ItemId = pd.MaterialId;
                vm.Qty = pd.MaterialQty;
                vm.SellingPrice = pd.SellingPrice;
                vm.CostPrice = pd.CostPrice;
                vm.ItemDesc = _bllproduct.GetProductById(pd.MaterialId).ProductName;
                vm.IssueQty = pd.IssueQty;
                vvm.Add(vm);
            }

            return new JsonResult { Data = vvm, JsonRequestBehavior = JsonRequestBehavior.AllowGet };


        }

        [Authorize(Roles = "Reports")]
        public ActionResult RPTTOGSummary(TOGSummaryViewModel vvm)
        {
            if (_bllconfiguration.GetConfiguration("UReports", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn)
            {

                if (!_appmanager.CheckReportPermissions(999, Session["loggeduserempcode"].ToString(), "RPTTOGSummary"))
                {
                    @ViewBag.Permissions = "No user permissions to View TOG Summary";
                    return View("~/Views/Account/AccessDenied.cshtml");
                }

            }
            var tog = _blltransferNote.GetTOGSummaryReport(vvm.LocationId, vvm.DocumentId, vvm.DateFrom, vvm.DateTo);
            List<TOGSummaryViewModel> togsummarymodel = new List<TOGSummaryViewModel>();
            foreach (var s in tog)
            {
                var fromlocation= _blllocation.GetLocationById(s.FromLocationId);
                var tolocation = _blllocation.GetLocationById(s.ToLocationId);

                TOGSummaryViewModel v = new TOGSummaryViewModel();
                v.TransferNoteHeaderId = s.TransferNoteHeaderId;
                v.DocumentNo = s.DocumentNo;
                if (fromlocation != null)
                {
                    v.Location = fromlocation.LocationName;

                }
                if (tolocation != null)
                {
                    v.ToLocation = tolocation.LocationName;
                }
                v.DocumentDate = s.TOGDate;
                v.NetAmount = s.NetAmount;
              
                var docstatus = _blldocstatus.GetDocStatusById(s.DocumentStatus);
                if (docstatus != null) {
                    v.Status = docstatus.Description;
                        }
                togsummarymodel.Add(v);
            }
         
            return View("~/Views/Reports/TOG/RPTTOGSummary.cshtml", togsummarymodel);
        }

        [HttpGet]
        public JsonResult GetDocNoByLocId(long locid, long tolocid, DateTime from,DateTime to)
        {
            var types = _blltransferNote.GetDocNoByLocId(locid, tolocid, Convert.ToInt32(Session["loggedusercompanyId"].ToString())
                ).Where(t=>t.TOGDate.Date>=from.Date && t.TOGDate.Date<=to.Date).Select(t=> new {t.TransferNoteHeaderId,t.DocumentNo });
            return Json(JsonConvert.SerializeObject(types, Formatting.None,
                new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }),
                JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize(Roles = "Reports")]
        public ActionResult GetTOGDetailReport()
        {
            if (_bllconfiguration.GetConfiguration("UReports", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn)
            {

                if (!_appmanager.CheckReportPermissions(999, Session["loggeduserempcode"].ToString(), "TOGDetail"))
                {
                    @ViewBag.Permissions = "No user permissions to View TOG Details";
                    return View("~/Views/Account/AccessDenied.cshtml");
                }

            }
            return View("~/Views/Reports/TOG/RPTTOGDetails.cshtml");
        }

        [HttpPost]
        [Authorize(Roles = "Reports")]
        public ActionResult GetTOGDetailReport(TOGDetailViewModel vm)
        {

            if (_bllconfiguration.GetConfiguration("UReports", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn)
            {

                if (!_appmanager.CheckReportPermissions(999, Session["loggeduserempcode"].ToString(), "TOGDetail"))
                {
                    @ViewBag.Permissions = "No user permissions to View TOG Details";
                    return View("~/Views/Account/AccessDenied.cshtml");
                }

            }
            var reportdata = _blltransferNote.GetTOGDetailReport(vm.LocationId, vm.ToLocationId, vm.DocumentId,
                                                                    vm.DateFrom, vm.DateTo);

            ViewBag.DateFrom = vm.DateFrom.ToString("yyyy-MM-dd");
            ViewBag.DateTo = vm.DateTo.ToString("yyyy-MM-dd");
            ViewBag.LocationId = vm.LocationId;
            ViewBag.ToLocationId = vm.ToLocationId;
            ViewBag.DocumentId = vm.DocumentId;

            return View("~/Views/Reports/TOG/RPTTOGDetails.cshtml", reportdata);
        }


        [HttpGet]
        public ActionResult Stock()
        {

            return View("~/Views/Transactions/TOG/StockErrors.cshtml");
        }

        [HttpGet]
        public PartialViewResult AddUserPartialView()
        {
            return PartialView("TOGError", new TransferNoteHeader());
        }

      //  [Authorize(Roles = "TogEdit")]
        [HttpGet]
        public ActionResult Print(long id, int printstatus)
        {
            // check permissions
                //if (!_appmanager.SetPermissions(5, Session["loggeduserempcode"].ToString(), "TogEdit"))
                //{
                //    @ViewBag.Permissions = "No user permissions to Edit TOG s";
                //    return View("~/Views/Account/AccessDenied.cshtml");
                //}
            //end check permissions

          
            TransferNoteHeader togheader = new TransferNoteHeader();
            togheader = _blltransferNote.GetTOGById(id);
            togheader.TOGDetail = _blltransferNote.GetTOGDetById(id).ToList();

            togheader.From = _blllocation.GetLocationById(togheader.FromLocationId).LocationName;
            togheader.To = _blllocation.GetLocationById(togheader.ToLocationId).LocationName;
            togheader.SysCompany = _bllcompany.GetCompanyById(togheader.CompanyID);
            togheader.Company = togheader.SysCompany.CompanyName;
            togheader.PrintStatus = printstatus;
            if (togheader.DocumentStatus != 0)
            {
                togheader.DocState = _blldocstatus.GetDocStatusById(togheader.DocumentStatus).Description;
            }
            else
            {
                togheader.DocState = "N/A";
            }
            //kapila 9/6/2020
            if (togheader.ReferenceDocumentId != 0)
            {
                if (togheader.TOGType == "GRNBased")
                    togheader.BaseDocNo = _bllgrn.GetGRNById(togheader.ReferenceDocumentId).DocumentNo;
                else if (togheader.TOGType == "RequestNoteBased")
                    togheader.BaseDocNo = _bllrequest.GetBaseDocNo(togheader.ReferenceDocumentId).DocumentNo;
            }
            else
                togheader.BaseDocNo = "N/A";

            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());


            long GRNHeadID = 0;
            PurchaseHeader P = _bllgrn.GetGRNByDocNo(togheader.GRNNo, companyid);
            if (P != null && P.PurchaseHeaderId != null)
                GRNHeadID = _bllgrn.GetGRNByDocNo(togheader.GRNNo, companyid).PurchaseHeaderId;

            
            List<PurchaseDetail> Qtys = _bllgrn.GetGRNDet(GRNHeadID);

            // Make sure both lists have the same count
            if (togheader.TOGDetail.Count != Qtys.Count)
            {
                // Handle the case where counts do not match
                // You may throw an exception, log a warning, or handle it according to your requirements
            }
            PurchaseHeader PH = new PurchaseHeader();
            //   PH = _bllgrn.GetGRNByDocNo(id, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            PH = _bllgrn.GetGRNById(Convert.ToInt64(GRNHeadID));


            List<InvRequestNotePOTransaction> POTransaction = new List<InvRequestNotePOTransaction>();

            if (PH != null)
                POTransaction = _bllgrn.GetRequestNoteDetailsforGRN(PH.POID, Convert.ToInt32(togheader.ToLocationId));

            

            if (POTransaction != null && POTransaction.Count > 0)
            {
                for (int i = 0; i < togheader.TOGDetail.Count; i++)
                {
                    // Match each item from poheader.PODetail with corresponding item from Qtys

                    //togheader.TOGDetail.ElementAt(i).OrderQty = Qtys[i].OrderQty;
                    var prd = _bllproduct.GetProductById(togheader.TOGDetail.ElementAt(i).ProductId);
                    togheader.TOGDetail.ElementAt(i).ProductName = prd.ProductName;
                    togheader.TOGDetail.ElementAt(i).ProductCode = prd.ProductCode;
                }
            }
            else if (Qtys.Count == togheader.TOGDetail.Count)
            {




                for (int i = 0; i < togheader.TOGDetail.Count; i++)
                {
                    // Match each item from poheader.PODetail with corresponding item from Qtys

                    togheader.TOGDetail.ElementAt(i).OrderQty = Qtys[i].OrderQty;
                    var prd = _bllproduct.GetProductById(togheader.TOGDetail.ElementAt(i).ProductId);
                    togheader.TOGDetail.ElementAt(i).ProductName = prd.ProductName;
                    togheader.TOGDetail.ElementAt(i).ProductCode = prd.ProductCode;
                    
                }
            }
           
            else
            {
                togheader.TOGDetail.ToList().ForEach(d =>
                {
                    var prd = _bllproduct.GetProductById(d.ProductId);
                    d.ProductName = prd.ProductName;
                    d.ProductCode = prd.ProductCode;
                    d.RequestNoteDocumentNo = "";
                });

            }

            //togheader.TOGDetail.ToList().ForEach(d =>
            //{
            //    var prd = _bllproduct.GetProductById(d.ProductId);
            //    d.ProductName = prd.ProductName;
            //    d.ProductCode = prd.ProductCode;
            //});

            return PartialView("~/Views/Receipts/TOGView.cshtml", togheader);
        }



    }
}