using Newtonsoft.Json;
using RIT.HMS.BLL.Configurations;
using RIT.HMS.BLL.Loyalty;
using RIT.HMS.Domain.Loyalty;
using RIT.HMS.Domain.ViewModels.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HospitalityManagement.Controllers.Loyalty
{
    [SessionTimeout]
    public class CardMasterController : Controller
    {
        private readonly BLL_Configuration _bllconfiguration;
        private readonly BLL_CardMaster _bllcardmaster;
        private AppManager _appmanager;
        public CardMasterController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
           _bllconfiguration = new BLL_Configuration(cn);
           _bllcardmaster = new BLL_CardMaster(cn);
           _appmanager = new AppManager(cn);
        }

        public ActionResult CardMaster()
        {

            if (!_appmanager.SetPermissions(13, Session["loggeduserempcode"].ToString(), "CardTypeCreatee"))
            {
                @ViewBag.Permissions = "No user permissions to Create Card Types";
                return View("~/Views/Account/AccessDenied.cshtml");
            }

            Session["CMWizadUI"] = _bllconfiguration.GetConfiguration("UIWZ", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;          
            return View("~/Views/Loyalty/CardType.cshtml", new CardMaster());
        }

        [HttpPost]
        public ActionResult CardMaster(CardMaster cardmaster)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors);
           
            if (ModelState.IsValid == false)
            {
                ViewBag.CardType = cardmaster.CardType;
                return View("~/Views/Loyalty/CardType.cshtml", cardmaster);
            }

            if (cardmaster.LoyaltyCardSchems.Count()==0)
            {
                ViewBag.CardType = cardmaster.CardType;
                ViewBag.Message = "2";
                return View("~/Views/Loyalty/CardType.cshtml", cardmaster);
            }
            cardmaster.CreatedDate = DateTime.Now;
            cardmaster.CreatedUser = Session["loggeduser"].ToString();
            cardmaster.LocationId = Convert.ToInt32(Session["loggeduserlocid"]);
            cardmaster.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            ViewBag.CTCode = cardmaster.CardCode;
            if (_bllcardmaster.SaveCardMaster(cardmaster))
            {
                ViewBag.Message = "1";
                ModelState.Clear();   
                return View("~/Views/Loyalty/CardType.cshtml", new CardMaster());
            }
            else
            {
                ViewBag.CardType = cardmaster.CardType;
                ViewBag.Message = "3";
                return View("~/Views/Loyalty/CardType.cshtml", cardmaster);
            }

           
        }

        public ActionResult AllCardTypes()
        {
            if (!_appmanager.SetPermissions(13, Session["loggeduserempcode"].ToString(), "CardTypeView"))
            {
                @ViewBag.Permissions = "No user permissions to View Card Types";
                return View("~/Views/Account/AccessDenied.cshtml");
            }

            Session["CMWizadUI"] = _bllconfiguration.GetConfiguration("UIWZ", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            var cards = _bllcardmaster.GetAllActiveCards(Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            foreach(var c in cards)
            {
                //c.CardTypeName = _bllcardmaster.GetReferanceTypes("25").Where(r =>
                //                r.ReferenceTypeId == c.CardType).FirstOrDefault().LookupValue;
                var cardtype = _bllcardmaster.GetReferanceTypes("25", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).Where(r =>
                               r.ReferenceTypeId == c.CardType).FirstOrDefault();
                if (cardtype != null)
                {
                    c.CardTypeName = _bllcardmaster.GetReferanceTypes("25", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).Where(r =>
                                   r.ReferenceTypeId == c.CardType).FirstOrDefault().LookupValue;
                }
                else
                {
                    c.CardTypeName = "N/A";
                }
            }
            return View("~/Views/Loyalty/ViewAllCardTypes.cshtml", cards);
        }

        public ActionResult EditCard(int id)
        {
            if (!_appmanager.SetPermissions(13, Session["loggeduserempcode"].ToString(), "CardTypeEdit"))
            {
                @ViewBag.Permissions = "No user permissions to Edit Card Types";
                return View("~/Views/Account/AccessDenied.cshtml");
            }

            var card = _bllcardmaster.GetCardById(id);
            card.IsExists = true;
            ViewBag.CardType = card.CardType;
           return View("~/Views/Loyalty/CardType.cshtml", card);
        }

        public ActionResult Expirations()
        {
            PointsExpiration pointsexp = new PointsExpiration();
            pointsexp.Year = DateTime.Now.Year;
          
            return View("~/Views/Loyalty/Expirations.cshtml", pointsexp);
        }

        [HttpPost]
        public ActionResult Expirations(PointsExpiration pointsexp)
        {
            if (ModelState.IsValid)
            {
                pointsexp.CreatedDate = DateTime.Now;
                pointsexp.CreatedUser = Session["loggeduser"].ToString();
                pointsexp.LocationId = Convert.ToInt32(Session["loggeduserlocid"].ToString());
                pointsexp.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
                pointsexp.ModifiedDate = DateTime.Now;
              
                if (_bllcardmaster.SaveExpiration(pointsexp))
                {
                    @ViewBag.Message = "1";
                    return View("~/Views/Loyalty/Expirations.cshtml", new PointsExpiration());
                }
                else
                {
                    @ViewBag.Message = "3";
                    return View("~/Views/Loyalty/Expirations.cshtml", pointsexp);
                }

               
            }
            else
            {
                @ViewBag.Message = "3";
                return View("~/Views/Loyalty/Expirations.cshtml", pointsexp);
            }
        }
        // report -------------------------------------------
        [Authorize(Roles = "Reports")]
        public ActionResult Statement()
        {
            if (_bllconfiguration.GetConfiguration("UReports", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn)
            {

                if (!_appmanager.CheckReportPermissions(999, Session["loggeduserempcode"].ToString(), "RPTStatement"))
                {
                    @ViewBag.Permissions = "No user permissions to View Customer Statement";
                    return View("~/Views/Account/AccessDenied.cshtml");
                }

            }

            return View("~/Views/Reports/Loyalty/CustomerPoints.cshtml", new CustomerStatementViewModel());
        }

        [HttpPost]
        [Authorize(Roles = "Reports")]
        public ActionResult Statement(CustomerStatementViewModel cus)
        {
            if (_bllconfiguration.GetConfiguration("UReports", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn)
            {

                if (!_appmanager.CheckReportPermissions(999, Session["loggeduserempcode"].ToString(), "RPTStatement"))
                {
                    @ViewBag.Permissions = "No user permissions to View Customer Statement";
                    return View("~/Views/Account/AccessDenied.cshtml");
                }

            }
            var statement = _bllcardmaster.CustomerStatement(cus.CustomerId);
            return View("~/Views/Reports/Loyalty/CustomerPoints.cshtml",statement);
        }

        [Authorize(Roles = "Reports")]
        public ActionResult StatementSummary()
        {
            if (_bllconfiguration.GetConfiguration("UReports", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn)
            {

                if (!_appmanager.CheckReportPermissions(999, Session["loggeduserempcode"].ToString(), "RPTStatementSummary"))
                {
                    @ViewBag.Permissions = "No user permissions to View Customer Statement Summary";
                    return View("~/Views/Account/AccessDenied.cshtml");
                }

            }

            return View("~/Views/Reports/Loyalty/CustomerPointsSummary.cshtml",new CustomerStatementViewModel());
        }

        [HttpPost]
        [Authorize(Roles = "Reports")]
        public ActionResult StatementSummary(CustomerStatementViewModel cus)
        {
            if (_bllconfiguration.GetConfiguration("UReports", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn)
            {

                if (!_appmanager.CheckReportPermissions(999, Session["loggeduserempcode"].ToString(), "RPTStatementSummary"))
                {
                    @ViewBag.Permissions = "No user permissions to View Customer Statement Summary";
                    return View("~/Views/Account/AccessDenied.cshtml");
                }

            }
            var statement = _bllcardmaster.CustomerStatementSummary(cus.CustomerId);
            return View("~/Views/Reports/Loyalty/CustomerPointsSummary.cshtml", statement);
        }

        // Loaders ----------------------------------------
        [HttpGet]
        public JsonResult GetActiveReferances()
        {

            var recipts = _bllcardmaster.GetReferanceTypes("25", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            return Json(JsonConvert.SerializeObject(recipts, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetCardNumber(string formname)
        {
            var no = _bllcardmaster.GetNewCode(formname);
            return new JsonResult
            {
                Data = no,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        [HttpGet]
        public JsonResult GetActiveCards()
        {

            var recipts = _bllcardmaster.GetAllActiveCards(Convert.ToInt32(Session["loggedusercompanyId"].ToString())).Select(a=> new {a.CardMasterId,a.CardCode });
            return Json(JsonConvert.SerializeObject(recipts, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetCardDetailsById(int cid1)
        {
            var card = _bllcardmaster.GetCardById(cid1);
            CardMaster c = new CardMaster();
            c.CardMasterId = card.CardMasterId;
            c.CardCode = card.CardCode;
            c.CardName = card.CardName;
            return new JsonResult
            {
                Data = c,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
    }
}