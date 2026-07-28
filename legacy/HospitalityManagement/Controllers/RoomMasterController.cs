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
    public class RoomMasterController : Controller
    {
        BLL_RoomMaster _bllroomMaster;
        public RoomMasterController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _bllroomMaster = new BLL_RoomMaster(cn);
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
            //RoomMasterService roommasterreporsitory = new RoomMasterService();
           
            var roommaster = _bllroomMaster.GetRoomById(id);
            ViewBag.RstRoomMasterID = roommaster.RstRoomMasterID;
            ViewBag.RoomTypeID = roommaster.RoomType;
            
            return View(roommaster);
        }

        [Authorize(Roles = "GOCView")]
        [HttpPost]
        public ActionResult Edit(RstRoomMaster rstroommaster)
        {
          
            var roommaster = _bllroomMaster.GetRoomById(rstroommaster.RstRoomMasterID);
            roommaster.RoomMasterCode = rstroommaster.RoomMasterCode;
            roommaster.RoomName = rstroommaster.RoomName;
            roommaster.RoomType = rstroommaster.RoomType;
            roommaster.Floor = rstroommaster.Floor;
            roommaster.RoomNo = rstroommaster.RoomNo;
            roommaster.InterComNo = rstroommaster.InterComNo;
            roommaster.RFIDNo = rstroommaster.RFIDNo;
            roommaster.IsActive = rstroommaster.IsActive;
            roommaster.IsDelete = rstroommaster.IsDelete;
            roommaster.ModifiedDate = DateTime.UtcNow;
            roommaster.ModifiedUser = Session["loggeduser"].ToString();


            if (_bllroomMaster.UpdateRoom(roommaster) == 1)
            {

                ViewBag.Message = "1";
            }
            else
            {
                ViewBag.Message = "0";
            }

            return View();
        }


        public ActionResult ViewRooms()
        {

         
            //GroupOfCompanyService gocreporsitory = new GroupOfCompanyService();
            var rooms = _bllroomMaster.GetRooms().OrderBy(c => c.RoomMasterCode);

            rooms.ToList().ForEach(c =>
            {

                //    c.UserName = userreporsitory.GetSysUserMasterID(c.SysUserMasterID).GroupOfCompanyName;
            });

            return View(rooms);
        }


        [Authorize(Roles = "GOCView")]
        [HttpPost]
        public ActionResult Create(RstRoomMaster rstroommaster)
        {

            if (!ModelState.IsValid)
            {
                return View("Index", rstroommaster);
            }

         
            rstroommaster.CreatedUser = Session["loggeduser"].ToString();
            rstroommaster.CreatedDate = DateTime.UtcNow;
            rstroommaster.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
            rstroommaster.IsActive = true;

            var existsroom = _bllroomMaster.GetRoomByCode(rstroommaster.RoomMasterCode);
            if (existsroom != null)
            {
                ViewBag.Message = "3";
                return View("Index",rstroommaster);
            }
            if (_bllroomMaster.SaveRoom(rstroommaster) == 1)
            {
                @ViewBag.Message = "1";
                //rstroommaster = null;
            }
            else
            {
                @ViewBag.Message = "2";
            }

            ViewBag.RoomCode = rstroommaster.RoomMasterCode;
            return View("Index",rstroommaster);
        }


        [HttpGet]
        public JsonResult GetActiveRooms()
        {
          
            var rooms = _bllroomMaster.GetRooms();
            return Json(JsonConvert.SerializeObject(rooms, Formatting.None, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

	}
}