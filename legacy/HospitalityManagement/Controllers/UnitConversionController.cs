using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
//using HospitalityManagement.Models;
using Newtonsoft.Json;
using RIT.HMS.BLL.MasterData;
using RIT.HMS.Domain;

namespace HospitalityManagement.Controllers
{
    [SessionTimeout]
    [Authorize(Roles = "PrdEdit")]
    public class UnitConversionController : Controller
    {
       private readonly BLL_UnitOfMeasure _unitOfMeasureService;
       private readonly BLL_UnitConversion _unitConversionService;

        public UnitConversionController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _unitOfMeasureService = new BLL_UnitOfMeasure(cn);
            _unitConversionService = new BLL_UnitConversion(cn);
       }



    public ActionResult Index()
        {
            return View("UnitConversion");
        } 

        [HttpPost]
        public ActionResult SaveConversions(List<UnitConversion> unitConversion)
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            int locid = Convert.ToInt32(Session["loggeduserlocId"]);
            int res = _unitConversionService.SaveUnitConversions(unitConversion,Session["loggeduser"].ToString(),companyid, locid);
           return Json(unitConversion.FirstOrDefault().UnitOfMeasureId);
        }


        [HttpGet]
        public JsonResult GetUnitOfMeasures()
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var unitsofmeasures = _unitOfMeasureService.GetActiveUnitOfMeasures(companyid);
            return Json(JsonConvert.SerializeObject(unitsofmeasures, Formatting.None, new JsonSerializerSettings
                { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetUnitOfConversionsById(long id)
        {

            var conversions = _unitConversionService.GetConversionByMeasurementId(id, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            return Json(JsonConvert.SerializeObject(conversions, Formatting.None, new JsonSerializerSettings
                { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult CheckConversion(long id)
        {

            var conversions = _unitConversionService.GetProductsByConversionId(id,Session["loggeduser"].ToString());
            return new JsonResult { Data = conversions, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
    }
}