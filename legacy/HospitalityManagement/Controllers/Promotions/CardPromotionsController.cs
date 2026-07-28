using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RIT.HMS.BLL.Promotions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HospitalityManagement.Controllers.Promotions
{
    [SessionTimeout]
    [Authorize(Roles = "PrdCreatee")]
    public class CardPromotionsController : Controller
    {
        // GET: CardPromotions

        BLL_BankBin _bankbin;
        public CardPromotionsController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _bankbin = new BLL_BankBin(cn);
        }
        public ActionResult CreateCardPromotions()
        {
            return View("~/Views/Promotions/CardPromotions/CreateCardPromotions.cshtml",
                _bankbin.GetActiveBankBins());
        }

        [HttpGet]
        public JsonResult GetActiveBanks()
        {           
            var banks = _bankbin.GetActiveBanks();
            return Json(JsonConvert.SerializeObject(banks, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetActiveCardTypes()
        {
            var cardtypes = _bankbin.GetActiveCardTypes();
            return Json(JsonConvert.SerializeObject(cardtypes, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        public JsonResult SubmitCardPromotion(string locid, int bankid,int cardid,string cardprefix,string cardname,
                                           decimal discountamt, decimal discountprc, decimal valuefrom, decimal valueto, 
                                           DateTime datefrom, DateTime dateto,TimeSpan starttime,TimeSpan endtime
                                            )
        {


            string[] locids = locid.Replace('\\', ' ').Replace('[', ' ').Replace(']', ' ').
                              Replace('"', ' ').Trim().Split(',');
         
            return new JsonResult
            {
                Data = _bankbin.SubmitCardPromotion(locids, bankid,cardid,cardprefix,cardname,
                                       discountamt,discountprc,valuefrom,valueto,
                                       datefrom, dateto, starttime,endtime, Convert.ToInt32(Session["loggedusercompanyId"].ToString())),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        [HttpGet]
        public JsonResult RemoveCardPromotion(int rowid)
        {

            var responsemsg = _bankbin.RemoveCardPromotions(rowid);
            return Json(JsonConvert.SerializeObject(responsemsg, Formatting.None,
            new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }),
            JsonRequestBehavior.AllowGet);

        }
    }
}