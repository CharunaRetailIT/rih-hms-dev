//using HospitalityManagement.Models;
//using HospitalityManagement.Service;
using Newtonsoft.Json;
using RIT.HMS.BLL.MasterData;
using RIT.HMS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace HospitalityManagement.Controllers
{
    [SessionTimeout]
    public class LocationController : Controller
    {
        private BLL_Company _bllCompany;
        private BLL_GroupOfCompany _bllgroupofcompany;
        private BLL_Location _blllocation;
        private BLL_LocationType _blllocationType;
        private BLL_LocationMapper _bLL_LocationMapper;

        public LocationController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _bllCompany = new BLL_Company(cn);
            _bllgroupofcompany = new BLL_GroupOfCompany(cn);
            _blllocation = new BLL_Location(cn);
            _blllocationType = new BLL_LocationType(cn);
            _bLL_LocationMapper = new BLL_LocationMapper(cn);
        }

        [Authorize(Roles = "LCreatee")]
        public ActionResult Create()
        {
            var result = new SysLocation();
            result.SysLocationTypeList = new List<SysLocationType>();
            result.SysLocationTypeList = _blllocationType.GetActiveAll();
            return View(result);
        }

        [Authorize(Roles = "LEdit")]
        public ActionResult Edit(long id)
        {
            //  LocationService locrepository = new LocationService();
            var loc = _blllocation.GetLocationById(id);
            ViewBag.GroupOfCompanyID = loc.GroupOfCompanyID;
            ViewBag.CompanyID = loc.CompanyID;

            if (_blllocation.GetStockMasterByLocId(id).Count() == 0)
            {
                ViewBag.IsInherit = "inherit";
            }

            loc.SysLocationTypeList = new List<SysLocationType>();
            loc.SysLocationTypeList = _blllocationType.GetActiveAll();
            return View(loc);
        }

        [Authorize(Roles = "LCreatee")]
        [HttpPost]
        public ActionResult Create(SysLocation sysloc)
        {
            //  LocationService locrepository = new LocationService();
            sysloc.SysLocationTypeList = _blllocationType.GetActiveAll();
            if (!ModelState.IsValid)
            {
                return View(sysloc);
            }

            if (sysloc.GroupOfCompanyID == 0)
            {
                ModelState.AddModelError("GroupOfCompanyID", "Select A Group Of Company!");
                return View(sysloc);
            }
            if (sysloc.CompanyID == 0)
            {
                ModelState.AddModelError("CompanyID", "Select A  Company!");
                return View(sysloc);
            }

            if (sysloc.IsHeadOffice == true && _blllocation.CheckHeadOffice() >= 1)
            {
                // ModelState.AddModelError("IsHeadOffice", "Only One Head Office can be create");
                @ViewBag.Message = "4";
                ViewBag.GroupOfCompanyID = sysloc.GroupOfCompanyID;
                ViewBag.CompanyID = sysloc.CompanyID;
                return View(sysloc);
            }

            sysloc.CreatedDate = DateTime.Now;
            sysloc.DataTransfer = 0;
            sysloc.CreatedUser = Session["loggeduser"].ToString();
            //Added by pavithra on 2019/11/30
            sysloc.ModifiedDate = DateTime.Now;
            sysloc.ModifiedUser = Session["loggeduser"].ToString();
            sysloc.IsActive = true;

            var existsloc = _blllocation.GetLocByCode(sysloc.LocationCode, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            if (existsloc != null)
            {
                ViewBag.Message = "3";
                return View("Create", sysloc);
            }

            if (_blllocation.SaveLocation(sysloc) == 1)
            {
                ViewBag.Message = "1";
            }
            else
            {
                ViewBag.Message = "2";
            }

            ViewBag.LocCode = sysloc.LocationCode;

            return View("Create", sysloc);
        }

        [Authorize(Roles = "LEdit")]
        [HttpPost]
        public ActionResult Edit(SysLocation sysloc)
        {
            sysloc.SysLocationTypeList = _blllocationType.GetActiveAll();

            if (!ModelState.IsValid)
            {
                ViewBag.GroupOfCompanyID = sysloc.GroupOfCompanyID;
                ViewBag.CompanyID = sysloc.CompanyID;
                return View(sysloc);
            }

            //  LocationService locrepository = new LocationService();
            var existsloc = _blllocation.GetLocationById(sysloc.SysLocationID);
            if (sysloc.GroupOfCompanyID == 0)
            {
                ModelState.AddModelError("GroupOfCompanyID", "Select A Group Of Company!");
                ViewBag.GroupOfCompanyID = sysloc.GroupOfCompanyID;
                ViewBag.CompanyID = sysloc.CompanyID;
                return View(sysloc);
            }
            if (sysloc.CompanyID == 0)
            {
                ModelState.AddModelError("CompanyID", "Select A  Company!");
                ViewBag.GroupOfCompanyID = sysloc.GroupOfCompanyID;
                ViewBag.CompanyID = sysloc.CompanyID;
                return View(sysloc);
            }

            if (sysloc.IsHeadOffice == true && _blllocation.CheckHeadOffice() >= 1)
            {
                if (sysloc.SysLocationID != _blllocation.GetHeadOfiice().SysLocationID)
                {
                    @ViewBag.Message = "4";
                    ViewBag.GroupOfCompanyID = sysloc.GroupOfCompanyID;
                    ViewBag.CompanyID = sysloc.CompanyID;
                    return View(sysloc);
                }
            }
            if (sysloc.IsActive == true && sysloc.IsDelete == true)
            {
                @ViewBag.Message = "5";
                ViewBag.GroupOfCompanyID = sysloc.GroupOfCompanyID;
                ViewBag.CompanyID = sysloc.CompanyID;
                sysloc.SysLocationTypeList = new List<SysLocationType>();
                sysloc.SysLocationTypeList = _blllocationType.GetActiveAll();
                return View(sysloc);
            }

            existsloc.InheritProducts = sysloc.InheritProducts;
            existsloc.LocationCode = sysloc.LocationCode;
            existsloc.LocationName = sysloc.LocationName;
            existsloc.Address1 = sysloc.Address1;
            existsloc.Address2 = sysloc.Address2;
            existsloc.Address3 = sysloc.Address3;
            existsloc.Telephone = sysloc.Telephone;
            existsloc.Fax = sysloc.Fax;
            existsloc.Email = sysloc.Email;
            existsloc.GroupOfCompanyID = sysloc.GroupOfCompanyID;
            existsloc.CompanyID = sysloc.CompanyID;
            existsloc.ContactPersonName = sysloc.ContactPersonName;
            existsloc.OtherBusinessName = sysloc.OtherBusinessName;
            existsloc.LocationPrefixCode = sysloc.LocationPrefixCode;
            existsloc.LocationIP = sysloc.LocationIP;
            existsloc.IsDelete = sysloc.IsDelete;
            existsloc.IsVAT = sysloc.IsVAT;
            existsloc.IsActive = sysloc.IsActive;
            existsloc.IsHeadOffice = sysloc.IsHeadOffice;
            existsloc.IsStockLocation = sysloc.IsStockLocation;
            existsloc.ModifiedDate = DateTime.Now;
            existsloc.ModifiedUser = Session["loggeduser"].ToString();
            existsloc.CostCenter = sysloc.CostCenter;
            existsloc.IsShowRoom = sysloc.IsShowRoom;
            existsloc.LocationTypeId = sysloc.LocationTypeId;
            if (_blllocation.UpdateLocation(existsloc) == 1)
            {
                ViewBag.Message = "1";
            }
            else
            {
                ViewBag.GroupOfCompanyID = sysloc.GroupOfCompanyID;
                ViewBag.CompanyID = sysloc.CompanyID;
                ViewBag.Message = "0";
            }
            ViewBag.LocCode = sysloc.LocationCode;
            sysloc.SysLocationTypeList = new List<SysLocationType>();
            sysloc.SysLocationTypeList = _blllocationType.GetActiveAll();
            return View(sysloc);
        }

        [Authorize(Roles = "LView")]
        public ActionResult ViewLocations()
        {
            //  LocationService locrepository = new LocationService();

            // GroupOfCompanyService gocrepository = new GroupOfCompanyService();
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var locs = _blllocation.GetLocations(companyid);

            locs.ToList().ForEach(c =>
            {
                c.GOCName = _bllgroupofcompany.GetGroupOfCompanyById(c.GroupOfCompanyID).GroupOfCompanyName;
                c.CompanyName = _bllCompany.GetCompanyById(c.CompanyID).CompanyName;
            });

            return View(locs);
        }

        [HttpGet]
        public JsonResult GetActiveLocations()
        {
            //  LocationService reporsitory = new LocationService();
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var locations = _blllocation.GetActiveLocations(companyid);
            return Json(JsonConvert.SerializeObject(locations, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetActiveShowRoomLocations()
        {
            //  LocationService reporsitory = new LocationService();
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var locations = _blllocation.GetActiveLocations(companyid).Where(l => l.IsShowRoom == true);
            return Json(JsonConvert.SerializeObject(locations, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetLocationById(int locid)
        {
            var location = _blllocation.GetLocationById(locid);
            return Json(location, JsonRequestBehavior.AllowGet);
            //return Json(JsonConvert.SerializeObject(locations, Formatting.None, new JsonSerializerSettings
            //{ ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        //Added by Pavithra on 2019-11-30
        [HttpGet]
        public JsonResult CheckLocationCode(string code)
        {
            var loc = _blllocation.FindByCode(code);
            return new JsonResult { Data = loc, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public ActionResult AssignKitchens()
        {
            Session["MessageId"] = "0";
            var result = new List<SysLocation>();
            try
            {
                int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
                result = _blllocation.GetActiveGeneralLocations(companyid).ToList();

                result.ToList().ForEach(c =>
                {
                    c.GOCName = _bllgroupofcompany.GetGroupOfCompanyById(c.GroupOfCompanyID).GroupOfCompanyName;
                    c.CompanyName = _bllCompany.GetCompanyById(c.CompanyID).CompanyName;
                    c.KitchenLocationCount = _bLL_LocationMapper.GetAllByLocationId(Convert.ToInt32(Session["loggedusercompanyId"].ToString()), c.SysLocationID).ToList().Count;
                });
            }
            catch (Exception ex)
            {
                Session["MessageId"] = "4";
                Session["Message"] = ex.Message;
            }

            return View(result);
        }

        [Authorize(Roles = "LEdit")]
        public ActionResult AddKitchen(int comId, int locationId)
        {
            var result = new KitchenAddToLocation();

            try
            {
                result.GeneralLocationId = locationId;
                result.GeneralLocationList = _blllocation.GetActiveGeneralLocations(comId).ToList();
                result.GeneralLocation = result.GeneralLocationList.Where(o => o.CompanyID == comId && o.SysLocationID == locationId).FirstOrDefault();
                result.LocationMapper = _bLL_LocationMapper.GetAllByLocationId(comId, locationId).ToList();
                result.KitchenLocationList = _blllocation.GetActiveKitchenLocations(comId).ToList();
                result.KitchenLocationList = _bLL_LocationMapper.MapperdLocationSelect(result);
            }
            catch (Exception ex)
            {
                Session["MessageId"] = "4";
                Session["Message"] = ex.Message;
            }

            return View(result);
        }

        [Authorize(Roles = "LEdit")]
        [HttpPost]
        public ActionResult AddKitchen(int comId, int locationId, KitchenAddToLocation entity)
        {
            try
            {
                entity.CreatedDate = DateTime.Now;
                entity.DataTransfer = 0;
                entity.CreatedUser = Session["loggeduser"].ToString();
                entity.ModifiedDate = DateTime.Now;
                entity.ModifiedUser = Session["loggeduser"].ToString();
                entity.IsActive = true;
                entity.LocationMapper = _bLL_LocationMapper.GetAllByLocationId(entity.GeneralLocation.CompanyID, entity.GeneralLocation.SysLocationID).ToList();
                if (_bLL_LocationMapper.SaveSubLocation(entity) == 1)
                {
                    Session["MessageId"] = "1";
                    return RedirectToAction("AddKitchen", "Location", new { @comId = entity.GeneralLocation.CompanyID, @locationId = entity.GeneralLocation.SysLocationID });
                }
                else
                {
                    Session["MessageId"] = "2";
                }
            }
            catch (Exception ex)
            {
                Session["MessageId"] = "4";
                Session["Message"] = ex.Message;
            }
            return View(entity);
        }
    }
}