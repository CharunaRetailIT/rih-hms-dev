using Newtonsoft.Json;
using RIT.HMS.BLL.MasterData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HospitalityManagement.Controllers
{
    public class CateringMoodController : Controller
    {
        BLL_CateringMood _cartreringmoods;
        BLL_Category _bllcategory;

        public CateringMoodController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _cartreringmoods = new BLL_CateringMood(cn);
            _bllcategory = new BLL_Category(cn);

        }


       
        public ActionResult Index()
        {
            return View();
        }


        [HttpGet]
        public JsonResult GetActiveCateringMoods()
        {
                  
            var categories = _cartreringmoods.GetByCateringMoods(Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            return Json(JsonConvert.SerializeObject(categories, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);

        }

    }
}