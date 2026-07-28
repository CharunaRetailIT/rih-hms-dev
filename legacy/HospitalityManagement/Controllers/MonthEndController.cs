using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RIT.HMS.BLL.MasterData;
using RIT.HMS.Domain;
using Newtonsoft.Json;
using RIT.HMS.Domain.ViewModels;
using RIT.HMS.BLL.TransactionData;
using RIT.HMS.Domain.ViewModels.Reports;

namespace HospitalityManagement.Controllers
{
    [SessionTimeout]
    public class MonthEndController : Controller
    {
        BLL_Location _blllocation;
        BLL_SysYears _sysYears;
        private BLL_MonthEnd _bllmonthend;

        public MonthEndController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _blllocation = new BLL_Location(cn);
            _sysYears = new BLL_SysYears(cn);
            _bllmonthend = new BLL_MonthEnd(cn);
        }

        [Authorize(Roles = "MonthEnd")]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetActiveLocations()
        {
            //  LocationService reporsitory = new LocationService();
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var locations = _blllocation.GetActiveLocations(companyid);
            return Json(JsonConvert.SerializeObject(locations, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetYears()
        {
            //  LocationService reporsitory = new LocationService();
            var years = _sysYears.GetYears();
            return Json(JsonConvert.SerializeObject(years, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetMonthEndDataByYear(int year, int loc)
        {
            int _sucss = 0;
            IEnumerable<MonthEndViewModel> mnthend = null;
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var sysloc = _blllocation.GetActiveLocations(companyid);
            foreach (var t in sysloc)
            {
                mnthend = _bllmonthend.GetMonthEndDataByYearLoc(year, t.SysLocationID);
                if (mnthend.Count() == 0)
                {
                    _sucss = _bllmonthend.SaveLocationMonths(year, t.SysLocationID, Session["loggeduser"].ToString());
                }
            }

            if (loc == 0)
                mnthend = _bllmonthend.GetMonthEndDataByYear(year,companyid);
            else
                mnthend = _bllmonthend.GetMonthEndDataByYearLoc(year, loc);

            return new JsonResult
            {
                Data = mnthend,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };


        }

        [Authorize(Roles = "MonthEnd")]
        [HttpPost]
        public ActionResult OpenMonth(MonthEnd mnthend)
        {
            @ViewBag.StockLocationId = mnthend.LocationId;
            @ViewBag.SysYearsId = mnthend.LocYear;
            @ViewBag.LocMonth = mnthend.LocMonth;


            if (mnthend.LocYear == 0)
            {
                ModelState.AddModelError("LocYear", "Select the year !");
                return View("Index", mnthend);

            }
            if (mnthend.LocMonth == 0)
            {
                ModelState.AddModelError("LocMonth", "Select the month !");
                return View("Index", mnthend);

            }
            if (mnthend.LocationId == 0)
            {
                ModelState.AddModelError("LocationId", "Select the location !");
                return View("Index", mnthend);

            }
            if (mnthend.LocYear > DateTime.Now.Year || mnthend.LocYear == DateTime.Now.Year && mnthend.LocMonth > DateTime.Now.Month)
            {
                @ViewBag.Message = "18";
                return View("Index", mnthend);
            }



            if (_bllmonthend.IsOpenMonthExist(mnthend.LocationId))
            {
                @ViewBag.Message = "5";
                return View("Index", mnthend);
            }

            MonthEnd mend = new MonthEnd();
            mend = _bllmonthend.GetMonthEndID(mnthend.LocYear, mnthend.LocMonth, mnthend.LocationId);
            if (mend.MonthEndId == 0)
            {
                @ViewBag.Message = "6";
                return View("Index", mnthend);
            }
            mnthend.MonthEndId = mend.MonthEndId;
            mnthend.ModifiedUser = Session["loggeduser"].ToString();
            mnthend.LocStatus = true;
            mnthend.LocIsClose = false;

            if (_bllmonthend.UpdateOpenMonth(mnthend) == 1)
            {
                ViewBag.Message = "1";
                return View("Index");
            }
            else
            {
                ViewBag.Message = "2";
                return View("Index");
            }
        }

        [Authorize(Roles = "MonthEnd")]
        [HttpPost]
        public ActionResult CloseMonth(MonthEnd mnthend)
        {
            ViewBag.StockLocationId = mnthend.LocationId;
            ViewBag.sysyearID = mnthend.LocYear;

            if (mnthend.LocYear == 0)
            {
                ModelState.AddModelError("LocYear", "Select the year !");
                return View("Index", mnthend);

            }
            if (mnthend.LocMonth == 0)
            {
                ModelState.AddModelError("LocMonth", "Select the month !");
                return View("Index", mnthend);

            }
            if (mnthend.LocationId == 0)
            {
                ModelState.AddModelError("LocationId", "Select the location !");
                return View("Index", mnthend);

            }

            MonthEnd mend = new MonthEnd();
            mend = _bllmonthend.GetMonthEndID(mnthend.LocYear, mnthend.LocMonth, mnthend.LocationId);
            if (mend.MonthEndId == 0)
            {
                @ViewBag.Message = "6";
                return View("Index", mnthend);
            }
            else if (mend.LocStatus == false)
            {
                @ViewBag.Message = "7";
                return View("Index", mnthend);
            }

            mnthend.MonthEndId = mend.MonthEndId;

            DailySalesViewMdel dailysales = new DailySalesViewMdel();
            dailysales.ValidMonthEndDataList = _bllmonthend.IsValidCloseMonth(mnthend.LocationId, mnthend.LocYear, mnthend.LocMonth);

            if (dailysales.ValidMonthEndDataList[0].Message != "NO PENDINGS")
            {
                if (dailysales.ValidMonthEndDataList[0].DocumentType == "8")  //temporary saved GRN
                {
                    @ViewBag.Message = "8";
                    @ViewBag.DocNumbers = dailysales.ValidMonthEndDataList[0].Message;
                    return View("Index", mnthend);
                }
                else if (dailysales.ValidMonthEndDataList[0].DocumentType == "9")  //approval pending GRN
                {
                    @ViewBag.Message = "9";
                    @ViewBag.DocNumbers = dailysales.ValidMonthEndDataList[0].Message;
                    return View("Index", mnthend);
                }
                else if (dailysales.ValidMonthEndDataList[0].DocumentType == "10")  //reopened GRN
                {
                    @ViewBag.Message = "10";
                    @ViewBag.DocNumbers = dailysales.ValidMonthEndDataList[0].Message;
                    return View("Index", mnthend);
                }
                else if (dailysales.ValidMonthEndDataList[0].DocumentType == "11")  //temporary save PRN
                {
                    @ViewBag.Message = "11";
                    @ViewBag.DocNumbers = dailysales.ValidMonthEndDataList[0].Message;
                    return View("Index", mnthend);
                }
                else if (dailysales.ValidMonthEndDataList[0].DocumentType == "12")  //approval pending PRN
                {
                    @ViewBag.Message = "12";
                    @ViewBag.DocNumbers = dailysales.ValidMonthEndDataList[0].Message;
                    return View("Index", mnthend);
                }
                else if (dailysales.ValidMonthEndDataList[0].DocumentType == "13")  //reopened PRN
                {
                    @ViewBag.Message = "13";
                    @ViewBag.DocNumbers = dailysales.ValidMonthEndDataList[0].Message;
                    return View("Index", mnthend);
                }
                else if (dailysales.ValidMonthEndDataList[0].DocumentType == "14")  //temporary saved TOG
                {
                    @ViewBag.Message = "14";
                    @ViewBag.DocNumbers = dailysales.ValidMonthEndDataList[0].Message;
                    return View("Index", mnthend);
                }
                else if (dailysales.ValidMonthEndDataList[0].DocumentType == "15")  //approval pending TOG
                {
                    @ViewBag.Message = "15";
                    @ViewBag.DocNumbers = dailysales.ValidMonthEndDataList[0].Message;
                    return View("Index", mnthend);
                }
                else if (dailysales.ValidMonthEndDataList[0].DocumentType == "16")  //reopened TOG
                {
                    @ViewBag.Message = "16";
                    @ViewBag.DocNumbers = dailysales.ValidMonthEndDataList[0].Message;
                    return View("Index", mnthend);
                }
                else
                {
                    @ViewBag.Message = "17";
                    return View("Index", mnthend);
                }
            }

            mnthend.ModifiedUser = Session["loggeduser"].ToString();
            mnthend.LocStatus = false;
            mnthend.LocIsClose = true;

            if (_bllmonthend.UpdateOpenMonth(mnthend) == 1)
            {
                ViewBag.Message = "3";
                return View("Index");
            }
            else
            {
                ViewBag.Message = "4";
                return View("Index");
            }
        }


    }
}