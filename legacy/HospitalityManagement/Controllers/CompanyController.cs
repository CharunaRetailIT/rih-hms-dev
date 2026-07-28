//using HospitalityManagement.Models;
//using HospitalityManagement.Service;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Web.Mvc;
using RIT.HMS.Domain;
using RIT.HMS.BLL.MasterData;
using RIT.HMS.BLL.Common;

namespace HospitalityManagement.Controllers
{
    [Authorize]
    [SessionTimeout]
    public class CompanyController : Controller
    {
        BLL_Company _bllcompany ;
        BLL_GroupOfCompany _bllgroupofcompany;
        private readonly BLL_Common _bllcommon;
        public CompanyController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _bllcompany = new BLL_Company(cn);
            _bllgroupofcompany = new BLL_GroupOfCompany(cn);
            _bllcommon = new BLL_Common(cn);
        }
        //
        //// GET: /Company/   
        [Authorize(Roles = "CCreatee")]
        public ActionResult Index()
        {
           
            return View();
        }
        public ActionResult Clear()
        {

            return View("Edit");
        }

        [Authorize(Roles = "CEdit")]
        public ActionResult Edit(long id)
        {
          //  CompanyService companyreporsitory = new CompanyService();
            var company = _bllcompany.GetCompanyById(id);
            ViewBag.SysGroupOfCompanyId = company.SysGroupOfCompanyId;
            return View(company);
        }

        [Authorize(Roles = "CEdit")]
        [HttpPost]
        public ActionResult Edit(SysCompany syscompany)
        {
            if (!ModelState.IsValid)
            {
                @ViewBag.SysGroupOfCompanyId = syscompany.SysGroupOfCompanyId;
                return View(syscompany);

            }
            if (syscompany.IsActive == true && syscompany.IsDelete == true)
            {
                @ViewBag.SysGroupOfCompanyId = syscompany.SysGroupOfCompanyId;
                @ViewBag.Message = "4";
                return View(syscompany);
            }

            @ViewBag.SysGroupOfCompanyId = syscompany.SysGroupOfCompanyId;
            
            if (_bllcompany.UpdateCompany(syscompany) == 1)
            {
                ViewBag.Message = "1";
                return View(new SysCompany());
            }
            else
            {
                @ViewBag.SysGroupOfCompanyId = syscompany.SysGroupOfCompanyId;
                ViewBag.Message = "0";
                return View(syscompany);
            }
        }

        [Authorize(Roles = "CView")]
        public ActionResult ViewCompanies()  
        {
            Session["CreateCompanies"] = _bllcommon.GetConfigurations().CreateCompanies;
          
            // CompanyService companyreporsitory = new CompanyService();
           // BLL_GroupOfCompany _bllGorupofCompany = new BLL_GroupOfCompany();
            var companies = _bllcompany.ShowCompanies(Convert.ToInt32(Session["loggedusercompanyId"].ToString())).OrderBy(c=>c.CompanyCode);

            companies.ToList().ForEach(c =>
            {
                c.GroupOfCompanyName = _bllgroupofcompany.GetGroupOfCompanyById(c.SysGroupOfCompanyId).GroupOfCompanyName;
            });


            return View(companies);
        }

        [Authorize(Roles = "CCreatee")]
        [HttpPost]
        public ActionResult Create(SysCompany syscompany)
        {
            if (!ModelState.IsValid)
            {
                @ViewBag.SysGroupOfCompanyId = syscompany.SysGroupOfCompanyId;
                return View("Index", syscompany);
            }

           // CompanyService reporsitory = new CompanyService();
         
            var existscom = _bllcompany.GetCompanyByCode(syscompany.CompanyCode);
            if (existscom != null)
            {
                ViewBag.Message = "3";
                @ViewBag.SysGroupOfCompanyId = syscompany.SysGroupOfCompanyId;
                return View("Index", syscompany);
            }

            ViewBag.CompanyCode = syscompany.CompanyCode;

            if (_bllcompany.SaveCompany(syscompany,Convert.ToInt32(Session["loggeduserid"].ToString())) == 1)
            {
                ViewBag.Message = "1";
                return View("Index", new SysCompany());
            }
            else
            {
                @ViewBag.SysGroupOfCompanyId = syscompany.SysGroupOfCompanyId;
                ViewBag.Message = "2";
                return View("Index", syscompany);
            }
                    
        }

        [HttpGet]
        public JsonResult GetActiveCompanies()
        {
          
            var companies = _bllcompany.GetCompanies();
            return Json(JsonConvert.SerializeObject(companies, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetCompanyByGOCId(long id)
        {
            //CompanyService reporsitory = new CompanyService();

            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var companies = _bllcompany.GetByGOCId(id,companyid);
            return Json(JsonConvert.SerializeObject(companies, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

    }
}