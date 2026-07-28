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
    public class BudgetOutletController :  Controller
    {
        BLL_Location _blllocation;
        BLL_BudgetOutlet _bllBudgetOutlet;
        public BudgetOutletController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _blllocation = new BLL_Location(cn);
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
         ///BudgetOutlet/Create

        //[Authorize(Roles = "ViewBudget")]
        public ActionResult ViewBudget()
        {
            var cuscat = _bllBudgetOutlet.GetBudgetOutletWise();
            //return View(cuscat);
            return View("~/Views/BudgetOutlet/ViewBudget.cshtml", cuscat);
        }
        public ActionResult Create()
        {
            return View(new Models.BudgetOutlet());
        }

        [Route("BudgetOutlet/Edit/{budgetOutletId}")]
        public ActionResult Edit(int? BudgetOutletID)
        {
            var cuscat = _bllBudgetOutlet.GetBudgetOutletWiseID(BudgetOutletID ?? 0);
            //return View(new Models.BudgetOutlet());
            return View("~/Views/BudgetOutlet/Edit.cshtml", cuscat);
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
        public ActionResult Create(RIT.HMS.Domain.BudgetOutlet invBudgetOutlet)
        {
            try
            {
                double datecount = 0;
                invBudgetOutlet.ModifiedDate = DateTime.Now;
                invBudgetOutlet.ModifiedUser = Session["loggeduser"].ToString();
                invBudgetOutlet.isActive = true;
                invBudgetOutlet.CreatedDate = DateTime.Now;
                if (invBudgetOutlet.BudgetType == 1) 
                {
                    invBudgetOutlet.EndDate = invBudgetOutlet.StartingDate.AddDays(Convert.ToDouble(invBudgetOutlet.NoofDMWY));
                }
                else if (invBudgetOutlet.BudgetType == 2) 
                {
                    datecount = Convert.ToDouble(invBudgetOutlet.NoofDMWY) * 7;
                    invBudgetOutlet.EndDate = invBudgetOutlet.StartingDate.AddDays(Convert.ToDouble(invBudgetOutlet.NoofDMWY));
                }
                else if (invBudgetOutlet.BudgetType == 3) 
                {
                    invBudgetOutlet.EndDate = invBudgetOutlet.StartingDate.AddMonths(invBudgetOutlet.NoofDMWY);
                }
                else if (invBudgetOutlet.BudgetType == 4) 
                {
                    invBudgetOutlet.EndDate = invBudgetOutlet.StartingDate.AddMonths(invBudgetOutlet.NoofDMWY*3);
                }
                else 
                {
                    invBudgetOutlet.EndDate = invBudgetOutlet.StartingDate.AddYears(invBudgetOutlet.NoofDMWY);
                }
                invBudgetOutlet.CreatedUser = Session["loggeduser"].ToString();                
                var res = _bllBudgetOutlet.SaveBudgetOutlet(invBudgetOutlet);
                if ( res != 0)
                {
                    ViewBag.Message = "3";
                    return View();
                }
                else
                {//fail
                    ViewBag.Message = "0";
                    return View(invBudgetOutlet);
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

