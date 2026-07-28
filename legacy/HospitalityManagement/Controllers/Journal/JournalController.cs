using RIT.HMS.BLL.Journal;
using RIT.HMS.BLL.MasterData;
using RIT.HMS.Domain.ViewModels.Journal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HospitalityManagement.Controllers.Journal
{
    public class JournalController : Controller
    {
        private readonly BLL_Journal _blljournal;
        private readonly BLL_Company _bllcompany;
        private readonly BLL_Location _blllocation;
        public JournalController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _blljournal = new BLL_Journal(cn);
            _bllcompany = new BLL_Company(cn);
            _blllocation = new BLL_Location(cn);
        }

        public ActionResult Journal()
        {
            JournalViewModel _journalViewModel = new JournalViewModel();
            _journalViewModel.DateFrom = DateTime.Now;
            _journalViewModel.DateTo = DateTime.Now;
            return View("~/Views/Journal/UploadData.cshtml", _journalViewModel);
        }
        
       public ActionResult Process(JournalViewModel vm)
       {
            @ViewBag.Message = "";
            vm.CompanyId = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            try {

                foreach (int l in vm.Locations)
                {
                    vm.LocationId = l;
                    var values = _blljournal.UploadJournalData(vm);
                }

                ViewBag.locations = "[";
                foreach (var i in vm.Locations)
                {
                    ViewBag.locations += i.ToString() + ",";
                }
                ViewBag.locations += "]";

                @ViewBag.Message = "1";
                @ViewBag.JournalId = 1;
            } catch (Exception e)
            {
                @ViewBag.Message = "2";
            }
            return View("~/Views/Journal/UploadData.cshtml",vm);
       }

        public ActionResult Transfer(JournalViewModel vm)
        {
            @ViewBag.Message = "";
            vm.CompanyId = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            try
            {
                foreach (int l in vm.Locations)
                {
                    vm.LocationId = l;
                    var values = _blljournal.TransferJournalData(vm);
                }
                ViewBag.locations = "[";
                foreach (var i in vm.Locations)
                {
                    ViewBag.locations += i.ToString() + ",";
                }
                ViewBag.locations += "]";

                @ViewBag.Message = "1";
                @ViewBag.JournalId = 1;
                
            }
            catch (Exception e)
            {
                @ViewBag.Message = "2";
            }
            return View("~/Views/Journal/UploadData.cshtml", vm);
        }

        [HttpGet]
        public ActionResult Print(string locations,string date)
        {
            var ld = locations.Length;
            var locs1 = locations.Remove(0,1).Remove(locations.Length-2,1).Split(',');
            List<string> locids = new List<string>();

           
            foreach (var s in locs1)
            {
                if (s != string.Empty && s != null)
                {
                    var loc = _blllocation.GetLocationById(Convert.ToInt32(s));
                    locids.Add(loc.LocationCode);
                   
                }
            }

            JournalViewModel _journalviewmodel = new JournalViewModel();
            _journalviewmodel.DateFrom =Convert.ToDateTime(date);
            _journalviewmodel.SysCompany = _bllcompany.GetCompanyById(Convert.ToInt32(Session["loggedusercompanyId"].ToString()));

            @ViewBag.LogUser = Convert.ToString(Session["loggeduser"]);
            var l= Convert.ToString(Session["loggeduserlocId"]);
            @ViewBag.LogLocation = _blllocation.GetLocationById(Convert.ToInt16(l)).LocationName;
            JournalReport jr = new JournalReport();
            jr.DATE = _journalviewmodel.DateFrom;
            jr.CCODE = _blllocation.GetLocationById(Convert.ToInt32(locs1[0])).LocationCode;
            jr.CompanyId= Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            jr.LocationCodes = locids;
            _journalviewmodel.JournalReport = _blljournal.JournalRecipt(jr);
            return PartialView("~/Views/Receipts/JournalDataUpload.cshtml", _journalviewmodel);

        }
     }
}