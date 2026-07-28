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
   
    public class ChairMasterController : Controller
    {
        BLL_Chair _bllchair;
        public ChairMasterController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _bllchair = new BLL_Chair(cn);
        }

        [Authorize(Roles = "TablesAndChairs")]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Clear()
        {

            return View("Edit");
        }

        [Authorize(Roles = "TablesAndChairs")]
        public ActionResult Edit(long id)
        {
            // ChairService chairmasterreporsitory = new ChairService();
          
            var chairmaster = _bllchair.GetChairById(id);
            ViewBag.ChairMasterID = chairmaster.ChairMasterID;
            ViewBag.TableID = chairmaster.TableID;
            return View(chairmaster);
        }

        [Authorize(Roles = "TablesAndChairs")]
        [HttpPost]
        public ActionResult Edit(ChairMaster chairmasters)
        {
           
            var chairmaster = _bllchair.GetChairById(chairmasters.ChairMasterID);
            chairmaster.ChairCode = chairmasters.ChairCode;
            chairmaster.TableID = chairmasters.TableID;
            chairmaster.ChairName = chairmasters.ChairName;
            chairmaster.TicketID = chairmasters.TicketID;
            chairmaster.IsDelete = chairmasters.IsDelete;


            if (_bllchair.UpdateChair(chairmaster) == 1)
            {

                ViewBag.Message = "1";
            }
            else
            {
                ViewBag.Message = "0";
            }
            ViewBag.TableID = chairmaster.TableID;
            return View();
        }

        [Authorize(Roles = "TablesAndChairs")]
        public ActionResult ViewChairs()
        {

          
            //GroupOfCompanyService gocreporsitory = new GroupOfCompanyService();
            var chairs = _bllchair.GetActiveChairs().OrderBy(c => c.ChairCode);

            chairs.ToList().ForEach(c =>
            {

                //    c.UserName = userreporsitory.GetSysUserMasterID(c.SysUserMasterID).GroupOfCompanyName;
            });

            return View(chairs);
        }


        [Authorize(Roles = "TablesAndChairs")]
        [HttpPost]
        public ActionResult Create(ChairMaster chairmasters)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", chairmasters);
            }

          
            var existschair = _bllchair.GetChairByCode(chairmasters.ChairCode);
            chairmasters.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
            if (existschair != null)
            {
                ViewBag.Message = "3";
                return View("Index", chairmasters);
            }
            if (_bllchair.SaveChair(chairmasters) == 1)
            {
                @ViewBag.Message = "1";
                // chairmasters = null;
            }

            else
            {
                @ViewBag.Message = "2";
            }
            
            

            ViewBag.ChairCode = chairmasters.ChairCode;
            return View("Index", chairmasters);

        }


        [HttpGet]
        public JsonResult GetActiveChairs()
        {
            
            var chairs = _bllchair.GetActiveChairs();
            return Json(JsonConvert.SerializeObject(chairs, Formatting.None, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }





	}
}