//using HospitalityManagement.Models;

using Newtonsoft.Json;
using RIT.HMS.BLL.MasterData;
using RIT.HMS.Domain;
using RIT.HMS.Domain.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HospitalityManagement.Controllers
{
    [SessionTimeout]

    public class TaxController : Controller
    {
        BLL_Tax _blltax;


        public TaxController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _blltax = new BLL_Tax(cn);

        }

        [Authorize(Roles = "TaxesAndPayments")]
        public ActionResult Index()
        {
            return View();
        }

       
        public ActionResult Clear()
        {

            return View("Edit");
        }

        [Authorize(Roles = "TaxesAndPayments")]
        public ActionResult Edit(long id)
        {
           
            var tax = _blltax.GetTaxById(id);
            ViewBag.TaxID = tax.TaxId;
            return View(tax);
        }

        [Authorize(Roles = "TaxesAndPayments")]
        [HttpPost]
        public ActionResult Edit(Tax Taxes)
        {
           
            var tax = _blltax.GetTaxById(Taxes.TaxId);
            tax.TaxCode = Taxes.TaxCode;
            tax.TaxName = Taxes.TaxName;
            tax.TaxPercentage = Taxes.TaxPercentage;
            //tax.EffectivePercentage = Taxes.EffectivePercentage;
            //tax.EffectiveDate = Taxes.EffectiveDate;
            tax.IsPurchasingTax = Taxes.IsPurchasingTax;
            tax.IsSellingTax = Taxes.IsSellingTax;
            tax.IsTaxOnTax = Taxes.IsTaxOnTax;
            tax.IsServiceCharge = Taxes.IsServiceCharge;
            tax.isExcludeTax = Taxes.isExcludeTax;
            tax.IsActive = Taxes.IsActive;
            tax.IsDelete = Taxes.IsDelete;
            tax.ModifiedDate = DateTime.UtcNow;
            tax.ModifiedUser = Session["loggeduser"].ToString();
            tax.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());

            if (_blltax.UpdateTax(tax) == 1)
            {

                ViewBag.Message = "1";
            }
            else
            {
                ViewBag.Message = "0";
            }

            return View(tax);
        }

        [Authorize(Roles = "TaxesAndPayments")]
        public ActionResult ViewTaxes()
        {

            // TaxService taxreporsitory = new TaxService();
            //GroupOfCompanyService gocreporsitory = new GroupOfCompanyService();
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var taxes = _blltax.GetTaxes(companyid).OrderBy(c => c.TaxCode);

            taxes.ToList().ForEach(c =>
            {

                //    c.UserName = userreporsitory.GetSysUserMasterID(c.SysUserMasterID).GroupOfCompanyName;
            });

            return View(taxes);
        }


        [Authorize(Roles = "TaxesAndPayments")]
        [HttpPost]
        public ActionResult Create(Tax tax)
        {
            tax.CreatedUser = Session["loggeduser"].ToString();
            tax.CreatedDate = DateTime.UtcNow;
            tax.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
            tax.IsActive = true;
            tax.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());

            if (!ModelState.IsValid)
            {
                return View("Index",tax);
            }

            // TaxService reporsitory = new TaxService();
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var existstax = _blltax.GetTaxByCode(tax.TaxCode,companyid);
            if (existstax != null)
            {
                ViewBag.Message = "3";
                return View("Index",tax);
            }
            if (_blltax.SaveTax(tax) == 1)
            {
                @ViewBag.Message = "1";
                //tax = null;
            }
            else
            {
                @ViewBag.Message = "2";
            }

            ViewBag.TaxCode = tax.TaxCode;
            return View("Index",tax);
        }


        [Authorize(Roles = "TaxesAndPayments")]
        public ActionResult TaxModes()
        {
            return View("~/Views/Tax/TaxModes.cshtml");
        }

        [HttpGet]
        public JsonResult GetActiveTaxes()
        {
            // TaxService reporsitory = new TaxService();
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var taxes = _blltax.GetActiveTaxes(companyid);
            return Json(JsonConvert.SerializeObject(taxes, Formatting.None, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult SaveTaxModes(List<TaxModesViewModel> taxmodes)
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            int locid = Convert.ToInt32(Session["loggeduserlocId"]);
            taxmodes.ForEach(t=>{
                t.CompanyId = companyid;
                t.LocationId = locid;
                t.CreateUser = Session["loggeduser"].ToString();
               
            });
          
            return Json(_blltax.SaveTaxModes(taxmodes));      
        }


        [HttpGet]
        public JsonResult GetTaxModes(long id)
        {

            //  var taxmodes = _unitConversionService.GetConversionByMeasurementId(id, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            //  return Json(JsonConvert.SerializeObject(conversions, Formatting.None, new JsonSerializerSettings
            // { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);

            return null;
        }



        [HttpGet]
        public JsonResult GetActivePayModes()
        {
            // TaxService reporsitory = new TaxService();
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var paymodes = _blltax.GetActivePayModes(companyid);
            return Json(JsonConvert.SerializeObject(paymodes, Formatting.None, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }
    }
}