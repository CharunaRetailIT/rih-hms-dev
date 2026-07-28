//using HospitalityManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
//using HospitalityManagement.Service;
using Newtonsoft.Json;
using RIT.HMS.BLL.MasterData;
using RIT.HMS.Domain;

namespace HospitalityManagement.Controllers
{
    [SessionTimeout]
    public class CurrencyController : Controller
    {
      
        BLL_Currency _currency;       
        BLL_CurrencyHistory _currencyHistory;
        BLL_Currency reporsitory;

        public CurrencyController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _currency = new BLL_Currency(cn);
            _currencyHistory = new BLL_CurrencyHistory(cn);
            reporsitory = new BLL_Currency(cn);
        }

        // GET: Currency
        [Authorize(Roles = "TaxesAndPayments")]
        [HttpGet]
        public ActionResult Currency()
        {
            return View("~/Views/Currency/Currency.cshtml");
        }

        [Authorize(Roles = "TaxesAndPayments")]
        [HttpPost]
        public ActionResult Currency(Currency currency)
        {
            if (!ModelState.IsValid)
            {
                return View("Currency");
            }
            else
            {
                currency.CreatedDate = DateTime.Now;
                currency.CreatedUser = Session["loggeduser"].ToString();
                currency.DataTransfer = 0;
                currency.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
                //ViewBag.Message = _currencyService.SaveCurrency(currency)==1 ? "1" : "2" ;

            }


            //CurrencyService currencyervice = new CurrencyService();
            var existscurrency = _currency.GetCurrencyByCode(currency.CurrencyCode);
            if (existscurrency != null)
            {
                ViewBag.Message = "3";
                return View("Currency", currency);
            }

            if (_currency.SaveCurrency(currency) == 1)
            {
                ViewBag.Message = "1";
            }
            else
            {
                ViewBag.Message = "2";
            }

            @ViewBag.CurrCode = currency.CurrencyCode;
            return View("Currency", currency);
        }

        [Authorize(Roles = "TaxesAndPayments")]
        [HttpGet]
        public ActionResult ViewCurrencies()
        {
            var curreies = _currency.GetCurrencies();          
            return View(curreies);
        }
        [Authorize(Roles = "TaxesAndPayments")]
        [HttpGet]
        public ActionResult Edit(long id)
        {
            var curriencies = _currency.GetCurrencyById(id);
            return View(curriencies);
        }
        [Authorize(Roles = "TaxesAndPayments")]
        [HttpGet]
        public ActionResult History(long id)
        {
            var history = _currencyHistory.GetCurrencyHistoryByCurrencyId(id);
            history.ToList().ForEach(c =>
            {
                c.CurrencyCode = _currency.GetCurrencyById(id).CurrencyCode;
                c.CurrencyDescription= _currency.GetCurrencyById(id).CurrencyDescription;
            });

            return View(history);
        }
        [Authorize(Roles = "TaxesAndPayments")]
        [HttpPost]
        public ActionResult Edit(Currency currency)
        {
            if (!ModelState.IsValid)
            {
                return View(currency);
            }

            var exists = _currency.GetCurrencyById(currency.CurrencyId);
            if (exists != null)
            {
                CurrencyHistory currencyhistory=new CurrencyHistory();
                currencyhistory.CurrencyId = exists.CurrencyId;
                currencyhistory.AsofDate = exists.AsofDate;
                currencyhistory.BuyingRate = exists.BuyingRate;
                currencyhistory.SellingRate = exists.SellingRate;
                currencyhistory.BuyingRate = exists.BuyingRate;
                currencyhistory.CreatedUser = exists.CreatedUser;
                currencyhistory.CreatedDate = exists.CreatedDate;
                currencyhistory.ModifiedDate = DateTime.Now;
                currencyhistory.ModifiedUser = Session["loggeduser"].ToString();
               

                if (_currencyHistory.SaveCurrencyHistory(currencyhistory) == 1)
                {
                   
                    exists.CurrencyCode = currency.CurrencyCode;
                    exists.CurrencyDescription = currency.CurrencyDescription;
                    exists.CurrencyFormat = currency.CurrencyFormat;
                    exists.CurrencySymbol = currency.CurrencySymbol;
                    exists.BuyingRate = currency.BuyingRate;
                    exists.SellingRate = currency.SellingRate;
                    exists.AsofDate = currency.AsofDate;
                    exists.BuyingRate = currency.BuyingRate;
                    exists.SellingRate = currency.SellingRate;
                    exists.BuyingRate = currency.BuyingRate;
                    exists.CreatedUser = currency.CreatedUser;
                    exists.CreatedDate = currency.CreatedDate;
                    exists.ModifiedDate = DateTime.Now;
                    exists.IsActive = currency.IsActive;
                    exists.IsDelete = currency.IsDelete;
                    exists.ModifiedUser = Session["loggeduser"].ToString();

                    @ViewBag.Message = _currency.UpdateCurrency(exists) ==1 ? "1" : "2";

                }
                else
                {
                    @ViewBag.Message = "3";
                }
            }

            ViewBag.CurrCode = currency.CurrencyCode;
           
            return View(currency);
        }

        [HttpGet]
        public JsonResult GetCurrencyies()
        {
           
            var currencies = reporsitory.GetActiveCurrencies();
            return Json(JsonConvert.SerializeObject(currencies, Formatting.None, new JsonSerializerSettings
                { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult GetCurrenciesForTransactions()
        {
           
            var currencies = reporsitory.GetCurrenciesForTransactions();
            return Json(JsonConvert.SerializeObject(currencies, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }


        //Added by pavithra to get the currency rate
        [HttpGet]
        public JsonResult GetCurrencyRateForID(int currencyid)
        {

           
            var currencies = reporsitory.GetCurrencyByID(currencyid).FirstOrDefault().SellingRate;

            return new JsonResult
            {
                Data = currencies,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

    }
}