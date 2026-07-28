using Newtonsoft.Json;
using RIT.HMS.BLL.Common;
using RIT.HMS.BLL.Configurations;
using RIT.HMS.BLL.MasterData;
using RIT.HMS.BLL.TransactionData;
using RIT.HMS.Domain.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HospitalityManagement.Controllers.Transactions
{
   [SessionTimeout]
    public class CustomProductionController : Controller
    {
        private readonly BLL_Configuration _bllconfiguration;
        private readonly BLL_CustomProductionNote _bllcustomproductionnote;
        private readonly BLL_Product _bllproduct;
        private readonly BLL_Location _blllocation;
        private readonly BLL_Common _bllCommon;
        private AppManager _appmanager;


        public CustomProductionController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _bllconfiguration = new BLL_Configuration(cn);
            _bllcustomproductionnote = new BLL_CustomProductionNote(cn);
            _bllproduct = new BLL_Product(cn);
            _blllocation = new BLL_Location(cn);
            _bllCommon = new BLL_Common(cn);
            _appmanager = new AppManager(cn);

        }

        public ActionResult Index()
        {


            // check permissions
            if (!_appmanager.SetPermissions(12, Session["loggeduserempcode"].ToString(), "CustomProductionCreatee"))
            {
                @ViewBag.Permissions = "No user permissions to Create Custom Production Notes";
                return View("~/Views/Account/AccessDenied.cshtml");
            }
            //end check permissions


            var productionheader = new CustomProductionNoteHeader();
            // productionheader.DocumentNo = commonService.GetDocumentNo("Production", 1, "1", 2, true);

            int docid = _bllCommon.GetDcumentId("CustomProductions", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            var docnum = _bllCommon.GetDocumentNo("CustomProductions",
                                                    Convert.ToInt32(Session["loggeduserlocId"]),
                                                    "1",
                                                     docid,
                                                     true, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));

            productionheader.DocumentNo = docnum;
            Session["CPWizadUI"] = _bllconfiguration.GetConfiguration("UIWZ", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            return View("~/Views/Transactions/CustomProduction/CustomProduction.cshtml", productionheader);
        }

        [HttpPost]
        public ActionResult Create(CustomProductionNoteHeader c)
        {

            // check permissions
            if (!_appmanager.SetPermissions(12, Session["loggeduserempcode"].ToString(), "CustomProductionCreatee"))
            {
                @ViewBag.Permissions = "No user permissions to Create Custom Production Notes";
                return View("~/Views/Account/AccessDenied.cshtml");
            }
            //end check permissions

            if (c.CustomProductionNoteDetail == null)
            {
                var productionheader = new CustomProductionNoteHeader();
              
                //int docid1 = _bllCommon.GetDcumentId("CustomProductions");
                //var docnum1 = _bllCommon.GetDocumentNo("CustomProductions",
                //                                        Convert.ToInt32(Session["loggeduserlocId"]),
                //                                        "1",
                //                                         docid,
                //                                         true);

                ModelState.Clear();                         
                productionheader.DocumentNo = c.DocumentNo;
                @ViewBag.Message = "3";
                @ViewBag.PCode = c.DocumentNo;
                return View("~/Views/Transactions/CustomProduction/CustomProduction.cshtml", c);

            }

            int docid = _bllCommon.GetDcumentId("CustomProductions", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            var docnum = _bllCommon.GetDocumentNo("CustomProductions",
                                                    Convert.ToInt32(Session["loggeduserlocId"]),
                                                    "1",
                                                     docid,
                                                     false, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            c.DocumentId = docid;
            c.DocumentNo = docnum;
            c.LocationId = Convert.ToInt32(Session["loggeduserlocId"]);
            c.CompanyID = 1;
            c.GroupOfCompanyID = 1;
            c.CreatedDate = DateTime.Now;
            c.CreatedUser= Session["loggeduser"].ToString();
            c.IsFinished = true;
            c.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var res = _bllcustomproductionnote.Save(c);
            if (res != 0)
            {
                var productionheader = new CustomProductionNoteHeader();
                // productionheader.DocumentNo = commonService.GetDocumentNo("Production", 1, "1", 2, true);

                int docid1 = _bllCommon.GetDcumentId("CustomProductions", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                var docnum1 = _bllCommon.GetDocumentNo("CustomProductions",
                                                        Convert.ToInt32(Session["loggeduserlocId"]),
                                                        "1",
                                                         docid,
                                                         true, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                ModelState.Clear();
                @ViewBag.Message = "1";
                @ViewBag.PCode = c.DocumentNo;
                productionheader.DocumentNo = docnum1;               
                return View("~/Views/Transactions/CustomProduction/CustomProduction.cshtml", productionheader);
            }
            else
            {
                @ViewBag.Message = "3";
                @ViewBag.PCode = c.DocumentNo;
                return View("~/Views/Transactions/CustomProduction/CustomProduction.cshtml", c);
            }

           
        }


        //------------ Loaders ---------------------------------------------------------------------------
        [HttpGet]
        public JsonResult GetActiveRecipts()
        {
            // changed to get active Advance notes 10/082020 
            var recipts = _bllcustomproductionnote.GetActiveRecipts(Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            return Json(JsonConvert.SerializeObject(recipts, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetReciptById(int reciptid)
        {
            var rec = _bllcustomproductionnote.GetReciptById(reciptid, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            return new JsonResult
            {
                Data = rec,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };        
        }

     

        [HttpGet]
        public JsonResult GetMaterialsByRecProcessLoc(int reciptid)
        {
            var processlocid = _bllcustomproductionnote.GetReciptById(reciptid, Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ProcessLoc;
            var locmaterials = _bllproduct.GetAllProductionItems(processlocid);
            return Json(JsonConvert.SerializeObject(locmaterials, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult GetMaterialsByProcessLoc(int processlocid)
        {
            
            var locmaterials = _bllproduct.GetAllProductionItems(processlocid);
            return Json(JsonConvert.SerializeObject(locmaterials, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAdvanceNoteDetByRecId(int reciptid)
        {
            var locmaterials = _bllcustomproductionnote.GetAdvanceNoteDetByRecId(reciptid, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            return Json(JsonConvert.SerializeObject(locmaterials, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetMaterialDataByLocIdMatId(int locid,int materialid)
        {
            var locmaterial = _bllcustomproductionnote.GetMaterialsByLocId(locid,materialid, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            return new JsonResult
            {
                Data = locmaterial,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
        [HttpGet]
        public JsonResult GetProductDataByPrdId(int productid,string advnote)
        {

            var prd = _bllcustomproductionnote.GetMaterialsByAdvNote(productid,advnote);
            return new JsonResult
            {
                Data = prd,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        // report -----------------------------------------------------------------------------
        [Authorize(Roles = "Reports")]
        public ActionResult Report()
        {
            if (_bllconfiguration.GetConfiguration("UReports", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn)
            {

                if (!_appmanager.CheckReportPermissions(999, Session["loggeduserempcode"].ToString(), "RPTCustomProduction"))
                {
                    @ViewBag.Permissions = "No user permissions to View Custom Productions";
                    return View("~/Views/Account/AccessDenied.cshtml");
                }

            }
            return View("~/Views/Reports/CustomProduction/CustomProductionReport.cshtml");
        }

        [HttpPost]
        [Authorize(Roles = "Reports")]
        public ActionResult Report(CustomProductionNoteHeader header)
        {
            if (_bllconfiguration.GetConfiguration("UReports", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn)
            {

                if (!_appmanager.CheckReportPermissions(999, Session["loggeduserempcode"].ToString(), "RPTCustomProduction"))
                {
                    @ViewBag.Permissions = "No user permissions to View Custom Productions";
                    return View("~/Views/Account/AccessDenied.cshtml");
                }

            }


            header.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var data = _bllcustomproductionnote.CustomProductionReport(header);
            string processlocname;
            string pickuplocname;
            var processloc = _blllocation.GetLocationById(header.ProcessLocId);
            if (processloc == null)
            {
                processlocname = "All";
            }
            else
            {
                processlocname = processloc.LocationName;
            }
            var pickuploc = _blllocation.GetLocationById(header.PickupLocId);         
            if (pickuploc == null)
            {
                pickuplocname = "All";
            }
            else
            {
                pickuplocname = pickuploc.LocationName;
            }

            ViewBag.Selection = "Date From: " + header.DateFrom.ToShortDateString() + " To: " + header.DateTo.ToShortDateString() + ". Process Location:" + processlocname + ". Pickup Location:" + pickuplocname+". ";
            return View("~/Views/Reports/CustomProduction/CustomProductionReport.cshtml",data);
        }

    }
}