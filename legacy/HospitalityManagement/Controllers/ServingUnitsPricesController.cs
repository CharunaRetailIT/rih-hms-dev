using Newtonsoft.Json;
using RIT.HMS.BLL.MasterData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RIT.HMS.Domain;
using RIT.HMS.Domain.ViewModels;
using RIT.HMS.BLL.Configurations;

namespace HospitalityManagement.Controllers
{
    [SessionTimeout]
  
    public class ServingUnitsPricesController : Controller
    {
        BLL_Product _bllProduct;
        BLL_ServingUnit _bllServingUnits;
        BLL_Location _location;
        BLL_Configuration _bllconfiguration;

        public ServingUnitsPricesController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _bllProduct = new BLL_Product(cn);
            _bllServingUnits = new BLL_ServingUnit(cn);
            _location = new BLL_Location(cn);
            _bllconfiguration = new BLL_Configuration(cn);
        }


        
        [Authorize(Roles = "PrdEdit")]
        public ActionResult ServingUnitsPrices()
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            ViewBag.productdata = _bllProduct.GetFinishGoods(companyid).ToList();
            return View("~/Views/ServingUnit/ServingUnitsPrices.cshtml");
        }
        [Authorize(Roles = "PrdEdit")]
        [HttpGet]
        public JsonResult GetServingUnitsByPrdId(long id, string unit)
        {
            var conversions = _bllServingUnits.GetServingUnitsByPrdId(id, unit, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            return new JsonResult { Data = conversions, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

        }
        [Authorize(Roles = "PrdEdit")]
        [HttpGet]
        public JsonResult GetSellingAndCostPriceByServUnitPrdId(long id, string unit)
        {
            var conversions = _bllServingUnits.GetCostSellingPriceByServingUnitsPrductId(id, unit, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            // return new JsonResult { Data = conversions, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            return Json(JsonConvert.SerializeObject(conversions, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);

        }
        //by Aruna
        [Authorize(Roles = "PrdEdit")]
        [HttpGet]
        public JsonResult GetServingUnitsByPrductId(long id)
        {
            var conversions = _bllServingUnits.GetServingUnitsByPrductId(id, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            //return new JsonResult { Data = conversions, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            return Json(JsonConvert.SerializeObject(conversions, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }
        
        [Authorize(Roles = "PrdEdit")]
        public ActionResult GetServingUnits()
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var servingunits = _bllServingUnits.GetAllServingUnits(companyid);

            return Json(JsonConvert.SerializeObject(servingunits, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }
        [Authorize(Roles = "PrdEdit")]
        [HttpPost]
        //public ActionResult UpdatePrices(ServingUnitPricesViewModel prices)
        public ActionResult UpdatePrices(ProductServingUnit prices)
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());

            bool updateCostPriceForAllLocations = _bllconfiguration.GetConfiguration("UpdateServingUnitCostPricesForAllLocations", companyid).ConfigurationOn;

            ViewBag.productdata = _bllProduct.GetFinishGoods(companyid).ToList();
            prices.CreatedDate = DateTime.Now;
            prices.ModifiedDate = DateTime.Now;
            prices.ModifiedUser = Session["loggeduser"].ToString();
            prices.CreatedUser = Session["loggeduser"].ToString();
            if(updateCostPriceForAllLocations)
            {
                prices.LocationId = 0;
            }
            else
            {
                prices.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
            }
            
            //var res = _bllServingUnits.UpdateProductServingUnits(prices);
            var res = _bllServingUnits.UpdateProductServingUnit(prices);// By Aruna
            if (res)
            {
                @ViewBag.Message = "1";
            }
            else
            {
                @ViewBag.Message = "2";
            }
            return View("~/Views/ServingUnit/ServingUnitsPrices.cshtml",new ServingUnitPricesViewModel());
        }
    }
}