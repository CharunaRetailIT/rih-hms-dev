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
    public class VehicleController : Controller
    {
        BLL_Vehicle _bllvehicle = new BLL_Vehicle();
        public VehicleController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _bllvehicle = new BLL_Vehicle(cn);

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
           
            var vehicle = _bllvehicle.GetVehicleById(id);
            ViewBag.VehicleID = vehicle.VehicleID;
            return View(vehicle);
        }

        [Authorize(Roles = "Others")]
        [HttpPost]
        public ActionResult Edit(Vehicle vehicles)
        {
           
            var vehicle = _bllvehicle.GetVehicleById(vehicles.VehicleID);
            vehicle.RegistrationNo = vehicles.RegistrationNo;
            vehicle.VehicleName = vehicles.VehicleName;
            vehicle.EngineNo = vehicles.EngineNo;
            vehicle.ChassesNo = vehicles.ChassesNo;
            vehicle.VehicleType = vehicles.VehicleType;
            vehicle.FuelType = vehicles.FuelType;
            vehicle.Make = vehicles.Make;
            vehicle.Model = vehicles.Model;
            vehicle.EngineCapacity = vehicles.EngineCapacity;
            vehicle.SeatingCapacity = vehicles.SeatingCapacity;
            vehicle.Weight = vehicles.Weight;
            vehicle.Remark = vehicles.Remark;
            vehicle.IsDelete = vehicles.IsDelete;
            vehicle.ModifiedDate = DateTime.UtcNow;
            vehicle.ModifiedUser = Session["loggeduser"].ToString();

            if (_bllvehicle.UpdateVehicle(vehicle) == 1)
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
        public ActionResult ViewVehicles()
        {

           
            var vehicles = _bllvehicle.GetVehicles().OrderBy(c => c.RegistrationNo);

            vehicles.ToList().ForEach(c =>
            {

                //    c.UserName = userreporsitory.GetSysUserMasterID(c.SysUserMasterID).GroupOfCompanyName;
            });

            return View(vehicles);
        }


        [Authorize(Roles = "Others")]
        [HttpPost]
        public ActionResult Create(Vehicle vehicles)
        {
            vehicles.CreatedUser = Session["loggeduser"].ToString();
            vehicles.CreatedDate = DateTime.UtcNow;
            vehicles.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());

            if (!ModelState.IsValid)
            {
                return View("Index", vehicles);
            }

        
            var existsvehicle = _bllvehicle.GetVehicleByRegistrationNo(vehicles.RegistrationNo);
            if (existsvehicle != null)
            {
                ViewBag.Message = "3";
                return View("Index", vehicles);
            }
            if (_bllvehicle.SaveVehicle(vehicles) == 1)
            {
                @ViewBag.Message = "1";
                //vehicles = null;
            }
            else
            {
                @ViewBag.Message = "2";
            }

            ViewBag.RegistrationNo = vehicles.RegistrationNo;
            return View("Index", vehicles);

        }


        [HttpGet]
        public JsonResult GetActiveVehicles()
        {
           
            var vehicles = _bllvehicle.GetVehicles();
            return Json(JsonConvert.SerializeObject(vehicles, Formatting.None, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }
	}
}