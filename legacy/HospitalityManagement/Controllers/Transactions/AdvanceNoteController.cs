using RIT.HMS.BLL.Configurations;
using RIT.HMS.BLL.MasterData;
using RIT.HMS.BLL.TransactionData;
using RIT.HMS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HospitalityManagement.Controllers.Transactions
{
    public class AdvanceNoteController : Controller
    {
        private readonly BLL_AdvanceNote _blladvancenote;
        private readonly BLL_Location _blllocation;
        private readonly BLL_Configuration _bllconfiguration;
        private AppManager _appmanager;
        public AdvanceNoteController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _blladvancenote = new BLL_AdvanceNote(cn);
            _blllocation = new BLL_Location(cn);
            _bllconfiguration = new BLL_Configuration(cn);
            _appmanager = new AppManager(cn);
        }

        [Authorize(Roles = "Reports")]
        public ActionResult AdvanceNote()
        {
            if (_bllconfiguration.GetConfiguration("UReports", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn)
            {

                if (!_appmanager.CheckReportPermissions(999, Session["loggeduserempcode"].ToString(), "RPTAdvanceNote"))
                {
                    @ViewBag.Permissions = "No user permissions to View Advance Note";
                    return View("~/Views/Account/AccessDenied.cshtml");
                }

            }
            return View("~/Views/Reports/AdvanceNote/AdvanceNoteReport.cshtml");
        }

        [HttpPost]
        [Authorize(Roles = "Reports")]
        public ActionResult AdvanceNote(InvAdvanceNoteHed header)
        {

            if (_bllconfiguration.GetConfiguration("UReports", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn)
            {

                if (!_appmanager.CheckReportPermissions(999, Session["loggeduserempcode"].ToString(), "RPTAdvanceNote"))
                {
                    @ViewBag.Permissions = "No user permissions to View Advance Note";
                    return View("~/Views/Account/AccessDenied.cshtml");
                }

            }
            header.CompanyId = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var data = _blladvancenote.AdvanceNoteReport(header);
            string processlocname;
            string pickuplocname;
            string reqlocname;
            var processloc = _blllocation.GetLocationById(header.ProcessLoc);
            if (processloc == null)
            {
                processlocname = "All";
            }
            else
            {
                processlocname = processloc.LocationName;
            }
            var pickuploc = _blllocation.GetLocationById(header.PickupLoc);
            if (pickuploc == null)
            {
                pickuplocname = "All";
            }
            else
            {
                pickuplocname = pickuploc.LocationName;
            }
            var reqloc = _blllocation.GetLocationById(header.LocationID);
            if (reqloc == null)
            {
                reqlocname = "All";
            }
            else
            {
                reqlocname = reqloc.LocationName;
            }

            ViewBag.Selection = "Date From: " + header.DateFrom.ToShortDateString() + " To: " + header.DateTo.ToShortDateString() + ". Process Location:" + processlocname + ". Pickup Location:" + pickuplocname + " Request Location:" + reqlocname + ". ";
            return View("~/Views/Reports/AdvanceNote/AdvanceNoteReport.cshtml",data);
        }
    }
}