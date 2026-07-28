using HospitalityManagement.Models;
using Newtonsoft.Json;
using RIT.HMS.BLL.MasterData;
using RIT.HMS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HospitalityManagement.Controllers
{
    public class BudgetItemWiseController :  Controller
    {
        BLL_Location _blllocation;
        BLL_BudgetItemWise _bllBudgetItemWise;
        BLL_BudgetOutlet _bllBudgetOutlet;
        public BudgetItemWiseController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _blllocation = new BLL_Location(cn);
            _bllBudgetItemWise = new BLL_BudgetItemWise(cn);
            _bllBudgetOutlet = new BLL_BudgetOutlet(cn);
        }
        public ActionResult Index()
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            return View();
        }
        public ActionResult Details(int id)
        {
            return View();
        }       
        public ActionResult Create()
        {
            return View(new Models.BudgetItemWise());
        }
        public ActionResult Edit()
        {
            return View(new Models.BudgetItemWise());
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
        [HttpPost]
        public ActionResult Create(RIT.HMS.Domain.BudgetItemWise invBudgetItemWise)
        {
            try
            {
                invBudgetItemWise.ModifiedDate = DateTime.Now;
                invBudgetItemWise.ModifiedUser = Session["loggeduser"].ToString();
                invBudgetItemWise.CreatedDate = DateTime.Now;
                invBudgetItemWise.CreatedUser = Session["loggeduser"].ToString();                
                var res = _bllBudgetItemWise.SaveBudgetItemWise(invBudgetItemWise);
                if ( res != 0)
                {
                    ViewBag.Message = "3";
                    return View();
                }
                else
                {//fail
                    ViewBag.Message = "0";
                    return View(invBudgetItemWise);
                }
                return View();
            }
            catch (Exception ex)
            {
                return View();
            }
        }
    }
}

