
using RIT.HMS.BLL.Common;
using RIT.HMS.BLL.MasterData;
using RIT.HMS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HospitalityManagement.Controllers
{
    public class StewardsMasterController : Controller
    {
        private readonly BLL_Common _bllcommon;
        BLL_Location _location;
        BLL_StewardsMaster _stewardsMaster;

        public StewardsMasterController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _bllcommon = new BLL_Common(cn);
            _stewardsMaster = new BLL_StewardsMaster(cn);
            _location = new BLL_Location(cn);
        }

        public ActionResult ViewStewards()
        {

            int compayid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var exists = _stewardsMaster.GetStewards(compayid);
            return View(exists);
        }

        public ActionResult Create()
        {
            return View(new StewardsMaster());
        }

        [HttpPost]
        public ActionResult Create(StewardsMaster stewardsMaster)
        {
            stewardsMaster.CreatedUser = Session["loggeduser"].ToString();
            stewardsMaster.CreatedDate = DateTime.Now;
            stewardsMaster.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
            stewardsMaster.DataTransfer = 1;
            stewardsMaster.ModifiedUser = Session["loggeduser"].ToString();
            stewardsMaster.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var existscust = _stewardsMaster.GetStewardsByCode(stewardsMaster.StewardCode, companyid);
            ViewBag.StewardCode = stewardsMaster.StewardCode;

            if (existscust != null)
            {
                ViewBag.Message = "3";
                return View(stewardsMaster);
            }

            if (_stewardsMaster.SaveStewards(stewardsMaster) != 0)
            {
                ViewBag.Message = "1";
                ModelState.Clear();
                return View(new StewardsMaster());
            }
            else
            {
                @ViewBag.StewardsCompanyID = stewardsMaster.CompanyID;
                ViewBag.Message = "2";
                return View(stewardsMaster);
            }
        }

        [HttpPost]
        public ActionResult Edit(StewardsMaster stewardsMaster)
        {
            var exists = _stewardsMaster.GetStewardsById(stewardsMaster.StewardsMasterID);
            exists.StewardName = stewardsMaster.StewardName;
            exists.Address1 = stewardsMaster.Address1;
            exists.Address2 = stewardsMaster.Address2;
            exists.Address3 = stewardsMaster.Address3;
            exists.StewardTitle = stewardsMaster.StewardTitle;
            exists.NIC = stewardsMaster.NIC;
            exists.Mobile = stewardsMaster.Mobile;
            exists.Email = stewardsMaster.Email;
            exists.IsActive = stewardsMaster.IsActive;
            exists.IsDeliveryPerson = stewardsMaster.IsDeliveryPerson;
            exists.IsKarokeGirl = stewardsMaster.IsKarokeGirl;
            exists.ModifiedDate = DateTime.Now;
            exists.ModifiedUser = Session["loggeduser"].ToString();
            exists.Target = stewardsMaster.Target;
            exists.Commission = stewardsMaster.Commission;
            exists.IsDelete = stewardsMaster.IsDelete;
            exists.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            if (_stewardsMaster.UpdateStewards(exists) > 0)
            {
                @ViewBag.Message = "1";
            }
            else
            {
                @ViewBag.Message = "2";
            }
            @ViewBag.StewardCode = exists.StewardCode;
            return View(stewardsMaster);
        }

        public ActionResult Edit(long id)
        {
            var exists = _stewardsMaster.GetStewardsById(id);
            @ViewBag.StewardsCompanyID = exists.CompanyID;

            if (exists.StewardTitle == "Mr")
            {
                @ViewBag.sel = "0";
            }
            else if (exists.StewardTitle == "Mrs")
            {
                @ViewBag.sel = "1";
            }
            else if (exists.StewardTitle == "Ms")
            {
                @ViewBag.sel = "2";
            }
            else if (exists.StewardTitle == "Miss")
            {
                @ViewBag.sel = "3";
            }
            else if (exists.StewardTitle == "Dr")
            {
                @ViewBag.sel = "4";
            }
            else if (exists.StewardTitle == "Rev")
            {
                @ViewBag.sel = "5";
            }

            return View(exists);
        }
        // GET: StewardsMaster
        public ActionResult Index()
        {
            return View();
        }
    }
}