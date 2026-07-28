using HospitalityManagement.Service;
using RIT.HMS.BLL.MasterData;
using RIT.HMS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HospitalityManagement.Controllers.Customizations
{
    [SessionTimeout]
    public class CustomizationsController : Controller
    {
        BLL_Customization _bllcustomization = new BLL_Customization();

        //
        // GET: /Customizations/
        public ActionResult Create()
        {
            return View("~/Views/Customizations/Index.cshtml");
        }
        [HttpGet]
        public ActionResult Edit(int id)
        {
             CompanyService companyreporsitory = new CompanyService();
            var customization = _bllcustomization.GetCustomizationById(id);
            ViewBag.CustomizationId = customization.CustomizationId;



            return View(customization);
        }

        [HttpPost]
        public ActionResult Edit(Customization customization)
        {
            if (!ModelState.IsValid)
            {
                @ViewBag.viewcustomization = customization.CustomizationId;

                return View(customization);

            }



            //   CompanyService companyreporsitory = new CompanyService();
            ViewBag.viewcustomization = customization.CustomizationId;



            if (_bllcustomization.Updatecustomization(customization) == 1)
            {
                ViewBag.Message = "1";
                return View(new Customization());
            }
            else
            {
                ViewBag.Message = "0";
                return View(customization);
            }


        }
        [HttpPost]
        public ActionResult Create(Customization customization)
        {
            customization.GroupOfCompanyID = 1;
            customization.CompanyID = 1;
            customization.CreatedUser = "1";
            customization.CreatedDate = DateTime.Now;
            customization.ModifiedDate = DateTime.Now;
            customization.ModifiedUser = "1";
            int status = customization.Status;
            

           var cus = _bllcustomization.GetCustomizationByKey(customization.KeyValue);
           //var cus = _bllcustomization.ShowCustomization();

           
            
            if (cus == null)
            {
                

                var res = _bllcustomization.SaveCustomization(customization);
                if (res == 1)
                {
                    @ViewBag.Message = "1";
                }
                else
                {
                    @ViewBag.Message = "2";
                }
            }
            else
            {
              

                @ViewBag.Message = "2";
            }
           

            return View("~/Views/Customizations/Index.cshtml");

        }


        
        public ActionResult Index()
        {

            var customization = _bllcustomization.ShowCustomization();
            //var customization = _bllcustomization.ShowCustomization();
            

            return View("Create",customization);
        }

        

        
    }
}