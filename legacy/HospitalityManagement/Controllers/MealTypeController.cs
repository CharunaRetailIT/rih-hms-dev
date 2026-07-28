//using HospitalityManagement.Models;
//using HospitalityManagement.Service;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RIT.HMS.BLL.MasterData;
using RIT.HMS.Domain;

namespace HospitalityManagement.Controllers
{
    [SessionTimeout]
    public class MealTypeController : Controller
    {
        BLL_MealType _bllMealType;

        public MealTypeController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _bllMealType = new BLL_MealType(cn);
        }

        [Authorize(Roles = "Others")]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Clear()
        {

            return View("Edit");
        }

        [Authorize(Roles = "Others")]
        public ActionResult Edit(long id)
        {
           
            //MealTypeService mealtypereporsitory = new MealTypeService();
            var mealtype = _bllMealType.GetMealTypeById(id);
            ViewBag.RstMealTypeId = mealtype.RstMealTypeId;
            return View(mealtype);
        }

        [Authorize(Roles = "Others")]
        [HttpPost]
        public ActionResult Edit(RstMealType mealtypes)
        {
           
            var mealtype = _bllMealType.GetMealTypeById(mealtypes.RstMealTypeId);
            mealtype.RstMealTypeCode = mealtypes.RstMealTypeCode;
            mealtype.Description = mealtypes.Description;
            mealtype.IsActive = mealtypes.IsActive;
            mealtype.ModifiedDate = DateTime.UtcNow;
            mealtype.ModifiedUser = Session["loggeduser"].ToString();
            mealtype.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());

            if (_bllMealType.UpdateMealType(mealtype) == 1)
            {

                ViewBag.Message = "1";
            }
            else
            {
                ViewBag.Message = "0";
            }

            return View();
        }

        [Authorize(Roles = "Others")]
        public ActionResult ViewMealTypes()
        {

           
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var mealtype = _bllMealType.GetMeals(companyid).OrderBy(c => c.RstMealTypeCode);

            mealtype.ToList().ForEach(c =>
            {

                //    c.UserName = userreporsitory.GetSysUserMasterID(c.SysUserMasterID).GroupOfCompanyName;
            });

            return View(mealtype);
        }


        [Authorize(Roles = "Others")]
        [HttpPost]
        public ActionResult Create(RstMealType mealtype)
        {
            mealtype.CreatedUser = Session["loggeduser"].ToString();
            mealtype.CreatedDate = DateTime.UtcNow;
            mealtype.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
            mealtype.IsActive = true;
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            mealtype.CompanyID =companyid;

            if (!ModelState.IsValid)
            {
                return View("Index", mealtype);
            }

         
            
            var existsmealtype = _bllMealType.GetMealTypeByCode(mealtype.RstMealTypeCode,companyid);
            if (existsmealtype != null)
            {
                ViewBag.Message = "3";
                return View("Index", mealtype);
            }
            if (_bllMealType.SaveMealType(mealtype) == 1)
            {
                @ViewBag.Message = "1";
            }
            else
            {
                @ViewBag.Message = "2";
            }

            ViewBag.MealTypeCode = mealtype.RstMealTypeCode;
            return View("Index", mealtype);

        }


        [HttpGet]
        public JsonResult GetActiveMealTypes()
        {
           
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var mealtypes = _bllMealType.GetMeals(companyid);
            return Json(JsonConvert.SerializeObject(mealtypes, Formatting.None, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }
	}
}