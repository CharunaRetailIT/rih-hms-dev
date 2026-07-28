using RIT.HMS.BLL.MasterData;
using RIT.HMS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HospitalityManagement.Controllers
{
    public class kitchenController : Controller
    {

        private BLL_Company _bllCompany;
        private BLL_GroupOfCompany _bllgroupofcompany;
        private BLL_Location _blllocation;
        private BLL_LocationType _blllocationType;
        private BLL_LocationMapper _bLL_LocationMapper;
        private BLL_Product _bll_productMapper;

        // GET: kitchen

        public kitchenController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _bllCompany = new BLL_Company(cn);
            _bllgroupofcompany = new BLL_GroupOfCompany(cn);
            _blllocation = new BLL_Location(cn);
            _blllocationType = new BLL_LocationType(cn);
            _bLL_LocationMapper = new BLL_LocationMapper(cn);
            _bll_productMapper = new BLL_Product(cn);
        }

        public ActionResult Create()
        {
            //int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            //var loc = _blllocation.GetActiveLocations(companyid);
            var Kitchen = new KitchenMaster();
            return View("Create", Kitchen);
        }

        [HttpPost]
        public ActionResult Create(KitchenMaster sysloc)
        {


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
            if (string.IsNullOrEmpty( sysloc.KitchenDesc))
            {
                ModelState.AddModelError("KitchenDesc", "Enter A Kitchen Name");
                return View(sysloc);
            }
            if (sysloc.KitchenPrinterType == 0)
            {
                ModelState.AddModelError("KitchenPrinterType", "Select A  Printer Type!");
                return View(sysloc);
            }
            if (string.IsNullOrEmpty(sysloc.KitchenPrinterName))
            {
                ModelState.AddModelError("KitchenPrinterName", "Enter A Kitchen Printer Name!");
                return View(sysloc);
            }
            if (sysloc.LocationId == 0)
            {
                ModelState.AddModelError("LocationId", "Select A  Location !");
                return View(sysloc);
            }
            



            sysloc.CreatedDate = DateTime.Now;
            sysloc.DataTransfer = 0;
            sysloc.CreatedUser = Session["loggeduser"].ToString();
            //Added by pavithra on 2019/11/30
            sysloc.ModifiedDate = DateTime.Now;
            sysloc.ModifiedUser = Session["loggeduser"].ToString();
            sysloc.IsActive = true;

            var existsloc = _blllocation.GetKitchenByCode(sysloc.KitchenCode, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            if (existsloc != null)
            {
                ViewBag.Message = "3";
                return View("Create", sysloc);
            }

            if (_blllocation.SaveKitchen(sysloc) == 1)
            {
                ViewBag.Message = "1";
            }
            else
            {
                ViewBag.Message = "2";
            }

            ViewBag.LocCode = sysloc.KitchenCode;

            return View("Create", sysloc);
        }

        public ActionResult ViewKitchens()
        {
        
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var kitch = _blllocation.GetKitchens(companyid);
            

            kitch.ToList().ForEach(c =>
            {              
                c.CompanyName = _bllCompany.GetCompanyById(c.CompanyID).CompanyName;
                c.LocationName = _blllocation.GetLocationById(c.LocationId).LocationName;
                c.PrinterTypeDesc = _bll_productMapper.GetPrinterTypebyId(c.KitchenPrinterType).PrinterTypeName;
            });

            return View(kitch);
        }

        
        public ActionResult Edit(long id)
        {
            //  LocationService locrepository = new LocationService();
            var loc = _blllocation.GetKitchenById(id);
            ViewBag.GroupOfCompanyID = loc.GroupOfCompanyID;
            ViewBag.CompanyID = loc.CompanyID;

            //if (_blllocation.GetStockMasterByLocId(id).Count() == 0)
            //{
            //    ViewBag.IsInherit = "inherit";
            //}

         
            return View(loc);
        }

        [HttpPost]
        public ActionResult Edit(KitchenMaster sysloc)
        {
            

            if (!ModelState.IsValid)
            {
                
                return View(sysloc);
            }

         
           // var existsloc = _blllocation.GetKitchenById(sysloc.KitchenID);

            var existsloc = _blllocation.GetKitchenByCode(sysloc.KitchenCode,sysloc.GroupOfCompanyID);




            existsloc.KitchenPrinterName = sysloc.KitchenPrinterName;
            existsloc.KitchenDesc = sysloc.KitchenDesc;
            existsloc.IsActive = sysloc.IsActive;
            existsloc.ModifiedDate = sysloc.ModifiedDate;

            if (_blllocation.UpdateKitchen(existsloc) == 1)
            {
                ViewBag.Message = "1";
            }
            else
            {
                ViewBag.GroupOfCompanyID = sysloc.GroupOfCompanyID;
                ViewBag.CompanyID = sysloc.CompanyID;
                ViewBag.Message = "0";
            }
            ViewBag.LocCode = sysloc.KitchenCode;
            return View(sysloc);
        }


    }
}