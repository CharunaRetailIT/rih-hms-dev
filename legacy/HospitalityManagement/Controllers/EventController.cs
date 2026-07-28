using RIT.HMS.BLL.MasterData;
using RIT.HMS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HospitalityManagement.Controllers
{
    [SessionTimeout]
    public class EventController : Controller
    {
        BLL_Event _bllEvent ;
        BLL_Product _bllProduct;

        public EventController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _bllEvent = new BLL_Event(cn);
            _bllProduct = new BLL_Product(cn);
        }

        [Authorize(Roles = "Others")]
        public ActionResult ViewEvents()
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            return View(_bllEvent.GetActiveEvents(companyid));
        }

        public ActionResult Create()
        {
            ViewBag.productdata = _bllProduct.GetOpenItems(Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ToList();
            ViewBag.IsPOS = false;
            return View();
        }
        [Authorize(Roles = "Others")]
        public ActionResult Edit(long id)
        {
           var lll = _bllProduct.GetOpenItems(Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ToList();
            var ccc = lll.Where(k => k.IsOpenItem == true);
            ViewBag.productdata = _bllProduct.GetOpenItems(Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ToList();
           
            //  CompanyService companyreporsitory = new CompanyService();
            var evnt = _bllEvent.GetEventById(id);
            evnt.EventProducts = _bllEvent.GetEventProductsByEventId(evnt.EventId);
            ViewBag.PaymentMethodId = evnt.EventId;
            ViewBag.IsPOS = evnt.IsPOS;
           // @ViewBag.LocationId = evnt.LocationId;
            return View(evnt);
        }

        [Authorize(Roles = "Others")]
        [HttpPost]
        public ActionResult Create(Event evnt)
        {
            ViewBag.productdata = _bllProduct.GetOpenItems(Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ToList();
            ViewBag.IsPOS = evnt.IsPOS;
            @ViewBag.LocationId = evnt.LocationId;
            //if (_bllEvent.GetEvents().Where(evt => evt.EventCode == evnt.EventCode).Count() != 0)
            //{
            //    ViewBag.Message = "3";
            //    @ViewBag.EventId = evnt.EventId;
            //    return View("Create", evnt);
            //}
            if (!ModelState.IsValid)
            {
                @ViewBag.EventId = evnt.EventId;
                return View("Create", evnt);
            }
            evnt.CreatedUser = Session["loggeduser"].ToString();
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            evnt.LocationId = Convert.ToInt16(Session["loggeduserlocid"]);
            evnt.CompanyID = companyid;
            var existscom = _bllEvent.GetEvents(companyid).Where(evt => evt.EventCode == evnt.EventCode).ToList();
            if (existscom.Count()!=0)
            {
                ModelState.Clear();
                ViewBag.Message = "3";
                @ViewBag.EventId = evnt.EventId;
                @ViewBag.EventCode = evnt.EventCode;
                return View("Create", evnt);
            }
            ViewBag.EventCode = evnt.EventCode;
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            if (_bllEvent.SaveEvent(evnt) !=0 )
            {
                ViewBag.Message = "1";
                ModelState.Clear();
                Event _evt = new Event();
                _evt.IsActive = true;
                _evt.IsPOS = false;
                ViewBag.IsPOS = _evt.IsPOS;
                return View("Create", _evt);
            }
            else
            {
                @ViewBag.EventId = evnt.EventId;
                ViewBag.Message = "2";
                return View("Create", evnt);
            }

        }

        [Authorize(Roles = "Others")]
        [HttpPost]
        public ActionResult Edit(Event evnt)
        {
            ViewBag.productdata = _bllProduct.GetOpenItems(Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ToList();
            ViewBag.IsPOS = evnt.IsPOS;
            @ViewBag.LocationId = evnt.LocationId;
            if (!ModelState.IsValid)
            {
                @ViewBag.EventId = evnt.EventId;
                return View(evnt);

            }
            if (evnt.IsActive == true && evnt.IsDelete == true)
            {
                @ViewBag.EventId = evnt.EventId;
                @ViewBag.Message = "4";
                return View(evnt);
            }

            evnt.ModifiedUser = Session["loggeduser"].ToString();
            evnt.CompanyID= Convert.ToInt32(Session["loggedusercompanyId"].ToString());

            evnt.LocationId= Convert.ToInt16(Session["loggeduserlocid"]);
            @ViewBag.EventId = evnt.EventId;

            if (_bllEvent.UpdateEvent(evnt) != 0)
            {
                ModelState.Clear();
                ViewBag.Message = "1";
                return View(new Event());
            }
            else
            {
                @ViewBag.EventId = evnt.EventId;
                ViewBag.Message = "2";
                return View(evnt);
            }
        }

        [HttpGet]
        public JsonResult ValidateEventCode(string eventcode)
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var events = _bllEvent.GetEvents(companyid).Where(evt=>evt.EventCode==eventcode).Count();
            return new JsonResult { Data = events, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }


    }
}