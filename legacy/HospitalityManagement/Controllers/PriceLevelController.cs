using Newtonsoft.Json;
using System;
using System.Linq;
using System.Web.Mvc;
using RIT.HMS.Domain;
using RIT.HMS.BLL.MasterData;
using RIT.HMS.BLL.Common;

namespace HospitalityManagement.Controllers
{
    public class PriceLevelController : Controller
    {
        InvPriceLevel _objInvPriceLevel;

        //
        // GET: /PriceLevel/
        BLL_InvPriceLevel _bllPricelevel;
        public PriceLevelController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _bllPricelevel = new BLL_InvPriceLevel(cn);

        }

        public ActionResult Create()
        {
            //int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            //var loc = _blllocation.GetActiveLocations(companyid);
            var IPriceLevel = new InvPriceLevel();
            return View("Create", IPriceLevel);
        }

        public ActionResult ViewPriceLevel()
        {

            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var pricelevel = _bllPricelevel.GetPriceLevel(companyid);


            pricelevel.ToList().ForEach(c =>
            {
                //c.PriceLevelCode = _bllPricelevel.GetCompanyById(c.CompanyID).CompanyName;
                c.LocationName = _bllPricelevel.GetLocationById(c.LocationId).LocationName;
            });

            return View(pricelevel);
        }
        //public ActionResult Create()
        //{

        //    return View(new RIT.HMS.Domain.InvPriceLevel());
        //}

        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public JsonResult VaidatePriceLevelCode(string PriceLevelCode)
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var dbproductcode = _bllPricelevel.FindByCode(PriceLevelCode, companyid);


            return new JsonResult { Data = dbproductcode, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        //
        // GET: /PriceLevel/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        ////
        //// POST: /PriceLevel/Create
        //[HttpPost]
        //public ActionResult Create(FormCollection collection)
        //{
        //    try
        //    {
        //        // TODO: Add insert logic here

        //        return RedirectToAction("Index");
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}


        //
        // POST: /PriceLevel/Create
        [HttpGet]
        public JsonResult VaidatePriceLevel(string PriceLevelCode)
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var dbproductcode = _bllPricelevel.FindByCode(PriceLevelCode, companyid);

            return new JsonResult { Data = dbproductcode, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        [HttpPost]
        public ActionResult Create(InvPriceLevel PriceLevel)
        {

            // TODO: Add insert logic here
            VaidatePriceLevel(PriceLevel.PriceLevelCode);

            PriceLevel.CreatedUser = Session["loggeduser"].ToString();
            PriceLevel.CreatedDate = DateTime.Now;
            PriceLevel.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
            PriceLevel.DataTransfer = 1;
            PriceLevel.ModifiedUser = Session["loggeduser"].ToString();
            PriceLevel.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            PriceLevel.ServingUnit = "";
            

            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var exists = _bllPricelevel.GetPriceLevel(PriceLevel.InvPriceLevelID);
            if (exists != null)
            {
                ViewBag.Message = "3";
                return View(PriceLevel);
            }

            if (_bllPricelevel.SavePriceLevel(PriceLevel) != 0)
            {
                ViewBag.Message = "1";
                ModelState.Clear();
                return View(new InvPriceLevel());
            }
            else
            {
                return View(PriceLevel);
            } 
        }

        public ActionResult Edit(long id)
        {
            //  LocationService locrepository = new LocationService();
            var loc = _bllPricelevel.GetPriceLevelById(id);
            ViewBag.GroupOfCompanyID = loc.GroupOfCompanyID;
            ViewBag.CompanyID = loc.CompanyID;
 
            return View(loc);
        }

        [HttpPost]
        public ActionResult Edit(InvPriceLevel sysloc)
        {
            if (!ModelState.IsValid)
            {

                return View(sysloc);
            }

            var existsloc = _bllPricelevel.GetPriceLevelByCode(sysloc.PriceLevelCode, sysloc.GroupOfCompanyID);

            existsloc.PriceLevelName = sysloc.PriceLevelName;
            existsloc.Remark = sysloc.Remark;
            existsloc.IsDelete = sysloc.IsDelete;

            if (_bllPricelevel.UpdateInvPriceLevel(existsloc) == 1)
            {
                ViewBag.Message = "1";
            }
            else
            {
                ViewBag.GroupOfCompanyID = sysloc.GroupOfCompanyID;
                ViewBag.CompanyID = sysloc.CompanyID;
                ViewBag.Message = "2";
            }
            ViewBag.LocCode = sysloc.PriceLevelCode;
            return View(sysloc);
        }

            ////
            //// GET: /PriceLevel/Edit/5
            //public ActionResult Edit(int id)
            //{
            //    return View();
            //}

            ////
            //// POST: /PriceLevel/Edit/5
            //[HttpPost]
            //public ActionResult Edit(int id, FormCollection collection)
            //{
            //    try
            //    {
            //        // TODO: Add update logic here

            //        return RedirectToAction("Index");
            //    }
            //    catch
            //    {
            //        return View();
            //    }
            //}

            ////
            //// GET: /PriceLevel/Delete/5
            //public ActionResult Delete(int id)
            //{
            //    return View();
            //}

            ////
            //// POST: /PriceLevel/Delete/5
            //[HttpPost]
            //public ActionResult Delete(int id, FormCollection collection)
            //{
            //    try
            //    {
            //        // TODO: Add delete logic here

            //        return RedirectToAction("Index");
            //    }
            //    catch
            //    {
            //        return View();
            //    }
            //}
        }
}