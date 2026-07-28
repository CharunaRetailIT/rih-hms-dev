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
    public class RoomTypeController : Controller
    {
        BLL_RoomType _bllroomType;
        public RoomTypeController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _bllroomType = new BLL_RoomType(cn);
        }

        //
        // GET: /RoomType/
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
            //RoomTypeService roomtypereporsitory = new RoomTypeService();
           
            var roomtype = _bllroomType.GetRoomTypeById(id);
            ViewBag.RstRoomTypeID = roomtype.RstRoomTypeID;
            return View(roomtype);
        }

        [Authorize(Roles = "GOCView")]
        [HttpPost]
        public ActionResult Edit(RstRoomType rstroomtype)
        {
           
            var roomtype = _bllroomType.GetRoomTypeById(rstroomtype.RstRoomTypeID);
            roomtype.RoomTypeCode = rstroomtype.RoomTypeCode;
            roomtype.RoomTypeName = rstroomtype.RoomTypeName;
            roomtype.BedType = rstroomtype.BedType;
            roomtype.MaxAdult = rstroomtype.MaxAdult;
            roomtype.MaxChild = rstroomtype.MaxChild;
            roomtype.MaxInfant = rstroomtype.MaxInfant;
            roomtype.IsAC = rstroomtype.IsAC;
            roomtype.IsSmoking = rstroomtype.IsSmoking;
            roomtype.IsMiniBar = rstroomtype.IsMiniBar;
            roomtype.IsNormalView = rstroomtype.IsNormalView;
            roomtype.IsOceanView = rstroomtype.IsOceanView;
            roomtype.IsLandside = rstroomtype.IsLandside;
            roomtype.IsBalcony = rstroomtype.IsBalcony;
            roomtype.IsActive = rstroomtype.IsActive;
            roomtype.IsDelete = rstroomtype.IsDelete;
            roomtype.ModifiedDate = DateTime.UtcNow;
            roomtype.ModifiedUser = Session["loggeduser"].ToString();

            if (_bllroomType.UpdateRoomType(roomtype) == 1)
            {

                ViewBag.Message = "1";
            }
            else
            {
                ViewBag.Message = "0";
            }

            return View();
        }


        public ActionResult ViewRoomTypes()
        {

           
            //GroupOfCompanyService gocreporsitory = new GroupOfCompanyService();
            var roomtypes = _bllroomType.GetRoomTypes().OrderBy(c => c.RoomTypeCode);

            roomtypes.ToList().ForEach(c =>
            {

                //    c.UserName = userreporsitory.GetSysUserMasterID(c.SysUserMasterID).GroupOfCompanyName;
            });

            return View(roomtypes);
        }


        [Authorize(Roles = "GOCView")]
        [HttpPost]
        public ActionResult Create(RstRoomType rstroomtype)
        {
            rstroomtype.CreatedUser = Session["loggeduser"].ToString();
            rstroomtype.CreatedDate = DateTime.UtcNow;
            rstroomtype.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
            rstroomtype.IsActive = true;

            if (!ModelState.IsValid)
            {
                return View("Index",rstroomtype);
            }

           
            var existroomtype = _bllroomType.GetRoomTypeByCode(rstroomtype.RoomTypeCode);
            if (existroomtype != null)
            {
                ViewBag.Message = "3";
                return View("Index",rstroomtype);
            }
            if (_bllroomType.SaveRoomType(rstroomtype) == 1)
            {
                @ViewBag.Message = "1";
                //rstroomtype = null;
            }
            else
            {
                @ViewBag.Message = "2";
            }

            ViewBag.RoomTypeCode = rstroomtype.RoomTypeCode;
            return View("Index",rstroomtype);
        }



        [HttpGet]
        public JsonResult GetActiveRoomTypes()
        {
           
            var roomtypes = _bllroomType.GetRoomTypes().ToList();
            return Json(JsonConvert.SerializeObject(roomtypes, Formatting.None, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }














	}
}