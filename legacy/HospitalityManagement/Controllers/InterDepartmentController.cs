//using HospitalityManagement.Models;
//using HospitalityManagement.Service;
using RIT.HMS.BLL.MasterData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RIT.HMS.Domain;

namespace HospitalityManagement.Controllers
{
    [SessionTimeout]
    public class InterDepartmentController : Controller
    {
        BLL_InterDepartment _bllinterDepartment;
        public InterDepartmentController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _bllinterDepartment = new BLL_InterDepartment(cn);

        }

        [Authorize(Roles = "InterDeptCreatee")]
        public ActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "InterDeptEdit")]
        public ActionResult Edit(long id)
        {
           
            //InterDepartmentService interdeptrepository = new InterDepartmentService();
            var exists = _bllinterDepartment.GetInterDepartmentById(id);

            ViewBag.InterDeptLocId = exists.InterDeptLocId;
            return View(exists);
        }

        [Authorize(Roles = "InterDeptEdit")]
        [HttpPost]
        public ActionResult Edit(InterDepartment interdept)
        {
           
            var exists = _bllinterDepartment.GetInterDepartmentById(interdept.InterDepartmentId);
            exists.InterDepartmentCode = interdept.InterDepartmentCode;
            exists.InterDepartmentName = interdept.InterDepartmentName;
            exists.InterDeptLocId = interdept.InterDeptLocId;
            exists.Remark = interdept.Remark;
            exists.IsActive = interdept.IsActive;
            exists.ModifiedDate = DateTime.Now;
            exists.ModifiedUser = Session["loggeduser"].ToString();
            exists.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
            //exists.GroupOfCompanyID = 0;
            //exists.CompanyID = 0;
            exists.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());

            //exists.DataTransfer = 0;
            if (_bllinterDepartment.UpdateInterDepartment(exists) == 1)
            {
                ViewBag.Message = "1";
            }
            else
            {
                ViewBag.Message = "0";
            }
            ViewBag.InterDeptCode = exists.InterDepartmentCode;
            return View(exists);
        }

        [Authorize(Roles = "InterDeptCreatee")]
        [HttpPost]
        public ActionResult Create(InterDepartment interdept)
        {

            if (!ModelState.IsValid)
            {
                return View();

            }

           
            interdept.CreatedDate = DateTime.Now;
            interdept.CreatedUser = Session["loggeduser"].ToString();
            interdept.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
            interdept.IsActive = true;

            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            interdept.CompanyID = companyid;
            var existsinterdept = _bllinterDepartment.GetInterDeptByCode(interdept.InterDepartmentCode,companyid);
            if (existsinterdept != null)
            {
                ViewBag.Message = "3";
                return View("Create", interdept);
            }

            if (_bllinterDepartment.SaveInterDepartment(interdept) == 1)
            {
                ViewBag.Message = "1";
            }
            else
            {
                ViewBag.Message = "2";
            }

            ViewBag.InterDeptCode = interdept.InterDepartmentCode;
            return View("Create", interdept);

        }

        [Authorize(Roles = "InterDeptView")]
        public ActionResult ViewInterDepartments()
        {
           
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var interdepts = _bllinterDepartment.GetInterDepartments(companyid);
            return View(interdepts);
        }

        [HttpGet]
        public JsonResult GetActiveInterDepartments()
        {
          
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var interdepts = _bllinterDepartment.GetActiveInterDepartments(companyid);
            return Json(JsonConvert.SerializeObject(interdepts, Formatting.None, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetActiveInterDepartmentsByLocationId(int id)
        {
           
            var interdepts = _bllinterDepartment.GetInterDepartmentsByLocationId(id);
            return Json(JsonConvert.SerializeObject(interdepts, Formatting.None, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }
    }
}