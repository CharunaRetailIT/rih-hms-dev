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
    public class RoomTypeRateController : Controller
    {

        BLL_RoomTypeRate _bllroomTypeRate;
        public RoomTypeRateController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _bllroomTypeRate = new BLL_RoomTypeRate(cn);

        }


        [Authorize(Roles = "GOCView")]
        public ActionResult Index()
        {
            return View();
        }



        public ActionResult Clear()
        {

            return View("Edit");
        }

        [Authorize(Roles = "GOCView")]
        public ActionResult Edit(long id)
        {
           
            var roomtyperate = _bllroomTypeRate.GetRoomTypeRateById(id);
            ViewBag.RstRoomTypeRateID = roomtyperate.RstRoomTypeRateID;
            return View(roomtyperate);
        }

        [Authorize(Roles = "GOCView")]
        [HttpPost]
        public ActionResult Edit(RstRoomTypeRate rstroomtyperate)
        {
           
            var roomtyperate = _bllroomTypeRate.GetRoomTypeRateById(rstroomtyperate.RstRoomTypeRateID);
            roomtyperate.RoomTypeRateCode = rstroomtyperate.RoomTypeRateCode;
            roomtyperate.RoomTypeRateName = rstroomtyperate.RoomTypeRateName;
            roomtyperate.Rate = rstroomtyperate.Rate;
            roomtyperate.FromDate = rstroomtyperate.FromDate;
            roomtyperate.ToDate = rstroomtyperate.ToDate;
            roomtyperate.ExtraAdultRate = rstroomtyperate.ExtraAdultRate;
            roomtyperate.ExtraChildRate = rstroomtyperate.ExtraChildRate;
            roomtyperate.ForeignRate = rstroomtyperate.ForeignRate;
            roomtyperate.Package = rstroomtyperate.Package;
            roomtyperate.IsActive = rstroomtyperate.IsActive;
            roomtyperate.IsDelete = rstroomtyperate.IsDelete;
            roomtyperate.ModifiedDate = DateTime.UtcNow;
            roomtyperate.ModifiedUser = Session["loggeduser"].ToString();

            if (_bllroomTypeRate.UpdateRoomTypeRate(roomtyperate) == 1)
            {

                ViewBag.Message = "1";
            }
            else
            {
                ViewBag.Message = "0";
            }

            return View(roomtyperate);
        }


        public ActionResult ViewRoomTypeRates()
        {

          
            var roomtyperates = _bllroomTypeRate.GetRoomTypeRates().OrderBy(c => c.RoomTypeRateCode);

            roomtyperates.ToList().ForEach(c =>
            {

                //    c.UserName = userreporsitory.GetSysUserMasterID(c.SysUserMasterID).GroupOfCompanyName;
            });

            return View(roomtyperates);
        }


        [Authorize(Roles = "GOCView")]
        [HttpPost]
        public ActionResult Create(RstRoomTypeRate rstroomtyperate)
        {
            rstroomtyperate.CreatedUser = Session["loggeduser"].ToString();
            rstroomtyperate.CreatedDate = DateTime.UtcNow;
            rstroomtyperate.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());

            if (!ModelState.IsValid)
            {
                return View("Index", rstroomtyperate);
            }

          
            var existroomtyperate = _bllroomTypeRate.GetRoomTypeRateByCode(rstroomtyperate.RoomTypeRateCode);
            if (existroomtyperate != null)
            {
                ViewBag.Message = "3";
                return View("Index", rstroomtyperate);
            }
            if (_bllroomTypeRate.SaveRoomTypeRate(rstroomtyperate) == 1)
            {
                @ViewBag.Message = "1";
                //rstroomtyperate = null;
            }
            else
            {
                @ViewBag.Message = "2";
            }

            ViewBag.RoomTypeRateCode = rstroomtyperate.RoomTypeRateCode;
            return View("Index", rstroomtyperate);
        }


        [HttpGet]
        public JsonResult GetActiveRoomTypeRates()
        {
          
            var roomtyperate = _bllroomTypeRate.GetRoomTypeRates();
            return Json(JsonConvert.SerializeObject(roomtyperate, Formatting.None, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }







	}
}