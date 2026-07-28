//using HospitalityManagement.Models;
//using HospitalityManagement.Service;
using Newtonsoft.Json;
using RIT.HMS.BLL.MasterData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RIT.HMS.Domain;
using RIT.HMS.BLL.Common;

namespace HospitalityManagement.Controllers
{
    [Authorize]
    [SessionTimeout]

  
    public class GroupOfCompanyController : Controller
    {
        private readonly BLL_Common _bllcommon;
        BLL_GroupOfCompany _bllgroupOfCompany;

        public GroupOfCompanyController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _bllcommon = new BLL_Common(cn);
            _bllgroupOfCompany = new BLL_GroupOfCompany(cn);
        }

        [Authorize(Roles = "GOCCreatee")]
        public ActionResult Index()
        {
          
            return View();
        }


        [Authorize(Roles = "GOCEdit")]
        [HttpGet]
        public ActionResult Edit(long id)
        {
           
            var goc = _bllgroupOfCompany.GetGroupOfCompanyById(id);
            @ViewBag.FileName = goc.CompanyLogoName;
            return View(goc);
        }

        [Authorize(Roles = "GOCEdit")]
        [HttpPost]
        public ActionResult Edit(SysGroupOfCompany gocedit)
        {
         

            Common common = new Common();

            if (gocedit.File != null && common.CheckImageType(gocedit.File.ContentType) == false)
            {
                ModelState.AddModelError("File", "Only an Image required !");
                return View(gocedit);
            }
            if (gocedit.IsActive == true && gocedit.IsDelete == true)
            {
                @ViewBag.Message = "4";
                return View(gocedit);
            }

            var goc = _bllgroupOfCompany.GetGroupOfCompanyById(gocedit.SysGroupOfCompanyId);

            goc.GroupOfCompanyName = gocedit.GroupOfCompanyName;
            goc.CompanyGmail = gocedit.CompanyGmail;
            goc.CompanyVatNumber = gocedit.CompanyVatNumber;
            goc.IsActive = gocedit.IsActive;
            goc.IsDelete = gocedit.IsDelete;

            if (gocedit.File != null)
            {
                byte[] newlogo;
                using (BinaryReader br = new BinaryReader(gocedit.File.InputStream))
                {
                    newlogo = br.ReadBytes(gocedit.File.ContentLength);
                    gocedit.CompanyLogo = newlogo;
                    gocedit.CompanyLogoName = gocedit.File.FileName;
                    gocedit.CompanyLogoType = gocedit.File.ContentType;
                }


                if (gocedit.CompanyLogoName != goc.CompanyLogoName)
                {
                    byte[] logo;
                    using (BinaryReader br = new BinaryReader(gocedit.File.InputStream))
                    {
                        logo = br.ReadBytes(gocedit.File.ContentLength);
                        goc.CompanyLogo = logo;
                        goc.CompanyLogoName = gocedit.File.FileName;
                        goc.CompanyLogoType = gocedit.File.ContentType;
                    }
                }
            }

            if (_bllgroupOfCompany.UpdateGroupOfCompany(goc) == 1)
            {
                @ViewBag.Message = "1";
            }
            else
            {
                @ViewBag.Message = "0";
            }

            return View("Edit");
        }

        [HttpGet]
        public ActionResult Clear(long id)
        {
           
            var goc = _bllgroupOfCompany.GetGroupOfCompanyById(id);
            @ViewBag.FileName = goc.CompanyLogoName;
            return View(goc);
        }

        [Authorize(Roles = "GOCView")]
        public ActionResult ViewGroupOfCompanies()
        {
            Session["CreateGroupOfCompanies"] = _bllcommon.GetConfigurations().CreateGroupOfCompanies;
          
            var groupofcompanies = _bllgroupOfCompany.GetGroupOfCompanies(Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            return View(groupofcompanies);
        }

        [Authorize(Roles = "GOCCreatee")]
        [HttpPost]
        public ActionResult Create(SysGroupOfCompany goc)
        {
            if (!ModelState.IsValid)
            {
                return View("Index");
            }

            Common common = new Common();
            goc.IsActive = true;

            if (goc.File != null && common.CheckImageType(goc.File.ContentType) == false)
            {
                ModelState.AddModelError("File", "Only an Image required !");
                return View("Index", goc);
            }
            

            if (goc.File != null)
            {
                byte[] logo;
                using (BinaryReader br = new BinaryReader(goc.File.InputStream))
                {
                    logo = br.ReadBytes(goc.File.ContentLength);
                    goc.CompanyLogo = logo;
                    goc.CompanyLogoName = goc.File.FileName;
                    goc.CompanyLogoType = goc.File.ContentType;
                }
            }

          
            var existsgoc = _bllgroupOfCompany.GetGOCByCode(goc.GroupOfCompanyCode);
            if (existsgoc != null)
            {
                ViewBag.Message = "3";
                return View("Index", goc);
            }
            if (goc.IsActive == true && goc.IsDelete == true)
            {
                @ViewBag.Message = "4";
                return View();
            }
            if (_bllgroupOfCompany.SaveGroupOfCompany(goc) == 1)
            {
                @ViewBag.Message = "1";
            }
            else
            {
                @ViewBag.Message = "2";
            }
            @ViewBag.GOCCode = goc.GroupOfCompanyCode;

            return View("Index", goc);
        }



        [HttpGet]
        public JsonResult GetGroupOfCompanies()
        {
           
            var groupofcompanies = _bllgroupOfCompany.GetActiveGroupOfCompanies();
            return Json(JsonConvert.SerializeObject(groupofcompanies, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

    }
}           